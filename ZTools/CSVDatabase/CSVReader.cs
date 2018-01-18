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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

namespace ZTools.CSVNS
{
    public static class CSVReader
    {
        private class Convertion : IEquatable<Convertion>, IComparable<Convertion>
        {

            public static bool IsFieldCSVReadable(FieldInfo _fieldInfo)
            {
                return _fieldInfo.GetCustomAttributes(typeof(CSVTitleAttribute), true).Length > 0;
            }

            public int orderIndex;
            public FieldInfo fieldInfo;
            private Func<string, object> method;
            ICustomConverter converter;

            private Convertion()
            {
                //不开放自定义构造
            }

            public Convertion(FieldInfo _fieldInfo)
            {
                fieldInfo = _fieldInfo;

                var attr = _fieldInfo.GetCustomAttributes(typeof(CSVTitleAttribute), true)[0] as CSVTitleAttribute;
                orderIndex = attr.order;
                converter = attr.Convert;
                if (converter == null)
                    method = SelectConvertion(_fieldInfo);
            }

            int IComparable<Convertion>.CompareTo(Convertion other)
            {
                return orderIndex.CompareTo(other.orderIndex);
            }

            bool IEquatable<Convertion>.Equals(Convertion other)
            {
                return orderIndex.Equals(other.orderIndex);
            }

            public void ConvertAndSet(object _object, string _line)
            {
                object value;

                if (method != null)
                    value = method(_line);
                else if (converter != null)
                    value = converter.Convert(_line);
                else
                    throw new Exception("转换失败");

                fieldInfo.SetValue(_object, value);
            }

            private static Func<string, object> SelectConvertion(FieldInfo _info)
            {
                try
                {
                    return defaultConvertions[_info.FieldType];
                }
                catch
                {
                    throw new Exception(string.Format("找不到{0}对应的转换方法！如果要支持，添加新的Convert方法即可", _info.FieldType.Name));
                }
            }

            private static readonly Dictionary<Type, Func<string, object>> defaultConvertions = new Dictionary<Type, Func<string, object>>()
            {
                [typeof(bool)] = ConvertToBool,
                [typeof(byte)] = ConvertToByte,
                [typeof(sbyte)] = ConvertToSByte,
                [typeof(short)] = ConvertToShort,
                [typeof(ushort)] = ConvertToUShort,
                [typeof(uint)] = ConvertToUInt,
                [typeof(int)] = ConvertToInt,
                [typeof(float)] = ConvertToFloat,
                [typeof(long)] = ConvertToLong,
                [typeof(string)] = ConvertToString,
            };

            private static object ConvertToBool(string _text)
            {
                return bool.Parse(_text);
            }

            private static object ConvertToByte(string _text)
            {
                return byte.Parse(_text);
            }

            private static object ConvertToSByte(string _text)
            {
                return sbyte.Parse(_text);
            }

            private static object ConvertToShort(string _text)
            {
                return short.Parse(_text);
            }

            private static object ConvertToUShort(string _text)
            {
                return ushort.Parse(_text);
            }

            private static object ConvertToInt(string _text)
            {
                return int.Parse(_text);
            }

            private static object ConvertToUInt(string _text)
            {
                return uint.Parse(_text);
            }

            private static object ConvertToFloat(string _text)
            {
                return float.Parse(_text);
            }

            private static object ConvertToLong(string _text)
            {
                return long.Parse(_text);
            }

            private static object ConvertToString(string _text)
            {
                return _text;
            }
        }

        public static List<T> Read<T>(string _path, int _initCapacity = 256) where T : class, new()
        {
            List<T> result = new List<T>(_initCapacity);

            List<Convertion> convertion = new List<Convertion>();
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (Convertion.IsFieldCSVReadable(fields[i]))
                {
                    convertion.Add(new Convertion(fields[i]));
                    convertion.Sort();
                }
            }

            using (var reader = new StreamReader(_path, System.Text.Encoding.Unicode))
            {
                while (reader.Peek() != -1)
                {
                    var loadedObject = new T();

                    for (int i = 0; i < convertion.Count; ++i)
                    {
                        convertion[i].ConvertAndSet(loadedObject, reader.ReadLine());
                    }

                    result.Add(loadedObject);
                }

                return result;
            }
        }

