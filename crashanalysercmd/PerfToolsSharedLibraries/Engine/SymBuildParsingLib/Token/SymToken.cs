/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Collections;
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Token
{
	public class SymToken
	{
		#region Enumerations
		public enum TClass
		{
			// The lexer only deals at this level of abstraction
			EClassNull = 0,
			EClassWhiteSpace,
			EClassAlphaNumeric,
			EClassSymbol,
			EClassNewLine,
			
			// The grouper also can identify these types
			EClassComment,
			EClassQuotation,
			EClassContinuation,
			EClassPreProcessor,
		};

		public enum TType
		{
			ETypeUnidentified = 0,
			ETypeCommentFullLine,
			ETypeCommentBlock,
			ETypeQuotationSingle, 
			ETypeQuotationDouble,
			ETypeAlphaNumericNormal,
			ETypeAlphaNumericUnderscore,
		}
		#endregion

		#region Constructors & destructor
		public static SymToken NullToken()
		{
			return new SymToken( "", TClass.EClassNull, new SymTextPosition() );
		}

		public SymToken( string aValue, TClass aClass, TType aType )
		{
			iValue = aValue;
			iClass = aClass;
			iType = aType;
		}

		public SymToken( string aValue, TClass aClass, SymTextPosition aPosition )
		{
			iValue = aValue;
			iClass = aClass;
			iPosition = aPosition;
		}

		public SymToken( SymToken aToken )
		{
			iValue = aToken.Value;
			iClass = aToken.Class;
			iType = aToken.Type;
			iPosition = aToken.Position;
			iTag = aToken.Tag;
		}
		#endregion

		#region API
		public void RefineTokenClass()
		{
			if	( Class == SymToken.TClass.EClassSymbol )
			{
				if	( Value.Length == 1 )
				{
					if	( Value == "\"" )
					{
						Class = SymToken.TClass.EClassQuotation; 
					}
					else if ( Value == "\'" )
					{
						Class = SymToken.TClass.EClassQuotation; 
					}
					else if ( Value == "_" )
					{
						Class = SymToken.TClass.EClassAlphaNumeric;
					}
					else if ( Value == "#" )
					{
						Class = SymToken.TClass.EClassPreProcessor;
					}
				}
			}
		}

		public void RefineTokenType()
		{
			if	( Type == SymToken.TType.ETypeUnidentified )
			{
				if	( Class == SymToken.TClass.EClassQuotation )
				{
					if	( Value.Length == 1 )
					{
						if	( Value == "\"" )
						{
							Type = SymToken.TType.ETypeQuotationDouble; 
						}
						else if ( Value == "\'" )
						{
							Type = SymToken.TType.ETypeQuotationSingle; 
						}
					}
				}
				else if ( Class == SymToken.TClass.EClassAlphaNumeric )
				{
					if	( Value.Length == 1 && Value == "_" )
					{
						Type = SymToken.TType.ETypeAlphaNumericUnderscore; 
					}
					else
					{
						Type = SymToken.TType.ETypeAlphaNumericNormal; 
					}
				}
			}
		}

		public void Combine( SymToken aToken )
		{
			if	( aToken.Value == ")" || Value == ")" )
			{
				int x = 9;
			}

			System.Diagnostics.Debug.Assert( Class != SymToken.TClass.EClassNull );
			System.Diagnostics.Debug.Assert( aToken.CombiningAllowed == true );
			System.Diagnostics.Debug.Assert( CombiningAllowed == true );
			//
			ForceCombine( aToken );
		}

		public void ForceCombine( SymToken aToken )
		{
			System.Diagnostics.Debug.Assert( Class != SymToken.TClass.EClassNull );
			//
			string newValue = iValue + aToken.Value;
			iValue = newValue;
		}
		#endregion

		#region Properties
		public bool CombiningAllowed
		{
			get
			{
				// We don't permit combining for new lines at all.
				// However, other combining is permitted (irrespective of class type).
				bool okayToCombine = ( Class != TClass.EClassNewLine );
				//
				if	( Value.Length == 1 )
				{
					// Check its not a disallowed character
					int index = KDisallowedCombiningCharacters.IndexOf( Value[ 0 ] );
					okayToCombine = ( index < 0 );
				}
				//
				return okayToCombine;
			}
		}

		public string Value
		{
			get { return iValue; }
		}

		public TClass Class
		{
			get { return iClass; }
			set
			{
				iClass = value;

				// When changing from some other type to a continuation, then
				// we also ensure we preseve the new line value
				if	( iClass == SymToken.TClass.EClassContinuation )
				{
					iValue = @"\" + System.Environment.NewLine;
				}
			}
		}

		public TType Type
		{
			get { return iType; }
			set { iType = value; }
		}

		public SymTextPosition Position
		{
			get { return iPosition; }
		}

		public object Tag
		{
			get { return iTag; }
			set { iTag = value; }
		}
		#endregion

		#region From System.Object
		public override string ToString()
		{
			return Value;
		}

		public override bool Equals( object aObject )
		{
			bool same = false;
			//
			if	( aObject is SymToken )
			{
				SymToken otherToken = (SymToken) aObject;
				//
				if	( otherToken.Class == Class )
				{
					if	( otherToken.Type == Type )
					{
						same = ( otherToken.Value == Value );
					}
				}
			}
			//
			return same;
		}
		#endregion

		#region Internal Constants
		private string KDisallowedCombiningCharacters = "!()\'\";";
		#endregion

		#region Data members
		private string iValue;
		private TClass iClass = TClass.EClassNull;
		private TType iType = TType.ETypeUnidentified;
		private object iTag;
		private SymTextPosition iPosition = new SymTextPosition();
		#endregion
	}
}
