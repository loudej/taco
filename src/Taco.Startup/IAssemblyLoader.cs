// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System.Collections.Generic;

namespace Taco.Startup {
    public interface IAssemblyLoader {
        IEnumerable<BuilderAttribute> Load(string name);
    }
}