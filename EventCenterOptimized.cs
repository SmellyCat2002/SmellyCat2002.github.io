using System.Collections.Generic;
using UnityEngine.Events;

// 定义一个事件信息接口，所有具体的事件信息类都需要实现这个接口
public interface IEventInfo
{
    // 添加接口方法以便子类必须实现移除特定目标的功能
    void RemoveTarget(object target);
}



// 泛型版本的事件信息类，用于处理带参数的事件
public class EventInfo<T> : IEventInfo
{
    // 存储事件触发时需要执行的动作（委托用）
    public UnityAction<T> actions;
    
    // 存储针对特定目标的事件回调（关键改进：添加目标绑定）
    public Dictionary<object, UnityAction<T>> targetActions = new Dictionary<object, UnityAction<T>>();

    // 构造函数，允许在创建事件时就绑定动作
    public EventInfo(UnityAction<T> action = null)
    {
        actions = action;
    }

    // 实现接口方法：移除特定目标的事件回调
    public void RemoveTarget(object target)
    {
        if (targetActions.ContainsKey(target))
        {
            targetActions.Remove(target);
        }
    }
}


// 双泛型版本的事件信息类，用于处理带两个参数的事件
public class EventInfo<T1, T2> : IEventInfo
{
    // 存储事件触发时需要执行的动作
    public UnityAction<T1, T2> actions;
    
    // 存储针对特定目标的事件回调
    public Dictionary<object, UnityAction<T1, T2>> targetActions = new Dictionary<object, UnityAction<T1, T2>>();

    // 构造函数，允许在创建事件时就绑定动作
    public EventInfo(UnityAction<T1, T2> action = null)
    {
        actions = action;
    }

    // 实现接口方法：移除特定目标的事件回调
    public void RemoveTarget(object target)
    {
        if (targetActions.ContainsKey(target))
        {
            targetActions.Remove(target);
        }
    }
}


// 无参数版本的事件信息类，用于处理不带参数的事件
public class EventInfo : IEventInfo
{
    // 存储事件触发时需要执行的动作
    public UnityAction actions;
    
    // 存储针对特定目标的事件回调
    public Dictionary<object, UnityAction> targetActions = new Dictionary<object, UnityAction>();

    // 构造函数，允许在创建事件时就绑定动作
    public EventInfo(UnityAction action = null)
    {
        actions = action;
    }

    // 实现接口方法：移除特定目标的事件回调
    public void RemoveTarget(object target)
    {
        if (targetActions.ContainsKey(target))
        {
            targetActions.Remove(target);
        }
    }
}


