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
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZTools;

namespace ZTools.DataStructures
{
    public static partial class EditorExtension
    {
        /// <summary>
        /// 给SDictionary创建一个可在面板中快速调用的界面
        /// Key和Value分开绘制
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="_dictionary"></param>
        /// <param name="_title">标题</param>
        /// <param name="_elementHeight">单元素的高度</param>
        /// <param name="_keyWidth">Key的宽度百分比（0.0-1.0）</param>
        /// <param name="_drawKey">绘制Key的方法，返回修改后的值</param>
        /// <param name="_drawValue">绘制Value的方法，返回修改后的值</param>
        /// <param name="_save">保存</param>
        /// <returns></returns>
        public static SDictionaryDrawer<TKey, TValue> CreateInspectorDrawer<TKey, TValue>(this SDictionary<TKey, TValue> _dictionary,
             Func<Rect, TKey, TKey> _drawKey, Func<Rect, TValue, TValue> _drawValue, Action _save,
             string _title = "字典", float _elementHeight = 30, float _keyWidth = 0.3f)
        {
            if (_dictionary == null)
            {
                Debug.LogError("传入的Dictionary为空");
                return null;
            }
            else
                return new SDictionaryDrawer<TKey, TValue>(_dictionary, _title, _elementHeight, _keyWidth,
                    _drawKey, _drawValue, _save);
        }

        /// <summary>
        /// 给SDictionary创建一个可在面板中快速调用的界面
        /// Key和Value同时绘制
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="_dictionary"></param>
        /// <param name="_title">标题</param>
        /// <param name="_elementHeight">单元素的高度</param>
        /// <param name="_keyWidth">Key的宽度百分比（0.0-1.0）</param>
        /// <param name="_save">保存</param>
        /// <param name="_drawKeyAndValue">同时绘制Key和Value的方法</param>
        /// <returns></returns>
        public static SDictionaryDrawer<TKey, TValue> CreateInspectorDrawer<TKey, TValue>(this SDictionary<TKey, TValue> _dictionary,
             Func<Rect, TKey, TValue, KeyValuePair<TKey, TValue>> _drawKeyAndValue, Action _save,
             string _title = "字典", float _elementHeight = 30)
        {
            if (_dictionary == null)
            {
                Debug.LogError("传入的Dictionary为空");
                return null;
            }
            else
                return new SDictionaryDrawer<TKey, TValue>(_dictionary, _title, _elementHeight,
                    _drawKeyAndValue, _save);
        }
    }

    /// <summary>
    /// 可序列化字典的Inspector绘制辅助类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SDictionaryDrawer<TKey, TValue>
    {
        private SDictionary<TKey, TValue> source;
        private List<KeyValuePair<TKey, TValue>> keys;
        private ReorderableList keyList;
        private Action saveAction;
        private Func<Rect, TKey, TKey> drawKey;
        private Func<Rect, TValue, TValue> drawValue;
        private Func<Rect, TKey, TValue, KeyValuePair<TKey, TValue>> drawKeyAndValues;
        private bool dirty;
        private float keyWidth;

        public ReorderableList KeyList { get { return keyList; } }

        private SDictionaryDrawer(SDictionary<TKey, TValue> _targetDictionary, string _title, float _elementHeight, float _keyWidth, Action _save)
        {
            source = _targetDictionary;
            saveAction = _save;
            keyWidth = _keyWidth;

            var dictionaryKeys = _targetDictionary.Keys;
            if (dictionaryKeys == null) throw new Exception("目标Dictionary的Keys不应当为空");

            this.keys = new List<KeyValuePair<TKey, TValue>>();

            for (int i = 0; i < dictionaryKeys.Count; ++i)
            {
                keys.Add(new KeyValuePair<TKey, TValue>(
                    dictionaryKeys[i],
                    _targetDictionary[dictionaryKeys[i]]
                ));
            }

            keyList = keys.CreateList()
                .SetAddFunction(Add)
                .SetRemoveAtFunction(Remove)
                .SetDrawEachFunction(Draw)
                .SetElementHeight(_elementHeight)
                .SetOnReordered(OnReorder)
                .SetHeader(_title, null, null);
        }

        public SDictionaryDrawer(SDictionary<TKey, TValue> _targetDictionary, string _title, float _elementHeight, float _keyWidth,
            Func<Rect, TKey, TKey> _drawKey, Func<Rect, TValue, TValue> _drawValue, Action _save) : this(_targetDictionary, _title,
                _elementHeight, _keyWidth, _save)
        {
            drawKey = _drawKey;
            drawValue = _drawValue;
        }

        public SDictionaryDrawer(SDictionary<TKey, TValue> _targetDictionary, string _title, float _elementHeight, 
            Func<Rect, TKey, TValue, KeyValuePair<TKey, TValue>> _drawKeyAndValues, Action _save) : this(_targetDictionary, _title,
                _elementHeight, 1.0f, _save)
        {
            drawKeyAndValues = _drawKeyAndValues;
        }

        private void Draw(Rect _rect, int _index, bool _isActive, bool _focus)
        {
            _rect.y += 1;
            _rect.height -= 4;

            EditorGUI.BeginChangeCheck();

            if (drawKeyAndValues == null)
            {
                var rects = _rect.Split(Mathf.Abs(_rect.width) * keyWidth, 10);
                var node = keys[_index];
                var newKey = drawKey(rects[0], node.Key);
                var newValue = drawValue(rects[2], node.Value);
                keys[_index] = new KeyValuePair<TKey, TValue>(newKey, newValue);
            }
            else
            {
                var node = keys[_index];
                keys[_index] = drawKeyAndValues(_rect, node.Key, node.Value);
            }

            if (EditorGUI.EndChangeCheck())
                dirty = true;
        }

        private void Add(ReorderableList _list)
        {
            keys.Add(new KeyValuePair<TKey, TValue>(default(TKey), default(TValue)));

            dirty = true;
        }

        private void Remove(ReorderableList _list)
        {
            keys.RemoveAt(_list.index);

            dirty = true;
        }

        private void OnReorder(ReorderableList _list)
        {
            dirty = true;
        }

        private void WriteToSource()
        {
            try
            {
                source.Clear();
                for (int i = 0; i < keys.Count; ++i)
                {
                    source.Add(keys[i].Key, keys[i].Value);
                }
                saveAction();
                dirty = false;
            }
            catch (Exception _e)
            {
                EditorUtility.DisplayDialog("失败", "无法保存 原因 : " + _e.Message, "确认");
            }
        }

        public void DrawInspector()
        {
            keyList.DoLayoutList();

            if (dirty)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                EditorGUILayout.HelpBox("必须手动保存，否则无效!", MessageType.Warning);
                if (GUILayout.Button("保存", GUILayout.Height(40)))
                    WriteToSource();

                EditorGUILayout.EndHorizontal();
            }
        }

        public List<KeyValuePair<TKey, TValue>> ToList()
        {
            return keys;
        }
    }
}
#endif
