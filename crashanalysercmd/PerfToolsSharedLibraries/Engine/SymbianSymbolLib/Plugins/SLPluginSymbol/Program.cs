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
﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SymbianUtils.Range;

namespace SymbianSymbolLibTest
{
    class Program
    {
        class Binary
        {
            public string iFileName = string.Empty;
            public byte[] iData = null;
            public long iDataStart = 0;
            public long iDataEnd = 0;
        }
        
        class Binary2
        {
            public string iFileName = string.Empty;
            public List<string> iData = new List<string>();
        }

        class Store
        {
            public Store()
            {
            }

            public void Add( string aFileName, Binary2 aData )
            {
                lock ( iEntries )
                {
                    if ( !iEntries.ContainsKey( aFileName ) )
                    {
                        iEntries.Add( aFileName, aData );
                    }
                }
            }

            public int Count
            {
                get { return iEntries.Count; }
            }

            public void Dump()
            {
            }

            private Dictionary<string, Binary2> iEntries = new Dictionary<string, Binary2>();
        }

        class ThreadReader
        {
            public delegate void CompletionHandler( ThreadReader aReader );
            public event CompletionHandler Completed = null;

            public ThreadReader( byte[] aData, int aStart, int aCount, Store aStore )
            {
                iData = aData;
                iStart = aStart;
                iCount = aCount;
                iStore = aStore;
            }

            public void Read()
            {
                //System.Console.WriteLine( string.Format( "Reading starting - pos: 0x{0}, len: {1:d8}", iStart, iCount ) );
                DateTime timeStamp = DateTime.Now;
                
                Binary2 currentBinary = null;

                long binaryCount = 0;
                long lineCount = 0;
                using ( StreamReader reader = new StreamReader( new MemoryStream( iData, iStart, iCount ) ) )
                {
                    string line = reader.ReadLine();
                    while ( line != null )
                    {
                        ++lineCount;
                        if ( line.StartsWith( "From    " ) )
                        {
                            currentBinary = new Binary2();
                            currentBinary.iFileName = line.Substring( 8 );
                            //
                            ++binaryCount;
                            iStore.Add( currentBinary.iFileName, currentBinary );
                        }
                        else if ( currentBinary != null )
                        {
                            currentBinary.iData.Add( line );
                        }

                        line = reader.ReadLine();
                    }
                }

                DateTime endTime = DateTime.Now;
                TimeSpan span = ( endTime - timeStamp );
                //System.Console.WriteLine( string.Format( "Reading complete - {0} time, {1} lines, {2} binaries", span, lineCount, binaryCount ) );

                if ( Completed != null )
                {
                    Completed( this );
                }
            }

            private readonly byte[] iData;
            private readonly int iStart;
            private readonly int iCount;
            private readonly Store iStore;
        }

        class SymbolFile
        {
            #region Constructors
            public SymbolFile( FileInfo aFile )
            {
                iFile = aFile;
            }
            #endregion

            #region API
            public void Read()
            {
                Partition();
                //
                iWaiter = new AutoResetEvent( false );
                for( int i=iReaders.Count-1; i>=0; i-- )
                {
                    ThreadReader reader = iReaders[ i ];
                    reader.Completed += new ThreadReader.CompletionHandler( Reader_Completed );
                    ThreadPool.QueueUserWorkItem( new WaitCallback(StartReader ), reader );
                }
                
                // Now wait
                using ( iWaiter )
                {
                    iWaiter.WaitOne();
                }
                iWaiter = null;
            }
            #endregion

            #region Properties
            public int Count
            {
                get { return iStore.Count; }
            }
            #endregion

            #region Event handlers
            private void Reader_Completed( ThreadReader aReader )
            {
                iCompleted.Add( aReader );
                iReaders.Remove( aReader );
                //
                if ( iReaders.Count == 0 )
                {
                    iWaiter.Set();
                }
            }
            #endregion

            #region Internal methods
            private static void StartReader( object aThreadReader )
            {
                ThreadReader reader = (ThreadReader) aThreadReader;
                reader.Read();
            }

