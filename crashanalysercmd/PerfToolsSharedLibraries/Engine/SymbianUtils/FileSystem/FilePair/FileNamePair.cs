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

namespace SymbianUtils.FileSystem.FilePair
{
	public class FileNamePair
	{
		#region Constructors
		public FileNamePair()
		{
		}

		public FileNamePair( string aSourceName )
		{
			iSource = aSourceName;
		}

		public FileNamePair( string aSourceName, string aDestinationName )
		{
			iSource = aSourceName;
			iDestination = aDestinationName;
		}
		#endregion

		#region API
		public void SetCustomDestinationPath( string aFolder )
		{
			string fileName = Path.GetFileName( Source );
            fileName = Path.Combine( aFolder, fileName );
			Destination = fileName;
            //
            if ( Destination.Length > 0 && ( Destination[ 0 ] == Path.AltDirectorySeparatorChar || Destination[ 0 ] == Path.DirectorySeparatorChar ) )
            {
                Destination = Destination.Substring( 1 );
            }
		}
		#endregion

		#region Properties
        public bool ProcessFile
        {
            get
            {
                bool ret = false;
                //
                if ( IsValid )
                {
                    if ( SourceExists )
                    {
                        if ( !SkipIfEmptyFile || SourceFileSize > 0 )
                        {
                            ret = true;
                        }
                    }
                }
                //
                return ret;
            }
        }

		public bool IsValid
		{
			get
			{
				bool sourceSet = ( Source.Length > 0 );
				bool destinationSet = ( Destination.Length > 0 );
				//
				return ( sourceSet && destinationSet );
			}
		}

        public bool SkipIfEmptyFile
        {
            get { return iSkipIfEmptyFile; }
            set { iSkipIfEmptyFile = value; }
        }

		public bool SourceExists
		{
			get
			{
				bool exists = File.Exists( Source );
				return exists;
			}
		}

		public bool DeleteFile
		{
            get { return iDeleteFile; }
            set { iDeleteFile = value; }
		}

		public string Source
		{
			get { return iSource; }
			set { iSource = value; }
		}

		public string Destination
		{
			get { return iDestination; }
			set { iDestination = value; }
		}

        public long SourceFileSize
        {
            get
            {
                long ret = 0;
                //
                try
                {
                    if ( SourceExists )
                    {
                        System.IO.FileInfo info = new FileInfo( Source );
                        ret = info.Length;
                    }
                }
                catch( Exception )
                {
                }
                //
                return ret;
            }
        }
		#endregion

		#region Data members
        private bool iDeleteFile = false;
        private bool iSkipIfEmptyFile = true;
		private string iSource = string.Empty;
		private string iDestination = string.Empty;
		#endregion
	}
}
