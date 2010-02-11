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
    public class XRefEngine
    {
        #region Constructors
        public XRefEngine()
        {
        }
        #endregion

        #region API
        public bool Contains( XRefIdentifer aIdentifier )
        {
            bool ret = false;
            //
            foreach ( XRefIdentifer identifier in iIdentifiers )
            {
                if ( identifier.Identifier == aIdentifier.Identifier )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public void ParseIdentifiers( string aText )
        {
            XRefIdentiferExtractor extractor = new XRefIdentiferExtractor( aText );

            // Add items, checking for dupes
            foreach ( XRefIdentifer identifier in extractor.Identifiers )
            {
                if ( !Contains( identifier ) )
                {
                    iIdentifiers.Add( identifier );
                }
            }
        }
        #endregion

        #region Properties
        public List<XRefIdentifer> Identifiers
        {
            get { return iIdentifiers; }
        }
        #endregion

        #region Data members
        private readonly List<XRefIdentifer> iIdentifiers = new List<XRefIdentifer>();
        #endregion
    }
}
