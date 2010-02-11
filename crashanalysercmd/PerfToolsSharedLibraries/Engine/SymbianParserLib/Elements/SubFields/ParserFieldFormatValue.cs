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
using System.ComponentModel;
using SymbianParserLib.Enums;

namespace SymbianParserLib.Elements.SubFields
{
    internal class ParserFieldFormatValue
    {
        #region Constructors
        public ParserFieldFormatValue( ParserField aField )
        {
            iField = aField;
        }

        internal ParserFieldFormatValue( ParserField aField, ParserFieldFormatValue aCopy )
            : this( aField )
        {
        }
        #endregion

        #region API
        public void SetValueString( string aValue )
        {
            iValue = aValue;
        }

        public void SetValueUint( uint aValue )
        {
            iValue = aValue;
        }

        public void SetValueUint64( ulong aValue )
        {
            iValue = aValue;
        }

        public void SetValueInt( int aValue )
        {
            iValue = aValue;
        }
        #endregion

        #region Properties
        public object Value
        {
            get { return iValue; }
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
        private object iValue = null;
        #endregion
    }
}