        public static List<T> Read<T>(TextAsset _textAsset, int _initCapacity = 256) where T : class, new()
        {
            List<T> result = new List<T>(_initCapacity);

            List<Convertion> convertion = new List<Convertion>();
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (Convertion.IsFieldCSVReadable(fields[i]))
                {
                    convertion.Add(new Convertion(fields[i]));
                    convertion.Sort();
                }
            }


            using (var stream = new MemoryStream(_textAsset.bytes))
            {
                using (var reader = new StreamReader(stream))
                {
                    string temp = null;
                    while (reader.Peek() != -1)
                    {
                        var loadedObject = new T();

                        for (int i = 0; i < convertion.Count; ++i)
                        {
                            try
                            {
                                temp = reader.ReadLine();
                                convertion[i].ConvertAndSet(loadedObject, temp);
                            }
                            catch (FormatException _e)
                            {
                                Debug.LogErrorFormat("转换结构 [{0}] 的过程中，转换第{1}个元素的时候出现了格式错误。期望的结构为{2}, 传入的字符串为{3}",
                                    typeof(T).ToString(), i + 1, convertion[i].fieldInfo.FieldType.ToString(), temp);
                                GlobalStringBuilder.Clear(true);
                                GlobalStringBuilder.Append("已经转换成功的内容部分如下: ");
                                for (int j = 0; j < i; ++j)
                                {
                                    GlobalStringBuilder.Append((convertion[j].fieldInfo.GetValue(loadedObject).ToString()));
                                    GlobalStringBuilder.Append(" | ");
                                }
                                Debug.LogError(GlobalStringBuilder.ToString(true));

                                throw _e;
                            }
                            catch
                            {
                                throw;
                            }
                        }

                        result.Add(loadedObject);
                    }

                    return result;
                }
            }
        }

        public static void Read(TextAsset _textAsset, Action<StreamReader> _read)
        {
            using (var stream = new MemoryStream(_textAsset.bytes))
            {
                using (var reader = new StreamReader(stream))
                {
                    _read(reader);
                }
            }
        }

        /// <summary>
        /// 读取CSV文件，并存放到指定的字典中。
        /// 字典不需要实例化，在执行过程中会被自动实例化
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="_insideResourcesFolder"></param>
        /// <param name="_path"></param>
        /// <param name="_storeDictionary"></param>
        /// <param name="_getID"></param>
        public static void Read<TKey, T>(bool _insideResourcesFolder, string _path, ref Dictionary<TKey, T> _storeDictionary, Func<T, TKey> _getID) where T : class, new()
        {
            if (_insideResourcesFolder)
            {
                var text = Resources.Load<TextAsset>(_path);
                var array = CSVReader.Read<T>(text);
                _storeDictionary = new Dictionary<TKey, T>(array.Count);

                TKey key = default(TKey);

                try
                {
                    foreach (var data in array)
                    {
                        key = _getID(data);
                        _storeDictionary.Add(key, data);
                    }
                }
                catch (ArgumentException)
                {
                    Debug.LogError($"处理文件 {_path} 时候发现重复ID {key.ToString()}");
                }
            }
            else
            {
                var array = CSVReader.Read<T>(_path);
                _storeDictionary = new Dictionary<TKey, T>(array.Count);
                foreach (var data in array)
                {
                    _storeDictionary.Add(_getID(data), data);
                }
            }
        }

        /// <summary>
        /// 读取CSV文件，以组的形式，存放在指定的字典中
        /// 字典不需要实例化，在执行过程中会被自动实例化
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="_insideResourcesFolder"></param>
        /// <param name="_path"></param>
        /// <param name="_errorKey"></param>
        /// <param name="_storeDictionary"></param>
        /// <param name="_getID"></param>
        public static void Read<Tkey, T>(bool _insideResourcesFolder, string _path, Tkey _errorKey,
            ref Dictionary<Tkey, T[]> _storeDictionary, Func<T, Tkey> _getID) where T : class, new()
            where Tkey : IComparable<Tkey>
        {
//#if UNITY_EDITOR
//            if (SpeedUpWindow.Instance != null)
//            {
//                if (SpeedUpWindow.Instance.storedData.ContainsKey(_path) && SpeedUpWindow.Instance.storedData[_path] is Dictionary<Tkey, T[]>)
//                {
//                    _storeDictionary = (Dictionary<Tkey, T[]>)SpeedUpWindow.Instance.storedData[_path];
//                    return;
//                }
//            }
//#endif

            var tempDictionary = new Dictionary<Tkey, T[]>();

            //初始化Convertion列表
            List<Convertion> convertion = new List<Convertion>();
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (Convertion.IsFieldCSVReadable(fields[i]))
                {
                    convertion.Add(new Convertion(fields[i]));
                    convertion.Sort();
                }
            }

            if (_insideResourcesFolder)
            {

                var text = Resources.Load<TextAsset>(_path);
                CSVReader.Read(text, (reader) =>
                {
                    List<T> tempList = new List<T>();
                    Tkey currentID = _errorKey;

                    try
                    {
                        while (reader.Peek() != -1)
                        {
                            var loadedObject = new T();
                            for (int i = 0; i < convertion.Count; ++i)
                            {
                                convertion[i].ConvertAndSet(loadedObject, reader.ReadLine());
                            }

                            if (currentID.CompareTo(_getID(loadedObject)) != 0)
                            {
                                tempDictionary.Add(currentID, tempList.ToArray());
                                tempList.Clear();
                                currentID = _getID(loadedObject);
                            }

                            tempList.Add(loadedObject);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogError($"在处理文件 {_path} 时发现了重复KEY {currentID}");
                    }

                    tempDictionary.Add(currentID, tempList.ToArray());
                    tempList.Clear();
                });
            }

            _storeDictionary = tempDictionary;

//#if UNITY_EDITOR
//            if (SpeedUpWindow.Instance != null)
//            {
//                if (SpeedUpWindow.Instance.storedData.ContainsKey(_path))
//                {
//                    SpeedUpWindow.Instance.storedData[_path] = _storeDictionary;
//                }
//                else
//                {
//                    SpeedUpWindow.Instance.storedData.Add(_path, _storeDictionary);
//                }
//            }
//#endif
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="_path"></param>
        /// <param name="_storeDictionary"></param>
        public static void Write<Tkey, T>(string _path, Dictionary<Tkey, T> _storeDictionary, bool _csv = false)
        {
            //初始化Convertion列表
            List<Convertion> convertion = new List<Convertion>();
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (Convertion.IsFieldCSVReadable(fields[i]))
                {
                    convertion.Add(new Convertion(fields[i]));
                    convertion.Sort();
                }
            }

            if (_csv)
            {
                using (StreamWriter sw = new StreamWriter(_path, false, System.Text.Encoding.UTF8))
                {
                    var totalCount = _storeDictionary.Count;
                    var index = 0;

                    foreach (var value in _storeDictionary.Values)
                    {
                        for (int i = 0, count = convertion.Count; i < count; ++i)
                        {
                            var v = convertion[i].fieldInfo.GetValue(value);
                            if (v != null)
                                sw.Write(v.ToString());
                            sw.Write(',');
                        }

                        ++index;
                        if (index < totalCount)
                        {
                            sw.WriteLine();
                        }
                    }

                    sw.Flush();
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(_path, false, System.Text.Encoding.UTF8))
                {
                    var totalCount = _storeDictionary.Count;
                    var index = 0;

                    foreach (var value in _storeDictionary.Values)
                    {
                        ++index;
                        if (index < totalCount)
                        {
                            for (int i = 0, count = convertion.Count; i < count; ++i)
                            {
                                sw.WriteLine(convertion[i].fieldInfo.GetValue(value).ToString());
                            }
                        }
                        else
                        {
                            for (int i = 0, count = convertion.Count - 1; i < count; ++i)
                            {
                                sw.WriteLine(convertion[i].fieldInfo.GetValue(value).ToString());
                            }

                            sw.Write(convertion[convertion.Count - 1].fieldInfo.GetValue(value).ToString());
                        }
                    }

                    sw.Flush();
                }
            }
        }

        //TODO:
    }
}
