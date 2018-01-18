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
using DG.Tweening;
using DG;
using ZTools.SingletonNS;
using System;

namespace ZTools.PluginExtension.DoTween
{
    public sealed class GlobalDotweenManager : Singleton<GlobalDotweenManager>
    {
        private Dictionary<object, Tween> excutingTweens;
        public enum AppendType
        {
            Kill,
            CompleteAndKill,
            Append
        }

        public override void FixedUpdate()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            excutingTweens = new Dictionary<object, Tween>();
        }

        public override void Reload()
        {
            foreach (var tween in excutingTweens)
            {
                tween.Value.Kill(false);
            }

            excutingTweens.Clear();
        }

        public override void Unload()
        {
        }

        public override void Update()
        {
        }

        /// <summary>
        /// 将一个Tween添加到管理器中
        /// 这样当有新的请求发生的时候，可以查询
        /// 
        /// 当Tween完成后，不会自动清除
        /// 需要手动清除
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="_tween"></param>
        /// <param name="_appendType"></param>
        public void AddTween(object _object, Tween _tween, AppendType _appendType)
        {
            if (excutingTweens.ContainsKey(_object))
            {
                if (_appendType == AppendType.Kill)
                    excutingTweens[_object].Kill(false);
                else
                    excutingTweens[_object].Kill(true);

                excutingTweens.Remove(_object);
            }

            excutingTweens.Add(_object, _tween);
        }

        /// <summary>
        /// 取消Tween
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="_kill"></param>
        /// <param name="_complete"></param>
        public void CancelTween(object _object, bool _kill, bool _complete)
        {
            if (excutingTweens.ContainsKey(_object))
            {
                if (_kill)
                {
                    excutingTweens[_object].Kill(_complete);
                }

                excutingTweens.Remove(_object);
            }
        }

        /// <summary>
        /// 是否正在Tweening
        /// </summary>
        /// <param name="_object"></param>
        /// <returns></returns>
        public bool IsTweening(object _object)
        {
            return excutingTweens.ContainsKey(_object);
        }
    }
}
