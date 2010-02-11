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
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace SymbianXmlInputLib.Elements.Types.Command
{
    public class SXILElementCommand : SXILElement
    {
        #region Constructors
        internal SXILElementCommand()
            : this( string.Empty )
        {
        }

        internal SXILElementCommand( string aName )
            : this( aName, string.Empty )
        {
        }

        internal SXILElementCommand( string aName, string aDetails )
        {
            iName = aName;
            iDetails = aDetails;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public string Details
        {
            get { return iDetails; }
            set { iDetails = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private string iName = string.Empty;
        private string iDetails = string.Empty;
        #endregion
    }
}
