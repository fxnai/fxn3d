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