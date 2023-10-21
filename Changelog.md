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