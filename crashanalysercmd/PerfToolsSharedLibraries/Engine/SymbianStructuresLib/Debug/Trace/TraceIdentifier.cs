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
using System.IO;
using System.Text;
using SymbianUtils;
using SymbianUtils.Threading;

namespace SymbianStructuresLib.Debug.Trace
{
    public class TraceIdentifier
    {
        #region Constructors
        public TraceIdentifier( uint aComponent, uint aGroup, uint aId )
        {
            iComponent = aComponent;
            iGroup = aGroup;
            iId = aId;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Component
        {
            get { return iComponent; }
        }

        public uint Group
        {
            get { return iGroup; }
        }

        public uint Id
        {
            get { return iId; }
        }

        public TraceLocation Location
        {
            get { return iLocation; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "[{0:x8} | {1:x4} | {2:x4}]", iComponent, iGroup, iId );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly uint iComponent;
        private readonly uint iGroup;
        private readonly uint iId;
        private TraceLocation iLocation = new TraceLocation();
        #endregion
    }
}
