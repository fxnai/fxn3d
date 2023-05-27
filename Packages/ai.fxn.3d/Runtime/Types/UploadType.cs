/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Types {

    /// <summary>
    /// Upload URL type.
    /// </summary>
    public enum UploadType : int {
        /// <summary>
        /// Prediction feature.
        /// </summary>
        Feature     = 1,
        /// <summary>
        /// Predictor media.
        /// </summary>
        Media       = 2,
        /// <summary>
        /// Predictor notebook.
        /// </summary>
        Notebook    = 3,
    }
}