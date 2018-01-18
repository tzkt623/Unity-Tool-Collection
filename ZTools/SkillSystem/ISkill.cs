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
using System;

namespace ZTools.SkillSystemNS
{
    /// <summary>
    /// 实现该接口的对象，可以被当作技能
    /// </summary>
    public interface ISkill<TSkillData, TSkillCasterInterfance, TSkillTargetInterface, TSkillModifierCollection, TSkillModifier>
        where TSkillCasterInterfance : ISkillCaster<TSkillCasterInterfance>
        where TSkillTargetInterface : ISkillTarget
        where TSkillModifierCollection : class, ISkillValueModifierCollection<TSkillModifier>
        where TSkillModifier : class, ISkillValueModifier
        where TSkillData : SkillData
    {
        /// <summary>
        /// 技能的施放者
        /// </summary>
        TSkillCasterInterfance Owner { get; set; }

        /// <summary>
        /// 技能的目标
        /// </summary>
        TSkillTargetInterface Target { get; set; }

        /// <summary>
        /// 技能Data
        /// </summary>
        int SkillID {get;}

        /// <summary>
        /// ReferenceID
        /// </summary>
        int ReferenceID { get; }

        /// <summary>
        /// 技能所在的层级
        /// </summary>
        int Layer { get; set; }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="_node"></param>
        void SetValue(int _referenceID, TSkillData _node, TSkillModifierCollection _collection);

        /// <summary>
        /// 发射特效
        /// </summary>
        /// <param name="_position"></param>
        /// <param name="_forwardDirection"></param>
        void Fire(ref Vector3 _position, ref Vector3 _forwardDirection);

        /// <summary>
        /// 执行伤害
        /// </summary>
        void Excute();

        /// <summary>
        /// 中断
        /// </summary>
        void Interrupt();
    }
}