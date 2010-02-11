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
	public abstract class AsyncBinaryFileReader : AsyncBinaryReaderBase
	{
		#region Construct & destruct
		public AsyncBinaryFileReader( string aFileName )
			: this( aFileName, KDefaultReadChunkSize )
		{
		}

		public AsyncBinaryFileReader( string aFileName, int aReadChunkSize )
		{
			iSourceFileName = aFileName;
			iReadChunkSize = aReadChunkSize;
		}
		#endregion

		#region Constants
		public const int KDefaultReadChunkSize = 512;
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

		protected BinaryReader Reader
		{
			get { return iReader; }
		}
		#endregion

		#region New API
		protected virtual void HandleReaderOpen()
		{
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

		#region From AsyncReaderBase
		protected override void HandleReadStarted()
		{
			iReader = new BinaryReader( new FileStream( FileName, FileMode.Open, FileAccess.Read ) );
			HandleReaderOpen();
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
		protected override byte[] ProvideReadBytes()
		{
			byte[] ret = iReader.ReadBytes( iReadChunkSize );
			return ret;
		}

		protected override long Size
		{
			get
			{
				long size = 0;
				//
				lock( this )
				{
					if	( iReader != null && iReader.BaseStream != null )
					{
						size = iReader.BaseStream.Length;
					}
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
					if	( iReader != null && iReader.BaseStream != null )
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
		private BinaryReader iReader;
		private readonly int iReadChunkSize;
		private readonly string iSourceFileName;
		#endregion
	}
}
