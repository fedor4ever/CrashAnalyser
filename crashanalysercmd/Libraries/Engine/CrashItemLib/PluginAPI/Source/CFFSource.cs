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
using CrashItemLib.Crash.Source;

namespace CrashItemLib.PluginAPI
{
    /// <summary>
    /// Source with file format-specific extensions
    /// </summary>
    public abstract class CFFSource : CISource
    {
        #region Enumerations
        public enum TReaderOperationType
        {
            EReaderOpTypeNotSupported = 0,
            EReaderOpTypeNative,
            EReaderOpTypeTrace
        }
        #endregion

        #region Constructors
        protected CFFSource( FileInfo aFile )
            : base( aFile )
		{
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public CFFReader Reader
        {
            get { return iReader; }
            set { iReader = value; }
        }

        public TReaderOperationType OpType
        {
            get { return iOpType; }
            set { iOpType = value; }
        }
        #endregion

		#region Data members
        private CFFReader iReader = null;
        private TReaderOperationType iOpType = TReaderOperationType.EReaderOpTypeNotSupported;
        #endregion
    }
}
