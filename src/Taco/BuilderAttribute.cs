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
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BuilderAttribute : Attribute {
        public BuilderAttribute(string componentName, Type type, string methodName) {
            ComponentName = componentName;
            Type = type;
            MethodName = methodName;
        }

        public string ComponentName { get; private set; }
        public Type Type { get; private set; }
        public string MethodName { get; private set; }
    }
}