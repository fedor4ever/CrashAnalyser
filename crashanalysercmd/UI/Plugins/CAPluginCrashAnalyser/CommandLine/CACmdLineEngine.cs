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
using System.Xml;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Sink;
using CrashItemLib.Engine;
using CrashItemLib.Engine.Sources;
using CrashItemLib.PluginAPI;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Plugins;
using CrashAnalyserEngine.Interfaces;
using SymbianUtils.FileSystem.Utilities;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Elements.Types.Category;
using SymbianXmlInputLib.Elements.Types.FileSystem;
using SymbianXmlInputLib.Elements.Types.Command;
using CAPCrashAnalysis.Plugin;
using CAPCrashAnalysis.CommandLine.Progress;
using SymbianDebugLib;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianUtils;

namespace CAPCrashAnalysis.CommandLine
{
	internal class CACmdLineEngine
	{
		#region Constructors
        public CACmdLineEngine( CAPluginCrashAnalysis aEngine )
		{
            iEngine = aEngine;
            iInputs = new CACmdLineInputParameters( aEngine );
		}
		#endregion

        #region API
        public bool IsCommandLineHandler( string aName )
        {
            bool ret = ( aName.ToUpper() == KPluginCommandLineName );
            return ret;
        }

        public int RunCommandLineOperations()
        {
            UITrace( "[CA Cmd] START " );
            UITrace( string.Empty );
            UITrace( "[CA Cmd] command line: " + System.Environment.CommandLine );
            UITrace( "[CA Cmd] command wd:   " + System.Environment.CurrentDirectory );
            UITrace( "[CA Cmd] proc count:   " + System.Environment.ProcessorCount );
            UITrace( "[CA Cmd] sysdir:       " + System.Environment.SystemDirectory );
            UITrace( "[CA Cmd] version:      " + System.Environment.Version.ToString() );
            UITrace( string.Empty );

            int error = CAPlugin.KErrCommandLineNone;
            //
            try
            {
                // We expect to see an "-input" parameter
                string inputFileName = ExtractCommandLineInputParameter( CommandLineArguments );
                bool generateReport = CheckForReportParameter( CommandLineArguments );
                iProgressReporter.Enabled = CheckForProgressParameter( CommandLineArguments );
                iProgressReporter.Detailed = CheckForProgressDetailedParameter(CommandLineArguments);

                // If no file was found then inputFileName will be an empty string.
                if ( string.IsNullOrEmpty( inputFileName ) )
                {
                    throw new CACmdLineException( "Input file parameter missing", CAPlugin.KErrCommandLinePluginArgumentsMissing );
                }
                else if ( !FSUtilities.Exists( inputFileName ) )
                {
                    throw new CACmdLineException( "Input file not found", CAPlugin.KErrCommandLinePluginArgumentsFileNotFound );
                }
                else
                {
                    // Switch off UI output at the debug engine level
                    iEngine.DebugEngine.UiMode = TDbgUiMode.EUiDisabled;

                    // Reading the input file will throw an exception upon error.
                    // This is caught below and mapped onto an error code. There's nothing
                    // else we can do in this situation.
                    ReadInputFile( inputFileName );

                    // At this point we have enough information to identify the exact total
                    // number of progress reporting steps that will follow during the
                    // rest of the processing operation.
                    CalculateNumberOfOperationSteps();

                    // Next, attempt to prime the crash engine with every source we
                    // identified from the input specification. The goal is to
                    // create all the needed Crash Item Source objects for each input file.
                    // Any inputs which we don't support will not have an associated source and
                    // will be flagged accordingly within the CACmdLineFileSource object.
                    TryToPrimeSources();

                    // Now link the input files with crash source objects
                    AssociateInputFilesWithCrashItemSources();

                    // Next, prime the debug engine will all the debug meta-data inputs.
                    // Again, individual error messages will be associated with each meta-data
                    // input.
                    //
                    // We don't need to do this for summary operations
                    if ( iInputs.SinkParameters.DetailLevel != CISinkSerializationParameters.TDetailLevel.ESummary )
                    {
                        TryToPrimeDbgEngine();
                    }

                    // Next we print out progress steps for skipped files, because those 
                    // files are count to total steps.
                    PrintOutSkippepFiles();

                    // Next, we invoke the crash engine to process all the crash item sources we
                    // created during the prime step. Exceptions are caught and associated 
                    // messages & diagnostics are created at the input-file level.
                    TryToIdentifyCrashes();

                    // Next, we start the output phase. Any 'valid' crash containers are serialized
                    // to xml. Any input files which could not be processed have 'dummy' containers
                    // created for them, and these are also serialised to 'failed' XML files.
                    // If the XML Sink plugin is unavailable, then we cannot create any XML output.
                    // In this situation, we throw an exception which is caught below.
                    TryToCreateXmlOutput();

                    // Finally, we want to create the XML manifest/report data, which we'll emit 
                    // via standard output.
                    if ( generateReport )
                    {
                        CreateAndEmitXmlReport();
                    }
                }
            }
            catch ( CACmdLineException cmdLineException )
            {
                error = cmdLineException.ErrorCode;
                //
                UITrace( "[CA Cmd] " + cmdLineException.Message + " " + cmdLineException.StackTrace );
            }
            catch ( Exception generalException )
            {
                error = CAPlugin.KErrCommandLineGeneral;
                //
                UITrace( "[CA Cmd] " + generalException.Message + " " + generalException.StackTrace );
            }
            
            UITrace( "[CA Cmd] - operation complete: " + error );
            return error;
        }
        #endregion

