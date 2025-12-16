---
layout: post
title: Unity 游戏开发中的命令模式：从入门到实战（附极简代码 + 命令栈实现）
date: 2025-11-26
header-img: img/post-bg-game.jpg
categories: [设计模式]
tags: [Unity, 命令模式]
---

# Unity 游戏开发中的命令模式：从入门到实战（附极简代码 + 命令栈实现）

在游戏开发中，我们经常需要处理**操作撤销、技能系统、输入与逻辑解耦合**等场景，而**命令模式**作为一种经典的行为型设计模式，正是解决这类问题的 “利器”。很多新手会觉得命令模式复杂、难以理解，其实它的核心思想非常简单 ——**把 “操作” 打包成独立的对象**。

本文将从**新手视角**出发，用最通俗的语言和极简的代码，讲解命令模式的核心逻辑、与原始写法的差异、实战实现（含命令栈撤销 / 重做），以及游戏开发中的实际应用场景，内容兼顾专业性和易懂性，适合刚接触设计模式的开发者学习。

## 一、命令模式的核心思想：把操作 “打包” 成对象

命令模式的本质可以用一句话概括：**将 “请求 / 操作” 封装成一个独立的命令对象，使调用者与执行者完全解耦**。

举个生活中的例子：



* 你（**调用者**）去餐厅点餐，只需要把写好的菜单（**命令对象**）交给服务员，不用直接指挥厨师；

* 厨师（**接收者**）按照菜单执行操作，不用知道是谁点的餐；

* 菜单上写清楚了 “要做什么”（比如宫保鸡丁），这就是命令对象的核心作用。

对应到游戏开发中：



* 玩家按空格键（调用者）→ 触发 “跳跃命令”（命令对象）→ 角色执行跳跃（接收者）。

## 二、先看：不用命令模式的原始写法（耦合严重）

我们以 **Unity 中 “玩家按空格键让角色跳跃”** 为例，先看最直接的写法，感受其中的问题：



```csharp
using UnityEngine;

// 接收者：角色（实际执行跳跃的对象）
public class Player : MonoBehaviour
{
    public void Jump()
    {
        Debug.Log("角色跳跃！");
        transform.Translate(Vector3.up * 2f); // 简单模拟跳跃
    }
}

// 调用者：输入管理器（触发操作的对象）
public class InputManager : MonoBehaviour
{
    private Player _player;
    private void Awake()
    {
        _player = FindObjectOfType<Player>();
    }
    private void Update()
    {
        // 按下空格，直接调用角色的Jump方法
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _player.Jump();
        }
    }
}
```

**问题分析**：



* 调用者（`InputManager`）直接依赖接收者（`Player`），还需要知道接收者的方法名（`Jump()`），耦合性极高；

* 无法实现**撤销操作**（比如角色跳错了，想回到原来的位置）；

* 新增操作（比如攻击）时，需要修改`InputManager`的代码，违反 “开闭原则”。

## 三、命令模式的核心：新增的 3 个关键层（解耦的关键）

使用命令模式后，会在**调用者**和**接收者**之间新增**3 个核心层**（其中`CommandManager`是实战必备的扩展层，非模式强制要求），正是这几层实现了完全解耦：



| 层级                         | 作用（通俗解释）              | 对应示例                              |
| -------------------------- | --------------------- | --------------------------------- |
| **命令接口层（ICommand）**        | 定义所有命令的统一标准（执行 + 撤销）  | `ICommand`（含`Execute()`/`Undo()`） |
| **具体命令类层**                 | 封装具体操作（绑定接收者和操作逻辑）    | `JumpCommand`（封装跳跃操作）             |
| **命令管理器层（CommandManager）** | 管理命令的执行、撤销、重做（用栈记录命令） | `CommandManager`（含命令栈）            |

**结构变化**：



```
调用者（InputManager）→ CommandManager → 命令接口（ICommand）→ 具体命令（JumpCommand）→ 接收者（Player）
```

## 四、Unity 实战：极简版命令模式（带命令栈，支持撤销 / 重做）

下面给出**实战中最常用的简化版代码**，保留核心功能（执行、撤销、重做），去掉冗余逻辑，注释清晰，可直接复制到 Unity 中测试：

### 1. 命令接口（核心标准）



```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 命令接口：所有命令的统一标准（执行+撤销）
/// </summary>
public interface ICommand
{
    void Execute(); // 执行命令
    void Undo();    // 撤销命令
}
```

### 2. 接收者：角色（实际执行操作）



