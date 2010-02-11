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
using System.ComponentModel;
using SymbianUtils;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine;
using SymbianUtils.FileSystem.Utilities;

namespace CrashItemLib.Sink
{
    public class CISinkSerializationParameters : DisposableObject
    {
        #region Enumerations
        public enum TDetailLevel
        {
            [Description("Full")]
            EFull = 0,

            [Description( "Summary" )]
            ESummary
        }
        #endregion

        #region Constructors
        public CISinkSerializationParameters( Version aUIVersion, string aUICommandLineArguments )
        {
            iUIVersion = aUIVersion;
            iUICommandLineArguments = aUICommandLineArguments;
            //
            string tempPath = FSUtilities.MakeTempPath();
            OutputDirectory = new DirectoryInfo( tempPath );
            //
            PrepareDefaultExtensions();
        }
        
        public CISinkSerializationParameters( CIContainer aContainer, Version aUIVersion, string aUICommandLineArguments )
            : this( aUIVersion, aUICommandLineArguments )
        {
            iContainer = aContainer;
        }

        public CISinkSerializationParameters( CISinkSerializationParameters aCopy )
            : this( aCopy.Container, aCopy.UIVersion, aCopy.UICommandLineArguments )
        {
            iDetailLevel = aCopy.DetailLevel;
            //
            FileExtensionSuccess = aCopy.FileExtensionSuccess;
            FileExtensionFailed = aCopy.FileExtensionFailed;
            //
            if ( aCopy.iOutputMode == TOutputMode.EOutputToFile )
            {
                OutputFile = aCopy.OutputFile;
            }
            else if ( aCopy.iOutputMode == TOutputMode.EOutputToDirectory )
            {
                OutputDirectory = aCopy.OutputDirectory;
            }
            //
            iOperationData1 = aCopy.OperationData1;
            iOperationData2 = aCopy.OperationData2;
            iOperationData3 = aCopy.OperationData3;
        }
        #endregion

        #region API
        public Stream CreateFile( out string aFileName )
        {
            return CreateFile( out aFileName, FileMode.Append );
        }

        public Stream CreateFile( out string aFileName, FileMode aMode )
        {
            System.Diagnostics.Debug.Assert( iContainer != null );

            // First, prepare the output directory information that we will write to.
            string fileName = Container.Source.MasterFileName;
            string sourceFileName = Path.GetFileName( fileName );
            string sourcePath = Path.GetDirectoryName( fileName );
            //
            switch ( iOutputMode )
            {
            case TOutputMode.EOutputToDirectory:
                // Use the OutputDirectory name, but combine with source file name
                fileName = Path.Combine( this.OutputDirectory.FullName, sourceFileName );
                fileName = AppendFileExtension( fileName );

                // Don't overwrite when writing to a specific directory.
                aMode = FileMode.CreateNew;
                break;
            default:
            case TOutputMode.EOutputToFile:
                // Use the specified OuputFile name.
                fileName = this.OutputFile.FullName;
                break;
            }

            // At this point we now have a fixed output path.
            // Ensure that it exists.
            DirectoryInfo outputDir = new DirectoryInfo( Path.GetDirectoryName( fileName ) );
            outputDir.Create();

            // Now try to make a unique file name if we are not appending.
            Stream ret = null;
            if ( aMode == FileMode.Append )
            {
                // Just append to file
                ret = TryToCreateStream( fileName, aMode );
            }
            else
            {
                // Update filename to just refer to the name and extension (no path)
                fileName = Path.GetFileName( fileName );

                // Try to create a unique file
                for ( int counter = 0; counter < KMaxRetries; counter++ )
                {
                    // First iteration is a special case were we use input name
                    // plus our standard extension
                    string finalFileName = AppendFileExtension( fileName );
                    if ( counter > 0 )
                    {
                        // Append a numerical value in order to create unique name
                        finalFileName = string.Format( "{0} ({1:d3})",
                                            Path.GetFileNameWithoutExtension( fileName ),
                                            counter );
                        finalFileName = AppendFileExtension( finalFileName );
                    }

                    string finalFullName = Path.Combine( outputDir.FullName, finalFileName );
 
                    // Attempt to create stream
                    ret = TryToCreateStream( finalFullName, aMode );
                    if ( ret != null )
                    {
                        fileName = finalFullName;
                        break;
                    }
                }
            }

            //
            if ( ret == null )
            {
                throw new IOException( "Unable to create sink file" );
            }
            else
            {
                // Ensure we inform caller of final output file name
                aFileName = fileName;
            }
            //
            return ret;
        }
        #endregion

