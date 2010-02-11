/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
* All rights reserved.
* This component and the accompanying materials are made available
* under the terms of "Eclipse Public License v1.0"
* which accompanies this distribution, and is available
* at the URL "http://www.eclipse.org/legal/epl-v10.html".
*
* Initial Contributors:
* Nokia Corporation - initial contribution.
*
* Contributors:
* 
* Description:
*
*/
#ifndef SYMBIANNATIVETOOLSAPI_H
#define SYMBIANNATIVETOOLSAPI_H

// from a DLL simpler. All files within this DLL are compiled with the SYMBIAN_NATIVE_TOOLS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SYMBIANBYTEPAIRLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SYMBIAN_NATIVE_TOOL_EXPORTS
#define SYMBIANNATIVETOOL_API __declspec(dllexport)
#else
#define SYMBIANNATIVETOOL_API __declspec(dllimport)
#endif

// User includes
#include "e32defwrap.h"

#endif