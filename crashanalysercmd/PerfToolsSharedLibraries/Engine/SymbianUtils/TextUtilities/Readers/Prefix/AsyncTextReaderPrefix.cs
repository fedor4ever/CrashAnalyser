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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;

namespace SymbianUtils
{
	public class AsyncTextReaderPrefix
	{
		#region Construct & destruct
		public AsyncTextReaderPrefix()
		:	this( string.Empty )
		{
		}

		public AsyncTextReaderPrefix( string aMsgPrefix )
		:   this( aMsgPrefix, string.Empty )
		{
		}

		public AsyncTextReaderPrefix( string aMsgPrefix, string aMsgPostfix )
		{
			iMsgPrefix = aMsgPrefix;
			iMsgPostfix = aMsgPostfix;
		}
		#endregion

		#region Properties
		public string MsgPrefix
		{
			get { return iMsgPrefix; }
		}

		public string MsgPostfix
		{
			get { return iMsgPostfix; }
		}
		#endregion

		#region Internal API
		internal void Clean( ref string aLine )
		{
			if ( MsgPrefix.Length > 0 )
			{
				int pos = aLine.IndexOf( MsgPrefix );
				if ( pos >= 0 )
				{
					pos += MsgPrefix.Length;
					aLine = aLine.Substring( pos );
				}
				else
				{
					aLine = string.Empty;
				}
			}
			//
			if ( MsgPostfix.Length > 0 )
			{
				int pos = aLine.LastIndexOf( MsgPostfix );
				if ( pos >= 0 )
				{
					pos -= MsgPostfix.Length;
					aLine = aLine.Substring( 0, pos - 1 );
				}
				else
				{
					aLine = string.Empty;
				}
			}
		}
		#endregion

		#region Data members
		private readonly string iMsgPrefix;
		private readonly string iMsgPostfix;
		#endregion
	}
}
