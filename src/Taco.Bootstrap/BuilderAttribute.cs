using System;

namespace Gate.Bootstrap {
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
