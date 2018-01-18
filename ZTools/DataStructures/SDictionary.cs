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
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZTools.DataStructures
{
    [Serializable]
    public class SDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public List<TKey> Keys
        {
            get { return keys; }
        }

        public List<TValue> Values
        {
            get { return values; }
        }

        private Dictionary<TKey, TValue> actullyDictionary = new Dictionary<TKey, TValue>();

        public int Count
        {
            get
            {
                return ((IDictionary<TKey, TValue>)actullyDictionary).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<TKey, TValue>)actullyDictionary).IsReadOnly;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return ((IDictionary<TKey, TValue>)actullyDictionary).Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return ((IDictionary<TKey, TValue>)actullyDictionary).Values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return ((IDictionary<TKey, TValue>)actullyDictionary)[key];
            }

            set
            {
                ((IDictionary<TKey, TValue>)actullyDictionary)[key] = value;
            }
        }

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception("Make sure that both key and value types are serializable.");
            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)actullyDictionary).Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)actullyDictionary).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<TKey, TValue>)actullyDictionary).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)actullyDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)actullyDictionary).GetEnumerator();
        }
    }

    [Serializable]
    public class SDictionary_int_int : SDictionary<int, int> { }
    [Serializable]
    public class SDictionary_int_float : SDictionary<int, float> { }
    [Serializable]
    public class SDictionary_int_string : SDictionary<int, string> { }
    [Serializable]
    public class SDictionary_int_bool : SDictionary<int, bool> { }
    [Serializable]
    public class SDictionary_int_object : SDictionary<int, object> { }
    [Serializable]
    public class SDictionary_int_unityObject : SDictionary<int, UnityEngine.Object> { }

    [Serializable]
    public class SDictionary_float_int : SDictionary<float, int> { }
    [Serializable]
    public class SDictionary_float_float : SDictionary<float, float> { }
    [Serializable]
    public class SDictionary_float_string : SDictionary<float, string> { }
    [Serializable]
    public class SDictionary_float_bool : SDictionary<float, bool> { }
    [Serializable]
    public class SDictionary_float_object : SDictionary<float, object> { }
    [Serializable]
    public class SDictionary_float_unityObject : SDictionary<float, UnityEngine.Object> { }

    [Serializable]
    public class SDictionary_string_int : SDictionary<string, int> { }
    [Serializable]
    public class SDictionary_string_float : SDictionary<string, float> { }
    [Serializable]
    public class SDictionary_string_string : SDictionary<string, string> { }
    [Serializable]
    public class SDictionary_string_bool : SDictionary<string, bool> { }
    [Serializable]
    public class SDictionary_string_object : SDictionary<string, object> { }
    [Serializable]
    public class SDictionary_string_unityObject : SDictionary<string, UnityEngine.Object> { }
    [Serializable]
    public class SDictionary_string_mesh : SDictionary<string, Mesh> { }
    [Serializable]
    public class SDictionary_component_string : SDictionary<Component, string> { }
    [Serializable]
    public class SDictionary_string_gameObject : SDictionary<string, GameObject> { }


    [Serializable]
    public class SDictionary_object_int : SDictionary<object, int> { }
    [Serializable]
    public class SDictionary_object_float : SDictionary<object, float> { }
    [Serializable]
    public class SDictionary_object_string : SDictionary<object, string> { }
    [Serializable]
    public class SDictionary_object_bool : SDictionary<object, bool> { }
    [Serializable]
    public class SDictionary_object_object : SDictionary<object, object> { }
    [Serializable]
    public class SDictionary_object_unityObject : SDictionary<object, UnityEngine.Object> { }

    [Serializable]
    public class SDictionary_unityObject_int : SDictionary<UnityEngine.Object, int> { }
    [Serializable]
    public class SDictionary_unityObject_float : SDictionary<UnityEngine.Object, float> { }
    [Serializable]
    public class SDictionary_unityObject_string : SDictionary<UnityEngine.Object, string> { }
    [Serializable]
    public class SDictionary_unityObject_bool : SDictionary<UnityEngine.Object, bool> { }
    [Serializable]
    public class SDictionary_unityObject_object : SDictionary<UnityEngine.Object, object> { }
    [Serializable]
    public class SDictionary_unityObject_unityObject : SDictionary<UnityEngine.Object, UnityEngine.Object> { }
}