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

#ifndef H_UTL_H
#define H_UTL_H
//
#include <stdio.h>

#ifdef __VC32__
 #ifdef __MSVCDOTNET__
  #include <iostream>
  #include <strstream>
  #include <fstream>
  using namespace std;
 #else //!__MSVCDOTNET__
  #include <iostream.h>
  #include <strstrea.h>
  #include <fstream.h>
 #endif //__MSVCDOTNET__
#else //!__VC32__
  #include <iostream.h>
  #include <strstream.h>
  #include <fstream.h>
#endif // __VC32__

#ifdef __LINUX__
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <ctype.h>

#define _close close
#define _filelength filelength
#define _lseek lseek
#define _read read
#define _snprintf snprintf
#define _vsnprintf vsnprintf

// linux case insensitive stromg comparisons have different names
#define stricmp  strcasecmp		
#define _stricmp strcasecmp		
#define strnicmp strncasecmp	

// to fix the linux problem: memcpy does not work with overlapped areas.
#define memcpy memmove

// hand-rolled strupr function for converting a string to all uppercase
char* strupr(char *a);

// return the length of a file
off_t filelength (int filedes);

#endif //__LINUX__

#include <e32defwrap.h>

#define ALIGN4K(a) ((a+0xfff)&0xfffff000)
#define ALIGN4(a) ((a+0x3)&0xfffffffc)


#ifdef HEAPCHK
#define NOIMAGE
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
void HeapCheck();
#endif
#define Print H.PrintString
//
const TInt KMaxStringLength=0x400;
//

class HMem
	{
public:
	static TAny* Alloc(TAny * const aBaseAddress,const TUint32 aImageSize);
	static TAny* AllocZ( const TUint32 aImageSize );
	static void Free(TAny * const aMem);
	static void Copy(TAny * const aDestAddr,const TAny * const aSourceAddr,const TUint32 aLength);
	static void Move(TAny * const aDestAddr,const TAny * const aSourceAddr,const TUint32 aLength);
	static void Set(TAny * const aDestAddr, const TUint8 aFillChar, const TUint32 aLength);
	static void FillZ(TAny * const aDestAddr, const TUint32 aLength);

	static TUint CheckSum(TUint *aPtr, TInt aSize);
	static TUint CheckSum8(TUint8 *aPtr, TInt aSize);
	static TUint CheckSumOdd8(TUint8 *aPtr, TInt aSize);
	static TUint CheckSumEven8(TUint8 *aPtr, TInt aSize);

	static void Crc32(TUint32& aCrc, const TAny* aPtr, TInt aLength);
	};


template <class T,class S>
inline T* PtrAdd(T* aPtr,S aVal)
    {return((T*)(((TUint8*)aPtr)+aVal));}

template <class T>
inline T Min(T aLeft,T aRight)
    {return(aLeft<aRight ? aLeft : aRight);}


#endif
