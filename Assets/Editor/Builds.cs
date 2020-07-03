using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public class Builds
{
	static void BuildFromCLI()
	{
		string buildTarget = GetArgument("buildTarget");
		switch(buildTarget)
		{
			case "OSXUniversal":
				OSXUniversal();
				break;
			case "Win64":
				Win64();
				break;
			case "Linux64":
				Linux();
				break;
			default:
				throw new Exception("No build command for " + buildTarget + ".");
		}
	}

	static void OSXUniversal()
	{
		string outputFile = GetArgument("buildPath");
		StartBuild(BuildTarget.StandaloneOSX, outputFile);
	}


	static void Win64()
	{
		string outputFile = GetArgument("buildPath") +".exe";
		StartBuild(BuildTarget.StandaloneWindows, outputFile);
	}

	static void Linux()
	{
		string outputFile = GetArgument("buildPath");
		StartBuild(BuildTarget.StandaloneLinux64, outputFile);
	}

	static void StartBuild(BuildTarget buildTarget, string outputFile)
	{
		if(File.Exists(outputFile))
			File.Delete(outputFile);

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = GetScenes();
		buildPlayerOptions.locationPathName = outputFile;
		buildPlayerOptions.target = buildTarget;
		buildPlayerOptions.options = BuildOptions.None;

		string report = BuildPipeline.BuildPlayer(buildPlayerOptions);

		Debug.Log(report);
	}

	static string[] GetScenes()
	{
		return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
	}

	static string GetArgument(string name)
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].Contains(name))
			{
				return args[i + 1];
			}
		}
		return null;
	}
}
