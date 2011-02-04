// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;

namespace Taco.Helpers.Utils {
    public static class ObservableExtensions {
        public static IObservable<T> Create<T>(Func<Action<T>, Action<Exception>, Action, IDisposable> subscribe) {
            return new Observable<T>(subscribe);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> next, Action<Exception> fault, Action done) {
            return source.Subscribe(new Observer<T>(next, fault, done));
        }

        public static IObservable<T> Filter<T>(this IObservable<T> source, Func<Func<Action<T>, Action<Exception>, Action, IDisposable>, Action<T>, Action<Exception>, Action, IDisposable> filter) {
            return new Observable<T>((next, fault, done) => filter((next2, fault2, done2) => source.Subscribe(new Observer<T>(next2, fault2, done2)), next, fault, done));
        }

        public static IObservable<T> Filter<T>(this IObservable<T> source, Func<T, T> filter) {
            return source.Filter((subscribe, next, fault, done) => subscribe(data => next(filter(data)), fault, done));
        }

        public static IObservable<Cargo<T>> Filter<T>(this IObservable<Cargo<T>> source, Func<T, T> filter) {
            return source.Filter((subscribe, next, fault, done) => subscribe(data => next(data.Alter(filter(data.Result))), fault, done));
        }

        class Observable<T> : IObservable<T> {
            readonly Func<Action<T>, Action<Exception>, Action, IDisposable> _subscribe;

            public Observable(Func<Action<T>, Action<Exception>, Action, IDisposable> subscribe) {
                _subscribe = subscribe;
            }

            public IDisposable Subscribe(IObserver<T> observer) {
                return _subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            }
        }

        class Observer<T> : IObserver<T> {
            readonly Action<T> _next;
            readonly Action<Exception> _error;
            readonly Action _completed;

            public Observer(Action<T> next, Action<Exception> error, Action completed) {
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
    }
}