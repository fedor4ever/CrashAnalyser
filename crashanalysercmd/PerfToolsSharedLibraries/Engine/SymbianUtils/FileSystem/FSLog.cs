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
using System.Windows.Forms;
using SymbianUtils;
using SymbianUtils.Tracer;

namespace SymbianUtils.FileSystem
{
    public sealed class FSLog : DisposableObject, ITracer
    {
        #region Constructors
        public FSLog()
            : this( false )
        {
        }

        public FSLog( bool aIncludeTimeStamp )
        {
            iIncludeTimeStamp = aIncludeTimeStamp;
            //
            try
            {
                string path = Application.ExecutablePath;
                string exe = Path.GetFileName( path );
                path = Path.GetDirectoryName( path );
                //
                string file = Path.Combine( path, exe + ".debug.txt" );
                iStream = new StreamWriter( new FileStream( file, FileMode.Create ) );
                iStream.AutoFlush = true;
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region API
        public void TraceAlways( string aMessage )
        {
            DoTrace( aMessage, true );
        }
        #endregion

        #region Properties
        public bool Verbose
        {
            get { return iVerbose; }
            set { iVerbose = value; }
        }
        #endregion

        #region Internal methods
        private void DoTrace( string aLine, bool aDiagnostics )
        {
            if ( aDiagnostics )
            {
                System.Diagnostics.Debug.WriteLine( aLine );
            }

            // Try to output to file
            if ( iStream != null )
            {
                try
                {
                    iStream.WriteLine( aLine );
                }
                catch ( Exception )
                {
                    iStream.Close();
                    iStream = null;
                }
            }
        }
        #endregion

        #region DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iStream != null )
                {
                    iStream.Close();
                }
                iStream = null;
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            StringBuilder text = new StringBuilder( aMessage );
            if ( iIncludeTimeStamp )
            {
                DateTime now = DateTime.Now;
                text.Insert( 0, now.ToLongTimeString() + " - " );
            }

            string msg = text.ToString();
            System.Diagnostics.Debug.WriteLine( msg );

            if ( iVerbose )
            {
                System.Console.WriteLine( msg );
                DoTrace( msg, false );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string text = string.Format( aFormat, aParams );
            Trace( text );
        }
        #endregion

        #region Data members
        private readonly bool iIncludeTimeStamp;
        private StreamWriter iStream;
        private bool iVerbose = false;
        #endregion
    }
}
