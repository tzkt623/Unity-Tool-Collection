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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZTools.SingletonNS;

namespace ZTools.MainThreadingNS
{
    public class MainThreadingController : Singleton<MainThreadingController>
    {
        private Dictionary<Type, Queue<IThreadable>> baseThreads;
        private Dictionary<Type, HashSet<IThreadable>> toCancel;
        private Dictionary<Type, float> maxTimeConsumptions;

        private List<IThreadable> alwaysUpdateQueue;

        public float TotalMaxTimeConsumption
        {
            get
            {
                float result = 0;

                foreach (var consumption in maxTimeConsumptions)
                    result += consumption.Value;

                return result;
            }
        }

        #region 实现 Singleton

        public override void FixedUpdate()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            alwaysUpdateQueue = new List<IThreadable>();
            baseThreads = new Dictionary<Type, Queue<IThreadable>>();
            maxTimeConsumptions = new Dictionary<Type, float>();
            toCancel = new Dictionary<Type, HashSet<IThreadable>>();
        }

        public override void Reload()
        {
            alwaysUpdateQueue.Clear();
            baseThreads.Clear();
            maxTimeConsumptions.Clear();
            toCancel.Clear();
        }

        public override void Unload()
        {
            alwaysUpdateQueue = null;
            baseThreads = null;
            maxTimeConsumptions = null;
            toCancel = null;
        }

        public override void Update()
        {
            #region 执行主循环

            Type currentType = null;
            Queue<IThreadable> currentQueue;
            HashSet<IThreadable> currentToCancel;
            float maxConsumption;
            IThreadable currentUnit;

            try
            {
                foreach (var thread in baseThreads)
                {
                    //try
                    //{
                    //    UnityEngine.Profiling.Profiler.BeginSample(thread.Key.ToString());

                        currentType = thread.Key;
                        currentQueue = thread.Value;
                        currentToCancel = toCancel[currentType];
                        maxConsumption = maxTimeConsumptions[currentType];

                        var startTime = Time.realtimeSinceStartup;
                        var maxLoop = currentQueue.Count;
                        var timesUp = false;

                        for (int i = 0; i < maxLoop; ++i)
                        {
                            currentUnit = currentQueue.Peek();

                            //如果已经被Dispose掉，那么自动跳过，不再放回队列内
                            if (currentUnit == null)
                            {
                                currentQueue.Dequeue();
                                continue;
                            }

                            //如果被标记为需要删除，那么删除标记，并跳过
                            if (currentToCancel.Count > 0 && currentToCancel.Contains(currentUnit))
                            {
                                currentQueue.Dequeue();
                                currentToCancel.Remove(currentUnit);
                                continue;
                            }

                            //检查时间
                            if (!timesUp && Time.realtimeSinceStartup - startTime >= maxConsumption)
                                timesUp = true;

                            if (!timesUp)
                            {
                                currentUnit.Tick();
                                currentQueue.Enqueue(currentQueue.Dequeue());
                            }
                        }
                    //}
                    //catch
                    //{
                    //    throw;
                    //}
                    //finally
                    //{
                    //    UnityEngine.Profiling.Profiler.EndSample();
                    //}
                }
            }
#pragma warning disable 168
            catch (InvalidOperationException _e)
            {
                //NOTHING
                //有可能在Ticking的时候对Foreach进行了添加操作
                //这个时候只需要无视错误，再执行一次即可
            }
#pragma warning restore 168
            catch (Exception _e)
            {
                if (currentType != null)
                {
                    Debug.LogErrorFormat("执行主线程循环时出现了错误，当前正在执行的队列类型名称为 {0}",
                        currentType.Name);
                    Debug.LogException(_e);
                }
            }
            #endregion

                #region 执行特殊行为者

                for (int i = 0; i < alwaysUpdateQueue.Count; ++i)
            {
                if (alwaysUpdateQueue[i] == null)
                {
                    FastRemoveFromList(alwaysUpdateQueue, i);
                    --i;
                }
                else
                {
                    alwaysUpdateQueue[i].Tick();
                }
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// 将一个实现IThreadable的对象加入到主线程中，进行执行
        /// </summary>
        /// <param name="_threadable"></param>
        public void GoThreading(IThreadable _threadable)
        {
            if (_threadable.AlwaysUpdate)
                alwaysUpdateQueue.Add(_threadable);
            else
            {
                var type = _threadable.Type;

                if (baseThreads.ContainsKey(type))
                {
                    if (!baseThreads[type].Contains(_threadable))
                        baseThreads[type].Enqueue(_threadable);
                    else if (toCancel[type].Contains(_threadable))
                        toCancel[type].Remove(_threadable);
                    else
                    {
                        Debug.LogWarningFormat("{0} {1}，在正在执行的情况下进行了RequestTick操作，这可能存在潜在的逻辑错误",
                                type.ToString(), _threadable.ToString());
                    }
                }
                else
                {
                    //开辟新的线程
                    float maxTime = 0.001f; //1毫秒
                    var attrs = type.GetCustomAttributes(typeof(MainThreadingAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        var attr = (MainThreadingAttribute)attrs[0];
                        maxTime = attr.MaxTime * 0.001f;
                        if (!Application.isEditor)
                        {
                            maxTime *= (attr.ForFPS / 60);
                            maxTime = Mathf.Min(attr.TopMostTime, maxTime);
                        }
                    }

                    baseThreads.Add(type, new Queue<IThreadable>());
                    toCancel.Add(type, new HashSet<IThreadable>());
                    maxTimeConsumptions.Add(type, maxTime);

                    //再执行一次，这样就直接开始了
                    GoThreading(_threadable);
                }
            }
        }

        /// <summary>
        /// 取消一个对象的执行
        /// 注意，如果这个对象是UnityObject，那么不要在OnDestroy中调用
        /// 因为Threading检测到为空后，会自己删除掉
        /// 如果是其他类型，那么必须调用，否则Threading会保留一份引用，导致内存泄漏
        /// </summary>
        /// <param name="_threadable"></param>
        public void CancelThreading(IThreadable _threadable)
        {
            if (_threadable.AlwaysUpdate)
            {
                if (alwaysUpdateQueue.Count == 0)
                    return;

                var index = alwaysUpdateQueue.IndexOf(_threadable);
                if (index != -1)
                    FastRemoveFromList(alwaysUpdateQueue, index);
            }
            else
            {
                if (baseThreads.Count == 0)
                    return;

                var type = _threadable.Type;
                if (toCancel.ContainsKey(type))
                {
                    if (!baseThreads[type].Contains(_threadable))
                        Debug.LogWarningFormat("{0} {1}，在没有执行的情况下进行了Quit操作，这没什么问题，但是尽量从逻辑上避免这种性能损耗。",
                            type.ToString(), _threadable.ToString());
                    else
                        toCancel[type].Add(_threadable);
                }
            }
        }

        /// <summary>
        /// 快速从列表中移除一个元素，不涉及重新分配的问题
        /// 但前提必须是不在乎顺序
        /// </summary>
        /// <param name="_list"></param>
        /// <param name="_index"></param>
        private void FastRemoveFromList(IList _list, int _index)
        {
            var lastIndex = _list.Count - 1;
            var temp = _list[lastIndex];
            _list[_index] = temp;
            _list.RemoveAt(lastIndex);
        }
    }
}
