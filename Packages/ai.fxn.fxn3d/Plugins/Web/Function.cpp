//
//  Function.cpp
//  Function
//
//  Created by Yusuf Olokoba on 3/11/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#define EMSCRIPTEN_HAS_UNBOUND_TYPE_NAMES 0

#include <string>
#include <dlfcn.h>
#include <emscripten.h>
#include <emscripten/val.h>

#define FXN_BRIDGE extern "C"
#define FXN_DECL(symbol) FXN_BRIDGE int symbol(void)
#define FXN_BIND(handle, symbol) FXNBind(handle, #symbol, reinterpret_cast<void*>(&symbol))

FXN_DECL(FXNValueRelease);
FXN_DECL(FXNValueGetData);
FXN_DECL(FXNValueGetType);
FXN_DECL(FXNValueGetDimensions);
FXN_DECL(FXNValueGetShape);
FXN_DECL(FXNValueCreateArray);
FXN_DECL(FXNValueCreateString);
FXN_DECL(FXNValueCreateList);
FXN_DECL(FXNValueCreateDict);
FXN_DECL(FXNValueCreateImage);
FXN_DECL(FXNValueCreateBinary);
FXN_DECL(FXNValueCreateNull);
FXN_DECL(FXNValueMapCreate);
FXN_DECL(FXNValueMapRelease);
FXN_DECL(FXNValueMapGetSize);
FXN_DECL(FXNValueMapGetKey);
FXN_DECL(FXNValueMapGetValue);
FXN_DECL(FXNValueMapSetValue);
FXN_DECL(FXNConfigurationGetUniqueID);
FXN_DECL(FXNConfigurationCreate);
FXN_DECL(FXNConfigurationRelease);
FXN_DECL(FXNConfigurationGetTag);
FXN_DECL(FXNConfigurationSetTag);
FXN_DECL(FXNConfigurationGetToken);
FXN_DECL(FXNConfigurationSetToken);
FXN_DECL(FXNConfigurationGetAcceleration);
FXN_DECL(FXNConfigurationSetAcceleration);
FXN_DECL(FXNConfigurationGetDevice);
FXN_DECL(FXNConfigurationSetDevice);
FXN_DECL(FXNConfigurationAddResource);
FXN_DECL(FXNPredictionRelease);
FXN_DECL(FXNPredictionGetID);
FXN_DECL(FXNPredictionGetLatency);
FXN_DECL(FXNPredictionGetResults);
FXN_DECL(FXNPredictionGetError);
FXN_DECL(FXNPredictionGetLogs);
FXN_DECL(FXNPredictionGetLogLength);
FXN_DECL(FXNPredictorCreate);
FXN_DECL(FXNPredictorRelease);
FXN_DECL(FXNPredictorPredict);
FXN_DECL(FXNGetVersion);

FXN_BRIDGE void FXNBind (void*, const char*, void*);

static void* FXNInitializeWebGL () {
    // Embind
    const auto console = emscripten::val::global("console");
    console.call<void>("log", std::string("Initializing Function WebGL"));
    //const auto name = emscripten::val("document");
    const auto window = emscripten::val::global("window");
    const auto originVal = window["origin"];
    const auto origin = originVal.as<std::string>();
    // Function
    const auto handle = dlopen("libFunction.so", RTLD_LAZY | RTLD_GLOBAL | RTLD_NODELETE);
    FXN_BIND(handle, FXNValueRelease);
    FXN_BIND(handle, FXNValueGetData);
    FXN_BIND(handle, FXNValueGetType);
    FXN_BIND(handle, FXNValueGetDimensions);
    FXN_BIND(handle, FXNValueGetShape);
    FXN_BIND(handle, FXNValueCreateArray);
    FXN_BIND(handle, FXNValueCreateString);
    FXN_BIND(handle, FXNValueCreateList);
    FXN_BIND(handle, FXNValueCreateDict);
    FXN_BIND(handle, FXNValueCreateImage);
    FXN_BIND(handle, FXNValueCreateBinary);
    FXN_BIND(handle, FXNValueCreateNull);
    FXN_BIND(handle, FXNValueMapCreate);
    FXN_BIND(handle, FXNValueMapRelease);
    FXN_BIND(handle, FXNValueMapGetSize);
    FXN_BIND(handle, FXNValueMapGetKey);
    FXN_BIND(handle, FXNValueMapGetValue);
    FXN_BIND(handle, FXNValueMapSetValue);
    FXN_BIND(handle, FXNConfigurationGetUniqueID);
    FXN_BIND(handle, FXNConfigurationCreate);
    FXN_BIND(handle, FXNConfigurationRelease);
    FXN_BIND(handle, FXNConfigurationGetTag);
    FXN_BIND(handle, FXNConfigurationSetTag);
    FXN_BIND(handle, FXNConfigurationGetToken);
    FXN_BIND(handle, FXNConfigurationSetToken);
    FXN_BIND(handle, FXNConfigurationGetAcceleration);
    FXN_BIND(handle, FXNConfigurationSetAcceleration);
    FXN_BIND(handle, FXNConfigurationGetDevice);
    FXN_BIND(handle, FXNConfigurationSetDevice);
    FXN_BIND(handle, FXNConfigurationAddResource);
    FXN_BIND(handle, FXNPredictionRelease);
    FXN_BIND(handle, FXNPredictionGetID);
    FXN_BIND(handle, FXNPredictionGetLatency);
    FXN_BIND(handle, FXNPredictionGetResults);
    FXN_BIND(handle, FXNPredictionGetError);
    FXN_BIND(handle, FXNPredictionGetLogs);
    FXN_BIND(handle, FXNPredictionGetLogLength);
    FXN_BIND(handle, FXNPredictorCreate);
    FXN_BIND(handle, FXNPredictorRelease);
    FXN_BIND(handle, FXNPredictorPredict);
    FXN_BIND(handle, FXNGetVersion);
    return nullptr;
}

extern "C" void* FXN_WEBGL_INIT EMSCRIPTEN_KEEPALIVE = FXNInitializeWebGL(); // __ATINIT__
extern "C" void* FXN_WEBGL_PUSH EMSCRIPTEN_KEEPALIVE = 0x0;
extern "C" void* FXN_WEBGL_POP EMSCRIPTEN_KEEPALIVE = 0x0;