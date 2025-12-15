---
layout: post
title: Unity 游戏开发中的命令模式：从入门到实战（附极简代码 + 命令栈实现）
date: 2025-11-26
header-img: img/post-bg-game.jpg
categories: [设计模式]
tags: [Unity, 命令模式]
---

# Unity 游戏开发中的命令模式：从入门到实战（附极简代码 + 命令栈实现）

在游戏开发中，我们经常需要处理**操作撤销、技能系统、输入与逻辑解耦**等场景，而**命令模式**作为一种经典的行为型设计模式，正是解决这类问题的 “利器”。很多新手会觉得命令模式复杂、难以理解，其实它的核心思想非常简单 ——**把 “操作” 打包成独立的对象**。

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

我们以 \*\*Unity 中 “玩家按空格键让角色跳跃”\*\* 为例，先看最直接的写法，感受其中的问题：



```
using UnityEngine;

// 接收者：角色（实际执行跳跃的对象）

public class Player : MonoBehaviour

{

&#x20;   public void Jump()

&#x20;   {

&#x20;       Debug.Log("角色跳跃！");

&#x20;       transform.Translate(Vector3.up \* 2f); // 简单模拟跳跃

&#x20;   }

}

// 调用者：输入管理器（触发操作的对象）

public class InputManager : MonoBehaviour

{

&#x20;   private Player \_player;

&#x20;   private void Awake()

&#x20;   {

&#x20;       \_player = FindObjectOfType\<Player>();

&#x20;   }

&#x20;   private void Update()

&#x20;   {

&#x20;       // 按下空格，直接调用角色的Jump方法

&#x20;       if (Input.GetKeyDown(KeyCode.Space))

&#x20;       {

&#x20;           \_player.Jump();

&#x20;       }

&#x20;   }

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



```
using UnityEngine;

using System.Collections.Generic;

/// \<summary>

/// 命令接口：所有命令的统一标准（执行+撤销）

/// \</summary>

public interface ICommand

{

&#x20;   void Execute(); // 执行命令

&#x20;   void Undo();    // 撤销命令

}
```

### 2. 接收者：角色（实际执行操作）



```
/// \<summary>

/// 角色（接收者）：包含具体操作和撤销的回滚逻辑

/// \</summary>

public class Player : MonoBehaviour

{

&#x20;   private Vector3 \_jumpOriginPos; // 记录跳跃前位置（用于撤销）

&#x20;   // 跳跃操作

&#x20;   public void Jump()

&#x20;   {

&#x20;       \_jumpOriginPos = transform.position; // 记录原始位置

&#x20;       transform.Translate(Vector3.up \* 2f);

&#x20;       Debug.Log(\$"角色跳跃：位置→{transform.position}");

&#x20;   }

&#x20;   // 撤销跳跃（回滚逻辑）

&#x20;   public void UndoJump()

&#x20;   {

&#x20;       transform.position = \_jumpOriginPos;

&#x20;       Debug.Log(\$"撤销跳跃：位置→{transform.position}");

&#x20;   }

&#x20;   // 攻击操作

&#x20;   public void Attack()

&#x20;   {

&#x20;       Debug.Log("角色发起攻击！");

&#x20;   }

&#x20;   // 撤销攻击

&#x20;   public void UndoAttack()

&#x20;   {

&#x20;       Debug.Log("撤销攻击：取消攻击效果");

&#x20;   }

}
```

### 3. 具体命令类（封装操作）



```
/// \<summary>

/// 跳跃命令：绑定角色和跳跃操作

/// \</summary>

public class JumpCommand : ICommand

{

&#x20;   private readonly Player \_player; // 接收者引用

&#x20;   public JumpCommand(Player player)

&#x20;   {

&#x20;       \_player = player;

&#x20;   }

&#x20;   public void Execute() => \_player.Jump();

&#x20;   public void Undo() => \_player.UndoJump();

}

/// \<summary>

/// 攻击命令：绑定角色和攻击操作

/// \</summary>

public class AttackCommand : ICommand

{

&#x20;   private readonly Player \_player;

&#x20;   public AttackCommand(Player player)

&#x20;   {

&#x20;       \_player = player;

&#x20;   }

&#x20;   public void Execute() => \_player.Attack();

&#x20;   public void Undo() => \_player.UndoAttack();

}
```

### 4. 命令管理器（核心：命令栈，管理撤销 / 重做）



```
/// \<summary>

