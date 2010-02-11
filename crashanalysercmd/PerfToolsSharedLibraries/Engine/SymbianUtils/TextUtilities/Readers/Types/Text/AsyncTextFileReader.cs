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
using SymbianUtils.Tracer;

namespace SymbianUtils
{
	public abstract class AsyncTextFileReader : AsyncTextReader
	{
		#region Constructors
		protected AsyncTextFileReader( string aFileName )
			: this( aFileName, new AsyncTextReaderPrefix(), null )
		{
		}

        protected AsyncTextFileReader( string aFileName, ITracer aTracer )
            : this( aFileName, new AsyncTextReaderPrefix(), false, aTracer )
        {
        }

        protected AsyncTextFileReader( string aFileName, bool aRouteBlankLines, ITracer aTracer )
            : this( aFileName, new AsyncTextReaderPrefix(), aRouteBlankLines, aTracer )
		{
		}

        protected AsyncTextFileReader( string aFileName, AsyncTextReaderPrefix aPrefixes )
            : this( aFileName, aPrefixes, false, null )
		{
		}

        protected AsyncTextFileReader( string aFileName, AsyncTextReaderPrefix aPrefixes, ITracer aTracer )
			: this( aFileName, aPrefixes, false, aTracer )
		{
		}

        protected AsyncTextFileReader( string aFileName, AsyncTextReaderPrefix aPrefixes, bool aRouteBlankLines )
            : this( aFileName, aPrefixes, aRouteBlankLines, null )
        {
		}

        protected AsyncTextFileReader( string aFileName, AsyncTextReaderPrefix aPrefixes, bool aRouteBlankLines, ITracer aTracer )
			: base( aPrefixes, aRouteBlankLines, aTracer )
		{
			iSourceFileName = aFileName;
		}
		#endregion

		#region From DisposableObject - Cleanup Framework
		protected override void CleanupManagedResources()
		{
			try
			{
				Cleanup();
			}
			finally
			{
				base.CleanupManagedResources();
			}
		}
		#endregion

		#region Properties
		public string FileName
		{
			get
			{
				string fileName = string.Empty;
				//
				lock( this )
				{
					fileName = iSourceFileName;
				}
				//
				return fileName; 
			}
		}

		public long LineNumber
		{
			get
			{
				long lineNumber = 0;
				//
				lock( this )
				{
					lineNumber = iLineNumber;
				}
				//
				return lineNumber; 
			}
		}
		#endregion

		#region From AsyncReaderBase
		protected override void HandleReadStarted()
		{
            FileStream stream = new FileStream( FileName, FileMode.Open, FileAccess.Read, FileShare.Read );
            iReader = new StreamReader( stream, Encoding.UTF8, false, 1024 );

            // Cache length because this property is VERY expensive to call.
            if ( iReader.BaseStream != null )
            {
                iSize = iReader.BaseStream.Length;
            }
            //
			base.HandleReadStarted();
		}

		protected override void HandleReadCompleted()
		{
			try
			{
				Cleanup();
			}
			finally
			{
				base.HandleReadCompleted();
			}
		}
		#endregion

		#region Abstract reading framework
		protected override string ProvideReadLine()
		{
			++iLineNumber;
			return iReader.ReadLine();
		}

		protected override long Size
		{
			get
			{
				long size = 0;
				//
				lock( this )
				{
                    size = iSize;
				}
				//
				return size;
			}
		}

		protected override long Position
		{
			get
			{
				long position = 0;
				//
				lock( this )
				{
					if (iReader != null && iReader.BaseStream != null)
					{
						position = iReader.BaseStream.Position;
					}
				}
				//
				return position;
			}
		}
		#endregion

		#region Internal methods
		private void Cleanup()
		{
			lock( this )
			{
				if	( iReader != null )
				{
					iReader.Close();
					iReader = null;
				}
			}
		}
		#endregion

		#region Data members
		private readonly string iSourceFileName;
		private StreamReader iReader;
		private long iLineNumber;
        private long iSize;
		#endregion
	}
}
