using UnityEngine;
using UnityEngine.Events;

// 示例脚本，演示EventCenter的使用
public class EventCenterExample : MonoBehaviour
{
    // 定义事件名称常量
    private const string EVENT_TEST_NO_PARAM = "TestNoParam";
    private const string EVENT_TEST_ONE_PARAM = "TestOneParam";
    private const string EVENT_TEST_TWO_PARAM = "TestTwoParam";
    
    // 测试对象A和B
    private GameObject testObjectA;
    private GameObject testObjectB;
    
    void Start()
    {
        // 初始化测试对象
        testObjectA = new GameObject("TestObjectA");
        testObjectB = new GameObject("TestObjectB");
        
        Debug.Log("=== EventCenter使用示例 ===");
        
        // 1. 测试无参数事件
        TestNoParamEvents();

        // 2. 测试带一个参数的事件
        TestOneParamEvents();

        // 3. 测试带两个参数的事件
        TestTwoParamEvents();

        // 4. 测试移除目标的所有事件监听器
        TestRemoveAllListeners();
    }

    // 测试无参数事件
    void TestNoParamEvents()
    {
        Debug.Log("\n1. 测试无参数事件：");
        
        // 为对象A添加无参数事件监听器
        UnityAction actionA = () => { Debug.Log("对象A接收到无参数事件"); };
        EventCenter.Instance.AddTargetedEventListener(EVENT_TEST_NO_PARAM, testObjectA, actionA);
        
        // 为对象B添加无参数事件监听器
        UnityAction actionB = () => { Debug.Log("对象B接收到无参数事件"); };
        EventCenter.Instance.AddTargetedEventListener(EVENT_TEST_NO_PARAM, testObjectB, actionB);
        
        // 触发对象A的事件
        Debug.Log("触发对象A的无参数事件：");
        EventCenter.Instance.EventTriggerToTarget(EVENT_TEST_NO_PARAM, testObjectA);
        
        // 触发对象B的事件
        Debug.Log("触发对象B的无参数事件：");
        EventCenter.Instance.EventTriggerToTarget(EVENT_TEST_NO_PARAM, testObjectB);
        
        // 移除对象A的无参数事件监听器
        EventCenter.Instance.RemoveTargetedEventListener(EVENT_TEST_NO_PARAM, testObjectA);
        
        // 再次触发对象A的事件（应该不会触发）
        Debug.Log("再次触发对象A的无参数事件（应该不会触发）：");
        EventCenter.Instance.EventTriggerToTarget(EVENT_TEST_NO_PARAM, testObjectA);
        
        // 触发对象B的事件（应该正常触发）
        Debug.Log("再次触发对象B的无参数事件（应该正常触发）：");
        EventCenter.Instance.EventTriggerToTarget(EVENT_TEST_NO_PARAM, testObjectB);
    }

    // 测试带一个参数的事件
    void TestOneParamEvents()
    {
        Debug.Log("\n2. 测试带一个参数的事件：");
        
        // 为对象A添加带参数事件监听器
        UnityAction<string> actionA = (message) => { Debug.Log($"对象A接收到带参数事件：{message}"); };
        EventCenter.Instance.AddTargetedEventListener<string>(EVENT_TEST_ONE_PARAM, testObjectA, actionA);
        
        // 为对象B添加带参数事件监听器
        UnityAction<string> actionB = (message) => { Debug.Log($"对象B接收到带参数事件：{message}"); };
        EventCenter.Instance.AddTargetedEventListener<string>(EVENT_TEST_ONE_PARAM, testObjectB, actionB);
        
        // 触发对象A的事件
        Debug.Log("触发对象A的带参数事件：");
        EventCenter.Instance.EventTriggerToTarget<string>(EVENT_TEST_ONE_PARAM, testObjectA, "Hello from A");
        
        // 触发对象B的事件
        Debug.Log("触发对象B的带参数事件：");
        EventCenter.Instance.EventTriggerToTarget<string>(EVENT_TEST_ONE_PARAM, testObjectB, "Hello from B");
    }

    // 测试带两个参数的事件
    void TestTwoParamEvents()
    {
        Debug.Log("\n3. 测试带两个参数的事件：");
        
        // 为对象A添加带两个参数的事件监听器
        UnityAction<int, string> actionA = (id, name) => { Debug.Log($"对象A接收到带两个参数事件：ID={id}, Name={name}"); };
        EventCenter.Instance.AddTargetedEventListener<int, string>(EVENT_TEST_TWO_PARAM, testObjectA, actionA);
        
        // 为对象B添加带两个参数的事件监听器
        UnityAction<int, string> actionB = (id, name) => { Debug.Log($"对象B接收到带两个参数事件：ID={id}, Name={name}"); };
        EventCenter.Instance.AddTargetedEventListener<int, string>(EVENT_TEST_TWO_PARAM, testObjectB, actionB);
        
        // 触发对象A的事件
        Debug.Log("触发对象A的带两个参数事件：");
        EventCenter.Instance.EventTriggerToTarget<int, string>(EVENT_TEST_TWO_PARAM, testObjectA, 1001, "ObjectA");
        
        // 触发对象B的事件
        Debug.Log("触发对象B的带两个参数事件：");
        EventCenter.Instance.EventTriggerToTarget<int, string>(EVENT_TEST_TWO_PARAM, testObjectB, 1002, "ObjectB");
    }

    // 测试移除目标的所有事件监听器
    void TestRemoveAllListeners()
    {
        Debug.Log("\n4. 测试移除目标的所有事件监听器：");
        
        // 移除对象B的所有事件监听器
        Debug.Log("移除对象B的所有事件监听器：");
        EventCenter.Instance.RemoveAllTargetedListeners(testObjectB);
        
        // 尝试触发对象B的所有类型事件（应该都不会触发）
        Debug.Log("尝试触发对象B的无参数事件（应该不会触发）：");
        EventCenter.Instance.EventTriggerToTarget(EVENT_TEST_NO_PARAM, testObjectB);
        
        Debug.Log("尝试触发对象B的带参数事件（应该不会触发）：");
        EventCenter.Instance.EventTriggerToTarget<string>(EVENT_TEST_ONE_PARAM, testObjectB, "This should not be received");
        
        Debug.Log("尝试触发对象B的带两个参数事件（应该不会触发）：");
        EventCenter.Instance.EventTriggerToTarget<int, string>(EVENT_TEST_TWO_PARAM, testObjectB, 9999, "This should not be received");
        
        // 对象A的事件应该仍然可以正常触发
        Debug.Log("触发对象A的带参数事件（应该正常触发）：");
        EventCenter.Instance.EventTriggerToTarget<string>(EVENT_TEST_ONE_PARAM, testObjectA, "Hello from A again");
    }
    
    void OnDestroy()
    {
        // 清理资源
        Destroy(testObjectA);
        Destroy(testObjectB);
        
        // 清空所有事件监听器
        EventCenter.Instance.Clear();
    }
}