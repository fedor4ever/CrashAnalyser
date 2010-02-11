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
using CrashItemLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using CAPCrashAnalysis.Plugin;
using SymbianUtils.FileSystem.Utilities;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Elements.Types.Category;
using SymbianXmlInputLib.Elements.Types.Extension;
using SymbianXmlInputLib.Elements.Types.FileSystem;
using SymbianXmlInputLib.Elements.Types.Command;
using CrashItemLib.Sink;

namespace CAPCrashAnalysis.CommandLine
{
	internal class CACmdLineInputParameters
	{
		#region Constructors
        public CACmdLineInputParameters( CAPluginCrashAnalysis aEngine )
		{
            iEngine = aEngine;
            //
            Version version = aEngine.UIManager.UIVersion;
            string commandLine = aEngine.UIManager.UICommandLineArguments;
            //
            iSinkParams = new CISinkSerializationParameters( version, commandLine );
		}
		#endregion

		#region API
        public void Read( string aFileName )
        {
            Trace( "[CmdInput] Read() - aFileName: " + aFileName );
            try
            {
                // First create the tree
                SXILDocument doc = CreateDocumentTree( aFileName );

                // Then convert it to the list of elements that we care about
                ExtractData( doc );
            }
            catch ( CACmdLineException cmdLineException )
            {
                Trace( "[CmdInput] Read() - CACmdLineException: " + cmdLineException.Message + " " + cmdLineException.StackTrace );
                throw cmdLineException;
            }
            catch ( Exception generalException )
            {
                Trace( "[CmdInput] Read() - generalException: " + generalException.Message + " " + generalException.StackTrace );
                throw new CACmdLineException( "Error reading input xml file", generalException, CAPlugin.KErrCommandLinePluginArgumentsFileInvalid );
            }
            Trace( "[CmdInput] Read() - read OK: " + aFileName );
        }
        #endregion

		#region Properties
        public CISinkSerializationParameters SinkParameters
        {
            get { return iSinkParams; }
        }

        public CACmdLineFSEntityList<CACmdLineFileSource> SourceFiles
        {
            get { return iSources; }
        }

        public CACmdLineFSEntityList<CACmdLineFSEntity> MetaDataFiles
        {
            get { return iMetaData; }
        }
        #endregion

        #region Internal constants
        private const string KInputFileDocumentRootNode = "crash_analysis";
        private const string KInputFileCategorySource = "source";
        private const string KInputFileCategoryDebugMetaData = "debug_meta_data";
        private const string KInputFileCategoryParameters = "parameters";
        private const string KInputFileCategoryOutput = "output";
        private const string KInputFileCommandNameAnalysis = "analysis_type";
        private const string KInputFileCommandNameAnalysisFull = "FULL";
        private const string KInputFileCommandNameAnalysisSummary = "SUMMARY";
        #endregion

        #region Internal methods
        private SXILDocument CreateDocumentTree( string aFileName )
        {
            SXILDocument doc = new SXILDocument();

            // Read input file into document
            using ( SXILParser parser = new SXILParser( aFileName, KInputFileDocumentRootNode, doc ) )
            {
                parser.CategoryAdd( KInputFileCategorySource, new SXILParserNodeFileSystem() );
                parser.CategoryAdd( KInputFileCategoryDebugMetaData, new SXILParserNodeFileSystem() );
                parser.CategoryAdd( KInputFileCategoryParameters,
                    new SXILParserNodeCommand(),
                    new SXILParserNodeExtension()
                    );
                parser.CategoryAdd( KInputFileCategoryOutput, new SXILParserNodeFileSystem() );
                parser.Parse();
            }

            return doc;
        }

        private void ExtractData( SXILDocument aDocument )
        {
            foreach ( SXILElement element in aDocument )
            {
                if ( element is SXILElementCategory )
                {
                    SXILElementCategory category = (SXILElementCategory) element;
                    string name = category.Name.ToLower();
                    //
                    switch ( name )
                    {
                    case KInputFileCategorySource:
                        ExtractFileList<CACmdLineFileSource>( iSources, category );
                        break;
                    case KInputFileCategoryDebugMetaData:
                        // The debug meta data engine doesn't support directories anymore
                        // so we have to expand all directories to files.
                        ExtractFileList<CACmdLineFSEntity>( iMetaData, category );
                        break;
                    case KInputFileCategoryParameters:
                        ExtractParameters( category );
                        break;
                    case KInputFileCategoryOutput:
                        ExtractOutput( category );
                        break;
                    }
                }
            }

            // We don't require debug meta data if performing a summary operation. Otherwise, we do.
            if ( iMetaData.Count == 0 && iSinkParams.DetailLevel == CISinkSerializationParameters.TDetailLevel.EFull )
            {
                Trace( "[CmdInput] ExtractData() - WARNING - no debug meta data supplied for full analysis." );
            }
        
        }

