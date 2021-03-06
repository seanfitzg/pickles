﻿#region License

/*
    Copyright [2011] [Jeffrey Cameron]

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

#endregion

using System;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Autofac;

namespace PicklesDoc.Pickles.MSBuild
{
    public class Pickles : Task
    {
        [Required]
        public string FeatureDirectory { get; set; }

        [Required]
        public string OutputDirectory { get; set; }

        public string Language { get; set; }

        public string ResultsFormat { get; set; }

        public string ResultsFile { get; set; }

        public string SystemUnderTestName { get; set; }

        public string SystemUnderTestVersion { get; set; }

        public string DocumentationFormat { get; set; }

        private void CaptureConfiguration(Configuration configuration, IFileSystem fileSystem)
        {
            configuration.FeatureFolder = fileSystem.DirectoryInfo.FromDirectoryName(this.FeatureDirectory);
            configuration.OutputFolder = fileSystem.DirectoryInfo.FromDirectoryName(this.OutputDirectory);
            if (!string.IsNullOrEmpty(this.Language)) configuration.Language = this.Language;
            if (!string.IsNullOrEmpty(this.ResultsFormat))
                configuration.TestResultsFormat =
                    (TestResultsFormat) Enum.Parse(typeof (TestResultsFormat), this.ResultsFormat, true);
            if (!string.IsNullOrEmpty(this.ResultsFile)) configuration.TestResultsFiles = new[] { fileSystem.FileInfo.FromFileName(this.ResultsFile) };
            if (!string.IsNullOrEmpty(this.SystemUnderTestName)) configuration.SystemUnderTestName = this.SystemUnderTestName;
            if (!string.IsNullOrEmpty(this.SystemUnderTestVersion))
                configuration.SystemUnderTestVersion = this.SystemUnderTestVersion;
            if (!string.IsNullOrEmpty(this.DocumentationFormat))
                configuration.DocumentationFormat =
                    (DocumentationFormat) Enum.Parse(typeof (DocumentationFormat), this.DocumentationFormat, true);
        }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage("Pickles v.{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                var builder = new ContainerBuilder();
                builder.RegisterAssemblyTypes(typeof(Runner).Assembly);
                builder.Register<FileSystem>(_ => new FileSystem()).As<IFileSystem>().SingleInstance();
                builder.RegisterModule<PicklesModule>();
                var container = builder.Build();

                var configuration = container.Resolve<Configuration>();
                this.CaptureConfiguration(configuration, container.Resolve<IFileSystem>());

                new ConfigurationReporter().ReportOn(configuration, message => Log.LogMessage(message));

                var runner = container.Resolve<Runner>();
                runner.Run(container);
            }
            catch (Exception e)
            {
                Log.LogWarningFromException(e, false);
            }

            // HACK - since this is merely producing documentation we do not want it to cause a build to fail if something goes wrong
            return true;
        }
    }
}