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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbolLib.Generics;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Sources.Symbol.Engine;
using SymbolLib.Sources.Symbol.Symbol;
using SymbolLib.Sources.Symbol.Collection;

namespace SymbolLib.Sources.Symbol.Parser
{
    #region SymbolCollectionCreator interface
    internal interface SymbolCollectionCreator
    {
        SymbolsForBinary CreateCollection( string aHostFileName );
    }
    #endregion

    #region SymbolEntryCreator interface
    internal interface SymbolEntryCreator
    {
        SymbolSymbol CreateSymbol();
    }
    #endregion

	internal class SymbolFileParser : AsyncTextFileReader
	{
        #region Delegates & events
        public delegate void CollectionCreatedHandler( SymbolsForBinary aCollection );
        public event CollectionCreatedHandler CollectionCreated;

        public delegate bool CollectionCompletedHandler( SymbolsForBinary aCollection );
        public event CollectionCompletedHandler CollectionCompleted;

        public delegate void SymbolCreatedHandler( SymbolSymbol aSymbol );
        public event SymbolCreatedHandler SymbolCreated;
        #endregion

		#region Constructors
		internal SymbolFileParser( SymbolCollectionCreator aCollectionCreator, SymbolEntryCreator aEntryCreator, string aFileName, ITracer aTracer )
		:	base( aFileName, aTracer )
		{
            iCollectionCreator = aCollectionCreator;
            iEntryCreator = aEntryCreator;
		}
		#endregion

        #region Constants
        public const bool KCollectionCompletedAndAbortParsing = false;
        public const bool KCollectionCompletedAndContinueParsing = true;
        #endregion

        #region Parsing API
        public void Read( TSynchronicity aSynchronicity )
        {
            base.StartRead( aSynchronicity );
        }
		#endregion

        #region Properties
        public bool ContainedAtLeastOneCollectionFileName
        {
            get { return iContainedAtLeastOneCollectionFileName; }
        }
        #endregion

        #region From AsyncTextReader
        protected override bool ImmediateAbort()
        {
            return iImmediateAbort;
        }

        protected override void HandleReadStarted()
        {
#if PROFILING
            System.DateTime startTime = DateTime.Now;

            using ( System.IO.StreamReader reader = new StreamReader( this.FileName ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    line = reader.ReadLine();
                }
            }

            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - startTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "SIMPLE READ COMPLETE - " + tickDuration.ToString( "d6" ) );
#endif

            base.HandleReadStarted();
        }

        protected override void HandleFilteredLine( string aLine )
		{
            if ( aLine != null )
            {
                int len = aLine.Length;
                if ( len > 5 && aLine.Substring( 0, 5 ) == "From " )
			    {
                    aLine = aLine.Substring( 5 ).TrimStart();
                    HandleStartOfNewCollection( aLine );
                }
                else
                {
                    HandleSymbolLine( aLine );
                }
            }
        }

		protected override void HandleReadCompleted()
		{
			try
			{
                OnCollectionCompleted( iCurrentCollection );
			}
			finally
			{
                base.HandleReadCompleted();
			}
		}

		protected override void HandleReadException( Exception aException )
		{
            base.HandleReadException( aException );
            //
            System.Windows.Forms.MessageBox.Show( aException.StackTrace, aException.Message );
		}
		#endregion

		#region Internal methods
        private void OnCollectionCreated( SymbolsForBinary aCollection )
        {
            if ( CollectionCreated != null && aCollection != null )
            {
                CollectionCreated( aCollection );
            }
        }

        private void OnCollectionCompleted( SymbolsForBinary aCollection )
        {
            if ( CollectionCompleted != null && aCollection != null )
            {
                // If returns immediate abort then we will stop parsing
                // straight away.
                iImmediateAbort = ( CollectionCompleted( aCollection ) == KCollectionCompletedAndAbortParsing );
            }
        }

        private void HandleStartOfNewCollection( string aHostFileName )
        {
            // Current collection is now complete
            OnCollectionCompleted( iCurrentCollection );
            
            // We will create the new collection once we see the first symbol
            iCurrentCollection = null;

            // Cache the name of the new collection
            iCurrentCollectionHostFileName = aHostFileName;

            // We've seen at least one "From" line.
            iContainedAtLeastOneCollectionFileName = true;
        }

        private void HandleSymbolLine( string aLine )
        {
            try
            {
                // If we have not yet made a collection for this symbol, then do so now
                CreateNewCollection();

                SymbolSymbol entry = iEntryCreator.CreateSymbol();
                bool entryOk = entry.Parse( aLine );
                if ( entryOk && SymbolCreated != null )
                {

                    // Notify that we created a symbol
                    SymbolCreated( entry );
                }
            }
            catch( GenericSymbolicCreationException )
            {
#if TRACE
                System.Diagnostics.Debug.WriteLine( "SymbolParseException: " + aLine );
#endif
            }
        }

        private void CreateNewCollection()
        {
            if ( iCurrentCollection == null )
            {
                System.Diagnostics.Debug.Assert( !string.IsNullOrEmpty( iCurrentCollectionHostFileName ) );

                // Combine symbol file drive letter with binary name from the symbol file itself.
                string hostBinaryFileName = Path.GetPathRoot( base.FileName );
                if ( iCurrentCollectionHostFileName.StartsWith( @"\" ) )
                {
                    iCurrentCollectionHostFileName = iCurrentCollectionHostFileName.Substring( 1 );
                }
                hostBinaryFileName = Path.Combine( hostBinaryFileName, iCurrentCollectionHostFileName );

                iCurrentCollection = iCollectionCreator.CreateCollection( hostBinaryFileName );
                OnCollectionCreated( iCurrentCollection );
                iCurrentCollectionHostFileName = string.Empty;
            }
        }
        #endregion

		#region Data members
        private SymbolsForBinary iCurrentCollection = null;
        private readonly SymbolEntryCreator iEntryCreator;
        private readonly SymbolCollectionCreator iCollectionCreator;
        private bool iImmediateAbort = false;
        private string iCurrentCollectionHostFileName = string.Empty;
        private bool iContainedAtLeastOneCollectionFileName = false;
        #endregion
	}
}
