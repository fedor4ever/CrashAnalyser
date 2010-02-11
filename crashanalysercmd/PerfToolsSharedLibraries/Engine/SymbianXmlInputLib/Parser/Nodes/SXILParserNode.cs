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
using SymbianTree;
using SymbianUtils;
using SymbianXmlInputLib.Elements;

namespace SymbianXmlInputLib.Parser.Nodes
{
    public abstract class SXILParserNode
    {
        #region Constructors
        protected SXILParserNode()
            : this( string.Empty )
        {
        }

        protected SXILParserNode( string aDescription )
        {
            iDescription = aDescription;
        }
        #endregion

        #region Framework API
        public virtual void XmlParse( XmlNode aNode )
        {
            foreach ( XmlNode child in aNode.ChildNodes )
            {
                iChildren.XmlParse( child, Parser );
            }
        }

        public virtual bool CanHandle( XmlNode aNode )
        {
            return false;
        }

        public virtual bool IsMulti
        {
            get { return false; }
        }
        #endregion

        #region Properties
        public SXILParserNodeCollection Children
        {
            get { return iChildren; }
            set { iChildren = value; }
        }

        protected SXILDocument Document
        {
            get { return Parser.Document; }
        }

        internal SXILParser Parser
        {
            get { return iParser; }
            set { iParser = value; }
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            int ret = 0;
            //
            if ( string.IsNullOrEmpty( iDescription ) )
            {
                ret = this.GetType().GetHashCode();
            }
            else
            {
                ret = iDescription.GetHashCode();
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly string iDescription;
        private SXILParser iParser = null;
        private SXILParserNodeCollection iChildren = new SXILParserNodeCollection();
        #endregion
    }
}
