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
using SymbianUtils.SourceParser.Parsers;

namespace SymbianUtils.SourceParser
{
    public class SourceParser
    {
        #region Constructors
        public SourceParser()
        {
        }
        #endregion

        #region API
        public SrcMethod Parse( string aText )
        {
            ParserSrcMethod methodParser = new ParserSrcMethod();
            SrcMethod method = methodParser.Parse( ref aText );

			// And then parse what's left as the class
            if ( method != null )
            {
                ParserSrcClass classParser = new ParserSrcClass();
                SrcClass classObject = classParser.Parse( ref aText );
                //
                classObject.AddMethod( method );
            }

            return method;
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
