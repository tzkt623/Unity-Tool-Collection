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

namespace ZTools.ViewCulling
{
    public class ViewCullingGroup : IDisposable
    {
        private int maxSize;
        private CullingGroup cullingGroup;
        private BoundingSphere[] boundingSpheres;
        private IViewCullingObject[] viewCullingObjects;
        private int currentRequestingCount;
        public event Action<IViewCullingObject, int> onVisibilityChanged;

        public enum CullType
        {
            ViewCull,
            DistanceCull,
        }

        public bool valid
        {
            get
            {
                return boundingSpheres != null;
            }
        }

        public bool cameraExist
        {
            get
            {
                return cullingGroup.targetCamera != null && cullingGroup.targetCamera.enabled;
            }
        }

        public bool isFull
        {
            get
            {
                return currentRequestingCount >= maxSize;
            }
        }

        public int Count
        {
            get
            {
                return currentRequestingCount;
            }
        }

        public CullType cullType { get; private set; }

        public ViewCullingGroup(int _maxSize)
        {
            maxSize = _maxSize;
            currentRequestingCount = 0;

            cullingGroup = new CullingGroup();
            boundingSpheres = new BoundingSphere[maxSize];
            viewCullingObjects = new IViewCullingObject[maxSize];
            cullingGroup.onStateChanged = OnStateVisibilityChanged;
            cullType = CullType.ViewCull;

            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingSphereCount(currentRequestingCount);
        }

        public void Dispose()
        {
            onVisibilityChanged = null;

            ClearAll();

            cullingGroup.onStateChanged = null;
            cullingGroup.Dispose();
            cullingGroup = null;

            viewCullingObjects = null;
            boundingSpheres = null;
        }

        /// <summary>
        /// 设置裁剪系统对应的摄像机，可以不是主摄像机
        /// </summary>
        /// <param name="_camera"></param>
        public void SetCullingGroupCamera(Camera _camera)
        {
            cullingGroup.targetCamera = _camera;
        }

        /// <summary>
        /// 设置CullingGroup的距离分区
        /// 例如传入三个值，10,20,30
        /// 如果距离在10以内，代表摄像机范围
        /// 如果距离在20以内，代表超出范围
        /// 如果距离在30以内，代表足够远，超出则代表非常远
        /// </summary>
        /// <param name="_distanceBands"></param>
        public void SetDistanceBands(params float[] _distanceBands)
        {
            if (cullType == CullType.ViewCull)
            {
                cullingGroup.onStateChanged = OnStateDistanceChanged;
                cullType = CullType.DistanceCull;
            }

            cullingGroup.SetBoundingDistances(_distanceBands);
        }

        /// <summary>
        /// 设置观察点
        /// </summary>
        /// <param name="_transform"></param>
        public void SetWatchPoint(Transform _transform)
        {
            cullingGroup.SetDistanceReferencePoint(_transform);
        }

        /// <summary>
        /// 获取距离Band
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetDistanceBand(int _index)
        {
            if (cullType == CullType.ViewCull)
                return cullingGroup.IsVisible(_index) ? 0 : 1;
            else
                return cullingGroup.GetDistance(_index);
        }

        public bool Visible(int _index)
        {
            return cullingGroup.IsVisible(_index);
        }

        /// <summary>
        /// 填加一个裁剪请求
        /// </summary>
        /// <param name="_cullingObject"></param>
        public void AddRequest(IViewCullingObject _cullingObject)
        {
            //检查当前的数量是否已经达到了最大值
            //如果达到了，那么就不允许继续添加
            if (currentRequestingCount >= maxSize)
            {
                this.Expand(Mathf.Max(1, maxSize / 2));
                this.AddRequest(_cullingObject);
                Debug.LogWarning($"{System.DateTime.Now.ToShortTimeString()} : Culling系统超出了范围，已增加大小至{Mathf.Max(1, maxSize)}。");
            }
            else
            {
                //currentRequestingCount其实就是最后一个可用的Index

                //把Object添加到追踪队列里面
                //并且赋值Index以供记录
                viewCullingObjects[currentRequestingCount] = _cullingObject;
                _cullingObject.index = currentRequestingCount;

                //将BoundingSpheres这边的数据与之同步
                boundingSpheres[currentRequestingCount].position = _cullingObject.position;
                boundingSpheres[currentRequestingCount].radius = _cullingObject.radium;

                //增加数量，并且同步设置CullingGroup的数量
                ++currentRequestingCount;
                cullingGroup.SetBoundingSphereCount(currentRequestingCount);
            }
        }

