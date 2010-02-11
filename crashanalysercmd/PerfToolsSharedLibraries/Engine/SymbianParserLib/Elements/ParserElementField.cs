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
using SymbianParserLib.Elements.SubFields;
using SymbianParserLib.ValueStores;

namespace SymbianParserLib.Elements
{
    public class ParserField : ParserElementBaseWithValueStore
    {
        #region Constructors
        public ParserField()
            : this( string.Empty )
        {
        }

        public ParserField( string aName )
            : base( aName )
        {
            iFormatSpecifier = new ParserFieldFormatSpecifier( this );
            iFormatValue = new ParserFieldFormatValue( this );
        }

        internal ParserField( ParserField aField )
            : this( aField.Name )
        {
            iFormatSpecifier = new ParserFieldFormatSpecifier( this, aField.FormatSpecifier );
            iFormatValue = new ParserFieldFormatValue( this, aField.FormatValue );
        }
        #endregion

        #region API
        internal void ExtractValue( Group aGroup )
        {
            string fieldValue = aGroup.Value;
            TParserValueType fieldValueType = FormatSpecifier.ExpectedType;
            //
            switch ( fieldValueType )
            {
            case TParserValueType.EValueTypeString:
                {
                    iFormatValue.SetValueString( fieldValue );
                    break;
                }
            case TParserValueType.EValueTypeInt32:
                {
                    int valInt32 = System.Convert.ToInt32( fieldValue );
                    iFormatValue.SetValueInt( valInt32 );
                    break;
                }
            case TParserValueType.EValueTypeUint32:
                {
                    int numberBase = FormatSpecifier.NumberBase;
                    uint valUint32 = System.Convert.ToUInt32( fieldValue, numberBase );
                    iFormatValue.SetValueUint( valUint32 );
                }
                break;
            case TParserValueType.EValueTypeUint64:
                {
                    int numberBase = FormatSpecifier.NumberBase;
                    ulong valUint64 = System.Convert.ToUInt32( fieldValue, numberBase );
                    iFormatValue.SetValueUint64( valUint64 );
                }
                break;
            }
            //
            SetValue( iFormatSpecifier, iFormatValue );
            IsComplete = true;
        }
        #endregion

        #region Properties
        public ParserLine Line
        {
            get
            {
                ParserLine ret = null;
                //
                if ( Parent != null && Parent is ParserLine )
                {
                    ret = Parent as ParserLine;
                }
                //
                return ret;
            }
        }

        internal ParserFieldFormatSpecifier FormatSpecifier
        {
            get { return iFormatSpecifier; }
        }

        internal ParserFieldFormatValue FormatValue
        {
            get { return iFormatValue; }
        }
        #endregion

        #region From ParserElementBase
        internal override ParserResponse Offer( ref string aLine )
        {
            throw new NotSupportedException();
        }
        #endregion

        #region From ParserElementBaseWithValueStore
        internal override void SetTargetProperty( object aPropertyObject, string aPropertyName, int aIndex )
        {
            iValueStore = new ValueStore();
            iValueStore.SetTargetProperty( aPropertyObject, aPropertyName );
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
        private readonly ParserFieldFormatSpecifier iFormatSpecifier;
        private readonly ParserFieldFormatValue iFormatValue;
        #endregion
    }
}
