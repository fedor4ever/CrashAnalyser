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
    public class CFFFileSpecification
    {
        #region Static factory functions
        public static CFFFileSpecification Custom( string aDescription, string aExtensions )
        {
            return new CFFFileSpecification( aDescription, aExtensions );
        }

        public static CFFFileSpecification AllFiles()
        {
            return new CFFFileSpecification();
        }

        public static CFFFileSpecification TraceFiles()
        {
            CFFFileSpecification ret = new CFFFileSpecification();
            ret.Description = "Text files";
            //
            StringBuilder extensions = new StringBuilder();
            foreach ( string extn in CISource.KExtensionsTrace )
            {
                extensions.Append( "*" + extn + ";" );
            }
            
            // Remove last trailing semi-colon
            extensions.Remove( extensions.Length - 1, 1 );
            ret.Extensions = extensions.ToString();
            return ret;
        }
        #endregion

        #region Constructors
        private CFFFileSpecification()
		{
        }

        private CFFFileSpecification( string aDescription, string aExtensions )
        {
            Description = aDescription;
            Extensions = aExtensions;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Description
        {
            get { return iDescription; }
            set { iDescription = value; }
        }

        public string Extensions
        {
            get { return iExtensions; }
            set { iExtensions = value; }
        }
        #endregion

		#region Data members
        private string iDescription = "All Files";
        private string iExtensions = "*.*";
        #endregion
    }
}
