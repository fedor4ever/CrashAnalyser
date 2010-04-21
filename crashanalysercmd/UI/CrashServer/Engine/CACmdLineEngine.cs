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
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Summarisable;
using CrashItemLib.Sink;
using CrashItemLib.Engine;
using CrashItemLib.Engine.Sources;
using CrashItemLib.Engine.Interfaces;
using CrashItemLib.PluginAPI;
using SymbianUtils.FileSystem.Utilities;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Elements.Types.Category;
using SymbianXmlInputLib.Elements.Types.FileSystem;
using SymbianXmlInputLib.Elements.Types.Command;
using SymbianDebugLib;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianUtils;
using SymbianUtils.Tracer;
using CrashItemLib.Crash.InfoSW;
using MobileCrashLib;
using MobileCrashLib.Parser;
using MobileCrashLib.Structures.Items;
using MobileCrashLib.Structures;

namespace CrashAnalyserServerExe.Engine
{
    internal class CACmdLineEngine : DisposableObject, ITracer, ICIEngineUI
	{
		#region Constructors
        public CACmdLineEngine( DbgEngine aDebugEngine )
		{
            iDebugEngine = aDebugEngine;
            iInputs = new CACmdLineInputParameters( aDebugEngine );
            iCrashItemEngine = new CIEngine( aDebugEngine, this as ICIEngineUI );
		}
		#endregion

        #region API
        public int RunCommandLineOperations()
        {
            Trace( "[CA Cmd] START " );
            Trace( string.Empty );
            Trace( "[CA Cmd] command line: " + System.Environment.CommandLine );
            Trace( "[CA Cmd] command wd:   " + System.Environment.CurrentDirectory );
            Trace( "[CA Cmd] proc count:   " + System.Environment.ProcessorCount );
            Trace( "[CA Cmd] sysdir:       " + System.Environment.SystemDirectory );
            Trace( "[CA Cmd] version:      " + System.Environment.Version.ToString() );
            Trace( string.Empty );

            int error = CACmdLineException.KErrNone;
                        
            //
            try
            {
                if (!iInputs.ParseCommandLine())
                {
                    throw new CACmdLineException("Error while parsing command line", CACmdLineException.KErrCommandLineError);
                }
                
                // We expect to see an "-input" parameter
                iReportProgress = CheckForProgressParameter();

                // Switch off UI output at the debug engine level
                iDebugEngine.UiMode = TDbgUiMode.EUiDisabled;

                // Next, attempt to prime the crash engine with every source we
                // identified from the input specification. The goal is to
                // create all the needed Crash Item Source objects for each input file.
                // Any inputs which we don't support will not have an associated source and
                // will be flagged accordingly within the CACmdLineFileSource object.
                TryToPrimeSources();

                // Try to the ROM ID and file type (i.e. crash, 
                // registration msg, report) out of the crash files. This information is
                // used in later phases (e.g. if there are only registration messages in 
                // source files then we do not have to load symbol files).
                TryToGetCrashInformation();

                // Next, prime the debug engine will all the debug meta-data inputs.
                // Again, individual error messages will be associated with each meta-data
                // input.

              //  if (SymbolFilesNeeded())
                {
                    TryToPrimeDbgEngine();
                }

                // Next, we invoke the crash engine to process all the crash item sources we
                // created during the prime step. Exceptions are caught and associated 
                // messages & diagnostics are created at the input-file level.
                TryToIdentifyCrashes();

                // Next, we start the output phase. Any 'valid' crash containers are serialized
                // to xml. Any input files which could not be processed have 'dummy' containers
                // created for them, and these are also serialised to 'failed' CI files.
                // If the CI Sink plugin is unavailable, then we cannot create any CI output.
                // In this situation, we throw an exception which is caught below.
                TryToCreateOutput();
            }
            catch ( CACmdLineException cmdLineException )
            {
                error = cmdLineException.ErrorCode;
                //
                Trace( "[CA Cmd] " + cmdLineException.Message + " " + cmdLineException.StackTrace );
            }
            catch ( Exception generalException )
            {
                error = CACmdLineException.KErrGeneral;
                //
                Trace( "[CA Cmd] " + generalException.Message + " " + generalException.StackTrace );
            }
            
            Trace( "[CA Cmd] - operation complete: " + error );
            return error;
        }

