using System.IO;
using UnityEngine;
using UnityEditor;
#if UNITY_IOS
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif
using UnityEditor.Callbacks;


namespace Facebook.Unity.PostProcess
{
    /// <summary>
    /// Automatically disables Bitcode on iOS builds
    /// </summary>
    public static class PostProcessBuildHook
    {
        [PostProcessBuildAttribute(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
#if UNITY_IOS
            if (buildTarget != BuildTarget.iOS) return;
            string projectPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            //Disabling Bitcode on all targets
            //Main
            string target = pbxProject.GetUnityMainTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            //Unity Tests
            target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            //Unity Framework
            target = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            // Fix cocoapods frameworks swift
            pbxProject.FixCocoaPods();
            pbxProject.WriteToFile(projectPath);
#endif
        }

#if UNITY_IOS        
        /// <summary>
        /// Collection of all needed pod frameworks for the SDK to function properly
        /// </summary>
        private static readonly string[] _podFrameworks = {
            "FBAEMKit",
            "FBSDKCoreKit",
            "FBSDKCoreKit_Basics",
            "FBSDKGamingServicesKit",
            "FBSDKLoginKit",
            "FBSDKShareKit"
        };

        /// <summary>
        /// Add a given pod handled framework into a target project
        /// </summary>
        private static void AddPodToProject(this PBXProject project, string targetGuid, string framework)
        {
            var src = Path.Combine("Pods", framework, "XCFrameworks", $"{framework}.xcframework");
            project.AddFileToEmbedFrameworks(targetGuid, project.AddFile(src, src));
        }
        
        /// <summary>
        /// The pod file installation does not embed the XCFrameworks on it's own failing the loading of the application
        /// </summary>
        private static void FixCocoaPods(this PBXProject project)
        {
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in _podFrameworks)
            {
                project.AddPodToProject(targetGuid, framework);
            }
        }        
#endif
    }
}
