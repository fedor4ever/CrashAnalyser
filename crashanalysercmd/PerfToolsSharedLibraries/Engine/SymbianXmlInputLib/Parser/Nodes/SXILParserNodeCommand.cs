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
using SymbianXmlInputLib.Elements.Types.Command;

namespace SymbianXmlInputLib.Parser.Nodes
{
    public class SXILParserNodeCommand : SXILParserNode
    {
        #region Constructors
        public SXILParserNodeCommand()
        {
        }

        public SXILParserNodeCommand( string aDescription )
            : base( aDescription )
        {
        }
        #endregion

        #region From SXILParserNode
        public override bool IsMulti
        {
            get
            {
                return true;
            }
        }

        public override void XmlParse( XmlNode aNode )
        {
            XmlAttributeCollection attribs = aNode.Attributes;
            if ( attribs.Count < 1 || attribs[ "name" ] == null )
            {
                throw new ArgumentException( "Mandatory name node missing" );
            }

            XmlAttribute nameAttribute = attribs[ "name" ];
            string name = nameAttribute.Value.Trim();

            // Get details if present
            string details = aNode.InnerText.Trim();

            SXILElementCommand command = new SXILElementCommand( name, details );
            base.Document.CurrentNode.Add( command );

            iHandled = true;
        }

        public override bool CanHandle( XmlNode aNode )
        {
            bool ret = false;
            
            // We must support categories that contain multiple commands
            // therefore we must provide a means of preventing an already-populated command from
            // processing multiple xml tags.
            if ( !iHandled )
            {
                // This command instance has not yet successfully processed a command, so
                // it's okay to at least attempt to handle it - providing the xml tag is
                // our expected "command" name.
                ret = ( aNode.Name.ToUpper() == KXmlTagName );
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private const string KXmlTagName = "COMMAND";
        #endregion

        #region Data members
        private bool iHandled = false;
        #endregion
    }
}
