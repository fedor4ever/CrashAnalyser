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
using System;

#region MOVING MEMORY MODEL
// 00000000-003FFFFF	Unmapped
// 00400000-2FFFFFFF	Moving process data
// 30000000-3FFFFFFF	DLL static data (=phys ram size/2 up to 128M, always ends at 40000000)
// 40000000-5FFFFFFF	RAM drive
// 60000000-60001FFF	Super page/CPU page
// 61000000-61003FFF	Page directory (16K)
// 61020000-6103FFFF	Page table info (4096 * 8bytes = 32K)
// 61100000-611FFFFF	Cache flush area
// 61200000-612FFFFF	Alternate cache flush area
// 62000000-623FFFFF	Page tables (up to 4096 * 1K)
// 63000000-63FFFFFF	Primary I/O mappings
// 64000000-64FFFFFF	Kernel .data/.bss, initial stack, kernel heap
// 65000000-655FFFFF	fixed processes - usually 2 or 3Mb each.
// 65600000-F1FFFFFF	Kernel section (includes extra I/O mappings)
// F2000000-F3FFFFFF	Kernel code (RAM size/2)
// F4000000-F7FFFFFF	User code (RAM size)
// F8000000-FFEFFFFF	ROM
// FFF00000-FFFFFFFF	Exception vectors
#endregion
#region MULTIPLE MEMORY MODEL 
// Linear address map (1Gb configuration):
// 00000000-003FFFFF	Unmapped
// 00400000-1FFFFFFF	Local data
// 20000000-3BFFFFFF	Shared data
// 3C000000-3DFFFFFF	RAM loaded code (=phys ram size up to 256M)
// 3E000000-3FFFFFFF	DLL static data (=phys ram size/2 up to 128M)
// 40000000-7FFFFFFF	Unused
//
// 80000000-8FFFFFFF	ROM
// 90000000-9FFFFFFF	User Global Area
// A0000000-BFFFFFFF	RAM drive
// C0000000-C0001FFF	Super page/CPU page
// C0040000-C00403FF	ASID info (256 ASIDs)
// C0080000-C00FFFFF	Page table info	
// C1000000-C13FFFFF	Page directories (up to 256 * 16KB)
// C2000000-C5FFFFFF	Page tables
// C6000000-C6FFFFFF	Primary I/O mappings
// C7000000-C7FFFFFF
// C8000000-C8FFFFFF	Kernel .data/.bss, initial stack, kernel heap
// C9000000-C91FFFFF	Kernel stacks
// C9200000-FFEFFFFF	Extra kernel mappings (I/O, RAM loaded device drivers)
// FFF00000-FFFFFFFF	Exception vectors
//
//
// Linear address map (2Gb configuration):
// 00000000-003FFFFF	Unmapped
// 00400000-37FFFFFF	Local data
// 38000000-3FFFFFFF	DLL static data (=phys ram size/2 up to 128M)
// 40000000-6FFFFFFF	Shared data
// 70000000-7FFFFFFF	RAM loaded code (=phys ram size up to 256M)
//
// 80000000-8FFFFFFF	ROM
// 90000000-9FFFFFFF	User Global Area
// A0000000-BFFFFFFF	RAM drive
// C0000000-C0001FFF	Super page/CPU page
// C0040000-C00403FF	ASID info (256 ASIDs)
// C0080000-C00FFFFF	Page table info	
// C1000000-C13FFFFF	Page directories (up to 256 * 16KB)
// C2000000-C5FFFFFF	Page tables
// C6000000-C6FFFFFF	Primary I/O mappings
// C7000000-C7FFFFFF
// C8000000-C8FFFFFF	Kernel .data/.bss, initial stack, kernel heap
// C9000000-C91FFFFF	Kernel stacks
// C9200000-FFEFFFFF	Extra kernel mappings (I/O, RAM loaded device drivers)
// FFF00000-FFFFFFFF	Exception vectors
#endregion

namespace SymbianStructuresLib.MemoryModel
{
	public enum TMemoryModelType
	{
		EMemoryModelUnknown = -1,
		EMemoryModelMoving = 0,
		EMemoryModelMultiple,
        EMemoryModelFlexible
	}
}
