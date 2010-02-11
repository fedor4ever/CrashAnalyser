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
    public class ParserSrcClass
    {
        #region Constructors
        public ParserSrcClass()
        {
        }
        #endregion

        #region API
        public SrcClass Parse( ref string aText )
        {
            SrcClass ret = new SrcClass();

            // First look for the class separator. If we find that, then everything before
            // it is the class name.
            //
            // If no class separator exists, then we treat the whole thing as the class
            // name.
            int pos = aText.IndexOf( SrcClass.KClassSeparator );
            if ( pos > 0 || pos < 0 )
            {
                // By default, treat the whole text as the class name
                string className = aText;
                if ( pos > 0 )
                {
                    className = aText.Substring( 0, pos );
                    aText = aText.Substring( pos + SrcClass.KClassSeparator.Length );
                    ret.Name = className;
                }
                else
                {
                    // Everything was consumed... We return the 'global function' class
                    // for this text
                    aText = string.Empty;
                }
            }
            else
            {
                // First thing in the string is the class separator.
                // Odd?
                System.Diagnostics.Debug.Assert( false );
            }

            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
