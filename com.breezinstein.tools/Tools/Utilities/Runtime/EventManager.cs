using System;
using System.Collections.Generic;

/// <summary>
/// A generic event manager class that allows registering, unregistering, and triggering events.
/// </summary>
/// <typeparam name="T">The type of the event payload.</typeparam>
public static class EventManager<T>
{
    private static Dictionary<string, Action<T>> eventDictionary = new Dictionary<string, Action<T>>();

    /// <summary>
    /// Registers a method to be called when the specified event is triggered.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="method">The method to be called.</param>
    public static void Register(string eventName, Action<T> method)
    {
        if (!eventDictionary.TryGetValue(eventName, out var eventActions))
        {
            eventActions = null;
            eventDictionary[eventName] = eventActions;
        }
        eventDictionary[eventName] += method;
    }

    /// <summary>
    /// Unregisters a method from being called when the specified event is triggered.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="method">The method to be unregistered.</param>
    public static void Unregister(string eventName, Action<T> method)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= method;
        }
    }

    /// <summary>
    /// Triggers the specified event with the given payload.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="payload">The payload to be passed to the event handlers.</param>
    public static void Trigger(string eventName, T payload)
    {
        Action<T> thisEvent;
        if (eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent?.Invoke(payload);
        }
    }
}
