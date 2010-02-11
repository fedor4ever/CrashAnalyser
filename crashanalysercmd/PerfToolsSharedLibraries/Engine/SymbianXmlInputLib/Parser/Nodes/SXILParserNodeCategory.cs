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
using SymbianXmlInputLib.Elements.Types.Category;

namespace SymbianXmlInputLib.Parser.Nodes
{
    public class SXILParserNodeCategory : SXILParserNode
    {
        #region Constructors
        public SXILParserNodeCategory( string aName )
        {
            iName = aName;
        }
        #endregion

        #region From SXILParserNode
        public override bool IsMulti
        {
            get { return true; }
        }

        public override bool CanHandle( XmlNode aNode )
        {
            bool ret = base.CanHandle( aNode );
            //
            if ( !ret )
            {
                string name = aNode.Name.Trim().ToUpper();
                if ( name == "CATEGORY" )
                {
                    // Is it our category?
                    foreach ( XmlAttribute attrib in aNode.Attributes )
                    {
                        name = attrib.Name.ToUpper();
                        if ( name == "NAME" )
                        {
                            string value = attrib.Value.Trim().ToUpper();
                            if ( value == iName.ToUpper() )
                            {
                                ret = true;
                                break;
                            }
                        }
                    }
                }
            }
            //
            return ret;
        }

        public override void XmlParse( XmlNode aNode )
        {
            System.Diagnostics.Debug.Assert( aNode.Name.ToUpper() == "CATEGORY" );
            System.Diagnostics.Debug.Assert( aNode.Attributes.Count >= 1 );

            SXILElementCategory category = new SXILElementCategory( iName );
            base.Document.AppendChild( category );
            base.Document.CurrentNode = category;
            //
            base.XmlParse( aNode );
            //
            base.Document.MakeParentCurrent();
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return iName.GetHashCode();
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private readonly string iName;
        #endregion
    }
}
