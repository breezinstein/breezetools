using System;
using System.Collections.Generic;

public static class EventManager<T>
{
    private static Dictionary<string, Action<T>> eventDictionary = new Dictionary<string, Action<T>>();

    public static void Register(string eventName, Action<T> method)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary.Add(eventName, null);
        }
        eventDictionary[eventName] += method;
    }

    public static void Unregister(string eventName, Action<T> method)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= method;
        }
    }

    public static void Trigger(string eventName, T payload)
    {
        Action<T> thisEvent;
        if (eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent?.Invoke(payload);
        }
    }
}
