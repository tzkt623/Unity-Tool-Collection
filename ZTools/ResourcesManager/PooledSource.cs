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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZTools.PoolingNS;
using UnityEngine.SceneManagement;
using System;

namespace ZTools.ResourceManagerNS
{
    /// <summary>
    /// 对象池资源管理
    /// </summary>
    /// <typeparam name="Tkey">键值类型，不允许是可为空的类型，string除外</typeparam>
    public abstract class PooledSource<TClass, Tkey> : Source<TClass, Tkey, GameObject>
        where TClass : PooledSource<TClass, Tkey>, new()
    {
        #region 变量定义

        private Dictionary<Tkey, GameObjectPool> pooling;
        private Scene resourcesScene;

        private int defaultPoolCount;
        private int autoExpandCount;
        private bool autoExpand;
        private bool allowNullReturn;

        #endregion

        #region 复写 Singleton 生命周期

        /// <summary>
        /// 复写加载函数
        /// 在执行完基础的加载函数上，
        /// 初始化PooledSource所需的额外列表与字典
        /// 同时设置初始大小等属性
        /// </summary>
        public override void Load()
        {
            base.Load();

            //尝试获取PooledResourcesConfigAttribute信息
            var attributes = GetType().GetCustomAttributes(typeof(PooledSourceConfigAttribute), false);

            string resourcesSceneName = null;

            if (attributes != null && attributes.Length > 0)
            {
                var config = ((PooledSourceConfigAttribute)attributes[0]);
                defaultPoolCount = config.DefaultPoolCount;
                autoExpand = config.AutoExpand;
                autoExpandCount = config.AutoExpandCount;
                allowNullReturn = config.AllowNullReturn;
                resourcesSceneName = config.ResoucesScene;
            }
            else
            {
                defaultPoolCount = 8;
                autoExpand = true;
                autoExpandCount = 1;
                allowNullReturn = false;
                resourcesSceneName = "Rs";
            }

            pooling = new Dictionary<Tkey, GameObjectPool>();
            var s = SceneManager.GetSceneByName(resourcesSceneName);
            if (s.IsValid())
                resourcesScene = s;
            else
                resourcesScene = SceneManager.CreateScene(resourcesSceneName);
        }

        public override void Reload()
        {
            foreach (var pool in pooling.Values)
            {
                pool.Destroy();
            }

            pooling.Clear();
            base.Reload();
        }

        public override void Unload()
        {
            foreach (var pool in pooling.Values)
            {
                if (pool == null)
                {

                }
                pool.Destroy();
            }

            pooling = null;
            base.Unload();
        }

        #endregion

        #region 功能函数

        /// <summary>
        /// 把旗下所有的资源移动到某个特定场景中
        /// </summary>
        /// <param name="_scene"></param>
        public void MoveToScene(Scene _scene)
        {
            resourcesScene = _scene;
            foreach (var pool in pooling.Values)
                pool.MoveToScene(resourcesScene);
        }

        /// <summary>
        /// 复写获取资源
        /// 不再返回原始资源
        /// 而是从对象池里面取出指定的对象
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_resources"></param>
        /// <returns></returns>
        public override bool Get(Tkey _key, out GameObject _resources)
        {
            if (pooling.ContainsKey(_key))
            {
                var pool = pooling[_key];
                if (pool == null)
                {
                    _resources = null;
                    return false;
                }
                else
                {
                    _resources = pooling[_key].Get();

                    if (_resources == null)
                    {
                        //如果允许返回空值，那么即便是为空，也应当返回true
                        return allowNullReturn;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                _resources = null;
                return false;
            }
        }

        /// <summary>
        /// 仅仅获取资源
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_resources"></param>
        /// <returns></returns>
        protected bool GetAssetOnly(Tkey _key, out GameObject _resources)
        {
            return base.Get(_key, out _resources);
        }

        /// <summary>
        /// 对象池专属的返还资源
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_gameObject"></param>
        public virtual void Return(Tkey _key, GameObject _gameObject)
        {
            pooling[_key].Return(_gameObject);
        }

        #endregion

        #region 资源加载与卸载

        /// <summary>
        /// 复写卸载资源的函数
        /// 在基本的卸载函数之前
        /// 要销毁指定的对象池
        /// </summary>
        /// <param name="_key"></param>
        public new void Unload(Tkey _key)
        {
            if (pooling.ContainsKey(_key))
            {
                pooling[_key].Destroy();
                pooling.Remove(_key);
            }

            base.Unload(_key);
        }

        public new void UnloadAll()
        {
            foreach (var pool in pooling.Values)
            {
                pool.Destroy();
            }

            pooling.Clear();

            base.UnloadAll();
        }

        /// <summary>
        /// 复写当资源加载完毕后的回调函数
        /// 需要额外执行创建对象池的功能
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_path"></param>
        /// <param name="_asset"></param>
        protected override void OnAssetLoaded(Tkey _key, string _path, GameObject _asset)
        {
            base.OnAssetLoaded(_key, _path, _asset);
            try
            {
                var pool = new GameObjectPool(_asset, defaultPoolCount);
                pool.AutoExpand = autoExpand;
                pool.AutoExpandCount = autoExpandCount;
                pool.MoveToScene(resourcesScene);
                pooling.Add(_key, pool);
            }
            catch
            {
                Debug.Log(_key.ToString());
            }
        }

        #endregion
    }

    /// <summary>
    /// 描述对象池的基本数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PooledSourceConfigAttribute : Attribute
    {
        /// <summary>
        /// 默认的对象池大小
        /// </summary>
        public int DefaultPoolCount { get; private set; }

        /// <summary>
        /// 当数量不足时，自动扩展的池子大小
        /// </summary>
        public int AutoExpandCount { get; private set; }

        /// <summary>
        /// 是否允许自动扩展
        /// </summary>
        public bool AutoExpand
        {
            get
            {
                return AutoExpandCount > 0;
            }
        }

        /// <summary>
        /// 是否允许返回NULL值
        /// </summary>
        public bool AllowNullReturn { get; private set; }

        /// <summary>
        /// 对象池存放的场景名称
        /// </summary>
        public string ResoucesScene { get; private set; }

        /// <summary>
        /// 构造一个对象池资源管理器的限制条件
        /// </summary>
        /// <param name="_defaultPoolCount">默认的对象池大小</param>
        /// <param name="_autoExpandCount">数量不足时的对象池扩展大小，使得对象池总是有足够的元素可以使用。如果为0，则表示不允许扩展</param>
        /// <param name="_allowNullReturn">是否允许返回NULL值。</param>
        public PooledSourceConfigAttribute(int _defaultPoolCount, int _autoExpandCount, bool _allowNullReturn, string _resourcesScene = "Rs")
        {
            DefaultPoolCount = _defaultPoolCount;
            AutoExpandCount = _autoExpandCount;
            AllowNullReturn = _allowNullReturn;
            ResoucesScene = _resourcesScene;
        }
    }
}
