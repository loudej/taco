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
            private readonly IObservable<T> _innerObservable;
            private readonly Func<T, IEnumerable<T2>> _next;
            private readonly Func<IEnumerable<T2>> _completed;

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
            private readonly Action<T> _next;
            private readonly Action<Exception> _error;
            private readonly Action _completed;

            public ShimObserver(Action<T> next, Action<Exception> error, Action completed) {
                _next = next;
                _error = error;
                _completed = completed;
            }

            public void OnNext(T value) { _next(value); }
            public void OnError(Exception error) { _error(error); }
            public void OnCompleted() { _completed(); }
        }

        class SyncObservable<T> : IObservable<T> {
            private readonly Action<Action<T>> _generate;

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
            private readonly Action _dispose;
            public Disposable() : this(() => { }) { }
            public Disposable(Action dispose) { _dispose = dispose; }
            public void Dispose() { _dispose(); }
        }

    }
}
