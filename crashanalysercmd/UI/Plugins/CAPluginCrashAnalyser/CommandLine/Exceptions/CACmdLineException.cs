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
using CrashAnalyserEngine.Plugins;

namespace CAPCrashAnalysis.CommandLine
{
	internal class CACmdLineException : Exception
	{
        #region Constructors
        public CACmdLineException( string aMessage )
            : this( aMessage, CAPlugin.KErrCommandLineGeneral )
		{
		}
       
        public CACmdLineException( string aMessage, Exception aInnerException )
            : this( aMessage, aInnerException, CAPlugin.KErrCommandLineGeneral )
        {
        }

        public CACmdLineException( string aMessage, Exception aInnerException, int aErrorCode )
            : base( aMessage, aInnerException )
        {
            iErrorCode = aErrorCode;
        }
       
        public CACmdLineException( string aMessage, int aErrorCode )
            : base( aMessage )
		{
            iErrorCode = aErrorCode;
		}
        #endregion

		#region API
        public static void CreateXmlErrorFile( Stream aStream, Exception aException )
        {
            using ( aStream )
            {
            }
        }
        #endregion

		#region Properties
        public int ErrorCode
        {
            get { return iErrorCode; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly int iErrorCode;
        #endregion
	}
}
