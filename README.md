# Function for Unity Engine

![function logo](https://raw.githubusercontent.com/fxnai/.github/main/logo_wide.png)

[![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fdiscord.com%2Fapi%2Finvites%2Fy5vwgXkz2f%3Fwith_counts%3Dtrue&query=%24.approximate_member_count&logo=discord&logoColor=white&label=Function%20community)](https://fxn.ai/community)

Run Python functions (a.k.a "predictors") locally in your Unity apps and games, with full GPU acceleration and zero dependencies.

> [!TIP]
> [Join our waitlist](https://fxn.ai/waitlist) to bring your custom Python functions and run them on-device across Android, iOS, macOS, Linux, web, and Windows.

## Installing Function
Add the following items to your Unity project's `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "Function",
      "url": "https://registry.npmjs.com",
      "scopes": ["ai.fxn"]
    }
  ],
  "dependencies": {
    "ai.fxn.fxn3d": "0.0.27"
  }
}
```

## Retrieving your Access Key
Head over to [fxn.ai](https://fxn.ai) to create an account by logging in. Once you do, generate an access key:

![generate access key](https://raw.githubusercontent.com/fxnai/.github/main/access_key.gif)

Then add it to your Unity project in `Project Settings > Function`:

![add access key to Unity](settings.gif)

> [!CAUTION]
> If your Unity project is open-source, make sure to add `ProjectSettings/Function.asset` to your `.gitignore` file to keep your Function access key private.

## Making a Prediction
First, create a Function client:
```csharp
using Function;

// Create a Function client
var fxn = FunctionUnity.Create();
```

Then make a prediction:
```csharp
// Make a prediction
var prediction = await fxn.Predictions.Create(
    tag: "@fxn/greeting",
    inputs: new () { ["name"] = "Roberta" }
);
// Log the result
Debug.Log(prediction.results[0]);
```

___

## Requirements
- Unity 2021.3+

## Supported Platforms
- Android API Level 24+
- iOS 14+
- macOS 10.15+ (Apple Silicon and Intel)
- Windows 10+ (64-bit only)
- WebGL:
  - Chrome 91+
  - Firefox 90+
  - Safari 16.4+

## Useful Links
- [Discover predictors to use in your apps](https://fxn.ai/explore).
- [Join our Discord community](https://fxn.ai/community).
- [Check out our docs](https://docs.fxn.ai).
- Learn more about us [on our blog](https://blog.fxn.ai).
- Reach out to us at [hi@fxn.ai](mailto:hi@fxn.ai).

Thank you very much!
