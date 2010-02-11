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
using SymbianXmlInputLib.Elements.Types.Extension;

namespace SymbianXmlInputLib.Parser.Nodes
{
    public class SXILParserNodeExtension : SXILParserNode
    {
        #region Constructors
        public SXILParserNodeExtension()
        {
        }
        #endregion

        #region From SXILParserNode
        public override void XmlParse( XmlNode aNode )
        {
            XmlAttributeCollection attribs = aNode.Attributes;
            if ( attribs.Count < 1 || attribs[ "name" ] == null )
            {
                throw new ArgumentException( "Mandatory name node missing" );
            }

            XmlAttribute nameAttrib = attribs[ "name" ];
            string name = nameAttrib.Value.Trim();

            SXILElementExtension.TType type = SXILElementExtension.TType.ETypeSuccess;
            if ( attribs[ "type" ] != null )
            {
                string typeName = attribs[ "type" ].Value.Trim().ToUpper();
                if ( typeName == "FAILED") 
                {
                    type = SXILElementExtension.TType.ETypeFailure;
                }
            }

            SXILElementExtension entry = new SXILElementExtension( name, type );
            base.Document.CurrentNode.Add( entry );
        }
        #endregion

        #region Properties
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return "extension".GetHashCode();
        }
        #endregion
    }
}
