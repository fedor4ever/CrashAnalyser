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

namespace SymbianUtils.XRef
{
    public class XRefLauncher
    {
        #region Constructors
        public XRefLauncher()
        {
        }
        #endregion

        #region API
        public void Launch( XRefIdentifer aIdentifier, XRefSettings aSettings )
        {
            string url = aSettings.ServerRootPath + "ident?i=" + aIdentifier.Identifier;
            System.Diagnostics.Process.Start( url );
        }
        #endregion
    }
}
