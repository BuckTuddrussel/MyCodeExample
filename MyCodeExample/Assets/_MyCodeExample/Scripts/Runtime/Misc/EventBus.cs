using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MyCodeExample
{
    /// <summary>
    /// Simple event bus just for this task, in real world we would like to make something more efficient
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, Bucket> _map = new Dictionary<Type, Bucket>();

        public void Publish<T>(T sender) where T : IEvent
        {
            if (_map.TryGetValue(sender.GetType(), out var bucket))
            {
                bucket.Listeners?.Invoke(sender);
            }
        }
        
        public Guid Register<T>(Action<T> listener) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (!_map.TryGetValue(eventType, out Bucket bucket))
            {
                bucket = new Bucket();
                _map.Add(eventType, bucket);
            }

            var listenerGuid = Guid.NewGuid();
            Action<IEvent> wrapper = @event => listener((T)@event);
           
            bucket.Listeners += wrapper;
            bucket.CachedDelegates.Add(listenerGuid, wrapper);

            return listenerGuid;
        }

        public Guid RegisterListenOnce<T>(Action<T> listener) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (!_map.TryGetValue(eventType, out Bucket bucket))
            {
                bucket = new Bucket();
                _map.Add(eventType, bucket);
            }

            var listenerGuid = Guid.NewGuid();
            Action<IEvent> wrapper = @event =>
            {
                listener((T)@event);
                Unregister<T>(listenerGuid);
            };
           
            bucket.Listeners += wrapper;
            bucket.CachedDelegates.Add(listenerGuid, wrapper);

            return listenerGuid;
        }
        
        public void Unregister<T>(Guid guid) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (_map.TryGetValue(eventType, out Bucket bucket) &&
                bucket.CachedDelegates.TryGetValue(guid, out var listener))
            {
                bucket.Listeners -= listener;
                bucket.CachedDelegates.Remove(guid);
                if (bucket.CachedDelegates.Count == 0)
                {
                    _map.Remove(eventType);
                }
            }
        }

        private class Bucket
        {
            public readonly Dictionary<Guid, Action<IEvent>> CachedDelegates = new Dictionary<Guid, Action<IEvent>>();
            public Action<IEvent> Listeners;
        }

        private static bool IsAnonymousDelegate(Delegate del)
        {
            return del.Method.IsDefined(typeof(CompilerGeneratedAttribute), false);
        }
        
        public interface IEvent
        {
        }
    }
}