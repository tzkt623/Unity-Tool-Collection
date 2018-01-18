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

namespace ZTools.ViewCulling.Test
{
    sealed class TestViewCullingObject : MonoBehaviour, IViewCullingObject
    {
        private enum Visibility
        {
            Full,
            Near,
            Far,
            Hide
        }

        private int _index = -1;
        int IViewCullingObject.index
        {
            get
            {
                return _index;
            }

            set
            {
                _index = value;
            }
        }

        Vector3 IViewCullingObject.position
        {
            get
            {
                return transform.position;
            }
        }

        float IViewCullingObject.radium
        {
            get
            {
                return 5;
            }
        }

        private Visibility visiblity;
        private Color[] colors = new Color[]
        {
            Color.white,
            Color.green,
            Color.yellow,
            Color.red
        };

        void IViewCullingObject.OnVisibilityChanged(int band)
        {
            visiblity = (Visibility)band;
        }

        void OnEnable()
        {
            ViewCullingManager.Instance[0].AddRequest(this);
        }

        void OnDisable()
        {
            if (ViewCullingManager.DirectInstance != null)
                ViewCullingManager.Instance[0].RemoveRequest(this);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = colors[(int)visiblity];
            Gizmos.DrawWireSphere(transform.position, 5);
        }
    }
}
