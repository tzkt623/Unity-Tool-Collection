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

namespace ZTools
{
    public class Formula
    {
        protected List<float> cachedList;
        protected const int leftBrackets = -(int)'(';
        protected const int rightBrackets = -(int)')';
        protected const int muiltple = -(int)'*';
        protected const int division = -(int)'/';
        protected const int plus = -(int)'+';
        protected const int minus = -(int)'-';
        protected const int unknown1 = -(int)'$';
        protected int cache = int.MinValue;
        protected float cacheFloat = float.NegativeInfinity;
        protected int cacheResult = int.MinValue;
        protected float cacheFloatResult = float.NegativeInfinity;

        public Formula(string _formula)
        {
            try
            {
                //http://baike.baidu.com/view/2582.htm
                //逆波兰算法实现

                Stack<float> s1 = new Stack<float>();
                s1.Push('#');

                Stack<float> s2 = new Stack<float>();

                //("( 400 + 200 ) + $");
                string[] param = _formula.Split(' ');
                foreach (string s in param)
                {
                    switch (s[0])
                    {
                        case '$':
                            s2.Push(Formula.unknown1);
                            break;
                        case '+':
                            while (s1.Peek() != Formula.leftBrackets && s1.Peek() != Formula.rightBrackets && s1.Peek() != '#')
                            {
                                s2.Push(s1.Pop());
                            }
                            s1.Push(Formula.plus);
                            break;
                        case '-':
                            while (s1.Peek() != Formula.leftBrackets && s1.Peek() != Formula.rightBrackets && s1.Peek() != '#')
                            {
                                s2.Push(s1.Pop());
                            }
                            s1.Push(Formula.minus);
                            break;
                        case '*':
                            s1.Push(Formula.muiltple);
                            break;
                        case '/':
                            s1.Push(Formula.division);
                            break;
                        case '(':
                            s1.Push(Formula.leftBrackets);
                            break;
                        case ')':
                            while (s1.Peek() != Formula.leftBrackets)
                            {
                                s2.Push(s1.Pop());
                            }
                            s1.Pop();
                            break;
                        default:
                            s2.Push(float.Parse(s));
                            break;
                    }
                }


                int count = s2.Count;
                for (int i = 0; i < count; i++)
                {
                    s1.Push(s2.Pop());
                }

                count = s1.Count;

                this.cachedList = new List<float>(count - 1);


                //'#' is not included
                for (int i = 0; i < count - 1; i++)
                {
                    this.cachedList.Add(s1.Pop());
                }
            }
            catch (System.Exception _e)
            {
                Debug.LogError("无法正确生成公式，公式字符串为 " + _formula);
                Debug.LogException(_e);
            }
        }

        public float GetValue(float _x)
        {
            if (_x == cacheFloat)
                return cacheFloatResult;

            Stack<float> formula = new Stack<float>();

            int temp = 0;
            for (int i = 0; i < this.cachedList.Count; ++i)
            {
                temp = (int)this.cachedList[i];

                switch (temp)
                {
                    case Formula.plus:
                        formula.Push(formula.Pop() + formula.Pop());
                        break;
                    case Formula.minus:
                        {
                            float b = formula.Pop();
                            float a = formula.Pop();
                            formula.Push(a - b);
                        }
                        break;
                    case Formula.muiltple:
                        formula.Push(formula.Pop() * formula.Pop());
                        break;
                    case Formula.division:
                        {
                            float b = formula.Pop();
                            float a = formula.Pop();
                            //                                if (b == 0)
                            //                                    return float.NaN;
                            formula.Push(a / b);
                        }
                        break;
                    case Formula.unknown1:
                        formula.Push(_x);
                        break;
                    default:
                        formula.Push(this.cachedList[i]);
                        break;
                }
            }

            cacheFloat = _x;
            cacheFloatResult = formula.Peek();
            return cacheFloatResult;
        }
    }

    public class FormulaMultiUnknowns : Formula
    {
        public FormulaMultiUnknowns(string _formula) : base(_formula) { }

        public float GetValue(params float[] _unKnowns)
        {
            int curUsedUnknow = 0;
            Stack<float> formula = new Stack<float>();

            try
            {
                int temp = 0;
                for (int i = 0; i < this.cachedList.Count; ++i)
                {
                    temp = (int)this.cachedList[i];
                    switch (temp)
                    {
                        case Formula.plus:
                            formula.Push(formula.Pop() + formula.Pop());
                            break;
                        case Formula.minus:
                            {
                                float b = formula.Pop();
                                float a = formula.Pop();
                                formula.Push(a - b);
                            }
                            break;
                        case Formula.muiltple:
                            formula.Push(formula.Pop() * formula.Pop());
                            break;
                        case Formula.division:
                            {
                                float b = formula.Pop();
                                float a = formula.Pop();
                                formula.Push(a / b);
                            }
                            break;
                        case Formula.unknown1:
                            formula.Push(_unKnowns[curUsedUnknow]);
                            ++curUsedUnknow;
                            break;
                        default:
                            formula.Push(this.cachedList[i]);
                            break;
                    }
                }

                return formula.Peek();
            }
            catch
            {
                Debug.LogError("计算公式出错，应该是赋值的参数与公式没有一一对应");
                return float.NaN;
            }
        }
    }

    public abstract class CustomUnknownSourceFormula : FormulaMultiUnknowns
    {
        public CustomUnknownSourceFormula(string _formula) : base(_formula)
        {
        }

        public abstract void SyncUnkowns(object _startSearch);
        public abstract float GetValue();
    }
}
