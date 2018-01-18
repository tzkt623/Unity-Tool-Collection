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

namespace ZTools
{
    [System.Serializable]
    public struct Int2
    {
        public int x;
        public int y;

        public Int2(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    public struct Int3
    {
        public int x;
        public int y;
        public int z;

        public Int3(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Int3(Int2 _xy, int _z)
        {
            x = _xy.x;
            y = _xy.y;
            z = _z;
        }
    }

    public struct Int4
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public Int4(int _x, int _y, int _z, int _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public Int4(Int2 _xy, int _z, int _w)
        {
            x = _xy.x;
            y = _xy.y;
            z = _z;
            w = _w;
        }
    }

    public struct Short2
    {
        public short x;
        public short y;
    }

    public struct Vector5
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public float q;

        public Vector5(Vector2 _pos2D, float _rotation2D, Vector2 _size2D)
        {
            x = _pos2D.x;
            y = _pos2D.y;

            z = _rotation2D;

            w = _size2D.x;
            q = _size2D.y;
        }
    }
}