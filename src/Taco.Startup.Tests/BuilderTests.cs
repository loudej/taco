// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Taco.Startup;

namespace Taco.Framework.Tests {
    using FnApp = Action<
        IDictionary<string, object> /*env*/,
        Action<Exception> /*fault*/,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>> /*result(status,headers,body)*/>;
    using FnResult = Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>;

    [TestFixture]
    public class BuilderTests {
        [Test]
        public void Builder_should_call_and_return_single_app() {
            FnApp app = (env, fault, result) => { };

            var builder = new Builder();
            builder.Run(app);
            var app2 = builder.ToApp();

            Assert.That(app2, Is.SameAs(app));
        }

        [Test]
        public void Simple_format_may_be_parsed_to_load_assemblies() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse("Load TestName");
            AssertLoadNames(loader, "TestName");
        }

        void AssertLoadNames(StubAssemblyLoader loader, params string[] names) {
            Assert.That(loader.LoadNames, Has.Count.EqualTo(names.Count()));
            foreach (var name in names) {
                Assert.That(loader.LoadNames, Has.Some.EqualTo(name));
            }
        }

        [Test]
        public void Newlines_seperate_directives() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"Load TestName
Load TestTwo");
            AssertLoadNames(loader, "TestName", "TestTwo");
        }

        [Test]
        public void Leading_and_trailing_newlines_ignored() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"

Load TestName
Load TestTwo

");
            AssertLoadNames(loader, "TestName", "TestTwo");
        }

        [Test]
        public void Run_directive_calls_app_factory() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"
Load TestName
Run TestApp
");
            var app = builder.ToApp();
            Assert.That(app, Is.SameAs(TestApp.Singleton));
        }


        [Test]
        public void Use_directive_calls_middleware_factory() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"
Load TestName
Use TestMiddleware
Run TestApp
");
            var app = builder.ToApp();
            IDictionary<string, object> env = new Dictionary<string, object>();
            app(env, ex => { }, (a, b, c) => { });
            Assert.That(env["TestMiddleware"], Is.True);
            Assert.That(env["TestApp"], Is.True);
        }

        [Test]
        public void Run_with_an_enumerable_fnapp_argument_will_wrap_all_previous_run_calls() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"
Load TestName
Use TestMiddleware
Run TestApp
Use TestMiddleware
Run TestApp
Run TestCascade
");
            var app = builder.ToApp();
            IDictionary<string, object> env = new Dictionary<string, object>();
            app(env, ex => { }, (a, b, c) => { });
            Assert.That(env.ContainsKey("TestMiddleware"), Is.False, "the use are attached to the apps, not the cascade");
            Assert.That(env["TestCascade.Count"], Is.EqualTo(2));
        }
        [Test]
        public void Use_with_more_text_provides_extra_argument() {
            var loader = new StubAssemblyLoader();
            var builder = new Builder(loader);
            builder.Parse(@"
Load TestName
Use TestWithData Foo
Run TestApp
");
            var app = builder.ToApp();
            IDictionary<string, object> env = new Dictionary<string, object>();
            app(env, ex => { }, (a, b, c) => { });
            Assert.That(env.ContainsKey("TestWithData"), Is.True);
            Assert.That(env["TestWithData.Data"], Is.EqualTo("Foo"));
        }
    }

    public class TestApp {
        public static FnApp Singleton = Call;

        public static FnApp Create() {
            return Singleton;
        }

        static void Call(IDictionary<string, object> env, Action<Exception> fault, FnResult result) {
            env["TestApp"] = true;
            fault(null);
        }
    }

    public class TestMiddleware {
        readonly FnApp _app;

        TestMiddleware(FnApp app) {
            _app = app;
        }

        public static FnApp Create(FnApp app) {
            return new TestMiddleware(app).Call;
        }

        void Call(IDictionary<string, object> env, Action<Exception> fault, FnResult result) {
            env["TestMiddleware"] = true;
            _app(env, fault, result);
        }
    }

    public class TestWithData {
        readonly FnApp _app;
        private readonly string _data;

        TestWithData(FnApp app, string data)
        {
            _app = app;
            _data = data;
        }

        public static FnApp Create(FnApp app, string data) {
            return new TestWithData(app, data).Call;
        }

        void Call(IDictionary<string, object> env, Action<Exception> fault, FnResult result) {
            env["TestWithData"] = true;
            env["TestWithData.Data"] = _data;
            _app(env, fault, result);
        }
    }
    public class TestCascade {
        public static FnApp Create(IEnumerable<FnApp> apps) {
            return (env, fault, result) => {
                env["TestCascade.Count"] = apps.Count();
                result(0, null, null);
            };
        }
    }

    public class StubAssemblyLoader : IAssemblyLoader {
        public List<string> LoadNames = new List<string>();

        public IEnumerable<BuilderAttribute> Load(string name) {
            LoadNames.Add(name);
            yield return new BuilderAttribute("TestApp", typeof(TestApp), "Create");
            yield return new BuilderAttribute("TestMiddleware", typeof(TestMiddleware), "Create");
            yield return new BuilderAttribute("TestCascade", typeof(TestCascade), "Create");
            yield return new BuilderAttribute("TestWithData", typeof(TestWithData), "Create");
        }
    }
}
