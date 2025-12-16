---
layout: post
title: Unity射线检测GC问题深度解析：从根源到优化
date: 2025-11-28
header-img: img/post-bg-universe.jpg
categories: [Unity性能优化]
tags: [Unity, 射线检测, GC优化]
---

# Unity射线检测GC问题深度解析：从根源到优化

## 引言

射线检测是Unity游戏开发中最常用的技术之一，广泛应用于点击检测、视线判断、射击系统等场景。然而，不正确的射线检测用法可能导致严重的GC（垃圾回收）问题，进而影响游戏性能。本文将深入分析射线检测GC问题的根源，并提供完整的优化方案。

## 一、射线检测为什么会产生GC？

**核心结论：射线检测本身不会产生GC，产生GC的是用于存储检测结果的数组。**

在Unity中，以下射线检测API会产生GC开销：

```csharp
// 这些API都会在每次调用时创建新的RaycastHit数组
RaycastHit[] hits1 = Physics.RaycastAll(origin, direction, distance);
RaycastHit[] hits2 = Physics.SphereCastAll(origin, radius, direction, distance);
RaycastHit[] hits3 = Physics.BoxCastAll(origin, halfExtents, direction, orientation, distance);
RaycastHit[] hits4 = Physics.CapsuleCastAll(point1, point2, radius, direction, distance);
```

### GC产生的原理

1. **数组是引用类型**：RaycastHit数组在**堆内存**中分配
2. **每次调用创建新数组**：无论检测到0个还是多个物体，都会创建新数组
3. **数组成为垃圾**：当数组不再被使用时，等待GC回收
4. **GC压力累积**：频繁调用导致大量垃圾，引发频繁GC

## 二、无GC的射线检测API

Unity提供了**NonAlloc版本**的射线检测API，这些API使用预分配的数组，完全避免GC开销：

```csharp
// 预分配数组（只需创建一次）
private RaycastHit[] hitResults = new RaycastHit[50];

// 使用NonAlloc版本，复用预分配数组
int hitCount = Physics.RaycastNonAlloc(origin, direction, hitResults, distance);
```

### NonAlloc API的工作原理

1. **结果直接写入预分配数组**：从数组第0个元素开始覆盖旧数据
2. **返回实际检测数量**：hitCount表示有效结果的数量
3. **忽略旧数据**：只有前hitCount个元素是有效的，后面的元素是旧数据

### 常用NonAlloc API列表

- `Physics.RaycastNonAlloc()`
- `Physics.SphereCastNonAlloc()`
- `Physics.BoxCastNonAlloc()`
- `Physics.CapsuleCastNonAlloc()`

## 三、代码示例：错误 vs 正确用法

### 错误用法（产生GC）

```csharp
void Update()
{
    // 每帧创建新数组，产生GC
    RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 100f);
    
    foreach (RaycastHit hit in hits)
    {
        Debug.Log(hit.collider.name);
    }
}
```

### 正确用法（无GC）

```csharp
// 预分配数组
private RaycastHit[] hitResults = new RaycastHit[50];

void Update()
{
    // 复用预分配数组，无GC
    int hitCount = Physics.RaycastNonAlloc(transform.position, transform.forward, hitResults, 100f);
    
    // 只处理前hitCount个有效结果
    for (int i = 0; i < hitCount; i++)
    {
        RaycastHit hit = hitResults[i];
        Debug.Log(hit.collider.name);
    }
}
```

## 四、扇形检测的GC优化案例

扇形检测是射线检测GC问题的重灾区，因为需要发射多条射线。以下是优化前后的对比：

### 优化前（产生大量GC）

```csharp
public List<GameObject> DetectEnemiesInSector()
{
    List<GameObject> enemies = new List<GameObject>(); // 创建列表，产生GC
    
    int rayCount = 15; // 15条射线覆盖扇形
    float halfAngle = 30f;
    
    for (int i = 0; i < rayCount; i++)
    {
        float angle = -halfAngle + (i * sectorAngle / (rayCount - 1));
        Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
        
        // 每条射线都产生GC
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, 100f);
        
        foreach (RaycastHit hit in hits)
        {
            GameObject enemy = hit.collider.gameObject;
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy); // 列表扩容可能产生GC
            }
        }
    }
    
    return enemies; // 返回新列表，产生GC
}
```

### 优化后（无GC）

