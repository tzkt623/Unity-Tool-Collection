/*
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
 *
 * Copyright 2010 ZMind
 *
 * This file is part of ZTOOLS.
 * ZTOOLS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ZTOOLS is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with ZTOOLS.  If not, see <http://www.gnu.org/licenses/>.
 *
 * 这个文件是ZTOOLS的一部分。
 * 您可以单独使用或分发这个文件，但请不要移除这个头部声明信息.
 * ZTOOLS是一个自由软件，您可以自由分发、修改其中的源代码或者重新发布它，
 * 新的任何修改后的重新发布版必须同样在遵守LGPL3或更后续的版本协议下发布.
 * 关于LGPL协议的细则请参考COPYING、COPYING.LESSER文件，
 * 您可以在ZTOOLS的相关目录中获得LGPL协议的副本，
 * 如果没有找到，请连接到 http://www.gnu.org/licenses/ 查看。
 *
 * - Author: ZMind
 * - License: GNU Lesser General Public License (LGPL)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

namespace ZTools.NavigationNS
{
    public class NavigationBaker : IDisposable
    {
        /// <summary>
        /// 包含了一个可以使用的NavmeshData的完整结构
        /// 需要手动卸载
        /// </summary>
        protected class NavmeshDataPair : IDisposable
        {
            public NavMeshData data;
            public NavMeshBuildSettings settings;
            public NavMeshDataInstance instance;
            public AsyncOperation updateOperation;

            public void Dispose()
            {
                NavMeshBuilder.Cancel(data);
                updateOperation = null;

                instance.Remove();
                NavMeshData.DestroyImmediate(data);
                data = null;
            }
        }

        /// <summary>
        /// 当前正在追踪的NavmeshSource列表
        /// </summary>
        protected List<NavMeshBuildSource> sources;

        /// <summary>
        /// 实际使用的配置
        /// </summary>
        private Dictionary<int, NavMeshBuildSettings> settings;

        /// <summary>
        /// 当前存在的Navmesh
        /// </summary>
        protected Dictionary<int, NavmeshDataPair> navmeshTracker;

        /// <summary>
        /// 是否允许添加删除操作
        /// </summary>
        private readonly bool m_allowAddAndRemove;

        /// <summary>
        /// 是否允许添加删除操作，如果该Baker的Source来源来自于外部，则不允许使用
        /// </summary>
        public bool allowAddAndRemove { get { return m_allowAddAndRemove; } }


        /// <summary>
        /// 创建一个空的NavmeshBaker, 
        /// 需要手动添加NavmeshBuildSource，
        /// 直到正式调用Update之前，Navmesh不会实际创建
        /// </summary>
        public NavigationBaker()
        {
            m_allowAddAndRemove = true;
            sources = new List<NavMeshBuildSource>();
            navmeshTracker = new Dictionary<int, NavmeshDataPair>();
            settings = new Dictionary<int, NavMeshBuildSettings>();
        }

        /// <summary>
        /// 创建一个基于指定Source列表的NavmeshBaker
        /// 之后可以手动更改NavmeshBuildSource
        /// 直到正式调用Update之前，Navmesh不会实际创建
        /// </summary>
        /// <param name="_sources">外部源，由外部负责更新，禁止使用Add或者Remove函数</param>
        public NavigationBaker(List<NavMeshBuildSource> _sources)
        {
            m_allowAddAndRemove = false;
            sources = _sources;
            navmeshTracker = new Dictionary<int, NavmeshDataPair>();
        }

        /// <summary>
        /// 移除所有的数据源，但是保留当前的Navmesh
        /// </summary>
        public void ClearSourcesButKeepNavmesh()
        {
            if (allowAddAndRemove)
            {
                foreach (var source in sources)
                {
                    ((INavigationSourceProvider)source.component).WriteSourceIndex(-1);
                }

                sources.Clear();
            }
        }

        /// <summary>
        /// 清空所有的Navmesh资源，必须手动调用
        /// 否则会内存泄漏 GC不会自动清理
        /// </summary>
        public void Dispose()
        {
            foreach (var trackedData in navmeshTracker)
                trackedData.Value.Dispose();

            settings = null;
            navmeshTracker = null;
            sources = null;
        }

        /// <summary>
        /// 清理指定AgentType的Navmesh资源
        /// </summary>
        /// <param name="_agentTypeID">AgentTypeID</param>
        public void Dipose(int _agentTypeID)
        {
            if (navmeshTracker.ContainsKey(_agentTypeID))
            {
                navmeshTracker[_agentTypeID].Dispose();
                navmeshTracker.Remove(_agentTypeID);
            }
        }

        /// <summary>
        /// 往Source表中添加一个Source
        /// </summary>
        /// <param name="_source">描述了基本的位置，旋转以及大小</param>
        public void AddSource(INavigationSourceProvider _source)
        {
            if (!m_allowAddAndRemove)
                throw new Exception("无法对外来Source的NavigationBaker进行增删操作");

            sources.Add(_source.GetSource());
            _source.WriteSourceIndex(sources.Count - 1);
        }

        /// <summary>
        /// 往Source表中添加一组Source
        /// </summary>
        /// <param name="_source"></param>
        public void AddSource(INavigationSourceProviderGroup _sourceGroup)
        {
            var array = _sourceGroup.GetSourceProviderArray();
            if (array != null)
            {
                for (int i = 0, count = array.Length; i < count; ++i)
                {
                    AddSource(array[i]);
                }
            }
        }

        /// <summary>
        /// 更新Source表中的Source
        /// </summary>
        /// <param name="_source"></param>
        public void UpdateSource(INavigationSourceProvider _source)
        {
            if (!m_allowAddAndRemove)
                throw new Exception("无法对外来Source的NavigationBaker进行增删操作");

            var index = _source.GetSourceIndex();

            if (index == -1)
                AddSource(_source);
            else
                sources[index] = _source.GetSource();
        }

        /// <summary>
        /// 更新Source表中的一组Source
        /// </summary>
        /// <param name="_sourceGroup"></param>
        public void UpdateSource(INavigationSourceProviderGroup _sourceGroup)
        {
            var array = _sourceGroup.GetSourceProviderArray();
            if (array != null)
            {
                for (int i = 0, count = array.Length; i < count; ++i)
                {
                    UpdateSource(array[i]);
                }
            }
        }

        /// <summary>
        /// 快速移除一个数据源
        /// </summary>
        /// <param name="_source">要移除的源</param>
        public void RemoveSource(INavigationSourceProvider _source)
        {
            if (!m_allowAddAndRemove)
                throw new Exception("无法对外来Source的NavigationBaker进行增删操作");

            try
            {
                var _index = _source.GetSourceIndex();
                if (_index != -1)
                {
                    var lastIndex = sources.Count - 1;
                    sources[_index] = sources[lastIndex];
                    ((INavigationSourceProvider)sources[_index].component).WriteSourceIndex(_index);
                    sources.RemoveAt(lastIndex);

                    _source.WriteSourceIndex(-1); //以便复用
                }
            }
            catch (InvalidCastException)
            {
                throw new Exception("实现INavigationSourceProvider的类型在返回BuildSource时，必须指定component为自身");
            }
        }

        /// <summary>
        /// 快速移除一组数据源
        /// </summary>
        /// <param name="_sourceGroup"></param>
        public void RemoveSource(INavigationSourceProviderGroup _sourceGroup)
        {
            var array = _sourceGroup.GetSourceProviderArray();
            if (array != null)
            {
                for (int i = 0, count = array.Length; i < count; ++i)
                {
                    RemoveSource(array[i]);
                }
            }
        }

        /// <summary>
        /// 创建/更新指定AgentType的整个场景的Navmesh
        /// </summary>
        /// <param name="_agentType">AgentTypeID</param>
        /// <param name="_async">是否异步</param>
        public void UpdateNavmesh(int _agentType, bool _async)
        {
            UpdateNavmesh(_agentType, new Bounds(Vector3.zero, Vector3.one * float.PositiveInfinity), _async);
        }

        /// <summary>
        /// 创建/更新指定AgentType的指定范围的的Navmesh
        /// </summary>
        /// <param name="_agentType">AgentTypeID</param>
        /// <param name="_position">中心坐标</param>
        /// <param name="_size">大小</param>
        /// <param name="_async">是否异步</param>
        public void UpdateNavmesh(int _agentType, Vector3 _position, Vector3 _size, bool _aynsc)
        {
            UpdateNavmesh(_agentType, new Bounds(_position, _size), _aynsc);
        }

        /// <summary>
        /// 创建/更新指定AgentType的指定范围的的Navmesh
        /// </summary>
        /// <param name="_agentType">AgentTypeID</param>
        /// <param name="_bounds">区域</param>
        /// <param name="_async">是否异步</param>
        private void UpdateNavmesh(int _agentType, Bounds _bounds, bool _async)
        {
            //如果不存在，则添加
            if (!navmeshTracker.ContainsKey(_agentType))
            {
                var data = new NavMeshData(_agentType);
                var instance = NavMesh.AddNavMeshData(data);

                navmeshTracker.Add(_agentType, new NavmeshDataPair()
                {
                    data = data,
                    settings = settings.ContainsKey(_agentType) ? settings[_agentType] : NavMesh.GetSettingsByID(_agentType),
                    instance = instance,
                    updateOperation = null
                });

                _async = false; //第一次必须非异步
            }

            var pair = navmeshTracker[_agentType];

            if (_async)
            {
                if (pair.updateOperation == null || pair.updateOperation.isDone)
                {
                    pair.updateOperation = null;
                    pair.updateOperation = NavMeshBuilder.UpdateNavMeshDataAsync(pair.data, pair.settings, sources, _bounds);
                }
            }
            else
            {
                if (pair.updateOperation != null && !pair.updateOperation.isDone)
                {
                    NavMeshBuilder.Cancel(pair.data);
                }

                pair.updateOperation = null;
                NavMeshBuilder.UpdateNavMeshData(pair.data, pair.settings, sources, _bounds);
            }
        }

        public void SetBuildSettings(int _agentTypeID, int _tileSize = 50, float _voxelSize = 0.3f,
            float _agentRadium = 0.5f, float _agentHeight = 2f, float _agentSlope = 40f, float _agentStep = 0.65f)
        {
            NavMeshBuildSettings setting = new NavMeshBuildSettings();

            setting.agentTypeID = _agentTypeID;
            setting.overrideTileSize = true;
            setting.tileSize = _tileSize;
            setting.overrideVoxelSize = true;
            setting.voxelSize = _voxelSize;
            setting.agentRadius = _agentRadium;
            setting.agentHeight = _agentHeight;
            setting.agentSlope = _agentSlope;
            setting.agentClimb = _agentStep;

            if (settings.ContainsKey(_agentTypeID))
            {
                settings[_agentTypeID] = setting;
            }
            else
            {
                settings.Add(_agentTypeID, setting);
            }

            if (navmeshTracker.ContainsKey(_agentTypeID))
            {
                var pair = navmeshTracker[_agentTypeID];
                pair.settings = setting;
                navmeshTracker[_agentTypeID] = pair;
            }
        }
    }
}