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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using SymbianParserLib.Enums;
using SymbianParserLib.Elements;
using SymbianParserLib.Elements.SubFields;

namespace SymbianParserLib.RegExTranslators.Cache
{
    internal class RegExTranslatorCache
    {
        #region Constructors
        public RegExTranslatorCache()
        {
        }
        #endregion

        #region API
        public ParserLine CreateClone( string aKey )
        {
            ParserLine ret = null;
            //
            lock ( iEntries )
            {
                if ( Exists( aKey ) )
                {
                    RegExTranslatorCacheEntry entry = iEntries[ aKey ];
                    ret = entry.Clone();
                }
            }
            //
            return ret;
        }

        public void Add( string aKey, ParserLine aLine )
        {
            System.Diagnostics.Debug.Assert( !string.IsNullOrEmpty( aKey ) );
            System.Diagnostics.Debug.Assert( aLine != null );
            System.Diagnostics.Debug.Assert( !string.IsNullOrEmpty( aLine.OriginalValue ) );
            System.Diagnostics.Debug.Assert( !string.IsNullOrEmpty( aLine.FinalValue ) );
            System.Diagnostics.Debug.Assert( iEntries != null );
            
            try
            {
                lock ( iEntries )
                {
                    if ( Exists( aKey ) == false )
                    {
                        RegExTranslatorCacheEntry entry = new RegExTranslatorCacheEntry( aLine );
                        iEntries.Add( aKey, entry );
                    }
                }
            }
            catch ( Exception )
            {
                SymbianUtils.SymDebug.SymDebugger.Break();
            }
        }

        public bool Exists( string aKey )
        {
            lock ( iEntries )
            {
                bool ret = iEntries.ContainsKey( aKey );
                return ret;
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private Dictionary<string, RegExTranslatorCacheEntry> iEntries = new Dictionary<string, RegExTranslatorCacheEntry>();
        #endregion
    }
}
