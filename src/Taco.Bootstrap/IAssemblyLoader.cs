using System.Collections.Generic;
using Gate.Bootstrap;

namespace Gate.Framework.BuilderServices {
    public interface IAssemblyLoader {
        IEnumerable<BuilderAttribute> Load(string name);
    }
}
