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

namespace SymbianUtils.Settings
{
	public class XmlSettingsBoundedList
	{
		#region Constructors
		public XmlSettingsBoundedList( string aCategory, XmlSettings aSettings )
		{
			// Since the max capacity was not supplied, we can attempt to read
			// it from the settings or then if not found, we default to 10.
			iSettings = aSettings;
			iCategory = aCategory;
			iMaxCapacity = SettingsMaxCapacity;
			//
			Restore();
		}

		public XmlSettingsBoundedList( string aCategory, XmlSettings aSettings, int aMaxCapacity )
		{
			iSettings = aSettings;
			iCategory = aCategory;
			iMaxCapacity = aMaxCapacity;
			//
			Restore();
		}
		#endregion

		#region API
		public void Restore()
		{
			// Newest additions are stored at the front of the list.
			// Oldest items are therefore at the end
			iList.Clear();
			//
			int settingsCount = SettingsCount;
            for ( int i = 0; i < settingsCount && i < MaxCapacity; i++ )
			{
				string item = SettingItem(i);
				iList.Add( item );
			}
		}

		public void Store()
		{
			// Remove all entries that already exist
			// within the settings object
			RemoveAllExistingSettings();

			// Now add the replacement items
			int settingsCount = iList.Count;
			SettingsCount = settingsCount;
			for(int i=0; i<settingsCount; i++)
			{
				string itemKey = MakeSettingsKey( i );
				string itemValue = iList[ i ];
				//
				iSettings[ Category, itemKey ] = itemValue;
			}
		}

		public void Add( string aValue )
		{
			// First, add the item
			iList.Insert( 0, aValue );

			// If the capacity has reached the limit, we remove the last (oldest) item
			if	( iList.Count > MaxCapacity )
			{
				iList.RemoveAt( MaxCapacity );
			}
		}
		#endregion

		#region Properties
		public int MaxCapacity
		{
			get { return iMaxCapacity; }
		}

		public string Category
		{
			get { return iCategory; }
		}

		public int Count
		{
			get { return iList.Count; }
		}
		#endregion

		#region Internal properties
		private int SettingsCount
		{
			get
			{
				int count = 0;
				string countAsString = iSettings[Category, "_NumberOfItems"];
				if	(countAsString != null)
				{
					count = System.Convert.ToInt32(countAsString);
				}
				return count;
			}
			set
			{
				iSettings[Category, "_NumberOfItems"] = value.ToString();
			}
		}

		private int SettingsMaxCapacity
		{
			get
			{
				int capacity = 0;
				string capacityAsString = iSettings[Category, "_MaxCapacity"];
				if	(capacityAsString != null)
				{
					capacity = System.Convert.ToInt32(capacityAsString);
				}
				return capacity;
			}
			set
			{
				iSettings[Category, "_MaxCapacity"] = value.ToString();
			}
		}
		#endregion

		#region Internal methods
		private string MakeSettingsKey( int aIndex )
		{
			return "_Entry_Id_" + aIndex.ToString( "0000" );
		}

		private string SettingItem( int aIndex )
		{
			string key = MakeSettingsKey( aIndex );
			string itemValue = iSettings[ Category, key ];
			//
			return itemValue;
		}

		private void RemoveAllExistingSettings()
		{
			iSettings.Remove( Category );
		}
		#endregion

		#region Data members
		int iMaxCapacity = 100;
		readonly string iCategory;
		readonly XmlSettings iSettings;
		List<string> iList = new List<string>(10);
		#endregion
	}
}
