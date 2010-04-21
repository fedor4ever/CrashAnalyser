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
using System.Reflection;
using System.Collections.Generic;
using SymbianUtils.Tracer;
using CrashItemLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using SymbianUtils.FileSystem.Utilities;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Elements.Types.Category;
using SymbianXmlInputLib.Elements.Types.Extension;
using SymbianXmlInputLib.Elements.Types.FileSystem;
using SymbianXmlInputLib.Elements.Types.Command;
using CrashItemLib.Sink;
using System.Text.RegularExpressions;
using System.Globalization;


namespace CrashAnalyserServerExe.Engine
{
	internal class CACmdLineInputParameters
	{
		#region Constructors
        public CACmdLineInputParameters( ITracer aTracer )
		{
            iTracer = aTracer;

            Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            iAppPath = Path.GetDirectoryName( myAssembly.Location );

            // CHECKME:
            // Source files are identified from command line current working directory.
            DirectoryInfo sourceDir = new DirectoryInfo( Environment.CurrentDirectory );
            iSources.AddRange( sourceDir.GetFiles( "*.bin", SearchOption.TopDirectoryOnly ) );

            // CHECKME:
            // Read selge.ini from tool directory or CWD
            FindDebugMetaDataFile( KDebugMetaDataFileSelgeIni );

            // CHECKME:
            // Read selge_event.ini from tool directory or CWD
            FindDebugMetaDataFile( KDebugMetaDataFileSelgeEventIni );

            // Sink parameters are fairly simple
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string commandLine = System.Environment.CommandLine;
            iSinkParams = new CISinkSerializationParameters( version, commandLine );
            iSinkParams.DetailLevel = CISinkSerializationParameters.TDetailLevel.EFull;
            iSinkParams.FileExtensionFailed = ".corrupt_ci";
            iSinkParams.FileExtensionSuccess = ".ci";

            // CHECKME:
            // The output data is written to the same directory as the input file.
            iSinkParams.OutputDirectory = sourceDir;
        }
        #endregion

