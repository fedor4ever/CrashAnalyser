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

namespace SymbianUtils.SourceParser.Objects
{
    public class SrcMethodParameter
    {
        #region Constructors
        public SrcMethodParameter()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private string iName = string.Empty;
        #endregion
    }
}
