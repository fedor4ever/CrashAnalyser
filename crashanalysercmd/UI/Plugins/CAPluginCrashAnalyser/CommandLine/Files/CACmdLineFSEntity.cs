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
using System.Collections.Generic;
using CrashAnalyserEngine.Plugins;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;

namespace CAPCrashAnalysis.CommandLine
{
    internal class CACmdLineFSEntity : CACmdLineMessageList
	{
        #region Constructors
        public CACmdLineFSEntity()
		{
		}
        #endregion

		#region API
        #endregion

		#region Properties
        public FileInfo File
        {
            get { return iFile; }
            set { iFile = value; }
        }

        public DirectoryInfo Directory
        {
            get { return iDirectory; }
            set { iDirectory = value; }
        }

        public bool IsFile
        {
            get { return ( iFile != null ); }
        }

        public bool IsDirectory
        {
            get { return ( iDirectory != null); }
        }

        public string Name
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                //
                if ( IsDirectory )
                {
                    ret.Append( Directory.FullName );
                }
                else if ( IsFile )
                {
                    ret.Append( File.FullName );
                }
                //
                return ret.ToString(); 
            }
        }

        public bool Exists
        {
            get
            {
                bool ret = false;
                //
                if ( IsDirectory )
                {
                    ret = Directory.Exists;
                }
                else if ( IsFile )
                {
                    ret = File.Exists;
                }
                //
                return ret;
            }
        }
        
        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        internal string NameUC
        {
            get
            {
                string ret = this.Name.ToUpper();
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator FileInfo( CACmdLineFSEntity aFile )
        {
            return aFile.iFile;
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private object iTag = null;
        private FileInfo iFile = null;
        private DirectoryInfo iDirectory = null;
        #endregion
	}
}
