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
using UnityEngine.AI;

namespace ZTools.NavigationNS.ExampleNS
{
    internal class WorldMovingNavigationCube : MonoBehaviour, INavigationSourceProvider
    {
#pragma warning disable 649

        public int area;
        public Transform rotateCenter;
        public float rotation;

        private int index = -1;

#pragma warning restore 649

        #region Interface INavigationSourceProvider

        NavMeshBuildSource INavigationSourceProvider.GetSource()
        {
            var source = new NavMeshBuildSource();
            source.component = this;
            source.area = area;
            source.shape = NavMeshBuildSourceShape.Box;
            source.transform = transform.localToWorldMatrix;
            source.size = GetComponent<BoxCollider>().size;

            return source;
        }

        int INavigationSourceProvider.GetSourceIndex()
        {
            return index;
        }

        void INavigationSourceProvider.WriteSourceIndex(int _index)
        {
            index = _index;
        }

        #endregion

        private void Update()
        {
            rotateCenter.Rotate(0, rotation * Time.deltaTime, 0);
            GetComponentInParent<WorldNavigationBaker>().baker.UpdateSource(this);
        }
    }
}