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

namespace ZTools.SkillSystemNS
{
    /// <summary>
    /// 实现该接口的对象，可以被当作技能释放者
    /// </summary>
    public interface ISkillCaster<TSkillCasterInterface>
        where TSkillCasterInterface : ISkillCaster<TSkillCasterInterface>
    {
        /// <summary>
        /// 由于可能存在子技能的概念
        /// 因此需要存储Root释放者
        /// </summary>
        TSkillCasterInterface Root { get; }

        /// <summary>
        /// 技能发射点
        /// </summary>
        Vector3 SpawnPosition { get; }

        /// <summary>
        /// 技能发射点的旋转
        /// </summary>
        Vector3 SpawnForward { get; }

        /// <summary>
        /// 技能的开火特效缓存
        /// 如果
        /// </summary>
        IReuseableFX FxCache { get; set; }

        /// <summary>
        /// 技能施放者的层级
        /// </summary>
        int Layer { get; }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <param name="_hash"></param>
        /// <returns></returns>
        bool Valid(int _hash);

        /// <summary>
        /// hash ID
        /// </summary>
        int ReferenceID { get; }
    }
}
