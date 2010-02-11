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
using SymbianETMLib.Common.Buffer;
using SymbianETMLib.Common.State;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.Utilities;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Engine;
using SymbianETMLib.ETB.Config;
using SymbianETMLib.ETB.Buffer;

namespace SymbianETMLib.ETB.Engine
{
    public class ETBEngine : ETEngineBase
    {
        #region Constructors
        public ETBEngine( ETBBuffer aBuffer, ETConfigBase aConfig )
            : base( aBuffer, aConfig )
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public new ETBBuffer Buffer
        {
            get { return base.Buffer as ETBBuffer; }
        }

        public new ETBConfig Config
        {
            get { return base.Config as ETBConfig; }
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
