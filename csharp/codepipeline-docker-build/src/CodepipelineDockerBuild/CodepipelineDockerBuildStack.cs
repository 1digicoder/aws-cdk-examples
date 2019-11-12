using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.CodeBuild;

namespace CodepipelineDockerBuild
{
    public class OutputProperties
    {
        public Bucket Bucket { get; set; }
        public PipelineProject CodeBuildDockerBuild { get; set; }
    }
    public class CodepipelineDockerBuildStack : Stack
    {
        public OutputProperties OutputProperties { get; }
        public CodepipelineDockerBuildStack(Construct parent, string id, PipelineProps props) : base(parent, id, props)
        {
            // The code that defines your stack goes here

            // pipeline requires a versioned bucket
            var bucket = new Bucket(this, "SourceBucket", new BucketProps 
            {
                BucketName=$"{props.Namespace.ToLower()}-{Aws.ACCOUNT_ID}",
                Versioned=true,
                RemovalPolicy=RemovalPolicy.DESTROY
            });

            // Create a string paramater for the bucket name
            var bucketParam = new StringParameter(
                this, "ParameterB",
                new StringParameterProps
                {
                    ParameterName=$"{props.Namespace}-bucket",
                    StringValue=bucket.BucketName,
                    Description="CDK Pipeline Bucket"
                });

            // create an ECR repo for the docker container
            var ecrRepo = new Repository(
                this, "ECR",
                new RepositoryProps
                {
                    RepositoryName=$"{props.Namespace}",
                    RemovalPolicy=RemovalPolicy.DESTROY
                });
            
            // create CodeBuild project

            var codeBuildDockerBuild = new PipelineProject(
                this, "DockerBuild",
                new PipelineProjectProps {
                    ProjectName=$"{props.Namespace}-Docker-Build",
                    BuildSpec=BuildSpec
                        .FromSourceFilename("pipeline_delivery/docker_build_buildspec.yml"),
                    Environment=new BuildEnvironment 
                    {
                        Privileged=true
                    },
                    EnvironmentVariables=new Dictionary<string, IBuildEnvironmentVariable>
                    {
                        // TODO: Resolve when JSII bug is fixed
                        // Due to JSII bug, need to do a substitution instead of natural call
                        { "ecr", new BuildEnvironmentVariable { Value=ecrRepo.RepositoryUri.ToString()} }, 
                        { 
                            "tag", new BuildEnvironmentVariable { Value="cdk"}
                        }
                    },
                    Description="Pipeline for CSharp CodeBuild",
                    Timeout=Duration.Minutes(60.0)
                }
            );

            // CodeBuild IAM permissions to read/write S3
            bucket.GrantReadWrite(codeBuildDockerBuild);

            // CodeBuild IAM permissions to interact with ECR
            //TODO: Also suffers from JSII bug
            ecrRepo.GrantPullPush(codeBuildDockerBuild as IGrantable);

            this.OutputProperties = new OutputProperties
            {
                Bucket = bucket,
                CodeBuildDockerBuild = codeBuildDockerBuild
            };
        }
    }
}
