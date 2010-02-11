/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.CodeSegments;
using SymbianSymbolLib.DbgEnginePlugin;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.QueryAPI;

namespace SymbianSymbolLib.Relocator
{
    internal class SymbolRelocator : IEnumerable<SymSourceAndCollection>
    {
        #region Constructors
        public SymbolRelocator( SymbolPlugin aPlugin )
		{
            iPlugin = aPlugin;
		}
		#endregion

        #region API
        public SymbolCollection Activate( CodeSegDefinition aCodeSegment )
        {
            SymbolCollection ret = null;

            // Find the corresponding code seg
            SymSourceAndCollection pair = SourceManager[ aCodeSegment ];
            if ( pair != null  )
            {
                SymbolCollection col = pair.Collection;
                lock ( col.SyncRoot )
                {
                    if ( col.IsFixed )
                    {
                        // Cannot activate a fixed code segment - TODO: should this return "true", i.e. already activated?
                    }
                    else
                    {
                        bool safe = CheckSafeToActivate( aCodeSegment );
                        if ( safe )
                        {
                            // Deep copy the collection
                            SymbolCollection dupe = SymbolCollection.NewCopy( iPlugin.ProvisioningManager.IdAllocator, col );

                            // Set new process-specific relocated base address
                            dupe.Relocate( aCodeSegment.Base );

                            // Save so that we can unload it later
                            pair = new SymSourceAndCollection( pair, dupe );
                            AddToActivationList( pair, aCodeSegment );

                            // We managed to activate a binary, so return the collection
                            ret = dupe;

                            iPlugin.Trace( "[S] ACTIVATE - {0}", aCodeSegment );
                        }
                    }
                }
            }
            //
            return ret;
        }

        public bool Deactivate( CodeSegDefinition aCodeSegment )
        {
            bool activated = iActivationLUT.ContainsKey( aCodeSegment );
            if ( activated )
            {
                SymSourceAndCollection pair = iActivationLUT[ aCodeSegment ];
                //
                lock ( iActivatedCollections )
                {
                    iActivatedCollections.Remove( pair.Collection );
                }
                lock ( iActivationLUT )
                {
                    iActivationLUT.Remove( aCodeSegment );
                }
                //
                iPlugin.Trace( "[S] DEACTIVATE - {0} @ {1}", pair.Collection.FileName, aCodeSegment );
            }
            //
            return activated;
        }
        #endregion

		#region Properties
        public SymbolCollectionList CollectionList
        {
            get { return iActivatedCollections; }
        }

        public int Count
        {
            get
            {
                lock ( iActivatedCollections )
                {
                    lock ( iActivationLUT )
                    {
                        System.Diagnostics.Debug.Assert( iActivatedCollections.Count == iActivationLUT.Count );
                        return iActivationLUT.Count;
                    }
                }
            }
        }
        #endregion

        #region Internal methods
        internal SymbolPlugin Plugin
        {
            get { return iPlugin; }
        }

        internal SymSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }

        private void AddToActivationList( SymSourceAndCollection aEntry, CodeSegDefinition aCodeSegment )
        {
            lock ( iActivatedCollections )
            {
                iActivatedCollections.AddAndBuildCache( aEntry.Collection );
            }
            lock ( iActivationLUT )
            {
                iActivationLUT.Add( aCodeSegment, aEntry );
            }
        }

        private bool CheckSafeToActivate( CodeSegDefinition aCodeSegment )
        {
            lock ( iActivationLUT )
            {
                bool alreadyExists = iActivationLUT.ContainsKey( aCodeSegment );
                if ( alreadyExists )
                {
                    // Specified code segment already activated
                    return false;
                }
                else
                {
                    // We must check that there's no overlap in activation ranges between code segments.
                    foreach ( KeyValuePair<CodeSegDefinition, SymSourceAndCollection> kvp in iActivationLUT )
                    {
                        AddressRange range = kvp.Key;
                        if ( range.Contains( aCodeSegment ) )
                        {
                            // Overlaps with existing activated code segment
                            return false;
                        }
                    }
                }
                //
                return true;
            }
        }
        #endregion

        #region From IEnumerable<SymSourceAndCollection>
        public IEnumerator<SymSourceAndCollection> GetEnumerator()
        {
            foreach ( KeyValuePair<CodeSegDefinition, SymSourceAndCollection> kvp in iActivationLUT )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<CodeSegDefinition, SymSourceAndCollection> kvp in iActivationLUT )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region Data members
        private readonly SymbolPlugin iPlugin;
        private SymbolCollectionList iActivatedCollections = new SymbolCollectionList();
        private Dictionary<CodeSegDefinition, SymSourceAndCollection> iActivationLUT = new Dictionary<CodeSegDefinition, SymSourceAndCollection>();
        #endregion
    }
}