        #endregion

		#region Properties
        public string CommandLineArguments
        {
            get { return Environment.CommandLine; }
        }
 
        public CIEngine CrashItemEngine
        {
            get { return iCrashItemEngine; }
        }
        #endregion

        #region Event handlers
        private void DbgEngine_EntityPrimingStarted( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            Trace( "[CA Cmd] Priming debug meta-data: " + aEntity.FullName );

            // Emit progress banner
            if ( iReportProgress )
            {
                Print( "Reading debug meta-data..." );
            }
        }

        private void DbgEngine_EntityPrimingProgress( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( aContext != null )
            {
                if ( aContext.GetType() == typeof( int ) )
                {
                    int value = (int) aContext;
                    Trace( "[CA Cmd] Priming debug meta-data progress: {0:d3}% {1}", value, aEntity.FullName );

                    // If reporting progress, then output something so the carbide extension is aware
                    // of what is going on in the background.
                    if ( iReportProgress )
                    {
                        string msg = string.Format( "{1:d3}%, {0}", aEntity.FullName, value );
                        Print( msg );
                    }
                }
            }
        }

        private void DbgEngine_EntityPrimingComplete( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            Trace( "[CA Cmd] Primed debug meta-data: " + aEntity.FullName );
        }

        private void CrashItemEngine_SourceObserver( CIEngine.TSourceEvent aEvent, CIEngineSource aSource, object aParameter )
        {
            if ( iReportProgress )
            {
                string msg = string.Empty;
                //
                switch ( aEvent )
                {
                case CIEngine.TSourceEvent.EEventSourceReady:
                    msg = string.Format( "Reading file: [{0}], progress: 100%", aSource.FileName );
                    break;
                case CIEngine.TSourceEvent.EEventSourceProgress:
                    if ( aParameter != null && aParameter is int )
                    {
                        msg = string.Format( "Reading file: [{0}], progress: {1:d3}%", aSource.FileName, (int) aParameter );
                    }
                    break;
                default:
                    break;
                }
                
                // Output a message only if we have one
                if ( string.IsNullOrEmpty( msg ) == false )
                {
                    Print( msg );
                }
            }
        }
        #endregion

        #region Internal constants
        private const string KCrashItemSinkName = "CRASH INFO FILE";
        private const string KXmlCrashItemSinkName = "XML CRASH FILE";
        private const string KParamProgress = "-PROGRESS";
        #endregion

        #region Internal methods
        private bool CheckForProgressParameter()
        {
            bool report = this.CommandLineArguments.Contains( KParamProgress );
            return report;
        }

        private CISink FindSink(bool aUseXmlSink)
        {
            Trace( "[CA Cmd] FindSink() - START" );
            CISink ret = null;
            //
            string sinkToUse = KCrashItemSinkName;
            if (aUseXmlSink)
            {
                sinkToUse = KXmlCrashItemSinkName;
            }

            CISinkManager sinkManager = iCrashItemEngine.SinkManager;
            foreach ( CISink sink in sinkManager )
            {
                Trace( "[CA Cmd] FindSink() - found sink: " + sink.Name );

                if ( sink.Name.ToUpper().Contains( sinkToUse ) )
                {
                    ret = sink;
                    break;
                }
            }
            //
            Trace( "[CA Cmd] FindSink() - END - ret: " + ret );
            return ret;
        }

