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

namespace CrashItemLib.Engine.Primer
{
    public class CIEnginePrimer
    {
        #region Constructors
        internal CIEnginePrimer( CIEngine aEngine )
		{
            iEngine = aEngine;
            iWorkingSet = new CIPrimerWorkingSet( aEngine );
        }
		#endregion

        #region API
        public bool Prime( FileInfo aFile )
        {
            Clear();
            //
            CFFFileList otherFiles = new CFFFileList();
            bool success = PrimeOne( aFile, otherFiles );
            Flush();
            //
            return success;
        }

        public bool Prime( DirectoryInfo aDirectory )
        {
            int successCount = 0;
            //
            Clear();
            //
            CFFFileList otherFiles = new CFFFileList( aDirectory );
            while ( !otherFiles.IsEmpty )
            {
                FileInfo file = otherFiles.Dequeue();
                //
                try
                {
                    bool success = PrimeOne( file, otherFiles );
                    if ( success )
                    {
                        ++successCount;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            Flush();
            //
            return ( successCount > 0 );
        }

        public bool PrimeRecursive( DirectoryInfo aDirectory )
        {
            int successCount = 0;
            //
            Clear();
            //
            CFFFileList otherFiles = new CFFFileList( aDirectory, SearchOption.AllDirectories );
            while ( !otherFiles.IsEmpty )
            {
                FileInfo file = otherFiles.Dequeue();
                //
                try
                {
                    bool success = PrimeOne( file, otherFiles );
                    if ( success )
                    {
                        ++successCount;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            Flush();
            //
            return ( successCount > 0 );
        }
        #endregion

        #region Properties
        internal CFFPluginRegistry PluginLoader
        {
            get { return iEngine.PluginRegistry; }
        }

        internal CIEngineSourceCollection Sources
        {
            get { return iEngine.Sources; }
        }
        #endregion

        #region Internal methods
        private void Clear()
        {
            // Start with a clear list
            iWorkingSet.Clear();
        }

        private void Flush()
        {
            // This populates the engine's source list
            iWorkingSet.Rationalise();
            iWorkingSet.Clear();
        }

        private bool PrimeOne( FileInfo aFile, CFFFileList aOtherFiles )
        {
            bool success = false;

            // Check with the plugin loader to find all handlers
            CFFSourceAndConfidence[] handlers = PluginLoader.GetHandlers( aFile, aOtherFiles );
            if ( handlers.Length > 0 )
            {
                iWorkingSet.Add( handlers );
                success = true;
            }
            //
            return success;
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private readonly CIPrimerWorkingSet iWorkingSet;
		#endregion
    }
}
