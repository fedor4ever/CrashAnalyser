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

namespace SymbianInstructionLib.Arm.Instructions.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class ArmRefAttribute : Attribute
    {
        #region Constructors
        public ArmRefAttribute( string aPageRef, string aDescription )
        {
            iPageRef = aPageRef;
            iDescription = aDescription;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string PageRef
        {
            get { return iPageRef; }
            set { iPageRef = value; }
        }

        public string Description
        {
            get { return iDescription; }
            set { iDescription = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        protected string iPageRef = string.Empty;
        protected string iDescription = string.Empty;
        #endregion
    }
}
