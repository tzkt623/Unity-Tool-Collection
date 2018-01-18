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
using System.Collections.Generic;
using ZTools.SingletonNS;
using System;
using ZTools.CSVNS;

namespace ZTools.SkillSystemNS
{
    public interface ISkillValueModifierCollection<T> : IDisposable
        where T : class, ISkillValueModifier
    {
        T GetModifier(int _skillID);
        bool IsDisposed { get; }
    }

    public interface ISkillValueModifier
    {
        int ReplaceID { get; }

        float Value { get; }

        float Range { get; }
    }

    /// <summary>
    /// 技能系统
    /// </summary>
    /// <typeparam name="TClass">技能系统本身类型，限定Instance返回的类型</typeparam>
    /// <typeparam name="TSkillData">技能系统所需的SkillData类，必须继承于SkillData</typeparam>
    /// <typeparam name="TSkillTargeting">技能系统所需的SkillTarget类，必须继承于SkillTarget</typeparam>
    /// <typeparam name="TSkillInterface">技能系统所采用的技能接口，该接口必须继承于ISkill</typeparam>
    /// <typeparam name="TSkillCasterInterface">技能系统所采用的施放者接口，该接口必须继承于ISkillCaster</typeparam>
    /// <typeparam name="TSkillTargetInterface">技能系统所采用的目标接口，该接口必须继承于ISkillTarget</typeparam>
    public abstract class SkillSystem<TClass, TSkillData, TSkillTargeting, TSkillInterface, TSkillCasterInterface, TSkillTargetInterface, TSkillModifierCollection, TSkillModifier> : Singleton<TClass>
        where TSkillData : SkillData
        where TSkillTargeting : SkillTargeting<TSkillCasterInterface, TSkillTargetInterface>
        where TSkillInterface : ISkill<TSkillData, TSkillCasterInterface, TSkillTargetInterface, TSkillModifierCollection, TSkillModifier>
        where TSkillCasterInterface : ISkillCaster<TSkillCasterInterface>
        where TSkillTargetInterface : ISkillTarget
        where TSkillModifierCollection : class, ISkillValueModifierCollection<TSkillModifier>
        where TSkillModifier : class, ISkillValueModifier
        where TClass : SkillSystem<TClass, TSkillData, TSkillTargeting, TSkillInterface, TSkillCasterInterface, TSkillTargetInterface, TSkillModifierCollection, TSkillModifier>, new()
    {
        protected Dictionary<int, TSkillData> skillNodes;
        protected Dictionary<int, TSkillTargeting> skillTargetings;
        protected TSkillTargetInterface[] skillTargetsBuffer;
        protected LayerMask interactionLayer;

        protected class CastRequest
        {
            public float delayTimer;
            public TSkillCasterInterface caster;
            public TSkillCasterInterface overrideCaster;
            public int id;
            public int hashCaster;
            public int hashOverrideCaster;
            public TSkillModifierCollection collection;

            public CastRequest(float _delay, TSkillCasterInterface _caster, TSkillCasterInterface _overrideCaster, int _id, TSkillModifierCollection _collection)
            {
                delayTimer = _delay;
                caster = _caster;
                hashCaster = caster.ReferenceID;
                overrideCaster = _overrideCaster;
                if (overrideCaster == null)
                    overrideCaster = caster;
                hashOverrideCaster = overrideCaster.ReferenceID;
                id = _id;
                collection = _collection;
            }

            public bool Valid()
            {
                return caster != null && caster.Valid(hashCaster) && overrideCaster != null && overrideCaster.Valid(hashOverrideCaster);
            }
        }

        protected List<CastRequest> requests;

        #region MONO
        public override void FixedUpdate()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Load()
        {
            requests = new List<CastRequest>();
        }

        public override void Reload()
        {
            requests.Clear();
        }

        public override void Unload()
        {
            requests = null;
        }

        public override void Update()
        {
            for (int i = 0; i < requests.Count; ++i)
            {
                requests[i].delayTimer -= Time.deltaTime;
                if (requests[i].delayTimer <= 0f)
                {
                    var r = requests[i];

                    if (r.Valid())
                        CastSpell(r.caster, r.id, r.overrideCaster, true, r.collection);

                    //快速移除
                    requests[i] = requests[requests.Count - 1];
                    requests.RemoveAt(requests.Count - 1);
                    --i;
                }
            }
        }

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_skillsConfigPath"></param>
        /// <param name="_skillTargetingsConfigPath"></param>
        public void Initilize(string _skillsConfigPath, string _skillTargetingsConfigPath, LayerMask _interactionLayer)
        {
            skillTargetsBuffer = new TSkillTargetInterface[128];
            LoadSkills(_skillsConfigPath);
            LoadSkillTargetings(_skillTargetingsConfigPath);
            interactionLayer = _interactionLayer;
        }

        /// <summary>
        /// 发射技能
        /// </summary>
        /// <param name="_caster"></param>
        /// <param name="_id"></param>
        public virtual bool CastSpell(TSkillCasterInterface _caster, int _id, TSkillCasterInterface _overrideCaster, bool _ignoreDelay = false, TSkillModifierCollection _collection = null)
        {
            if (skillNodes == null || !skillNodes.ContainsKey(_id))
                throw new Exception(string.Format("不存在{0}的技能！", _id));

            var skill = skillNodes[_id];

            if (!_ignoreDelay && skill.delay > 0)
            {
                requests.Add(new CastRequest(skill.delay, _caster, _overrideCaster, _id, _collection));
                return false;
            }
            else
            {
                var modifier = (_collection?.IsDisposed ?? true) ? null : _collection.GetModifier(_id);
                if (modifier != null && modifier.ReplaceID != 0)
                {
                    LoadRequiredAssets(modifier.ReplaceID);
                    return CastSpell(_caster, modifier.ReplaceID, _overrideCaster, true, _collection);
                }

                var spawnPosition = _caster.SpawnPosition;
                var spawnDirection = _caster.SpawnForward;

                if (_caster.FxCache == null)
                {
                    IReuseableFX reuseableFXCache;
                    var fx = GetFX(skill.fxID, out reuseableFXCache);

                    if (fx != null)
                    {
                        _caster.FxCache = reuseableFXCache;
                        reuseableFXCache = null;

                        fx.transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(spawnDirection));
                        fx.SetActive(true);
                    }
                }
                else
                {
                    _caster.FxCache.Refresh();
                }

                var skillTargeting = GetSkillTargeting(skill.skillTargetingID);
                int targetCount;
                if (modifier != null)
                {
                    var originalValue = skillTargeting.range;
                    skillTargeting.range = (int)modifier.Range;
                    targetCount = skillTargeting.FindTargets(_caster, ref skillTargetsBuffer, interactionLayer);
                    skillTargeting.range = originalValue;
                }
                else
                {
                    targetCount = skillTargeting.FindTargets(_caster, ref skillTargetsBuffer, interactionLayer);
                }

                if (skillTargeting.skillSharedByAllTargets)
                {
                    if (targetCount > 0)
                    {
                        var ammo = GetAmmo(skill.ammoID);
                        ammo.Owner = _overrideCaster == null ? _caster : _overrideCaster;
                        ammo.Layer = ammo.Owner.Layer;

                        if (modifier != null)
                        {
                            var originalValue = skill.value;
                            skill.value = modifier.Value;
                            ammo.SetValue(GetBulletReferenceID(), skill, _collection);
                            skill.value = originalValue;
                        }
                        else
                        {
                            ammo.SetValue(GetBulletReferenceID(), skill, _collection);
                        }

                        var finalIndex = targetCount - 1;
                        for (int i = 0; i < finalIndex; ++i)
                        {
                            ammo.Target = skillTargetsBuffer[i];
                            ammo.Excute();
                        }

                        ammo.Target = skillTargetsBuffer[finalIndex];
                        ammo.Fire(ref spawnPosition, ref spawnDirection);
                    }
                }
                else
                {
                    for (int i = 0; i < targetCount; ++i)
                    {
                        var ammo = GetAmmo(skill.ammoID);

                        if (ammo != null)
                        {
                            ammo.Owner = _overrideCaster == null ? _caster : _overrideCaster;
                            ammo.Target = skillTargetsBuffer[i];
                            ammo.Layer = ammo.Owner.Layer;

                            if (modifier != null)
                            {
                                var originalValue = skill.value;
                                skill.value = modifier.Value;
                                ammo.SetValue(GetBulletReferenceID(), skill, _collection);
                                skill.value = originalValue;
                            }
                            else
                            {
                                ammo.SetValue(GetBulletReferenceID(), skill, _collection);
                            }

                            ammo.Fire(ref spawnPosition, ref spawnDirection);
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 获取技能数据
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public TSkillData GetSkill(int _id)
        {
            return skillNodes[_id];
        }

        /// <summary>
        /// 获取技能目标算法
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public TSkillTargeting GetSkillTargeting(int _id)
        {
            return skillTargetings[_id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_id"></param>
        public abstract void LoadRequiredAssets(int _id);

        /// <summary>
        /// 加载技能节点
        /// </summary>
        /// <param name="_path"></param>
        protected abstract void LoadSkills(string _path);

        /// <summary>
        /// 加载技能目标判定
        /// </summary>
        /// <param name="_path"></param>
        protected abstract void LoadSkillTargetings(string _path);

        /// <summary>
        /// 获取特效的方式
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        protected abstract GameObject GetFX(int _id, out IReuseableFX _fxIfReuseable);

        /// <summary>
        /// 获取技能Prefab的方式
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        protected abstract TSkillInterface GetAmmo(int _id);

        private int m_lastReferenceID = int.MinValue;
        private int GetBulletReferenceID()
        {
            ++m_lastReferenceID;
            if (m_lastReferenceID == 0)
                ++m_lastReferenceID;
            return m_lastReferenceID;
        }
    }
}