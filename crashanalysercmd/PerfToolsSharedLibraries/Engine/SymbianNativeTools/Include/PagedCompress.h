// Copyright (c) 2005-2009 Nokia Corporation and/or its subsidiary(-ies).
// All rights reserved.
// This component and the accompanying materials are made available
// under the terms of "Eclipse Public License v1.0"
// which accompanies this distribution, and is available
// at the URL "http://www.eclipse.org/legal/epl-v10.html".
//
// Initial Contributors:
// Nokia Corporation - initial contribution.
//
// Contributors:
//
// Description:
//

#ifndef PAGEDCOMPRESS_H
#define PAGEDCOMPRESS_H

// System includes
#include <istream>
#include <e32defwrap.h>

// Returns the number of output bytes created, and the number of input bytes read via aInputBytesRead
TInt UnpackBytePairE32Image( TUint8* aSource, TInt aSourceSize, TUint8* aDest, TInt aDestSize, TInt& aInputBytesRead );

#endif
