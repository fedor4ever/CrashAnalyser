/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using CrashDebuggerLib.Structures.KernelObjects;

namespace CrashDebuggerLib.Structures.Common
{
    public class ExitInfo
    {
        #region Enumerations
        public enum TExitType
        {
            /**
            The thread or process has ended as a result of a kill,
            i.e. Kill() has been called on the RThread or RProcess handle.
            Or a thread was ended as a result of calling User::Exit().
            */
            EExitKill,

            /**
            The thread or process has ended as a result of a terminate,
            i.e. Terminate() has been called on the RThread or RProcess handle.
            */
            EExitTerminate,

            /**
            The thread or process has been panicked.
            */
            EExitPanic,

            /**
            The thread or process is alive.
            */
            EExitPending
        }
        #endregion

        #region Constructors
        public ExitInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TExitType Type
        {
            get { return iType; }
            set { iType = value; }
        }

        public string Category
        {
            get { return iCategory; }
            set { iCategory = value; }
        }

        public int Reason
        {
            get { return iReason; }
            set { iReason = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( Type == TExitType.EExitPending )
            {
                ret.Append( "[Pending]" );
            }
            else
            {
                switch ( Type )
                {
                    case TExitType.EExitKill:
                        ret.Append( "[Kill]" );
                        break;
                    case TExitType.EExitPanic:
                        ret.Append( "[Panic]" );
                        break;
                    case TExitType.EExitTerminate:
                        ret.Append( "[Terminate]" );
                        break;
                }

                ret.AppendFormat( " {0}-{1}", Category, Reason );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private TExitType iType = TExitType.EExitPending;
        private int iReason = 0;
        private string iCategory = string.Empty;
        #endregion
    }
}
