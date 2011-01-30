﻿// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System.Collections.Generic;
using System.Reflection;

namespace Taco.Startup {
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