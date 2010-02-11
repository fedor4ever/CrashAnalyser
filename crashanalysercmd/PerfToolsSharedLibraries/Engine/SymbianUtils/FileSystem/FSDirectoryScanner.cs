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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using SymbianUtils;

namespace SymbianUtils.FileSystem
{
    public class FSDirectoryScanner : DisposableObject
    {
        #region Delegates & events
        public delegate void OperationStarted( FSDirectoryScanner aScanner );
        public event OperationStarted Started;
        public delegate void ProgressHandler( FSDirectoryScanner aScanner, int aProgress, FileInfo aFile );
        public event ProgressHandler Progress;
        public delegate void OperationComplete( FSDirectoryScanner aScanner );
        public event OperationComplete Complete;
        #endregion

        #region Constructors
        public FSDirectoryScanner()
        {
            iWorker.ProgressChanged += new ProgressChangedEventHandler( Worker_ProgressChanged );
            iWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( Worker_RunWorkerCompleted );
            iWorker.DoWork += new DoWorkEventHandler( Worker_DoWork );
        }
        #endregion

        #region API
        public virtual void Start( DirectoryInfo aDirectory )
        {
            iWorker.RunWorkerAsync( aDirectory );
        }
        #endregion

        #region Framework API
        protected virtual void OnFileLocated( FileInfo aFile )
        {
        }
        #endregion

        #region Worker event handlers
        private void Worker_DoWork( object aSender, DoWorkEventArgs aArgs )
        {
            if ( Started != null )
            {
                Started( this );
            }
            //
            DirectoryInfo dir = aArgs.Argument as DirectoryInfo;
            if ( dir != null && dir.Exists )
            {
                // Locate all the map files in the directory
                FileInfo[] fileInfoList = dir.GetFiles( "*.*" );
                int count = fileInfoList.Length;
                for( int i=0; i<count; i++ )
                {
                    FileInfo file = fileInfoList[ i ];
                    //
                    ReportProgress( file, i, count );
                }
            }
        }

        private void Worker_RunWorkerCompleted( object aSender, RunWorkerCompletedEventArgs aArgs )
        {
            if ( Complete != null )
            {
                Complete( this );
            }
        }

        private void Worker_ProgressChanged( object aSender, ProgressChangedEventArgs aArgs )
        {
            if ( Progress != null )
            {
                Progress( this, aArgs.ProgressPercentage, aArgs.UserState as FileInfo );
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            if ( iWorker != null )
            {
                iWorker.Dispose();
                iWorker = null;
            }
            base.CleanupManagedResources();
        }
        #endregion

        #region Internal methods
        protected void ReportProgress( FileInfo aFile, int aFileIndex, int aFileCount )
        {
            OnFileLocated( aFile );
            //
            int progress = (int) ( ( (float) aFileIndex / (float) aFileCount ) * 100.0f );
            iWorker.ReportProgress( progress, aFile );
        }
        #endregion

        #region Data members
        private BackgroundWorker iWorker = new BackgroundWorker();
        #endregion
    }
}
