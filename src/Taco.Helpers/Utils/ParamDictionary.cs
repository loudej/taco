﻿// Licensed to .NET HTTP Abstractions (the "Project") under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The Project licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//  
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Taco.Helpers.Utils {
    public class ParamDictionary : IDictionary<string, string> {
        public static IDictionary<string, string> Parse(string queryString) {
            // TODO: this is wrong in many, many ways
            var d = queryString.Split("&".ToCharArray())
                .Select(item => item.Split("=".ToCharArray(), 2))
                .Where(item => item.Length == 2)
                .GroupBy(item => item[0], item => Decode(item[1]), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => string.Join(",", g.ToArray()), StringComparer.OrdinalIgnoreCase);

            return new ParamDictionary(d);
        }

        static string Decode(string value) {
            return value.Replace("%3A", ":").Replace("%2F", "/");
        }

        readonly IDictionary<string, string> _impl;

        ParamDictionary(IDictionary<string, string> impl) {
            _impl = impl;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() {
            return _impl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _impl.GetEnumerator();
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) {
            _impl.Add(item);
        }

        void ICollection<KeyValuePair<string, string>>.Clear() {
            _impl.Clear();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) {
            return _impl.Contains(item);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
            _impl.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) {
            return _impl.Remove(item);
        }

        int ICollection<KeyValuePair<string, string>>.Count {
            get { return _impl.Count; }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly {
            get { return _impl.IsReadOnly; }
        }

        bool IDictionary<string, string>.ContainsKey(string key) {
            return _impl.ContainsKey(key);
        }

        void IDictionary<string, string>.Add(string key, string value) {
            _impl.Add(key, value);
        }

        bool IDictionary<string, string>.Remove(string key) {
            return _impl.Remove(key);
        }

        bool IDictionary<string, string>.TryGetValue(string key, out string value) {
            return _impl.TryGetValue(key, out value);
        }

        string IDictionary<string, string>.this[string key] {
            get {
                string value;
                return _impl.TryGetValue(key, out value) ? value : default(string);
            }
            set { _impl[key] = value; }
        }

        ICollection<string> IDictionary<string, string>.Keys {
            get { return _impl.Keys; }
        }

        ICollection<string> IDictionary<string, string>.Values {
            get { return _impl.Values; }
        }
    }
}