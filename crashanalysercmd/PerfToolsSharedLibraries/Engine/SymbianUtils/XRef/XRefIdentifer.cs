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

namespace SymbianUtils.XRef
{
    public class XRefIdentifer : IComparable<XRefIdentifer>
    {
        #region Constructors
        public XRefIdentifer( string aIdentifier )
        {
            iIdentifier = aIdentifier;
        }
        #endregion

        #region Properties
        public string Identifier
        {
            get { return iIdentifier; }
        }
        #endregion

        #region Data members
        private readonly string iIdentifier;
        #endregion

        #region From IComparable<XRefIdentifer>
        public int CompareTo( XRefIdentifer aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = aOther.Identifier.CompareTo( Identifier );
            }
            //
            return ret;
        }
        #endregion
    }
}
