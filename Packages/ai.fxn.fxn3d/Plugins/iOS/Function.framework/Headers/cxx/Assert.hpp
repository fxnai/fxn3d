//
//  Assert.hpp
//  Function
//
//  Created by Yusuf Olokoba on 1/30/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#ifndef FXN_ASSERT
/*!
 @abstract Make an assertion and return if the assertion fails.

 @discussion Make an assertion and return if the assertion fails.
 
 @param assertion
 Assertion that results to a boolean.

 @param returnValue
 Value to return if the assertion fails

 @param ...
 Message and format arguments to log about the assertion.
*/
#define FXN_ASSERT(assertion, returnValue, ...)
#endif


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