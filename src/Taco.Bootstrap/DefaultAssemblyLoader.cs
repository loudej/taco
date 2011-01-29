using System.Collections.Generic;
using System.Reflection;
using Gate.Framework.BuilderServices;

namespace Gate.Bootstrap
{
    public class DefaultAssemblyLoader : IAssemblyLoader {
        public IEnumerable<BuilderAttribute> Load(string name) {
            var assembly = Assembly.Load(name);
            var attrs = assembly.GetCustomAttributes(typeof(BuilderAttribute), false);
            var result = new BuilderAttribute[attrs.Length];
            attrs.CopyTo(result, 0);
            return result;
        }
    }
}