        private void TryToPrimeSources()
        {
            Trace( "[CA Cmd] TryToPrimeSources() - START" );
            CrashItemEngine.ClearAll();
            
            // Prime engine with source files
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;
            int progress = -1;
            int count = sourceFileNames.Count;

            // Emit progress banner
            if ( iReportProgress )
            {
                Print( "Locating crash files..." );
            }

            for ( int i = 0; i < count; i++ )
            {
                CACmdLineFileSource file = sourceFileNames[ i ];
                //
                try
                {
                    // We prime each file individually. If an exception is thrown then we
                    // record an appropriate error in the associated file object.
                    Trace( "[CA Cmd] TryToPrimeSources() - priming: " + file );

                    bool primeSuccess = CrashItemEngine.Prime( file );

                    // Report progress as we work through the sources
                    if ( iReportProgress )
                    {
                        float newProgress = ( ( (float) i + 1 ) / (float) count ) * 100.0f;
                        if ( (int) newProgress != progress || i == count - 1 )
                        {
                            progress = (int) newProgress;
                            Print( string.Format( "{0:d3}%", progress ) );
                        }
                    }

                    Trace( "[CA Cmd] TryToPrimeSources() - primed result: " + primeSuccess );
                }
                catch ( Exception sourcePrimerException )
                {
                    file.AddError( "Error Identifying Source Type", "There was an error when attempting to identify the source file type. The file could not be processed." );
                    file.AddDiagnostic( "Crash Primer Exception Message", sourcePrimerException.Message );
                    file.AddDiagnostic( "Crash Primer Exception Stack", sourcePrimerException.StackTrace );
                }
            }

            AssociateInputFilesWithCrashItemSources();
            Trace( "[CA Cmd] TryToPrimeSources() - END" );
        }

        /**
         * Get important data (i.e. RomId and content type)
         */
        private void TryToGetCrashInformation()
        {
            Trace("[CA Cmd] TryToGetCrashInformation() - START");

            foreach (CACmdLineFileSource file in iInputs.SourceFiles)
            {
                if (file.Source != null)
                {
                    byte[] bytes = File.ReadAllBytes(file.File.FullName);
                    MobileCrashBin bin = new MobileCrashBin(bytes);
                    MobileCrashData data = new MobileCrashData();
                    MobileCrashParser parser = new MobileCrashParser(bin, data);
                    parser.Parse(TSynchronicity.ESynchronous);
                    data = parser.MobileCrashData;
                    file.RomId = data.ItemById<MobileCrashItemUint32>(TMobileCrashId.EMobileCrashId_ROMID);
                    file.ContentType = data.ContentType;
                }
            }

            Trace("[CA Cmd] TryToGetCrashInformation() - END");

        }

