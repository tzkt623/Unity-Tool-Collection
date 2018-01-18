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
using UnityEngine.SceneManagement;
using System.Linq;
using System;

namespace ZTools.PoolingNS
{
    public class GameObjectPool : IEnumerable<GameObject>
    {
#if UNITY_EDITOR
        private const bool hide = true;
#else
        private const bool hide = false;
#endif

        private GameObject asset;
        private Transform root;
        private int totalCount;
        private HashSet<GameObject> objectInsideThisPool;

        public bool AutoExpand
        {
            get;
            set;
        }

        public int AutoExpandCount
        {
            get;
            set;
        }

        public Transform AnotherRoot
        {
            get; set;
        }

        public int Count
        {
            get
            {
                { return totalCount; }
            }
        }

        public GameObject Asset { get { return asset; } }

        /// <summary>
        /// 创建一个对象池，传入的GameObject必须是内存当中的Asset
        /// </summary>
        /// <param name="_asset"></param>
        /// <param name="_initCount"></param>
        public GameObjectPool(GameObject _asset, int _initCount)
        {
            if (_asset == null)
                throw new System.Exception("传入的资源为空，不能生成对象池");

            if (_asset.activeSelf)
            {
                _asset.SetActive(false);

                //#if UNITY_EDITOR
                //                Debug.LogWarningFormat("资源{0}处于打开状态，这可能导致OnEnable中执行许多错误的方法。", _asset.name);
                //#endif
            }

            objectInsideThisPool = new HashSet<GameObject>();

            var poolName = "Pool_" + _asset.name;
            var rootObject = new GameObject(poolName);
            root = rootObject.transform;
            root.hierarchyCapacity = _initCount;
            root.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;

            asset = _asset;

            Expand(_initCount);
        }

        public void MoveToScene(Scene _scene)
        {
            SceneManager.MoveGameObjectToScene(root.gameObject, _scene);
        }

        public void DontDestroyOnLoad()
        {
            GameObject.DontDestroyOnLoad(root.gameObject);
        }

        public void Destroy()
        {
            foreach (var obj in objectInsideThisPool)
            {
                if (obj != null && obj.transform.parent != root)
                    obj.transform.parent = root;
            }

            objectInsideThisPool = null;
            GameObject.Destroy(root.gameObject);
            asset = null;
        }

        /// <summary>
        /// 强制重新回收全部子元素
        /// </summary>
        public void RecollectAll()
        {
            foreach (var obj in objectInsideThisPool)
            {
                if (obj != null && obj.transform.parent != root)
                {
                    obj.SetActive(false);
                    obj.transform.parent = root;
                }
            }
        }

        public void Expand(int _count)
        {
            var name = asset.name;

            for (int i = 0; i < _count; ++i)
            {
                var newObject = GameObject.Instantiate(asset);
                newObject.SetActive(false);
                newObject.name = string.Concat(name, '_', totalCount);
                newObject.transform.parent = root;
                newObject.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;
                objectInsideThisPool.Add(newObject);
                ++totalCount;
            }
        }

        public void Return(GameObject _object)
        {
            if (objectInsideThisPool.Contains(_object))
            {
                _object.SetActive(false);
                _object.transform.parent = root;
            }
            else
            {
                _object.SetActive(false);
                _object.transform.parent = null;
            }
        }

        public GameObject Get()
        {
            var lastIndex = root.childCount - 1;

            if (lastIndex == -1)
            {
                if (AutoExpand)
                {
                    Expand(AutoExpandCount);
                    return Get();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var obj = root.GetChild(lastIndex);
                obj.parent = AnotherRoot;
                return obj.gameObject;
            }
        }

        IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator()
        {
            return objectInsideThisPool.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return objectInsideThisPool.GetEnumerator();
        }
    }
}
