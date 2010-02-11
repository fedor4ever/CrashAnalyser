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
using System.Reflection;
using SymbianParserLib.Enums;
using SymbianParserLib.Elements;
using SymbianParserLib.Elements.SubFields;

namespace SymbianParserLib.RegExTranslators.Cache
{
    internal class RegExTranslatorCacheEntry
    {
        #region Constructors
        public RegExTranslatorCacheEntry( ParserLine aLine )
        {
            iLine = aLine;
        }
        #endregion

        #region API
        public ParserLine Clone()
        {
            ParserLine ret = ParserLine.NewCopy( iLine );
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly ParserLine iLine;
        #endregion
    }
}
