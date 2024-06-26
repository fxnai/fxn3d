//
//  FXNConfiguration.h
//  Function
//
//  Created by Yusuf Olokoba on 5/27/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <Function/FXNStatus.h>

#pragma region --Enumerations--
/*!
 @enum FXNAcceleration

 @abstract Hardware acceleration used for predictions.

 @constant FXN_ACCELERATION_DEFAULT
 Use the default acceleration for the given platform.

 @constant FXN_ACCELERATION_CPU
 Use the CPU.
 
 @constant FXN_ACCELERATION_GPU
 Use the GPU.
 
 @constant FXN_ACCELERATION_NPU
 Use the neural processing unit.
*/
enum FXNAcceleration {
    FXN_ACCELERATION_DEFAULT  = 0,
    FXN_ACCELERATION_CPU      = 1 << 0,
    FXN_ACCELERATION_GPU      = 1 << 1,
    FXN_ACCELERATION_NPU      = 1 << 2,
};
typedef enum FXNAcceleration FXNAcceleration;
#pragma endregion


#pragma region --Types--
/*!
 @struct FXNConfiguration

 @abstract Opaque type for predictor configuration.
*/
struct FXNConfiguration;
typedef struct FXNConfiguration FXNConfiguration;

/*!
 @typedef FXNConfigurationAddResourceHandler

 @abstract Callback invoked when a configuration resource has been loaded.
 
 @param context
 User context.

 @param status
 Result status.
*/
typedef void (*FXNConfigurationAddResourceHandler) (
    void* context,
    FXNStatus status
);
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNConfigurationGetUniqueID

 @abstract Get the configuration unique identifier.

 @discussion Get the configuration unique identifier.

 @param identifier
 Configuration unique identifier.

 @param size
 Identifier size.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetUniqueID (
    char* identifier,
    int32_t size
);

/*!
 @function FXNConfigurationGetClientID

 @abstract Get the configuration client identifier.

 @discussion Get the configuration client identifier.

 @param identifier
 Configuration client identifier.

 @param size
 Identifier size.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetClientID (
    char* identifier,
    int32_t size
);

/*!
 @function FXNConfigurationCreate

 @abstract Create a predictor configuration.

 @discussion Create a predictor configuration.

 @param configuration
 Created configuration. MUST NOT be `NULL`.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationCreate (FXNConfiguration** configuration);

/*!
 @function FXNConfigurationRelease

 @abstract Release the predictor configuration.

 @discussion Release the predictor configuration.

 @param configuration
 Predictor configuration.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationRelease (FXNConfiguration* configuration);
#pragma endregion


#pragma region --Configuration--
/*!
 @function FXNConfigurationGetTag

 @abstract Get the predictor tag.

 @discussion Get the predictor tag.

 @param configuration
 Predictor configuration.

 @param tag
 Destination buffer.

 @param size
 Size of destination buffer.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetTag (
    FXNConfiguration* configuration,
    char* tag,
    int32_t size
);

/*!
 @function FXNConfigurationSetTag

 @abstract Set the predictor tag.

 @discussion Set the predictor tag.
 This is requried for Function to load the predictor.

 @param configuration
 Predictor configuration.

 @param tag
 Predictor tag.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationSetTag (
    FXNConfiguration* configuration,
    const char* tag
);

/*!
 @function FXNConfigurationGetToken

 @abstract Get the configuration token.

 @discussion Get the configuration token.

 @param configuration
 Predictor configuration.

 @param token
 Destination buffer.

 @param size
 Size of destination buffer.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetToken (
    FXNConfiguration* configuration,
    char* token,
    int32_t size
);

/*!
 @function FXNConfigurationSetToken

 @abstract Set the configuration token.

 @discussion Set the configuration token.
 This is requried for Function to load the predictor.

 @param configuration
 Predictor configuration.

 @param token
 Configuration token.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationSetToken (
    FXNConfiguration* configuration,
    const char* token
);

/*!
 @function FXNConfigurationGetAcceleration

 @abstract Get the acceleration used for making predictions.

 @discussion Get the acceleration used for making predictions.

 @param configuration
 Predictor configuration.

 @param acceleration
 Acceleration.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetAcceleration (
    FXNConfiguration* configuration,
    FXNAcceleration* acceleration
);

/*!
 @function FXNConfigurationSetAcceleration

 @abstract Specify the acceleration used for making predictions.

 @discussion Specify the acceleration used for making predictions.

 @param configuration
 Predictor configuration.

 @param acceleration
 Acceleration used for making predictions predictions.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationSetAcceleration (
    FXNConfiguration* configuration,
    FXNAcceleration acceleration
);

/*!
 @function FXNConfigurationGetDevice

 @abstract Get the compute device used for ML model inference.

 @discussion Get the compute device used for ML model inference.

 @param configuration
 Predictor configuration.

 @param device
 Compute device used for ML model inference.
 The type of this device is platform-dependent (see https://docs.fxn.ai).
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationGetDevice (
    FXNConfiguration* configuration,
    void** device
);

/*!
 @function FXNConfigurationSetDevice

 @abstract Specify the compute device used for ML model inference.

 @discussion Specify the compute device used for ML model inference.

 @param configuration
 Predictor configuration.

 @param device
 Compute device used for ML model inference.
 The type of this device is platform-dependent (see https://docs.fxn.ai).
 Pass `NULL` to use the default compute device.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationSetDevice (
    FXNConfiguration* configuration,
    void* device
);

/*!
 @function FXNConfigurationAddResource

 @abstract Add a prediction resource.

 @discussion Add a prediction resource.

 @param configuration
 Predictor configuration.

 @param type
 Resource type.

 @param path
 Resource path.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNConfigurationAddResource (
    FXNConfiguration* configuration,
    const char* type,
    const char* path
);
#pragma endregion