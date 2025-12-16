---
layout: post
title: 信号量（Semaphore）超详细教学：从原理到Unity实战（小球碰撞音效限流）
date: 2025-11-27
header-img: img/post-bg-universe.jpg
categories: [并发编程]
tags: [Unity, 信号量, Semaphore]
---

# 信号量（Semaphore）超详细教学：从原理到 Unity 实战（小球碰撞音效限流）

在编程中，我们常遇到 “资源有限，多操作争抢” 的问题 —— 比如多线程抢打印机、Unity 多小球碰撞同时触发音效导致噪音。锁（`lock`）能解决 “同一时间 1 个操作执行”，但需 “同时允许 3 个、5 个操作” 时，锁就无能为力了。这时，**信号量（Semaphore）** 就是最优解。

本文从核心概念、与锁的对比（重点解答 “写法几乎一致” 的疑问），到 Unity 实战（小球碰撞音效限流），一步步讲透信号量，新手也能看懂、会用。

## 一、信号量核心概念：一句话搞懂

信号量的本质是 **“限流工具”**：它维护一个**许可数（N）**，限制**同一时间最多有 N 个操作（线程 / 逻辑）访问某一资源**。



* 操作需访问资源时，先**申请许可（WaitOne()）**：有剩余许可则占用 1 个，无则等待 / 放弃；

* 操作用完资源后，**释放许可（Release()）**：归还 1 个许可，供其他操作使用。

简单比喻：信号量像超市收银台数量（N=3），同一时间最多 3 个顾客结账，第 4 个需排队。

## 二、信号量与锁（lock）的对比：写法几乎一致，功能更灵活

这是你最关心的点：**信号量和锁的写法几乎是 “同一位置替换”**，但信号量支持更多并发，是锁的 “升级版”。

### 2.1 写法对比：核心流程完全一致（位置替换）

锁（`lock`）是 C# 语法糖，自动处理 “申请 / 释放”；信号量需手动调用`WaitOne()`和`Release()`，但操作位置对应锁的 “大括号开头 / 结尾”。

#### ① 锁（lock）的写法：适用于 N=1（同一时间 1 个操作）



```csharp
using UnityEngine;

public class LockDemo : MonoBehaviour
{
    // 1. 定义锁对象（全局/类级，必须是引用类型）
    private readonly object _lockObj = new object();
    void Start()
    {
        // 模拟多次调用受限逻辑
        for (int i = 0; i < 5; i++)
        {
            DoSomethingWithLock(i);
        }
    }
    // 2. 执行受限逻辑：锁包裹代码块
    void DoSomethingWithLock(int index)
    {
        // 申请锁（自动）→ 执行逻辑 → 释放锁（自动）
        lock (_lockObj)
        {
            Debug.Log($"操作{index}：开始执行（锁限制，同一时间1个）");
            // 模拟耗时操作（如播放音效、打印）
            System.Threading.Thread.Sleep(500);
            Debug.Log($"操作{index}：执行完成");
        }
    }
}
```

#### ② 信号量的写法：适用于 N≥1（这里 N=1，和锁等价）



```csharp
using UnityEngine;
using System.Threading;

public class SemaphoreDemo : MonoBehaviour
{
    // 1. 定义信号量（全局/类级，N=1：初始1个许可，最大1个许可）
    private Semaphore _semaphore = new Semaphore(1, 1);
    void Start()
    {
        // 模拟多次调用受限逻辑
        for (int i = 0; i < 5; i++)
        {
            DoSomethingWithSemaphore(i);
        }
    }
    // 2. 执行受限逻辑：信号量替换锁的位置
    void DoSomethingWithSemaphore(int index)
    {
        // 申请许可（手动，对应锁的大括号开头）
        bool hasPermission = _semaphore.WaitOne(0); // 非阻塞式，Unity推荐
        if (hasPermission)
        {
            try
            {
                Debug.Log($"操作{index}：开始执行（信号量N=1，和锁等价）");
                // 模拟耗时操作
                System.Threading.Thread.Sleep(500);
                Debug.Log($"操作{index}：执行完成");
            }
            finally
            {
                // 释放许可（手动，对应锁的大括号结尾）
                _semaphore.Release();
            }
        }
    }
}
```

