using System.Diagnostics.CodeAnalysis;

namespace EventModule;

using Module;

public class EventModule : BaseModule
{
    private readonly Dictionary<Type, SortedList<int, List<WeakReference>>> _listeners = new();

    private readonly object _locker = new(); // 添加一个锁对象

    private readonly Dictionary<IEventListener, List<Tuple<Type, int, WeakReference>>> _reverseLookup = new();
    #region --- Build in Func ---

    private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return y.CompareTo(x);
        }
    }

    private void CleanUpListeners()
    {
        lock (_locker)
        {
            foreach (var priorityMap in _listeners.Values)
            {
                foreach (var listenerList in priorityMap.Values)
                {
                    listenerList.RemoveAll(weakRef => !weakRef.IsAlive);
                }
                var listenersToRemoveFromReverseLookup = new List<IEventListener>();
                foreach (var kvp in _reverseLookup)
                {
                    var listener = kvp.Key;
                    var registrations = kvp.Value;
                    registrations.RemoveAll(reg => !reg.Item3.IsAlive); // 清理失效的弱引用

                    if (registrations.Count == 0)
                    {
                        listenersToRemoveFromReverseLookup.Add(listener); // 如果该监听器没有任何有效注册，则将其从反向查找表中移除
                    }
                }

                foreach (var listener in listenersToRemoveFromReverseLookup)
                {
                    _reverseLookup.Remove(listener);
                }
            }
        }
    }

    private IEnumerable<IEventListener<T>> GetAliveListeners<T>() where T : IEvent
    {
        var type = typeof(T);
        // 深刻复制
        var priorityMapCopy = new Dictionary<int, List<WeakReference>>();

        lock (_locker)
        {
            if (_listeners.TryGetValue(type, out var priorityMap))
            {
                foreach (var kv in priorityMap)
                {
                    priorityMapCopy[kv.Key] = kv.Value.Select(w => new WeakReference(w.Target)).ToList();
                }
            }
        }

        foreach (var weakRefs in priorityMapCopy.Values)
        {
            foreach (var weakRef in weakRefs)
            {
                if (weakRef.Target is IEventListener<T> listener)
                {
                    yield return listener;
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 注册事件监听
    /// </summary>
    /// <param name="listener">事件监听</param>
    /// <param name="priority">事件优先级</param>
    /// <typeparam name="T">事件类型</typeparam>
    private void RegisterListener<T>(IEventListener<T> listener, int priority = 0) where T : IEvent
    {
        var type = typeof(T);
        lock (_locker)
        {


            if (!_listeners.TryGetValue(type, out var priorityMap))
            {
                priorityMap = new SortedList<int, List<WeakReference>>(new DescendingComparer<int>());
                _listeners.Add(type, priorityMap);
            }

            if (!priorityMap.TryGetValue(priority, out var listenerList))
            {
                listenerList = [];
                priorityMap.Add(priority, listenerList);
            }

            if (listenerList.Any(weakRef => weakRef.Target == listener))
            {
                return;
            }

            var weakRef = new WeakReference(listener);
            listenerList.Add(weakRef);

            // 更新反向查找表
            if (!_reverseLookup.TryGetValue(listener, out var registrations))
            {
                registrations = [];
                _reverseLookup.Add(listener, registrations);
            }
            registrations.Add(Tuple.Create(type, priority, weakRef));
        }
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="listener">事件监听</param>
    /// <param name="priority">事件优先级</param>
    /// <typeparam name="T">事件类型</typeparam>
    private void RemoveListener<T>(IEventListener<T> listener, int priority = 0) where T : IEvent
    {
        var type = typeof(T);

        lock (_locker)
        {
            if (_listeners.TryGetValue(type, out var priorityMap))
            {
                if (priorityMap.TryGetValue(priority, out var listenerList))
                {
                    // 只在指定的优先级列表中移除
                    listenerList.RemoveAll(weakRef => weakRef.Target == listener);


                    // 如果该优先级下没有其他监听器，则移除该优先级条目
                    if (listenerList.Count == 0)
                    {
                        priorityMap.Remove(priority);
                    }

                    // 如果该事件类型下没有其他优先级，则移除该事件类型条目
                    if (priorityMap.Count == 0)
                    {
                        _listeners.Remove(type);
                    }

                    // 更新反向查找表
                    if (_reverseLookup.TryGetValue(listener, out var registrations))
                    {
                        registrations.RemoveAll(reg => reg.Item1 == type && reg.Item2 == priority);
                        if (registrations.Count == 0)
                        {
                            _reverseLookup.Remove(listener);
                        }
                    }
                }
            }


        }
    }

    /// <summary>
    /// 移除所有指定事件类型的事件监听
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    private void RemoveAllListenersWithEvent<T>() where T : IEvent
    {
        var type = typeof(T);
        lock (_locker)
        {
            if (_listeners.Remove(type, out var priorityMap))
            {
                // 从 _listeners 中移除

                // 从 _reverseLookup 中移除对应的引用
                var listenersToUpdate = new HashSet<IEventListener>();
                foreach (var priorityList in priorityMap.Values)
                {
                    foreach (var weakRef in priorityList)
                    {
                        if (weakRef.Target is IEventListener listener)
                        {
                            listenersToUpdate.Add(listener);
                        }
                    }
                }

                foreach (var listener in listenersToUpdate)
                {
                    if (_reverseLookup.TryGetValue(listener, out var registrations))
                    {
                        registrations.RemoveAll(reg => reg.Item1 == type);
                        if (registrations.Count == 0)
                        {
                            _reverseLookup.Remove(listener);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 移除一个监听器的所有事件
    /// </summary>
    /// <param name="listener">事件监听</param>
    private void RemoveAllListenersWithListener(IEventListener listener)
    {
        lock (_locker)
        {
            if (_reverseLookup.TryGetValue(listener, out var registrations))
            {
                // 遍历所有注册信息并从主 _listeners 字典中移除
                foreach (var registration in registrations)
                {
                    var eventType = registration.Item1;
                    var priority = registration.Item2;
                    var weakRefToRemove = registration.Item3;

                    if (_listeners.TryGetValue(eventType, out var priorityMap) &&
                        priorityMap.TryGetValue(priority, out var listenerList))
                    {
                        // 找到并移除精确的弱引用实例
                        listenerList.RemoveAll(wr => ReferenceEquals(wr.Target, weakRefToRemove.Target));

                        // 清理空的优先级列表
                        if (listenerList.Count == 0)
                        {
                            priorityMap.Remove(priority);
                        }
                    }
                    // 清理空的事件类型列表
                    if (priorityMap != null && priorityMap.Count == 0)
                    {
                        _listeners.Remove(eventType);
                    }
                }
                // 从反向查找表中移除该监听器
                _reverseLookup.Remove(listener);
            }

        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="evt">事件</param>
    /// <param name="args">事件参数</param>
    /// <typeparam name="T">事件类型</typeparam>
    private void DispatchEvent<T>(T evt, params object[] args) where T : IEvent
    {
        foreach (var listener in GetAliveListeners<T>())
        {
            listener.OnEvent(evt, args);
        }
    }

    /// <summary>
    /// 异步触发事件
    /// </summary>
    /// <param name="evt">事件</param>
    /// <param name="args">事件参数</param>
    /// <typeparam name="T">事件类型</typeparam>
    private async Task DispatchEventAsync<T>(T evt, params object[] args) where T : IEvent
    {
        var tasks = new List<Task>();
        foreach (var listener in GetAliveListeners<T>())
        {
            tasks.Add(Task.Run(() => listener.OnEvent(evt, args)));
        }

        await Task.WhenAll(tasks);
    }

    #region --- Static Interface ---

    public static EventModule Instance => ModuleCenter.SGetModule<EventModule>();

    public static void SRegisterListener<T>(IEventListener<T> listener, int priority = 0) where T : IEvent
    {
        Instance.RegisterListener(listener, priority);
    }

    public static void SRemoveListener<T>(IEventListener<T> listener, int priority = 0) where T : IEvent
    {
        Instance.RemoveListener(listener, priority);
    }

    public static void SRemoveAllListenersWithEvent<T>() where T : IEvent
    {
        Instance.RemoveAllListenersWithEvent<T>();
    }

    public static void SRemoveAllListenersWithListener(IEventListener listener)
    {
        Instance.RemoveAllListenersWithListener(listener);
    }

    public static void SDispatchEvent<T>(T evt, params object[] args) where T : IEvent
    {
        Instance.DispatchEvent(evt, args);
    }

    public static Task SDispatchEventAsync<T>(T evt, params object[] args) where T : IEvent
    {
        return Instance.DispatchEventAsync(evt, args);
    }

    #endregion
}