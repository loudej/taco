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
using System.Text;
using Taco;

namespace AspNet.Taco {
    public class ResponseBody {
        readonly Encoding _encoding;
        readonly Action<byte[], int, int> _write;
        readonly Func<byte[], int, int, AsyncCallback, object, IAsyncResult> _beginWrite;
        readonly Action<IAsyncResult> _endWrite;

        public ResponseBody(
            Encoding encoding,
            Action<byte[], int, int> write,
            Func<byte[], int, int, AsyncCallback, object, IAsyncResult> beginWrite,
            Action<IAsyncResult> endWrite) {
            _encoding = encoding;
            _write = write;
            _beginWrite = beginWrite;
            _endWrite = endWrite;
        }


        public void Write(Cargo<ArraySegment<byte>> cargo) {
            if (!cargo.Delayable) {
                _write(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count);
                return;
            }

            var result = _beginWrite(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count, asyncResult => {
                if (asyncResult.CompletedSynchronously)
                    return;
                _endWrite(asyncResult);
                cargo.Resume();
            }, null);

            if (result.CompletedSynchronously)
                _endWrite(result);
            else
                cargo.Delay();
        }
    }
}