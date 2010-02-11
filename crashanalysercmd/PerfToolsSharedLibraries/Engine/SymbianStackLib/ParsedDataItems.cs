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

namespace StackLib
{
	public class ParsedDataItem
	{
		#region Constructors & destructor
		public ParsedDataItem( long aAddress, long aReversedData, long aOriginalData, string aCharacterisedData )
		{
			iAddress = aAddress;
			iData = aReversedData;
			iOriginalData = aOriginalData;
			iCharacterisedData = aCharacterisedData;
		}
		#endregion

		#region Properties
		public long Address
		{
			get { return iAddress; }
			set { iAddress = value; }
		}

		public long Data
		{
			get { return iData; }
			set { iData = value; }
		}

		public long OriginalData
		{
			get { return iOriginalData; }
			set { iOriginalData = value; }
		}

		public string CharacterisedData
		{
			get { return iCharacterisedData; }
			set { iCharacterisedData = value; }
		}

		public string OriginalCharacterisedData
		{
			get 
			{
				char[] reversedCharacterisedData = new char[iCharacterisedData.Length];
				for (int i = 0; i < iCharacterisedData.Length; i+=4)
				{
					string bytes = iCharacterisedData.Substring(i, 4);
					reversedCharacterisedData[i]   = bytes[3];
					reversedCharacterisedData[i+1] = bytes[2];
					reversedCharacterisedData[i+2] = bytes[1];
					reversedCharacterisedData[i+3] = bytes[0];
				}
				//
				string characterisedData = new string(reversedCharacterisedData);
				return characterisedData;
			}
		}

		public object Tag
		{
			get { return iTag; }
			set { iTag = value; }
		}
		#endregion
		
		#region Data members
		private object iTag;
		private long iAddress;
		private long iData;
		private long iOriginalData;
		private string iCharacterisedData;
		#endregion
	}
}
