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
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace SymbianParserLib.RegExTranslators.Types
{
    internal class RegExTranslatorString : RegExTranslatorBase
    {
        #region Constructors
        public RegExTranslatorString()
        {
        }
        #endregion

        #region API
        public override ParserField Process( Capture aCapture, int aStartAt, ParserLine aLine )
        {
            ParserField ret = null;
            //
            RegExTranslatorExtractionInfo m = new RegExTranslatorExtractionInfo( aLine.OriginalValue, aStartAt );
            //
            if ( m.Success && m.ValueType == TParserValueType.EValueTypeString )
            {
                ret = CreateField( ".+", m.Name, m.ValueType, m.CapturePos, m.CaptureLength );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
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
        /// <summary>
        ///  Regular expression built for C# on: Thu, May 15, 2008, 01:10:03 PM
        ///  Using Expresso Version: 3.0.2766, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  %
        ///  [1]: A numbered capture group. [s|S]
        ///      Select from 2 alternatives
        ///          sS
        ///  
        ///
        /// </summary>
        private static Regex KRegEx = new Regex(
              "%(s|S)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        #endregion
    }
}
