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

namespace CrashDebuggerLib.Structures.NThread
{
    public class NThreadExtraContextInfo
    {
        #region Enumerations
        public enum TExtraContextType
        {
            ENone = 0,
            EDynamicallyAllocated,
            EStaticallyAllocated,
        }
        #endregion

        #region Constructors
        public NThreadExtraContextInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint ExtraContext
        {
            get { return iExtraContext; }
            set { iExtraContext = value; }
        }

        public int ExtraContextSize
        {
            get { return iExtraContextSize; }
            set { iExtraContextSize = value; }
        }

        public uint ExtraContextSizeRaw
        {
            get { return (uint) iExtraContextSize; }
            set { iExtraContextSize = (int) value; }
        }

        public TExtraContextType ExtraContextType
        {
            get
            {
                TExtraContextType ret = TExtraContextType.ENone;
                //
                if ( ExtraContextSize > 0 )
                {
                    ret = TExtraContextType.EDynamicallyAllocated;
                }
                else if ( ExtraContextSize < 0 )
                {
                    ret = TExtraContextType.EStaticallyAllocated;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private uint iExtraContext = 0;		// coprocessor context
        private int iExtraContextSize = 0;	// +ve=dynamically allocated, 0=none, -ve=statically allocated
        #endregion
    }
}
