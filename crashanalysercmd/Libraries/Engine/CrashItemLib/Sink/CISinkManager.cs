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
using SymbianUtils.PluginManager;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.Engine;

namespace CrashItemLib.Sink
{
    public class CISinkManager : IEnumerable<CISink>
    {
        #region Constructors
        internal CISinkManager( CIEngine aEngine )
        {
            iEngine = aEngine;
            LoadSinks();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public int Count
        {
            get { return iSinks.Count; }
        }

        public CISink this[ int aIndex ]
        {
            get { return iSinks[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        private void LoadSinks()
        {
            object[] parameters = new object[ 1 ];
            parameters[ 0 ] = this;
            //
            try
            {
                iSinks.Load( parameters );
            }
            catch ( Exception assemblyLoadException )
            {
                iEngine.Trace( "SINK CREATION EXCEPTION: " + assemblyLoadException.Message );
                iEngine.Trace( "  " + assemblyLoadException.StackTrace );
            }
        }
        #endregion

        #region From System.Object
        #endregion

        #region From IEnumerable<CISink>
        public IEnumerator<CISink> GetEnumerator()
        {
            return iSinks.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return iSinks.GetEnumerator();
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private PluginManager<CISink> iSinks = new PluginManager<CISink>();
		#endregion
    }
}
