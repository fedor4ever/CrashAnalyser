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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.SerializedOperations;

namespace SymbolLib.CodeSegDef
{
	public class CodeSegResolver : ITracer
	{
		#region Constructors
        public CodeSegResolver()
            : this( null )
        {
        }

        public CodeSegResolver( ITracer aTracer )
		{
            iTracer = aTracer;
            iPrimer = new CodeSegDefinitionPrimer( this );
		}
		#endregion

        #region Constants
        public const string KSysBinPath = @"\sys\bin\";
        public const string KROMBinaryPath = "z:" + KSysBinPath;
        public const string KMapFileExtension = ".map";
        #endregion

        #region API
        public void Clear()
        {
            lock ( iResolvedEntries )
            {
                iResolvedEntries.Reset();
            }
        }

        public void Sort()
        {
            lock ( iResolvedEntries )
            {
                iResolvedEntries.Sort();
            }
        }

        public void Remove( CodeSegResolverEntry aEntry )
        {
            lock ( iResolvedEntries )
            {
                iResolvedEntries.Remove( aEntry );
            }
        }

        public void Add( CodeSegResolverEntry aEntry )
        {
            lock ( iResolvedEntries )
            {
                iResolvedEntries.Add( aEntry );
            }
        }

        public CodeSegDefinition Resolve( CodeSegDefinition aDefinition )
        {
            return Resolve( aDefinition, false );
        }

        public CodeSegDefinition Resolve( CodeSegDefinition aDefinition, bool aMapFileMustExistsWhenCreatingEntry )
        {
            CodeSegDefinition ret = aDefinition;

            // ... and now try to resolve real file name on the PC side
            string realPCMapFileName = ResolveByImageCodeSegmentFileName( ret.ImageFileNameAndPath );
            if ( realPCMapFileName != string.Empty )
            {
                ret.EnvironmentFileNameAndPath = realPCMapFileName;
            }
            else if ( aMapFileMustExistsWhenCreatingEntry == false )
            {
                MemoryModel.TMemoryModelType memModelType = MemoryModel.TypeByAddress( ret.AddressStart );
                MemoryModel.TMemoryModelRegion memModelRegion = MemoryModel.RegionByAddress( ret.AddressStart, memModelType );

                // Don't cache ROM code segments
                switch ( memModelRegion )
                {
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionRAMLoadedCode:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionUserGlobalArea:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionDLLStaticData:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionKernelGlobalsInitialStackKernelHeap:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionKernelCode:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionMovingProcessData:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionFixedProcesses:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionUserCode:
                        System.Diagnostics.Debug.WriteLine( "[WARNING] Map file for: " + ret.ImageFileNameAndPath + " was not found => creating empty MapFile for this RAM-loaded code seg." );
                        ret.EnvironmentFileNameAndPath = CreateDefaultReleaseFileName( ret.ImageFileName );
                        break;

                    // Common
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionUnmapped:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionRAMDrive:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionSuperAndCPUPages:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionPageTableInfo:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionPageDirectories:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionPageTables:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionPrimaryIOMappings:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionUnknown:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionExtraKernelMappings:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionExceptionVectors:
                    case MemoryModel.TMemoryModelRegion.EMemoryModelRegionROM:
        		
			        // Moving
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionCacheFlushArea:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionCacheFlushAreaAlternate:

			        // Multiple
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionSharedData:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionLocalData:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionASIDInfo:
			        case MemoryModel.TMemoryModelRegion.EMemoryModelRegionKernelStacks:
                        ret = null;
                        break;
                }
            }
            else
            {
                ret = null;
            }

            return ret;
        }

        public string ResolveByImageCodeSegmentFileName( string aFileName )
		{
			// See if the collection contains a suitable entry
            lock ( iResolvedEntries )
            {
                string ret = string.Empty;
                CodeSegResolverEntry entry = iResolvedEntries.FindByCodeSegmentFileNameAndPath( aFileName.ToLower() );
                if ( entry != null )
                {
                    ret = entry.EnvironmentFileNameAndPath;
                }
                //
                return ret;
            }
        }

		public string CreateDefaultReleaseFileName( string aImageFileName )
		{
			string ret = CombineWithDriveLetter( @"epoc32\release\armv5\urel\" + Path.GetFileName( aImageFileName ) );
			return ret;
		}

		public static string ConvertCodeSegToMapFileName( string aCodeSegmentName )
		{
			// We first must extract the actual binary (Dll/Exe) name:
			string name = Path.GetFileName( aCodeSegmentName );

			// We append the map extension (don't replace the original extension)
			name += CodeSegDefinition.KMapFileExtension;

			return name;
		}
		#endregion

        #region Properties
        public CodeSegDefinitionPrimer Primer
        {
            get { return iPrimer; }
        }

        public int ResolvedEntryCount
        {
            get
            {
                lock ( iResolvedEntries )
                {
                    return iResolvedEntries.Count;
                }
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( aFormat, aParams );
            }
        }
        #endregion

		#region Internal methods
		internal string DriveLetter
		{
			get { return iPrimer.DriveLetter; }
            set { iPrimer.DriveLetter = value; }
		}

        protected string CombineWithDriveLetter( string aFileName )
        {
            string ret = string.Empty;
            lock ( this )
            {
                ret = Path.Combine( DriveLetter, aFileName ).ToLower();
            }
            return ret;
        }
        #endregion

		#region Data members
        private readonly CodeSegDefinitionPrimer iPrimer;
        private readonly ITracer iTracer;
        private CodeSegResolverEntryCollection iResolvedEntries = new CodeSegResolverEntryCollection( 50 );
		#endregion
    }
}
