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
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;

namespace Taco.Helpers.Utils {
    class FaultThunk : MarshalByRefObject, ILogicalThreadAffinative {
        readonly Action<Exception> _fault;

        FaultThunk(Action<Exception> fault) {
            _fault = fault;
        }

        protected FaultThunk(SerializationInfo info, StreamingContext context) {
            _fault = (Action<Exception>)Marshal.GetDelegateForFunctionPointer((IntPtr)info.GetValue("_fault", typeof(IntPtr)), typeof(Action<Exception>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("_fault", Marshal.GetFunctionPointerForDelegate(_fault), typeof(IntPtr));
        }

        void Fire(Exception ex) {
            _fault(ex);
        }

        public static Action<Exception> Current {
            get {
                var thunk = CallContext.GetData("taco.fault");
                if (thunk != null) return ex => ((FaultThunk)thunk).Fire(ex);
                return null;
            }
            set { CallContext.SetData("taco.fault", new FaultThunk(value)); }
        }

        public static IDisposable Scope(Action<Exception> ex) {
            var prior = Current;
            Current = ex;
            return new Disposable(() => Current = prior);
        }
    }
}