        #region Command line parsing
        public bool ParseCommandLine()
        {
            bool retval = true;
            string commandLine = System.Environment.CommandLine;

            string ppattern = @"\s*(-.\s*[^-]*)";
            MatchCollection matches = Regex.Matches(commandLine, ppattern, RegexOptions.None);
            if (matches.Count < 1)
            {
                PrintCommandHelp();
                System.Console.WriteLine("Error: No valid parameters given");                
                throw new CACmdLineException("Error: No valid parameters given", CACmdLineException.KErrCommandLineError);
            }
            foreach (Match parameter in matches)
            {
                Regex pparser = new Regex(@"(?<id>-.)\s*(?<content>.*)");

                Match m = pparser.Match(parameter.ToString());
                if (m.Success)
                {
                    GroupCollection groups = m.Groups;
                    string paramId = m.Result("${id}").Trim();
                    string paramContent = m.Result("${content}").Trim();

                    if (paramId == "-a")
                    {
                        ArchivePath = paramContent;
                    }
                    if (paramId == "-s")
                    {
                        SkippedPath = paramContent;
                        ErrorPath = paramContent + @"\errors";
                    }
                    if (paramId == "-f")
                    {
                        DecodeWithoutSymbols = true;
                    }
                    if (paramId == "-t")
                    {
                        TestWithoutMovingFiles = true;
                    }
                    if (paramId == "-x")
                    {
                        UseXmlSink = true;
                    }

                    if (paramId == "-c")
                    {
                        CITargetPath = paramContent;
                        System.Console.WriteLine("CITargetPath -c is " +CITargetPath);
                    }
                    
                    // Plain text output
                    if (paramId == "-p")
                    {
                        UseXmlSink = true; // XML sink is used for plain text output
                        iSinkParams.PlainTextOutput = true;
                    }
                    
                    // Crash files
                    if (paramId == "-b")
                    {
                        FileInfo fi = new FileInfo(paramContent);
                        CACmdLineFSEntityList<CACmdLineFileSource> fileList = new CACmdLineFSEntityList<CACmdLineFileSource>();

                        if (fi.Exists)
                        {
                            fileList.Add(fi);
                        }
                        else
                        {
                            System.Console.WriteLine("Error: Crash file " + fi.FullName + " does not exist");
                            retval = false;
                        }
                        iSources = fileList;
                    }
                    
                    // Symbol/map/dictionary files
                    if (paramId == "-m")
                    {
                        string[] symbolFileTable = paramContent.Split(',');
                        CACmdLineFSEntityList<CACmdLineFSEntity> fileList = new CACmdLineFSEntityList<CACmdLineFSEntity>();
                        foreach (string fileName in symbolFileTable)
                        {
                            FileInfo fi = new FileInfo(fileName);
                            if(fi.Exists)
                            {
                                fileList.Add(fi);
                            }
                        }
                        
                        iMetaData = fileList;

                        if (fileList.Count == 0)
                        {
                            System.Console.WriteLine("Error: Invalid symbol/map/dictionary files: " + paramContent);
                            retval = false;
                        }
                    }
                    
                }
                else
                {
                    System.Console.WriteLine("Error: No parameters found");
                    retval = false;
                }          

            }

            //Parameter scanning finished - validate content
            if (ArchivePath == string.Empty)
            {
                System.Console.WriteLine("Error: No archive path given");
                retval = false;
            }
            else if (!Directory.Exists(ArchivePath))
            {
                System.Console.WriteLine("Error: Archive path " + ArchivePath +" cannot be found");
                retval = false;
          
            }
            if (SkippedPath == string.Empty)
            {
                System.Console.WriteLine("Error: No skipped file path given");
                retval = false;
            }
            else if (!Directory.Exists(SkippedPath))
            {
                System.Console.WriteLine("Error: Skipped path " + SkippedPath + " cannot be found");
                retval = false;
            }
            else //skipped path exists, create error path if not there
            {
                if (!Directory.Exists(ErrorPath))
                {
                    Directory.CreateDirectory(ErrorPath);
                }
                
            }

            if (CITargetPath != string.Empty)
            {
                CITargetPath = Path.GetFullPath(CITargetPath);
                System.Console.WriteLine("CITargetPath used! Resulting files will be created to " +CITargetPath);
                
                if (!Directory.Exists(CITargetPath))
                {
                    Directory.CreateDirectory(CITargetPath);
                }
                iSinkParams.OutputDirectory = new DirectoryInfo(CITargetPath);
            }

            //make sure paths are absolute
            iArchivePath = Path.GetFullPath(iArchivePath);
            iSkippedPath = Path.GetFullPath(iSkippedPath);
            iErrorPath = Path.GetFullPath(iErrorPath);

            //add weekly division        
            DateTime today = DateTime.Now;
            CultureInfo culInfo = CultureInfo.CurrentCulture;
            int weekNum = culInfo.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = culInfo.Calendar.GetYear(today);

            iArchivePath = iArchivePath + @"\" + year + "_" + weekNum.ToString().PadLeft(2, '0');
            iSkippedPath = iSkippedPath + @"\" + year + "_" + weekNum.ToString().PadLeft(2, '0');
            iErrorPath = iErrorPath + @"\" + year + "_" + weekNum.ToString().PadLeft(2, '0');


            if (TestWithoutMovingFiles)
            {
                System.Console.WriteLine("Test mode parameter -t given: Not moving any files!" );
                retval = true;
            }
            else if (retval) //Archive & skipped directories exsits, clean up paths and add week numbers
            {
                
                if (!Directory.Exists(iArchivePath))
                {
                    Directory.CreateDirectory(iArchivePath);
                }
                if (!Directory.Exists(iSkippedPath))
                {
                    Directory.CreateDirectory(iSkippedPath);
                }
                if (!Directory.Exists(iErrorPath))
                {
                    Directory.CreateDirectory(iErrorPath);
                }                    
            }
            else
            {
                PrintCommandHelp();
            }
            System.Console.WriteLine("Using archive path " + ArchivePath + ", skipped path " + SkippedPath + " and error path " + ErrorPath);
    

            return retval;
        }