### 2.2 功能差异：信号量支持 N>1（锁做不到）

若将信号量许可数设为`N=3`（同一时间 3 个操作执行），锁完全无法替代，只需修改初始化代码：



```csharp
// 信号量N=3：初始3个许可，最大3个许可
private Semaphore _semaphore = new Semaphore(3, 3);
```

此时同一时间 3 个操作并行，第 4 个需等待 —— 这是锁（仅 N=1）的核心局限。

### 2.3 关键总结：锁与信号量的关系



| 特性   | 锁（lock）        | 信号量（Semaphore）                   |
| ---- | -------------- | -------------------------------- |
| 写法   | 自动申请 / 释放（语法糖） | 手动申请（WaitOne()）/ 释放（Release()） |
| 并发支持 | 仅 N=1（互斥）      | 支持 N≥1（有限并发）                     |
| 核心位置 | 包裹代码块的大括号      | WaitOne() 对应开头，Release() 对应结尾  |
| 本质   | 信号量的子集         | 锁的升级版                            |

## 三、Unity 实战：小球碰撞音效限流（核心例子）

用**信号量限制 “同一时间最多播放 2 个碰撞音效”**，解决多小球碰撞音效重叠的问题。

### 核心设计原则（重点！）



* **音效管理全局唯一**：信号量需全局唯一（否则每个小球有自己的信号量，限流失效）；

* **职责分离**：


  * 「全局音效管理类」：负责信号量限流和音效播放（单例，挂空物体，仅 1 个实例）；

  * 「小球碰撞检测类」：仅检测碰撞，调用音效管理类方法（每个小球挂 1 份）。

### 3.1 场景准备（30 秒搞定）



1. **创建小球**：Unity 场景新建 3\~5 个`Sphere`（小球），命名`Ball1`/`Ball2`…，摆放在 Y=2 高度（重力下落碰撞）；

2. **物理组件**：

* 每个小球保留`Sphere Collider`，**取消勾选**`Is Trigger`（开启物理碰撞）；

* 每个小球加`Rigidbody`，**取消勾选**`Is Kinematic`，`Gravity Scale`设 1（开启重力）；

1. **标签设置**：给所有小球加`Ball`标签（判断碰撞对象）；

2. **音频准备**：导入短碰撞音效（如`collision.wav`），放`Audio`文件夹。

### 3.2 脚本 1：全局音效管理类（SoundManager）—— 信号量核心逻辑

单例类，负责信号量限流和音效播放，挂空物体（仅 1 个实例）。



```csharp
using UnityEngine;
using System.Threading;
using System.Collections;

/// <summary>
/// 全局音效管理类（单例）：负责信号量限流和碰撞音效播放
/// </summary>
public class SoundManager : MonoBehaviour
{
    // 单例：确保全局只有1个SoundManager实例
    public static SoundManager Instance;
    
    [Header("音效配置")]
    public AudioClip collisionClip; // 拖拽碰撞音效到这里
    public int maxSoundCount = 2; // 最多同时播放2个音效（信号量许可数）
    
    // 信号量：全局唯一的限流工具
    private Semaphore _soundSemaphore;
    
    // 全局音频源（1个足够，避免每个小球都加）
    private AudioSource _audioSource;
    
    private void Awake()
    {
        // 单例逻辑：防止重复创建
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换不销毁（可选）
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 初始化信号量：许可数=maxSoundCount
        _soundSemaphore = new Semaphore(maxSoundCount, maxSoundCount);
        
        // 添加并配置全局音频源
        _audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false; // 不自动播放
        _audioSource.loop = false; // 不循环
    }
    
    /// <summary>
    /// 对外提供的音效播放方法（小球碰撞时调用）
    /// </summary>
    /// <param name="volume">音效音量（按碰撞力度调整）</param>
    public void PlayCollisionSound(float volume)
    {
        // 申请信号量许可：非阻塞式（Unity主线程不能阻塞）
        bool hasPermission = _soundSemaphore.WaitOne(0);
        if (hasPermission)
        {
            try
            {
                // 播放音效：PlayOneShot支持同时播放多个
                _audioSource.PlayOneShot(collisionClip, Mathf.Clamp01(volume));
                
                // 协程：音效播完后释放信号量（延时释放，关键！）
                StartCoroutine(ReleaseSemaphoreAfterSound());
            }
            catch (System.Exception e)
            {
                Debug.LogError("播放碰撞音效出错：" + e.Message);
                // 报错时立即释放，避免信号量泄露
                _soundSemaphore.Release();
            }
        }
        // 无许可则跳过，避免噪音
    }
    
    /// <summary>
    /// 协程：等待音效播放完释放信号量（非阻塞延时）
    /// </summary>
    private IEnumerator ReleaseSemaphoreAfterSound()
    {
        // 等待音效总时长（clip.length为秒数）
        yield return new WaitForSeconds(collisionClip.length);
        
        // 释放许可
        _soundSemaphore.Release();
    }
    
    // 场景销毁时释放信号量，避免内存泄漏
    private void OnDestroy()
    {
        _soundSemaphore?.Dispose();
    }
}
```

