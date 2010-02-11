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
using System.Text;
using System.IO;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Summarisable;

namespace CrashItemLib.Crash.Processes
{
    #region Attributes
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    #endregion
    public class CIProcess : CIElement, ICISymbolManager
	{
		#region Constructors
        public CIProcess( CIContainer aContainer )
            : base( aContainer )
		{
            base.AddChild( new CISymbolDictionary( aContainer ) );
		}
		#endregion

        #region API
        public CIThread CreateThread()
        {
            CIThread ret = new CIThread( this );
            base.AddChild( ret );
            return ret;
        }

        public CICodeSeg CreateCodeSeg( string aName, uint aBase, uint aLimit )
        {
            CICodeSeg ret = CreateCodeSeg( aName, aBase, aLimit, true );
            return ret;
        }
        #endregion

        #region Properties
        [CIDBAttributeCell( "UIDs", 5 )]
        public UidType Uids
        {
            get { return iUids; }
        }

        [CIDBAttributeCell( "Generation", 3 )]
        public int Generation
        {
            get { return iGeneration; }
            set { iGeneration = value; }
        }

        [CIDBAttributeCell( "Priority", 4, 0 )]
        public int Priority
        {
            get { return iPriority; }
            set { iPriority = value; }
        }

        [CIDBAttributeCell( "SID", 2, "x8", 0u )]
        public uint SID
        {
            get
            {
                uint ret = iSID;
                //
                if ( ret == 0 && iUids.MostSignificant != 0 )
                {
                    ret = iUids.MostSignificant;
                }
                //
                return ret; 
            }
            set 
            { 
                iSID = value;

                // Keep UID3 in line with SID
                if ( Uids[ 2 ] != value )
                {
                    Uids[ 2 ] = value;
                }
            }
        }

        [CIDBAttributeCell( "Name", 1 )]
        public override string Name
        {
            get { return iName; }
            set 
            { 
                iName = value; 

                // Make sure it ends in .exe
                if ( !iName.ToLower().EndsWith( KProcessExtension ) )
                {
                    iName += KProcessExtension;
                }
            }
        }

        public CICodeSegList CodeSegments
        {
            get
            {
                CIElementList<CICodeSeg> codeSegs = base.ChildrenByType<CICodeSeg>();

                // Sort them
                Comparison<CICodeSeg> comparer = delegate( CICodeSeg aLeft, CICodeSeg aRight )
                {
                    return string.Compare( aLeft.Name, aRight.Name, true );
                };
                codeSegs.Sort( comparer );

                CICodeSegList ret = new CICodeSegList( this, codeSegs );
                return ret; 
            }
        }

        public CIThread[] Threads
        {
            get
            {
                CIElementList<CIThread> list = base.ChildrenByType<CIThread>();
                CIThread[] ret = list.ToArray();
                return ret;
            }
        }

        public CIThread[] ThreadsWhichExitedAbnormally
        {
            get
            {
                // We use an anonymous delegate (predicate in this case) to search through
                // all the direct children of this object that have crashed.
                CIElementList<CIThread> threads = base.ChildrenByType<CIThread>( 
                    delegate(CIThread aThread )
                    {
                        return aThread.IsAbnormalTermination;
                    }
                );
                return threads.ToArray();
            }
        }

        public CIThread PrimaryThread
        {
            get
            {
                CIThread ret = null;
                //
                CIElementList<CIThread> threads = this.ChildrenByType<CIThread>();
                if ( threads.Count == 1 )
                {
                    ret = threads[ 0 ];
                }
                else
                {
                    // Assumption: The main thread (at least in symbian OS) is the thread which has
                    // the closest id to that of the process itself.
                    CIElementId minDelta = int.MaxValue;
                    foreach ( CIThread thread in this.GetEnumeratorThreads() )
                    {
                        CIElementId delta = thread.Id - this.Id;
                        //
                        if ( delta < minDelta )
                        {
                            ret = thread;
                        }
                        if ( delta == 1 )
                        {
                            // Optimisation
                            break;
                        }
                    }
                }
                //
                return ret;
            }
        }

        public CIThread FirstCrashedThread
        {
            get
            {
                CIThread  ret = null;

                // Try the primary thread first
                CIThread primary = PrimaryThread;
                if ( primary != null && primary.IsAbnormalTermination )
                {
                    ret = primary;
                }
                else
                {
                    // The primary summary is the first summary we can locate
                    // that relates to a crash.
                    foreach ( CIThread thread in this.GetEnumeratorThreads() )
                    {
                        bool isCrash = thread.IsAbnormalTermination;
                        if ( isCrash )
                        {
                            // Try to find corresponding summarisable entry
                            ret = thread;
                            break;
                        }
                    }
                }
                //
                return ret;
            }
        }

        public CISymbolDictionary Symbols
        {
            get { return base.ChildByType( typeof( CISymbolDictionary ) ) as CISymbolDictionary; }
        }

