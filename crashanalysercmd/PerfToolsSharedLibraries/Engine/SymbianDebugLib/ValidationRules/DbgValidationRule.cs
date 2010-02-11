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
using System.Reflection;
using SymbianDebugLib.Engine;

namespace SymbianDebugLib.ValidationRules
{
    public abstract class DbgValidationRule
    {
        #region Enumerations
        public enum TOperation
        {
            EOperationPrime = 0
        }
        #endregion

        #region Constructors
        protected DbgValidationRule( DbgValidationManager aValidationManager )
        {
            iValidationManager = aValidationManager;
        }
        #endregion

        #region Abstract API
        public abstract bool IsValid( TOperation aType, ref string aErrorDescription );
        #endregion

        #region Properties
        protected DbgEngine Engine
        {
            get { return iValidationManager.Engine; }
        }

        protected DbgValidationManager ValidationManager
        {
            get { return iValidationManager; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgValidationManager iValidationManager;
        #endregion
    }
}
