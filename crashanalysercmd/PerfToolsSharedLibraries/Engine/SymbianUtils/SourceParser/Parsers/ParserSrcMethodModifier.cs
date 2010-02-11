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
using SymbianUtils.SourceParser.Objects;

namespace SymbianUtils.SourceParser.Parsers
{
    public class ParserSrcMethodModifier
    {
        #region Constructors
        public ParserSrcMethodModifier()
        {
        }
        #endregion

        #region API
        public SrcMethodModifier Parse( ref string aText )
        {
            SrcMethodModifier ret = null;

            // Should not be the first or second character as
            // we should always have an opening bracket before
            // the closing bracket, and the shortest possible
            // valid sequence is "(<char>)" where <char> is a
            // single character (type).
            //
            // Needs to also handle tricky things like...:
            //
            // RDbHandle::operator ->() const

            int closingBracketPos = aText.LastIndexOf( ")" );
            System.Diagnostics.Debug.Assert( closingBracketPos >= 1 );

            // Get the modifier text. We currently only support
            // one modifier and that's the const keyword
            string modifierText = aText.Substring( closingBracketPos + 1 ).Trim();
            if ( modifierText.Length > 0 )
            {
                ret = new SrcMethodModifier();

                if ( modifierText.ToLower() == "const" )
                {
                    ret.Type = SrcMethodModifier.TModifier.EConst;
                }
            }

            // Clean up the text object we were passed...
            aText = aText.Substring( 0, closingBracketPos + 1 );

            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