### 3.3 脚本 2：小球碰撞检测类（BallCollision）—— 仅负责碰撞检测

挂在**每个小球**上，仅检测碰撞，调用音效管理类方法。



```csharp
using UnityEngine;

/// <summary>
/// 小球碰撞检测类：只检测碰撞，不处理音效和信号量
/// </summary>
public class BallCollision : MonoBehaviour
{
    /// <summary>
    /// 碰撞开始时触发
    /// </summary>
    /// <param name="other">碰撞的另一个物体</param>
    private void OnCollisionEnter(Collision other)
    {
        // 只响应与其他小球的碰撞（通过Ball标签判断）
        if (other.gameObject.CompareTag("Ball"))
        {
            // 计算碰撞力度：用于调整音量
            float collisionForce = other.relativeVelocity.magnitude;
            
            // 过滤轻微碰撞（避免轻轻碰也发声）
            if (collisionForce > 0.5f)
            {
                // 调用全局音效管理类的播放方法（不处理信号量）
                SoundManager.Instance?.PlayCollisionSound(collisionForce / 10f);
            }
        }
    }
}
```

### 3.4 挂载步骤（关键！一步不能错）



| 脚本            | 挂载对象                 | 操作步骤                                                      |
| ------------- | -------------------- | --------------------------------------------------------- |
| SoundManager  | 空物体（命名 SoundManager） | 1. 场景右键→创建空物体→命名 SoundManager； 拖脚本到空物体； 赋值 collisionClip。 |
| BallCollision | 每个小球                 | 1. 选中所有小球；. 拖脚本到每个小球（每个小球都有该脚本）。                          |

### 3.5 效果演示

点击 Unity 播放按钮，小球下落碰撞：



* 同一时间最多播放 2 个音效（信号量限流）；

* 第 3 次及以后碰撞因许可被占，暂不播放；

* 音效播完后释放许可，后续碰撞可正常发声。

## 四、新手避坑：关键注意事项



1. **信号量必须手动释放**：务必在`finally`或协程中调用`Release()`，否则许可泄露（后续操作无法执行）；

2. **Unity 主线程用非阻塞 WaitOne()**：禁用无参数`WaitOne()`（阻塞主线程致卡死），用`WaitOne(0)`（立即检查许可）；

3. **延时释放是必要的**：音效场景需用协程 / Invoke 实现 “播完释放”，立即释放会导致限流失效；

4. **单例确保全局唯一**：信号量需全局唯一，通过单例`SoundManager`实现，避免多实例导致限流失效。

## 五、总结



1. **信号量核心作用**：限制同一时间 N 个操作执行（限流），是锁的升级版（锁仅 N=1）；

2. **写法特点**：`WaitOne()`和`Release()`替换锁的大括号，核心流程一致；

3. **Unity 实战价值**：不仅解决多线程资源竞争，还能复用限流逻辑解决单线程业务需求（如音效限制）；

4. **设计原则**：信号量全局唯一，职责分离（碰撞检测与音效管理分开），代码易维护。

掌握信号量后，你可以轻松应对 “有限资源并发” 问题 —— 锁解决 “1 个操作”，信号量解决 “N 个操作”，这就是两者的核心区别。
