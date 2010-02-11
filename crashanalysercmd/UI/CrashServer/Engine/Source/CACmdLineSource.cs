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
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Source;

namespace CrashAnalyserServerExe.Engine
{
	internal class CACmdLineSource : CISource
    {
        #region Constructors
        public CACmdLineSource( FileInfo aFile )
            : base( aFile )
		{
		}
        #endregion

		#region From CISource
        public override Version ImplementorVersion
        {
            get { return new Version( 0, 0 ); }
        }

        public override string ImplementorName
        {
            get { return string.Empty; }
        }
        #endregion

		#region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
	}
}
