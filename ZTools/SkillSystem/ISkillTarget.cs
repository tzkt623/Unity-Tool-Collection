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
    public enum SkillTargetDeflectType
    {
        None,
        Block,
        Revert,
    }

    /// <summary>
    /// 可作为技能目标的对象，必须实现该接口
    /// </summary>
    public interface ISkillTarget
    {


        /// <summary>
        /// 技能目标的Transform，用来得知技能目标当前的位置
        /// </summary>
        Transform TargetTransform { get; }

        /// <summary>
        /// 技能命中目标，目中点的坐标
        /// </summary>
        Vector3 HitPosition { get; set; }

        /// <summary>
        /// 技能命中目标，命中点的法线
        /// </summary>
        Vector3 HitNormal { get; set; }

        /// <summary>
        /// 技能会命中的层级
        /// </summary>
        int Layer { get; }

        /// <summary>
        /// 是否可以被当做目标？例如已经死亡或者关闭就不允许当做目标
        /// </summary>
        bool Valid { get; }

        /// <summary>
        /// 是否反弹伤害
        /// </summary>
        SkillTargetDeflectType DeflectDMG { get; }

        /// <summary>
        /// 当成功反弹一个技能的时候触发的回调
        /// </summary>
        /// <param name="_deflectSkillID"></param>
        void OnSuceessDeflect(int _deflectSkillID);
    }
}
