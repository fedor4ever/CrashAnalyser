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
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.CodeSegments;
using SymbianCodeLib.DbgEnginePlugin;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.QueryAPI;

namespace SymbianCodeLib.Relocator
{
    internal class CodeRelocator : DisposableObject, IEnumerable<CodeSourceAndCollection>
    {
        #region Constructors
        public CodeRelocator( CodePlugin aPlugin )
		{
            iPlugin = aPlugin;
		}
		#endregion

        #region API
        public CodeCollection Activate( CodeSegDefinition aCodeSegment )
        {
            CodeCollection ret = null;

            // Find the corresponding Code seg
            CodeSourceAndCollection pair = SourceManager[ aCodeSegment ];
            if ( pair != null  )
            {
                CodeCollection col = pair.Collection;
                if ( col.IsFixed )
                {
                    // Cannot activate a fixed Code segment - TODO: should this return "true", i.e. already activated?
                }
                else
                {
                    bool safe = CheckSafeToActivate( aCodeSegment );
                    if ( safe )
                    {
                        // Deep copy the collection
                        CodeCollection dupe = CodeCollection.NewCopy( iPlugin.ProvisioningManager.IdAllocator, col );

                        // Set new process-specific relocated base address. This causes the underlying code to be
                        // decompressed/read if not already done so.
                        dupe.Relocate( aCodeSegment.Base );

                        // At this point, the code owned by 'col' will have definitely been read (if available)
                        // and that means col.IsCodeAvailable is probably true. However, dupe.IsCodeAvailable
                        // is almost certainly false so we may need to copy the code over...
                        if ( dupe.IsCodeAvailable == false && col.IsCodeAvailable == true )
                        {
                            dupe.Code = col.Code;
                        }
                        System.Diagnostics.Debug.Assert( dupe.IsCodeAvailable == col.IsCodeAvailable );

                        // Save so that we can unload it later
                        pair = new CodeSourceAndCollection( pair, dupe );
                        AddToActivationList( pair, aCodeSegment );

                        // We managed to activate a binary, so return the collection
                        ret = dupe;

                        iPlugin.Trace( "[C] ACTIVATE - {0}", aCodeSegment );
                    }
                }
            }
            //
            return ret;
        }

        public bool Deactivate( CodeSegDefinition aCodeSegment )
        {
            bool activated = iActivationLUT.ContainsKey( aCodeSegment );
            //
            if ( activated )
            {
                CodeSourceAndCollection pair = iActivationLUT[ aCodeSegment ];
                //
                iActivatedCollections.Remove( pair.Collection );
                iActivationLUT.Remove( aCodeSegment );
                //
                iPlugin.Trace( "[C] DEACTIVATE - {0} @ {1}", pair.Collection.FileName, aCodeSegment );
            }
            //
            return activated;
        }
        #endregion

		#region Properties
        public CodeCollectionList CollectionList
        {
            get { return iActivatedCollections; }
        }

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert( iActivatedCollections.Count == iActivationLUT.Count );
                return iActivationLUT.Count;
            }
        }
        #endregion

        #region Internal methods
        internal CodePlugin Plugin
        {
            get { return iPlugin; }
        }

        internal CodeSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }

        private void AddToActivationList( CodeSourceAndCollection aEntry, CodeSegDefinition aCodeSegment )
        {
            lock ( iActivatedCollections )
            {
                iActivatedCollections.Add( aEntry.Collection );
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
                    // Specified Code segment already activated
                    return false;
                }
                else
                {
                    // We must check that there's no overlap in activation ranges between Code segments.
                    foreach ( KeyValuePair<CodeSegDefinition, CodeSourceAndCollection> kvp in iActivationLUT )
                    {
                        AddressRange range = kvp.Key;
                        if ( range.Contains( aCodeSegment ) )
                        {
                            // Overlaps with existing activated Code segment
                            return false;
                        }
                    }
                }
                //
                return true;
            }
        }
        #endregion

        #region From IEnumerable<CodeSourceAndCollection>
        public IEnumerator<CodeSourceAndCollection> GetEnumerator()
        {
            foreach ( KeyValuePair<CodeSegDefinition, CodeSourceAndCollection> kvp in iActivationLUT )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<CodeSegDefinition, CodeSourceAndCollection> kvp in iActivationLUT )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                foreach ( CodeCollection col in iActivatedCollections )
                {
                    // Fixed collections are immediately discarded
                    if ( !col.IsFixed )
                    {
                        col.Dispose();
                    }
                }

                iActivatedCollections.Clear();
                iActivationLUT.Clear();
            }
        }
        #endregion

        #region Data members
        private readonly CodePlugin iPlugin;
        private CodeCollectionList iActivatedCollections = new CodeCollectionList();
        private Dictionary<CodeSegDefinition, CodeSourceAndCollection> iActivationLUT = new Dictionary<CodeSegDefinition, CodeSourceAndCollection>();
        #endregion
    }
}
