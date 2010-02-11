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
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using SymbianUtils;
using SymbianUtils.SerializedOperations;
using SymbolLib.Sources.Map.Parser;

namespace SymbolLib.CodeSegDef
{
    internal class CSROpScanDirectory : CodeSegResolverOperation
	{
		#region Constructors
        public CSROpScanDirectory( CodeSegResolver aResolver, bool aResetResolver, DirectoryInfo aDirectory )
            : base( aResolver, aResetResolver )
		{
            iDirectory = aDirectory;
		}
		#endregion

        #region From CodeSegResolverOperation
        protected override void Scan()
        {
            base.DriveLetter = Path.GetPathRoot( iDirectory.FullName );
            if ( iDirectory.Exists )
            {
                // Locate all the map files in the directory
                FileInfo[] fileInfoList = iDirectory.GetFiles( "*" + CodeSegResolver.KMapFileExtension );
                foreach ( FileInfo file in fileInfoList )
                {
                    string pcFileName = file.FullName;

                    // Remove .map extension in order to get back to the pure binary dll/exe/etc name.
                    pcFileName = RemoveMapExtension( pcFileName );

                    // Prepare phone binary name by merging the PC-side filename with Z:\Sys\Bin
                    string phoneFileName = GeneratePhoneBinaryNameAndPath( pcFileName );

                    // Make entry
                    CodeSegResolverEntry entry = new CodeSegResolverEntry( pcFileName, phoneFileName );

                    base.Add( entry );
                }
            }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion
        
        #region Internal constants
        private const string KRomDriveFileLetter = "Z:\\";
		#endregion

        #region Data members
        private readonly DirectoryInfo iDirectory;
		#endregion
	}
}
