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
using System.Collections;
using System.Text;
using SymbolLib;

namespace StackLib
{
	public class StackElement
	{
		public class StackElementDescriptorInfo
		{
			#region Public enumerations
			public enum TType
			{
				EBuf
			}
			#endregion

			#region API
			public string AsString( out int aNumberOfLines, bool aPostfixInfo )
			{
				aNumberOfLines = 1;
				StringBuilder sb = new StringBuilder();
				//
				int count = iDescriptorCharacters.Count;
				for(int i=0; i<count && sb.Length < iLength; i++ )
				{
					char character = (char) iDescriptorCharacters[i];

					if	( character < ' ' || character > '~' )
					{
						sb.Append( KReplacementForUnprintableCharacters );
					}
					else
					{
						sb.Append( character );
					}

					if	( sb.Length > 0 && ((sb.Length % KNumberOfCharactersPerLine ) == 0))
					{
						sb.Append( System.Environment.NewLine );
						++aNumberOfLines;
					}
				}

				if	( aPostfixInfo )
				{
					string header = System.Environment.NewLine  + System.Environment.NewLine + "[Len: " + iLength.ToString() + ", max: " + iMaxLength.ToString() + "]";
					aNumberOfLines += 2;
					sb.Append( header );
				}
				//
				return sb.ToString();
			}
			#endregion

			#region Internal constants
			private const int KNumberOfCharactersPerLine = 64;
			private const char KReplacementForUnprintableCharacters = '.';
			#endregion

			#region Data members
			public TType iType;
			public long iLength;
			public long iMaxLength;
			public int iByteWidth = 1;
			public ArrayList iDescriptorCharacters = new ArrayList( 100 );
			#endregion
		}

		#region Constructors & destructor
		public StackElement( long aAddress, long aData, string aCharacterisedData )
		{
			iAddress = aAddress;
			iData = aData;
			iCharacterisedData = aCharacterisedData;
			iSymbol = null;
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

		public string CharacterisedData
		{
			get { return iCharacterisedData; }
		}

		public GenericSymbol Symbol
		{
			get { return iSymbol; }
			set { iSymbol = value; }
		}

		public bool IsDescriptor
		{
			get { return iDescriptorInfo != null; }
		}

		public StackElementDescriptorInfo DescriptorInfo
		{
			get { return iDescriptorInfo; }
			set { iDescriptorInfo = value; }
		}
		#endregion

		#region From System.Object
		public override string ToString()
		{
			string fixedElement = "= " + iData.ToString("x8") + " " + iCharacterisedData + " ";
			if	(iSymbol != null)
			{
				return fixedElement + iSymbol.Symbol;
			}
			return fixedElement;
		}
		#endregion
	
		#region Data members
		private GenericSymbol iSymbol;
		private long iAddress;
		private long iData;
		private string iCharacterisedData;
		private StackElementDescriptorInfo iDescriptorInfo = null;
		#endregion
	}
}
