/*
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
 *
 * Copyright 2010 ZMind
 *
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
 * ZTOOLS是一个自由软件，您可以自由分发、修改其中的源代码或者重新发布它，
 * 新的任何修改后的重新发布版必须同样在遵守LGPL3或更后续的版本协议下发布.
 * 关于LGPL协议的细则请参考COPYING、COPYING.LESSER文件，
 * 您可以在ZTOOLS的相关目录中获得LGPL协议的副本，
 * 如果没有找到，请连接到 http://www.gnu.org/licenses/ 查看。
 *
 * - Author: ZMind
 * - License: GNU Lesser General Public License (LGPL)
 */

#ZTOOLS

ZTools是一个简易工具包，集成了大量的常见功能，可以很方便地进行调用。

---

#目录
* AlertArea - 地面警示区域
* CSVDataBase - CSV读取与数据格式转换
* DataStructures - 包含了工具包中所用的到数据格式
* Editor - 包含了Editor相关的扩展程序以及皮肤资源
* FastSceneLocator - 一个可以便捷进行场景导航的窗口
* MainThreading - 一个存在于主线程的负载均衡器
* Navigation - 操作5.6以后相关的寻路功能
* Pooling - 一个高性能的对象池
* ResourcesManager - 一个支持同步/异步加载的稳定的资源管理器
* Singleton - 一个可以轻松控制生命周期，并且便捷扩展的单例模板以及管理器
* SkillSystem - 一个技能系统的模板
* ViewCulling - 一个CullingGroup的扩展集合，可以用于进行高性能的视野/距离剔除。

---
#NAVIGATION
##NAVIGATION BAKER
###描述
负责用于Navmesh的烘焙，支持动态烘焙和区域烘焙。该类型是一个IDisposal类，需要手动释放资源。

###API
####public NavigationBaker(GameObject _owner)
创建一个空的NavigationBaker，该方法构造的Baker需要通过AddSource添加烘焙信息，并且需要通过UpdateSource手动更新，同时还需要通过RemoveSource才可以将洪培信息移除。

####public NavigationBaker(GameObject _owner, List<NavMeshBuildSource> _sources)
创建一个空的NavigationBaker，该方法构造的Baker所使用的的烘焙信息均来自所提供的的source list，因此尝试使用AddSource，UpdateSource或者RemoveSource将会抛出异常。所有的增删改直接在外部对传入的list对象进行即可。

####public Dispose()
将该Baker所使用的到的全部资源都销毁掉，该方法必须手动调用，否则会造成内存泄漏。

####public Dispose(int _agentTypeID)
从Baker中移除指定类型的Agent所对应的寻路信息。

####public AddSource(INavigationSourceProvider _source)
将指定的Source对象添加到该Baker中。前提是该Baker是采用非传递列表的方法构建出来的。

####public UpdateSource(INavigationSourceProvider _source)
更新指定Source的信息。如果该Source并没有被加入到Baker中，会自动进行添加，而不需要手动调用AddSource。

####public RemoveSource(INavigationSourceProvider _source)
将指定的Source从列表中移除。

####public UpdateNavmesh(int _agentType, bool _async)
更新指定Agent类型所对应的Navmesh，通过_async指定是否采取异步或者同步方式。如果采取异步计算，在上一次计算未完成之前，将忽略此次请求。如果采取同步计算，在上一次异步计算未完成之前，将中断异步计算，立刻进行同步计算。

####public UpdateNavmesh(int _agentType, Vector3 _position, Vector3 _size, bool _async)
更新指定Agent类型所对应的在范围内的Navmesh，通过_async指定是否采取异步或者同步方式。如果采取异步计算，在上一次计算未完成之前，将忽略此次请求。如果采取同步计算，在上一次异步计算未完成之前，将中断异步计算，立刻进行同步计算。采用的坐标系为世界坐标系，不存在旋转。

##I NAVIGATION SOURCE PROVIDER
