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

namespace ZTools.DataStructures
{
    public struct BitArray32
    {
        private uint data;

        public uint Data { get { return data; } }

        public BitArray32(uint _data)
        {
            data = _data;
        }

        public bool this[int _bit]
        {
            get
            {
                return (data & ((uint)1 << _bit)) != 0;
            }

            set
            {
                //如果是True，说明原来的值是FALSE，也就是原来该位置是0
                //因此只需要+=
                if (value)
                {
                    data |= ((uint)1 << _bit);
                }
                //如果是FALSE, 说明原来是TRUE，也就是原来该值的位置是1
                //因此只需要-=
                else
                {
                    data &= ~((uint)1 << _bit);
                }
            }
        }
    }
}
