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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentAsync {
    public static class Observable {
        public static IObservable<T> Create<T>(IEnumerable<T> enumerable) {
            return new SyncObservable<T>(next => {
                foreach (var item in enumerable) {
                    next(item);
                }
            });
        }

        public static IObservable<T> Create<T>(Action<Action<T>> generate) {
            return new SyncObservable<T>(generate);
        }

        public static IObservable<T2> Select<T, T2>(this IObservable<T> observable, Func<T, T2> next) {
            return new WrapObservable<T, T2>(observable, t => Unit(next(t)), Enumerable.Empty<T2>);
        }

        public static IObservable<T2> SelectMany<T, T2>(this IObservable<T> observable, Func<T, IEnumerable<T2>> next) {
            return new WrapObservable<T, T2>(observable, next, Enumerable.Empty<T2>);
        }

        public static Task ForEach<T>(this IObservable<T> observable, Action<T> next) {
            var source = new DisposableTaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);
            try {
                observable.Subscribe(new ShimObserver<T>(next, source.SetException, () => source.SetResult(null)));
            }
            catch (Exception ex) {
                source.SetException(ex);
            }
            return source.Task;
        }

        public static Task<T2> Aggregate<T, T2>(this IObservable<T> observable, T2 initialValue, Func<T2, T, T2> next) {
            var source = new DisposableTaskCompletionSource<T2>(TaskCreationOptions.AttachedToParent);
            try {
                var currentValue = initialValue;
                observable.Subscribe(new ShimObserver<T>(t => currentValue = next(currentValue, t), source.SetException, () => source.SetResult(currentValue)));
            }
            catch (Exception ex) {
                source.SetException(ex);
            }
            return source.Task;
        }

        static IEnumerable<T> Unit<T>(T next) {
            yield return next;
        }

        class WrapObservable<T, T2> : IObservable<T2> {
            readonly IObservable<T> _innerObservable;
            readonly Func<T, IEnumerable<T2>> _next;
            readonly Func<IEnumerable<T2>> _completed;

            public WrapObservable(IObservable<T> innerObservable, Func<T, IEnumerable<T2>> next, Func<IEnumerable<T2>> completed) {
                _innerObservable = innerObservable;
                _next = next;
                _completed = completed;
            }

            public IDisposable Subscribe(IObserver<T2> outerObserver) {
                Action<IEnumerable<T2>> send = et2 => {
                    foreach (var t2 in et2) {
                        outerObserver.OnNext(t2);
                    }
                };
                return _innerObservable.Subscribe(new ShimObserver<T>(
                    t => send(_next(t)),
                    outerObserver.OnError,
                    () => {
                        send(_completed());
                        outerObserver.OnCompleted();
                    }));
            }
        }

        class ShimObserver<T> : IObserver<T> {
            readonly Action<T> _next;
            readonly Action<Exception> _error;
            readonly Action _completed;

            public ShimObserver(Action<T> next, Action<Exception> error, Action completed) {
                _next = next;
                _error = error;
                _completed = completed;
            }

            public void OnNext(T value) {
                _next(value);
            }

            public void OnError(Exception error) {
                _error(error);
            }

            public void OnCompleted() {
                _completed();
            }
        }

        class SyncObservable<T> : IObservable<T> {
            readonly Action<Action<T>> _generate;

            public SyncObservable(Action<Action<T>> generate) {
                _generate = generate;
            }

            public IDisposable Subscribe(IObserver<T> observer) {
                try {
                    _generate(observer.OnNext);
                    observer.OnCompleted();
                }
                catch (Exception ex) {
                    observer.OnError(ex);
                }
                return new Disposable();
            }
        }


        class Disposable : IDisposable {
            readonly Action _dispose;
            public Disposable() : this(() => { }) {}

            public Disposable(Action dispose) {
                _dispose = dispose;
            }

            public void Dispose() {
                _dispose();
            }
        }
    }
}