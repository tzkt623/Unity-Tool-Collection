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
using ZTools.SingletonNS;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ZTools.ResourceManagerNS
{
    public sealed class ResourceManager : Singleton<ResourceManager>
    {
        private struct LoadingRequest
        {
            public Coroutine loadingProcess;
            public ResourceRequest request;
        }

        public event Action<string> onResourcesLoaded;
        private Dictionary<string, UnityEngine.Object> loadedResources;
        private Dictionary<string, LoadingRequest> loadingRequest;
        private ResourcesExcuter excuter;

        private float freshTimer;
        private GameObject freshObject;
        private const float freshDuration = 1f;

        //private const float testLoadingExtraTime = 3f;

        public override void FixedUpdate()
        {
            //if (freshObject == null)
            //{
            //    if (freshTimer >= freshDuration)
            //    {
            //        freshObject = Resources.Load<GameObject>("EmptyFresh");
            //        freshTimer = 0f;
            //    }
            //    else
            //    {
            //        freshTimer += Time.fixedDeltaTime;
            //    }
            //}
            //else
            //{
            //    freshObject = null;
            //}
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            loadedResources = new Dictionary<string, UnityEngine.Object>();
            loadingRequest = new Dictionary<string, LoadingRequest>();

            excuter = new GameObject("[Resources Manager Excuter]").AddComponent<ResourcesExcuter>();
            GameObject.DontDestroyOnLoad(excuter.gameObject);

            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }

        public override void Reload()
        {
            UnloadAll();
        }

        public override void Unload()
        {
            UnloadAll();

            excuter.StopAllCoroutines();
            GameObject.DestroyImmediate(excuter.gameObject);
            excuter = null;
        }

        public override void Update()
        {
        }

        public bool IsLoaded(string _path)
        {
            return loadedResources.ContainsKey(_path);
        }

        public bool IsLoading(string _path)
        {
            return loadingRequest.ContainsKey(_path);
        }

        public bool IsLoadingOrLoaded(string _path)
        {
            return IsLoaded(_path) || IsLoading(_path);
        }

        #region Resource

        /// <summary>
        /// 非异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_path"></param>
        public void LoadFromResources<T>(string _path) where T : UnityEngine.Object
        {
            if (IsLoaded(_path))
            {

                Debug.LogWarningFormat("路径{0}已经存在", _path);
                return;
            }

            try
            {
                var asset = Resources.Load<T>(_path);
                OnResourceAsyncLoaded(_path, asset);
            }
            catch
            {
                Debug.LogErrorFormat("加载资源{0}出错, 不存在此路径", _path);
                throw;
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_path"></param>
        /// <param name="_onLoaded"></param>
        public void LoadFromResources<T>(string _path, Action _onLoaded) where T : UnityEngine.Object
        {
            if (IsLoadingOrLoaded(_path))
            {
                Debug.LogWarningFormat("路径{0}已经存在", _path);
                return;
            }

            var request = Resources.LoadAsync<T>(_path);
            var process = excuter.StartCoroutine(LoadAsyncFromResources<T>(_path, request, _onLoaded));
            loadingRequest.Add(_path, new LoadingRequest()
            {
                request = request,
                loadingProcess = process
            });
        }

        private IEnumerator LoadAsyncFromResources<T>(string _path, ResourceRequest _request, Action _onLoaded) where T : UnityEngine.Object
        {
            yield return _request;
            //yield return new WaitForSeconds(testLoadingExtraTime);

            try
            {
                OnResourceAsyncLoaded(_path, (T)_request.asset);
                if (_onLoaded != null) _onLoaded();
            }
            catch (Exception _e)
            {
                Debug.LogErrorFormat("加载资源{0}出错", _path);
                throw _e;
            }
        }

        private void OnResourceAsyncLoaded<T>(string _path, T _asset) where T : UnityEngine.Object
        {
            loadingRequest.Remove(_path);
            loadedResources.Add(_path, _asset);

            if (onResourcesLoaded != null)
                onResourcesLoaded(_path);
        }

        #endregion

        #region Asset Bundle

        /// <summary>
        /// 非异步从AssetBundle中加载资源
        /// </summary>
        /// <param name="_path"></param>
        public void LoadFromAssetBundle<T>(string _path) where T : UnityEngine.Object
        {

        }

        #endregion
        public T GetAsset<T>(string _path) where T : UnityEngine.Object
        {
            if (!IsLoaded(_path))
            {
                if (IsLoading(_path))
                {
                    Debug.LogWarningFormat("资源{0}尚未完成加载", _path);
                }
                else
                {
                    Debug.LogWarningFormat("资源{0}尚未开始加载", _path);
                }

                return null;
            }
            else
            {
                return loadedResources[_path] as T;
            }
        }

        public void Unload(string _path)
        {
            if (IsLoaded(_path))
            {
                loadedResources.Remove(_path);
            }
            else if (IsLoading(_path))
            {
                var process = loadingRequest[_path].loadingProcess;
                excuter.StopCoroutine(process);
                process = null;

                loadingRequest.Remove(_path);
            }
        }

        private void UnloadAll()
        {
            var allRequest = loadingRequest.Keys.ToArray();
            foreach (var request in allRequest)
                Unload(request);

            var allLoaded = loadedResources.Keys.ToArray();
            foreach (var loaded in allLoaded)
                Unload(loaded);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Log Loaded Assets Path")]
        public static void LogCurrenExistedObjects()
        {
            if (DirectInstance != null)
            {
                foreach (var loaded in DirectInstance.loadedResources)
                {
                    Debug.Log(loaded.Key);
                }
            }
        }
#endif
    }
}