var target = Argument("target", "Default");
var buildInAppveyor = bool.Parse(EnvironmentVariable("APPVEYOR") ?? "False");
var manualBuild = bool.Parse(EnvironmentVariable("APPVEYOR_FORCED_BUILD") ?? "False");
var isNotForPullRequest = string.IsNullOrWhiteSpace(EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"));
var slnPath = "./../Netsy.sln";
var versionNumber = (EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "1.0.0.0").Split('-')[0];

Task("CleanFolders")
    .Does(() => 
{
    var paths = new string[] 
    {
        "./../src/Netsy/",
        "./../tests/Netsy.Tests.Playground/"
    };

    foreach (var path in paths)
    {
        CleanDirectory(path + "bin");
        CleanDirectory(path + "obj");
    }
});

Task("NuGetRestore")
    .IsDependentOn("CleanFolders")
    .Does(() => 
{
	var settings = new MSBuildSettings().WithTarget("restore");
	MSBuild(slnPath, settings);
});

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() => 
{
    MSBuildSettings settings;
    if (buildInAppveyor && manualBuild && isNotForPullRequest)
    {
        settings = new MSBuildSettings 
        {
            Configuration = "Release",
			MSBuildPlatform = MSBuildPlatform.x86,
			ToolVersion = MSBuildToolVersion.VS2017,
        };
    }
    else
    {
        settings = new MSBuildSettings 
        {
            Configuration = "Debug",
			MSBuildPlatform = MSBuildPlatform.x86,
			ToolVersion = MSBuildToolVersion.VS2017,
        };
    }

    MSBuild(slnPath, settings);
});

Task("CreateNuGetPackage")
	.IsDependentOn("Build")
	.Does(() => 
{
	var settings = new DotNetCorePackSettings
	{
        Configuration = buildInAppveyor && manualBuild && isNotForPullRequest ? "Release" : "Debug",
		OutputDirectory = "./../artifacts/"
	};
	DotNetCorePack("./../src/Netsy/Netsy.csproj", settings);
});

Task("UploadArtifacts")
	.IsDependentOn("CreateNuGetPackage")
	.WithCriteria(() => buildInAppveyor)
	.Does(() => 
{	
	var nugetPackagePath = string.Format("./../artifacts/Netsy.{0}.nupkg", versionNumber);
	BuildSystem.AppVeyor.UploadArtifact(nugetPackagePath);
});

Task("Default")
    .IsDependentOn("UploadArtifacts");

RunTarget(target);