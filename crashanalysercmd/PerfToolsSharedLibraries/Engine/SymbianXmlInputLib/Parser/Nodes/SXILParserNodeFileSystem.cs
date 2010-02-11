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
using SymbianXmlInputLib.Elements.Types.FileSystem;

namespace SymbianXmlInputLib.Parser.Nodes
{
    public class SXILParserNodeFileSystem : SXILParserNode
    {
        #region Constructors
        public SXILParserNodeFileSystem()
        {
        }
        #endregion

        #region From SXILParserNode
        public override bool CanHandle( XmlNode aNode )
        {
            bool ret = base.CanHandle( aNode );
            //
            if ( !ret && aNode.NodeType == XmlNodeType.Element )
            {
                string name = aNode.Name.ToUpper();
                if ( name == "FILE" || name == "DIRECTORY" )
                {
                    int count = aNode.Attributes.Count;
                    if ( count == 1 && aNode.Attributes[ "name" ] != null )
                    {
                        ret = true;
                    }
                }
            }
            //
            return ret;
        }

        public override bool IsMulti
        {
            get { return true; }
        }

        public override void XmlParse( XmlNode aNode )
        {
            string type = aNode.Name;
            XmlAttribute name = aNode.Attributes[ "name" ];
            //
            if ( type == "file" )
            {
                string value = name.Value.Trim();
                base.Document.CurrentNode.Add( new SXILElementFile( new FileInfo( value ) ) );
            }
            else if ( type == "directory" )
            {
                string value = name.Value.Trim();
                base.Document.CurrentNode.Add( new SXILElementDirectory( new DirectoryInfo( value ) ) );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion
    }
}
