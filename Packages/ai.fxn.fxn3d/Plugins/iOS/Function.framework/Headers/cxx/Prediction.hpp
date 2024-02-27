//
//  Prediction.hpp
//  Function
//
//  Created by Yusuf Olokoba on 2/25/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <cstddef>
#include <string>
#include <utility>
#include <vector>
#include <Function/Function.h>
#include "ValueMap.hpp"

namespace Function {

    class Prediction final {
    public:
        explicit Prediction (FXNPrediction* prediction, bool owner = true) noexcept;

        Prediction (const Prediction&) = delete;

        Prediction (Prediction&& other) noexcept;

        ~Prediction ();

        Prediction& operator= (const Prediction&) = delete;

        Prediction& operator= (Prediction&& other) noexcept;

        std::string GetID () const;

        double GetLatency () const;

        ValueMap GetResults () const;

        std::string GetError () const;

        std::string GetLogs () const;

        operator FXNPrediction* () const;
    private:
        FXNPrediction* prediction;
        bool owner;
    };
}