//
//  ValueMap.hpp
//  Function
//
//  Created by Yusuf Olokoba on 2/1/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <cstddef>
#include <string>
#include <utility>
#include <vector>
#include <Function/Function.h>
#include "Value.hpp"

namespace Function {

    class ValueMap final {
    private:
        class Proxy {
            friend class ValueMap;
        public:
            Value Get () const;
            template<typename T>
            T Get () const;
            operator Value () const;
            const Proxy& operator= (float value) const;
            const Proxy& operator= (double value) const;
            const Proxy& operator= (int8_t value) const;
            const Proxy& operator= (int16_t value) const;
            const Proxy& operator= (int32_t value) const;
            const Proxy& operator= (int64_t value) const;
            const Proxy& operator= (uint8_t value) const;
            const Proxy& operator= (uint16_t value) const;
            const Proxy& operator= (uint32_t value) const;
            const Proxy& operator= (uint64_t value) const;
            const Proxy& operator= (const std::string& value) const;
            const Proxy& operator= (bool value) const;
            const Proxy& operator= (std::nullptr_t) const;
            const Proxy& operator= (Value&& value) const;
        private:
            const ValueMap& map;
            std::string key;
            Proxy (const ValueMap& map, const std::string& key);
        };
    public:
        class Iterator {
            friend class ValueMap;
        public:
            std::pair<std::string, Value> operator* () const;
            Iterator& operator++ ();
            bool operator== (const Iterator& other) const;
            bool operator!= (const Iterator& other) const;
        private:
            ValueMap& map;
            int index;
            Iterator (const ValueMap& map, int index = 0);
        };
    public:
        ValueMap ();

        explicit ValueMap (FXNValueMap* map, bool owner = true) noexcept;

        ValueMap (const ValueMap&) = delete;

        ValueMap (ValueMap&& other) noexcept;

        ~ValueMap ();

        ValueMap& operator= (const ValueMap&) = delete;

        ValueMap& operator= (ValueMap&& other) noexcept;

        bool Contains (const std::string& key) const;

        size_t Size () const;

        void Remove (const std::string& key) const;

        Proxy operator[] (const std::string& key) const;

        Iterator begin () const;

        Iterator end () const;

        operator FXNValueMap* () const;
    private:
        FXNValueMap* map;
        bool owner;
    };
};