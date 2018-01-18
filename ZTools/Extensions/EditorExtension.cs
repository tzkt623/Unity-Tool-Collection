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
using System.Reflection;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#endif

namespace ZTools
{
    public static partial class EditorExtension
    {
        public static void SetVariable<T>(this object _object, string _variableName, T _value)
        {
            var fieldInfo = _object.GetType().GetField(_variableName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null)
                throw new System.Exception(string.Format("Couldn't find a field named {0} in class {1}",
                    _variableName, _object.GetType().FullName));

            fieldInfo.SetValue(_object, _value);
        }

        public static void CopyVariable<T>(this object _object, object _targetObject, string _variableName)
        {
            _targetObject.SetVariable<T>(_variableName, _object.GetVariable<T>(_variableName));
        }

        public static T GetVariable<T>(this object _object, string _variableName)
        {
            var fieldInfo = _object.GetType().GetField(_variableName,
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null)
                throw new System.Exception(string.Format("Couldn't find a field named {0} in class {1}",
                    _variableName, _object.GetType().FullName));

            return (T)fieldInfo.GetValue(_object);
        }

        public static FieldInfo[] GetVariables(this object _object, System.Func<FieldInfo, bool> _requirements)
        {
            var fields = _object.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fields == null)
                return new FieldInfo[0];
            else
            {
                List<FieldInfo> list = new List<FieldInfo>();
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (_requirements(fields[i]))
                    {
                        list.Add(fields[i]);
                    }
                }

                return list.ToArray();
            }
        }

        public static FieldInfo[] GetVariables(this object _object, System.Func<FieldInfo, bool> _requirements, bool _includeBaseType)
        {
            if (!_includeBaseType)
            {
                return GetVariables(_object, _requirements);
            }
            else
            {
                List<FieldInfo> list = new List<FieldInfo>();
                var type = _object.GetType();

                while(type != null)
                {
                    var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if(fields != null)
                    {
                        for (int i = 0; i < fields.Length; ++i)
                        {
                            if (_requirements(fields[i]) && !list.Contains(fields[i]))
                            {
                                list.Add(fields[i]);
                            }
                        }
                    }

                    type = type.BaseType;
                }

                return list.ToArray();
            }
        }

        public static void Split(this Rect _rect, float _x, out Rect _left, out Rect _right)
        {
            var results = _rect.Split(_x);
            _left = results[0];
            _right = results[1];
        }

        public static Rect[] Split(this Rect _rect, params float[] _splitAt)
        {
            Rect[] result = new Rect[_splitAt.Length + 1];
            float lastX = _rect.x;

            for (int i = 0; i < _splitAt.Length; ++i)
            {
                result[i] = new Rect(lastX, _rect.y, _splitAt[i], _rect.height);
                lastX += _splitAt[i];
            }

            result[result.Length - 1] = new Rect(lastX, _rect.y, _rect.width - lastX, _rect.height);

            return result;
        }


