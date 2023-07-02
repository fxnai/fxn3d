/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class FunctionSettingsProvider {

        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/Function", SettingsScope.Project) {
            label = @"Function",
            guiHandler = searchContext => {
                EditorGUILayout.LabelField(@"Function Account", EditorStyles.boldLabel);
                FunctionProjectSettings.instance.AccessKey = EditorGUILayout.TextField(@"Access Key", FunctionProjectSettings.instance.AccessKey);
            },
            keywords = new HashSet<string>(new[] { @"Function", @"NatML", @"NatCorder", @"NatDevice", @"NatShare", @"Hub" }),
        };
    }
}
