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
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine;

namespace CrashItemLib.Sink
{
    public abstract class CISink
    {
        #region Constructors
        protected CISink( string aName, CISinkManager aManager )
        {
            iName = aName;
            iSinkManager = aManager;
        }
        #endregion

        #region API
        public abstract object Serialize( CISinkSerializationParameters aParams );

        public virtual object CustomOperation( string aOpName, params object[] aArguments )
        {
            throw new NotSupportedException();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
        }

        public CISinkManager Manager
        {
            get { return iSinkManager; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private readonly string iName;
        private readonly CISinkManager iSinkManager;
		#endregion
    }
}
