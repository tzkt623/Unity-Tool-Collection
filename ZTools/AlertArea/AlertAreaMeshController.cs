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

namespace ZTools.AlertArea
{
    public static class AlertAreaMeshController
    {
        public static AlertAreaMeshPack CreateLineAlertMesh(float length, float width, float borderWidth)
        {
            var mesh = new Mesh();
            var vertices = new Vector3[8];
            var uv = new Vector2[8];
            var triangles = new int[18];

            vertices[0] = new Vector3(-width, 0, 0);
            vertices[1] = new Vector3(-width + borderWidth, 0, 0);
            vertices[2] = new Vector3(width - borderWidth, 0, 0);
            vertices[3] = new Vector3(width, 0, 0);
            vertices[4] = new Vector3(-width, 0, length);
            vertices[5] = new Vector3(-width + borderWidth, 0, length);
            vertices[6] = new Vector3(width - borderWidth, 0, length);
            vertices[7] = new Vector3(width, 0, length);

            var vScale = length / borderWidth;
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(0.25f, 0);
            uv[2] = new Vector2(0.75f, 0);
            uv[3] = new Vector2(1, 0);
            uv[4] = new Vector2(0, vScale);
            uv[5] = new Vector2(0.25f, vScale);
            uv[6] = new Vector2(0.75f, vScale);
            uv[7] = new Vector2(1, vScale);

            var uv2 = new Vector2[8];
            uv2[0] = new Vector2(0, 0);
            uv2[1] = new Vector2(0.25f, 0);
            uv2[2] = new Vector2(0.75f, 0);
            uv2[3] = new Vector2(1, 0);
            uv2[4] = new Vector2(0, 1);
            uv2[5] = new Vector2(0.25f, 1);
            uv2[6] = new Vector2(0.75f, 1);
            uv2[7] = new Vector2(1, 1);

            {
                triangles[0] = 0;
                triangles[1] = 4;
                triangles[2] = 1;

                triangles[3] = 1;
                triangles[4] = 4;
                triangles[5] = 5;

                triangles[6] = 1;
                triangles[7] = 5;
                triangles[8] = 2;

                triangles[9] = 2;
                triangles[10] = 5;
                triangles[11] = 6;

                triangles[12] = 2;
                triangles[13] = 6;
                triangles[14] = 3;

                triangles[15] = 3;
                triangles[16] = 6;
                triangles[17] = 7;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.uv2 = uv2;
            mesh.triangles = triangles;
            return new AlertAreaMeshPack { mesh = mesh, vertices = vertices, uv = uv };
        }

        public static AlertAreaMeshPack CreateSectorAlertMesh(float radium, float angle, float borderWidth, int sectorCount)
        {
            var mesh = new Mesh();
            sectorCount = Mathf.Max(sectorCount, 6);
            var vertices = new Vector3[sectorCount + 8];
            var uv = new Vector2[vertices.Length];
            var triangles = new List<int>(3 * (sectorCount + 5));
            var length = CalculateVScale(radium, borderWidth);

            var verticeCount = vertices.Length;
            var finalIndex = verticeCount - 1;

            //VERTICES
            vertices[0] = Vector3.zero;
            vertices[finalIndex] = new Vector3(0, 0, radium);
            uv[finalIndex] = new Vector2(0.5f, length);

            angle = Mathf.Min(angle, 180);
            var a = angle * 2f / sectorCount;
            var rotInverse = Quaternion.AngleAxis(a, Vector3.up);
            var rotReverse = Quaternion.AngleAxis(-a, Vector3.up);
            //var u = 0.5f / sectorCount;

            vertices[finalIndex - 1] = rotInverse * new Vector3(0, 0, radium);
            uv[finalIndex - 1] = new Vector2(0.5f, length);
            triangles.Add(finalIndex);
            triangles.Add(finalIndex - 1);
            triangles.Add(0);

            for (int i = vertices.Length - 4; i >= 8; i -= 2) //右边
            {
                vertices[i] = rotInverse * vertices[i + 2];
                uv[i] = new Vector2(0.5f, length);

                triangles.Add(i + 2);
                triangles.Add(i);
                triangles.Add(0);
            }

            for (int i = vertices.Length - 3; i >= 7; i -= 2) //左边
            {
                vertices[i] = rotReverse * vertices[i + 2];
                uv[i] = new Vector2(0.5f, length);

                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(0);
            }

            vertices[1] = -vertices[8].normalized * borderWidth;
            vertices[2] = -vertices[7].normalized * borderWidth;
            vertices[3] = vertices[7] + vertices[1];
            vertices[4] = vertices[8] + vertices[2];

            vertices[6] = vertices[8];
            vertices[5] = vertices[7];

            uv[0] = new Vector2(0.25f, 0f);
            uv[1] = new Vector2(0f, 0f);
            uv[2] = new Vector2(0f, 0f);
            uv[3] = new Vector2(0f, length);
            uv[4] = new Vector2(0f, length);
            uv[5] = new Vector2(0.25f, length);
            uv[6] = new Vector2(0.25f, length);

            var uv2 = new Vector2[uv.Length];
            uv2[0] = uv[0];
            uv2[1] = uv[1];
            uv2[2] = uv[2];
            for (int i = 3, count = uv.Length; i < count; ++i)
            {
                uv2[i] = uv[i];
                uv2[i].y = 1f;
            }

            triangles.AddRange(new int[] { 0, 1, 3, 0, 3, 5, 0, 5, 7, 0, 8, 6, 0, 6, 4, 0, 4, 2 });

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.uv2 = uv2;
            mesh.triangles = triangles.ToArray();
            return new SectorAlertAreaMeshPack { mesh = mesh, vertices = vertices, uv = uv, sectorCount = sectorCount };
        }

        public static AlertAreaMeshPack CreateRadioAlertMesh(float _radium, float _borderWidth, int _sectorCount)
        {
            if (_sectorCount % 2 != 0)
                _sectorCount += 1;

            var mesh = new Mesh();

            var vertices = new Vector3[((_sectorCount + 1) * 3) + 1];
            var uv = new Vector2[vertices.Length];
            var uv2 = new Vector2[vertices.Length];
            var triangles = new int[_sectorCount * 9];

            var a = 360f / _sectorCount;
            var rotInverse = Quaternion.AngleAxis(a, Vector3.up);
            var actullyRadium = Mathf.Max(_borderWidth, _radium - _borderWidth);
            var v1Max = CalculateVScale(Mathf.PI * 2 * _radium, _borderWidth); //外圈周长V
            var v2Max = CalculateVScale(Mathf.PI * 2 * actullyRadium, _borderWidth);//内圈周长V;
            var vForEachSector = v1Max / _sectorCount;
            var v2ForEachSector = v2Max / _sectorCount;

            vertices[0] = Vector3.zero;
            vertices[1] = vertices[2] = new Vector3(0, 0, actullyRadium);
            vertices[3] = new Vector3(0, 0, _radium);
            for (int i = 0; i < _sectorCount; ++i)
            {
                var index = i * 3 + 4;
                vertices[index] = vertices[index + 1] = rotInverse * vertices[index - 3];
                vertices[index + 2] = rotInverse * vertices[index - 1];
            }

            uv[0] = new Vector2(0.5f, 0);
            uv[1] = new Vector2(0.6f, 0f);
            uv[2] = new Vector2(0.75f, 0f);
            uv[3] = new Vector2(1f, 0f);
            for (int i = 0; i < _sectorCount; ++i)
            {
                var index = i * 3 + 4;
                var currentOutterV = vForEachSector * (i + 1);
                var currentInnerV = v2ForEachSector * (i + 1);

                uv[index] = new Vector2(0.6f, currentInnerV);
                uv[index + 1] = new Vector2(0.75f, currentInnerV);
                uv[index + 2] = new Vector2(1f, currentOutterV);
            }

            for (int i = 0; i < _sectorCount; ++i)
            {
                var triangleIndex = i * 9;
                var verticeIndex = i * 3;

                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = verticeIndex + 4;
                triangles[triangleIndex + 2] = verticeIndex + 1;
                triangles[triangleIndex + 3] = verticeIndex + 5;
                triangles[triangleIndex + 4] = verticeIndex + 6;
                triangles[triangleIndex + 5] = verticeIndex + 2;
                triangles[triangleIndex + 6] = verticeIndex + 2;
                triangles[triangleIndex + 7] = verticeIndex + 6;
                triangles[triangleIndex + 8] = verticeIndex + 3;
            }

            //var finalTriangleIndex = (_sectorCount - 1) * 9;
            //triangles[finalTriangleIndex + 1] = 1;
            //triangles[finalTriangleIndex + 3] = 2;
            //triangles[finalTriangleIndex + 4] = 3;
            //triangles[finalTriangleIndex + 7] = 3;

            uv2[0] = Vector2.one;
            for (int i = 1; i < uv2.Length; ++i)
            {
                uv2[i] = Vector2.zero;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.uv2 = uv2;
            return new SectorAlertAreaMeshPack { mesh = mesh, vertices = vertices, uv = uv, sectorCount = _sectorCount };
        }

        public static void UpdateLineAlertMesh(this AlertAreaMeshPack _pack, float length, float width, float borderWidth)
        {
            var vertices = _pack.vertices;
            var uv = _pack.uv;

            vertices[0] = new Vector3(-width, 0, 0);
            vertices[1] = new Vector3(-width + borderWidth, 0, 0);
            vertices[2] = new Vector3(width - borderWidth, 0, 0);
            vertices[3] = new Vector3(width, 0, 0);
            vertices[4] = new Vector3(-width, 0, length);
            vertices[5] = new Vector3(-width + borderWidth, 0, length);
            vertices[6] = new Vector3(width - borderWidth, 0, length);
            vertices[7] = new Vector3(width, 0, length);

            var vScale = CalculateVScale(length, borderWidth);
            uv[4] = new Vector2(0, vScale);
            uv[5] = new Vector2(0.25f, vScale);
            uv[6] = new Vector2(0.75f, vScale);
            uv[7] = new Vector2(1, vScale);

            _pack.mesh.vertices = _pack.vertices;
            _pack.mesh.uv = _pack.uv;
            _pack.mesh.RecalculateBounds();
        }

        public static void UpdateSectorAlertMesh(this AlertAreaMeshPack _pack, float radium, float angle, float borderWidth)
        {
            var vertices = _pack.vertices;
            var uv = _pack.uv;
            var verticeCount = vertices.Length;
            var finalIndex = verticeCount - 1;

            //VERTICES
            vertices[0] = Vector3.zero;
            vertices[finalIndex] = new Vector3(0, 0, radium);

            angle = Mathf.Min(angle, 180);
            var a = angle * 2f / ((SectorAlertAreaMeshPack)_pack).sectorCount;
            var rotInverse = Quaternion.AngleAxis(a, Vector3.up);
            var rotReverse = Quaternion.AngleAxis(-a, Vector3.up);
            vertices[finalIndex - 1] = rotInverse * new Vector3(0, 0, radium);

            for (int i = vertices.Length - 4; i >= 8; i -= 2) //右边
            {
                vertices[i] = rotInverse * vertices[i + 2];
            }

            for (int i = vertices.Length - 3; i >= 7; i -= 2) //左边
            {
                vertices[i] = rotReverse * vertices[i + 2];
            }

            vertices[1] = -(vertices[9] - vertices[7]).normalized * borderWidth;
            vertices[2] = -(vertices[10] - vertices[8]).normalized * borderWidth;
            vertices[3] = vertices[7] + vertices[1];
            vertices[4] = vertices[8] + vertices[2];
            vertices[6] = vertices[8];
            vertices[5] = vertices[7];

            var length = CalculateVScale(radium, borderWidth);
            for (int i = 3; i < verticeCount; ++i)
            {
                uv[i].y = length;
            }

            _pack.mesh.vertices = vertices;
            _pack.mesh.uv = uv;
            _pack.mesh.RecalculateBounds();
        }

        public static void UpdateRadioAlertMesh(this AlertAreaMeshPack _pack, float _radium, float _borderWidth)
        {
            var vertices = _pack.vertices;
            var uv = _pack.uv;
            var _sectorCount = ((SectorAlertAreaMeshPack)_pack).sectorCount;

            var totalRadium = _radium + _borderWidth;
            //var circleLength = Mathf.PI * 2 * _radium;
            //var vMax = circleLength / _borderWidth; //V坐标的最大值
            //var vForEachSector = vMax / _sectorCount;

            var a = 360f / _sectorCount;
            var rotInverse = Quaternion.AngleAxis(a, Vector3.up);

            vertices[0] = Vector3.zero;
            vertices[1] = vertices[2] = new Vector3(0, 0, _radium);
            vertices[3] = new Vector3(0, 0, totalRadium);
            for (int i = 0; i < _sectorCount; ++i)
            {
                var index = i * 3 + 4;
                vertices[index] = vertices[index + 1] = rotInverse * vertices[index - 3];
                vertices[index + 2] = rotInverse * vertices[index - 1];
            }



            //var prevRadium = vertices[1].z;
            //prevRadium = Mathf.Max(prevRadium, 0.1f);
            //var prevTotalRadium = vertices[3].z;
            //prevTotalRadium = Mathf.Max(prevTotalRadium, 0.1f);
            //var multiRadium = _radium / prevRadium;
            //var multiTotalRadium = totalRadium / prevTotalRadium;

            //vertices[1] = vertices[2] = new Vector3(0, 0, _radium);
            //vertices[3] = new Vector3(0, 0, totalRadium);
            //for (int i = 0; i < _sectorCount; ++i)
            //{
            //    var index = i * 3 + 4;
            //    vertices[index] *= multiRadium;
            //    vertices[index + 1] *= multiRadium;
            //    vertices[index + 2] *= multiTotalRadium;
            //}

            //for (int i = 0; i < _sectorCount; ++i)
            //{
            //    var index = i * 3 + 4;
            //    var currentV = vForEachSector * (i + 1);
            //    uv[index].y = currentV;
            //    uv[index + 1].y = currentV;
            //    uv[index + 2].y = currentV;
            //}

            _pack.mesh.vertices = vertices;
            _pack.mesh.uv = uv;
            _pack.mesh.RecalculateBounds();
        }

        private static float CalculateVScale(float _linerlength, float _borderWidth)
        {
            return _linerlength / _borderWidth / 5;
        }
    }
}