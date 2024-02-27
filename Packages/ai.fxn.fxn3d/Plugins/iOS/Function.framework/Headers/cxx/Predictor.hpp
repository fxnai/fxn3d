//
//  Predictor.hpp
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
#include "Configuration.hpp"
#include "Prediction.hpp"

namespace Function {

    class Predictor final {
    public:
        explicit Predictor (const Configuration& configuration);

        explicit Predictor (FXNPredictor* value, bool owner = true) noexcept;

        Predictor (const Predictor&) = delete;

        Predictor (Predictor&& other) noexcept;

        ~Predictor ();

        Predictor& operator= (const Predictor&) = delete;

        Predictor& operator= (Predictor&& other) noexcept;

        Prediction operator() (const ValueMap& inputs) const;
    private:
        FXNPredictor* predictor;
        bool owner;
    };
}