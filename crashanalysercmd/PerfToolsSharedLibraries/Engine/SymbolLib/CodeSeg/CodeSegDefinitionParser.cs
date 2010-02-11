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
using System.Text.RegularExpressions;
using SymbianUtils;
using SymbolLib.Sources.Map.Engine;

namespace SymbolLib.CodeSegDef
{
	public class CodeSegDefinitionParser
	{
        #region Constructors & destructor
        public CodeSegDefinitionParser()
        {
            iResolver = null;
        }

	    public CodeSegDefinitionParser( CodeSegResolver aResolver )
		{
            iResolver = aResolver;
		}
        #endregion

        #region Properties
        public bool MapFileMustExistsWhenCreatingEntry
        {
            get { return iMapFileMustExistsWhenCreatingEntry; }
            set { iMapFileMustExistsWhenCreatingEntry = value; }
        }
        #endregion

        #region API
        public CodeSegDefinition ParseDefinition( string aLine )
        {
            CodeSegDefinition ret = null;
            //
            Match m = iCodeSegRegEx.Match( aLine );
            if ( m.Success )
            {
                ret = new CodeSegDefinition();
                //
                string gpAddressStart = m.Groups[ "StartAddress" ].Value;
                string gpAddressEnd = m.Groups[ "EndAddress" ].Value;
                string gpBinary = m.Groups[ "Binary" ].Value;
                //
                ret.AddressStart = uint.Parse( gpAddressStart, System.Globalization.NumberStyles.HexNumber );
                ret.AddressEnd = uint.Parse( gpAddressEnd, System.Globalization.NumberStyles.HexNumber );
                ret.ImageFileNameAndPath = gpBinary;
            }
            //
            return ret;
        }

        public CodeSegDefinition ParseAndResolveDefinition( string aLine )
        {
            if ( iResolver == null )
            {
                throw new Exception( "Resolver is not initialised" );
            }
            //
            CodeSegDefinition ret = null;
            CodeSegDefinition entry = ParseDefinition( aLine );
            if ( entry != null )
            {
                ret = iResolver.Resolve( entry, MapFileMustExistsWhenCreatingEntry );
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private static readonly Regex iCodeSegRegEx = new Regex(
            @"(?<StartAddress>[a-fA-F0-9]{8})-(?<EndAddress>[a-fA-F0-9]{8})\s{1}(?<Binary>.+)",
            RegexOptions.IgnoreCase
            );
        private const int KBaseHex = 16;
        #endregion

        #region Data members
        private readonly CodeSegResolver iResolver;
        private bool iMapFileMustExistsWhenCreatingEntry = false;
        #endregion
    }
}