#if UNITY_EDITOR

        public static bool IsAsset(this GameObject _object)
        {
            return AssetDatabase.IsMainAsset(_object);
        }

        /// <summary>
        /// PingObject("Assets/xxxxx/yyyy.png");
        /// </summary>
        /// <param name="_path"></param>
        public static void PingObject(string _path)
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(_path));
        }

        #region Event

        public static void AddPersistentEvent(this UnityEvent _event, UnityAction _action)
        {
            UnityEventTools.RemovePersistentListener(_event, _action);
            UnityEventTools.AddPersistentListener(_event, _action);
        }

        public static void RemovePersistentEvent(this UnityEvent _event, UnityAction _action)
        {
            UnityEventTools.RemovePersistentListener(_event, _action);
        }

        public static void AddPersistentEvent<T0>(this UnityEvent<T0> _event, UnityAction<T0> _action)
        {
            UnityEventTools.AddPersistentListener(_event, _action);
        }

        #endregion

        #region Spliter

        public static void DrawSpliter(this Editor _editor, Color _color, bool _horizontal = true, bool _space = true,
                                      float _size = 1f, float _startExtend = 0f, float _endExtend = 0f)
        {
            DrawSpliter(_horizontal, _space, _size, _color, _startExtend, _endExtend);
        }

        public static void DrawSpliter(this EditorWindow _editor, Color _color, bool _horizontal = true, bool _space = true,
                                      float _size = 1f, float _startExtend = 0f, float _endExtend = 0f)
        {
            DrawSpliter(_horizontal, _space, _size, _color, _startExtend, _endExtend);
        }

        private static void DrawSpliter(bool _horizontal, bool _space,
                                       float _size, Color _color, float _startExtend, float _endExtend)
        {
            Rect rect;

            if (_horizontal)
            {
                rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(_size));
                rect.xMin -= _startExtend;
                rect.xMax += _endExtend;
            }
            else
            {
                rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.Width(_size));
                rect.yMin -= _startExtend;
                rect.yMax += _endExtend;
            }

            if (_space)
                EditorGUILayout.Space();

            EditorGUI.DrawRect(rect, _color);

            if (_space)
                EditorGUILayout.Space();
        }

        #endregion

        #region Property

        public static bool DrawProperty(this Editor _editor, string _propertyName,
                                       GUIStyle _style = null, params GUILayoutOption[] _options)
        {
            return DrawProperty(_editor.serializedObject, _propertyName, _style, _options);
        }

        public static bool DrawProperty(this SerializedObject _target, string _propertyName,
                                       GUIStyle _style = null, params GUILayoutOption[] _options)
        {
            try
            {
                var property = _target.FindProperty(_propertyName);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(property);

                if (EditorGUI.EndChangeCheck())
                {
                    _target.ApplyModifiedProperties();
                    return true;
                }
                else return false;
            }
            catch
            {
                EditorGUILayout.HelpBox(string.Format("Draw property {0} failed", _propertyName), MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Common

        public static void Save(this Editor _this)
        {
            Save(_this.serializedObject);
        }

        public static void Save(this SerializedObject _object)
        {
            _object.ApplyModifiedProperties();

            if (_object.targetObject is MonoBehaviour)
                EditorSceneManager.MarkSceneDirty(((MonoBehaviour)_object.targetObject).gameObject.scene);
        }

        public static void Save(this SerializedProperty _property)
        {
            Save(_property.serializedObject);
        }

        #endregion

        #region SmartList

        /// <summary>
        /// 创建智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_this">Inspector编辑器</param>
        /// <param name="_list">目标队列</param>
        /// <typeparam name="T">目标队列中的元素类型</typeparam>
        public static ReorderableList CreateList<T>(this Editor _this, IList<T> _list)
        {
            return CreateList(_list);
        }

        /// <summary>
        /// 创建智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_this">EditorWindow编辑器</param>
        /// <param name="_list">目标队列</param>
        /// <typeparam name="T">目标队列中的元素类型</typeparam>
        public static ReorderableList CreateList<T>(this EditorWindow _this, IList<T> _list)
        {
            return CreateList(_list);
        }

        /// <summary>
        /// 创建智能列表
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="_list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static ReorderableList CreateList<T>(this IList<T> _list)
        {
            return new ReorderableList((IList)_list, typeof(T));
        }

        public static ReorderableList CreateList(this IList _list, System.Type _type)
        {
            return new ReorderableList(_list, _type);
        }

        public static ReorderableList SetHeaderHeight(this ReorderableList _list,
                                                     float _height)
        {
            _list.headerHeight = _height;

            return _list;
        }

        public static ReorderableList SetHeader(this ReorderableList _list,
                                               ReorderableList.HeaderCallbackDelegate _drawHeader)
        {
            _list.drawHeaderCallback = _drawHeader;

            return _list;
        }

        public static ReorderableList SetHeader(this ReorderableList _list,
                                               string _title, string _toolTip, Texture2D _icon, GUIStyle _style = null)
        {
            return SetHeader(_list, (rect) => {
                if (_style == null)
                    EditorGUI.LabelField(rect, new GUIContent(_title, _icon, _toolTip));
                else
                    EditorGUI.LabelField(rect, new GUIContent(_title, _icon, _toolTip), _style);
            });
        }

        public static ReorderableList SetFooterHeight(this ReorderableList _list,
                                                     float _height)
        {
            _list.footerHeight = _height;

            return _list;
        }

        public static ReorderableList SetDraggable(this ReorderableList _list,
                                                  bool _draggable)
        {
            _list.draggable = _draggable;

            if (!_draggable)
                _list.onReorderCallback = null;

            return _list;
        }

        public static ReorderableList SetOnReordered(this ReorderableList _list,
                                                    ReorderableList.ReorderCallbackDelegate _onReordered)
        {
            _list.draggable = true;
            _list.onReorderCallback = _onReordered;

            return _list;
        }

        public static ReorderableList SetAddFunction(this ReorderableList _list,
                                                    ReorderableList.AddCallbackDelegate _add)
        {
            if (_add == null)
            {
                _list.displayAdd = false;
            }
            else
            {
                _list.onAddCallback = _add;
            }

            return _list;
        }

        public static ReorderableList SetRemoveAtFunction(this ReorderableList _list,
                                                         ReorderableList.RemoveCallbackDelegate _remove)
        {
            if (_remove == null)
            {
                _list.displayRemove = false;
            }
            else
            {
                _list.onRemoveCallback = _remove;
            }

            return _list;
        }

        public static ReorderableList SetDrawEachFunction(this ReorderableList _list,
                                                         ReorderableList.ElementCallbackDelegate _drawEach)
        {
            _list.drawElementCallback = _drawEach;

            return _list;
        }

        public static ReorderableList SetElementHeight(this ReorderableList _list,
                                                      ReorderableList.ElementHeightCallbackDelegate _getHeight)
        {
            _list.elementHeightCallback = _getHeight;

            return _list;
        }

        public static ReorderableList SetElementHeight(this ReorderableList _list,
                                                      float _constantHeight)
        {
            return SetElementHeight(_list,
                (index) => {
                    return _constantHeight;
                });
        }

        public static ReorderableList SetSaveable(this ReorderableList _list)
        {
            if (_list.serializedProperty != null)
            {
                _list.onChangedCallback += (reorderList) => {
                    Save(reorderList.serializedProperty);
                };
            }

            return _list;
        }

        public static ReorderableList SetOnChanged(this ReorderableList _list,
                                                  ReorderableList.ChangedCallbackDelegate _onChanged)
        {
            _list.onChangedCallback += _onChanged;

            return _list;
        }

        #region Common Use SmartList

        /// <summary>
        /// 创建一个默认的Int类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        /// <param name="_allowReorder">如果为<c>true</c>则允许改变顺序</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList<int> _list, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true, bool _allowReorder = true)
        {
            var list = _list.CreateList()
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.IntField(rect,
                    string.Format("Index[{0}]", index), _list[index]);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(0);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            if (_allowReorder)
                list.SetOnReordered((L) => {
                    _editor.Save();
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        /// <summary>
        /// 创建一个默认的Bool类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList<bool> _list, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true)
        {
            var list = _list.CreateList()
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.Toggle(rect,
                    string.Format("Index[{0}]", index), _list[index]);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(false);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        /// <summary>
        /// 创建一个默认的Float类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        /// <param name="_allowReorder">如果为<c>true</c>则允许改变顺序</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList<float> _list, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true, bool _allowReorder = true)
        {
            var list = _list.CreateList()
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.FloatField(rect,
                    string.Format("Index[{0}]", index), _list[index]);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(0f);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            if (_allowReorder)
                list.SetOnReordered((L) => {
                    _editor.Save();
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        /// <summary>
        /// 创建一个默认的String类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        /// <param name="_allowReorder">如果为<c>true</c>则允许改变顺序</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList<string> _list, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true, bool _allowReorder = true)
        {
            var list = _list.CreateList()
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.TextField(rect,
                    string.Format("Index[{0}]", index), _list[index]);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(string.Empty);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            if (_allowReorder)
                list.SetOnReordered((L) => {
                    _editor.Save();
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        /// <summary>
        /// 创建一个默认的Vector3类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        /// <param name="_allowReorder">如果为<c>true</c>则允许改变顺序</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList<Vector3> _list, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true, bool _allowReorder = true)
        {
            var list = _list.CreateList()
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.Vector3Field(rect,
                    string.Format("Index[{0}]", index), _list[index]);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(Vector3.zero);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            if (_allowReorder)
                list.SetOnReordered((L) => {
                    _editor.Save();
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        /// <summary>
        /// 创建一个默认的Unity可序列化（拖拽）类型的智能列表
        /// </summary>
        /// <returns>智能列表</returns>
        /// <param name="_editor">Inspector编辑器对象</param>
        /// <param name="_list">目标列表</param>
        /// <param name="_objectType">Unity可序列化对象类型</param>
        /// <param name="_title">列表标头</param>
        /// <param name="_allowAdd">如果为<c>true</c>则允许添加新元素</param>
        /// <param name="_allowRemove">如果为<c>true</c>则允许删除元素</param>
        /// <param name="_allowReorder">如果为<c>true</c>则允许改变顺序</param>
        public static ReorderableList CreateList(this Editor _editor,
                                                IList _list, System.Type _objectType, string _title = null,
                                                bool _allowAdd = true, bool _allowRemove = true, bool _allowReorder = true)
        {
            var list = _list.CreateList(_objectType)
            .SetDrawEachFunction((rect, index, isActive, focous) => {
                _list[index] = EditorGUI.ObjectField(rect,
                    string.Format("Index[{0}]", index), (Object)_list[index], _objectType, true);
            }).SetElementHeight(30);

            if (!string.IsNullOrEmpty(_title))
                list.SetHeader(_title, null, null);

            if (_allowAdd)
                list.SetAddFunction((L) => {
                    L.list.Add(null);
                });

            if (_allowRemove)
                list.SetRemoveAtFunction((L) => {
                    L.list.RemoveAt(L.index);
                });

            if (_allowReorder)
                list.SetOnReordered((L) => {
                    _editor.Save();
                });

            list.SetOnChanged((L) => {
                _editor.Save();
            });

            return list;
        }

        #endregion

        #endregion

#endif
    }
}