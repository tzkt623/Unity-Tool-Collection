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
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace ZTools.EditorNS
{
    sealed class SceneLocator : EditorWindow
    {
        #region 说明
        //"{"describe":"Describe","scenes":[" / _MyAssets / Scenes / TestVRRoom.unity"," / _MyAssets / Scenes / test_other.unity"]}"
        #endregion

        private static SceneLocator instance;

        [MenuItem("Tools/Scenes #&s")]
        public static void ShowSceneLocator()
        {
            if (instance != null)
                return;

            var window = CreateInstance<SceneLocator>();
            instance = window;
            window.ShowPopup();

            var size = new Vector2(300, 300);
            var center = new Vector2(Screen.currentResolution.width / 2 - size.x / 2, Screen.currentResolution.height / 2 - size.y / 2);

            window.position = new Rect(center, size);
            window.Focus();
        }

        private SceneAsset asset;
        private GUIContent[] buttonContents = new GUIContent[]
        {
        new GUIContent("Load"),
        new GUIContent("LoadAddictive")
        };

#pragma warning disable 649

        [System.Serializable]
        private class SceneBatchCollection
        {
            public SceneBatch[] batches;
        }
        [System.Serializable]
        private class SceneBatch
        {
            public string tag = "common";
            public string describe;
            public string[] scenes;
        }

#pragma warning restore 649

        private SceneBatchCollection currentBatches;
        private Dictionary<string, List<SceneBatch>> taggedBatches;
        private Vector2 scrollView;

        private void CheckBatchJson()
        {
            if (currentBatches == null)
            {
                var setting = Resources.Load<TextAsset>("SceneLocatorSettings");
                if (setting == null)
                {
                    throw new System.Exception("SceneLocatorSettings文件不存在!");
                }
                currentBatches = JsonUtility.FromJson<SceneBatchCollection>(setting.text);
                Resources.UnloadAsset(setting);

                //重构Tag
                taggedBatches = new Dictionary<string, List<SceneBatch>>();
                foreach (var batch in currentBatches.batches)
                {
                    if (taggedBatches.ContainsKey(batch.tag))
                    {
                        taggedBatches[batch.tag].Add(batch);
                    }
                    else
                    {
                        taggedBatches.Add(batch.tag, new List<SceneBatch>());
                        taggedBatches[batch.tag].Add(batch);
                    }
                }
            }
        }

        private void OnGUI()
        {
            CheckBatchJson();
            if (currentBatches == null)
            {
                EditorGUILayout.HelpBox("读取文件错误", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

            //如果按下了ESC，可以推出
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                Close();

            asset = EditorGUILayout.ObjectField("选择加载场景", asset, typeof(SceneAsset), false) as SceneAsset;


            EditorGUI.BeginDisabledGroup(asset == null);

            int selection = -1;

            selection = GUILayout.SelectionGrid(selection, buttonContents, 2);

            switch (selection)
            {
                case 0:
                    {
                        var path = AssetDatabase.GetAssetPath(asset);
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        Close();
                    }
                    break;
                case 1:
                    {
                        var path = AssetDatabase.GetAssetPath(asset);
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        Close();
                    }
                    break;
                default:
                    break;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("按下Esc退出该面板", EditorStyleExtention.Title);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("常用快速访问");
            EditorGUILayout.Space();

            scrollView = EditorGUILayout.BeginScrollView(scrollView);


            foreach (var list in taggedBatches)
            {
                if (EditorGUILayout.Foldout(true, list.Key))
                {
                    foreach (var batch in list.Value)
                    {
                        if (batch.scenes.Length >= 1 && GUILayout.Button(batch.describe))
                        {
                            //如果是场景
                            if (batch.scenes[0].Contains(".unity"))
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.OpenScene(Application.dataPath + batch.scenes[0], OpenSceneMode.Single);

                                    for (int i = 1; i < batch.scenes.Length - 1; ++i)
                                        EditorSceneManager.OpenScene(Application.dataPath + batch.scenes[i], OpenSceneMode.Additive);

                                    var scene = EditorSceneManager.OpenScene(Application.dataPath + batch.scenes[batch.scenes.Length - 1], OpenSceneMode.Additive);
                                    EditorSceneManager.SetActiveScene(scene);
                                }
                            }
                            else //文件
                            {
                                EditorUtility.OpenWithDefaultApp(Application.dataPath + batch.scenes[0]);
                            }

                            this.Close();
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}
#endif
