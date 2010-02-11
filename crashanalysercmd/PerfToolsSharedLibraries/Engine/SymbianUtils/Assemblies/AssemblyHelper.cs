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
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SymbianUtils.Assemblies
{
    public static class AssemblyHelper
    {
        #region API
        public static bool IsCLRAssembly( string aFileName )
        {
            bool ret = false;
            //
            try
            {
                Assembly anAssembly = Assembly.LoadFrom( aFileName );
                ret = true;
            }
            catch ( BadImageFormatException )
            {
            }
            //
            return ret;
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
