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
using System.Linq;

namespace ZTools.SingletonNS
{
    /// <summary>
    /// 单例管理器
    /// 存放当前场景中所有存活的单例
    /// 
    /// 该脚本的销毁顺序放在所有脚本的最后，避免卸载的时候找不到单例的情况
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SingletonManager : MonoBehaviour
    {
#if UNITY_EDITOR

        //[UnityEditor.MenuItem("Tools/Log Data Size")]
        //public static void LogSize()
        //{
        //    if (managerObj != null)
        //    {
        //        Debug.Log($"{System.Runtime.InteropServices.Marshal.SizeOf(managerObj.GetComponent<SingletonManager>())}");
        //    }
        //}

        [UnityEditor.MenuItem("Tools/Log Singletons")]
        private static void LogSingleton()
        {
            foreach(var singleton in allSingletons)
            {
                Debug.Log(singleton.GetType().ToString());
            }
        }
        
#endif

        /// <summary>
        /// Singleton Manager 是寄存在一个GameObject上的
        /// 因此可以选择该物体在层级面板中是否可视
        /// </summary>
        private static bool shouldThisObjectHide = false;

        /// <summary>
        /// Singleton Manager 所寄存的GameObject
        /// </summary>
        private static GameObject managerObj;

        /// <summary>
        /// 当前所有注册的单例
        /// </summary>
        private static HashSet<SingletonBase> allSingletons;

        /// <summary>
        /// 缓存上述Hash列表的迭代器
        /// 避免每一帧都产生GC
        /// </summary>
        private static SingletonBase[] allSingletonsArray;

        /// <summary>
        /// 注册一个单例
        /// </summary>
        /// <param name="_singleton">要注册的单例</param>
        public static void Regist(SingletonBase _singleton)
        {
            //Debug.Log(_singleton.GetType().ToString());

            if (allSingletons == null)
            {
                managerObj = new GameObject("[Singleton Manager]");
                managerObj.AddComponent<SingletonManager>();
                DontDestroyOnLoad(managerObj);
                managerObj.hideFlags = shouldThisObjectHide ? HideFlags.HideAndDontSave : HideFlags.None;
                allSingletons = new HashSet<SingletonBase>();
            }
            if (allSingletons.Contains(_singleton)) throw new System.Exception(string.Format("重复注册单例 : {0}", _singleton.GetType().FullName));

            allSingletons.Add(_singleton);
            allSingletonsArray = allSingletons.ToArray();
        }

        /// <summary>
        /// 注销一个单例
        /// </summary>
        /// <param name="_singleton">要注销的单例</param>
        public static void UnRegist(SingletonBase _singleton)
        {
            if (_singleton == null)
                return;

            if (allSingletons == null)
                throw new System.Exception(string.Format("单例管理器SingletonManager尚未初始化，不允许注销单例 ： {0}", _singleton.GetType().FullName));

            if (!allSingletons.Contains(_singleton))
                throw new System.Exception(string.Format("单例{0}没有注册过，不允许注销", _singleton.GetType().FullName));

            _singleton.Unload();
            _singleton.Loaded = false;
            allSingletons.Remove(_singleton);
            allSingletonsArray = allSingletons.ToArray();

            ((System.IDisposable)_singleton).Dispose();
        }

        /// <summary>
        /// 注销所有的单例
        /// </summary>
        public static void UnRegistAll()
        {
            List<string> names = new List<string>(allSingletons.Count);

            foreach (var singleton in allSingletons)
                names.Add(singleton.GetType().Name);

            var name = string.Empty;

            try
            {
                foreach (var singleton in allSingletons)
                {
                    name = singleton.GetType().Name;

                    singleton.Unload();
                    singleton.Loaded = false;
                    ((System.IDisposable)singleton).Dispose();
                }
            }
            catch
            {
                Debug.LogError($"卸载时候出现了错误，正进行到{name}, 以下是当前所有存在的Singleton");
                foreach (var n in names)
                {
                    Debug.LogError(n);
                }

                throw;
            }

            allSingletons = null;
            allSingletonsArray = null;
        }

        public static void Destroy()
        {
            DestroyImmediate(managerObj);
            managerObj = null;
        }

        #region 更新函数 Updates

        /// <summary>
        /// 检查是否应该更新，避免报错
        /// </summary>
        /// <returns></returns>
        private bool ShouldUpdate()
        {
            return allSingletonsArray != null && allSingletonsArray.Length > 0;
        }

        /// <summary>
        /// Unity默认回调，每帧更新
        /// </summary>
        private void Update()
        {
            if (!ShouldUpdate())
                return;

            for (int i = 0; i < allSingletonsArray.Length; ++i)
            {
                if (allSingletonsArray[i] == null)
                    throw new System.Exception("存在一个为空值的单例!");

                //UnityEngine.Profiling.Profiler.BeginSample(allSingletonsArray[i].GetType().ToString());

                allSingletonsArray[i].Update();

                //UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Unity默认回调，固定频率更新
        /// </summary>
        private void FixedUpdate()
        {
            if (!ShouldUpdate())
                return;

            for (int i = 0; i < allSingletonsArray.Length; ++i)
            {
                if (allSingletonsArray[i] == null)
                    throw new System.Exception("存在一个为空值的单例!");

                //UnityEngine.Profiling.Profiler.BeginSample(allSingletonsArray[i].GetType().ToString());

                allSingletonsArray[i].FixedUpdate();

                //UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Unity默认回调，帧尾更新
        /// </summary>
        private void LateUpdate()
        {
            if (!ShouldUpdate())
                return;

            for (int i = 0; i < allSingletonsArray.Length; ++i)
            {
                if (allSingletonsArray[i] == null)
                    throw new System.Exception("存在一个为空值的单例!");

                //UnityEngine.Profiling.Profiler.BeginSample(allSingletonsArray[i].GetType().ToString());

                allSingletonsArray[i].LateUpdate();

                //UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        #endregion

        private void OnDisable()
        {
            foreach (var manager in allSingletonsArray)
            {
                manager.ReleaseMemories();
                manager.Loaded = false;
            }

            bool isDisabledByQuitEditor;
#if UNITY_EDITOR
            isDisabledByQuitEditor = !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
            isDisabledByQuitEditor = false;
#endif

            if (isDisabledByQuitEditor)
            {
                foreach (var manager in allSingletonsArray)
                {
                    ((System.IDisposable)manager).Dispose();
                }
                return;
            }
            else
            {
                UnRegistAll();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            try
            {
                if (focus)
                {
                    foreach (var singleton in allSingletons)
                    {
                        try
                        {
                            singleton.OnApplicationFocused();
                        }
                        catch (System.Exception _e)
                        {
                            Debug.LogError($"恢复焦点时，{singleton.GetType().Name}出现了一些问题。");
                            Debug.LogException(_e);
                        }
                    }
                }
                else
                {
                    foreach (var singleton in allSingletons)
                    {
                        try
                        {
                            singleton.OnApplicationGoesIntoBackground();
                        }
                        catch (System.Exception _e)
                        {
                            Debug.LogError($"进入背景程序时，{singleton.GetType().Name}出现了一些问题。");
                            Debug.LogException(_e);
                        }
                    }
                }
            }
            catch (System.Exception _e)
            {
                Debug.LogException(_e);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            foreach(var singleton in allSingletons)
            {
                singleton.OnDrawGizmos();
            }
        }

#endif
    }
}
