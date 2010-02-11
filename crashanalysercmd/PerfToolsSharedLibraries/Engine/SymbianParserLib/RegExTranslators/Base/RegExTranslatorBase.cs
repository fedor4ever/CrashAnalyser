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

namespace SymbianParserLib.RegExTranslators
{
    internal abstract class RegExTranslatorBase
    {
        #region Constructors
        public RegExTranslatorBase()
        {
        }
        #endregion

        #region API
        public abstract ParserField Process( Capture aCapture, int aStartAt, ParserLine aLine );
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        protected ParserField CreateField( string aRegEx, string aFieldName, TParserValueType aValueType, int aCapturePos, int aCaptureLength )
        {
            ParserField ret = CreateField( aRegEx, aFieldName, aValueType, aCapturePos, aCaptureLength, true );
            return ret;
        }

        protected ParserField CreateField( string aRegEx, string aFieldName, TParserValueType aValueType, int aCapturePos, int aCaptureLength, bool aRequiresNumberedCaptureGroup )
        {
            string fieldName = CompressName( aFieldName );
            ParserField ret = new ParserField( fieldName );

            // Update format specifier
            ParserFieldFormatSpecifier formatSpecifier = ret.FormatSpecifier;
            formatSpecifier.OriginalLocation = aCapturePos;
            formatSpecifier.OriginalLength = aCaptureLength;

            // Surround in numbered group if needed...
            StringBuilder finalRegEx = new StringBuilder( aRegEx );
            if ( aRequiresNumberedCaptureGroup )
            {
                finalRegEx.Insert( 0, "(" );
                finalRegEx.Append( ")" );
            }

            formatSpecifier.RegularExpressionString = finalRegEx.ToString();
            formatSpecifier.ExpectedType = aValueType;

            return ret;
        }

        private static string CompressName( string aUncompressedFieldName )
        {
            Regex spaces = new Regex(@"\s+");
            string[] fields = spaces.Split( aUncompressedFieldName );
            //
            StringBuilder ret = new StringBuilder();
            foreach ( string field in fields )
            {
                ret.Append( field );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        #endregion
    }
}
