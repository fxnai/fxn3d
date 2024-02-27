//
//  FXNAPI.h
//  Function
//
//  Created by Yusuf Olokoba on 5/27/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#ifdef __cplusplus
    #define FXN_BRIDGE extern "C"
#else
    #define FXN_BRIDGE extern
#endif

#ifdef _WIN64
    #define FXN_EXPORT __declspec(dllexport)
#else
    #define FXN_EXPORT __attribute__((visibility("default")))
#endif

#ifdef __EMSCRIPTEN__
    #define FXN_API EMSCRIPTEN_KEEPALIVE
#else
    #define FXN_API
#endif