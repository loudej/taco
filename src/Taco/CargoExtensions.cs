// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;

namespace Taco {
    public static class CargoExtensions {
        public static bool InvokeAsync<T>(this Action<Cargo<T>> action, T result, Action continuation) {
            var cargo = Cargo.From(result, continuation);
            action(cargo);
            return cargo.Delayed;
        }

        public static void InvokeSync<T>(this Action<Cargo<T>> action, T result) {
            action(Cargo.From(result));
        }

        public static bool OnNextAsync<T>(this IObserver<Cargo<T>> observer, T result, Action continuation) {
            var cargo = Cargo.From(result, continuation);
            observer.OnNext(cargo);
            return cargo.Delayed;
        }
    }
}