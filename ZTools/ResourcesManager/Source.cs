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
using ZTools.SingletonNS;
using ZTools.PoolingNS;
using System;

namespace ZTools.ResourceManagerNS
{
    /// <summary>
    /// 适用于加载Unity单项资源的通用管理器
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResource"></typeparam>
    public abstract class Source<TClass, TKey, TResource> : Singleton<TClass>
        where TResource : UnityEngine.Object
        where TClass : Source<TClass, TKey, TResource>, new()
    {
        #region 变量定义

        public static int maxStreamingCount = 5;

        /// <summary>
        /// （已加载的）资源管理列表
        /// </summary>
        private Dictionary<TKey, KeyValuePair<string, TResource>> resources;

        /// <summary>
        /// 不同ID对应着同一个Path的请求映射表
        /// 当其中任何一个ID加载完毕时，会自动填充其他的
        /// 这样解决几个异步请求的问题
        /// </summary>
        private Dictionary<string, HashSet<TKey>> samePathDifferentKeyRequests;

        private Queue<TKey> streamingAssets;

        #endregion

        #region 事件定义

        /// <summary>
        /// 当库中有新的资源完成加载时
        /// </summary>
        public event Action<TKey> onNewAssetLoaded;

        #endregion

        #region 实现 Singleton

        public override void FixedUpdate()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            resources = new Dictionary<TKey, KeyValuePair<string, TResource>>();
            samePathDifferentKeyRequests = new Dictionary<string, HashSet<TKey>>();
            streamingAssets = new Queue<TKey>();
        }

        public override void Reload()
        {
            UnloadAll();
        }

        public override void Unload()
        {
            UnloadAll();

            resources = null;
            samePathDifferentKeyRequests = null;
            streamingAssets = null;
        }

        public override void Update()
        {
        }

        #endregion

        #region 加载与卸载

        /// <summary>
        /// 根据键值进行加载
        /// </summary>
        /// <param name="_key">键值</param>
        /// <param name="_async">是否异步</param>
        public void Load(TKey _key, bool _async)
        {
            if (!IsLoadingOrCompetelyLoaded(_key))
            {
                Load(_key, GetPathByKey(_key), _async);
            }
        }

