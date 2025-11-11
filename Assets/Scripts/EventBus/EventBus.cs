using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, Delegate> subscriptions = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (subscriptions.ContainsKey(eventType))
            subscriptions[eventType] = Delegate.Combine(subscriptions[eventType], listener);
        else
            subscriptions[eventType] = listener;
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (subscriptions.ContainsKey(eventType))
            subscriptions[eventType] = Delegate.Remove(subscriptions[eventType], listener);
    }

    public static void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);
        if (subscriptions.TryGetValue(eventType, out Delegate d))
            (d as Action<T>)?.Invoke(eventData);
    }
}