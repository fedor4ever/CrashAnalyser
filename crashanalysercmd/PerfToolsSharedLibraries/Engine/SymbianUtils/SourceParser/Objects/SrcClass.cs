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
    public class SrcClass
    {
        #region Enumerations
        public enum TType
        {
            ETypeGlobalFunctionClass = 0,
            ETypeProperClass
        }
        #endregion

        #region Constructors
        public SrcClass()
        {
        }
        #endregion

        #region Constants
        public const string KClassSeparator = "::";
        #endregion

        #region API
        public void AddMethod( SrcMethod aMethod )
        {
            aMethod.Class = this;
            iMethods.Add( aMethod );
        }
        #endregion

        #region Properties
        public TType Type
        {
            get
            {
                TType ret = TType.ETypeGlobalFunctionClass;
                //
                if ( Name.Length != 0 )
                {
                    ret = TType.ETypeProperClass;
                }
                //
                return ret;
            }
        }

        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public List<SrcMethod> Methods
        {
            get { return iMethods; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( Name );
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private string iName = string.Empty;
        private List<SrcMethod> iMethods = new List<SrcMethod>();
        #endregion
    }
}
