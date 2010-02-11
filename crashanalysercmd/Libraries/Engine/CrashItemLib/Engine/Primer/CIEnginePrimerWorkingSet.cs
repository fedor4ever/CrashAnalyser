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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.PluginAPI;
using CrashItemLib.Crash.Source;
using CrashItemLib.Engine.Sources;

namespace CrashItemLib.Engine.Primer
{
    internal class CIPrimerWorkingSet
    {
        #region Constructors
        public CIPrimerWorkingSet( CIEngine aEngine )
		{
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Clear()
        {
            iEntries.Clear();
        }

        public void Add( CFFSourceAndConfidence aEntry )
        {
            // For every single source file we might actually have multiple
            // plugins that claim they can interpet the contents.
            // 
            // Therefore we maintain a dictionary that is keyed by the file name
            // and contains a list of plugin handlers (and their associatied
            // confidence level for handling the source file).
            List<CFFSourceAndConfidence> entries = null;
            //
            FileInfo file = aEntry.MasterFile;
            System.Diagnostics.Debug.Assert( file != null );
            //
            if ( !iEntries.ContainsKey( file ) )
            {
                entries = new List<CFFSourceAndConfidence>();
                iEntries.Add( file, entries );
            }
            else
            {
                entries = iEntries[ file ];
            }
            //
            entries.Add( aEntry );
        }

        public void Add( IEnumerable<CFFSourceAndConfidence> aEntries )
        {
            foreach ( CFFSourceAndConfidence level in aEntries )
            {
                Add( level );
            }
        }

        public void Rationalise()
        {
            // At this point we're ready to decide how we will process the source
            // files. Some may be trace files, in which case we probably need to multiple
            // the various plugin handlers (that support trace-based content) so that they
            // can each have a stab at handling the trace content.
            //
            // For native (non-trace) handlers, we only let the handler with the highest
            // confidence ultimately read the file.
            //
            // 1) For any native entries, sort them by priority and discard
            //    everything but the highest priority entry.
            //
            // 2) For any trace entries, then we basically don't need to do anything
            //    because all entries will require processing via a trace reader.
            foreach ( KeyValuePair<FileInfo, List<CFFSourceAndConfidence>> kvp in iEntries )
            {
                // For native entries relating to this file
                List<CFFSourceAndConfidence> listNative = new List<CFFSourceAndConfidence>();

                // For trace entries relating to this file
                List<CFFSourceAndConfidence> listTrace = new List<CFFSourceAndConfidence>();

                FileInfo file = kvp.Key;
                List<CFFSourceAndConfidence> entries = kvp.Value;
                foreach ( CFFSourceAndConfidence conf in entries )
                {
                    if ( conf.OpType == CFFSource.TReaderOperationType.EReaderOpTypeNative )
                    {
                        listNative.Add( conf );
                    }
                    else if ( conf.OpType == CFFSource.TReaderOperationType.EReaderOpTypeTrace )
                    {
                        listTrace.Add( conf );
                    }
                    else
                    {
                        // Not supported
                    }
                }

                // Sort the native list based upon confidence level
                Comparison<CFFSourceAndConfidence> comparer = delegate( CFFSourceAndConfidence aLeft, CFFSourceAndConfidence aRight )
                {
                    return aLeft.CompareTo( aRight );
                };
                listNative.Sort( comparer );

                // Save highest priority native entry - only try to treat the source file as a
                // trace entry if no native entries claim to be able to read it.
                if ( listNative.Count > 0 )
                {
                    CFFSourceAndConfidence highestConfidence = listNative[ 0 ];
                    CIEngineSource sourceNative = CIEngineSource.NewNative( iEngine, highestConfidence );
                    iEngine.Sources.Add( sourceNative );
                }
                else if ( listTrace.Count > 0 ) 
                {
                    // If we found some trace entries, then immediately store the trace source.
                    // The listTrace array contains all of the plugins that claim to be able to
                    // read the trace-based source file.
                    CIEngineSource sourceTrace = CIEngineSource.NewTrace( iEngine, listTrace.ToArray() );
                    iEngine.Sources.Add( sourceTrace );
                }
            }
        }
        #endregion

        #region Properties
        #endregion

		#region Data members
        private readonly CIEngine iEngine;
        private Dictionary< FileInfo, List<CFFSourceAndConfidence> > iEntries = new Dictionary< FileInfo, List<CFFSourceAndConfidence> >();
        #endregion
    }
}
