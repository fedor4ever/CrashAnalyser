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

namespace SymbianUtils
{
	public static class MemoryModel
	{
		#region Enumerations
		public enum TMemoryModelType
		{
			EMemoryModelUnknown = -1,
			EMemoryModelMoving = 0,
			EMemoryModelMultiple
		}

		public enum TMemoryModelRegion
		{
			// Common
			EMemoryModelRegionUnmapped = 0,
			EMemoryModelRegionDLLStaticData,
			EMemoryModelRegionRAMLoadedCode,
			EMemoryModelRegionROM,
			EMemoryModelRegionUserGlobalArea,
			EMemoryModelRegionRAMDrive,
			EMemoryModelRegionSuperAndCPUPages,
			EMemoryModelRegionPageTableInfo,
			EMemoryModelRegionPageDirectories,
			EMemoryModelRegionPageTables,
			EMemoryModelRegionPrimaryIOMappings,
			EMemoryModelRegionUnknown,
			EMemoryModelRegionKernelGlobalsInitialStackKernelHeap,
			EMemoryModelRegionExtraKernelMappings,
			EMemoryModelRegionExceptionVectors,
		
			// Moving
			EMemoryModelRegionMovingProcessData,
			EMemoryModelRegionCacheFlushArea,
			EMemoryModelRegionCacheFlushAreaAlternate,
			EMemoryModelRegionKernelCode,
			EMemoryModelRegionFixedProcesses,
			EMemoryModelRegionUserCode,

			// Multiple
			EMemoryModelRegionSharedData,
			EMemoryModelRegionLocalData,
			EMemoryModelRegionASIDInfo,
			EMemoryModelRegionKernelStacks,
		}
		#endregion

		#region API
		public static TMemoryModelType TypeByAddress( long aAddress )
		{
			// This is not a very good way of doing this. Should be 
			// either a UI option or then something in the symbol file
			// that we are reading...
			TMemoryModelType ret = TMemoryModelType.EMemoryModelUnknown;
			//
			if ( aAddress >= 0xc8000000 && aAddress < 0xC8FFFFFF)
			{
				// Kernel global, Multiple Memory Model
				ret = TMemoryModelType.EMemoryModelMultiple;
			}
			else if ( aAddress >= 0x80000000 && aAddress < 0x8FFFFFFF )
			{
				// ROM Symbol, Multiple Memory Model
				ret = TMemoryModelType.EMemoryModelMultiple;
			}
			else if ( aAddress >= 0x3C000000 && aAddress < 0x3DFFFFFF )
			{
				// [1gb] RAM Symbol, Moving Memory Model
				ret = TMemoryModelType.EMemoryModelMultiple;
			}
			else if ( aAddress >= 0x70000000 && aAddress < 0x7FFFFFFF )
			{
				// [2gb] RAM Symbol, Moving Memory Model
				ret = TMemoryModelType.EMemoryModelMultiple;
			}
            else if ( aAddress >= 0xF8000000 && aAddress < 0xFFEFFFFF )
            {
                // ROM Symbol, Moving Memory Model
                ret = TMemoryModelType.EMemoryModelMoving;
            }
            else if ( aAddress >= 0xF4000000 && aAddress < 0xF7FFFFFF )
            {
                // RAM Symbol, Moving Memory Model
                ret = TMemoryModelType.EMemoryModelMoving;
            }
            else if ( aAddress >= 0x64000000 && aAddress < 0x64FFFFFF )
            {
                // Kernel global, Moving Memory Model
                ret = TMemoryModelType.EMemoryModelMoving;
            }

			return ret;
		}

