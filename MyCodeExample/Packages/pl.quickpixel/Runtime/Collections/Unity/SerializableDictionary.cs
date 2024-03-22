using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPixel.Collections.Unity
{
    /// <summary>
    ///     SerializableDictionary is a dictionary that can be serialized by Unity.
    ///     It inherits from the standard Dictionary class and implements ISerializationCallbackReceiver
    ///     to enable proper serialization/deserialization, however is not drawn in inspector!
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    [Serializable]
    public sealed class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [field: SerializeField] [field: HideInInspector]
        private SerializedEntry[] serializedEntries = Array.Empty<SerializedEntry>();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var serializationHelper = new List<SerializedEntry>();
            foreach (var assetsMap in this)
                serializationHelper.Add(new SerializedEntry { Key = assetsMap.Key, Value = assetsMap.Value });

            serializedEntries = serializationHelper.Count > 0
                ? serializationHelper.ToArray()
                : Array.Empty<SerializedEntry>();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (var entry in serializedEntries) Add(entry.Key, entry.Value);

            serializedEntries = Array.Empty<SerializedEntry>();
        }

        [Serializable]
        private struct SerializedEntry
        {
            public TKey Key;
            public TValue Value;
        }
    }
}