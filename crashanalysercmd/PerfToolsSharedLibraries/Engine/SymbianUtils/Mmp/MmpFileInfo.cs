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
using System.Collections.Generic;
using System.Text;

namespace SymbianUtils.Mmp
{
    public class MmpFileInfo
    {
        #region Enumerations
        public enum TTargetType
        {
            ETargetTypeEXE = 0,
            ETargetTypeDLL,
            ETargetTypeUnsupported
        }
        #endregion

        #region Constructor & destructor
        public MmpFileInfo( string aFileName )
        {
            iFileName = aFileName;
        }
        #endregion

        #region Properties
        public string FileName
        {
            get { return iFileName; }
        }

        public List<uint> Uids
        {
            get { return iUids; }
        }

        public uint MostSignificantUid
        {
            get
            {
                uint ret = 0;
                //
                if ( Uids.Count > 0 )
                {
                    ret = Uids[ Uids.Count - 1 ];
                }
                //
                return ret;
            }
        }

        public string Target
        {
            get { return iTarget; }
            set { iTarget = value; }
        }

        public TTargetType TargetType
        {
            get { return iTargetType; }
            set { iTargetType = value; }
        }
        #endregion

        #region Data members
        private readonly string iFileName;
        private List<uint> iUids = new List<uint>();
        private string iTarget = string.Empty;
        private TTargetType iTargetType = TTargetType.ETargetTypeUnsupported;
        #endregion
    }
}
