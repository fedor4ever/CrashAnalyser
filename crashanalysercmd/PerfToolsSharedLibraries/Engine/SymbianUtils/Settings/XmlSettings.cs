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
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;

namespace SymbianUtils.Settings
{
	public class XmlSettings : IEnumerable<XmlSettingCategory>
	{
		#region Constructors
		public XmlSettings( string aFileName )
		{
			FullyQualifiedFileName = aFileName;
		}
		#endregion

		#region Store & Restore API
		public void Restore()
		{
			string file = FullyQualifiedFileName;
			if (System.IO.File.Exists(file))
			{
				XmlTextReader reader = new XmlTextReader(file);
				try
				{
					reader.WhitespaceHandling = WhitespaceHandling.None;
					reader.MoveToContent();
					ReadData(reader);
				}
				catch (XmlException)
				{
					// Ignore for now
				}
				finally
				{
					reader.Close();
				}
			}
		}

		public void Store()
		{
			string file = FullyQualifiedFileName;
			XmlTextWriter writer = new XmlTextWriter(file, System.Text.Encoding.UTF8);
			try
			{
				writer.Formatting = Formatting.Indented;
				WriteData(writer);
			}
			catch (XmlException)
			{
				// Ignore for now
			}
			finally
			{
				writer.Close();
			}
		}
		#endregion

		#region API
        public void Clear()
        {
            iCategories.Clear();
        }

        public void Add( XmlSettingCategory aCategory )
        {
            iCategories.Add( aCategory.Name, aCategory );
        }

		public bool Exists( string aCategory )
		{
            return iCategories.ContainsKey( aCategory );
		}

        public bool Exists( string aCategory, string aKey )
        {
            bool ret = false;
            //
            if ( Exists( aCategory ) )
            {
                XmlSettingCategory category = this[ aCategory ];
                ret = category.Exists( aKey );
            }
            //
            return ret;
        }

		public void Remove( string aCategory )
		{
            if ( Exists( aCategory ) )
            {
                iCategories.Remove( aCategory );
			}
		}

		public void Remove( string aCategory, string aKey )
		{
            if ( Exists( aCategory ) )
            {
                XmlSettingCategory category = this[ aCategory ];
                category.Remove( aKey );
            }
		}
        #endregion

        #region Loading & Saving
        public void Save( string aCategory, object aObject )
        {
            if ( aObject is TextBox )
            {
                DoSave( aCategory, aObject as TextBox );
            }
            else if ( aObject is RadioButton )
            {
                DoSave( aCategory, aObject as RadioButton );
            }
            else if ( aObject is CheckBox )
            {
                DoSave( aCategory, aObject as CheckBox );
            }
            else if ( aObject is ComboBox )
            {
                DoSave( aCategory, aObject as ComboBox );
            }
            else if ( aObject is IXmlSettingsSimple )
            {
                DoSave( aCategory, aObject as IXmlSettingsSimple );
            }
            else if ( aObject is IXmlSettingsExtended )
            {
                DoSave( aCategory, aObject as IXmlSettingsExtended );
            }
            else if ( aObject is Form )
            {
                DoSave( aCategory, aObject as Form );
            }
        }

        public void Load( string aCategory, object aObject )
        {
            if ( aObject is TextBox )
            {
                DoLoad( aCategory, aObject as TextBox );
            }
            else if ( aObject is RadioButton )
            {
                DoLoad( aCategory, aObject as RadioButton );
            }
            else if ( aObject is CheckBox )
            {
                DoLoad( aCategory, aObject as CheckBox );
            }
            else if ( aObject is ComboBox )
            {
                DoLoad( aCategory, aObject as ComboBox );
            }
            else if ( aObject is IXmlSettingsSimple )
            {
                DoLoad( aCategory, aObject as IXmlSettingsSimple );
            }
            else if ( aObject is IXmlSettingsExtended )
            {
                DoLoad( aCategory, aObject as IXmlSettingsExtended );
            }
            else if ( aObject is Form )
            {
                DoLoad( aCategory, aObject as Form );
            }
        }

        #region Basic types
        public void Save( string aCategory, string aName, int aValue )
        {
            this[ aCategory, aName ] = aValue;
        }

        public void Save( string aCategory, string aName, uint aValue )
        {
            this[ aCategory, aName ] = aValue;
        }

        public void Save( string aCategory, string aName, long aValue )
        {
            this[ aCategory, aName ] = aValue;
        }

        public void Save( string aCategory, string aName, bool aValue )
        {
            this[ aCategory, aName ] = aValue;
        }

