using System.Collections.Concurrent;
using Microsoft.Maui.ApplicationModel;

namespace Prism.Events
{
    /// <summary>
    /// How a subscriber callback is marshalled when the event is published.
    /// </summary>
    public enum ThreadOption
    {
        PublisherThread,
        UIThread,
        BackgroundThread
    }

    /// <summary>
    /// Opaque handle returned from Subscribe. Not strictly required by the
    /// ported code but kept for API compatibility.
    /// </summary>
    public class SubscriptionToken
    {
        internal Delegate Handler { get; }
        internal SubscriptionToken(Delegate handler) => Handler = handler;
    }

    public abstract class EventBase { }

    /// <summary>
    /// Parameterless pub/sub event.
    /// </summary>
    public class PubSubEvent : EventBase
    {
        private readonly List<(Action action, ThreadOption option)> _subscribers = new();
        private readonly object _lock = new();

        public SubscriptionToken Subscribe(Action action, ThreadOption option = ThreadOption.PublisherThread)
        {
            lock (_lock)
            {
                _subscribers.Add((action, option));
            }
            return new SubscriptionToken(action);
        }

        public void Publish()
        {
            List<(Action action, ThreadOption option)> snapshot;
            lock (_lock)
            {
                snapshot = new List<(Action, ThreadOption)>(_subscribers);
            }

            foreach (var (action, option) in snapshot)
                Dispatch(() => action(), option);
        }

        internal static void Dispatch(Action callback, ThreadOption option)
        {
            if (option == ThreadOption.UIThread && !MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(callback);
            else
                callback();
        }
    }

    /// <summary>
    /// Pub/sub event carrying a payload.
    /// </summary>
    public class PubSubEvent<TPayload> : EventBase
    {
        private readonly List<(Action<TPayload> action, ThreadOption option)> _subscribers = new();
        private readonly object _lock = new();

        public SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption option = ThreadOption.PublisherThread)
        {
            lock (_lock)
            {
                _subscribers.Add((action, option));
            }
            return new SubscriptionToken(action);
        }

        public void Publish(TPayload payload)
        {
            List<(Action<TPayload> action, ThreadOption option)> snapshot;
            lock (_lock)
            {
                snapshot = new List<(Action<TPayload>, ThreadOption)>(_subscribers);
            }

            foreach (var (action, option) in snapshot)
                PubSubEvent.Dispatch(() => action(payload), option);
        }
    }

    public interface IEventAggregator
    {
        TEventType GetEvent<TEventType>() where TEventType : EventBase, new();
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, EventBase> _events = new();

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            return (TEventType)_events.GetOrAdd(typeof(TEventType), _ => new TEventType());
        }
    }
}
