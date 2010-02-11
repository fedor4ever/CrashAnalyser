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
	public abstract class CodeSegResolverOperation : SerializedOperation
    {
        #region Events
        public delegate void LocatedFileNameHandler( string aFileName );
        public event LocatedFileNameHandler LocatedFile;
        public delegate void RawTextHandler( string aText );
        public event RawTextHandler RawText;
        #endregion

        #region Constructors
        protected CodeSegResolverOperation( CodeSegResolver aResolver, bool aResetResolver )
            : base( false )
		{
            iResolver = aResolver;
            iResetResolver = aResetResolver;
		}
		#endregion

        #region From SerializedOperation
        protected override void PerformOperation()
        {
            base.Trace( "" );
            base.Trace( " => => => => => => => => => => => => => => => => => => => => => " );
            base.Trace( "" );
            base.Trace( "PerformOperation() - START - this: " + this.GetType().Name );

            if ( iResetResolver )
            {
                iResolver.Clear();
            }

            this.NotifyRawText( "Scanning..." );
            Scan();

            base.Trace( "PerformOperation() - END - this: " + this.GetType().Name );
        }
        #endregion

        #region API - new framework
        protected abstract void Scan();
        #endregion

        #region API
        protected void Add( CodeSegResolverEntry aEntry )
        {
            NotifyFile( aEntry.ImageFileNameAndPath );
            base.Trace( "[{0}] device: {1}, host: {2}", this.GetType().Name, aEntry.ImageFileNameAndPath, aEntry.EnvironmentFileNameAndPath );
            //
            iResolver.Add( aEntry );
        }
        #endregion

        #region Properties
        protected string DriveLetter
        {
            get { return iResolver.DriveLetter; }
            set { iResolver.DriveLetter = value; }
        }
        #endregion

        #region Internal methods
        protected void NotifyRawText( string aMessage )
        {
            if ( RawText != null )
            {
                RawText( aMessage );
            }
        }

        protected void NotifyFile( string aFileName )
        {
            if ( LocatedFile != null )
            {
                LocatedFile( aFileName );
            }
        }

        protected static string RemoveMapExtension( string aFileName )
        {
            StringBuilder ret = new StringBuilder( aFileName );
            //
            int pos = aFileName.LastIndexOf( CodeSegResolver.KMapFileExtension );
            if ( pos >= 1 )
            {
                ret.Remove( pos, CodeSegResolver.KMapFileExtension.Length );
            }
            //
            return ret.ToString().ToLower();
        }

        protected static string GeneratePhoneBinaryNameAndPath( string aFileName )
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( CodeSegResolver.KROMBinaryPath );
            ret.Append( Path.GetFileName( aFileName ).ToLower() );
            //
            return ret.ToString();
        }

        protected string CombineWithDriveLetter( string aFileName )
		{
			string ret = string.Empty;
			lock( this )
			{
				ret = Path.Combine( iResolver.DriveLetter, aFileName ).ToLower();
			}
			return ret;
		}
        #endregion

        #region Data members
        private readonly CodeSegResolver iResolver;
        private readonly bool iResetResolver;
		#endregion
	}
}
