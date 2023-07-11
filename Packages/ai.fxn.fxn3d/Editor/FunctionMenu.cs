/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Internal;

    internal static class FunctionMenu {

        private const int BasePriority = -50;
        
        [MenuItem(@"Function/Function " + Function.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"Function/Function " + Function.Version, true, BasePriority)]
        private static bool EnableVersion () => false;

        [MenuItem(@"Function/Get Access Key", false, BasePriority + 1)]
        private static void OpenAccessKey () => Help.BrowseURL(@"https://fxn.ai/account/developers");

        [MenuItem(@"Function/Explore Predictors", false, BasePriority + 2)]
        private static void OpenExplore () => Help.BrowseURL(@"https://fxn.ai/explore");

        [MenuItem(@"Function/View the Docs", false, BasePriority + 3)]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.fxn.ai");

        [MenuItem(@"Function/Open an Issue", false, BasePriority + 4)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/fxnai/fxn3d");

        [MenuItem(@"Function/Clear Predictor Cache", false, BasePriority + 5)]
        private static void ClearCache () {
            Debug.Log("Function: Cleared predictor cache");
        }
    }
}