        #region Framework API
        protected virtual void PrepareDefaultExtensions()
        {
            FileExtensionSuccess = string.Empty;
            FileExtensionFailed = string.Empty;
        }
        #endregion

        #region Properties
        public CIEngine Engine
        {
            get { return Container.Engine; }
        }

        public CIContainer Container
        {
            get { return iContainer; }
            set { iContainer = value; }
        }

        public TDetailLevel DetailLevel
        {
            get { return iDetailLevel; }
            set { iDetailLevel = value; }
        }

        public DirectoryInfo OutputDirectory
        {
            get
            {
                if ( iOutputMode != TOutputMode.EOutputToDirectory )
                {
                    throw new InvalidOperationException( "Output mode is invalid" );
                }
                return iOutputDirectory;
            }
            set
            {
                iOutputDirectory = value;
                iOutputDirectory.Create();
                //
                iOutputMode = TOutputMode.EOutputToDirectory;
                iOutputFile = null;
            }
        }

        public FileInfo OutputFile
        {
            get
            {
                if ( iOutputMode != TOutputMode.EOutputToFile )
                {
                    throw new InvalidOperationException( "Output mode is invalid" );
                }
                return iOutputFile; 
            }
            set
            {
                iOutputFile = value;
                //
                iOutputMode = TOutputMode.EOutputToFile;
                iOutputDirectory = null;
            }
        }

        public Version UIVersion
        {
            get { return iUIVersion; }
        }

        public string FileExtensionSuccess
        {
            get { return iFileExtensionSuccess; }
            set { iFileExtensionSuccess = value; }
        }

        public string FileExtensionFailed
        {
            get { return iFileExtensionFailed; }
            set { iFileExtensionFailed = value; }
        }

        public string UICommandLineArguments
        {
            get { return iUICommandLineArguments; }
        }

        public object OperationData1
        {
            get { return iOperationData1; }
            set { iOperationData1 = value; }
        }

        public object OperationData2
        {
            get { return iOperationData2; }
            set { iOperationData2 = value; }
        }

        public object OperationData3
        {
            get { return iOperationData3; }
            set { iOperationData3 = value; }
        }
        #endregion

        #region Internal methods
        private string AppendFileExtension( string aFileName )
        {
            System.Diagnostics.Debug.Assert( iContainer != null );
            
            // Work out which extension we should be adding to the output
            string extensionToAppend = FileExtensionSuccess;
            if ( Container.Status == CIContainer.TStatus.EStatusErrorContainer )
            {
                extensionToAppend = FileExtensionFailed;
            }

            // Then make the name
            string ret = aFileName;
            string extn = Path.GetExtension( aFileName );
            //
            if ( extn.ToUpper() == extensionToAppend.ToUpper() )
            {
                // Job done
            }
            else
            {
                ret += extensionToAppend;
            }
            //
            return ret;
        }

        private Stream TryToCreateStream( string aFileName, FileMode aMode )
        {
            Stream ret = null;
            //
            try
            {
                // We do this inside a catch block because in multi-threaded situations, there
                // could be a race between the entity checking whether a file exists, and another
                // thread actually just about to create the file. The File.Exists() check just
                // avoids unnecessarily attempting to create the file if we *know* that it already
                // exists. The catch copes with the unexpected pre-emption.
                if ( !File.Exists( aFileName ) )
                {
                    ret = new FileStream( aFileName, aMode, FileAccess.Write, FileShare.None, 1024 * 12 );
                }
            }
            catch ( IOException )
            {
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private const int KMaxRetries = 100;
        #endregion

        #region Internal enumerations
        private enum TOutputMode
        {
            EOutputToFile = 0,
            EOutputToDirectory
        }
        #endregion

        #region Data members
        private readonly Version iUIVersion;
        private readonly string iUICommandLineArguments;
        private CIContainer iContainer;
        private TDetailLevel iDetailLevel = TDetailLevel.EFull;
        private string iFileExtensionSuccess = string.Empty;
        private string iFileExtensionFailed = string.Empty;
        private FileInfo iOutputFile;
        private DirectoryInfo iOutputDirectory;
        private TOutputMode iOutputMode = TOutputMode.EOutputToDirectory;
        private object iOperationData1 = null;
        private object iOperationData2 = null;
        private object iOperationData3 = null;
        #endregion
    }
}
