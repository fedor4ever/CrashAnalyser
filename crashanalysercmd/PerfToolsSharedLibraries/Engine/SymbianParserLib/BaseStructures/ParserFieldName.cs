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

namespace SymbianParserLib.BaseStructures
{
    public class ParserFieldName
    {
        #region Constructors
        public ParserFieldName()
        {
        }

        public ParserFieldName( string aName )
        {
            iName = aName;
        }

        internal ParserFieldName( ParserFieldName aCopy )
        {
            iName = aCopy.Name;
        }
        #endregion

        #region API
        public static implicit operator string( ParserFieldName aFieldName )
        {
            return aFieldName.Name;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
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
