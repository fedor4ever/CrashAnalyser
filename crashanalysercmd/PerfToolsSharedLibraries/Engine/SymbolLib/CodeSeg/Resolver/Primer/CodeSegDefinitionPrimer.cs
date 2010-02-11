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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianUtils;
using SymbianUtils.SerializedOperations;

namespace SymbolLib.CodeSegDef
{
	public class CodeSegDefinitionPrimer : SerializedOperationManager
	{
		#region Constructor
		internal CodeSegDefinitionPrimer( CodeSegResolver aResolver )
            : base( aResolver )
		{
            iResolver = aResolver;
            base.Start();
		}
		#endregion

        #region Constants
        #endregion

		#region API
        public CodeSegResolverOperation PrimeFromDirectory( DirectoryInfo aDirectory )
		{
            return PrimeFromDirectory( aDirectory, false );
		}

        public CodeSegResolverOperation PrimeFromDirectory( DirectoryInfo aDirectory, bool aResetResolver )
        {
            CodeSegResolverOperation ret = new CSROpScanDirectory( iResolver, aResetResolver, aDirectory );
            Queue( ret );
            return ret;
        }

        public CodeSegResolverOperation PrimeFromObey( FileInfo aFile )
		{
            return PrimeFromObey( aFile, false );
		}

        public CodeSegResolverOperation PrimeFromObey( FileInfo aFile, bool aResetResolver )
        {
            CodeSegResolverOperation ret = new CSROpScanObey( iResolver, aResetResolver, aFile );
            Queue( ret );
            return ret;
        }

        public CodeSegResolverOperation PrimeFromBinaries( FileInfo aFile, IEnumerable<string> aBinaryNames )
		{
            return PrimeFromBinaries( aFile, aBinaryNames, false );
        }

        public CodeSegResolverOperation PrimeFromBinaries( FileInfo aFile, IEnumerable<string> aBinaryNames, bool aResetResolver )
        {
            CodeSegResolverOperation ret = new CSROpScanSymbol( iResolver, aResetResolver, aFile, aBinaryNames );
            Queue( ret );
            return ret;
        }
        #endregion

        #region Properties
        #endregion

		#region Internal properties
		internal string DriveLetter
		{
			get
            {
                lock ( iDriveLetter )
                {
                    return iDriveLetter;
                }
            }
			set
            {
                lock ( iDriveLetter )
                {
                    iDriveLetter = value;
                }
            }
		}
		#endregion

		#region Data members
        private readonly CodeSegResolver iResolver;
        private string iDriveLetter = string.Empty;
		#endregion
	}
}