        private void ExtractFileList<T>( CACmdLineFSEntityList<T> aList, SXILElementCategory aCategory ) where T : CACmdLineFSEntity, new()
        {
            foreach ( SXILElement element in aCategory )
            {
                if ( element is SXILElementFile )
                {
                    SXILElementFile file = (SXILElementFile) element;
                    Trace( "[CmdInput] ExtractFileList() - file: " + file );
                    if ( !file.Exists )
                    {
                        throw new FileNotFoundException( "File not found", file.Name );
                    }
                    //
                    aList.Add( file );
                }
                else if ( element is SXILElementDirectory )
                {
                    SXILElementDirectory dir = (SXILElementDirectory) element;
                    Trace( "[CmdInput] ExtractFileList() - dir:  " + dir );
                    if ( !dir.Exists )
                    {
                        throw new DirectoryNotFoundException( "Directory not found: " + dir.Name );
                    }
                    //
                    aList.AddRange( dir.Files );
                }
            }
        }

        private void ExtractParameters( SXILElementCategory aCategory )
        {
            foreach ( SXILElement element in aCategory )
            {
                if ( element is SXILElementExtension )
                {
                    SXILElementExtension entry = (SXILElementExtension) element;
                    //
                    string extension = entry.Name;
                    if ( !extension.StartsWith( "." ) )
                    {
                        extension = "." + extension;
                    }
                    //
                    if ( entry.Type == SXILElementExtension.TType.ETypeFailure )
                    {
                        Trace( "[CmdInput] ExtractFileList() - failed extension: " + extension );
                        iSinkParams.FileExtensionFailed = extension;
                    }
                    else if ( entry.Type == SXILElementExtension.TType.ETypeSuccess )
                    {
                        Trace( "[CmdInput] ExtractFileList() - success extension: " + extension );
                        iSinkParams.FileExtensionSuccess = extension;
                    }
                }
                else if ( element is SXILElementCommand )
                {
                    SXILElementCommand entry = (SXILElementCommand) element;
                    //
                    if ( entry.Name == KInputFileCommandNameAnalysis )
                    {
                        string type = entry.Details.Trim().ToUpper();
                        Trace( "[CmdInput] ExtractFileList() - command: " + type );
                        switch ( type )
                        {
                        case KInputFileCommandNameAnalysisFull:
                            iSinkParams.DetailLevel = CISinkSerializationParameters.TDetailLevel.EFull;
                            break;
                        case KInputFileCommandNameAnalysisSummary:
                            iSinkParams.DetailLevel = CISinkSerializationParameters.TDetailLevel.ESummary;
                            break;
                        default:
                            throw new NotSupportedException( "Unsupported analysis type" );
                        }
                    }
                    else
                    {
                        throw new NotSupportedException( "Unsupported command: " + entry.Name );
                    }
                }
            }
        }

        private void ExtractOutput( SXILElementCategory aCategory )
        {
            // We either output to file or directory - if both are present then bail out
            FileInfo savedFile = null;
            DirectoryInfo savedDir = null;
            //
            foreach ( SXILElement element in aCategory )
            {
                if ( element is SXILElementFile )
                {
                    if ( savedFile != null )
                    {
                        throw new InvalidDataException( "Output file already specified" );
                    }
                    else
                    {
                        SXILElementFile file = (SXILElementFile) element;
                        savedFile = file;
                    }
                }
                else if ( element is SXILElementDirectory )
                {
                    if ( savedDir != null )
                    {
                        throw new InvalidDataException( "Output directory already specified" );
                    }
                    else
                    {
                        SXILElementDirectory dir = (SXILElementDirectory) element;
                        savedDir = dir;
                    }
                }
            }
            
            // Ensure we have only one type
            if ( savedFile != null && savedDir != null )
            {
                throw new InvalidDataException( "Output must be EITHER file or directory" );
            }
            else if ( savedFile != null )
            {
                iSinkParams.OutputFile = savedFile;
            }
            else if ( savedDir != null )
            {
                iSinkParams.OutputDirectory = savedDir;
            }
        }

        public void Trace( string aMessage )
        {
            iEngine.UIManager.UITrace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iEngine.UIManager.UITrace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly CAPluginCrashAnalysis iEngine;
        private readonly CISinkSerializationParameters iSinkParams;
        private CACmdLineFSEntityList<CACmdLineFSEntity> iMetaData = new CACmdLineFSEntityList<CACmdLineFSEntity>();
        private CACmdLineFSEntityList<CACmdLineFileSource> iSources = new CACmdLineFSEntityList<CACmdLineFileSource>();
        #endregion
	}
}
