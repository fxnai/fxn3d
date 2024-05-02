/* 
*   Function
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

const FXNC = {

  $_throw_fxn_binding_error: function () {
    throw new Error("Function Error: Binding for Function library function is missing. Send console logs to hi@fxn.ai");
  },
  $_get_fxn: function (symName) {
    const fxnc = LDSO.loadedLibsByName["libFunction.so"];
    const fxn = fxnc ? fxnc.module[symName] : null;
    return fxn;
  },
  FXNValueRelease: function (value) {
    _throw_fxn_binding_error();
  },
  FXNValueGetData: function (value, data) {
    _throw_fxn_binding_error();
  },
  FXNValueGetType: function (value, type) {
    _throw_fxn_binding_error();
  },
  FXNValueGetDimensions: function (value, dimensions) {
    _throw_fxn_binding_error();
  },
  FXNValueGetShape: function (value, shape, shapeLen) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateArray: function (data, shape, dims, dtype, flags, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateString: function (data, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateList: function (data, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateDict: function (data, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateImage: function (pixelBuffer, width, height, channels, flags, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateBinary: function (buffer, bufferLen, flags, value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateNull: function (value) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateBySerializingValue: function (value, flags, result) {
    _throw_fxn_binding_error();
  },
  FXNValueCreateByDeserializingValue: function (value, type, flags, result) {
    _throw_fxn_binding_error();
  },
  FXNValueMapCreate: function (map) {
    _throw_fxn_binding_error();
  },
  FXNValueMapRelease: function (map) {
    _throw_fxn_binding_error();
  },
  FXNValueMapGetSize: function (map, size) {
    _throw_fxn_binding_error();
  },
  FXNValueMapGetKey: function (map, index, key, size) {
    _throw_fxn_binding_error();
  },
  FXNValueMapGetValue: function (map, key, value) {
    _throw_fxn_binding_error();
  },
  FXNValueMapSetValue: function (map, key, value) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationGetUniqueID: function (identifier, size) {
    const fxn = _get_fxn("FXNConfigurationGetUniqueID");
    return fxn ? fxn(identifier, size) : _throw_fxn_binding_error();
  },
  FXNConfigurationCreate: function (configuration) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationRelease: function (configuration) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationGetTag: function (configuration, tag, size) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationSetTag: function (configuration, tag) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationGetToken: function (configuration, token, size) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationSetToken: function (configuration, token) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationGetAcceleration: function (configuration, acceleration) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationSetAcceleration: function (configuration, acceleration) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationGetDevice: function (configuration, device) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationSetDevice: function (configuration, device) {
    _throw_fxn_binding_error();
  },
  FXNConfigurationAddResource: function (configuration, type, path) {
    _throw_fxn_binding_error();
  },
  FXNPredictionRelease: function (prediction) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetID: function (prediction, destination, size) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetLatency: function (prediction, latency) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetResults: function (prediction, map) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetError: function (prediction, error, size) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetLogs: function (prediction, logs, size) {
    _throw_fxn_binding_error();
  },
  FXNPredictionGetLogLength: function (prediction, length) {
    _throw_fxn_binding_error();
  },
  FXNPredictorCreate: function (configuration, predictor) {
    _throw_fxn_binding_error();
  },
  FXNPredictorRelease: function (predictor) {
    _throw_fxn_binding_error();
  },
  FXNPredictorPredict: function (predictor, inputs, prediction) {
    _throw_fxn_binding_error();
  },
  FXNGetVersion: function () {
    const fxn = _get_fxn("FXNGetVersion");
    return fxn ? fxn() : _throw_fxn_binding_error();
  },
  FXNBind: function (handle, sym, ptr) {
    const symName = UTF8ToString(sym);
    const fxn = _get_fxn(symName);
    if (fxn) // don't break app if this fails
      wasmTable.set(ptr, fxn);
  },
  FXNBind__postset: `addOnPreRun(() => {
    var ___c_longjmp = new WebAssembly.Tag({ "parameters": ["i32"] });
    var ___cpp_exception = new WebAssembly.Tag({ "parameters": ["i32"] });
    var _emscripten_console_log = (str) => console.log(UTF8ToString(str));
    var _emscripten_console_warn = (str) => console.warn(UTF8ToString(str));
    var _emscripten_console_error = (str) => console.error(UTF8ToString(str));
    var _emscripten_err = (str) => err(UTF8ToString(str));
    var _emscripten_out = (str) => out(UTF8ToString(str));
    var __emscripten_get_progname = (str, len) => stringToUTF8(thisProgram, str, len);
    _emscripten_console_log.sig = "vi";
    _emscripten_console_warn.sig = "vi";
    _emscripten_console_error.sig = "vi";
    _emscripten_err.sig = "vi";
    _emscripten_out.sig = "vi";
    __emscripten_get_progname.sig = "vii";
    const fxnImports = {
      __c_longjmp: ___c_longjmp,
      __cpp_exception: ___cpp_exception,
      __heap_base: 1,
      emscripten_console_log: _emscripten_console_log,
      emscripten_console_warn: _emscripten_console_warn,
      emscripten_console_error: _emscripten_console_error,
      emscripten_err: _emscripten_err,
      emscripten_out: _emscripten_out,
      _emscripten_get_progname: __emscripten_get_progname,
    };
    if (typeof wasmImports !== "undefined")
      wasmImports = Object.assign({}, wasmImports, fxnImports); 
    else if (typeof asmLibraryArg !== "undefined")
      asmLibraryArg = Object.assign({}, asmLibraryArg, fxnImports); 
  })`,
};

autoAddDeps(FXNC, "$_throw_fxn_binding_error");
autoAddDeps(FXNC, "$_get_fxn");
mergeInto(LibraryManager.library, FXNC);