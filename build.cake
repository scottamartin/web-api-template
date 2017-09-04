#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.Docker"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var solutionPath = "./WebApi.sln";
var dockerfilePath = "./src/WebApi/";
var testsFilePath = "./tests/WebApi.Tests/WebApi.Tests.csproj";
var imageTag = "web-api";
GitVersion versionInfo = null;

Task("Default")
  .IsDependentOn("Build");

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Version")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            VersionSuffix = versionInfo.NuGetVersion
        };

        DotNetCoreBuild(solutionPath, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true
        };
        DotNetCoreTest(testsFilePath, settings);
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var settings = new DockerBuildSettings
        {
            BuildArg = new [] { 
                string.Format("CONFIGURATION={0}", configuration), 
                string.Format("VERSION_SUFFIX={0}", versionInfo.NuGetVersion)
                },
            ForceRm = true,
            Tag = new[] { imageTag.ToLower() }
        };

        DockerBuild(settings, dockerfilePath);
    });

Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore(solutionPath);
    });

Task("Clean")
    .Does(() =>
    {
        DotNetCoreClean(solutionPath);
    });

Task("Version")
    .Does(() => 
    {
        GitVersion(new GitVersionSettings{
            OutputType = GitVersionOutput.BuildServer,
            NoFetch = true
        });

        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json, NoFetch = true });
    });

RunTarget(target);