/// <summary>
/// 事件中心，作为单例管理系统中的事件订阅与发布
/// 使用Dictionary存储事件名称和对应的事件信息对象
/// 通过泛型机制实现事件的订阅和发布
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    // 存储事件名称和对应事件信息的字典
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// 添加事件监听器，支持带一个参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        // 检查是否已有相同名称的事件
        if( eventDic.ContainsKey(name) )
        {
            // 如果存在，则向现有事件添加新的监听器
            (eventDic[name] as EventInfo<T>).actions += action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo<T>( action ));
        }
    }

    /// <summary>
    /// 添加事件监听器，支持带两个参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddEventListener<T1, T2>(string name, UnityAction<T1, T2> action)
    {
        // 检查是否已有相同名称的事件
        if( eventDic.ContainsKey(name) )
        {
            // 如果存在，则向现有事件添加新的监听器
            (eventDic[name] as EventInfo<T1, T2>).actions += action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo<T1, T2>( action ));
        }
    }

    /// <summary>
    /// 添加目标绑定的事件监听器，支持带一个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddTargetedEventListener<T>(string name, object target, UnityAction<T> action)
    {
        // 检查是否已有相同名称的事件
        if( eventDic.ContainsKey(name) )
        {
            // 如果存在，则向该目标的事件列表添加新的监听器
            (eventDic[name] as EventInfo<T>).targetActions[target] = action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo<T>(null));
            (eventDic[name] as EventInfo<T>).targetActions[target] = action;
        }
    }

    /// <summary>
    /// 添加目标绑定的事件监听器，支持带两个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddTargetedEventListener<T1, T2>(string name, object target, UnityAction<T1, T2> action)
    {
        // 检查是否已有相同名称的事件
        if( eventDic.ContainsKey(name) )
        {
            // 如果存在，则向该目标的事件列表添加新的监听器
            (eventDic[name] as EventInfo<T1, T2>).targetActions[target] = action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo<T1, T2>(null));
            (eventDic[name] as EventInfo<T1, T2>).targetActions[target] = action;
        }
    }

    /// <summary>
    /// 添加事件监听器，适用于无参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddEventListener(string name, UnityAction action)
    {
        // 检查是否已有相同名称的事件
        if (eventDic.ContainsKey(name))
        {
            // 如果存在，则向现有事件添加新的监听器
            (eventDic[name] as EventInfo).actions += action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo(action));
        }
    }

    /// <summary>
    /// 添加目标绑定的事件监听器，适用于无参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    /// <param name="action">事件触发时执行的委托</param>
    public void AddTargetedEventListener(string name, object target, UnityAction action)
    {
        // 检查是否已有相同名称的事件
        if (eventDic.ContainsKey(name))
        {
            // 如果存在，则向该目标的事件列表添加新的监听器
            (eventDic[name] as EventInfo).targetActions[target] = action;
        }
        else
        {
            // 否则，创建新的事件并添加到字典中
            eventDic.Add(name, new EventInfo(null));
            (eventDic[name] as EventInfo).targetActions[target] = action;
        }
    }

    /// <summary>
    /// 移除目标绑定的事件监听器，支持带一个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    public void RemoveTargetedEventListener<T>(string name, object target)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).RemoveTarget(target);
        }
    }

    /// <summary>
    /// 移除目标绑定的事件监听器，支持带两个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    public void RemoveTargetedEventListener<T1, T2>(string name, object target)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T1, T2>).RemoveTarget(target);
        }
    }

    /// <summary>
    /// 移除目标绑定的事件监听器，适用于无参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    public void RemoveTargetedEventListener(string name, object target)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).RemoveTarget(target);
        }
    }

    /// <summary>
    /// 移除目标的所有事件监听器
    /// </summary>
    /// <param name="target">目标对象</param>
    public void RemoveAllTargetedListeners(object target)
    {
        foreach (var eventInfo in eventDic.Values)
        {
            eventInfo.RemoveTarget(target);
        }
    }

    /// <summary>
    /// 触发带一个参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="info">传递给事件处理函数的信息</param>
    public void EventTrigger<T>(string name, T info)
    {
        if (eventDic.ContainsKey(name))
        {
            if((eventDic[name] as EventInfo<T>).actions != null)
            {
                (eventDic[name] as EventInfo<T>).actions.Invoke(info); // 执行所有注册的事件处理函数
            }
        }
    }

    /// <summary>
    /// 触发带两个参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="info1">传递给事件处理函数的第一个参数</param>
    /// <param name="info2">传递给事件处理函数的第二个参数</param>
    public void EventTrigger<T1, T2>(string name, T1 info1, T2 info2)
    {
        if (eventDic.ContainsKey(name))
        {
            if((eventDic[name] as EventInfo<T1, T2>).actions != null)
            {
                (eventDic[name] as EventInfo<T1, T2>).actions.Invoke(info1, info2); // 执行所有注册的事件处理函数
            }
        }
    }

    /// <summary>
    /// 触发特定目标的带一个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    /// <param name="info">传递给事件处理函数的信息</param>
    public void EventTriggerToTarget<T>(string name, object target, T info)
    {
        if (eventDic.ContainsKey(name))
        {
            EventInfo<T> eventInfo = eventDic[name] as EventInfo<T>;
            if (eventInfo.targetActions.ContainsKey(target) && eventInfo.targetActions[target] != null)
            {
                eventInfo.targetActions[target].Invoke(info);
            }
            // 直接调用特定目标的回调函数，避免广播和循环
        }
    }

    /// <summary>
    /// 触发特定目标的带两个参数的事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    /// <param name="info1">传递给事件处理函数的第一个参数</param>
    /// <param name="info2">传递给事件处理函数的第二个参数</param>
    public void EventTriggerToTarget<T1, T2>(string name, object target, T1 info1, T2 info2)
    {
        if (eventDic.ContainsKey(name))
        {
            EventInfo<T1, T2> eventInfo = eventDic[name] as EventInfo<T1, T2>;
            if (eventInfo.targetActions.ContainsKey(target) && eventInfo.targetActions[target] != null)
            {
                eventInfo.targetActions[target].Invoke(info1, info2);
            }
            // 直接调用特定目标的回调函数，避免广播和循环
        }
    }

    /// <summary>
    /// 触发无参数的事件（广播用）
    /// </summary>
    /// <param name="name">事件名称</param>
    public void EventTrigger(string name)
    {
        if (eventDic.ContainsKey(name))
        {
            if((eventDic[name] as EventInfo).actions != null)
            {
                (eventDic[name] as EventInfo).actions.Invoke(); // 执行所有注册的事件处理函数
            }
        }
    }

    /// <summary>
    /// 触发特定目标的无参数事件
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="target">目标对象</param>
    public void EventTriggerToTarget(string name, object target)
    {
        if (eventDic.ContainsKey(name))
        {
            EventInfo eventInfo = eventDic[name] as EventInfo;
            if (eventInfo.targetActions.ContainsKey(target) && eventInfo.targetActions[target] != null)
            {
                eventInfo.targetActions[target].Invoke();
            }
            // 直接调用特定目标的回调函数，避免广播和循环
        }
    }

    /// <summary>
    /// 清空所有事件监听器，通常在场景切换时调用
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}