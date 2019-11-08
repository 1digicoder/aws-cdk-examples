using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodepipelineDockerBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);
            var props = new PipelineProps() {
                Namespace = "cdk-csharp-example-pipeline",
            };
            var baseStack = new CodepipelineDockerBuildStack(app, "CodepipelineDockerBuildStack", props);
            app.Synth();
        }
    }
}
