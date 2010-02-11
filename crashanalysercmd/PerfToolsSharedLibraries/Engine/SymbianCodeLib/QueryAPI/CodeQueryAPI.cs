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
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianUtils.Range;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.Relocator;

namespace SymbianCodeLib.QueryAPI
{
    internal class CodeQueryAPI : IEnumerable<CodeCollection>
    {
        #region Constructors
        internal CodeQueryAPI( CodeRelocator aRelocator )
		{
            iRelocator = aRelocator;
		}
		#endregion

        #region API
        public bool Contains( uint aAddress )
        {
            // First check with the relocated/activated symbol collections,
            // i.e. RAM-loaded code that has been fixed up.
            bool ret = iRelocator.CollectionList.Contains( aAddress );
            if ( ret == false )
            {
                // Wasn't a relocated symbol collection, so search through
                // all sources for ROM/XIP symbols that might match.
                foreach ( CodeSource source in SourceManager )
                {
                    if ( source.Contains( aAddress ) )
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }

        public bool GetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            // First check with the relocated/activated code collections,
            // i.e. RAM-loaded code that has been fixed up.
            bool ret = iRelocator.CollectionList.GetInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
            if ( ret == false )
            {
                foreach ( CodeSource source in SourceManager )
                {
                    if ( source.Contains( aAddress ) )
                    {
                        ret = source.ProvideInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion

		#region Properties
        public CodeCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get
            {
                CodeCollection ret = null;
                CodeSourceAndCollection pair = SourceManager[ aCodeSeg ];
                if ( pair != null )
                {
                    ret = pair.Collection;
                }
                return ret;
            }
        }

        public CodeCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                CodeCollection ret = null;
                //
                foreach ( CodeSource source in SourceManager )
                {
                    CodeCollection col = source[ aFileName ];
                    if ( col != null )
                    {
                        ret = col;
                        break;
                    }
                }
                //
                return ret;
            }
        }
		#endregion

        #region Internal methods
        internal CodeSourceManager SourceManager
        {
            get { return iRelocator.SourceManager; }
        }
        #endregion

        #region From IEnumerable<CodeCollection>
        public IEnumerator<CodeCollection> GetEnumerator()
        {
            // This gives us explicit activations
            CodeCollectionList list = iRelocator.CollectionList;
            foreach ( CodeCollection col in list )
            {
                yield return col;
            }

            // Next we need fixed collections
            IEnumerable<CodeCollection> fixedCols = iRelocator.SourceManager.GetFixedCollectionEnumerator();
            foreach ( CodeCollection col in fixedCols )
            {
                if ( col.IsFixed )
                {
                    yield return col;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // This gives us explicit activations
            CodeCollectionList list = iRelocator.CollectionList;
            foreach ( CodeCollection col in list )
            {
                yield return col;
            }

            // Next we need fixed collections
            IEnumerable<CodeCollection> fixedCols = iRelocator.SourceManager.GetFixedCollectionEnumerator();
            foreach ( CodeCollection col in fixedCols )
            {
                if ( col.IsFixed )
                {
                    yield return col;
                }
            }
        }
        #endregion

        #region Data members
        private readonly CodeRelocator iRelocator;
        #endregion
    }
}
