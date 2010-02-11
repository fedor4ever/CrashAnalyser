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
#include "SymbianInflate.h"

// User includes
#include "huffman.h"

// External references
extern TInt InflateUnCompress(unsigned char* source, int sourcesize,unsigned char* dest, int destsize);


SYMBIANNATIVETOOL_API TInt SymbianInflateImage( TUint8* aSource, TInt aSourceSize, TUint8* aDest, TInt aDestSize )
	{
    TInt ret = 0;
    //
    try
        {
        ret = InflateUnCompress( aSource, aSourceSize, aDest, aDestSize );
        }
    catch( E32ImageCompressionError& aError )
        {
        ret = aError.iError;
        }
    //
    return ret;
	}

