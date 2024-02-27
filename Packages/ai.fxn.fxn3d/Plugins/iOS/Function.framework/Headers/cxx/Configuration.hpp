//
//  Configuration.hpp
//  Function
//
//  Created by Yusuf Olokoba on 1/30/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <filesystem>
#include <string>
#include <vector>
#include <Function/Function.h>

namespace Function {

    class Configuration final {
    public:
        Configuration ();

        explicit Configuration (const std::string& tag);

        explicit Configuration (FXNConfiguration* value, bool owner = true) noexcept;

        Configuration (const Configuration&) = delete;

        Configuration (Configuration&& other) noexcept;

        ~Configuration ();

        Configuration& operator= (const Configuration&) = delete;

        Configuration& operator= (Configuration&& other) noexcept;

        std::string GetTag () const;

        void SetTag (const std::string& tag) const;

        std::string GetToken () const;

        void SetToken (const std::string& token) const;

        FXNAcceleration GetAcceleration () const;

        void SetAcceleration (FXNAcceleration acceleration) const;

        void* GetDevice () const;

        template<typename T>
        T* GetDevice () const;

        template<typename T>
        void SetDevice (T* device) const;

        void AddResource (const std::string& type, const std::filesystem::path& path) const;

        operator FXNConfiguration* () const;

        static std::string GetUniqueID ();

    private:
        FXNConfiguration* configuration;
        bool owner;
    };
}