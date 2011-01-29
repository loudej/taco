using System.Collections.Generic;

namespace Taco.Startup {
    public interface IAssemblyLoader {
        IEnumerable<BuilderAttribute> Load(string name);
    }
}
