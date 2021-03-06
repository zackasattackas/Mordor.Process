﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;

namespace Mordor.Process.Linq.IQToolkit
{
    public class ScopedDictionary<TKey, TValue>
    {
        private readonly ScopedDictionary<TKey, TValue> _previous;
        private readonly Dictionary<TKey, TValue> _map;

        public ScopedDictionary(ScopedDictionary<TKey, TValue> previous)
        {
            _previous = previous;
            _map = new Dictionary<TKey, TValue>();
        }

        public ScopedDictionary(ScopedDictionary<TKey, TValue> previous, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
            : this(previous)
        {
            foreach (var p in pairs)
            {
                _map.Add(p.Key, p.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            _map.Add(key, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (var scope = this; scope != null; scope = scope._previous)
            {
                if (scope._map.TryGetValue(key, out value))
                    return true;
            }
            value = default;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            for (var scope = this; scope != null; scope = scope._previous)
            {
                if (scope._map.ContainsKey(key))
                    return true;
            }
            return false;
        }
    }
}