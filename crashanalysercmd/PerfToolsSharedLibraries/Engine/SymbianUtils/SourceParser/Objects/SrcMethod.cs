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

namespace SymbianUtils.SourceParser.Objects
{
    public class SrcMethod
    {
        #region Constructors
        public SrcMethod()
        {
        }
        #endregion

        #region API
        public void AddParameter( SrcMethodParameter aParameter )
        {
            iParameters.Add( aParameter );
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public string FullName
        {
            get { return ToString(); }
        }

        public string FullNameWithoutParameters
        {
            get
            {
                StringBuilder ret = new StringBuilder();

                if ( Class != null )
                {
                    ret.Append( Class );
                    ret.Append( SrcClass.KClassSeparator );
                }

                // Add method name itself
                ret.Append( Name );
                return ret.ToString();
            }
        }

        public SrcMethodModifier Modifier
        {
            get { return iModifier; }
            set { iModifier = value; }
        }

        public List<SrcMethodParameter> Parameters
        {
            get { return iParameters; }
        }

        public SrcClass Class
        {
            get { return iClass; }
            set { iClass = value; }
        }

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append( FullNameWithoutParameters );

            // Add parameters if present
            ret.Append( "(" );
            if ( Parameters.Count > 0 )
            {
                foreach ( SrcMethodParameter param in Parameters )
                {
                    ret.Append( param );
                    ret.Append( ", " );
                }

                // Remove redundant trailing comma
                ret.Remove( ret.Length - 2, 2 );
            }
            ret.Append( ")" );

            // Add the modifier
            ret.Append( Modifier );

            return ret.ToString();
        }
        #endregion

        #region Data members
        private string iName = string.Empty;
        private object iTag = null;
        private SrcClass iClass = null;
        private SrcMethodModifier iModifier = new SrcMethodModifier();
        private List<SrcMethodParameter> iParameters = new List<SrcMethodParameter>();
        #endregion
    }
}
