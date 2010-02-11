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
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace SymbianUtils.Settings
{
    public class XmlSettingCategory
	{
		#region Constructors
		public XmlSettingCategory( string aName )
		{
			iName = aName;
			iVersion = (Version) XmlSettings.CurrentSettingsVersion.Clone();
		}

		public XmlSettingCategory( XmlReader aReader )
		{
            XmlUtils.ReadAttributeValue( aReader, "name", out iName );
            //
            string versionAsString;
			XmlUtils.ReadAttributeValue( aReader, "version", out versionAsString );
            iVersion = new Version( versionAsString );
		}
		#endregion

		#region Store & Restore API
		internal void WriteSettings( XmlWriter aWriter )
		{
			aWriter.WriteStartElement( Name, null );
		    //
            foreach ( KeyValuePair<string, XmlSettingsValue> pair in iSettings )
            {
                // Write overall item encapsulation header
                aWriter.WriteStartElement( "item", null );
                
                    // Write key
                    aWriter.WriteAttributeString( "key", pair.Key );
                
                    // Get value object to write itself out
                    pair.Value.Write( aWriter );

                // End item
                aWriter.WriteEndElement();
            }
            //
            aWriter.WriteEndElement();
		}

        internal void ReadSettings( XmlReader aReader )
		{
			while ( !aReader.EOF && aReader.Name == "item" )
			{
				if ( aReader.NodeType != XmlNodeType.EndElement )
				{
                    string key = XmlUtils.ReadAttributeValue( aReader, "key" );
                    //
                    XmlSettingsValue value = new XmlSettingsValue( aReader );
                    iSettings.Add( key, value );
				}
				aReader.Read();
			}
            //
			SymbianUtils.SymDebug.SymDebugger.Assert(aReader.Name == Name);
		}
		#endregion

        #region Misc. API
        internal void Add( string aKey, string aValue )
		{
            XmlSettingsValue value = new XmlSettingsValue( aValue );
            iSettings.Add( aKey, value );
		}

        internal void Remove( string aKey )
		{
            if ( iSettings.ContainsKey( aKey ) )
            {
                iSettings.Remove( aKey );
			}
		}

        public bool Contains( string aKey )
        {
            return iSettings.ContainsKey( aKey );
        }
        
		public bool Exists( string aKey )
		{
            return Contains( aKey );
		}
		#endregion

		#region Properties
		public string Name
		{
			get { return iName; }
		}

		public Version Version
		{
			get { return iVersion; }
		}
		#endregion

		#region Indexers
        public XmlSettingsValue this[ string aKey ]
		{
			get
			{
                XmlSettingsValue ret = new XmlSettingsValue( string.Empty );
                //
                if ( Exists( aKey ) )
                {
                    ret = iSettings[ aKey ];
                }
                else
                {
                    iSettings.Add( aKey, ret );
                }
                //
                return ret;
			}
			set
			{
                if ( !Exists( aKey ) )
                {
                    iSettings.Add( aKey, value );
                }
                else
                {
                    // Item already exists
                    iSettings[ aKey ] = value;
                }
            }
		}
		#endregion
		
		#region Internal methods
		#endregion

		#region Data members
		private readonly string iName;
		private readonly Version iVersion;
        private SortedList<string, XmlSettingsValue> iSettings = new SortedList<string, XmlSettingsValue>();
		#endregion
	}
}
