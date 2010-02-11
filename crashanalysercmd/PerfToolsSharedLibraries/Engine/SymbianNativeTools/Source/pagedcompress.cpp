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
#include "pagedcompress.h"

// User includes
#include "h_utl.h"
#include "byte_pair.h"
#include "e32defwrap.h"
#include "e32errwrap.h"

// Constants and defines
const TInt MaxBlockSize = 0x1000;
#define PAGE_SIZE 4096


typedef struct IndexTableItemTag
{
	TUint16 iSizeOfCompressedPageData;	// pointer to an array TUint16[NumberOfPages]
	TUint8 *iCompressedPageData;		// pointer to an array TUint8*. Each elemet of 
										// this array point a compressed Page data	
}IndexTableItem;


typedef struct IndexTableHeaderTag
{
	TInt	iSizeOfData;					// Includes the index and compressed pages
	TInt	iDecompressedSize;
	TUint16	iNumberOfPages;
} IndexTableHeader;


class CBytePairCompressedImage
{
	public:
		static CBytePairCompressedImage* NewLC(TUint16 aNumberOfPages, TInt aSize);
		
		~CBytePairCompressedImage();
		
		int  GetPage(TUint16 aPageNum, TUint8 * aPageData);
		int  ReadInTable(std::istream &is, TUint & aNumberOfPages);
	
	private:
		TInt ConstructL( TUint16 aNumberOfPages, TInt aSize);
		CBytePairCompressedImage();
		
	private:
		IndexTableHeader 	iHeader;
		IndexTableItem*		iPages;
		TUint8* 			iOutBuffer; 
		
};


CBytePairCompressedImage::CBytePairCompressedImage()
{
	
}


CBytePairCompressedImage* CBytePairCompressedImage::NewLC(TUint16 aNumberOfPages, TInt aSize)
{
	CBytePairCompressedImage* self = new CBytePairCompressedImage;
	if( NULL == self)
	{
		return self;
	}
	
	if( KErrNone == self->ConstructL(aNumberOfPages, aSize))
	{
		return self;
	}
	return NULL;
}


TInt CBytePairCompressedImage::ConstructL(TUint16 aNumberOfPages, TInt aSize)
{
	//Print(EWarning,"Start ofCBytePairCompressedImage::ConstructL(%d, %d)\n", aNumberOfPages, aSize );
	iHeader.iNumberOfPages = aNumberOfPages;
	iHeader.iDecompressedSize = aSize;
	
	if( 0 != aNumberOfPages)
	{
		iPages = (IndexTableItem *) calloc(aNumberOfPages, sizeof(IndexTableItem));
		
		if( NULL == iPages )
		{
			return KErrNoMemory;
		}
	}
		
	iHeader.iSizeOfData = 	sizeof(iHeader.iSizeOfData) + 
							sizeof(iHeader.iDecompressedSize) + 
							sizeof(iHeader.iNumberOfPages) + 
							aNumberOfPages * sizeof(TUint16);
	
	iOutBuffer = (TUint8 *) calloc(4 * PAGE_SIZE, sizeof(TUint8) ); 
	
	if ( NULL == iOutBuffer)
	{
		return KErrNoMemory;
	}
	return KErrNone;
} // End of ConstructL()

CBytePairCompressedImage::~CBytePairCompressedImage()
{
	
	for( int i = 0; i < iHeader.iNumberOfPages; i++)
	{
		free(iPages[i].iCompressedPageData);
		iPages[i].iCompressedPageData = NULL;
	}
	
	free( iPages );
	iPages = NULL;
	
	free( iOutBuffer );
	iOutBuffer = NULL;
}


int CBytePairCompressedImage::ReadInTable(std::istream &is, TUint & aNumberOfPages)
{
	// Read page index table header
    int count = (sizeof(iHeader.iSizeOfData)+sizeof(iHeader.iDecompressedSize)+sizeof(iHeader.iNumberOfPages));
	is.read((char *)&iHeader, count);

	// Allocatin place to Page index table entries
	iPages = (IndexTableItem *) calloc(iHeader.iNumberOfPages, sizeof(IndexTableItem));
		
	if( NULL == iPages )
	{
		return KErrNoMemory;
	}

	// Read whole Page index table 
	for(TInt i = 0; i < iHeader.iNumberOfPages; i++)
	{
		is.read((char *) &(iPages[i].iSizeOfCompressedPageData), sizeof(TUint16));
	}

	// Read compressed data pages page by page, decompress and store them
	for(TInt i = 0; i < iHeader.iNumberOfPages; i++)
	{

		iPages[i].iCompressedPageData = (TUint8 *) calloc(iPages[i].iSizeOfCompressedPageData, sizeof(TUint8) );
	
		if( NULL == iPages[i].iCompressedPageData )
		{
			return KErrNoMemory;
		}

		is.read((char *)iPages[i].iCompressedPageData, iPages[i].iSizeOfCompressedPageData);
	}

	aNumberOfPages = iHeader.iNumberOfPages;

	return KErrNone;
}

int  CBytePairCompressedImage::GetPage(TUint16 aPageNum, TUint8 * aPageData)
{
	TUint8* pakEnd;
        
       
   	TUint16 uncompressedSize = (TUint16) UnpackBytePair( aPageData, 
												         MaxBlockSize, 
												         iPages[aPageNum].iCompressedPageData, 
												         iPages[aPageNum].iSizeOfCompressedPageData, 
												         pakEnd );

	return uncompressedSize;


}



int DecompressPages(TUint8 * bytes, std::istream& is)
{
	TUint decompressedSize = 0;
	CBytePairCompressedImage *comprImage = CBytePairCompressedImage::NewLC(0, 0);
	if( NULL == comprImage)
	{
		return KErrNoMemory;
	}

	TUint numberOfPages = 0;
	comprImage->ReadInTable(is, numberOfPages);


	TUint8* iPageStart;
	TUint16 iPage = 0;
	
	while(iPage < numberOfPages )
	{
		iPageStart = &bytes[iPage * PAGE_SIZE];
		
		decompressedSize += comprImage->GetPage(iPage, iPageStart);

		++iPage;
	}
	
	delete comprImage;
	return decompressedSize;

}

TInt UnpackBytePairE32Image( TUint8* aSource, TInt aSourceSize, TUint8* aDest, TInt aDestSize, TInt& aInputBytesRead )
{
    istrstream stream( (const char*) aSource, aSourceSize );
    const streampos startPos = stream.tellg();
    const TInt ret = DecompressPages( aDest, stream );
    const streampos endPos = stream.tellg();
    aInputBytesRead = endPos - startPos;
    return ret;
}