        /// <summary>
        /// 加载键值与路径共同确定的资源
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_path"></param>
        /// <param name="_async"></param>
        protected void Load(TKey _key, string _path, bool _async)
        {
            if (_path == null)
                return;

            //如果从来没有加载过，则加载
            if (!resources.ContainsKey(_key))
            {
                if (!_async)
                {
                    //占位
                    resources.Add(_key, new KeyValuePair<string, TResource>(_path, null));

                    //如果被加载过，那么不需要重复加载，直接获取即可
                    if (!ResourceManager.Instance.IsLoadingOrLoaded(_path))
                        ResourceManager.Instance.LoadFromResources<TResource>(_path);

                    OnAssetLoaded(_key, _path, ResourceManager.Instance.GetAsset<TResource>(_path));
                }
                else
                {
                    //占位
                    resources.Add(_key, new KeyValuePair<string, TResource>(_path, null));

                    if(ResourceManager.Instance.IsLoaded(_path))
                    {
                        OnAssetLoaded(_key, _path, ResourceManager.Instance.GetAsset<TResource>(_path));
                    }
                    else if (ResourceManager.Instance.IsLoading(_path))
                    {
                        if (!samePathDifferentKeyRequests.ContainsKey(_path))
                            samePathDifferentKeyRequests.Add(_path, new HashSet<TKey>());

                        samePathDifferentKeyRequests[_path].Add(_key);
                    }
                    else
                    {
                        ResourceManager.Instance.LoadFromResources<TResource>(_path, () =>
                        {
                            //如果本脚本尚未被清理掉
                            //（避免发生抢占线程的情况）
                            if (resources != null && resources.ContainsKey(_key))
                            {
                                OnAssetLoaded(_key, _path, ResourceManager.Instance.GetAsset<TResource>(_path));
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 根据键值进行卸载
        /// </summary>
        /// <param name="_key"></param>
        public void Unload(TKey _key)
        {
            if (resources.ContainsKey(_key))
            {
                var path = resources[_key].Key;
                resources.Remove(_key);
                ResourceManager.DirectInstance?.Unload(path);
            }
        }

        /// <summary>
        /// 卸载全部资源
        /// </summary>
        public void UnloadAll()
        {
            if (resources != null)
            {
                foreach (var loadedResouces in resources)
                {
                    var path = loadedResouces.Value.Key;
                    ResourceManager.DirectInstance?.Unload(path);
                }
            }

            streamingAssets.Clear();
            resources.Clear();
            samePathDifferentKeyRequests.Clear();
            onNewAssetLoaded = null;
        }

        /// <summary>
        /// 键值所对应的资源是否已经加载完毕
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public bool IsCompetelyLoaded(TKey _key)
        {
            return resources.ContainsKey(_key) && resources[_key].Value != null;
        }

        /// <summary>
        /// 键值所对应的资源是否正在加载
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public bool IsLoading(TKey _key)
        {
            return resources.ContainsKey(_key) && resources[_key].Value == null;
        }

        /// <summary>
        /// 键值所对应的资源是否正在加载，或者已经加载完毕
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public bool IsLoadingOrCompetelyLoaded(TKey _key)
        {
            return resources.ContainsKey(_key);
        }

        /// <summary>
        /// 当资源加载完毕后的回调函数
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_path"></param>
        /// <param name="_asset"></param>
        protected virtual void OnAssetLoaded(TKey _key, string _path, TResource _asset)
        {
            //则修改之前占位的数据
            var oldPair = resources[_key];
            var newPair = new KeyValuePair<string, TResource>(oldPair.Key, _asset);
            resources[_key] = newPair;

            if (onNewAssetLoaded != null)
            {
                onNewAssetLoaded(_key);
            }

            if (samePathDifferentKeyRequests.ContainsKey(_path))
            {
                foreach (var key in samePathDifferentKeyRequests[_path])
                {
                    oldPair = resources[key];
                    newPair = new KeyValuePair<string, TResource>(oldPair.Key, _asset);
                    resources[key] = newPair;

                    if (onNewAssetLoaded != null)
                    {
                        onNewAssetLoaded(key);
                    }
                }

                samePathDifferentKeyRequests.Remove(_path);
            }
        }

        /// <summary>
        /// 返回键值对应的路径，以支持只根据键值，就能加载的功能
        /// 每个子类必须实现
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        protected abstract string GetPathByKey(TKey _key);

        #endregion

        #region 访问函数

        /// <summary>
        /// 根据键值获取资源
        /// </summary>
        /// <param name="_key">键值</param>
        /// <param name="_resources">资源，根据Config的配置，可能返回NULL</param>
        /// <returns>返回是否获取成功</returns>
        public virtual bool Get(TKey _key, out TResource _resources)
        {
            if (resources.ContainsKey(_key))
            {
                _resources = resources[_key].Value;
                return _resources != null;
            }
            else
            {
                _resources = null;
                return false;
            }
        }

        /// <summary>
        /// 申请StreamingIcon,
        /// 只有当有可以使用的Sprite时候，返回True
        /// 在此之前，应当不停的发出申请
        /// </summary>
        public virtual bool RequestStreamingAssets(TKey _key)
        {
            if (this.IsLoadingOrCompetelyLoaded(_key))
            {
                if (this.IsCompetelyLoaded(_key))
                {
                    //完全加载完毕，可以使用
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //发出申请
                this.Load(_key, true);
                streamingAssets.Enqueue(_key);

                if (streamingAssets.Count > maxStreamingCount)
                {
                    this.Unload(streamingAssets.Dequeue());
                }

                return false;
            }
        }

        #endregion
    }
}