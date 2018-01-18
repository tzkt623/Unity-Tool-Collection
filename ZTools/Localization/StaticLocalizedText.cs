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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace ZTools.LocalizationNS
{
    [DefaultExecutionOrder(150)]
    public abstract class StaticLocalizedText : MonoBehaviour
    {
        //public int id;
        public string text;

        private void Start()
        {
            Strings.onLanguageLoaded += OnLanguageLoaded;
            OnLanguageLoaded();
        }

        private void OnDestroy()
        {
            Strings.onLanguageLoaded -= OnLanguageLoaded;
        }

        private void OnLanguageLoaded()
        {
            //if (id == -1)
            //    ApplyText(Strings.Get(text));
            //else
            //    ApplyText(Strings.Get(id));

            ApplyText(Strings.Get(text));
        }

        protected abstract void ApplyText(string _text);

//#if UNITY_EDITOR
//        [MenuItem("Debug/替换所有ID字符")]
//        public static void ChangeAllIDToString()
//        {
//            Dictionary<string, int> oldMapping;
//            TwoWayDictionary<string, int> twoWayDictionary;
//            Dictionary<int, string> oldValues;
//            List<StaticLocalizedText> cache = new List<StaticLocalizedText>();

//            Strings.Editor_GetOriginalData(ConfigDefines.LanguagePath, Strings.LoadedLanguage, out oldMapping, out oldValues);

//            twoWayDictionary = new TwoWayDictionary<string, int>();
//            foreach (var m in oldMapping)
//            {
//                twoWayDictionary.Add(m);
//            }

//            for (int i = 0, count = EditorSceneManager.sceneCount; i < count; ++i)
//            {
//                var s = EditorSceneManager.GetSceneAt(i);
//                if (s.rootCount > 0)
//                {
//                    foreach (var root in s.GetRootGameObjects())
//                    {
//                        cache.Clear();
//                        root.GetComponentsInChildren<StaticLocalizedText>(true, cache);
//                        foreach (var c in cache)
//                        {
//                            if (c.id != -1)
//                            {
//                                if (!twoWayDictionary.ContainsValue(c.id))
//                                    Debug.LogError("脚本上的ID是错误的，手动改变", c.gameObject);
//                                else
//                                {
//                                    c.text = twoWayDictionary.GetKeyByValue(c.id);
//                                    c.id = -1;
//                                    Debug.Log($"修改了{c.gameObject.GetFullName()}");
//                                    EditorUtility.SetDirty(c);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//#endif
    }

//#if UNITY_EDITOR

//    [CustomEditor(typeof(StaticLocalizedText), true)]
//    [CanEditMultipleObjects()]
//    public class StaticLocalizedText_Editor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            EditorGUI.BeginChangeCheck();

//            if (targets.Length > 1)
//            {
//                EditorGUILayout.LabelField("* Editing multiple component..");
//                EditorGUI.BeginChangeCheck();
//                DrawInspectorGUI((StaticLocalizedText)targets[0]);
//                if (EditorGUI.EndChangeCheck())
//                {
//                    for (int i = 1; i < targets.Length; ++i)
//                    {
//                        EditorUtility.CopySerialized(targets[0], targets[i]);
//                    }
//                }
//            }
//            else
//            {
//                DrawInspectorGUI((StaticLocalizedText)target);
//            }

//            if (EditorGUI.EndChangeCheck())
//                this.Save();
//        }

//        protected virtual void DrawInspectorGUI(StaticLocalizedText _target)
//        {
//            bool useID = _target.id != -1;
//            EditorGUILayout.BeginHorizontal();
//            bool newUseID = EditorGUILayout.ToggleLeft("USE ID", useID);

//            if (useID != newUseID)
//            {
//                useID = newUseID;
//                if (useID)
//                {
//                    _target.id = 0;
//                    _target.text = null;
//                }
//                else
//                {
//                    _target.text = "Enter NakeName";
//                    _target.id = -1;
//                }
//            }

//            if (useID)
//            {
//                _target.id = EditorGUILayout.IntField(_target.id);
//            }
//            else
//            {
//                _target.text = EditorGUILayout.TextField(_target.text);
//            }

//            EditorGUILayout.EndHorizontal();
//        }
//    }

//#endif
}
