// Licensed to .NET HTTP Abstractions (the "Project") under one
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
using System.Web.SessionState;

namespace AspNet.Taco {
    class Session : IDictionary<string, object> {
        readonly HttpSessionState _session;

        public Session(HttpSessionState session) {
            _session = session;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _session.Keys.OfType<string>().Select(key => new KeyValuePair<string, object>(key, _session[key])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item) {
            _session.Add(item.Key, item.Value);
        }

        public void Clear() {
            _session.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item) {
            return _session[item.Key] == item.Value;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item) {
            if (!Contains(item))
                return false;

            Remove(item.Key);
            return true;
        }

        public int Count {
            get { return _session.Keys.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool ContainsKey(string key) {
            return _session[key] != null;
        }

        public void Add(string key, object value) {
            _session.Add(key, value);
        }

        public bool Remove(string key) {
            if (!ContainsKey(key))
                return false;

            _session.Remove(key);
            return true;
        }

        public bool TryGetValue(string key, out object value) {
            value = _session[key];
            return value != null;
        }

        public object this[string key] {
            get { return _session[key]; }
            set { _session[key] = value; }
        }

        public ICollection<string> Keys {
            get { return _session.Keys.OfType<string>().ToList(); }
        }

        public ICollection<object> Values {
            get { return _session.Keys.OfType<string>().Select(key => _session[key]).ToList(); }
        }
    }
}