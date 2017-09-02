#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = "Debug";
var solutionPath = "./WebApi.sln";
var projectFilePath = "./src/WebApi/WebApi.csproj";
var testsFilePath = "./tests/WebApi.Tests/WebApi.Tests.csproj";
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
            Configuration = configuration
        };

        if (versionInfo != null)
        {
            settings.VersionSuffix = versionInfo.NuGetVersion;
        }

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

Task("Version")
    .Does(() => 
    {
        GitVersion(new GitVersionSettings{
            OutputType = GitVersionOutput.BuildServer,
            NoFetch = true
        });

        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json, NoFetch = true });
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
    {
        /*
        if(AppVeyor.IsRunningOnAppVeyor &&
            EnvironmentVariable("APPVEYOR_REPO_TAG") != "true" &&
            versionInfo.BranchName == "master")
            {
                return;
            }
        */

        var settings = new DotNetCorePublishSettings
        {
            Configuration = configuration
        };

        if (versionInfo != null)
        {
            settings.VersionSuffix = versionInfo.NuGetVersion;
        }

        DotNetCorePublish(projectFilePath, settings);
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

RunTarget(target);