using System;

namespace Taco {

    public class Cargo {
        readonly Delivery _delivery;
        
        protected Cargo() {
        }

        protected Cargo(Delivery delivery) {
            _delivery = delivery;
        }
        protected Cargo(Cargo cargo) {
            _delivery = cargo._delivery;
        }

        public bool Delayable { get { return _delivery != null; } }
        public bool Delayed { get { return _delivery != null && _delivery.Delayed; } }
        public void Delay() { _delivery.Delayed = true; }
        public void Resume() { _delivery.Delayed = true; _delivery.Continuation(); }

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

        public T Result { get { return _result; } }

        public Cargo<T2> Alter<T2>(T2 result) {
            return new Cargo<T2>(result, this);
        }
    }
}
