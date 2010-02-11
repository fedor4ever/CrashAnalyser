#define TRACK_INTERNING_STATISTICS
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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SymbianUtils.Range;
using SymbianUtils.Strings;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.MemoryModel;
using SymbianStructuresLib.Debug.Symbols.Constants;
using SymbianStructuresLib.Debug.Common.Id;

namespace SymbianStructuresLib.Debug.Symbols
{
	internal class InternedName
    {
        #region Static constructors
        public static InternedName NewExplicit( string aText )
        {
            InternedName ret = new InternedName( aText, string.Empty );
            return ret;
        }

        public static InternedName New( string aText )
        {
            InternedName ret = new InternedName( aText );
            return ret;
        }
        #endregion

        #region Constructors
        private InternedName( string aName )
        {
            int doubleColonPos = aName.LastIndexOf( "::" );
            if ( doubleColonPos > 0 )
            {
                iNamePart1 = aName.Substring( 0, doubleColonPos + 2 );
                iNamePart2 = aName.Substring( doubleColonPos + 2 );
            }
            else if ( aName.Contains( "typeinfo for " ) )
            {
                iNamePart1 = "typeinfo for ";
                iNamePart2 = aName.Substring( 13 );
            }
            else if ( aName.Contains( "vtable for " ) )
            {
                iNamePart1 = "vtable for ";
                iNamePart2 = aName.Substring( 11 );
            }
            else
            {
                iNamePart1 = aName;
                iNamePart2 = string.Empty;
            }

            // Record intern stats
#if TRACK_INTERNING_STATISTICS
            UpdateStatistics();
#endif

            // Now get the interned strings
            iNamePart1 = string.Intern( iNamePart1 );
            iNamePart2 = string.Intern( iNamePart2 );
        }

        private InternedName( string aPart1, string aPart2 )
        {
            iNamePart1 = string.Intern( aPart1 );
            iNamePart2 = string.Intern( aPart2 );
        }
        #endregion

        #region API
        public static bool IsFunction( string aText )
        {
            bool ret = aText.Contains( "(" ) && aText.Contains( ")" );
            return ret;
        }

        public static bool IsVTable( string aText )
        {
            bool ret = StringUtils.StartsWithAny( SymbolConstants.KVTableOrTypeInfoPrefixes, aText );
            return ret;
        }

        public static bool IsReadOnly( string aText )
        {
            bool ret = aText.StartsWith( SymbolConstants.KPrefixReadonly );
            return ret;
        }

        public static bool IsSubObject( string aText )
        {
            bool ret = aText.EndsWith( SymbolConstants.KPrefixSubObject );
            return ret;
        }

        public void PrintStats()
        {
#if TRACK_INTERNING_STATISTICS
#endif
        }
        #endregion

        #region Internal methods
#if TRACK_INTERNING_STATISTICS
        private void UpdateStatistics()
        {
            UpdateStatistics( iNamePart1 );
            UpdateStatistics( iNamePart2 );
        }

        private void UpdateStatistics( string aText )
        {
            if ( string.IsInterned( aText ) != null )
            {
                InternStats[ 0 ].Update( aText ); 
            }
            else
            {
                InternStats[ 1 ].Update( aText );
            }
        }
#endif
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iNamePart1 + iNamePart2;
        }
        #endregion

        #region Internal classes
#if TRACK_INTERNING_STATISTICS
        private class InternStatistics
        {
            public void Update( string aText )
            {
                ++iCount;
                iSize += (uint) ( aText.Length * 2 );
            }
            //
            private uint iCount = 0;
            private ulong iSize = 0;
        }
#endif
        #endregion

        #region Data members
        private readonly string iNamePart1;
        private readonly string iNamePart2;

#if TRACK_INTERNING_STATISTICS
        private static readonly InternStatistics[] InternStats = new InternStatistics[] { new InternStatistics(), new InternStatistics() };
#endif
       #endregion
    }
}