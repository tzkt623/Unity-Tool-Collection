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

namespace ZTools.SingletonNS
{
    public abstract class SingletonBase
    {
        /// <summary>
        /// 是否已经加载, 可以使用
        /// 如果已经被卸载, 该值会返回False
        /// </summary>
        public bool Loaded { get; set; }
        public abstract void Load();
        public abstract void Unload();
        public abstract void Reload();
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void FixedUpdate();

        public virtual void ReleaseMemories() { }
        public virtual void OnApplicationFocused() { }
        public virtual void OnApplicationGoesIntoBackground() { }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos() { }
#endif
    }

    public abstract class Singleton<Type> : SingletonBase, IDisposable where Type : Singleton<Type>, new()
    {
        /// <summary>
        /// 单例, 如果不存在, 会自动创建
        /// </summary>
        public static Type Instance
        {
            get
            {
                if (instance == null)
                {
                    Construct();
                }

                return instance;
            }
        }

        /// <summary>
        /// 自动创建
        /// </summary>
        private static void Construct()
        {
            instance = new Type();
            SingletonManager.Regist(instance);
            instance.Load();
            instance.Loaded = true;
        }

        /// <summary>
        /// 直接引用, 不会自动创建, 在OnDestroy时调用最为保险
        /// </summary>
        public static Type DirectInstance
        {
            get
            {
                if (instance != null && instance.Loaded)
                    return instance;
                else
                    return null;
            }
        }

        private static Type instance;

        void IDisposable.Dispose()
        {
            instance = null;
        }
    }
}
