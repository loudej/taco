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

namespace Taco {
    public class Cargo {
        readonly Delivery _delivery;

        protected Cargo() {}

        protected Cargo(Delivery delivery) {
            _delivery = delivery;
        }

        protected Cargo(Cargo cargo) {
            _delivery = cargo._delivery;
        }

        public static Cargo<T> From<T>(T result) {
            return new Cargo<T>(result);
        }

        public static Cargo<T> From<T>(T result, Action continuation) {
            return new Cargo<T>(result, continuation);
        }

        public bool Delayable {
            get { return _delivery != null; }
        }

        public bool Delayed {
            get { return _delivery != null && _delivery.Delayed; }
        }

        public void Delay() {
            _delivery.Delayed = true;
        }

        public void Resume() {
            _delivery.Delayed = true;
            _delivery.Continuation();
        }

        protected class Delivery {
            public Delivery(Action continuation) {
                Continuation = continuation;
            }

            public bool Delayed { get; set; }
            public Action Continuation { get; private set; }
        }
    }

    public class Cargo<T> : Cargo {
        readonly T _result;

        public Cargo(T result) {
            _result = result;
        }

        public Cargo(T result, Action continuation)
            : base(new Delivery(continuation)) {
            _result = result;
        }

        Cargo(T result, Cargo cargo)
            : base(cargo) {
            _result = result;
        }

        //public static implicit operator Cargo<T>(T result) {
        //    return new Cargo<T>(result);
        //}

        public T Result {
            get { return _result; }
        }

        public Cargo<T2> Alter<T2>(T2 result) {
            return new Cargo<T2>(result, this);
        }
    }
}