        public void Save( string aCategory, string aName, string aValue )
        {
            this[ aCategory, aName ] = aValue;
        }

        public void Save( string aCategory, string aName, object aValue )
        {
            this[ aCategory, aName ].Value = aValue;
        }

        public void Save( string aCategory, string aName, Point aValue )
        {
            this[ aCategory, aName ].Value = string.Format( "{0},{1}", aValue.X, aValue.Y );
        }

        public void Save( string aCategory, string aName, Size aValue )
        {
            this[ aCategory, aName ].Value = string.Format( "{0},{1}", aValue.Width, aValue.Height );
        }

        public int LoadInt( string aCategory, string aName )
        {
            return Load( aCategory, aName, 0 );
        }

        public int Load( string aCategory, string aName, int aDefault )
        {
            int ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public uint LoadUint( string aCategory, string aName )
        {
            return Load( aCategory, aName, 0U );
        }

        public uint Load( string aCategory, string aName, uint aDefault )
        {
            uint ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public long LoadLong( string aCategory, string aName )
        {
            return Load( aCategory, aName, 0L );
        }

        public long Load( string aCategory, string aName, long aDefault )
        {
            long ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public bool LoadBool( string aCategory, string aName )
        {
            return Load( aCategory, aName, false );
        }

        public bool Load( string aCategory, string aName, bool aDefault )
        {
            bool ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public string Load( string aCategory, string aName )
        {
            return Load( aCategory, aName, string.Empty );
        }

        public string Load( string aCategory, string aName, string aDefault )
        {
            string ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public object LoadObject( string aCategory, string aName )
        {
            return Load( aCategory, aName, null );
        }

        public object Load( string aCategory, string aName, object aDefault )
        {
            object ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ].Value;
            }
            return ret;
        }

        public Point Load( string aCategory, string aName, Point aDefault )
        {
            Point ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }

        public Size Load( string aCategory, string aName, Size aDefault )
        {
            Size ret = aDefault;
            if ( Exists( aCategory, aName ) )
            {
                ret = this[ aCategory, aName ];
            }
            return ret;
        }
        #endregion

        #endregion

		#region Properties
        public int Count
        {
            get { return iCategories.Count; }
        }

		public string FileName
		{
			get
			{
				FileInfo info = new FileInfo(FullyQualifiedFileName);
				return info.Name;
			}
			set
			{
				FileInfo newNameInfo = new FileInfo(value);
				FileInfo fullNameInfo = new FileInfo(FullyQualifiedFileName);
				string fileName = fullNameInfo.DirectoryName;
				fileName += newNameInfo.Name;
				FullyQualifiedFileName = fileName;
			}
		}

		public string FullyQualifiedFileName
		{
			get { return iFullyQualifiedFileName; }
			set
			{
				string path = string.Empty;
				try
				{
					path = Path.GetDirectoryName(value);

				}
				catch(ArgumentException)
				{
				}

				if	(path == null || path.Length == 0)
				{
					// Get the path from the current app domain
					iFullyQualifiedFileName = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.GetFileName(value));
				}
				else
				{
					iFullyQualifiedFileName = value; 
				}
			}
		}

		public static Version CurrentSettingsVersion
		{
			get { return iCurrentSettingsVersion; }
		}
		#endregion

        #region From IEnumerable<XmlSettingCategory>
        public IEnumerator<XmlSettingCategory> GetEnumerator()
        {
            foreach ( KeyValuePair<string, XmlSettingCategory> pair in iCategories )
            {
                yield return pair.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, XmlSettingCategory> pair in iCategories )
            {
                yield return pair.Value;
            }
        }
        #endregion

		#region Indexers
        public XmlSettingCategory this[ int aIndex ]
        {
            get
            {
                string key = iCategories.Keys[ aIndex ];
                return iCategories[ key ];
            }
        }

        public XmlSettingCategory this[ string aCategory ]
        {
            get
            {
                XmlSettingCategory ret = new XmlSettingCategory( aCategory );
                if ( Exists( aCategory ) )
                {
                    ret = iCategories[ aCategory ];
                }
                else
                {
                    iCategories.Add( aCategory, ret );
                }
                //
                return ret;
            }
        }
        
		public XmlSettingsValue this[ string aCategory, string aKey ]
		{
			get
			{
                XmlSettingCategory category = this[ aCategory ];
                XmlSettingsValue value = category[ aKey ];
                return value;
            }
			set
			{
                XmlSettingCategory category = this[ aCategory ];
                category[ aKey ] = value;
			}
		}

        public XmlSettingsValue this[ string aCategory, object aObject ]
        {
            get
            {
                string key = aObject.ToString();
                if ( aObject is System.Windows.Forms.Control )
                {
                    key = ( aObject as Control ).Name;
                }
                return this[ aCategory, key ];
            }
            set
            {
                string key = aObject.ToString();
                if ( aObject is System.Windows.Forms.Control )
                {
                    key = ( aObject as Control ).Name;
                }
                this[ aCategory, key ] = value;
            }
        }
		#endregion

        #region Internal write helpers
        private void WriteData(XmlWriter aWriter)
		{
			aWriter.WriteStartElement( null, "configuration", null );
            //
			WriteConfigSections( aWriter );
			WriteSectionItems( aWriter );
            //
			aWriter.WriteEndElement();
		}

		private void WriteConfigSections(XmlWriter aWriter)
		{
			aWriter.WriteStartElement( null, "configSections", null );
			//
            foreach( XmlSettingCategory cat in this )
			{
				aWriter.WriteStartElement("section", null);
				aWriter.WriteAttributeString("name", cat.Name);
				aWriter.WriteAttributeString("version", cat.Version.ToString());
				aWriter.WriteRaw(" ");
				aWriter.WriteEndElement();
			}
            //
			aWriter.WriteEndElement();
		}

		private void WriteSectionItems(XmlWriter aWriter)
		{
			aWriter.WriteStartElement(null, "sectionItems", null);
            //
            foreach ( XmlSettingCategory cat in this )
			{
				cat.WriteSettings( aWriter );
			}
            //
			aWriter.WriteEndElement();
		}
		#endregion

		#region Internal read helpers
		private void ReadData(XmlReader aReader)
		{
			aReader.ReadStartElement("configuration");
			//
			ReadConfigSections( aReader );
			ReadSectionItems( aReader );
			//
			aReader.ReadEndElement(); // configuration
		}

		private void ReadConfigSections(XmlReader aReader)
		{
			aReader.ReadStartElement("configSections");
			//
			while (!aReader.EOF && aReader.Name == "section")
			{
				if (aReader.NodeType != XmlNodeType.EndElement)
				{
                    XmlSettingCategory category = new XmlSettingCategory( aReader );
                    iCategories.Add( category.Name, category );
				}
				aReader.Read();
			}
            //
			aReader.ReadEndElement(); // configSections
		}

		private void ReadSingleSection( XmlReader aReader )
		{
			string sectionName = aReader.Name;

			// Advance to first section key & pair
            if ( aReader.IsEmptyElement )
            {
                // It's empty - doesn't contain any content
            }
            else
            {
                bool readOkay = aReader.Read();

                // Search for the existing section definition
                if ( readOkay && Exists( sectionName ) )
                {
                    XmlSettingCategory cat = this[ sectionName ];
                    cat.ReadSettings( aReader );
                }
            }
		}

		private void ReadSectionItems(XmlReader aReader)
		{
			aReader.ReadStartElement( "sectionItems" );
            //
            while ( !aReader.EOF && !( aReader.NodeType == XmlNodeType.EndElement && aReader.Name == "sectionItems" ) )
			{
				ReadSingleSection( aReader );
				aReader.Read();
			}
            //
			aReader.ReadEndElement(); // sectionItems
		}
		#endregion

        #region Internal settings savers
        private void DoSave( string aCategory, IXmlSettingsSimple aObject )
        {
            string name = aObject.XmlSettingsPersistableName;
            string value = aObject.XmlSettingPersistableValue;
            this[ aCategory, name ] = value;
        }

        private void DoLoad( string aCategory, IXmlSettingsSimple aObject )
        {
            string name = aObject.XmlSettingsPersistableName;
            string value = this[ aCategory, name ];
            aObject.XmlSettingPersistableValue = value;
        }

        private void DoSave( string aCategory, IXmlSettingsExtended aObject )
        {
            aObject.XmlSettingsSave( this, aCategory );
        }

        private void DoLoad( string aCategory, IXmlSettingsExtended aObject )
        {
            aObject.XmlSettingsLoad( this, aCategory );
        }

        private void DoSave( string aCategory, TextBox aControl )
        {
            this[ aCategory, aControl.Name ] = aControl.Text;
        }

        private void DoLoad( string aCategory, TextBox aControl )
        {
            DoLoad( aCategory, aControl, string.Empty );
        }

        private void DoLoad( string aCategory, TextBox aControl, string aDefault )
        {
            string val = this[ aCategory, aControl.Name ];
            if ( val.Length > 0 )
            {
                aControl.Text = val;
            }
            else
            {
                aControl.Text = aDefault;
            }
        }

        private void DoSave( string aCategory, CheckBox aControl )
        {
            this[ aCategory, aControl.Name ] = aControl.Checked;
        }

        private void DoLoad( string aCategory, CheckBox aControl )
        {
            aControl.Checked = this[ aCategory, aControl ];
        }

        private void DoSave( string aCategory, RadioButton aControl )
        {
            this[ aCategory, aControl.Name ] = aControl.Checked;
        }

        private void DoLoad( string aCategory, RadioButton aControl )
        {
            aControl.Checked = this[ aCategory, aControl ];
        }

        private void DoSave( string aCategory, ComboBox aControl )
        {
            XmlSettingCategory category = this[ aCategory ];
 
            // Basic prefix
            string baseName = string.Format( "{0}###Item###", aControl.Name );
            
            // Write count, selection
            category[ baseName + "Count" ] = aControl.Items.Count;
            category[ baseName + "SelectedIndex" ] = aControl.SelectedIndex;

            // Write items
            int itemIndex = 0;
            foreach ( object item in aControl.Items )
            {
                string itemName = string.Format( "{0}{1:d6}", baseName, itemIndex++ );
                category[ itemName ] = item.ToString();
            }
        }

        private void DoLoad( string aCategory, ComboBox aControl )
        {
            XmlSettingCategory category = this[ aCategory ];

            aControl.BeginUpdate();

            // Basic prefix
            string baseName = string.Format( "{0}###Item###", aControl.Name );

            // Get count, selection
            int count = category[ baseName + "Count" ].ToInt( 0 );
            int selectedIndex = category[ baseName + "SelectedIndex" ].ToInt( 0 );

            // Get items
            for ( int itemIndex = 0; itemIndex < count; itemIndex++ )
            {
                string itemName = string.Format( "{0}{1:d6}", baseName, itemIndex++ );
                string itemText = category[ itemName ];
                bool alreadyExists = aControl.Items.Contains( itemText );
                if ( !alreadyExists )
                {
                    aControl.Items.Add( itemText );
                }
            }

            // Set selection if possible
            if ( selectedIndex >= 0 && selectedIndex < aControl.Items.Count )
            {
                aControl.SelectedIndex = selectedIndex;
            }
            aControl.EndUpdate();
        }

        private void DoSave( string aCategory, Form aForm )
        {
            XmlSettingCategory category = this[ aCategory ];

            // Save location (if relevant)
            Point location = new Point();
            if ( aForm.WindowState == FormWindowState.Normal )
            {
                location = aForm.Location;
            }
            Save( aCategory, "__#Location#__", location );

            // Save size 
            Size size = new Size();
            if ( aForm.WindowState == FormWindowState.Normal )
            {
                size = aForm.Size;
            }
            Save( aCategory, "__#Size#__", size );

            // Save window state
            category[ "__#State#__" ] = aForm.WindowState.ToString();
        }

        private void DoLoad( string aCategory, Form aForm )
        {
            aForm.StartPosition = FormStartPosition.Manual;

            if ( Exists( aCategory ) )
            {
                XmlSettingCategory category = this[ aCategory ];
                
                // Get initial string setting values
                string stringState = category[ "__#State#__" ];
                string stringLocation = category[ "__#Location#__" ];
                string stringSize = category[ "__#Size#__" ];
                
                // Convert to correct types
                FormWindowState state = (FormWindowState) System.Enum.Parse( typeof( FormWindowState ), stringState );
                Point location = Load( aCategory, "__#Location#__" , new Point( 0, 0 ) );
                Size size = Load( aCategory, "__#Size#__", aForm.MinimumSize );
                //
                aForm.Location = location;
                aForm.WindowState = state;
                if ( aForm.WindowState == FormWindowState.Normal )
                {
                    aForm.Size = size;
                }
            }
            else
            {
                // Go with defaults for everything
                aForm.WindowState = FormWindowState.Maximized;
            }
        }
        #endregion

		#region Data members
		private static Version iCurrentSettingsVersion = new Version( 2, 0, 0 );
        private string iFullyQualifiedFileName = string.Empty;
        private SortedList<string, XmlSettingCategory> iCategories = new SortedList<string, XmlSettingCategory>();
		#endregion
    }
}


