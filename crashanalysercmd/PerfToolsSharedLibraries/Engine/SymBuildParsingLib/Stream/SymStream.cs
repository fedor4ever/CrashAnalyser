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
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace SymBuildParsingLib
{
	#region Exceptions
	public class SymParserStreamExceptionInvalidRewindAmount : Exception
	{
		public SymParserStreamExceptionInvalidRewindAmount( string aMessage )
			: base( aMessage )
		{
		}
	}
	#endregion

	public class SymStream
	{
		#region Constructors & destructor
		public SymStream( string aFileName )
		{
			iFileName = aFileName;
			iFileStream = new FileStream( aFileName, System.IO.FileMode.Open );
			iStream = new StreamReader( iFileStream );
			iFileData = new Stack( 1024 * 20 );
		}
		#endregion

		#region API
		public char PeekChar()
		{
			Debug.Assert( !EOF );
			//
			char ret = (char) iStream.Peek();
			//
			if	( iPushedBackCharacters.Count > 0 )
			{
				ret = (char) iPushedBackCharacters.Peek();
			}
			return ret;
		}

		public char ReadChar()
		{
			Debug.Assert( !EOF );
			//
			char ret;
			//
			if	( iPushedBackCharacters.Count > 0 )
			{
				ret = (char) iPushedBackCharacters.Pop();
			}
			else
			{
				ret = (char) iStream.Read();
			}
			//
			iFileData.Push( ret );
			++iPosition;
			//
			return ret;
		}

		public void RewindChar()
		{
			RewindChars( 1 );
		}

		public void RewindChars( int aRewindCount )
		{
			long newPos = Position - aRewindCount;
			if	( aRewindCount < 0 )
				throw new SymParserStreamExceptionInvalidRewindAmount( "Rewind specifier must be >= 0" );
			else if ( newPos < 0 || newPos >= Size ) 
				throw new SymParserStreamExceptionInvalidRewindAmount( "Rewind specifier takes stream position beyond scope of file" );

			for( int i=0; i<aRewindCount; i++ )
			{
				char character = (char) iFileData.Pop();
				iPushedBackCharacters.Push( character );
			}

			iPosition -= aRewindCount;
		}
		#endregion

		#region Properties
		public string FileName
		{
			get { return iFileName; }
		}

		public long Position
		{
			get { return iPosition; }
		}

		public long Size
		{
			get { return iStream.BaseStream.Length; }
		}

		public bool EOF
		{
			get
			{
				long fileSize = Size;
				bool atEnd = ( Position >= fileSize );
				return atEnd;
			}
		}
		#endregion

		#region Constants
		private const int KStreamEOF = -1;
		#endregion

		#region Data members
		private readonly string iFileName;
		private StreamReader iStream;
		private FileStream iFileStream;
		private Stack iPushedBackCharacters = new Stack( 1024 );
		private Stack iFileData;
		private long iPosition;
		#endregion
	}
}
