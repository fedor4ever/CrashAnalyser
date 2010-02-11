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
    public class SrcMethodModifier
    {
        #region Enumerations
        public enum TModifier
        {
            ENone = 0,
            EConst
        }
        #endregion

        #region Constructors
        public SrcMethodModifier()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TModifier Type
        {
            get { return iModifier; }
            set { iModifier = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = string.Empty;
            //
            switch ( iModifier )
            {
                case TModifier.EConst:
                    ret = " const";
                    break;
                default:
                case TModifier.ENone:
                    break;
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private TModifier iModifier;
        #endregion
    }
}
