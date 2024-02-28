//
//  Assert.hpp
//  Function
//
//  Created by Yusuf Olokoba on 1/30/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#ifndef FXN_ASSERT_THROW
/*!
 @abstract Make an assertion and raise an exception if the assertion fails.

 @discussion Make an assertion and raise an exception if the assertion fails.
 
 @param assertion
 Assertion that results to a boolean.

 @param message
 Exception message.

 @param ...
 Format arguments in the exception message.
*/
#define FXN_ASSERT_THROW(assertion, message, ...)
#endif