/// 命令管理器（实战必备）：用栈管理命令，支持执行、撤销、重做

/// \</summary>

public class CommandManager : MonoBehaviour

{

&#x20;   // 单例：全局唯一（Unity游戏开发常用）

&#x20;   public static CommandManager Instance { get; private set; }

&#x20;   // 执行栈：存储已执行的命令（用于撤销）

&#x20;   private readonly Stack\<ICommand> \_executeStack = new Stack\<ICommand>();

&#x20;   // 重做栈：存储被撤销的命令（用于重做）

&#x20;   private readonly Stack\<ICommand> \_redoStack = new Stack\<ICommand>();

&#x20;   private void Awake()

&#x20;   {

&#x20;       // 单例初始化（避免多个管理器）

&#x20;       if (Instance == null)

&#x20;       {

&#x20;           Instance = this;

&#x20;           DontDestroyOnLoad(gameObject); // 场景切换不销毁

&#x20;       }

&#x20;       else

&#x20;       {

&#x20;           Destroy(gameObject);

&#x20;       }

&#x20;   }

&#x20;   /// \<summary>

&#x20;   /// 执行命令（核心方法）

&#x20;   /// \</summary>

&#x20;   public void ExecuteCommand(ICommand command)

&#x20;   {

&#x20;       command.Execute();

&#x20;       \_executeStack.Push(command); // 压入执行栈

&#x20;       \_redoStack.Clear(); // 执行新命令后，清空重做栈（符合用户直觉）

&#x20;   }

&#x20;   /// \<summary>

&#x20;   /// 撤销上一个命令

&#x20;   /// \</summary>

&#x20;   public void UndoLast()

&#x20;   {

&#x20;       if (\_executeStack.Count == 0)

&#x20;       {

&#x20;           Debug.LogWarning("无命令可撤销！");

&#x20;           return;

&#x20;       }

&#x20;       var cmd = \_executeStack.Pop();

&#x20;       cmd.Undo();

&#x20;       \_redoStack.Push(cmd); // 压入重做栈

&#x20;   }

&#x20;   /// \<summary>

&#x20;   /// 重做上一个被撤销的命令

&#x20;   /// \</summary>

&#x20;   public void RedoLast()

&#x20;   {

&#x20;       if (\_redoStack.Count == 0)

&#x20;       {

&#x20;           Debug.LogWarning("无命令可重做！");

&#x20;           return;

&#x20;       }

&#x20;       var cmd = \_redoStack.Pop();

&#x20;       cmd.Execute();

&#x20;       \_executeStack.Push(cmd);

&#x20;   }

}
```

### 5. 调用者：输入管理器（仅检测输入，解耦）



```
/// \<summary>

/// 输入管理器（调用者）：仅检测输入，不关心具体操作

/// \</summary>

public class InputManager : MonoBehaviour

{

&#x20;   private Player \_player;

&#x20;   private void Awake()

&#x20;   {

&#x20;       \_player = FindObjectOfType\<Player>();

&#x20;   }

&#x20;   private void Update()

&#x20;   {

&#x20;       // 空格：执行跳跃命令

&#x20;       if (Input.GetKeyDown(KeyCode.Space))

&#x20;       {

&#x20;           CommandManager.Instance.ExecuteCommand(new JumpCommand(\_player));

&#x20;       }

&#x20;       // 鼠标左键：执行攻击命令

&#x20;       if (Input.GetMouseButtonDown(0))

&#x20;       {

&#x20;           CommandManager.Instance.ExecuteCommand(new AttackCommand(\_player));

&#x20;       }

&#x20;       // Z键：撤销

&#x20;       if (Input.GetKeyDown(KeyCode.Z))

&#x20;       {

&#x20;           CommandManager.Instance.UndoLast();

&#x20;       }

&#x20;       // R键：重做

&#x20;       if (Input.GetKeyDown(KeyCode.R))

&#x20;       {

&#x20;           CommandManager.Instance.RedoLast();

&#x20;       }

&#x20;   }

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

### 2. 输入与逻辑解耦



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

4. **适用场景**：操作撤销、输入解耦、技能系统、连招系统等。

命令模式并不是 “多余的复杂代码”，而是工业级项目中解决特定问题的标准方案。新手可以先从极简版代码入手，理解核心逻辑后，再根据项目需求扩展功能（如宏命令、命令序列化）。

希望本文能帮助你彻底理解命令模式，并能在 Unity 项目中灵活应用！

> （注：文档部分内容可能由 AI 生成）