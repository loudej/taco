using System;
using System.Threading.Tasks;

namespace Taco {
    public class Cargo<T> {
        readonly T _result;
        readonly Delivery _delivery;

        public Cargo(T result) {
            _result = result;
        }

        public Cargo(T result, Action continuation) {
            _result = result;
            _delivery = new Delivery(continuation);
        }

        Cargo(T result, Delivery delivery) {
            _result = result;
            _delivery = delivery;
        }

        //public static implicit operator Cargo<T>(T result) {
        //    return new Cargo<T>(result);
        //}

        public T Result { get { return _result; } }
        public bool Delayable { get { return _delivery != null; } }
        public bool Delayed { get { return _delivery != null && _delivery.Delayed; } }

        public void Delay() { _delivery.Delayed = true; }
        public void Resume() { _delivery.Delayed = true; _delivery.Continuation(); }

        public Cargo<T2> Alter<T2>(T2 result) {
            return new Cargo<T2>(result, _delivery);
        }
    }

    class Delivery {
        public Delivery(Action continuation) {
            Continuation = continuation;
        }

        public bool Delayed { get; set; }
        public Action Continuation { get; private set; }
    }

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