		public static TMemoryModelRegion RegionByAddress( long aAddress, TMemoryModelType aType )
		{
			TMemoryModelRegion ret = TMemoryModelRegion.EMemoryModelRegionUnknown;
			//
			if	( aType == TMemoryModelType.EMemoryModelMoving )
			{
				#region Moving Memory Model
				if	( aAddress >= 0x00000000 && aAddress < 0x003FFFFF )
				{
					// 00000000-003FFFFF	Unmapped
					ret = TMemoryModelRegion.EMemoryModelRegionUnmapped;
				}
				else if ( aAddress >= 0x00400000 && aAddress < 0x2FFFFFFF )
				{
					// 00400000-2FFFFFFF	Moving process data
					ret = TMemoryModelRegion.EMemoryModelRegionMovingProcessData;
				}
				else if ( aAddress >= 0x30000000 && aAddress < 0x3FFFFFFF )
				{
					// 30000000-3FFFFFFF	DLL static data (=phys ram size/2 up to 128M, always ends at 40000000)
					ret = TMemoryModelRegion.EMemoryModelRegionDLLStaticData;
				}
				else if ( aAddress >= 0x40000000 && aAddress < 0x5FFFFFFF )
				{
					// 40000000-5FFFFFFF	RAM drive
					ret = TMemoryModelRegion.EMemoryModelRegionRAMDrive;
				}
				else if ( aAddress >= 0x60000000 && aAddress < 0x60001FFF )
				{
					// 60000000-60001FFF	Super page/CPU page
					ret = TMemoryModelRegion.EMemoryModelRegionSuperAndCPUPages;
				}
				else if ( aAddress >= 0x61000000 && aAddress < 0x61003FFF )
				{
					// 61000000-61003FFF	Page directory (16K)
					ret = TMemoryModelRegion.EMemoryModelRegionPageDirectories;
				}
				else if ( aAddress >= 0x61020000 && aAddress < 0x6103FFFF )
				{
					// 61020000-6103FFFF	Page table info (4096 * 8bytes = 32K)
					ret = TMemoryModelRegion.EMemoryModelRegionPageTableInfo;
				}
				else if ( aAddress >= 0x61100000 && aAddress < 0x611FFFFF )
				{
					// 61100000-611FFFFF	Cache flush area
					ret = TMemoryModelRegion.EMemoryModelRegionCacheFlushArea;
				}
				else if ( aAddress >= 0x61200000 && aAddress < 0x612FFFFF )
				{
					// 61200000-612FFFFF	Alternate cache flush area
					ret = TMemoryModelRegion.EMemoryModelRegionCacheFlushAreaAlternate;
				}
				else if ( aAddress >= 0x62000000 && aAddress < 0x623FFFFF )
				{
					// 62000000-623FFFFF	Page tables (up to 4096 * 1K)
					ret = TMemoryModelRegion.EMemoryModelRegionPageTables;
				}
				else if ( aAddress >= 0x63000000 && aAddress < 0x63FFFFFF )
				{
					// 63000000-63FFFFFF	Primary I/O mappings
					ret = TMemoryModelRegion.EMemoryModelRegionPrimaryIOMappings;
				}
				else if ( aAddress >= 0x64000000 && aAddress < 0x64FFFFFF )
				{
					// 64000000-64FFFFFF	Kernel .data/.bss, initial stack, kernel heap
					ret = TMemoryModelRegion.EMemoryModelRegionKernelGlobalsInitialStackKernelHeap;
				}
				else if ( aAddress >= 0x65000000 && aAddress < 0x655FFFFF )
				{
					// 65000000-655FFFFF	fixed processes - usually 2 or 3Mb each.
					ret = TMemoryModelRegion.EMemoryModelRegionFixedProcesses;
				}
				else if ( aAddress >= 0x65600000 && aAddress < 0xF1FFFFFF )
				{
					// 65600000-F1FFFFFF	Kernel section (includes extra I/O mappings)
					ret = TMemoryModelRegion.EMemoryModelRegionExtraKernelMappings;
				}
				else if ( aAddress >= 0xF2000000 && aAddress < 0xF3FFFFFF )
				{
					// F2000000-F3FFFFFF	Kernel code (RAM size/2)
					ret = TMemoryModelRegion.EMemoryModelRegionKernelCode;
				}
				else if ( aAddress >= 0xF4000000 && aAddress < 0xF7FFFFFF )
				{
					// F4000000-F7FFFFFF	User code (RAM size)
					ret = TMemoryModelRegion.EMemoryModelRegionUserCode;
				}
				else if ( aAddress >= 0xF8000000 && aAddress < 0xFFEFFFFF )
				{
					// F8000000-FFEFFFFF	ROM
					ret = TMemoryModelRegion.EMemoryModelRegionROM;
				}
				else if ( aAddress >= 0xFFF00000 && aAddress < 0xFFFFFFFF )
				{
					// FFF00000-FFFFFFFF	Exception vectors
					ret = TMemoryModelRegion.EMemoryModelRegionExceptionVectors;
				}
				#endregion
			}
			else if ( aType == TMemoryModelType.EMemoryModelMultiple )
			{
				#region Multiple Memory Model
				if	( aAddress >= 0x00000000 && aAddress < 0x003FFFFF )
				{
					// 00000000-003FFFFF	Unmapped
					ret = TMemoryModelRegion.EMemoryModelRegionUnmapped;
				}
				else if ( aAddress >= 0x00400000 && aAddress < 0x6FFFFFFF )
				{
					// Skip overlapping 2gb vs 1gb regions
					ret = TMemoryModelRegion.EMemoryModelRegionUnknown;
				}
                else if ( aAddress >= 0x70000000 && aAddress < 0x7FFFFFFF )
                {
                    // 70000000-7FFFFFFF	RAM Loaded Code
                    ret = TMemoryModelRegion.EMemoryModelRegionRAMLoadedCode;
                }
                else if ( aAddress >= 0x80000000 && aAddress < 0x8FFFFFFF )
				{
					// 80000000-8FFFFFFF	ROM
					ret = TMemoryModelRegion.EMemoryModelRegionROM;
				}
				else if ( aAddress >= 0x90000000 && aAddress < 0x9FFFFFFF )
				{
					// 90000000-9FFFFFFF	User Global Area
					ret = TMemoryModelRegion.EMemoryModelRegionUserGlobalArea;
				}
				else if ( aAddress >= 0xA0000000 && aAddress < 0xBFFFFFFF )
				{
					// A0000000-BFFFFFFF	RAM drive
					ret = TMemoryModelRegion.EMemoryModelRegionRAMDrive;
				}
				else if ( aAddress >= 0xC0000000 && aAddress < 0xC0001FFF )
				{
					// C0000000-C0001FFF	Super page/CPU page
					ret = TMemoryModelRegion.EMemoryModelRegionSuperAndCPUPages;
				}
				else if ( aAddress >= 0xC0040000 && aAddress < 0xC00403FF )
				{
					// C0040000-C00403FF	ASID info (256 ASIDs)
					ret = TMemoryModelRegion.EMemoryModelRegionASIDInfo;
				}
				else if ( aAddress >= 0xC0080000 && aAddress < 0xC00FFFFF )
				{
					// C0080000-C00FFFFF	Page table info	
					ret = TMemoryModelRegion.EMemoryModelRegionPageTableInfo;
				}
				else if ( aAddress >= 0xC1000000 && aAddress < 0xC13FFFFF )
				{
					// C1000000-C13FFFFF	Page directories (up to 256 * 16KB)
					ret = TMemoryModelRegion.EMemoryModelRegionPageDirectories;
				}
				else if ( aAddress >= 0xC2000000 && aAddress < 0xC5FFFFFF )
				{
					// C2000000-C5FFFFFF	Page tables
					ret = TMemoryModelRegion.EMemoryModelRegionPageTables;
				}
				else if ( aAddress >= 0xC6000000 && aAddress < 0xC6FFFFFF )
				{
					// C6000000-C6FFFFFF	Primary I/O mappings
					ret = TMemoryModelRegion.EMemoryModelRegionPrimaryIOMappings;
				}
				else if ( aAddress >= 0xC7000000 && aAddress < 0xC7FFFFFF )
				{
					// C7000000-C7FFFFFF
					ret = TMemoryModelRegion.EMemoryModelRegionUnknown;
				}
				else if ( aAddress >= 0xC8000000 && aAddress < 0xC8FFFFFF )
				{
					// C8000000-C8FFFFFF	Kernel .data/.bss, initial stack, kernel heap
					ret = TMemoryModelRegion.EMemoryModelRegionKernelGlobalsInitialStackKernelHeap;
				}
				else if ( aAddress >= 0xC9000000 && aAddress < 0xC91FFFFF )
				{
					// C9000000-C91FFFFF	Kernel stacks
					ret = TMemoryModelRegion.EMemoryModelRegionKernelStacks;
				}
				else if ( aAddress >= 0xC9200000 && aAddress < 0xFFEFFFFF )
				{
					// C9200000-FFEFFFFF	Extra kernel mappings (I/O, RAM loaded device drivers)
					ret = TMemoryModelRegion.EMemoryModelRegionExtraKernelMappings;
				}
				else if ( aAddress >= 0xFFF00000 && aAddress < 0xFFFFFFFF )
				{
					// FFF00000-FFFFFFFF	Exception vectors
					ret = TMemoryModelRegion.EMemoryModelRegionExceptionVectors;
				}
				#endregion
			}
			//
			return ret;
		}
		#endregion
	}
}
