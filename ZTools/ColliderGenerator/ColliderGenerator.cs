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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ZTools.ColliderGenerator
{
    public class ColliderGenerator : EditorWindow
    {
        private enum Axis
        {
            X,
            Y,
            Z
        }

        private GameObject tempAdvance;
        private Transform colliderRoot;
        private string prefixName;

        [MenuItem("Tools/Create Collider Generator")]
        public static void OpenWindow()
        {
            var window = EditorWindow.CreateInstance<ColliderGenerator>();
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnGUI()
        {
            if (Selection.activeGameObject != null)
            {
                if (GUILayout.Button("Fix Negative Collider"))
                {
                    FixAllNegativeCollider(Selection.activeGameObject);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if ((Selection.gameObjects?.Length ?? 0) > 1)
            {
                prefixName = EditorGUILayout.TextField("Name", prefixName);

                if (!string.IsNullOrEmpty(prefixName) && GUILayout.Button("Rename All"))
                {
                    RenameAll(Selection.gameObjects, prefixName);
                }
            }
            else
            {
                colliderRoot = EditorGUILayout.ObjectField("Root", colliderRoot, typeof(Transform), true) as Transform;

                EditorGUILayout.Space();

                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MeshRenderer>() != null)
                {
                    if (GUILayout.Button("Create Box Collider"))
                    {
                        CreateBoxCollider(Selection.activeGameObject);
                    }
                }

                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<BoxCollider>() != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Advanced X"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.X, true, Selection.activeGameObject, tempAdvance);
                    }
                    if (GUILayout.Button("Advanced -X"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.X, false, Selection.activeGameObject, tempAdvance);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Advanced Y"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.Y, true, Selection.activeGameObject, tempAdvance);
                    }
                    if (GUILayout.Button("Advanced -Y"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.Y, false, Selection.activeGameObject, tempAdvance);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Advanced Z"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.Z, true, Selection.activeGameObject, tempAdvance);
                    }
                    if (GUILayout.Button("Advanced -Z"))
                    {
                        CheckTempAdvanceGameObject();
                        Scale(Axis.Z, false, Selection.activeGameObject, tempAdvance);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (tempAdvance != null && GUILayout.Button("Confirmed"))
                    {
                        var current = Selection.activeGameObject;
                        Selection.activeGameObject = tempAdvance;
                        var vis = tempAdvance.GetComponent<ColliderVisualizer>();
                        if (vis != null)
                        {
                            DestroyImmediate(vis);
                        }
                        tempAdvance = null;

                        DestroyImmediate(current);
                    }
                }

                EditorGUILayout.Space();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            //if (Event.current.type == EventType.Repaint)
            //{
            //    if (tempAdvance != null)
            //    {
            //        var box = tempAdvance.GetComponent<BoxCollider>();
            //        Handles.CubeHandleCap(0, tempAdvance.transform.TransformPoint(box.center), tempAdvance.transform.rotation, )
            //        Gizmos.color = Color.blue;
            //        Gizmos.matrix = tempAdvance.transform.localToWorldMatrix;
            //        Gizmos.DrawCube(box.center, box.size);
            //    }
            //}
        }

        private void CreateBoxCollider(GameObject _gameObject)
        {
            var collider = _gameObject.AddComponent<BoxCollider>();

            var newObject = new GameObject("collider (1)");
            newObject.transform.parent = _gameObject.transform;
            newObject.transform.localPosition = collider.center;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localScale = Vector3.one;

            var newCollider = newObject.AddComponent<BoxCollider>();
            var size = newObject.transform.lossyScale;
            size.x *= collider.size.x;
            size.y *= collider.size.y;
            size.z *= collider.size.z;
            newCollider.size = size;
            newObject.transform.parent = null;
            newObject.transform.localScale = Vector3.one;

            newObject.transform.parent = colliderRoot;

            Selection.activeGameObject = newObject;

            DestroyImmediate(collider);
        }

        private void CheckTempAdvanceGameObject()
        {
            if (tempAdvance == null)
            {
                tempAdvance = new GameObject("collider (2)");
                tempAdvance.AddComponent<ColliderVisualizer>();
                tempAdvance.transform.parent = colliderRoot;
            }
        }

        private void Scale(Axis _axis, bool _foward, GameObject _original, GameObject _new)
        {
            _new.transform.position = _original.transform.position;
            _new.transform.rotation = _original.transform.rotation;

            var oldCollider = _original.GetComponent<BoxCollider>();

            var newCollider = _new.GetComponent<BoxCollider>();
            if (newCollider == null)
            {
                newCollider = _new.AddComponent<BoxCollider>();
                newCollider.size = oldCollider.size;
                newCollider.center = oldCollider.center;
            }

            if (_axis == Axis.X)
            {
                var size = newCollider.size;
                size.x += oldCollider.size.x;

                newCollider.size = size;

                _new.gameObject.transform.Translate(new Vector3((_foward ? 1 : -1) * (size.x / 2 - oldCollider.size.x / 2), 0, 0), Space.Self);
            }
            else if (_axis == Axis.Y)
            {
                var size = newCollider.size;
                size.y += oldCollider.size.y;

                newCollider.size = size;

                _new.gameObject.transform.Translate(new Vector3(0, (_foward ? 1 : -1) * (size.y / 2 - oldCollider.size.y / 2), 0), Space.Self);
            }
            else
            {
                var size = newCollider.size;
                size.z += oldCollider.size.z;

                newCollider.size = size;

                _new.gameObject.transform.Translate(new Vector3(0, 0, (_foward ? 1 : -1) * (size.z / 2 - oldCollider.size.z / 2)), Space.Self);
            }
        }

        private void FixAllNegativeCollider(GameObject _root)
        {
            HashSet<Transform> issueTransform = new HashSet<Transform>();

            {
                var colliders = _root.GetComponentsInChildren<Collider>(true);
                foreach (var c in colliders)
                {
                    var scale = c.transform.lossyScale;
                    if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                    {
                        issueTransform.Add(c.transform);
                    }
                }
            }

            //TO 
            foreach (var trans in issueTransform)
            {
                FixNegetiveCollider(trans.gameObject);
                Debug.Log($"Fixed {trans.gameObject.name}", trans.gameObject);
            }
        }

        private void FixNegetiveCollider(GameObject _root)
        {
            var colliders = _root.GetComponents<Collider>();
            var scale = _root.transform.lossyScale;
            if (scale.x < 0)
            {
                //var localScale = _root.transform.localScale;
                //localScale.x *= -1;
                //_root.transform.localScale = localScale;

                foreach (var c in colliders)
                {
                    FixNegetiveCollider(c, Axis.X);
                }
            }
            if (scale.y < 0)
            {
                //var localScale = _root.transform.localScale;
                //localScale.y *= -1;
                //_root.transform.localScale = localScale;

                foreach (var c in colliders)
                {
                    FixNegetiveCollider(c, Axis.Y);
                }
            }
            if (scale.z < 0)
            {
                //var localScale = _root.transform.localScale;
                //localScale.z *= -1;
                //_root.transform.localScale = localScale;

                foreach (var c in colliders)
                {
                    FixNegetiveCollider(c, Axis.Z);
                }
            }
        }

        private void FixNegetiveCollider(Collider c, Axis axis)
        {
            if (c is BoxCollider)
            {
                var size = ((BoxCollider)c).size;

                if (axis == Axis.X)
                    size.x *= -1;
                else if (axis == Axis.Y)
                    size.y *= -1;
                else
                    size.z *= -1;

                ((BoxCollider)c).size = size;
            }
            else if (c is CapsuleCollider)
            {
                var cap = c as CapsuleCollider;
                if (cap.direction == 0 && axis == Axis.X)
                {
                    cap.height *= -1;
                }
                else if (cap.direction == 1 && axis == Axis.Y)
                {
                    cap.height *= -1;
                }
                else if (cap.direction == 1 && axis == Axis.Z)
                {
                    cap.height *= -1;
                }
            }

            //if (c is BoxCollider)
            //{
            //    center = ((BoxCollider)c).center;
            //}
            //else if (c is SphereCollider)
            //{
            //    center = ((SphereCollider)c).center;
            //}
            //else if (c is CapsuleCollider)
            //{
            //    center = ((CapsuleCollider)c).center;
            //}
            //else
            //{
            //    return;
            //}

            //if (axis == Axis.X)
            //    center.x *= -1;
            //else if (axis == Axis.Y)
            //    center.y *= -1;
            //else
            //    center.z *= -1;

            //if (c is BoxCollider)
            //{
            //    ((BoxCollider)c).center = center;
            //}
            //else if (c is SphereCollider)
            //{
            //    ((SphereCollider)c).center = center;
            //}
            //else
            //{
            //    ((CapsuleCollider)c).center = center;
            //}
        }

        private void RenameAll(GameObject[] _objects, string _prefix)
        {
            var sorted = _objects.OrderBy((obj) => { return obj.transform.GetSiblingIndex(); });

            int index = 0;
            foreach (var s in sorted)
            {
                s.name = $"{_prefix} ({index})";
                index++;
            }
        }
    }
}
#endif