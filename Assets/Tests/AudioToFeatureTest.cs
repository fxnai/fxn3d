/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.IO;
    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class AudioToFeatureTest : MonoBehaviour {

        [SerializeField] private AudioClip clip;

        private async void Start () {
            var feature = await clip.ToFeature(minUploadSize: 100 * 1024 * 1024); // ensure data URL is created
            using var dataStream = await FunctionUnity.Create().Storage.Download(feature.data);
            using var fileStream = new FileStream("audio.wav", FileMode.OpenOrCreate);
            dataStream.CopyTo(fileStream);
            Debug.Log("Wrote audio feature to file");
        }
    }
}