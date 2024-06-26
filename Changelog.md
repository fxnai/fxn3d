## 0.0.22
+ Fixed Function API web requests failing due to internet unreachability errors on iOS.
+ Fixed WebGL build errors on Unity 2023.

## 0.0.21
+ Fixed edge prediction support on WebGL.
+ Fixed duplicate interface compiler errors when project depends on `Microsoft.Bcl.AsyncInterfaces` library.
+ Updated `fxn.Predictions.ToObject` method to return an `Image` for image values instead of a `Value`.
+ Updated `FunctionUnity.ToTexture` extension method to accept an `Image` instead of a `Value`.

## 0.0.20
+ Fixed build errors on WebGL.
+ Updated `fxn.Predictions.ToObject` method to return a `Newtonsoft.Json.Linq.JArray` insteaf of a `List<object>` for list values.
+ Updated `fxn.Predictions.ToObject` method to return a `Newtonsoft.Json.Linq.JObject` insteaf of a `Dictionary<string, object>` for dictionary values.

## 0.0.19
+ Added `PrivacyInfo.xcprivacy` iOS privacy manifest in `Function.framework`.
+ Fixed `InvalidOperationException` when edge predictions return image values.
+ Fixed Apple App Store upload errors due to incorrect `CFBundleVersion` key in `Function.framework`.
+ Fixed embedded edge predictors failing to load from cache causing unnecessary downloads.
+ Fixed Android build errors when embedding edge predictors.
+ Updated to Function C 0.0.23.

## 0.0.18
+ Fixed edge predictions failing when passing in `Dictionary<TKey, TValue>` input values.
+ Updated to Function C 0.0.20.

## 0.0.17
+ Fixed edge prediction errors caused by request backpressure while the predictor is being loaded.

## 0.0.16
+ Added support for `Enum` input values in `Function.Predictions.Create` method.
+ Fixed JSON deserialization errors caused by code stripping in builds.
+ Removed `Function.Types.Converters` namespace. Bring your own JSON converters.

## 0.0.15
+ Minor stability improvements.

## 0.0.14
+ Fixed Apple App Store app rejections due to missing Bundle Version key in Function.framework.
+ Updated to Function C 0.0.18.

## 0.0.13
+ Added support for Unity 2021 LTS.
+ Updated `FunctionUnity.ToImage` extension method to accept an optional buffer to avoid allocating memory.
+ Removed `FunctionUnity.ToValue(Texture2D)` extension method. Use `FunctionUnity.ToImage` method instead.

## 0.0.12
+ Added `Function.Types.Image` struct for making edge predictions on images.
+ Added `FunctionUnity.ToImage(Texture2D)` helper function for creating an image from a `Texture2D`.

## 0.0.11
+ Fixed WebGL build error when building in Release mode due to JavaScript minification.

## 0.0.10
+ Fixed WebGL build error when Function is installed with Unity Package Manager.

## 0.0.9
+ Added support for making on-device predictions on Android.
+ Added support for making on-device predictions on iOS.
+ Added support for making on-device predictions on macOS.
+ Added support for making on-device predictions on WebGL.
+ Added support for making on-device predictions on Windows.

## 0.0.8
+ Fixed linker errors when compiling iOS projects in Xcode.

## 0.0.7
+ Fixed compiler error on some platforms related to Function version.

## 0.0.6
+ Fixed `NullReferenceException` when calling `Tag.TryParse` with `null` input string.
+ Removed `CloudPrediction` class. Use `Prediction` class instead.
+ Removed `EdgePrediction` class. Use `Prediction` class instead.

## 0.0.5
+ Added `Function.Predictions.Stream` method for making streaming predictions.
+ Refactored `IGraphClient` interface to `IFunctionClient`.
+ Refactored `UnityGraphClient` class to `UnityClient`.
+ Refactored `Function.Graph` namespace to `Function.API`.

## 0.0.4
+ Minor updates.

## 0.0.3
+ Refactored `Predictor.readme` field to `card`.
+ Refactored `Dtype.Undefined` enumeration member to `Dtype.Null`.

## 0.0.2
+ Added `FunctionUnity.ToAudioClip` extension method for converting a Function `Value` to an `AudioClip`.
+ Fixed Function access key not being available in builds.
+ Refactored `Feature` class to `Value` for improved clarity.
+ Refactored `Function.Predictions.ToFeature` method to `Function.Predictions.ToValue`.
+ Refactored `Function.Predictions.ToValue` method to `Function.Predictions.ToObject`.
+ Refactored `FunctionUnity.ToFeature` extension method to `FunctionUnity.ToValue`.
+ Refactored `UploadType.Feature` enumeration member to `UploadType.Value`.

## 0.0.1
+ First pre-release.