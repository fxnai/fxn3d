/* 
*   Function
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

const FXNC = {

  $_throw_fxn_binding_error: function () {
    throw new Error("Function Error: Binding for Function library function is missing. Send console logs to hi@fxn.ai");
  },
  $_get_fxn: function (symName, require=true) {
    const fxnc = LDSO.loadedLibsByName["libFunction.so"];
    const fxn = fxnc ? fxnc.module[symName] : null;
    return fxn || !require ? fxn : _throw_fxn_binding_error();
  },
  FXNValueRelease: function (value) {
    return _get_fxn("FXNValueRelease")(value);
  },
  FXNValueGetData: function (value, data) {
    return _get_fxn("FXNValueGetData")(value, data);
  },
  FXNValueGetType: function (value, type) {
    return _get_fxn("FXNValueGetType")(value, type);
  },
  FXNValueGetDimensions: function (value, dimensions) {
    return _get_fxn("FXNValueGetDimensions")(value, dimensions);
  },
  FXNValueGetShape: function (value, shape, shapeLen) {
    return _get_fxn("FXNValueGetShape")(value, shape, shapeLen);
  },
  FXNValueCreateArray: function (data, shape, dims, dtype, flags, value) {
    return _get_fxn("FXNValueCreateArray")(data, shape, dims, dtype, flags, value);
  },
  FXNValueCreateString: function (data, value) {
    return _get_fxn("FXNValueCreateString")(data, value);
  },
  FXNValueCreateList: function (data, value) {
    return _get_fxn("FXNValueCreateList")(data, value);
  },
  FXNValueCreateDict: function (data, value) {
    return _get_fxn("FXNValueCreateDict")(data, value);
  },
  FXNValueCreateImage: function (pixelBuffer, width, height, channels, flags, value) {
    return _get_fxn("FXNValueCreateImage")(pixelBuffer, width, height, channels, flags, value);
  },
  FXNValueCreateBinary: function (buffer, bufferLen, flags, value) {
    return _get_fxn("FXNValueCreateBinary")(buffer, bufferLen, flags, value);
  },
  FXNValueCreateNull: function (value) {
    return _get_fxn("FXNValueCreateNull")(value);
  },
  FXNValueCreateBySerializingValue: function (value, flags, result) {
    return _get_fxn("FXNValueCreateBySerializingValue")(value, flags, result);
  },
  FXNValueCreateByDeserializingValue: function (value, type, flags, result) {
    return _get_fxn("FXNValueCreateByDeserializingValue")(value, type, flags, result);
  },
  FXNValueMapCreate: function (map) {
    return _get_fxn("FXNValueMapCreate")(map);
  },
  FXNValueMapRelease: function (map) {
    return _get_fxn("FXNValueMapRelease")(map);
  },
  FXNValueMapGetSize: function (map, size) {
    return _get_fxn("FXNValueMapGetSize")(map, size);
  },
  FXNValueMapGetKey: function (map, index, key, size) {
    return _get_fxn("FXNValueMapGetKey")(map, index, key, size);
  },
  FXNValueMapGetValue: function (map, key, value) {
    return _get_fxn("FXNValueMapGetValue")(map, key, value);
  },
  FXNValueMapSetValue: function (map, key, value) {
    return _get_fxn("FXNValueMapSetValue")(map, key, value);
  },
  FXNConfigurationGetUniqueID: function (identifier, size) {
    return _get_fxn("FXNConfigurationGetUniqueID")(identifier, size);
  },
  FXNConfigurationCreate: function (configuration) {
    return _get_fxn("FXNConfigurationCreate")(configuration);
  },
  FXNConfigurationRelease: function (configuration) {
    return _get_fxn("FXNConfigurationRelease")(configuration);
  },
  FXNConfigurationGetTag: function (configuration, tag, size) {
    return _get_fxn("FXNConfigurationGetTag")(configuration, tag, size);
  },
  FXNConfigurationSetTag: function (configuration, tag) {
    return _get_fxn("FXNConfigurationSetTag")(configuration, tag);
  },
  FXNConfigurationGetToken: function (configuration, token, size) {
    return _get_fxn("FXNConfigurationGetToken")(configuration, token, size);
  },
  FXNConfigurationSetToken: function (configuration, token) {
    return _get_fxn("FXNConfigurationSetToken")(configuration, token);
  },
  FXNConfigurationGetAcceleration: function (configuration, acceleration) {
    return _get_fxn("FXNConfigurationGetAcceleration")(configuration, acceleration);
  },
  FXNConfigurationSetAcceleration: function (configuration, acceleration) {
    return _get_fxn("FXNConfigurationSetAcceleration")(configuration, acceleration);
  },
  FXNConfigurationGetDevice: function (configuration, device) {
    return _get_fxn("FXNConfigurationGetDevice")(configuration, device);
  },
  FXNConfigurationSetDevice: function (configuration, device) {
    return _get_fxn("FXNConfigurationSetDevice")(configuration, device);
  },
  FXNConfigurationAddResource: function (configuration, type, path) {
    return _get_fxn("FXNConfigurationAddResource")(configuration, type, path);
  },
  FXNPredictionRelease: function (prediction) {
    return _get_fxn("FXNPredictionRelease")(prediction);
  },
  FXNPredictionGetID: function (prediction, destination, size) {
    return _get_fxn("FXNPredictionGetID")(prediction, destination, size);
  },
  FXNPredictionGetLatency: function (prediction, latency) {
    return _get_fxn("FXNPredictionGetLatency")(prediction, latency);
  },
  FXNPredictionGetResults: function (prediction, map) {
    return _get_fxn("FXNPredictionGetResults")(prediction, map);
  },
  FXNPredictionGetError: function (prediction, error, size) {
    return _get_fxn("FXNPredictionGetError")(prediction, error, size);
  },
  FXNPredictionGetLogs: function (prediction, logs, size) {
    return _get_fxn("FXNPredictionGetLogs")(prediction, logs, size);
  },
  FXNPredictionGetLogLength: function (prediction, length) {
    return _get_fxn("FXNPredictionGetLogLength")(prediction, length);
  },
  FXNPredictionStreamRelease: function (stream) {
    return _get_fxn("FXNPredictionStreamRelease")(stream);
  },
  FXNPredictionStreamReadNext: function (stream, prediction) {
    return _get_fxn("FXNPredictionStreamReadNext")(stream, prediction);
  },
  FXNPredictorCreate: function (configuration, predictor) {
    return _get_fxn("FXNPredictorCreate")(configuration, predictor);
  },
  FXNPredictorRelease: function (predictor) {
    return _get_fxn("FXNPredictorRelease")(predictor);
  },
  FXNPredictorCreatePrediction: function (predictor, inputs, prediction) {
    return _get_fxn("FXNPredictorCreatePrediction")(predictor, inputs, prediction);
  },
  FXNPredictorStreamPrediction: function (predictor, inputs, stream) {
    return _get_fxn("FXNPredictorStreamPrediction")(predictor, inputs, stream);
  },
  FXNGetVersion: function () {
    return _get_fxn("FXNGetVersion")();
  },
  FXNBind: function (handle, sym, ptr) {
    const symName = UTF8ToString(sym);
    const fxn = _get_fxn(symName, false);
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
    var _emscripten_memcpy_js = (dest, src, num) => HEAPU8.copyWithin(dest, src, src + num);
    _emscripten_console_log.sig = "vi";
    _emscripten_console_warn.sig = "vi";
    _emscripten_console_error.sig = "vi";
    _emscripten_err.sig = "vi";
    _emscripten_out.sig = "vi";
    __emscripten_get_progname.sig = "vii";
    _emscripten_memcpy_js.sig = "viii";
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
      emscripten_memcpy_js: _emscripten_memcpy_js,
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