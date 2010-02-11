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
#include "SymbianBytePair.h"

// User includes
#include "byte_pair.h"
#include "e32errwrap.h"
#include "pagedcompress.h"


SYMBIANNATIVETOOL_API TInt SymbianBytePairUnpackRaw( TUint8* aSource, TInt aSourceSize, TUint8* aDest, TInt aDestSize )
	{
    TUint8* notUsed = 0;
    const TInt ret = UnpackBytePair( aDest, aDestSize, aSource, aSourceSize, notUsed );
    return ret;
	}

SYMBIANNATIVETOOL_API TInt SymbianBytePairUnpackImage( TUint8* aSource, TInt aSourceSize, TUint8* aDest, TInt aDestSize, TInt* aAmountOfInputRead )
	{
    TInt amountOfInputRead = 0;
    const TInt ret = UnpackBytePairE32Image( aSource, aSourceSize, aDest, aDestSize, amountOfInputRead );
    *aAmountOfInputRead = amountOfInputRead;
    return ret;
	}

