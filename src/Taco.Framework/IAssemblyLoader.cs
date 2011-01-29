using System.Collections.Generic;
using System.Reflection;

namespace Gate.Framework.BuilderServices {
    public interface IAssemblyLoader {
        IEnumerable<GateFactoryAttribute> Load(string name);
    }
    public class DefaultAssemblyLoader : IAssemblyLoader {
        public IEnumerable<GateFactoryAttribute> Load(string name) {
            var assembly = Assembly.Load(name);
            var attrs = assembly.GetCustomAttributes(typeof(GateFactoryAttribute), false);
            var result = new GateFactoryAttribute[attrs.Length];
            attrs.CopyTo(result, 0);
            return result;
        }
    }
}
