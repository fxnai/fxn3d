//
//  API.hpp
//  Function
//
//  Created by Yusuf Olokoba on 1/30/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <vector>
#include <Function/cxx/Assert.hpp>
#include <Function/cxx/Configuration.hpp>
#include <Function/cxx/Value.hpp>

#ifdef _WIN64
    #include <windows.h>
#endif

namespace Function {

#pragma region --Configuration--

inline Configuration::Configuration () : configuration(nullptr) {
    FXNConfigurationCreate(&configuration);
    owner = true;
}

inline Configuration::Configuration (const std::string& tag) : Configuration() {
    SetTag(tag);
}

inline Configuration::Configuration (FXNConfiguration* value, bool owner) noexcept : configuration(value), owner(owner) {

}

inline Configuration::Configuration (Configuration&& other) noexcept : configuration(other.configuration), owner(other.owner) {
    other.configuration = nullptr;
    other.owner = false;
}

inline Configuration::~Configuration () {
    if (owner)
        FXNConfigurationRelease(configuration);
}

inline Configuration& Configuration::operator= (Configuration&& other) noexcept {
    if (this == &other)
        return *this;
    if (owner)
        FXNConfigurationRelease(configuration);
    configuration = nullptr;
    owner = false;
    std::swap(configuration, other.configuration);
    std::swap(owner, other.owner);
    return *this;
}

inline std::string Configuration::GetTag () const {
    std::string tag;
    tag.resize(1024);
    FXNConfigurationGetTag(configuration, tag.data(), static_cast<int>(tag.size()));
    tag.resize(strlen(tag.c_str()));
    return tag;
}

inline void Configuration::SetTag (const std::string& tag) const {
    FXNConfigurationSetTag(configuration, tag.c_str());
}

inline std::string Configuration::GetToken () const {
    std::string token;
    token.resize(4096);
    FXNConfigurationGetToken(configuration, token.data(), static_cast<int>(token.size()));
    token.resize(strlen(token.c_str()));
    return token;
}

inline void Configuration::SetToken (const std::string& token) const {
    FXNConfigurationSetToken(configuration, token.c_str());
}

inline FXNAcceleration Configuration::GetAcceleration () const {
    FXNAcceleration acceleration = FXN_ACCELERATION_DEFAULT;
    FXNConfigurationGetAcceleration(configuration, &acceleration);
    return acceleration;
}

inline void Configuration::SetAcceleration (FXNAcceleration acceleration) const {
    FXNConfigurationSetAcceleration(configuration, acceleration);
}

inline void* Configuration::GetDevice () const {
    void* device = nullptr;
    FXNConfigurationGetDevice(configuration, &device);
    return device;
}

template<typename T>
inline T* Configuration::GetDevice () const {
    return static_cast<T*>(GetDevice());
}

template<typename T>
inline void Configuration::SetDevice (T* device) const {
    FXNConfigurationSetDevice(configuration, device);
}

inline void Configuration::AddResource (const std::string& type, const std::filesystem::path& path) const {
    #ifdef _WIN32
    std::wstring wide_path_str = path.wstring();
    int utf8_size = WideCharToMultiByte(CP_UTF8, 0, wide_path_str.c_str(), -1, nullptr, 0, nullptr, nullptr);
    std::string utf8_path_str(utf8_size, 0);
    WideCharToMultiByte(CP_UTF8, 0, wide_path_str.c_str(), -1, &utf8_path_str[0], utf8_size, nullptr, nullptr);
    utf8_path_str.resize(utf8_size - 1);
    #else
    std::string utf8_path_str = path.string();
    #endif
    FXNConfigurationAddResource(configuration, type.c_str(), utf8_path_str.c_str());
}

inline Configuration::operator FXNConfiguration* () const {
    return configuration;
}

inline std::string Configuration::GetUniqueID () {
    std::string id;
    id.resize(1024);
    FXNConfigurationGetUniqueID(id.data(), static_cast<int>(id.size()));
    id.resize(strlen(id.c_str()));
    return id;
}
#pragma endregion


#pragma region --Value--

template <typename T>
struct ToFXNDtype;
template <>
struct ToFXNDtype<float> {
    static constexpr FXNDtype type = FXN_DTYPE_FLOAT32;
};
template <>
struct ToFXNDtype<double> {
    static constexpr FXNDtype type = FXN_DTYPE_FLOAT64;
};
template <>
struct ToFXNDtype<int8_t> {
    static constexpr FXNDtype type = FXN_DTYPE_INT8;
};
template <>
struct ToFXNDtype<int16_t> {
    static constexpr FXNDtype type = FXN_DTYPE_INT16;
};
template <>
struct ToFXNDtype<int32_t> {
    static constexpr FXNDtype type = FXN_DTYPE_INT32;
};
template <>
struct ToFXNDtype<int64_t> {
    static constexpr FXNDtype type = FXN_DTYPE_INT64;
};
template <>
struct ToFXNDtype<uint8_t> {
    static constexpr FXNDtype type = FXN_DTYPE_UINT8;
};
template <>
struct ToFXNDtype<uint16_t> {
    static constexpr FXNDtype type = FXN_DTYPE_UINT16;
};
template <>
struct ToFXNDtype<uint32_t> {
    static constexpr FXNDtype type = FXN_DTYPE_UINT32;
};
template <>
struct ToFXNDtype<uint64_t> {
    static constexpr FXNDtype type = FXN_DTYPE_UINT64;
};
template <>
struct ToFXNDtype<bool> {
    static constexpr FXNDtype type = FXN_DTYPE_BOOL;
};

inline Value::Value (FXNValue* value, bool owner) noexcept : value(value), owner(owner) {

}

inline Value::Value (Value&& other) noexcept : value(other.value), owner(other.owner) {
    other.value = nullptr;
    other.owner = false;
}

inline Value::~Value () {
    if (owner)
        FXNValueRelease(value);
}

inline Value& Value::operator= (Value&& other) noexcept {
    if (this == &other)
        return *this;
    if (owner)
        FXNValueRelease(value);
    value = nullptr;
    owner = false;
    std::swap(value, other.value);
    std::swap(owner, other.owner);
    return *this;
}

inline void* Value::GetData () const {
    void* data = nullptr;
    FXNValueGetData(value, &data);
    return data;
}

template<typename T>
inline T* Value::GetData () const {
    return static_cast<T*>(GetData());
}

inline FXNDtype Value::GetType () const {
    FXNDtype type = FXN_DTYPE_NULL;
    FXNValueGetType(value, &type);
    return type;
}

inline int32_t Value::GetDimensions () const {
    int32_t dimensions = 0;
    FXNValueGetDimensions(value, &dimensions);
    return dimensions;
}

inline std::vector<int32_t> Value::GetShape () const {
    int32_t dimensions = GetDimensions();
    std::vector<int32_t> shape(dimensions);
    if (dimensions > 0)
        FXNValueGetShape(value, shape.data(), dimensions);
    return shape;
}

inline FXNValue* Value::Release () {
    auto v = value;
    value = nullptr;
    owner = false;
    return v;
}

inline Value::operator FXNValue* () const {
    return value;
}

template<typename T>
inline Value Value::CreateArray (const std::vector<int32_t>& shape) {
    return CreateArray<T>(nullptr, shape, FXN_VALUE_FLAG_NONE);
}

template<typename T>
inline Value Value::CreateArray (T* data, const std::vector<int32_t>& shape, FXNValueFlags flags) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateArray(data, shape.data(), static_cast<int32_t>(shape.size()), ToFXNDtype<T>::type, flags, &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateString (const std::string& str) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateString(str.c_str(), &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateList (const std::string& jsonList) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateList(jsonList.c_str(), &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateDict (const std::string& jsonDict) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateDict(jsonDict.c_str(), &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateImage (const uint8_t* pixelBuffer, int32_t width, int32_t height, int32_t channels, FXNValueFlags flags) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateImage(pixelBuffer, width, height, channels, flags, &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateBinary (void* buffer, int64_t bufferLen, FXNValueFlags flags) {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateBinary(buffer, bufferLen, flags, &fxnValue);
    return Value(fxnValue);
}

inline Value Value::CreateNull () {
    FXNValue* fxnValue = nullptr;
    FXNValueCreateNull(&fxnValue);
    return Value(fxnValue);
}
#pragma endregion


#pragma region --ValueMap--

inline ValueMap::ValueMap () : map(nullptr) {
    FXNValueMapCreate(&map);
    owner = true;
}

inline ValueMap::ValueMap (FXNValueMap* map, bool owner) noexcept : map(map), owner(owner) {

}

inline ValueMap::ValueMap (ValueMap&& other) noexcept : map(other.map), owner(other.owner) {
    other.map = nullptr;
    other.owner = false;
}

inline ValueMap::~ValueMap () {
    if (owner)
        FXNValueMapRelease(map);
}

inline ValueMap& ValueMap::operator= (ValueMap&& other) noexcept {
    if (this == &other)
        return *this;
    if (owner)
        FXNValueMapRelease(map);
    map = other.map;
    owner = other.owner;
    other.map = nullptr;
    other.owner = false;
    return *this;
}

inline bool ValueMap::Contains (const std::string& key) const {
    FXNValue* value = nullptr;
    return FXNValueMapGetValue(map, key.c_str(), &value) == FXN_OK;
}

inline size_t ValueMap::Size () const {
    int size = 0;
    FXNValueMapGetSize(map, &size);
    return size;
}

inline Value ValueMap::Pop (const std::string& key) {
    FXNValue* value = nullptr;
    auto status = FXNValueMapGetValue(map, key.c_str(), &value);
    FXN_ASSERT_THROW(status == FXN_OK, "Value map does not contain a value for key `{}`", key);
    FXNValueMapSetValue(map, key.c_str(), nullptr);
    return Value(value);
}

inline ValueMap::Proxy ValueMap::operator[] (const std::string& key) const {
    return Proxy(*this, key);
}

inline ValueMap::Iterator ValueMap::begin () const {
    return Iterator(*this, 0);
}

inline ValueMap::Iterator ValueMap::end () const {
    return Iterator(*this, static_cast<int>(Size()));
}

inline ValueMap::operator FXNValueMap* () const {
    return map;
}

inline ValueMap::Proxy::Proxy (const ValueMap& map, const std::string& key) : map(map), key(key) {

}

inline ValueMap::Proxy::operator Value () const {
    FXNValue* value = nullptr;
    auto status = FXNValueMapGetValue(map, key.c_str(), &value);
    FXN_ASSERT_THROW(status == FXN_OK, "Value map does not contain a value for key `{}`", key);
    return Value(value, false);
}

inline ValueMap::Proxy::operator float () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_FLOAT32, "Value map value for key `{}` with type {} cannot be cast to float", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to float because it is not scalar", key);
    return *value.GetData<float>();
}

inline ValueMap::Proxy::operator double () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_FLOAT64, "Value map value for key `{}` with type {} cannot be cast to double", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to double because it is not scalar", key);
    return *value.GetData<double>();
}

inline ValueMap::Proxy::operator int16_t () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_INT16, "Value map value for key `{}` with type {} cannot be cast to short", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to short because it is not scalar", key);
    return *value.GetData<int16_t>();
}

inline ValueMap::Proxy::operator int32_t () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_INT32, "Value map value for key `{}` with type {} cannot be cast to integer", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to integer because it is not scalar", key);
    return *value.GetData<int32_t>();
}

inline ValueMap::Proxy::operator int64_t () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_INT64, "Value map value for key `{}` with type {} cannot be cast to long", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to long because it is not scalar", key);
    return *value.GetData<int64_t>();
}

inline ValueMap::Proxy::operator std::string () const {
    Value value = *this;
    auto type = value.GetType();
    FXN_ASSERT_THROW(type == FXN_DTYPE_STRING, "Value map value for key `{}` with type {} cannot be cast to string", key, type);
    return std::string(value.GetData<char>());
}

inline ValueMap::Proxy::operator bool () const {
    Value value = *this;
    auto type = value.GetType();
    auto shape = value.GetShape();
    FXN_ASSERT_THROW(type == FXN_DTYPE_BOOL, "Value map value for key `{}` with type {} cannot be cast to boolean", key, type);
    FXN_ASSERT_THROW(shape.size() == 0, "Value map value for key `{}` cannot be cast to boolean because it is not scalar", key);
    return *value.GetData<int8_t>();
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (float input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (double input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (int8_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (int16_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (int32_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (int64_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (uint8_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (uint16_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (uint32_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (uint64_t input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (bool input) const {
    auto value = Value::CreateArray(&input, {}, FXN_VALUE_FLAG_COPY_DATA);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (const std::string& input) const {
    auto value = Value::CreateString(input);
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (std::nullptr_t) const {
    auto value = Value::CreateNull();
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline const ValueMap::Proxy& ValueMap::Proxy::operator= (Value&& value) const {
    FXNValueMapSetValue(map, key.c_str(), value.Release());
    return *this;
}

inline ValueMap::Iterator::Iterator (const ValueMap& map, int index) : map(const_cast<ValueMap&>(map)), index(index) { }

inline std::pair<std::string, Value> ValueMap::Iterator::operator* () const {
    // Get key
    char key[256] { };
    auto status = FXNValueMapGetKey(map, index, key, sizeof key);
    FXN_ASSERT_THROW(status == FXN_OK, "Value map iterator failed to get value map key with status: {}", status);
    // Get value
    FXNValue* value = nullptr;
    status = FXNValueMapGetValue(map, key, &value);
    FXN_ASSERT_THROW(status == FXN_OK, "Value map iterator failed to get value map value with status: {}", status);
    // Return
    return { std::string(key), Value(value, false) };
}

inline ValueMap::Iterator& ValueMap::Iterator::operator++ () {
    ++index;
    return *this;
}

inline bool ValueMap::Iterator::operator== (const ValueMap::Iterator& other) const {
    return map.map == other.map.map && index == other.index;
}

inline bool ValueMap::Iterator::operator!= (const ValueMap::Iterator& other) const {
    return !(*this == other);
}
#pragma endregion


#pragma region --Prediction--

inline Prediction::Prediction (FXNPrediction* prediction, bool owner) noexcept : prediction(prediction), owner(owner) {

}

inline Prediction::Prediction (Prediction&& other) noexcept : prediction(other.prediction), owner(other.owner) {
    other.prediction = nullptr;
    other.owner = false;
}

inline Prediction::~Prediction () {
    if (owner)
        FXNPredictionRelease(prediction);
}

inline Prediction& Prediction::operator= (Prediction&& other) noexcept {
    if (this == &other)
        return *this;
    if (owner)
        FXNPredictionRelease(prediction);
    prediction = nullptr;
    owner = false;
    std::swap(prediction, other.prediction);
    std::swap(owner, other.owner);
    return *this;
}

inline std::string Prediction::GetID () const {
    std::string id;
    id.resize(1024);
    FXNPredictionGetID(prediction, id.data(), static_cast<int>(id.size()));
    id.resize(strlen(id.c_str()));
    return id;
}

inline double Prediction::GetLatency () const {
    double latency = 0;
    FXNPredictionGetLatency(prediction, &latency);
    return latency;
}

inline ValueMap Prediction::GetResults () const {
    FXNValueMap* outputs = nullptr;
    FXNPredictionGetResults(prediction, &outputs);
    return ValueMap(outputs, false);
}

inline std::string Prediction::GetError () const {
    std::string error;
    error.resize(4096);
    FXNPredictionGetError(prediction, error.data(), static_cast<int>(error.size()));
    error.resize(strlen(error.c_str()));
    return error;
}

inline std::string Prediction::GetLogs () const {
    int32_t length = 0;
    FXNPredictionGetLogLength(prediction, &length);
    std::string logs;
    logs.resize(length);
    FXNPredictionGetLogs(prediction, logs.data(), length);
    return logs;
}

inline Prediction::operator FXNPrediction* () const {
    return prediction;
}

#pragma endregion


#pragma region --Predictor--

inline Predictor::Predictor (const Configuration& configuration) : predictor(nullptr) {
    FXNPredictorCreate(configuration, &predictor);
    owner = true;
}

inline Predictor::Predictor (FXNPredictor* predictor, bool owner) noexcept : predictor(predictor), owner(owner) {

}

inline Predictor::Predictor (Predictor&& other) noexcept : predictor(other.predictor), owner(other.owner) {
    other.predictor = nullptr;
    other.owner = false;
}

inline Predictor::~Predictor () {
    if (owner)
        FXNPredictorRelease(predictor);
}

inline Predictor& Predictor::operator= (Predictor&& other) noexcept {
    if (this == &other)
        return *this;
    if (owner)
        FXNPredictorRelease(predictor);
    predictor = nullptr;
    owner = false;
    std::swap(predictor, other.predictor);
    std::swap(owner, other.owner);
    return *this;
}

inline Prediction Predictor::operator() (const ValueMap& inputs) const {
    FXNPrediction* prediction = nullptr;
    FXNPredictorCreatePrediction(predictor, inputs, &prediction);
    return Prediction(prediction, true);
}
#pragma endregion

}