//
//  FXNPredictor.h
//  Function
//
//  Created by Yusuf Olokoba on 10/14/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include "FXNConfiguration.h"
#include "FXNPrediction.h"

#pragma region --Types--
/*!
 @struct FXNPredictor
 
 @abstract Predictor.

 @discussion Predictor.
*/
struct FXNPredictor;
typedef struct FXNPredictor FXNPredictor;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNPredictorCreate

 @abstract Create a predictor.

 @discussion Create a predictor.

 @param configuration
 Predictor configuration.

 @param predictor
 Created predictor.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNPredictorCreate (
    FXNConfiguration* configuration,
    FXNPredictor** predictor
);

/*!
 @function FXNPredictorRelease

 @abstract Release a predictor.

 @discussion Release a predictor.

 @param predictor
 Predictor.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNPredictorRelease (FXNPredictor* predictor);
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNPredictorPredict

 @abstract Create a prediction.

 @discussion Create a prediction.

 @param predictor
 Predictor.

 @param inputs
 Prediction inputs.

 @param prediction
 Prediction outputs.
 You MUST release the prediction with `FXNPredictionRelease` when no longer needed.
*/
FXN_BRIDGE FXN_EXPORT FXNStatus FXN_API FXNPredictorPredict (
    FXNPredictor* predictor,
    FXNValueMap* inputs,
    FXNPrediction** prediction
);
#pragma endregion