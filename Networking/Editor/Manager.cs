using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SmallProdGame.Networking.Editor
{
    public class Manager : EditorWindow
    {
        private readonly string[] _clientPlatforms =
        {
            "Linux",
            "Windows",
            "Android",
            "IOS",
            "MacOS"
        };

        private readonly string[] _serverPlatforms =
        {
            "Linux",
            "Windows"
        };

        private int _clientPlatform;
        private int _serverPlatform;

        [MenuItem("Window/SmallProdGame/Networking")]
        public static void ShowWindow()
        {
            GetWindow<Manager>("SmallProdGame Networking Manager");
        }

        private void OnGUI()
        {
            GUILayout.Label("Build settings", EditorStyles.boldLabel);
            _serverPlatform = EditorGUILayout.Popup("Server platform", _serverPlatform, _serverPlatforms);
            _clientPlatform = EditorGUILayout.Popup("Client platform", _clientPlatform, _clientPlatforms);
            if (GUILayout.Button("Build server")) BuildServer();

            if (GUILayout.Button("Build client")) BuildClient();
            if (GUILayout.Button("Build server & client"))
            {
                BuildServer();
                BuildClient();
            }
        }

        private void BuildServer()
        {
            var opt = new BuildPlayerOptions();
            var scenes = new List<string>();
            foreach (var item in EditorBuildSettings.scenes) scenes.Add(item.path);
            opt.scenes = scenes.ToArray();
            opt.locationPathName = "Builds/Server/server.exe";
            opt.options = BuildOptions.EnableHeadlessMode;
            Build(opt, _serverPlatforms[_serverPlatform]);
        }

        private void BuildClient()
        {
            var opt = new BuildPlayerOptions();
            var scenes = new List<string>();
            foreach (var item in EditorBuildSettings.scenes) scenes.Add(item.path);
            opt.scenes = scenes.ToArray();
            opt.locationPathName = "Builds/Client/client.exe";
            opt.options = BuildOptions.None;
            Build(opt, _clientPlatforms[_clientPlatform]);
        }

        private void Build(BuildPlayerOptions opt, string platform)
        {
            Debug.Log(platform);
            switch (platform)
            {
                case "Linux":
                    opt.target = BuildTarget.StandaloneLinux64;
                    break;
                case "Windows":
                    Debug.Log("Build for windows");
                    opt.target = BuildTarget.StandaloneWindows64;
                    break;
                case "MacOS":
                    opt.target = BuildTarget.StandaloneOSX;
                    break;
                case "Android":
                    opt.target = BuildTarget.Android;
                    break;
                case "IOS":
                    opt.target = BuildTarget.iOS;
                    break;
            }

            var report = BuildPipeline.BuildPlayer(opt);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                EditorUtility.RevealInFinder(opt.locationPathName);
            }

            if (summary.result == BuildResult.Failed) Debug.Log("Build failed");
        }

        private BuildPlayerOptions FindPlatform(BuildPlayerOptions opt, string platform)
        {
            return opt;
        }
    }
}