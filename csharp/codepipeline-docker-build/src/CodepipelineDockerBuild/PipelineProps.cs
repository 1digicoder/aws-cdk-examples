using Amazon.CDK;

namespace CodepipelineDockerBuild 
{
    public class PipelineProps : StackProps
    {
        public string Namespace { get; set; }
        public string BucketName { get; set; }

        public PipelineProps() : base() { }
    }
}