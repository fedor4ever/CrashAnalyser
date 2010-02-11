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
using System.Reflection;

namespace CrashDebuggerLib.Attributes
{
    public class PropCat : Attribute
    {
        #region Enumerations
        public enum TFormatType
        {
            EFormatAsString = 0,
            EFormatAsDecimal,
            EFormatAsHex,
            EFormatAsHexWithoutPrefix,
            EFormatAsYesNo,
            EFormatRecurseInto
        }
        #endregion

        #region Constructors
        public PropCat( string aCategory )
            : this( aCategory, string.Empty )
        {
        }

        public PropCat( string aCategory, string aTitle )
            : this( aCategory, aTitle, TFormatType.EFormatAsString )
        {
        }

        public PropCat( string aCategory, TFormatType aFormatType )
            : this( aCategory, string.Empty, aFormatType )
        {
        }

        public PropCat( string aCategory, string aTitle, TFormatType aFormatType )
            : this( aCategory, aTitle, aFormatType, string.Empty )
        {
        }

        public PropCat( string aCategory, string aTitle, TFormatType aFormatType, string aNumericalFormat )
        {
            iCategory = aCategory;
            iTitle = aTitle;
            iFormatType = aFormatType;
            iNumericalFormat = aNumericalFormat;
        }
        #endregion

        #region Properties
        public string Category
        {
            get { return iCategory; }
        }

        public string Title
        {
            get { return iTitle; }
        }

        public string NumericalFormat
        {
            get
            {
                string ret = iNumericalFormat;
                //
                if ( String.IsNullOrEmpty( ret ) )
                {
                    switch ( FormatType )
                    {
                    case TFormatType.EFormatAsDecimal:
                        ret = "d";
                        break;
                    case TFormatType.EFormatAsHex:
                        ret = "x";
                        break;
                    }
                }
                //
                return ret;
            }
        }

        public TFormatType FormatType
        {
            get { return iFormatType; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly string iCategory;
        private readonly string iTitle;
        private readonly string iNumericalFormat;
        private readonly TFormatType iFormatType;
        #endregion
    }
}