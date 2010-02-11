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
using System.Collections;
using System.Text;
using SymbianStructuresLib.Debug.Common.Interfaces;

namespace SymbianStructuresLib.Debug.Common.Id
{
    public class PlatformIdAllocator : IPlatformIdAllocator
    {
        #region Constructors
        public PlatformIdAllocator()
            : this( PlatformId.KInitialValue )
        {
        }

        public PlatformIdAllocator( uint aInitialId )
        {
            iId = aInitialId;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region IPlatformIdAllocator Members
        public PlatformId AllocateId()
        {
            return new PlatformId( ++iId );
        }
        #endregion

        #region Data members
        private uint iId = 0u;
        #endregion
    }
}