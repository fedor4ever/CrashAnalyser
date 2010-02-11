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
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using CrashItemLib;

namespace CrashItemLib.PluginAPI
{
    /// <summary>
    /// Master crash file format (CFF) plugin interface. All crash file formats
    /// must implement this base class in order to be invoked by Crash Analyser
    /// </summary>
    public abstract class CFFPlugin
	{
		#region Constructors
        protected CFFPlugin( CFFDataProvider aDataProvider )
		{
            iDataProvider = aDataProvider;
		}
		#endregion

        #region API
        public abstract CFFSourceAndConfidence GetConfidence( FileInfo aFile, CFFFileList aOtherFiles );

        public abstract void GetSupportedFileTypes( List<CFFFileSpecification> aFileTypes );
        #endregion

        #region Properties
        public abstract string Name
        {
            get;
        }

        public CFFDataProvider DataProvider
        {
            get { return iDataProvider; }
        }
        #endregion

        #region Data members
        private readonly CFFDataProvider iDataProvider;
		#endregion
	}
}
