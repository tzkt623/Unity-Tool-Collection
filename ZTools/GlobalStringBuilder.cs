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
using System;
using System.Text;

public static class GlobalStringBuilder
{
    public static void Begin()
    {
        ++currentIndex;
        while (currentIndex >= builders.Length)
        {
            Array.Resize(ref builders, builders.Length + 1);
            builders[builders.Length - 1] = new StringBuilder();
        }
    }

    public static void End()
    {
        --currentIndex;
    }

    private static StringBuilder[] builders = new StringBuilder[1] { new StringBuilder() };
    private static int currentIndex = -1;
    private static StringBuilder sb { get { return builders[currentIndex]; } }

    public static int Length { get { return sb.Length; } }

    public static void Clear(bool _begin = false) { if (_begin) Begin(); sb.Length = 0; }

    public static void RemoveChar(int _count) { sb.Length -= _count; }

    public static void Append(char _char) { sb.Append(_char); }

    public static void Append(string _string) { sb.Append(_string); }

    public static void AppendLine(string _string) { sb.AppendLine(_string); }

    public static void NewLine() { sb.AppendLine(); }

    public static new string ToString() { return ToString(false); }

    public static string ToString(bool _end = false) { var result = sb.ToString(); if (_end) End(); return result; }

    public static void BeginColor(string _colorHex)
    {
        sb.Append("<color=#");
        sb.Append(_colorHex);
        sb.Append(">");
    }

    public static void EndColor() { sb.Append("</color>"); }
}