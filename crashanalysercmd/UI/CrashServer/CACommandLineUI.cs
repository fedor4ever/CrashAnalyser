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
using System.Collections.Generic;
using System.Data;
using SymbianDebugLib.Engine;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianUtils.FileSystem;
using SymbianUtils.Tracer;
using SymbianUtils;
using CrashAnalyserServerExe.Engine;

namespace CrashAnalyserServerExe
{
    internal class CACommandLineUI : DisposableObject, ITracer
	{
		#region Constructors
        public CACommandLineUI( string[] aArguments, FSLog aLog )
		{
            iLog = aLog;

            // Create engine
            iDebugEngine = new DbgEngine( this );

            // Work out if we are in verbose mode
            CheckArgsForVerbose();

            // Create main command line engine
            iEngine = new CACmdLineEngine( iDebugEngine );
        }
		#endregion

        #region API
        public int Run()
        {
            iLog.TraceAlways( "[SvrExe] Run() - START" );
            //
            int error = CACmdLineException.KErrNone;
            try
            {
                error = iEngine.RunCommandLineOperations();
            }
            finally
            {
                iLog.TraceAlways( "[SvrExe] Run() - END - error: " + error );
            }
            //
            return error;
        }
        #endregion

        #region Properties
        public bool Verbose
        {
            get { return iLog.Verbose; }
            private set
            { 
                iLog.Verbose = value;
                iLog.TraceAlways( "[SvrExe] Verbose Mode: " + value.ToString() );
            }
        }
        #endregion

        #region Internal constants
        private const string KParamVerbose = "-V";
        #endregion

        #region Internal methods
        private void CheckArgsForVerbose()
        {
            bool ret = Environment.CommandLine.Contains( KParamVerbose );
            this.Verbose = ret;
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iLog.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string msg = string.Format( aFormat, aParams );
            Trace( msg );
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
                iEngine.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iDebugEngine;
        private readonly FSLog iLog;
        private readonly CACmdLineEngine iEngine;
        #endregion
    }
}
