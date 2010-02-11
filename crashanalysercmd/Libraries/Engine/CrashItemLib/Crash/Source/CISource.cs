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
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;

namespace CrashItemLib.Crash.Source
{
    /// <summary>
    /// Base class for all crash sources
    /// </summary>
    public abstract class CISource : IEnumerable<FileInfo>
    {
        #region Constructors
        protected CISource( FileInfo aFile )
		{
            iFile = aFile;
        }
        #endregion

        #region Constants
        public static readonly string[] KExtensionsTrace = new string[] { ".txt", ".log", ".trace" };
        #endregion

        #region API
        public void AddAdditionalFile( FileInfo aFile )
        {
            iAdditionalFiles.Add( aFile );
        }

        public static bool SubstringMatch( string aText, string[] aCandidates )
        {
            string text = aText.ToUpper();
            foreach ( string candidate in aCandidates )
            {
                if ( text.Contains( candidate.ToUpper() ) )
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The primary file which essentially describes the crash.
        /// </summary>
        public FileInfo MasterFile
        {
            get { return iFile; }
        }

        public string MasterFileName
        {
            get
            {
                string ret = string.Empty;
                //
                if ( MasterFile != null )
                {
                    ret = MasterFile.FullName;
                }
                //
                return ret; 
            }
        }

        /// <summary>
        /// Get other files which may be related to the master file.
        /// For example, D_EXC stores it stack file separately to
        /// the main text file, therefore the STK file would be present
        /// only within the 'all files' array.
        /// </summary>
        public FileInfo[] AllFiles
        {
            get
            {
                List<FileInfo> ret = new List<FileInfo>( this.iAdditionalFiles );
                ret.Insert( 0, MasterFile );
                return ret.ToArray();
            }
        }

        public string Extension
        {
            get { return Path.GetExtension( MasterFileName ).ToLower(); }
        }

        public int AdditionalFileCount
        {
            get { return iAdditionalFiles.Count; }
        }

        public long LineNumber
        {
            get { return iLineNumber; }
            set { iLineNumber = value; }
        }

        public bool Exists
        {
            get { return File.Exists( MasterFileName ); }
        }

        public bool IsTraceExtension
        {
            get
            {
                string extn = Extension;
                bool ret = SubstringMatch( extn, KExtensionsTrace );
                return ret;
            }
        }

        public bool IsLineNumberAvailable
        {
            get { return iLineNumber != KLineNumberNotApplicable; }
        }
        #endregion

        #region Framework properties
        public abstract Version ImplementorVersion
        {
            get;
        }

        public abstract string ImplementorName
        {
            get;
        }
        #endregion
        
        #region Internal constants
        private const long KLineNumberNotApplicable = -1;
        #endregion

        #region Internal methods
        internal void RawDataClear()
        {
            iRawData.Clear();
        }

        internal byte[] RawData
        {
            get { return iRawData.ToArray(); }
        }

        internal void RawDataAdd( byte[] aRawData )
        {
            iRawData.AddRange( aRawData );
        }
        #endregion

        #region From IEnumerable<FileInfo>
        public IEnumerator<FileInfo> GetEnumerator()
        {
            foreach ( FileInfo file in iAdditionalFiles )
            {
                yield return file;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( FileInfo file in iAdditionalFiles )
            {
                yield return file;
            }
        }
        #endregion

        #region Data members
        private FileInfo iFile = null;
        private List<FileInfo> iAdditionalFiles = new List<FileInfo>();
        private List<byte> iRawData = new List<byte>( 1024 );
        private long iLineNumber = KLineNumberNotApplicable;
        #endregion
    }
}
