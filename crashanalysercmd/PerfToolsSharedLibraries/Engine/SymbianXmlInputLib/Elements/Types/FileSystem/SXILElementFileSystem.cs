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
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace SymbianXmlInputLib.Elements.Types.FileSystem
{
    public abstract class SXILElementFileSystem : SXILElement
    {
        #region Constructors
        protected SXILElementFileSystem()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public abstract bool Exists
        {
            get;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
