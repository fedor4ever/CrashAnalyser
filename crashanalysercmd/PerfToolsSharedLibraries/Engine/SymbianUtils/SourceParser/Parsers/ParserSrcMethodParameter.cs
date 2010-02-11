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
    public class ParserSrcMethodParameter
    {
        #region Constructors
        public ParserSrcMethodParameter()
        {
        }
        #endregion

        #region API
        public void Parse( ref string aText, SrcMethod aMethod )
        {
            /*
             * TPtrC16::TPtrC16(const unsigned short*) 
             * TPtrC16::TPtrC16(const TDesC16&) 
             * UserHal::MemoryInfo(TDes8&) 
             * RHandleBase::Close() 
             * TBufCBase16::Copy(const TDesC16&, int) 
             * CBufFlat::NewL(int) 
             * TBufCBase16::TBufCBase16() 
             * CServer2::RunL() 
             * CServer2::StartL(const TDesC16&) 
             * CServer2::DoCancel() 
             * CServer2::RunError(int) 
             * CServer2::DoConnect(const RMessage2&) 
             * CServer2::CServer2__sub_object(int, CServer2::TServerType) 
             */
            string paramType = string.Empty;
            while ( aText.Length > 0 )
            {
                int commaPos = aText.IndexOf( "," );
                //
                paramType = aText;
                if ( commaPos > 0 )
                {
                    paramType = aText.Substring( 0, commaPos ).Trim();
                    if ( commaPos < aText.Length )
                        aText = aText.Substring( commaPos + 1 ).Trim();
                    else
                        aText = string.Empty;
                }
                else
                {
                    // Everything was consumed
                    aText = string.Empty;
                }

                // Should have the parameter same now. Make a new parameter
                SrcMethodParameter parameter = new SrcMethodParameter();
                parameter.Name = paramType;
                aMethod.AddParameter( parameter );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