        /// <summary>
        /// 移除一个裁剪请求
        /// </summary>
        /// <param name="_cullingObject"></param>
        public void RemoveRequest(IViewCullingObject _cullingObject)
        {
            var index = _cullingObject.index;

            //如果Index为-1，说明根本就不在队列中
            if (index != -1)
            {
                //从CullingGroup中移除该元素
                cullingGroup.EraseSwapBack(index);

                //减少数量，并且同步设置CullingGroup的数量
                --currentRequestingCount;
                cullingGroup.SetBoundingSphereCount(currentRequestingCount);

                //CullingGroup中是将最后一个元素移动到当前元素进行替换的
                //因此ViewCullingObjects这边也采取相同的措施
                //并更新被替换的Object的Index
                viewCullingObjects[index] = viewCullingObjects[currentRequestingCount];
                viewCullingObjects[index].index = index;

                //将申请者的Index设置为-1，表示其已经不属于列表内
                _cullingObject.index = -1;
            }
        }

        /// <summary>
        /// 移除一个裁剪请求
        /// </summary>
        /// <param name="_index"></param>
        public void RemoveRequest(int _index)
        {
            RemoveRequest(GetRequestObject(_index));
        }

        /// <summary>
        /// 更新裁剪数据
        /// </summary>
        /// <param name="_cullingObject"></param>
        public void UpdateRequest(IViewCullingObject _cullingObject)
        {
            var index = _cullingObject.index;

            //如果index为-1，说明不在队列内
            if (index != -1)
            {
                //更新对应的BoundingSpheres的位置和半径
                boundingSpheres[index].position = _cullingObject.position;
                boundingSpheres[index].radius = _cullingObject.radium;
            }
        }

        /// <summary>
        /// 更新裁剪数据
        /// </summary>
        /// <param name="_index"></param>
        public void UpdateRequest(int _index)
        {
            boundingSpheres[_index].position = viewCullingObjects[_index].position;
            boundingSpheres[_index].radius = viewCullingObjects[_index].radium;
        }

        /// <summary>
        /// 获取指定Index的CullingObject
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public IViewCullingObject GetRequestObject(int _index)
        {
            return viewCullingObjects[_index];
        }

        /// <summary>
        /// 清空全部CullingObject
        /// </summary>
        public void ClearAll()
        {
            cullingGroup.SetBoundingSphereCount(0);

            for (int i = 0; i < currentRequestingCount; ++i)
            {
                if (viewCullingObjects[i] != null)
                {
                    viewCullingObjects[i].index = -1;
                    viewCullingObjects[i] = null;
                }
            }

            currentRequestingCount = 0;
        }

        /// <summary>
        /// 找到指定DistanceBand中的所有元素
        /// 顺序查找，速度为O(N)
        /// </summary>
        /// <param name="_distanceBand"></param>
        /// <param name="_resultBuffer"></param>
        /// <returns></returns>
        public int Find(int _distanceBand, int[] _resultBuffer)
        {
            return cullingGroup.QueryIndices(_distanceBand, _resultBuffer, 0);
        }

        /// <summary>
        /// 找到可见的所有元素
        /// 顺序查找，速度为O(N)
        /// </summary>
        /// <param name="_visible"></param>
        /// <param name="_resultBuffer"></param>
        /// <returns></returns>
        public int Find(bool _visible, int[] _resultBuffer)
        {
            return cullingGroup.QueryIndices(_visible, _resultBuffer, 0);
        }