```csharp
/// <summary>
/// 角色（接收者）：包含具体操作和撤销的回滚逻辑
/// </summary>
public class Player : MonoBehaviour
{
    private Vector3 _jumpOriginPos; // 记录跳跃前位置（用于撤销）
    
    // 跳跃操作
    public void Jump()
    {
        _jumpOriginPos = transform.position; // 记录原始位置
        transform.Translate(Vector3.up * 2f);
        Debug.Log($"角色跳跃：位置→{transform.position}");
    }
    
    // 撤销跳跃（回滚逻辑）
    public void UndoJump()
    {
        transform.position = _jumpOriginPos;
        Debug.Log($"撤销跳跃：位置→{transform.position}");
    }
    
    // 攻击操作
    public void Attack()
    {
        Debug.Log("角色发起攻击！");
    }
    
    // 撤销攻击
    public void UndoAttack()
    {
        Debug.Log("撤销攻击：取消攻击效果");
    }
}
```

### 3. 具体命令类（封装操作）



```csharp
/// <summary>
/// 跳跃命令：绑定角色和跳跃操作
/// </summary>
public class JumpCommand : ICommand
{
    private readonly Player _player; // 接收者引用
    
    public JumpCommand(Player player)
    {
        _player = player;
    }
    
    public void Execute() => _player.Jump();
    public void Undo() => _player.UndoJump();
}

/// <summary>
/// 攻击命令：绑定角色和攻击操作
/// </summary>
public class AttackCommand : ICommand
{
    private readonly Player _player;
    
    public AttackCommand(Player player)
    {
        _player = player;
    }
    
    public void Execute() => _player.Attack();
    public void Undo() => _player.UndoAttack();
}
```

### 4. 命令管理器（核心：命令栈，管理撤销 / 重做）



```csharp
/// <summary>
/// 命令管理器（实战必备）：用栈管理命令，支持执行、撤销、重做
/// </summary>
public class CommandManager : MonoBehaviour
{
    // 单例：全局唯一（Unity游戏开发常用）
    public static CommandManager Instance { get; private set; }
    
    // 执行栈：存储已执行的命令（用于撤销）
    private readonly Stack<ICommand> _executeStack = new Stack<ICommand>();
    
    // 重做栈：存储被撤销的命令（用于重做）
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
    
    private void Awake()
    {
        // 单例初始化（避免多个管理器）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 执行命令（核心方法）
    /// </summary>
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _executeStack.Push(command); // 压入执行栈
        _redoStack.Clear(); // 执行新命令后，清空重做栈（符合用户直觉）
    }
    
    /// <summary>
    /// 撤销上一个命令
    /// </summary>
    public void UndoLast()
    {
        if (_executeStack.Count == 0)
        {
            Debug.LogWarning("无命令可撤销！");
            return;
        }
        
        var cmd = _executeStack.Pop();
        cmd.Undo();
        _redoStack.Push(cmd); // 压入重做栈
    }
    
    /// <summary>
    /// 重做上一个被撤销的命令
    /// </summary>
    public void RedoLast()
    {
        if (_redoStack.Count == 0)
        {
            Debug.LogWarning("无命令可重做！");
            return;
        }
        
        var cmd = _redoStack.Pop();
        cmd.Execute();
        _executeStack.Push(cmd);
    }
}
```

### 5. 调用者：输入管理器（仅检测输入，解耦合）



```csharp
/// <summary>
/// 输入管理器（调用者）：仅检测输入，不关心具体操作
/// </summary>
public class InputManager : MonoBehaviour
{
    private Player _player;
    
    private void Awake()
    {
        _player = FindObjectOfType<Player>();
    }
    
    private void Update()
    {
        // 空格：执行跳跃命令
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CommandManager.Instance.ExecuteCommand(new JumpCommand(_player));
        }
        
        // 鼠标左键：执行攻击命令
        if (Input.GetMouseButtonDown(0))
        {
            CommandManager.Instance.ExecuteCommand(new AttackCommand(_player));
        }
        
        // Z键：撤销
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CommandManager.Instance.UndoLast();
        }
        
        // R键：重做
        if (Input.GetKeyDown(KeyCode.R))
        {
            CommandManager.Instance.RedoLast();
        }
    }
}
```

### 测试步骤（Unity 中直接运行）



1. 给空物体挂载`CommandManager`、`InputManager`；

2. 给角色挂载`Player`脚本；

3. 运行游戏：按空格跳跃→按鼠标左键攻击→按 Z 键撤销→按 R 键重做，观察控制台输出。

## 五、命令模式的核心优势（新手必懂）

新增的几层看似多写了代码，但带来的好处是原始写法无法比拟的，也是工业级项目中必须考虑的点：

### 1. 完全解耦

调用者（`InputManager`）只需要创建命令对象，交给`CommandManager`即可，**不需要知道接收者（**`Player`**）的存在，也不需要知道操作的具体逻辑**。比如要把 “跳跃” 改成 “冲刺”，只需要新增`DashCommand`，无需修改`InputManager`。

### 2. 轻松实现撤销 / 重做

