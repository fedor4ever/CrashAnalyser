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
using SymbianParserLib.BaseStructures;

namespace SymbianParserLib.Elements.SubFields
{
    internal class ParserFieldFormatSpecifier
    {
        #region Constructors
        public ParserFieldFormatSpecifier( ParserField aField )
        {
            iField = aField;
            iFieldName.Name = aField.Name;
        }

        public ParserFieldFormatSpecifier( ParserField aField, ParserFieldFormatSpecifier aCopy )
            : this( aField )
        {
            OriginalLocation = aCopy.OriginalLocation;
            OriginalLength = aCopy.OriginalLength;
            RegularExpressionString = aCopy.RegularExpressionString;
            NumberBase = aCopy.NumberBase;
            ExpectedType = aCopy.ExpectedType;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public ParserFieldName Name
        {
            get { return iFieldName; }
        }

        public int OriginalLocation
        {
            get { return iOriginalLocation; }
            set { iOriginalLocation = value; }
        }

        public int OriginalLength
        {
            get { return iOriginalLength; }
            set { iOriginalLength = value; }
        }

        public string RegularExpressionString
        {
            get { return iRegexString; }
            set { iRegexString = value; }
        }

        public int NumberBase
        {
            get { return iNumberBase; }
            set { iNumberBase = value; }
        }

        public Regex RegularExpression
        {
            get { return new Regex( iRegexString ); }
        }

        public TParserValueType ExpectedType
        {
            get { return iExpectedType; }
            set { iExpectedType = value; }
        }

        internal ParserField Field
        {
            get { return iField; }
        }
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
        private readonly ParserField iField;
        private ParserFieldName iFieldName = new ParserFieldName();
        private int iOriginalLocation = -1;
        private int iOriginalLength = -1;
        private string iRegexString = null;
        private int iNumberBase = 10;
        private TParserValueType iExpectedType = TParserValueType.EValueTypeByte;
        #endregion
    }
}
