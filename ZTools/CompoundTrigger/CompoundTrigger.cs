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

namespace ZTools.CompoundTrigger
{
    sealed class CompoundTrigger : MonoBehaviour
    {
        private class ColliderInfo
        {
            public Collider collider;
            public short counter;
            public bool enterMessageSent;

            public bool ShouldFireExitMessage { get { return counter <= 0; } }
            public bool ShouldFireEnterMessage { get { return counter > 0 && !enterMessageSent; } }

            public override int GetHashCode()
            {
                return collider?.GetInstanceID() ?? 0;
            }
        }

        private static List<int> toRemove = new List<int>();

        public MonoBehaviour targetBehavior;

        private const string EnterMethodName = "OnTriggerEnter";
        private const string ExitMethodName = "OnTriggerExit";
        private SortedList<int, ColliderInfo> counter;

        private void Awake()
        {
            counter = new SortedList<int, ColliderInfo>();
        }

        private void OnDisable()
        {
            counter.Clear();
        }

        private void Update()
        {
            foreach (var c in counter)
            {
                var info = c.Value;
                if (info.GetHashCode() != c.Key)
                    toRemove.Add(c.Key);
                else
                {
                    if (info.ShouldFireExitMessage)
                    {
                        toRemove.Add(c.Key);
                        targetBehavior?.SendMessage(ExitMethodName, info.collider, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (info.ShouldFireEnterMessage)
                    {
                        info.enterMessageSent = true;
                        targetBehavior?.SendMessage(EnterMethodName, info.collider, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var key in toRemove)
                {
                    counter.Remove(key);
                }

                toRemove.Clear();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var key = other.GetInstanceID();
            if (counter.ContainsKey(key))
            {
                var info = counter[key];
                info.counter++;
            }
            else
            {
                counter.Add(key, new ColliderInfo()
                {
                    collider = other,
                    counter = 1,
                    enterMessageSent = false
                });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var key = other.GetInstanceID();

            if (counter.ContainsKey(key))
            {
                counter[key].counter--;
            }
        }
    }
}