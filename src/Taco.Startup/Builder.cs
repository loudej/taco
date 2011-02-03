﻿// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Taco.Startup {
    using AppAction = Action<IDictionary<string, object>, Action<Exception>, Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    public class Builder {
        readonly IAssemblyLoader _loader;
        readonly Stack<Func<AppAction, AppAction>> _middlewares = new Stack<Func<AppAction, AppAction>>();
        readonly List<AppAction> _apps = new List<AppAction>();

        static readonly char[] EndOfLineCharacters = new[] {'\r', '\n'};
        readonly IDictionary<string, Action<string>> _directives;

        readonly IDictionary<string, IList<MethodInfo>> _componentFactories = new Dictionary<string, IList<MethodInfo>>();


        public Builder()
            : this(new DefaultAssemblyLoader()) {}

        public Builder(IAssemblyLoader loader) {
            _loader = loader;
            _directives = new Dictionary<string, Action<string>> {
                {"Load ", DoLoad},
                {"Run ", DoRun},
                {"Use ", DoUse},
            };
        }

        public Builder Use(Func<AppAction, AppAction> middleware) {
            _middlewares.Push(middleware);
            return this;
        }

        public Builder Run(AppAction app) {
            var wrapped = app;
            while (_middlewares.Count != 0) {
                var middleware = _middlewares.Pop();
                wrapped = middleware(wrapped);
            }
            _apps.Add(wrapped);
            return this;
        }

        public AppAction ToApp() {
            return _apps.Single();
        }

        public void Parse(string text) {
            ParseLines(SplitLines(text));
        }

        static IEnumerable<string> SplitLines(string text) {
            var scanIndex = 0;
            while (scanIndex < text.Length) {
                var endOfLineIndex = text.IndexOfAny(EndOfLineCharacters, scanIndex);
                if (endOfLineIndex == -1)
                    endOfLineIndex = text.Length;

                if (scanIndex != endOfLineIndex)
                    yield return text.Substring(scanIndex, endOfLineIndex - scanIndex);

                scanIndex = endOfLineIndex + 1;
            }
        }

        public void ParseLines(IEnumerable<string> lines) {
            foreach (var line in lines) {
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("//"))
                    continue;

                var executed = false;
                foreach (var directive in _directives) {
                    if (!line.StartsWith(directive.Key, StringComparison.InvariantCultureIgnoreCase)) continue;
                    directive.Value(line.Substring(directive.Key.Length));
                    executed = true;
                    break;
                }
                if (!executed)
                    throw new ApplicationException("Line not executed: " + line);
            }
        }

        void DoLoad(string text) {
            var factoryAttributes = _loader.Load(text);
            foreach (var factoryAttribute in factoryAttributes) {
                var methods = factoryAttribute.Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var methodInfo in methods) {
                    if (!methodInfo.Name.Equals(factoryAttribute.MethodName)) continue;

                    IList<MethodInfo> factories;
                    if (!_componentFactories.TryGetValue(factoryAttribute.ComponentName, out factories)) {
                        factories = new List<MethodInfo>();
                        _componentFactories[factoryAttribute.ComponentName] = factories;
                    }
                    factories.Add(methodInfo);
                }
            }
        }

        void DoRun(string text) {
            IList<MethodInfo> factories;
            if (!_componentFactories.TryGetValue(text, out factories)) {
                throw new ApplicationException("No factory methods registered for component: " + text);
            }
            var factory = factories.Single();
            var parameters = factory.GetParameters();
            var args = new List<object>();

            // provide all apps that have previously been pushed to a fnapp[] compatible argument
            if (parameters.Any() && parameters.First().ParameterType.IsAssignableFrom(typeof(AppAction[]))) {
                args.Add(_apps.ToArray());
                _apps.Clear();
            }

            var app = factories.Single().Invoke(null, args.ToArray());
            var app2 = Coerce.CoerceDelegate<AppAction>(app);
            Run(app2);
        }

        void DoUse(string text) {
            IList<MethodInfo> factories;
            if (!_componentFactories.TryGetValue(text, out factories)) {
                throw new ApplicationException("No factory methods registered for component: " + text);
            }

            Use(app => {
                var methodInfo = factories.Single();
                var neededDelegateType = methodInfo.GetParameters().Single().ParameterType;
                var middleware = methodInfo.Invoke(null, new[] {Coerce.CoerceDelegate(neededDelegateType, app)});
                return Coerce.CoerceDelegate<AppAction>(middleware);
            });
        }
    }
}