通过`CommandManager`的命令栈，能记录操作的顺序，实现多次撤销 / 重做 —— 这是游戏开发中（如战棋游戏悔棋、编辑器 Ctrl+Z）的核心需求，原始写法几乎无法实现。

### 3. 符合开闭原则

新增操作时，只需要添加新的具体命令类（如`DashCommand`、`SkillCommand`），无需修改原有代码，降低维护成本。

### 4. 支持批量操作（宏命令）

可以把多个命令组合成一个 “宏命令”（比如格斗游戏的连招），一次性执行，扩展能力极强。

## 六、游戏开发中的常见使用场景（新手重点关注）

命令模式在游戏开发中应用广泛，新手可以重点关注以下场景：

### 1. 操作撤销 / 悔棋



* 战棋游戏（如《三国志》）：玩家移动棋子、释放技能后，点击 “悔棋” 按钮，通过命令栈撤销操作；

* 关卡编辑器：玩家摆放物体后，按 Ctrl+Z 撤销操作。

### 2. 输入与逻辑解耦合



* 自定义按键：玩家把 “跳跃键” 从空格改成 C 键，只需要绑定命令对象，无需修改输入逻辑；

* 多平台适配：PC 端用键盘、移动端用触屏，触发的是同一个命令对象，逻辑统一。

### 3. 技能系统 / 连招系统



* 技能释放：把每个技能封装成命令对象，支持技能撤销（如 BUG 导致的误放）、技能组合（连招）；

* 自动战斗：把多个技能命令按顺序放入队列，让角色自动执行。

### 4. 异步操作的统一管理



* 异步加载资源：把 “加载完成后更新 UI” 封装成命令对象，交给主线程执行，避免线程安全问题。

## 七、总结（核心要点，新手牢记）



1. **命令模式的核心**：将操作封装成独立的命令对象，实现调用者与接收者的解耦；

2. **核心新增层**：命令接口、具体命令类、`CommandManager`（命令栈），这是实战中必备的结构；

3. **核心优势**：解耦、支持撤销 / 重做、符合开闭原则、可扩展宏命令；

4. **适用场景**：操作撤销、输入解耦合、技能系统、连招系统等。

命令模式并不是 "多余的复杂代码"，而是工业级项目中解决特定问题的标准方案。新手可以先从极简版代码入手，理解核心逻辑后，再根据项目需求扩展功能（如宏命令、命令序列化）。

## 八、命令模式的灵活实践：简化版实现与选择

前面介绍的三层结构（接口 + 具体类 + 管理层）是命令模式的标准实现，但在实际开发中，并非所有场景都需要这么 "重" 的结构。本章节将补充命令模式的**简化实践（委托实现）**，并解答：什么场景下无需三层结构？简化版为何算命令模式？以及如何区分不同实现方案的选择。

### 1. 核心思想：命令模式的本质是 "思想"，而非 "三层结构"

命令模式的核心是 **"封装操作 + 解耦调用者与接收者"**，而 "三层结构" 只是实现这一思想的**常见方式**，而非唯一方式。

GoF 对命令模式的定义核心是：
> 将 "请求 / 操作" 封装为独立对象，实现 "发出请求的调用者" 与 "执行请求的接收者" 解耦，并支持操作的排队、存储、扩展（如撤销）。

简化版命令模式（委托实现），正是在 "无需扩展功能（如撤销）" 的场景下，用更轻量的方式实现了命令模式的核心思想。

### 2. 无需使用三层命令模式的场景

当场景满足以下条件时，简化版（委托实现）更合适：

#### （1）操作简单，无需 "扩展能力"
当操作仅需 "执行"，不需要 "撤销、重做、日志记录" 时，三层结构的 "扩展设计" 成了冗余。

**典型案例**：
- Unity 子线程更新 UI：子线程执行耗时任务后，仅需通知主线程更新文本、图片等简单 UI；
- 后台任务回调：接口请求成功后，仅需更新页面数据；
- 简单工具的单次操作：游戏中 "领取奖励" 后，仅需弹出提示文本。

#### （2）追求开发效率，避免 "抽象过度"
三层结构需要为每个操作写一个具体命令类（如`UpdateTextCommand`、`ChangeColorCommand`），若操作类型多但逻辑简单，会导致 "类爆炸"。

#### （3）操作需 "排队执行"，但无需复杂管理
当操作需要 "跨线程调度" 或 "延迟执行"（如子线程任务需主线程执行），但仅需 "排队" 而无需 "存储历史记录" 时，简化版的队列 + 委托足以满足需求。

**典型案例**：Unity 的`MainThreadDispatcher`—— 子线程将 UI 操作封装为委托，加入主线程队列，主线程逐帧执行。

### 3. 简化版（委托实现）：为何算命令模式？

简化版完全满足命令模式的核心角色和意图，只是用 C# 的`Action`/`Func`委托，隐式替代了三层结构中的 "命令接口" 和 "具体命令类"。

