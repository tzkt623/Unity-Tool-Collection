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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZTools.CSVNS;

namespace ZTools.LocalizationNS
{
    public class Strings
    {
        private static Strings instance;

        public static void Load(string _folderPath, string _location)
        {
            if (instance == null)
            {
                instance = new Strings();
            }

            instance.LoadLanguage(_folderPath, _location);
        }
        public static string Get(string _nakeName)
        {
            return instance.GetText(_nakeName);
        }
        public static string Get(string _nakeName, params object[] _parameters)
        {
            try
            {
                return string.Format(Get(_nakeName), _parameters);
            }
            catch
            {
                return "FORMAT_ERROR_" + _nakeName;
            }
        }
        public static event System.Action onLanguageLoaded;
        public static string LoadedLanguage
        {
            get { return instance?.loaded ?? "cn"; }
        }

        private Dictionary<string, string> mapping;
        private string loaded;

        private void LoadLanguage(string _folderPath, string _location)
        {
            try
            {
                if (_location == loaded)
                    return;
                else
                    loaded = _location;

                var path = string.Concat(_folderPath, "_", _location);

                var tempMapping = new Dictionary<string, string>();
                var text = Resources.Load<TextAsset>(path);
                CSVReader.Read(text, (reader) =>
                {
                    string neakName = null;
                    string stringContent = null;

                    while (reader.Peek() != -1)
                    {
                        try
                        {
                            neakName = reader.ReadLine();
                            stringContent = reader.ReadLine();
                            tempMapping.Add(neakName, stringContent);
                        }
                        catch (Exception _e)
                        {
                            Debug.LogErrorFormat("加载{0}时出现了错误，错误内容: {1}", neakName, _e.Message);
                        }
                    }
                });

                mapping = tempMapping;

                if (onLanguageLoaded != null)
                    onLanguageLoaded();
            }
            catch
            {
                Debug.Log("Load language " + _location + " failed. Nothing changed");

                if (mapping == null)
                    mapping = new Dictionary<string, string>();
            }
        }


        private string GetText(string _nakeName)
        {
            if (mapping.ContainsKey(_nakeName))
                return mapping[_nakeName];
            else
                return string.Concat(" [ERROR_{", _nakeName, "}] ");
        }

        public static Dictionary<string, string> Editor_GetValues()
        {
            return instance.mapping;
        }
        public static void Editor_GetOriginalData(string _folderPath, string _location, out Dictionary<string, string> values)
        {
            try
            {
                var path = string.Concat(_folderPath, "_", _location);
                Debug.Log("Try to load language at path : " + path);
                var tempValues = new Dictionary<string, string>();
                var text = Resources.Load<TextAsset>(path);
                CSVReader.Read(text, (reader) =>
                {
                    string neakName = null;
                    string stringContent = null;

                    while (reader.Peek() != -1)
                    {
                        try
                        {
                            neakName = reader.ReadLine();
                            stringContent = reader.ReadLine();

                            tempValues.Add(neakName, stringContent);
                        }
                        catch (Exception _e)
                        {
                            Debug.LogErrorFormat("加载{0}时出现了错误，错误内容: {1}", neakName, _e.Message);
                        }
                    }
                });

                values = tempValues;
            }
            catch
            {
                Debug.Log("Load language " + _location + " failed. Nothing changed");

                values = new Dictionary<string, string>();
            }
        }

        public static void Editor_Create(string _nickName, string _text)
        {
            instance.mapping.Add(_nickName, _text);
        }
    }
}