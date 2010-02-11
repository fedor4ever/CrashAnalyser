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
using System.Reflection;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine.Sources;

namespace CrashItemLib.PluginAPI
{
    public class CFFFileList
    {
        #region Constructors
        internal CFFFileList()
        {
        }

        internal CFFFileList( DirectoryInfo aDir )
            : this( aDir, SearchOption.TopDirectoryOnly )
        {
        }

        internal CFFFileList( DirectoryInfo aDir, SearchOption aSearchOption )
		{
            BuildLists( aDir, aSearchOption );
        }
		#endregion

        #region API
        public void Clear()
        {
            iDictionary.Clear();
            iFiles.Clear();
        }

        public bool Contains( string aFileName )
        {
            string key = Key( aFileName );
            return iDictionary.ContainsKey( key );
        }

        public bool Contains( FileInfo aFile )
        {
            return Contains( aFile.FullName );
        }

        public void Remove( string aFileName )
        {
            string key = Key( aFileName );
            if ( iDictionary.ContainsKey( key ) )
            {
                FileInfo file = iDictionary[ key ];
                iFiles.Remove( file );
                iDictionary.Remove( key );
            }
        }

        public void Remove( FileInfo aFile )
        {
            Remove( aFile.FullName );
        }

        public FileInfo Dequeue()
        {
            System.Diagnostics.Debug.Assert( Count > 0 );
            FileInfo ret = this[ 0 ];
            //
            iFiles.RemoveAt( 0 );
            iDictionary.Remove( Key( ret ) );
            //
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert( iFiles.Count == iDictionary.Count );
                return iFiles.Count;
            }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public FileInfo this[ int aIndex ]
        {
            get { return iFiles[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        private string Key( FileInfo aFile )
        {
            return Key( aFile.FullName );
        }

        private string Key( string aFileName )
        {
            return aFileName.ToUpper();
        }

        private void BuildLists( DirectoryInfo aDirectory, SearchOption aSearchOption )
        {
            Clear();
            //
            FileInfo[] files = aDirectory.GetFiles( "*.*", aSearchOption );
            iFiles.AddRange( files );
            //
            iDictionary = new Dictionary<string, FileInfo>();
            foreach ( FileInfo file in files )
            {
                string key = file.FullName.ToUpper();
                if ( iDictionary.ContainsKey( key ) )
                {
                    iDictionary.Add( key, file );
                }
            }
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private List<FileInfo> iFiles = new List<FileInfo>();
        private Dictionary<string, FileInfo> iDictionary = new Dictionary<string, FileInfo>();
		#endregion
    }
}
