﻿using BenchmarkDotNet.Attributes;
using System;
using System.IO;

namespace DotNetCliPerf
{
    public abstract class CoreApp: App
    {
        private const string _globalJson = @"{ ""sdk"": { ""version"": ""0.0.0"" } }";

        [Params("2.0.2", "2.1.1")]
        public string SdkVersion { get; set; }

        // [Params(false, true)]
        public bool TieredJit { get; set; }

        [GlobalSetup]
        public override void GlobalSetup()
        {
            if (TieredJit)
            {
                Environment.Add("COMPLUS_EXPERIMENTAL_TieredCompilation", "1");
            }

            base.GlobalSetup();
        }

        protected override void CopyApp()
        {
            base.CopyApp();

            File.WriteAllText(Path.Combine(RootTempDir, "global.json"), _globalJson.Replace("0.0.0", SdkVersion));

            // Verify version
            var output = DotNet("--info");
            if (!output.Contains($"Version:            {SdkVersion}"))
            {
                throw new InvalidOperationException($"Incorrect SDK version");
            }
        }

        protected override void Build()
        {
            DotNet("build");
        }

        protected override string Run()
        {
            return DotNet("run");
        }

        protected string DotNet(string arguments, bool throwOnError = true)
        {
            return Util.RunProcess("dotnet", arguments, RootTempDir, throwOnError: throwOnError, environment: Environment);
        }
    }
}