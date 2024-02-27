//
//  Value.hpp
//  Function
//
//  Created by Yusuf Olokoba on 1/30/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <string>
#include <vector>
#include <Function/Function.h>

namespace Function {

    class Value final {
    public:
        /*!
         @abstract Create a value from an existing `FXNValue*`.

         @param value
         Function value.

         @param owner
         Whether to transfer ownership to the `Function::Value`.
        */
        explicit Value (FXNValue* value, bool owner = true) noexcept;

        Value (const Value&) = delete;

        Value (Value&& other) noexcept;

        ~Value ();

        Value& operator= (const Value&) = delete;

        Value& operator= (Value&& other) noexcept;

        /*!
         @abstract Get the value data.
        */
        void* GetData () const;

        /*!
         @abstract Get the value data.
        */
        template<typename T>
        T* GetData () const;

        /*!
         @abstract Get the data type of a given value.
        */
        FXNDtype GetType () const;

        /*!
         @abstract Get the number of dimensions for a given value.

         @discussion Get the number of dimensions for a given value.
         If the value is not a tensor, this function will return zero.
        */
        int32_t GetDimensions () const;

        /*!
         @abstract Get the shape of a given value.
        */
        std::vector<int32_t> GetShape () const;

        FXNValue* Release ();

        operator FXNValue* () const;

        /*!
         @abstract Create an array value.

         @discussion The value will allocate its own data.

         @param shape
         Array shape.
         This should be empty for scalar values.
        */
        template<typename T>
        static Value CreateArray (const std::vector<int32_t>& shape);

        /*!
         @abstract Create an array value from a data buffer.

         @param data
         Array data.

         @param shape
         Array shape.
         This should be empty for scalar values.

         @param flags
         Value creation flags.
        */
        template<typename T>
        static Value CreateArray (T* data, const std::vector<int32_t>& shape, FXNValueFlags flags);

        /*!
         @abstract Create a string value from a UTF-8 encoded string.
        
         @param str
         Input string.
        */
        static Value CreateString (const std::string& str);

        /*!
         @abstract Create a list value from a JSON-encoded list.
        
         @param jsonList
         JSON-encoded list.
        */
        static Value CreateList (const std::string& jsonList);

        /*!
         @abstract Create a dictionary value from a JSON-encoded dictionary.
        
         @param jsonDict
         JSON-encoded dictionary.
        */
        static Value CreateDict (const std::string& jsonDict);

        /*!
         @abstract Create an image value from a pixel buffer.

         @discussion Create an image value from a pixel buffer.
         The pixel buffer MUST have an interleaved R8 (8bpp), RGB888 (24bpp), or RGBA8888 layout (32bpp).

         @param pixelBuffer
         Pixel buffer.

         @param width
         Pixel buffer width.

         @param height
         Pixel buffer height.

         @param channels
         Pixel buffer channels.
         MUST be 1, 3, or 4.

         @param flags
         Value creation flags.
        */
        static Value CreateImage (const uint8_t* pixelBuffer, int32_t width, int32_t height, int32_t channels, FXNValueFlags flags);

        /*!
         @abstract Create a binary value from a raw buffer.
        
         @param buffer
         Buffer.

         @param bufferLen
         Buffer length in bytes.

         @param flags
         Value creation flags.
        */
        static Value CreateBinary (void* buffer, int64_t bufferLen, FXNValueFlags flags);
        
        /*!
         @abstract Create a null value.
        */
        static Value CreateNull ();
    private:
        FXNValue* value;
        bool owner;
    };
}