        /// <summary>
        /// 扩展
        /// </summary>
        /// <param name="_count"></param>
        public void Expand(int _count)
        {
            maxSize = boundingSpheres.Length + _count;

            Array.Resize(ref boundingSpheres, maxSize);
            Array.Resize(ref viewCullingObjects, maxSize);
            cullingGroup.SetBoundingSpheres(boundingSpheres);
        }

        private void OnStateDistanceChanged(CullingGroupEvent _event)
        {
            viewCullingObjects[_event.index].OnVisibilityChanged(_event.currentDistance);
            onVisibilityChanged?.Invoke(viewCullingObjects[_event.index], _event.currentDistance);
        }

        private void OnStateVisibilityChanged(CullingGroupEvent _event)
        {
            viewCullingObjects[_event.index].OnVisibilityChanged(_event.isVisible ? 0 : 1);
            onVisibilityChanged?.Invoke(viewCullingObjects[_event.index], _event.isVisible ? 0 : 1);
        }
    }

    public sealed class ViewCullingManager : Singleton<ViewCullingManager>
    {
        private Dictionary<int, ViewCullingGroup> cullingGroups;
        private int currentUpdateIndex;

        #region Singleton Overriden

        public override void FixedUpdate()
        {
            if (cullingGroups.ContainsKey(0)
                && !cullingGroups[0].cameraExist)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    cullingGroups[0].SetCullingGroupCamera(cam);
                    cullingGroups[0].SetWatchPoint(cam.transform);
                }
                //Debug.Log("重新设置公共视野裁剪的摄像机");
            }
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            cullingGroups = new Dictionary<int, ViewCullingGroup>();
        }

        public override void Reload()
        {
            foreach (var group in cullingGroups.Values)
            {
                group.ClearAll();
            }
        }

        public override void Unload()
        {

        }

        public override void Update()
        {
            //更新SharedViewCullingGroup
            var targetIndex = Mathf.Min(currentUpdateIndex + 20, SharedViewCullingGroup.Count);
            for (int i = currentUpdateIndex; i < targetIndex; ++i)
            {
                SharedViewCullingGroup.UpdateRequest(i);
            }
            currentUpdateIndex = targetIndex;
            if (currentUpdateIndex >= SharedViewCullingGroup.Count)
                currentUpdateIndex = 0;
        }

        public override void ReleaseMemories()
        {
            foreach (var group in cullingGroups.Values)
            {
                group.Dispose();
            }

            cullingGroups = null;
        }

        #endregion

        #region 公共函数

        /// <summary>
        /// 开辟一个新的区域
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_maxSize"></param>
        /// <returns></returns>
        public ViewCullingGroup RequestNewGroup(int _key, int _maxSize)
        {
            if (cullingGroups.ContainsKey(_key))
                Debug.LogFormat("经存在Key为{0}的裁剪组", _key);
            else
                cullingGroups.Add(_key, new ViewCullingGroup(_maxSize));

            return cullingGroups[_key];
        }

        /// <summary>
        /// 销毁一个群组
        /// </summary>
        /// <param name="_key"></param>
        public void DestroyGroup(int _key)
        {
            if (cullingGroups != null && cullingGroups.ContainsKey(_key))
            {
                cullingGroups[_key].Dispose();
                cullingGroups.Remove(_key);
            }
        }

        /// <summary>
        /// 获取一个ViewCulling组
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public ViewCullingGroup this[int _key]
        {
            get
            {
                if (cullingGroups.ContainsKey(_key))
                    return cullingGroups[_key];
                else
                    return null;
            }
        }

        /// <summary>
        /// 是否存在公共组
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public bool HasGroup(int _key)
        {
            return cullingGroups.ContainsKey(_key);
        }

        /// <summary>
        /// 获取公共视觉裁剪
        /// </summary>
        public ViewCullingGroup SharedViewCullingGroup
        {
            get
            {
                if (!cullingGroups.ContainsKey(0))
                {
                    var group = RequestNewGroup(0, 2048);
                    var cam = Camera.main;
                    if (cam != null)
                    {
                        group.SetCullingGroupCamera(cam);
                        group.SetWatchPoint(cam.transform);
                    }
                }

                return cullingGroups[0];
            }
        }

        #endregion

    }
}