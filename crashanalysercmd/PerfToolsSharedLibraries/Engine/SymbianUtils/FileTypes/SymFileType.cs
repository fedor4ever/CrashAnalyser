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

namespace SymbianUtils.FileTypes
{
    public class SymFileType
    {
        #region Constructors
        public SymFileType( string aExtension, string aDescription )
        {
            iExtension = aExtension;
            iDescription = aDescription;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Extension
        {
            get { return iExtension; }
        }

        public string Description
        {
            get { return iDescription; }
        }
        #endregion

        #region From System.Object
        public override bool Equals( object aObject )
        {
            if ( aObject != null )
            {
                if ( aObject is SymFileType )
                {
                    SymFileType type = (SymFileType) aObject;
                    return string.Compare( Extension, type.Extension, StringComparison.CurrentCultureIgnoreCase ) == 0;
                }
            }
            //
            return base.Equals( aObject );
        }

        public override int GetHashCode()
        {
            return Extension.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.AppendFormat( "{0} ({1})|{2}", Description, Extension, Extension );
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly string iExtension;
        private readonly string iDescription;
        #endregion
    }
}