        private void PrintCommandHelp()
        {
            System.Console.WriteLine("Command line parameters:");
            System.Console.WriteLine("-a C:\\folderarchive\\   Location where to move files to permanent archive.");
            System.Console.WriteLine("-s C:\\folder\\skipped\\  Location where to put skipped files to wait reprocessing.");
            System.Console.WriteLine("-c C:\\folder\\output\\ Location where to put output files. Defaults to current working dir.");
            System.Console.WriteLine("-b crashfile.bin   Crash file to be decoded.");
            System.Console.WriteLine("-m crash.symbol,crash.map   Symbol/map/dictionary files.");
            System.Console.WriteLine("-f Force decoding even if files are without symbols.");
            System.Console.WriteLine("-t Test mode, will not move any files, ignores -a and -s.");
            System.Console.WriteLine("-x Prints output in Xml format");
            System.Console.WriteLine("-p Prints output in plain text format");
        }

        #endregion

        #region API
        #endregion

        #region Properties
        public CISinkSerializationParameters SinkParameters
        {
            get { return iSinkParams; }
        }

        public CACmdLineFSEntityList<CACmdLineFileSource> SourceFiles
        {
            get { return iSources; }
            set { iSources = value; }
        }

        public CACmdLineFSEntityList<CACmdLineFSEntity> MetaDataFiles
        {
            get { return iMetaData; }
        }

        public string ArchivePath
        {
            get { return iArchivePath; }
            set { iArchivePath = value; }
        }
        public string SkippedPath
        {
            get { return iSkippedPath; }
            set { iSkippedPath = value; }
        }
        public string ErrorPath
        {
            get { return iErrorPath; }
            set { iErrorPath = value; }
        }
        public string CITargetPath
        {
            get { return iCITargetPath; }
            set { iCITargetPath = value; }
        }  
        public bool DecodeWithoutSymbols
        {
            get { return iDecodeWithoutSymbols; }
            set { iDecodeWithoutSymbols = value; }
        }
        public bool TestWithoutMovingFiles
        {
            get { return iTestWithoutMovingFiles; }
            set { iTestWithoutMovingFiles = value; }
        }
        public bool UseXmlSink
        {
            get { return iUseXmlSink; }
            set { iUseXmlSink = value; }
        }

        #endregion

        #region Internal constants
        private const string KDebugMetaDataFileSelgeIni = "selge.ini";
        private const string KDebugMetaDataFileSelgeEventIni = "selge_event.ini";
        #endregion

        #region Internal methods
        private void FindDebugMetaDataFile( string aFileName )
        {
            // First try current working directory, and if not, then try application path.
            DirectoryInfo sourceDir = new DirectoryInfo( Environment.CurrentDirectory );

            string fileName = Path.Combine( sourceDir.FullName, aFileName );
            FileInfo file = new FileInfo( fileName );
            if ( file.Exists )
            {
                iMetaData.Add( file );
            }
            else
            {
                // Try app path
                fileName = Path.Combine(iAppPath, aFileName);
                file = new FileInfo( fileName );
                if ( file.Exists )
                {
                    iMetaData.Add( file );
                }
                else
                {
                    iTracer.Trace( "WARNING: Could not find debug meta data file: " + aFileName );
                }
            }
        }
        #endregion


        #region Data members
        private readonly ITracer iTracer;
        private readonly string iAppPath;
        private readonly CISinkSerializationParameters iSinkParams;
        private CACmdLineFSEntityList<CACmdLineFSEntity> iMetaData = new CACmdLineFSEntityList<CACmdLineFSEntity>();
        private CACmdLineFSEntityList<CACmdLineFileSource> iSources = new CACmdLineFSEntityList<CACmdLineFileSource>();

        private string iCITargetPath = string.Empty;  
        private string iArchivePath = string.Empty;       
        private string iSkippedPath = string.Empty;      
        private string iErrorPath = string.Empty;    
        private bool iTestWithoutMovingFiles = false;
        private bool iUseXmlSink = false;
        private bool iDecodeWithoutSymbols = false;



  

        #endregion
	}
}
