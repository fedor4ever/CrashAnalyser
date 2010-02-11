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
using System.Text;
using System.IO;
using System.Collections;

namespace SymbianUtils
{
	public class NumberBaseUtils
	{
        #region Enumerations
		public enum TNumberBase
		{
			EDecimal = 10,
			EHex = 16
		}
        #endregion

		public static long TextToDecimalNumber( string aText )
		{
			string copyOfText = aText;
			TNumberBase numberBase = TNumberBase.EDecimal;
			long longResult = 0;
			//
			bool result = TextToDecimalNumber( ref copyOfText, out longResult, out numberBase );
			if	( result == false )
			{
				longResult = 0;
			}
			//
			return longResult;
		}

		public static long TextToDecimalNumber( string aText, TNumberBase aNumberBase )
		{
			long ret = 0;
			//
			try
			{
				ret = System.Convert.ToInt64( aText, (int) aNumberBase );
			}
			finally
			{
			}
			//
			return ret;
		}

		public static bool TextToDecimalNumber( ref string aText, out long aValue, out TNumberBase aBase )
		{
			string address = aText;

			// Check if its a decimal or hex string
			aBase = TNumberBase.EDecimal;
			aValue = 0;
			//
			TNumberBase numberBase = TNumberBase.EDecimal;
			if	(address.Length > 2 && (address.Substring(0, 2) == "0x" || address.Substring(0, 2) == "0X"))
			{
				// Assume hex & remove prefix
				numberBase = TNumberBase.EHex;
				address = address.Substring(2);
			}

			// Check each char
			bool okToConvert = (address.Length > 0);
			foreach (char character in address)
			{
				if (character != ' ' && char.IsDigit(character) == false)
				{
					// Is it a hex digit?
					char upperCaseChar = char.ToUpper(character);
					switch (upperCaseChar)
					{
						case 'A':
						case 'B':
						case 'C':
						case 'D':
						case 'E':
						case 'F':
							numberBase = TNumberBase.EHex; // Now its definite
							break;
						default:
							okToConvert = false;
							break;
					}
				}
			}

			address = address.Trim();
			if	( address.Length > 0 && okToConvert )
			{
				// Convert number to base 10.
				try
				{
					aValue = System.Convert.ToInt64( address, (int) numberBase );
					aText = address;
					aBase = numberBase;
				}
				catch( ArgumentOutOfRangeException )
				{
					okToConvert = false;
				}
			}
			return okToConvert;
		}

        public static bool TryTextToDecimalNumber( ref string aText, out string aOutput, out long aValue, out TNumberBase aBase )
		{
			string address = aText;
			int endingOffset = 0;

			// Check if its a decimal or hex string
			aBase = TNumberBase.EDecimal;
			aValue = 0;
			//
			TNumberBase numberBase = TNumberBase.EDecimal;
			int characterIndex = 0;
			string prefix = string.Empty;
			if	(address.Length > 2 && (address.Substring(0, 2) == "0x" || address.Substring(0, 2) == "0X"))
			{
				// Assume hex & remove prefix
				numberBase = TNumberBase.EHex;
				prefix = address.Substring(0, 2);
				address = address.Substring(2);
			}

			// Check each char
			bool validCharacter = (address.Length > 0);
			while (validCharacter && characterIndex < address.Length )
			{
				char character = address[characterIndex];
				if (char.IsDigit(character) == false)
				{
					// Is it a hex digit?
					char upperCaseChar = char.ToUpper(character);
					switch (upperCaseChar)
					{
						case '-':
							break;
						case 'A':
						case 'B':
						case 'C':
						case 'D':
						case 'E':
						case 'F':
							numberBase = TNumberBase.EHex; // Now its definite
							break;
						default:
							validCharacter = false;
							break;
					}
				}
			
				characterIndex++;
				if	(validCharacter)
					endingOffset++;
			}

			if	(endingOffset > 0)
			{
				// Convert number to base 10.
				aOutput = address.Substring(0, endingOffset).Trim();
				aValue = System.Convert.ToInt64(aOutput, (int) numberBase);
				aOutput = prefix + aOutput;
				aBase = numberBase;
				aText = aText.Substring(endingOffset + prefix.Length);
			}
			else
			{
				aOutput = string.Empty;
				aValue = 0;
				aBase = TNumberBase.EDecimal;
			}

			return (endingOffset > 0);
		}

		public static bool TryTextToDecimalNumber( ref string aText, out string aOutput, out long aValue, TNumberBase aBase )
		{
			string address = aText;
			int endingOffset = 0;

			// Check if its a decimal or hex string
			aValue = 0;
			//
			TNumberBase numberBase = aBase;
			int characterIndex = 0;
			string prefix = string.Empty;
			if	(address.Length > 2 && (address.Substring(0, 2) == "0x" || address.Substring(0, 2) == "0X"))
			{
				// Assume hex & remove prefix
				numberBase = TNumberBase.EHex;
				prefix = address.Substring(0, 2);
				address = address.Substring(2);
			}

			// Check each char
			bool validCharacter = (address.Length > 0);
			while( validCharacter && characterIndex < address.Length )
			{
				char character = address[characterIndex];
				if (char.IsDigit(character) == false)
				{
					// Is it a hex digit?
					char upperCaseChar = char.ToUpper(character);
					switch (upperCaseChar)
					{
						case '-':
							break;
						case 'A':
						case 'B':
						case 'C':
						case 'D':
						case 'E':
						case 'F':
							numberBase = TNumberBase.EHex; // Now its definite
							break;
						default:
							validCharacter = false;
							break;
					}
				}
			
				characterIndex++;
				if	(validCharacter)
					endingOffset++;
			}

			if	(endingOffset > 0)
			{
				try
				{
					// Convert number to base 10.
					string output = address.Substring(0, endingOffset).Trim();;
					aValue = System.Convert.ToInt64( output, (int) numberBase );
					aOutput = prefix + output;
					aText = aText.Substring(endingOffset + prefix.Length);
				}
				catch( Exception )
				{
					endingOffset = -1;
					aOutput = string.Empty;
					aValue = 0;
				}
			}
			else
			{
				aOutput = string.Empty;
				aValue = 0;
			}

			return (endingOffset > 0);
		}	
	}
}
