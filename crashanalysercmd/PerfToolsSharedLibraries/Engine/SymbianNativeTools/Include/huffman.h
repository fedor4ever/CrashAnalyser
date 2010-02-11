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

#ifndef __HUFFMAN_H__
#define __HUFFMAN_H__

#include "e32defwrap.h"
#include <fstream>



/**
Base class for E32Image Compression Errors.
@internalComponent
@released
*/
class E32ImageCompressionError
{
    public:
        enum
            {
		    HUFFMANBUFFEROVERFLOWERROR = -1,
            HUFFMANBUFFERUNDERFLOWERROR = -2,
		    HUFFMANTOOMANYCODESERROR = -3,
		    HUFFMANINVALIDCODINGERROR = -4,
            };
	public:
		E32ImageCompressionError(int aError)
            {
            iError = aError;
            }

    public:
        int iError;
};



/**
Class for Bit input stream.
Good for reading bit streams for packed, compressed or huffman data algorithms.
@since 8.0
@library euser.lib
@internalComponent
@released
*/
class TBitInput
{
	public:
		TBitInput();
		TBitInput(const TUint8* aPtr, TInt aLength, TInt aOffset=0);
		void Set(const TUint8* aPtr, TInt aLength, TInt aOffset=0);
		TUint ReadL();
		TUint ReadL(TInt aSize);
		TUint HuffmanL(const TUint32* aTree);
	private:
		virtual void UnderflowL();
	private:
		TInt iCount;
		TUint iBits;
		TInt iRemain;
		const TUint32* iPtr;
};

/**
Class derived from TBitInput
@internalComponent
@released
*/
class TFileInput : public TBitInput
{
	public:
		TFileInput(unsigned char* source,int size);
		~TFileInput();
	private:
		void UnderflowL();
	private:
		TUint8* iReadBuf;
		TInt iSize;
};
/*
Class for Huffman code toolkit.

This class builds a huffman encoding from a frequency table and builds a decoding tree from a
code-lengths table.

The encoding generated is based on the rule that given two symbols s1 and s2, with code
length l1 and l2, and huffman codes h1 and h2:
	if l1<l2 then h1<h2 when compared lexicographically
	if l1==l2 and s1<s2 then h1<h2 ditto

This allows the encoding to be stored compactly as a table of code lengths

@since 8.0
@library euser.lib
@internalComponent
@released
*/
class Huffman
{
	public:
		enum {KMaxCodeLength=27};
		enum {KMetaCodes=KMaxCodeLength+1};
		enum {KMaxCodes=0x8000};
	public:
		static void HuffmanL(const TUint32 aFrequency[],TInt aNumCodes,TUint32 aHuffman[]);
		static void Encoding(const TUint32 aHuffman[],TInt aNumCodes,TUint32 aEncodeTable[]);
		static TBool IsValid(const TUint32 aHuffman[],TInt aNumCodes);
		static void Decoding(const TUint32 aHuffman[],TInt aNumCodes,TUint32 aDecodeTree[],TInt aSymbolBase=0);
		static void InternalizeL(TBitInput& aInput,TUint32 aHuffman[],TInt aNumCodes);
};

// local definitions used for Huffman code generation
typedef TUint16 THuff;		/** @internal */
const THuff KLeaf=0x8000;	/** @internal */
struct TNode
/** @internal */
{
	TUint iCount;
	THuff iLeft;
	THuff iRight;
};

const TInt KDeflateLengthMag=8;
const TInt KDeflateDistanceMag=12;

/**
class for TEncoding
@internalComponent
@released
*/
class TEncoding
{
	public:
		enum {ELiterals=256,ELengths=(KDeflateLengthMag-1)*4,ESpecials=1,EDistances=(KDeflateDistanceMag-1)*4};
		enum {ELitLens=ELiterals+ELengths+ESpecials};
		enum {EEos=ELiterals+ELengths};
	public:
		TUint32 iLitLen[ELitLens];
		TUint32 iDistance[EDistances];
};

const TInt KDeflationCodes=TEncoding::ELitLens+TEncoding::EDistances;

#endif