            private void Partition()
            {
                // Read entire file into buffer
                byte[] data = new byte[ iFile.Length ];
                using ( Stream reader = new FileStream( iFile.FullName, FileMode.Open ) )
                {
                    reader.Read( data, 0, data.Length );
                }

                // This is the pattern we are searching for:
                byte[] pattern = new byte[] { (byte) 'F', (byte) 'r', (byte) 'o', (byte) 'm', (byte) ' ', (byte) ' ', (byte) ' ', (byte) ' ' };

                int threadCount = System.Environment.ProcessorCount;
                int chunkSize = (int) iFile.Length / threadCount;

                //
                int blockPosStart = 0;
                for ( int i = 0; i < threadCount; i++ )
                {
                    int pos = 0;
                    int blockPosEnd = Math.Min( data.Length - 1, blockPosStart + chunkSize );
                    while ( pos >= 0 )
                    {
                        pos = Array.IndexOf( data, pattern[ 0 ], blockPosEnd );
                        if ( pos > 0 )
                        {
                            if ( pos + 8 >= data.Length )
                            {
                                break;
                            }
                            else if ( pos + 8 < data.Length && data[ pos + 7 ] == pattern[ 7 ] )
                            {
                                bool isMatch = CompareByteArrays( pattern, data, pos );
                                if ( isMatch )
                                {
                                    int length = pos - blockPosStart;
                                    System.Console.WriteLine( string.Format( "Block {0:d2} @ 0x{1:x8}, length: {2:d8}", i, blockPosStart, length ) );
                                    //
                                    ThreadReader reader = new ThreadReader( data, blockPosStart, length, iStore );
                                    iReaders.Add( reader );

                                    blockPosStart = pos;
                                    break;
                                }
                            }
                            else
                            {
                                // Didn't find a match, move forwards
                                blockPosEnd = pos + 1;
                            }
                        }
                        else
                        {
                            // Searched to end of file and didn't find another block, so just create
                            // a new reader for everything that remains.
                            int length2 = data.Length - blockPosStart;
                            System.Console.WriteLine( string.Format( "Block {0:d2} @ 0x{1:x8}, length: {2:d8}", i, blockPosStart, length2 ) );
                            //
                            ThreadReader reader2 = new ThreadReader( data, blockPosStart, length2, iStore );
                            iReaders.Add( reader2 );
                            break;
                        }
                    }
                }
            }

            private static bool CompareByteArrays( byte[] aSearchFor, byte[] aSearchIn, int aStartPos )
            {
                bool areEqual = true;
                //
                for ( int i = aStartPos; i < aSearchFor.Length; i++ )
                {
                    byte b = aSearchIn[ i ];
                    byte c = aSearchFor[ i - aStartPos ];
                    if ( b != c )
                    {
                        areEqual = false;
                        break;
                    }
                }
                //
                return areEqual;
            }
            #endregion

            #region Data members
            private readonly FileInfo iFile;
            private AutoResetEvent iWaiter = null;
            private Store iStore = new Store();
            private List<ThreadReader> iCompleted = new List<ThreadReader>();
            private List<ThreadReader> iReaders = new List<ThreadReader>();
            #endregion
        }

        static void Main( string[] args )
        {
            string path = @"C:\Tool Demo Files\2. Crash Data\File43\CoreImage"; // C:\Tool Demo Files\2. Crash Data\File28\";
            DirectoryInfo dir = new DirectoryInfo( path );
            FileInfo[] files = dir.GetFiles( "*.symbol" );
            //
            //
            foreach ( FileInfo file in files )
            {
                //
                DateTime timeStamp = DateTime.Now;
                System.Console.WriteLine( string.Format( "[{0}] - Reading starting...", file.Name ) );

                SymbolFile symbolFile = new SymbolFile( file );
                symbolFile.Read();

                DateTime endTime = DateTime.Now;
                TimeSpan span = ( endTime - timeStamp );

                System.Console.WriteLine( string.Format( "[{0}] - Reading complete - {1} time, {2} binaries", file.Name, span, symbolFile.Count ) );
                System.Console.WriteLine( " " );
                System.Console.WriteLine( " " );
                System.Console.WriteLine( " " );
                System.Console.ReadKey();
            }
        }
    }
}
