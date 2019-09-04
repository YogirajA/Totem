using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Fixie;
using static Totem.Tests.Testing;

namespace Totem.Tests
{
    public class TestingConvention : Discovery, Execution
    {
        public TestingConvention()
        {
            Parameters.Add<InputParameterSource>();
        }
        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                @case.Execute(instance);

                instance.Dispose();

                var methodWasExplicitlyRequested = testClass.TargetMethod != null;

                if (methodWasExplicitlyRequested && @case.Exception is MatchException exception)
                    LaunchDiffTool(exception);
            });
        }

        public class InputParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                var inputAttributes = method.GetCustomAttributes<Input>().ToArray();

                foreach (var input in inputAttributes)
                    yield return input.Parameters;
            }
        }

        private static void LaunchDiffTool(MatchException exception)
        {
            var diffTool = Configuration["Tests:DiffTool"];

            if (!File.Exists(diffTool)) return;

            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            File.WriteAllText(expectedPath, Json(exception.Expected));
            File.WriteAllText(actualPath, Json(exception.Actual));

            using (Process.Start(diffTool, $"{expectedPath} {actualPath}")) { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class Input : Attribute
        {
            public Input(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; }
        }
    }
}
