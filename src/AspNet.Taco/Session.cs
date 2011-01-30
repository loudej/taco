// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
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