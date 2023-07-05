/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;

    internal sealed class AudioToValueTest : MonoBehaviour {

        [SerializeField] private AudioClip clip;

        private async void Start () {
            var value = await clip.ToValue(minUploadSize: 100 * 1024 * 1024); // ensure data URL is created
            var output = await value.ToAudioClip();
            AudioSource.PlayClipAtPoint(output, Vector3.zero);
        }
    }
}