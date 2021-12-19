using System;

namespace TopDownWpfClient.Models {
    public class EventArgs<T> : EventArgs {
        public EventArgs(T value) {
            Value = value;
        }

        public T Value { get; }
    }
}