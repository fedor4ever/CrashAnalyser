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

namespace SymbianXmlInputLib.Elements.Types.Extension
{
    public class SXILElementExtension : SXILElement
    {
        #region Enumerations
        public enum TType
        {
            ETypeSuccess = 0,
            ETypeFailure
        }
        #endregion

        #region Constructors
        internal SXILElementExtension()
            : this( string.Empty, TType.ETypeSuccess )
        {
        }

        internal SXILElementExtension( string aName, TType aType )
        {
            Name = aName;
            Type = aType;
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

        public TType Type
        {
            get { return iType; }
            set { iType = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private string iName = string.Empty;
        private TType iType = TType.ETypeSuccess;
        #endregion
    }
}