| 命令模式核心角色 | 三层结构实现 | 简化版实现（委托） | 角色职责（两者完全一致） |
| ---------------- | ------------ | ------------------ | ------------------------ |
| 命令接口（ICommand） | 自定义`ICommand`接口（含`Execute()`） | `System.Action`委托（含`Invoke()`） | 定义 "执行操作" 的统一契约 |
| 具体命令类 | `JumpCommand`类（封装跳跃操作） | lambda 表达式（如`() => player.Jump()`） | 封装 "具体操作逻辑" 和 "接收者" |
| 调用者（Invoker） | `CommandManager`类（管理命令队列） | `MainThreadDispatcher`类（管理委托队列） | 负责 "存储命令"和"调度执行" |
| 接收者（Receiver） | `Player`组件 | `Player`组件 | 实际执行操作的对象 |

**代码对比**：

```csharp
// 三层结构实现：跳跃命令
public class JumpCommand : ICommand {
    private Player _player;
    public JumpCommand(Player player) { _player = player; }
    public void Execute() { _player.Jump(); }
}
// 调用者执行：
commandManager.ExecuteCommand(new JumpCommand(player));

// 简化版实现：委托封装命令
Action jumpCommand = () => player.Jump(); // 封装接收者+逻辑
commandQueue.Enqueue(jumpCommand); // 调用者执行
```

两者的逻辑完全一致：都是 "封装操作→交给调用者→调用者触发执行"，唯一区别是简化版用委托省去了 "自定义接口和类" 的步骤。

### 4. 关键区分："单纯用委托" vs "用委托实现命令模式"

核心区别在于：**是否满足 "封装操作 + 解耦 + 操作管理" 的命令模式核心意图**。

#### （1）单纯用委托（不是命令模式）——Unity 按钮点击回调
```csharp
button.onClick.AddListener(() => {
    Debug.Log("点击成功");
    text.text = "点击成功";
});
```
**为什么不是？**
- 没有 "解耦"：按钮直接绑定操作逻辑，没有 "调用者" 中间层；
- 没有 "操作管理"：逻辑触发后直接执行，没有 "排队、存储"；
- 核心目的是 "回调"，而非 "封装操作并管控"。

#### （2）用委托实现命令模式（是命令模式）——Unity 主线程更新 UI
```csharp
// 子线程：封装操作成委托（命令对象）
Action uiCommand = () => text.text = "任务完成";
// 交给调用者（MainThreadDispatcher）管理
mainThreadDispatcher.Enqueue(uiCommand);

// 主线程：调用者逐帧执行命令
private void Update() {
    while (queue.Count > 0) {
        var command = queue.Dequeue();
        command.Invoke();
    }
}
```
**为什么是？**
- 解耦：子线程不知道主线程何时执行，主线程不知道命令具体逻辑；
- 操作管理：命令被存储在队列中，支持 "排队执行"；
- 封装操作：委托是独立对象，包含 "接收者" 和 "操作逻辑"。

### 5. 三层模式 vs 简化版：选择指南

| 维度 | 三层命令模式（接口 + 类） | 简化版命令模式（委托） |
| ---- | ------------------------- | ---------------------- |
| 核心优势 | 支持撤销、重做、日志记录；操作类型清晰 | 代码轻量、开发效率高；避免类爆炸 |
| 核心劣势 | 代码冗余；抽象过度（简单场景下） | 不支持扩展功能；复杂场景下逻辑分散 |
| 适用场景 | 1. 需要撤销/重做（如编辑器、设计工具）；2. 操作类型多且需长期维护；3. 需要日志记录 | 1. 简单操作（如 UI 更新、后台任务回调）；2. 无需扩展功能；3. 跨线程排队执行 |
| 典型案例 | Word 的 "撤销打字"、PS 的 "历史记录"、审批系统的 "步骤回退" | Unity 子线程更新 UI、APP 的 "接口请求回调"、工具的 "单次操作提示" |

## 九、总结（核心要点，新手牢记）

命令模式的价值，从来不是 "写出接口 + 类的三层结构"，而是 "用封装和解耦的思想，解决操作管控的问题"。

1. **命令模式的核心**：将操作封装成独立的命令对象，实现调用者与接收者的解耦；
2. **实现方式的选择**：
   - 当需要 "精细化管控"（撤销、日志）时，选择三层结构；
   - 当只需要 "简单执行 + 解耦" 时，选择委托简化版；
3. **判断标准**：是否封装了操作、是否解耦了调用者与接收者——而非 "是否用了接口" 或 "是否用了委托"。

希望本文能帮助你跳出 "设计模式 = 固定模板" 的误区，真正做到 "因场景择方案"，让设计模式成为提升效率的工具，而非束缚思维的框架。

希望本文能帮助你彻底理解命令模式，并能在 Unity 项目中灵活应用！