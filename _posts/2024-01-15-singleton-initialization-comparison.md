---
layout: post
title: "Singleton 初始化位置差异解析：饿汉式与懒汉式"
date: 2024-01-15
tags: [C#, 设计模式, 单例模式]
categories: 编程技术
---

# Singleton 初始化位置差异解析：饿汉式与懒汉式

核心结论先明确：**饿汉式的实例初始化绝对不是写在私有构造函数里，而懒汉式的初始化也并非只能写在属性里（方法里也可以）**。两者的初始化位置差异，本质是由**“创建时机”**（类型初始化阶段 vs 第一次使用时）决定的。

### 一、先澄清：私有构造函数的真正作用

不管是饿汉式还是懒汉式，**私有构造函数的唯一核心作用是禁止外部通过** **`new`** **关键字创建类的实例**，它是实现单例的“必要条件”，但**不是**用来初始化单例实例的地方。

```csharp
private Singleton()
{
    // 这里是实例构造逻辑（比如初始化实例的成员变量），不是创建单例实例本身的逻辑
    // 比如：_logPath = "log.txt";
}
```

如果把单例实例的创建逻辑写在私有构造函数里，反而会陷入“先有鸡还是先有蛋”的矛盾——因为构造函数只有创建实例时才会执行，而你又需要通过构造函数来创建实例，这在逻辑上是不成立的。

### 二、饿汉式：初始化在**静态字段/静态构造函数**中（类型初始化阶段）

饿汉式的核心是**在类的“类型初始化阶段”就完成实例创建**（CLR保证线程安全），这个阶段的代码只有两个位置：

1. **直接在静态字段声明时初始化**（最常用、最简洁的方式）；

2. **在静态构造函数中初始化**（属于类型初始化的一部分，效果和上面一致）。

这两个位置都属于CLR的类型初始化流程，和私有构造函数没有关系。

#### 示例1：饿汉式（静态字段直接初始化，最常见）

```csharp
public sealed class SingletonHungry1
{
    // 👉 饿汉式的初始化：静态字段声明时直接创建实例（类型初始化阶段）
    private static readonly SingletonHungry1 _instance = new SingletonHungry1();

    // 私有构造函数：禁止外部new，和初始化无关
    private SingletonHungry1() { }

    // 仅提供访问点
    public static SingletonHungry1 Instance => _instance;
}
```

#### 示例2：饿汉式（静态构造函数中初始化）

```csharp
public sealed class SingletonHungry2
{
    private static readonly SingletonHungry2 _instance;

    // 👉 静态构造函数：属于类型初始化阶段，CLR保证只执行一次、线程安全
    static SingletonHungry2()
    {
        _instance = new SingletonHungry2();
    }

    private SingletonHungry2() { }

    public static SingletonHungry2 Instance => _instance;
}
```

这两种饿汉式的效果完全一致，都是在类型初始化时创建实例，天生线程安全。

### 三、懒汉式：初始化在**静态属性/静态方法**中（第一次使用时）

懒汉式的核心是**懒加载**（第一次调用访问点时才创建实例），所以初始化逻辑必须放在**用户触发访问的入口处**——也就是静态属性的`get`访问器，或者静态方法里（本质是同一个逻辑）。

#### 示例1：懒汉式（静态属性中初始化，最常用）

```csharp
public sealed class SingletonLazy
{
    private static SingletonLazy _instance;
    private static readonly object _lockObj = new object();

    private SingletonLazy() { }

    // 👉 懒汉式的初始化：在静态属性的get访问器中（第一次调用时执行）
    public static SingletonLazy Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new SingletonLazy(); // 初始化逻辑
                    }
                }
            }
            return _instance;
        }
    }
}
```

#### 示例2：懒汉式（静态方法中初始化，效果相同）

```csharp
public sealed class SingletonLazy2
{
    private static SingletonLazy2 _instance;
    private static readonly object _lockObj = new object();

    private SingletonLazy2() { }

    // 👉 懒汉式的初始化：在静态方法中（第一次调用时执行）
    public static SingletonLazy2 GetInstance()
    {
        if (_instance == null)
        {
            lock (_lockObj)
            {
                if (_instance == null)
                {
                    _instance = new SingletonLazy2(); // 初始化逻辑
                }
            }
        }
        return _instance;
    }
}
```

甚至C#的`Lazy<T>`实现的懒汉式，本质也是把初始化逻辑放在`Lazy<T>`的委托中，在`Value`属性（访问点）被调用时执行，依然属于这个范畴。

### 总结

1. **饿汉式的初始化位置**：只能在**静态字段声明时**或**静态构造函数中**（类型初始化阶段），**绝对不是私有构造函数**；私有构造函数仅用于禁止外部`new`。

2. **懒汉式的初始化位置**：在**静态属性的get访问器**或**静态方法**中（第一次访问时），核心是“用户触发时才初始化”。

3. 两者的初始化位置差异，根源是**创建时机**：饿汉式是“类加载时就创建”，懒汉式是“第一次使用时才创建”。

以下写法是所有单例都具备的，不能用来判断饿汉 / 懒汉：
- 私有构造函数（private 类名()）：目的是禁止外部new，所有单例都有。
- 提供静态的访问入口（无论是GetInstance方法还是Instance属性）：C# 更推荐用静态属性，但这不是区分特征。


在 C# 中，从写法层面判断饿汉式和懒汉式，核心要点如下：
- **核心依据**：实例化代码的位置 —— 
  - 饿汉式的new出现在**静态字段声明处 / 静态构造函数**；
  - 懒汉式的new出现在**静态方法 / 属性 get 访问器 / 嵌套类 /Lazy<T>的委托中**。
- **直观特征**：饿汉式字段加**readonly且非null**，访问入口无逻辑；懒汉式字段初始**为null**，有if (null)判空，可选lock/volatile/Lazy<T>。
- **C# 特有技巧**：懒汉式优先使用Lazy<T>类，这是.NET 框架提供的线程安全、简洁的懒加载方案，是实际开发中的最佳实践。

从记忆角度，除了初始化位置和创建时机，你还可以记住这 4 个核心区分点：
1. 线程安全：饿汉**天生安全**，懒汉默认不安全（需手动加锁）。
2. 资源占用：饿汉**提前占资源**，懒汉按**需占资源**（结合 “饿 / 懒” 字面记忆）。
3. 实现复杂度：饿汉简单直接，懒汉**有多个复杂版本**。
4. 异常处理：饿汉**初始化异常会导致类加载失败**，懒汉**可在方法内捕获异常**。