		#region Properties
        public string[] CommandLineArguments
        {
            get { return iEngine.UIEngine.CommandLineArguments; }
        }
 
        public CIEngine CrashItemEngine
        {
            get { return iEngine.CrashItemEngine; }
        }
        #endregion

        #region Event handlers
        private void DbgEngine_EntityPrimingStarted( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            UITrace( "[CA Cmd] Priming debug meta-data: " + aEntity.FullName );

            iProgressReporter.StepBegin("Priming debug meta-data: " + aEntity.FullName, aEntity.FullName, 100);
        }

        private void DbgEngine_EntityPrimingProgress( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( aContext != null )
            {
                if ( aContext.GetType() == typeof( int ) )
                {
                    int value = (int) aContext;
                    UITrace( "[CA Cmd] Priming debug meta-data progress: {0:d3}% {1}", value, aEntity.FullName );

                    // If reporting progress, then output something so the carbide extension is aware
                    // of what is going on in the background.
                    iProgressReporter.StepProgress(string.Empty, value, aEntity.FullName);
                }
            }
        }

        private void DbgEngine_EntityPrimingComplete( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            iProgressReporter.StepEnd(string.Empty, aEntity.FullName);
            UITrace( "[CA Cmd] Primed debug meta-data: " + aEntity.FullName );
        }

        private void CrashItemEngine_SourceObserver( CIEngine.TSourceEvent aEvent, CIEngineSource aSource, object aParameter )
        {
            string msg = string.Empty;
            //
            switch ( aEvent )
            {
            case CIEngine.TSourceEvent.EEventSourceStateChanged:
                if ( aSource.State == CIEngineSource.TState.EStateProcessing )
                {
                    iProgressReporter.StepBegin("Processing crash file: " + aSource.FileName, aSource.FileName, 100);
                }
                break;
            case CIEngine.TSourceEvent.EEventSourceReady:
                iProgressReporter.StepEnd(string.Empty, aSource.FileName);
                break;
            case CIEngine.TSourceEvent.EEventSourceProgress:
                if ( aParameter != null && aParameter is int )
                {
                    iProgressReporter.StepProgress(string.Empty, (int)aParameter, aSource.FileName);
                }
                break;
            default:
                break;
            }
        }
        #endregion

        #region Internal constants
        private const string KParamReport = "-REPORT";
        private const string KParamProgress = "-PROGRESS";
        private const string KParamProgressDetailed = "-PROGRESS_DETAILS";
        private const string KPluginCommandLineName = "CRASH_ANALYSIS";
        private const string KPluginInputParameter = "-INPUT";
        private const string KPluginInputFileDocumentRootNode = "crash_analysis";

