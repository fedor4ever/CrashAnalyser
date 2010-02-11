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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using SymbianUtils.Tracer;
using CrashItemLib.PluginAPI;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Engine.Sources.Types
{
    internal abstract class CIEngineSourceReader : ITracer
    {
        #region Constructors
        protected CIEngineSourceReader( CIEngineSource aSource )
        {
            iSource = aSource;
        }
        #endregion

        #region Abstract API
        public abstract CIEngineSource.TState Read();

        public abstract CFFSource.TReaderOperationType OpType
        {
            get;
        }
        #endregion

        #region API
        protected void AddException( Exception aException )
        {
            iExceptions.Add( aException );
        }

        protected void SaveCrash( CIContainer aCrashContainer )
        {
            iSource.SaveContainer( aCrashContainer );
        }
        #endregion

        #region Properties
        public CIEngineSource Source
        {
            get { return iSource; }
        }

        public int CrashItemCount
        {
            get { return iSource.ContainerCount; }
        }

        public bool HasExceptions
        {
            get { return iExceptions.Count > 0; }
        }

        public Exception[] Exceptions
        {
            get { return iExceptions.ToArray(); }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iSource.Engine.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iSource.Engine.Trace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly CIEngineSource iSource;
        private List<Exception> iExceptions = new List<Exception>();
        #endregion
    }
}
