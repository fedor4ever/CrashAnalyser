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
using System.Xml;
using System.Text;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SymbianUtils.Settings
{
    public class XmlSettingsValue
	{
		#region Constructors
        internal XmlSettingsValue( object aValue )
        {
            Value = aValue;
        }

        internal XmlSettingsValue( XmlReader aReader )
		{
            try
            {
                Read( aReader );
            }
            catch ( Exception )
            {
            }
		}
		#endregion

		#region Store & Restore API
		internal void Write( XmlWriter aWriter )
		{
            string typeString = Value.GetType().ToString();
            aWriter.WriteAttributeString( "type", typeString );
            //
            string valueString = Value.ToString();
            aWriter.WriteAttributeString( "value", valueString );
        }

        internal void Read( XmlReader aReader )
		{
            string typeString = XmlUtils.ReadAttributeValue( aReader, "type" );
            string valueString = XmlUtils.ReadAttributeValue( aReader, "value" );
            //
            Type type = Type.GetType( typeString );
            TypeConverter converter = TypeDescriptor.GetConverter( type );
            //
            bool canConvertFromString = converter.CanConvertFrom( valueString.GetType() );
            if ( canConvertFromString )
            {
                object value = converter.ConvertFromString( valueString );
                Value = value;
            }
            else
            {
                Value = valueString;
            }
        }
		#endregion

        #region Operators
        public static implicit operator XmlSettingsValue( int aValue )
        {
            XmlSettingsValue ret = new XmlSettingsValue( aValue );
            return ret;
        }

        public static implicit operator XmlSettingsValue( string aValue )
        {
            XmlSettingsValue ret = new XmlSettingsValue( aValue );
            return ret;
        }

        public static implicit operator XmlSettingsValue( bool aValue )
        {
            XmlSettingsValue ret = new XmlSettingsValue( aValue );
            return ret;
        }

        public static implicit operator XmlSettingsValue( uint aValue )
        {
            XmlSettingsValue ret = new XmlSettingsValue( aValue );
            return ret;
        }

        public static implicit operator XmlSettingsValue( long aValue )
        {
            XmlSettingsValue ret = new XmlSettingsValue( aValue );
            return ret;
        }

        public static implicit operator int( XmlSettingsValue aValue )
        {
            int ret = aValue.ToInt();
            return ret;
        }

        public static implicit operator string( XmlSettingsValue aValue )
        {
            string ret = aValue.ToString();
            return ret;
        }

        public static implicit operator bool( XmlSettingsValue aValue )
        {
            bool ret = aValue.ToBool();
            return ret;
        }

        public static implicit operator uint( XmlSettingsValue aValue )
        {
            uint ret = aValue.ToUint();
            return ret;
        }

        public static implicit operator long( XmlSettingsValue aValue )
        {
            long ret = aValue.ToLong();
            return ret;
        }

        public static implicit operator Size( XmlSettingsValue aValue )
        {
            Size ret = aValue.ToSize();
            return ret;
        }

        public static implicit operator Point( XmlSettingsValue aValue )
        {
            Point ret = aValue.ToPoint();
            return ret;
        }
        #endregion

        #region Conversion methods
        public int ToInt()
        {
            return ToInt( 0 );
        }

        public int ToInt( int aDefault )
        {
            int ret = XmlSettingsValueConverter<int>.Convert( this, aDefault );
            return ret;
        }

        public uint ToUint()
        {
            return ToUint( 0 );
        }

        public uint ToUint( uint aDefault )
        {
            uint ret = XmlSettingsValueConverter<uint>.Convert( this, aDefault );
            return ret;
        }

        public long ToLong()
        {
            return ToLong( 0 );
        }

        public long ToLong( long aDefault )
        {
            long ret = XmlSettingsValueConverter<long>.Convert( this, aDefault );
            return ret;
        }

        public bool ToBool()
        {
            return ToBool( false );
        }

        public bool ToBool( bool aDefault )
        {
            bool defaultApplied = false;
            bool ret = XmlSettingsValueConverter<bool>.Convert( this, aDefault, out defaultApplied );
            //
            if ( defaultApplied )
            {
                string valueAsString = ToString().ToLower().Trim();
                //
                switch ( valueAsString )
                {
                case "true":
                case "yes":
                case "1":
                    ret = true;
                    break;
                case "false":
                case "no":
                case "0":
                    ret = false;
                    break;
                default:
                    ret = aDefault;
                    break;
                }
            }
            //
            return ret;
        }

        public string ToString( string aDefault )
        {
            string ret = XmlSettingsValueConverter<string>.Convert( this, aDefault );
            return ret;
        }

        public Point ToPoint()
        {
            return ToPoint( new Point( 0, 0 ) );
        }

        public Point ToPoint( Point aDefault )
        {
            Point ret = XmlSettingsValueConverter<Point>.Convert( this, aDefault );
            return ret;
        }

        public Size ToSize()
        {
            return ToSize( new Size( 0, 0 ) );
        }

        public Size ToSize( Size aDefault )
        {
            Size ret = XmlSettingsValueConverter<Size>.Convert( this, aDefault );
            return ret;
        }
        #endregion

		#region Properties
		internal object Value
		{
			get { return iValue; }
			set
            {
                if ( value == null )
                {
                    throw new Exception( "Value cannot be NULL" );
                }
                else if ( value.GetType() == typeof( XmlSettingsValue ) )
                {
                    SymbianUtils.SymDebug.SymDebugger.Break();
                }

                iValue = value;
            }
		}
		#endregion

		#region From System.Object
        public override string ToString()
        {
            return Value.ToString();
        }
		#endregion
		
		#region Internal methods
		#endregion

		#region Data members
        private object iValue = string.Empty;
		#endregion
	}
}


