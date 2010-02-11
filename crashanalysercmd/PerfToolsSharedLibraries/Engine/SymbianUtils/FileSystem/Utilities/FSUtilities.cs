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

namespace SymbianUtils.FileSystem.Utilities
{
	public static class FSUtilities
    {
        #region Constants
        public const int KKiloByte = 1024;
        public const int KMegaByte = KKiloByte * KKiloByte;
        #endregion

        #region API
        public static bool Exists( string aFileName )
        {
            bool ret = false;
            //
            try
            {
                ret = File.Exists( aFileName );
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }

        public static void DeleteFile( string aFileName )
        {
            try
            {
                if ( File.Exists( aFileName ) )
                {
                    FileAttributes attribs = File.GetAttributes( aFileName );
                    attribs &= ~FileAttributes.ReadOnly;
                    attribs &= ~FileAttributes.System;
                    attribs &= ~FileAttributes.Hidden;
                    File.SetAttributes( aFileName, attribs );

                    File.Delete( aFileName );
                }
            }
            finally
            {
            }
        }

		public static string PrettyFileFormatSize( long aValue, bool aExtraRounding )
		{
			string ret = string.Empty;

			if  ( aValue < 1024000 )					// If < 1000K
			{
				long sizeInK = 0;

				if  ( aValue != 0 )
				{
					sizeInK = (long) ( (aValue + 512) >> 10 );

					if  (sizeInK < 1)
					{
						sizeInK = 1;
					}
					if  (sizeInK > 999)
					{
						sizeInK = 999;
					}
				}

				ret = sizeInK.ToString() + "K";
			}
			else
			{
				double sizeInM = (double) aValue;
				sizeInM /= (double) KMegaByte;
				if  ( sizeInM < 1 )
				{
					sizeInM = 1;
				}

				string postfix = "M";
				if  ( sizeInM >= 1000 )
				{
					sizeInM /= 1024;				// Size in G
					if  (sizeInM < 1)
					{
						sizeInM = 1;
					}
					
					postfix = "G";
				}

				if  ( sizeInM > 999.9)
				{
					sizeInM = 999.9;
				}

				if  ( aExtraRounding )
				{
					sizeInM += 0.499999;
				}

				ret = sizeInM.ToString( "##############.00" ) + postfix;
			}

			return ret;
		}

		public static string FormatFileSize( long aSize )
		{
			string ret = "";
			if	( aSize > KMegaByte )
			{
				ret = aSize.ToString("###,000,000");
			}
			else if ( aSize > 1024 )
			{
				ret = aSize.ToString("###,000");
			}
			else
			{
				ret += aSize.ToString("###");
			}
			return ret;
		}

		public static string MakeTempFileName( string aPath )
		{
			string tempFileName = string.Empty;
			//
			while( tempFileName == string.Empty || File.Exists( tempFileName ) )
			{
				DateTime date = DateTime.Now;
				//
				tempFileName = "temp_file_" + date.Minute.ToString("d2") + "_" + date.Ticks.ToString("d6") + ".tmp";
				tempFileName = Path.Combine( aPath, tempFileName );
				//
				System.Threading.Thread.Sleep( 5 );
			}
			//
			return tempFileName;
        }

        public static string StripAllExtensions( string aFileName )
        {
            string temp = Path.GetFileNameWithoutExtension( aFileName );
            string ret = string.Empty;
            //
            do
            {
                ret = temp;
                temp = Path.GetFileNameWithoutExtension( ret );
            }
            while( temp != ret );

            return ret;
        }

        public static void ClearTempPath()
        {
            try
            {
                string path = TempPathBaseDir;
                DirectoryInfo dir = new DirectoryInfo( path );
                //
                if ( dir.Exists )
                {
                    dir.Delete( true );
                }
            }
            catch ( Exception )
            {
            }
        }

        public static string MakeTempPath()
        {
            string ret = string.Empty;
            string temp = TempPathBaseDir;
            //
            while ( ret == string.Empty )
            {
                try
                {
                    string tempPathExtension = SymbianUtils.Strings.StringUtils.MakeRandomString();
                    string path = Path.Combine( temp, tempPathExtension ) + Path.DirectorySeparatorChar;
                    //
                    DirectoryInfo dir = new DirectoryInfo( path );
                    if ( !dir.Exists )
                    {
                        dir.Create();
                        ret = path;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return ret;
        }
        #endregion

        #region Internal properties
        private static string TempPathBaseDir
        {
            get
            {
                string temp = Path.Combine( Path.GetTempPath(), KTempPathFolder );
                return temp;
            }
        }
        #endregion

        #region Internal constants
        private const string KTempPathFolder = "SymbianNetTools";
        #endregion
    }
}