        public bool IsAbnormalTermination
        {
            get
            {
                bool ret = false;
                //
                foreach ( CIThread thread in this.GetEnumeratorThreads() )
                {
                    if ( thread.IsAbnormalTermination )
                    {
                        ret = true;
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Enumerators
        public IEnumerable<CIThread> GetEnumeratorThreads()
        {
            return Threads;
        }

        public IEnumerable<CICodeSeg> GetEnumeratorCodeSegs()
        {
            return CodeSegments;
        }
        #endregion

        #region Internal constants
        private const string KProcessExtension = ".exe";
        private const string KUnknownProcessName = "UnknownProcess.exe";
        private const int KBaseOperationMultiplier = 5;
        #endregion

        #region Internal methods
        private CICodeSeg CreateCodeSeg( string aName, uint aBase, uint aLimit, bool aExplict )
        {
            CICodeSeg ret = new CICodeSeg( this );
            ret.Name = aName;
            ret.Base = aBase;
            ret.Limit = aLimit;
            ret.IsExplicit = aExplict;

            // Primary store is the child nodes
            base.AddChild( ret );

            base.Trace( "[CIProcess] CreateCodeSeg() - this: {0}, ret: {1}", this, ret );
            return ret;
        }

        private void EnsureCodeSegmentExistsForSymbol( CIElementFinalizationParameters aParams, CISymbol aSymbol )
        {
            if ( !aSymbol.IsNull )
            {
                Symbol symbol = aSymbol;
                SymbolCollection collection = symbol.Collection;
                //
                string binaryInDevice = PlatformFileNameConstants.Device.KPathWildcardSysBin;
                binaryInDevice += Path.GetFileName( collection.FileName.EitherFullNameButDevicePreferred );
                //
                CICodeSegList codeSegs = CodeSegments;
                bool alreadyExists = codeSegs.Contains( binaryInDevice );
                if ( !alreadyExists )
                {
                    // Assume no match found - create implicit/speculative code segment
                    AddressRange newCodeSegRange = collection.SubsumedPrimaryRange;
                    CICodeSeg newCodeSeg = CreateCodeSeg( binaryInDevice, newCodeSegRange.Min, newCodeSegRange.Max, false );

                    base.Trace( "[CIProcess] EnsureCodeSegmentExistsForSymbol() - creating implicitly identified code seg: " + newCodeSeg.ToString() + " for symbol: " + aSymbol.ToString() );

                    // Resolve it
                    newCodeSeg.Resolve( aParams.DebugEngineView );
                }
            }
        }

        private void DiscardImplicitCodeSegments()
        {
            // Go through each child and see if it's a code seg. If it is, and if 
            // it is implicit, throw it away
            int childCount = base.Count;
            for( int i=childCount-1; i>=0; i-- )
            {
                CIElement element = base[ i ];
                if ( element is CICodeSeg )
                {
                    CICodeSeg cs = (CICodeSeg) element;
                    //
                    if ( !cs.IsExplicit )
                    {
                        base.Trace( string.Format( "[CIProcess] DiscardImplicitCodeSegments() - dicarded: {0}", cs ) );
                        base.RemoveChild( cs );
                    }
                }
            }
        }

        private void CreateImplicitCodeSegments( CIElementFinalizationParameters aParams )
        {
            CIElementList<CISymbol> children = base.ChildrenByType<CISymbol>( TChildSearchType.EEntireHierarchy );
            base.Trace( string.Format( "[CIProcess] CreateImplicitCodeSegments() - children count: {1}, {0}", this, children.Count ) );
            //
            foreach ( CISymbol symbol in children )
            {
                EnsureCodeSegmentExistsForSymbol( aParams, symbol );
            }
        }
        #endregion

        #region From CIElement
        internal override void DoFinalize( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize )
        {
            base.Trace( string.Format( "[CIProcess] DoFinalize() - START - {0}", this ) );

            // The process' children need to use a process-relative debug engine view in order that they
            // can correctly resolve any RAM-loaded code.
            // Therefore, rather than use the so-called "global" debug engine view (which only has
            // XIP visibility) we create a process-specific set of finalization parameters (for use with 
            // the process' children) which contain a process-relative view of the world.
            using ( CIElementFinalizationParameters processRelativeParameters = new CIElementFinalizationParameters( aParams.Engine, this.Name, this.CodeSegments ) )
            {
                // Discard any implicit code segments. These are created automagically when an XIP symbol
                // is found. We'll re-create them anyway in a moment after the symbols have been updated.
                base.Trace( string.Format( "[CIProcess] DoFinalize() - discarding implicit XIP CodeSegs... - {0}", this ) );
                DiscardImplicitCodeSegments();
                base.Trace( string.Format( "[CIProcess] DoFinalize() - discarded implicit XIP CodeSegs - {0}", this ) );

                // Tell our children
                base.DoFinalize( processRelativeParameters, aCallBackLast, aForceFinalize );

                // Finally, re-create implicit XIP codesegments
                base.Trace( string.Format( "[CIProcess] DoFinalize() - creating implicit XIP CodeSegs... - {0}", this ) );
                CreateImplicitCodeSegments( aParams );
                base.Trace( string.Format( "[CIProcess] DoFinalize() - created implicit XIP CodeSegs - {0}", this ) );
            }
            //
            base.Trace( string.Format( "[CIProcess] DoFinalize() - END - {0}", this ) );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region From ICISymbolManager
        public CISymbolDictionary SymbolDictionary
        {
            get { return this.Symbols; }
        }
        #endregion

        #region Data members
        private uint iSID = 0;
        private int iPriority = 0;
        private int iGeneration = 1;
        private UidType iUids = new UidType();
        private string iName = KUnknownProcessName;
        #endregion
    }
}
