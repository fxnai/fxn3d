//
//  FXNVersion.h
//  Function
//
//  Created by Yusuf Olokoba on 10/14/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <Function/FXNAPI.h>

#define FXN_VERSION_MAJOR 0
#define FXN_VERSION_MINOR 0
#define FXN_VERSION_PATCH 20

/*!
 @function FXNGetVersion

 @abstract Get the Function version.

 @discussion Get the Function version.

 @returns Function version string.
*/
FXN_BRIDGE FXN_EXPORT const char* FXN_API FXNGetVersion ();