        private void TryToPrimeDbgEngine()
        {
            DbgEngine debugEngine = iDebugEngine;
            //
            Exception primerException = null;
            CACmdLineFSEntityList<CACmdLineFSEntity> metaDataFiles = iInputs.MetaDataFiles;
            //
            try
            {
                debugEngine.Clear();

                foreach (CACmdLineFileSource file in iInputs.SourceFiles)
                {
                    if (file.RomId != null)
                        debugEngine.AddActiveRomId(file.RomId.Value);
                }

                foreach ( CACmdLineFSEntity entry in metaDataFiles )
                {
                    Trace( "[CA Cmd] Seeding debug meta engine with entry: " + entry.Name );
                    DbgEntity entity = debugEngine.Add( entry.Name );
                    if ( entity != null )
                    {
                        Trace( "[CA Cmd] Entry type detected as: [" + entity.CategoryName + "]" );
                        entity.Tag = entry;
                    }
                    else
                    {
                        Trace( "[CA Cmd] Entry not handled: " + entry.Name );
                        entry.AddError( "Meta-Data File Not Supported", "The file \'" + entry.Name + "\' is of unknown origin." );
                    }
                }

                // Listen to prime events
                try
                {
                    Trace( "[CA Cmd] Starting prime operation... " );
                    debugEngine.EntityPrimingStarted += new DbgEngine.EventHandler( DbgEngine_EntityPrimingStarted );
                    debugEngine.EntityPrimingProgress += new DbgEngine.EventHandler( DbgEngine_EntityPrimingProgress );
                    debugEngine.EntityPrimingComplete += new DbgEngine.EventHandler( DbgEngine_EntityPrimingComplete );
                    debugEngine.Prime( TSynchronicity.ESynchronous );
                    Trace( "[CA Cmd] Debug meta data priming completed successfully." );
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
                Trace( "[CA Cmd] Debug meta data priming exception: " + exception.Message + ", " + exception.StackTrace );
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
                iCrashItemEngine.SourceObservers += new CIEngine.CIEngineSourceObserver( CrashItemEngine_SourceObserver );
                iCrashItemEngine.IdentifyCrashes( TSynchronicity.ESynchronous );
            }
            catch ( Exception exception )
            {
                crashEngineException = exception;
            }
            finally
            {
                iCrashItemEngine.SourceObservers -= new CIEngine.CIEngineSourceObserver( CrashItemEngine_SourceObserver );
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

        private void TryToCreateOutput()
        {
            CACmdLineFSEntityList<CACmdLineFileSource> inputFiles = iInputs.SourceFiles;
            //
            CISink sink = FindSink(iInputs.UseXmlSink);
            if ( sink == null )
            {
                throw new CACmdLineException( "CI Output Plugin Not Available", CACmdLineException.KErrSinkNotAvailable );
            }
  
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;
            int progress = -1;
            int count = sourceFileNames.Count;

            // Emit progress banner
            if ( iReportProgress )
            {
                Print( "Creating CI content..." );
            }
            
            for ( int i = 0; i < count; i++ )
            {
                CACmdLineFileSource file = sourceFileNames[ i ];

                System.Console.WriteLine("Starting to process file " +file.Name);

                // If the file has a corresponding source then we know that crash item recognised it.
                // Otherwise, we skip it.
                if ( file.Source != null )
                {
                    // We copy and remove all the file-level messages. These will be added to the container
                    // (where appropriate) or then to an output entry otherwise.
                    CACmdLineMessage[] fileMessages = file.ToArray();
                    file.Clear();

                    // At this point, the input file is guaranteed to have an associated container. In
                    // the current mobile crash file format, there will be a 1:1 mapping, i.e. each file
                    // will contain a single crash report ("container") and therefore if symbols were
                    // found for just a single container (within the file) then it's good enough to treat
                    // the file as archivable.
                    foreach ( CIContainer container in file.Containers )
                    {
                        // Firstly, add any meta-data errors/messages/warnings to this container
                        // as crash item message entries
                        AddMetaDataMessagesToContainer( container );

                        SetArchiveFileName(file.Name, container, sink);

                        foreach (CIMessage message in container.Messages)
                        {
                            if (message.Type == CrashItemLib.Crash.Messages.CIMessage.TType.ETypeError)
                            {
                                container.Status = CIContainer.TStatus.EStatusErrorContainer;
                            }
                        }
                        // Now we can try to serialize the container to CI. This method will
                        // not throw an exception.
                        //
                        // If the operation succeeds, then the input file will have an associated
                        // container object (and associated xml output file name) and we need not
                        // do anymore.
                        //
                        // If it fails, then the input file will not be assigned the container
                        // object and therefore, later on, we'll invoke the CI Sink directly to 
                        // create a stub 'FAILED' CI output file.
                        //
                        // NB: If Symbols were not available for the specified container, then
                        // we don't create a CI file but instead callback to a helper that
                        // will move the file to another temporary location for later repeat
                        // processing
                        bool hasSymbols = ContainsSymbols( container );
                        
                        if (container.Status == CIContainer.TStatus.EStatusErrorContainer) //normally don't output containers with errors
                        {
                            file.State = CACmdLineFileSource.TState.EStateUninitialized;
                            if (iInputs.DecodeWithoutSymbols) //with force mode, output no matter what
                            {
                                TryToCreateOutput(sink, container, file, fileMessages);
                            }
                        }
                        else if (hasSymbols || IsSymbollessMobileCrash( container ))
                        {                            
                            file.State = CACmdLineFileSource.TState.EStateProcessedAndReadyToBeArchived;
                            TryToCreateOutput(sink, container, file, fileMessages);
                        }
                        else if (IsSymbollessMobileCrash(container)) //Crash api and registration files do not need symbols
                        {
                            
                            file.State = CACmdLineFileSource.TState.EStateProcessedAndReadyToBeArchived;
                            TryToCreateOutput(sink, container, file, fileMessages);
                        }
                        else 
                        {                            
                            file.State = CACmdLineFileSource.TState.EStateSkippedDueToMissingSymbols;
                            if (iInputs.DecodeWithoutSymbols) //with force mode, output no matter what
                            {
                                //remove this to prevent .corrupt_ci creation!
                                TryToCreateOutput(sink, container, file, fileMessages);
                            }
                        }
                    }
                }
                else
                {
                    file.State = CACmdLineFileSource.TState.EStateSkippedDueToNotBeingRecognized;
                }

                // Move file to final location
                MoveProcessedFile( file );

                // Report progress as we work through the sources
                if ( iReportProgress )
                {
                    float newProgress = ( ( (float) i + 1 ) / (float) count ) * 100.0f;
                    if ( (int) newProgress != progress || i == count - 1 )
                    {
                        progress = (int) newProgress;
                        Print( string.Format( "{0:d3}%", progress ) );
                    }
                }
            }
        }

       
        private void SetArchiveFileName(string aFileFullPath, CIContainer aContainer, CISink sink)
        {
            string fileName = Path.GetFileName(aFileFullPath);

            //add romid to filename if not already there
            CIInfoSW info = (CIInfoSW) aContainer.ChildByType( typeof( CIInfoSW ) );
            if (info != null)
            {
                //RomID
                if (info.ImageCheckSum != 0)
                {
                    string romid = info.ImageCheckSum.ToString("x8");

                    if (fileName.Length < 8 || fileName.Substring(0, 8) != romid)
                    {
                        fileName = romid + "_" + fileName;
                        
                    }
                }
            }
            
            
            string basename = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(aFileFullPath);
            
            string archiveNamePath = Path.Combine(iInputs.ArchivePath, fileName); //Path.GetFileName(iInputs.ArchivePath));
            int counter = 1;               
            while (File.Exists(archiveNamePath))
            {
                archiveNamePath = Path.Combine(iInputs.ArchivePath, basename + "_" + counter.ToString() + extension);
                counter++;
            }

            iInputs.SinkParameters.OperationData1 = (Object)archiveNamePath;
            iBinFileArchivePathName = archiveNamePath;
           
        }

       
        private void TryToCreateOutput( CISink aXmlSink, CIContainer aContainer, CACmdLineFileSource aFile, CACmdLineMessage[] aMessagesToAdd )
        {
            Trace( "[CA Cmd] TryToCreateOutput() - START - container source: {0}", aContainer.Source.MasterFileName );

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
                Trace( "[CA Cmd] TryToCreateOutput() - serializing..." );
                object output = aXmlSink.Serialize( sinkParams );
                Trace( "[CA Cmd] TryToCreateOutput() - serialization returned: " + output );

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
                Trace( "[CA Cmd] TryToCreateOutput() - outputException.Message:    " + outputException.Message );
                Trace( "[CA Cmd] TryToCreateOutput() - outputException.StackTrace: " + outputException.StackTrace );

                if ( aFile != null )
                {
                    // Something went wrong with CI serialisation for the specified container.
                    outputEntry = aFile.AddOutput( aContainer, string.Empty, TOutputStatus.EFailed );
                    //
                    outputEntry.AddError( "Could not Create CI", "CI output could not be created" );
                    outputEntry.AddDiagnostic( "CI Sink Exception Message", outputException.Message );
                    outputEntry.AddDiagnostic( "CI Sink Exception Stack", outputException.StackTrace );
                    
                    // Since we didn't manage to sink the container to CI successfully, we must
                    // make sure we don't lose any associated messages from the original file. 
                    // Merge these into the output entry also.
                    outputEntry.AddRange( aMessagesToAdd );
                }
            }
        }

        private void AssociateInputFilesWithCrashItemSources()
        {
            CACmdLineFSEntityList<CACmdLineFileSource> sourceFileNames = iInputs.SourceFiles;

            // Emit progress banner
            if ( iReportProgress )
            {
                Print( "Categorizing files..." );
            }
            
            // Check each source in the engine and try to map it back onto an input source
            // file name. The goal is to identify input files which have no corresponding crash engine
            // source. These files are unsupported.
            CIEngineSourceCollection sources = iCrashItemEngine.Sources;
            int count = sources.Count;
            int progress = -1;
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
                if ( iReportProgress )
                {
                    float newProgress = ( ( (float) i+1 ) / (float) count ) * 100.0f;
                    if ( (int) newProgress != progress || i == count - 1 )
                    {
                        progress = (int) newProgress;
                        Print( string.Format( "{0:d3}%", progress ) );
                    }
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


        private bool ContainsSymbols( CIContainer aContainer )
        {
            bool retval = false;
            if (aContainer.FileNames.Length > 1)
            {
                retval = true;
            }
            return retval;

        }

        private bool IsSymbollessMobileCrash(CIContainer aContainer)
        {
            bool retval = false;


            foreach (CIMessage message in aContainer.Messages)
            {
                if (message.Title == "MobileCrash content type")
                {
                    /* We could also decode registrations without but that will cause server to have
                     * lots of registration only sw versions
                    if (message.Description.Trim() == "registration")
                    {
                        retval = true;
                    }
                    else if (message.Description.Trim() == "alive")
                    {
                        retval = true;
                    }
                    else */

                    if (message.Description.Trim() == "report")
                    {
                        retval = true;
                    }
                }
            }


            return retval;
        }


        /*private bool ContainsSymbols( CIContainer aContainer )
        {
            // Symbols can be registered as the global level, or then per-process.
            bool ret = ( aContainer.SymbolDictionary.Count > 0 );            
            if ( ret == false )
            {
                // Check at process level
                CISummarisableEntityList summaries = aContainer.Summaries;
                foreach ( CISummarisableEntity entity in summaries.ChildrenByType<CISummarisableEntity>() )
                {
                    CIProcess process = entity.Process;
                    if ( process != null )
                    {
                        ret = ( process.SymbolDictionary.Count > 0 );
                        if ( ret == true )
                        {
                            break;
                        }
                    }
                }
            }
            //
            return ret;
        }*/

        private void MoveProcessedFile( CACmdLineFileSource aFile )
        {
            string skippedTarget = Path.Combine(iInputs.SkippedPath, Path.GetFileName(iBinFileArchivePathName));
            string errorTarget = Path.Combine(iInputs.ErrorPath, Path.GetFileName(iBinFileArchivePathName));

            switch ( aFile.State )
            {
            case CACmdLineFileSource.TState.EStateProcessedAndReadyToBeArchived:
                MoveFile(aFile, iBinFileArchivePathName);
                break;
            case CACmdLineFileSource.TState.EStateSkippedDueToMissingSymbols:
                MoveFile(aFile, skippedTarget);
                break;
            case CACmdLineFileSource.TState.EStateSkippedDueToNotBeingRecognized:                
                MoveFile(aFile, errorTarget);
                break;
            default:               
                MoveFile(aFile, errorTarget);
                break;
            }
        }

        private void MoveFile(CACmdLineFileSource aFile, string aTargetPath)
        {
            string newName = aTargetPath;             
            
            //Name availability has already been checked before starting decoding for archive location
            //If file is going to skipped, its name may need changing
            if (File.Exists(newName))
            {
                int counter = 1;
                string original_name = newName;
                while (File.Exists(newName))
                {
                    string basepath = Path.GetDirectoryName(original_name);
                    string basename = Path.GetFileNameWithoutExtension(original_name);
                    string extension = Path.GetExtension(original_name);
                   
                    newName = Path.Combine(basepath, basename + "_" + counter.ToString() + extension);
                    counter++;
                }
                
            } 

            // Move the file.
            System.Console.WriteLine("Moving file " + aFile.Name + " to " + newName);
            if (!iInputs.TestWithoutMovingFiles)
            {
                File.Move(aFile.Name, newName);
                if (!File.Exists(newName))
                {
                    System.Console.WriteLine("Error: unable to move file " +aFile.Name +" to " +newName );
                }
            
            }


        }

        private bool SymbolFilesNeeded()
        {
            foreach (CACmdLineFileSource file in iInputs.SourceFiles)
            {
                if (file.ContentType == TMobileCrashContentType.EContentTypeException ||
                    file.ContentType == TMobileCrashContentType.EContentTypePanic)
                    return true;
            }
            return false;
        }

        #endregion

        #region Output methods
        public void Print( string aMessage )
        {
            System.Console.WriteLine( aMessage );
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            //System.Console.WriteLine("MANUAL TRACE:" +aMessage);
            iDebugEngine.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string msg = string.Format( aFormat, aParams );
            Trace( msg );
        }
        #endregion

        #region From ICIEngineUI
        void ICIEngineUI.CITrace( string aMessage )
        {
            Trace( aMessage );
        }

        void ICIEngineUI.CITrace( string aFormat, params object[] aParameters )
        {
            Trace( aFormat, aParameters );
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                iCrashItemEngine.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iDebugEngine;
        private readonly CIEngine iCrashItemEngine;
        private readonly CACmdLineInputParameters iInputs;
        private bool iReportProgress = false;
        private string iBinFileArchivePathName = string.Empty;
        #endregion
    }
}
