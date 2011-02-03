// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;

namespace Taco {
    public static class CargoExtensions {
        public static bool InvokeAsync<T>(this Action<Cargo<T>> action, T result, Action continuation) {
            var cargo = new Cargo<T>(result, continuation);
            action(cargo);
            return cargo.Delayed;
        }

        public static void InvokeSync<T>(this Action<Cargo<T>> action, T result) {
            action(new Cargo<T>(result));
        }

        public static bool OnNextAsync<T>(this IObserver<Cargo<T>> observer, T result, Action continuation) {
            var cargo = new Cargo<T>(result, continuation);
            observer.OnNext(cargo);
            return cargo.Delayed;
        }
    }
}