```csharp
// 预分配数组和列表
private RaycastHit[] hitResults = new RaycastHit[50];
private List<GameObject> detectedEnemies = new List<GameObject>(50);
private HashSet<GameObject> uniqueEnemies = new HashSet<GameObject>();

public List<GameObject> DetectEnemiesInSectorOptimized()
{
    detectedEnemies.Clear();
    uniqueEnemies.Clear();
    
    int rayCount = 15;
    float halfAngle = 30f;
    
    for (int i = 0; i < rayCount; i++)
    {
        float angle = -halfAngle + (i * sectorAngle / (rayCount - 1));
        Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
        
        // 复用数组，无GC
        int hitCount = Physics.RaycastNonAlloc(transform.position, direction, hitResults, 100f);
        
        for (int j = 0; j < hitCount; j++)
        {
            GameObject enemy = hitResults[j].collider.gameObject;
            uniqueEnemies.Add(enemy);
        }
    }
    
    // 使用HashSet去重，效率更高
    detectedEnemies.AddRange(uniqueEnemies);
    return detectedEnemies;
}
```

## 五、最佳实践总结

### 1. 根据需求选择合适的射线检测API

**检测单个物体（无GC）**：如果只需要检测射线碰到的第一个物体，使用`Physics.Raycast()`：

```csharp
if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
{
    // 处理检测结果
}
```

**检测多个物体（有GC）**：如果需要检测射线碰到的所有物体，使用`Physics.RaycastAll()`：

```csharp
// 每次调用都会产生GC
RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance);
```

**检测多个物体（无GC）**：如果需要检测多个物体且避免GC，使用`Physics.RaycastNonAlloc()`：

```csharp
RaycastHit[] hitResults = ArrayPool<RaycastHit>.Shared.Rent(50);
int hitCount = Physics.RaycastNonAlloc(origin, direction, hitResults, distance);
```

**核心原则**：GC问题与射线数量无关，只与是否使用返回数组的API有关。即使是单条射线，使用`RaycastAll()`也会产生GC。

### 2. 多射线检测使用NonAlloc版本

```csharp
// 预分配数组
private RaycastHit[] hitResults = new RaycastHit[100];

// 使用NonAlloc版本
int hitCount = Physics.RaycastNonAlloc(origin, direction, hitResults, distance);
```

### 3. 合理设置数组大小

- 数组大小应大于等于预期的最大检测数量
- 避免数组过大造成内存浪费
- 避免数组过小导致检测结果被截断

### 4. 使用对象池管理数组和列表

#### 使用.NET ArrayPool<T>（推荐）

对于数组对象池，推荐使用.NET内置的`ArrayPool<T>`，它是经过高度优化的数组对象池实现：

```csharp
using System.Buffers;

// 从ArrayPool获取预分配数组
RaycastHit[] hitResults = ArrayPool<RaycastHit>.Shared.Rent(50);

// 使用数组
int hitCount = Physics.RaycastNonAlloc(origin, direction, hitResults, distance);

// 处理检测结果
for (int i = 0; i < hitCount; i++)
{
    RaycastHit hit = hitResults[i];
    // 处理结果
}

// 归还数组到池中
ArrayPool<RaycastHit>.Shared.Return(hitResults);
```

**优势**：
- 线程安全，适合多线程环境
- 自动管理数组大小，避免过度分配
- 减少内存碎片，提高内存使用效率
- 无需手动实现对象池逻辑

#### 使用自定义列表对象池

对检测结果列表使用对象池，避免频繁创建销毁：

```csharp
// 列表对象池
private List<GameObject> enemyListPool = new List<GameObject>(100);

// 从池中获取列表
List<GameObject> GetEnemyListFromPool()
{
    if (enemyListPool.Count > 0)
    {
        List<GameObject> list = enemyListPool[enemyListPool.Count - 1];
        enemyListPool.RemoveAt(enemyListPool.Count - 1);
        list.Clear();
        return list;
    }
    return new List<GameObject>(50);
}

// 归还列表到池中
void ReturnEnemyListToPool(List<GameObject> list)
{
    list.Clear();
    enemyListPool.Add(list);
}
```

## 六、总结

射线检测的GC问题**完全可以避免**，关键在于：

1. 理解GC问题的根源：**数组创建**而非射线检测本身
2. 选择正确的API：根据需求选择Raycast、RaycastAll或RaycastNonAlloc
3. 合理管理内存：使用预分配数组或ArrayPool避免频繁创建
4. 遵循最佳实践：根据检测需求选择合适的API

通过正确的优化措施，您可以在享受射线检测便利性的同时，完全消除其GC开销，为游戏性能保驾护航。

## 延伸阅读

- Unity官方文档：[Physics.RaycastNonAlloc](https://docs.unity3d.com/ScriptReference/Physics.RaycastNonAlloc.html)
- Unity性能优化指南：[减少垃圾回收](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity3.html#ReducingGarbageCollection)
- .NET ArrayPool文档：[ArrayPool<T> Class](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1?view=net-7.0)