        // Step keys for progress reporting
        private const string KStepKeyReadingInputXml = "READING_INPUT_XML";
        private const string KStepKeyPrimingSources = "PRIMING_SOURCES";
        private const string KStepKeyWritingOutputXml = "WRITING_OUTPUT_XML";
        private const string KStepKeyCategorizingInputFiles = "CATEGORIZING_INPUT_FILES";
        #endregion

        #region Internal methods
        private string ExtractCommandLineInputParameter( string[] aArgs )
        {
            string ret = string.Empty;

            // -nogui -plugin CRASH_ANALYSIS -input d:\ca_fullsummary.xml
            for( int i=0; i<aArgs.Length; i++ )
            {
                string cmd = aArgs[ i ].Trim().ToUpper();
                string nextArg = ( i < aArgs.Length - 1 ? aArgs[ i + 1 ].Trim().ToUpper() : string.Empty );
                //
                if ( cmd == KPluginInputParameter && nextArg != string.Empty )
                {
                    ret = nextArg;
                }
            }

            return ret;
        }

        private bool CheckForReportParameter( string[] aArgs )
        {
            bool report = false;
            //
            string[] args = aArgs;
            for ( int i = 0; i < args.Length; i++ )
            {
                string cmd = args[ i ].Trim().ToUpper();
                string nextArg = ( i < args.Length - 1 ? args[ i + 1 ].Trim().ToUpper() : string.Empty );
                //
                try
                {
                    if ( cmd == KParamReport )
                    {
                        UITrace( "[CA Cmd] XML report requested." );
                        report = true;
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return report;
        }

        private bool CheckForProgressParameter( string[] aArgs )
        {
            bool report = false;
            //
            string[] args = aArgs;
            for ( int i = 0; i < args.Length; i++ )
            {
                string cmd = args[ i ].Trim().ToUpper();
                string nextArg = ( i < args.Length - 1 ? args[ i + 1 ].Trim().ToUpper() : string.Empty );
                //
                try
                {
                    if ( cmd == KParamProgress )
                    {
                        UITrace( "[CA Cmd] progress requested." );
                        report = true;
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return report;
        }

        private bool CheckForProgressDetailedParameter(string[] aArgs)
        {
            bool report = false;
            //
            string[] args = aArgs;
            for (int i = 0; i < args.Length; i++)
            {
                string cmd = args[i].Trim().ToUpper();
                string nextArg = (i < args.Length - 1 ? args[i + 1].Trim().ToUpper() : string.Empty);
                //
                try
                {
                    if (cmd == KParamProgressDetailed)
                    {
                        UITrace("[CA Cmd] detailed progress requested.");
                        report = true;
                        break;
                    }
                }
                catch (Exception)
                {
                }
            }
            //
            return report;
        }

        private CISink FindXmlSink()
        {
            UITrace( "[CA Cmd] FindXmlSink() - START" );
            CISink ret = null;
            //
            CISinkManager sinkManager = iEngine.CrashItemEngine.SinkManager;
            foreach ( CISink sink in sinkManager )
            {
                UITrace( "[CA Cmd] FindXmlSink() - found sink: " + sink.Name );

                if ( sink.Name.ToUpper().Contains( "CRASH XML" ) )
                {
                    ret = sink;
                    break;
                }
            }
            //
            UITrace( "[CA Cmd] FindXmlSink() - END - ret: " + ret );
            return ret;
        }

        private void ReadInputFile( string aFileName )
        {
            UITrace( "[CA Cmd] ReadInputFile() - START - aFileName: " + aFileName );

            iProgressReporter.StepBegin( "Reading XML input file...", KStepKeyReadingInputXml );
            iInputs.Read( aFileName );

            iProgressReporter.PrintProgress( string.Format( "{0} META DATA FILES", iInputs.MetaDataFiles.Count ), 1 );
            iProgressReporter.PrintProgress( string.Format( "{0} CRASH FILES", iInputs.SourceFiles.Count ), 1 );
            
            iProgressReporter.StepEnd( "Read XML input file.", KStepKeyReadingInputXml );

            UITrace( "[CA Cmd] ReadInputFile() - END - read okay: " + aFileName );
        }

        private void CalculateNumberOfOperationSteps()
        {
            // We start at one, since reading the input file is the first step
            int totalStepCount = 1;
            
            // 2) Prime sources
            ++totalStepCount;

            // 3) Cross reference/associate sources with inputs
            ++totalStepCount;
            
            // 4) Debug meta data priming - one step for each debug meta data entity.
            if ( iInputs.SinkParameters.DetailLevel != CISinkSerializationParameters.TDetailLevel.ESummary )
            {
                totalStepCount += iInputs.MetaDataFiles.Count;
            }

            // 5) Reading source files - one step for each file
            totalStepCount += iInputs.SourceFiles.Count;

            // 6) Outputting XML - treated as one entire step, with sub-step progress reporting.
            // We can't do any better than this because the number of XML files depends on the number
            // of crash containers that are created when the files are read (step 5).
            ++totalStepCount;

            // All future operations will include an accurate step number prefix
            iProgressReporter.TotalNumberOfSteps = totalStepCount;
        }

        public void UITrace( string aMessage )
        {
            iEngine.UIManager.UITrace( aMessage );
        }

        public void UITrace( string aFormat, params object[] aParams )
        {
            string msg = string.Format( aFormat, aParams );
            UITrace( msg );
        }

        private void TryToPrimeSources()
        {
            UITrace( "[CA Cmd] TryToPrimeSources() - START" );
            CrashItemEngine.ClearAll();
            
            // Prime engine with source files
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;
            int count = sourceFileNames.Count;

            // Emit progress banner
            iProgressReporter.StepBegin( "Locating crash files...", KStepKeyPrimingSources, count );
            skippedFiles.Clear();
            for ( int i = 0; i < count; i++ )
            {
                CACmdLineFileSource file = sourceFileNames[ i ];
                //
                try
                {
                    // We prime each file individually. If an exception is thrown then we
                    // record an appropriate error in the associated file object.
                    UITrace( "[CA Cmd] TryToPrimeSources() - priming: " + file );

                    bool primeSuccess = CrashItemEngine.Prime( file );
                    if ( primeSuccess == false )
                    {
                        skippedFiles.Add(file.Name);
                    }

 
                    UITrace( "[CA Cmd] TryToPrimeSources() - primed result: " + primeSuccess );
                }
                catch ( Exception sourcePrimerException )
                {
                    file.AddError( "Error Identifying Source Type", "There was an error when attempting to identify the source file type. The file could not be processed." );
                    file.AddDiagnostic( "Crash Primer Exception Message", sourcePrimerException.Message );
                    file.AddDiagnostic( "Crash Primer Exception Stack", sourcePrimerException.StackTrace );
                }

                // Report progress as we work through the sources
                iProgressReporter.StepProgress(string.Empty, i, KStepKeyPrimingSources);
            }

            iProgressReporter.StepEnd( string.Empty, KStepKeyPrimingSources );
            
            UITrace( "[CA Cmd] TryToPrimeSources() - END" );
        }

        private void TryToPrimeDbgEngine()
        {
            DbgEngine debugEngine = iEngine.DebugEngine;
            //
            Exception primerException = null;
            CACmdLineFSEntityList<CACmdLineFSEntity> metaDataFiles = iInputs.MetaDataFiles;
            //
            try
            {
                debugEngine.Clear();

                foreach ( CACmdLineFSEntity entry in metaDataFiles )
                {
                    UITrace( "[CA Cmd] Seeding debug meta engine with entry: " + entry.Name );
                    DbgEntity entity = debugEngine.Add( entry.Name );
                    if ( entity != null )
                    {
                        UITrace( "[CA Cmd] Entry type detected as: [" + entity.CategoryName + "]" );
                        entity.Tag = entry;
                    }
                    else
                    {
                        UITrace( "[CA Cmd] Entry not handled: " + entry.Name );
                        entry.AddError( "Meta-Data File Not Supported", "The file \'" + entry.Name + "\' is of unknown origin." );
                    }
                }

                // Listen to prime events
                try
                {
                    UITrace( "[CA Cmd] Starting prime operation... " );
                    debugEngine.EntityPrimingStarted += new DbgEngine.EventHandler( DbgEngine_EntityPrimingStarted );
                    debugEngine.EntityPrimingProgress += new DbgEngine.EventHandler( DbgEngine_EntityPrimingProgress );
                    debugEngine.EntityPrimingComplete += new DbgEngine.EventHandler( DbgEngine_EntityPrimingComplete );
                    debugEngine.Prime( TSynchronicity.ESynchronous );
                    UITrace( "[CA Cmd] Debug meta data priming completed successfully." );
                }
                finally
                {
                    debugEngine.EntityPrimingStarted -= new DbgEngine.EventHandler( DbgEngine_EntityPrimingStarted );
                    debugEngine.EntityPrimingProgress -= new DbgEngine.EventHandler( DbgEngine_EntityPrimingProgress );
                    debugEngine.EntityPrimingComplete -= new DbgEngine.EventHandler( DbgEngine_EntityPrimingComplete );
                }
            }
            catch ( Exception exception )
            {
                UITrace( "[CA Cmd] Debug meta data priming exception: " + exception.Message + ", " + exception.StackTrace );
                primerException = exception;
            }

            // Go through each debug entity and check it for errors. Add diagnostics
            // and error messages where appropriate.
            foreach ( DbgEntity entity in debugEngine )
            {
                string name = entity.FullName;
                //
                CACmdLineFSEntity file = metaDataFiles[ name ];
                file.Clear();
                //
                if ( entity.PrimerResult.PrimedOkay )
                {
                    if ( !entity.Exists )
                    {
                        file.AddError( "Meta-Data File Missing", string.Format( "The file \'{0}\' could not be found.", file.Name ) );
                    }
                    else if ( entity.IsUnsupported )
                    {
                        file.AddError( "Meta-Data File Not Supported", string.Format( "The file \'{0}\' is of unknown origin.", file.Name ) );
                    }
                }
                else
                {
                    // Add error
                    file.AddError( "Meta-Data Read Error", entity.PrimerResult.PrimeErrorMessage );

                    // And diagnostic information
                    Exception exception = entity.PrimerResult.PrimeException != null ? entity.PrimerResult.PrimeException : primerException;
                    if ( exception != null )
                    {
                        file.AddDiagnostic( "Meta-Data Exception Message", entity.PrimerResult.PrimeException.Message );
                        file.AddDiagnostic( "Meta-Data Exception Stack", entity.PrimerResult.PrimeException.StackTrace );
                    }
                    else
                    {
                        file.AddDiagnostic( "Meta-Data Unknown Failure", "No exception occurred at the primer or entity level?" );
                    }
                }
            }
        }

        private void TryToIdentifyCrashes()
        {
            Exception crashEngineException = null;
            //
            try
            {
                iEngine.CrashItemEngine.SourceObservers += new CIEngine.CIEngineSourceObserver( CrashItemEngine_SourceObserver );
                iEngine.IdentifyCrashes( TSynchronicity.ESynchronous );
            }
            catch ( Exception exception )
            {
                crashEngineException = exception;
            }
            finally
            {
                iEngine.CrashItemEngine.SourceObservers -= new CIEngine.CIEngineSourceObserver( CrashItemEngine_SourceObserver );
            }

            // Check each source in the engine and create messages based upon it's
            // state at the end of processing.
            foreach ( CACmdLineFileSource file in iInputs.SourceFiles )
            {
                if ( file.Source != null )
                {
                    CIEngineSource source = file.Source;
                    switch ( source.State )
                    {
                    case CIEngineSource.TState.EStateReady:
                        // Success case - the source resulted in the creation of at least one container
                        file.AddDiagnostic( "Source Read Successfully", string.Format( "{0} crash container(s) created", source.ContainerCount ) );
                        break;
                    case CIEngineSource.TState.EStateReadyNoItems:
                        file.AddWarning( "Source File Contains No Crashes", "The input data was read successfully but contains no crash information." );
                        break;
                    case CIEngineSource.TState.EStateReadyCorrupt:
                        file.AddError( "Source File is Corrupt", "The input data is invalid or corrupt." );
                        break;
                    case CIEngineSource.TState.EStateUninitialised:
                        file.AddError( "Source File not Read", "The input data was never read." );
                        file.AddDiagnostic( "Source State Invalid", "Source is still in unitialised state, even though reading is complete?" );
                        break;
                    case CIEngineSource.TState.EStateProcessing:
                        file.AddDiagnostic( "Source State Invalid", "Source is still in processing state, even though reading is complete?" );
                        break;
                    default:
                        break;
                    }
                }
                else
                {
                    file.AddError( "File is Not Supported", "There file type is not recognized and was not processed." );
                }

                // Add in details of any exception
                if ( crashEngineException != null )
                {
                    file.AddDiagnostic( "Crash Identification Exception Message", crashEngineException.Message );
                    file.AddDiagnostic( "Crash Identification Exception Stack", crashEngineException.StackTrace );
                }
            }
        }

        private void TryToCreateXmlOutput()
        {
            CACmdLineFSEntityList<CACmdLineFileSource> inputFiles = iInputs.SourceFiles;
            //
            CISink xmlSink = FindXmlSink();
            if ( xmlSink == null )
            {
                throw new CACmdLineException( "XML Output Plugin Not Available", CAPlugin.KErrCommandLinePluginSinkNotAvailable );
            }
  
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;
            int count = sourceFileNames.Count;

            // Emit progress banner
            iProgressReporter.StepBegin( "Creating crash XML content...", KStepKeyWritingOutputXml, count );
            
            for ( int i = 0; i < count; i++ )
            {
                 CACmdLineFileSource file = sourceFileNames[ i ];

                // If the file has a corresponding source then we know that crash item recognised it.
                if ( file.Source == null )
                {
                    // File is not supported by crash item engine. Create dummy container which we'll
                    // serialize below.
                    CACmdLineSource cmdLineSource = new CACmdLineSource( file.File );
                    CIContainer failedContainer = CIContainer.NewErrorContainer( CrashItemEngine, cmdLineSource );
                    file.Add( failedContainer );
                }

                // We copy and remove all the file-level messages. These will be added to the container
                // (where appropriate) or then to an output entry otherwise.
                CACmdLineMessage[] fileMessages = file.ToArray();
                file.Clear();

                // At this point, the input file is guaranteed to have associated containers. Either
                // valid ones (created by crash item engine) or a single 'FAILED' one which we just 
                // added above.
                foreach ( CIContainer container in file.Containers )
                {
                    // Firstly, add any meta-data errors/messages/warnings to this container
                    // as crash item message entries
                    AddMetaDataMessagesToContainer( container );

                    // Now we can try to serialize the container to XML. This method will
                    // not throw an exception.
                    //
                    // If the operation succeeds, then the input file will have an associated
                    // container object (and associated xml output file name) and we need not
                    // do anymore.
                    //
                    // If it fails, then the input file will not be assigned the container
                    // object and therefore, later on, we'll invoke the XML Sink directly to 
                    // create a stub 'FAILED' XML output file.
                    TryToCreateXmlOutput( xmlSink, container, file, fileMessages );
                }

                // Report progress as we work through the sources
                iProgressReporter.StepProgress(string.Empty, i, KStepKeyWritingOutputXml);
            }

            iProgressReporter.StepEnd( string.Empty, KStepKeyWritingOutputXml );
        }

        private void TryToCreateXmlOutput( CISink aXmlSink, CIContainer aContainer, CACmdLineFileSource aFile, CACmdLineMessage[] aMessagesToAdd )
        {
            UITrace( "[CA Cmd] TryToCreateXmlOutput() - START - container source: {0}", aContainer.Source.MasterFileName );

            // By the time we are outputting a container, there should no longer be any messages
            // associated with the file.
            System.Diagnostics.Debug.Assert( aFile.Count == 0 );

            // Check whether the file contained any errors or 
            // messages of it own.
            if ( aMessagesToAdd.Length > 0 )
            {
                // Copy warnings, messages and errors into crash item container.
                // Diagnostic messages are not copied.
                CACmdLineFSEntity.CopyMessagesToContainer( aMessagesToAdd, aContainer );
            }

            // This is where we will record the output attempt
            CACmdLineFileSource.OutputEntry outputEntry = null;
            //
            try
            {
                // Finish preparing the sink parameters
                CISinkSerializationParameters sinkParams = iInputs.SinkParameters;
                sinkParams.Container = aContainer;

                // Perform serialization
                UITrace( "[CA Cmd] TryToCreateXmlOutput() - serializing..." );
                object output = aXmlSink.Serialize( sinkParams );
                UITrace( "[CA Cmd] TryToCreateXmlOutput() - serialization returned: " + output );

                if ( aFile != null )
                {
                    // Create new output
                    string outputFileName = output is string ? (string) output : string.Empty;

                    // Save output file name
                    outputEntry = aFile.AddOutput( aContainer, outputFileName, TOutputStatus.ESuccess );
                }

                // Merge in any diagnostic messages that were left into the output entry.
                // This ensure we output diagnostics in the final manifest data.
                outputEntry.AddRange( aMessagesToAdd, CACmdLineMessage.TType.ETypeDiagnostic );
            }
            catch ( Exception outputException )
            {
                UITrace( "[CA Cmd] TryToCreateXmlOutput() - outputException.Message:    " + outputException.Message );
                UITrace( "[CA Cmd] TryToCreateXmlOutput() - outputException.StackTrace: " + outputException.StackTrace );

                if ( aFile != null )
                {
                    // Something went wrong with XML serialisation for the specified container.
                    outputEntry = aFile.AddOutput( aContainer, string.Empty, TOutputStatus.EFailed );
                    //
                    outputEntry.AddError( "Could not Create XML", "XML output could not be created" );
                    outputEntry.AddDiagnostic( "XML Sink Exception Message", outputException.Message );
                    outputEntry.AddDiagnostic( "XML Sink Exception Stack", outputException.StackTrace );
                    
                    // Since we didn't manage to sink the container to XML successfully, we must
                    // make sure we don't lose any associated messages from the original file. 
                    // Merge these into the output entry also.
                    outputEntry.AddRange( aMessagesToAdd );
                }
            }
        }

        private void CreateAndEmitXmlReport()
        {
            CACmdLineManifestWriter writer = new CACmdLineManifestWriter( iInputs.SourceFiles );
            string xml = writer.Create();
            //
            using( StringReader reader = new StringReader( xml ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    System.Console.WriteLine( line );
                    line = reader.ReadLine();
                }
            }
        }

        private void AssociateInputFilesWithCrashItemSources()
        {
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;
            CIEngineSourceCollection sources = iEngine.CrashItemEngine.Sources;
            int count = sources.Count;

            // Emit progress banner
            iProgressReporter.StepBegin( "Categorizing files...", KStepKeyCategorizingInputFiles, count );
            
            // Check each source in the engine and try to map it back onto an input source
            // file name. The goal is to identify input files which have no corresponding crash engine
            // source. These files are unsupported.
            for( int i=0; i<count; i++ )
            {
                CIEngineSource source = sources[ i ];
                string sourceFileName = source.FileName;
                
                // Try to match an input file with a given source object
                CACmdLineFileSource inputFile = sourceFileNames[ sourceFileName ];
                if ( inputFile != null )
                {
                    inputFile.Source = source;
                }

                // Report progress as we work through the sources
                iProgressReporter.StepProgress( string.Empty, i, KStepKeyCategorizingInputFiles );
            }

            iProgressReporter.StepEnd( string.Empty, KStepKeyCategorizingInputFiles );
        }

        private void PrintOutSkippepFiles()
        {
            if ( skippedFiles.Count > 0 )
            {
                foreach ( string skippedFile in skippedFiles )
                {
                    iProgressReporter.StepBegin( "Skipped non-crash file: " + skippedFile, skippedFile, 100 );
                    iProgressReporter.StepEnd( "", skippedFile );
                }
            }
        }

        private void AddMetaDataMessagesToContainer( CIContainer aContainer )
        {
            // All meta-data errors, warnings & messages are added as 
            // children of the container.
            CACmdLineFSEntityList<CACmdLineFSEntity> metaDataFiles = iInputs.MetaDataFiles;
            foreach ( CACmdLineFSEntity file in metaDataFiles )
            {
                file.CopyMessagesToContainer( aContainer );
            }
        }
        #endregion

        #region Data members
        private readonly CAPluginCrashAnalysis iEngine;
        private readonly CACmdLineInputParameters iInputs;
        private CACmdLineProgressReporter iProgressReporter = new CACmdLineProgressReporter();
        List<string> skippedFiles = new List<string>();
        #endregion
	}
}
