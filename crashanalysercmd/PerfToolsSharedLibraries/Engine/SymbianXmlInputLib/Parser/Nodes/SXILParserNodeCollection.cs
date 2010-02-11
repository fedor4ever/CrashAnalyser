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
    public class SXILParserNodeCollection
    {
        #region Constructors
        public SXILParserNodeCollection()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            iNodes.Clear();
        }

        internal void Add( SXILParserNode aParser )
        {
            int hash = aParser.GetHashCode();
            if ( iNodes.ContainsKey( hash ) )
            {
                throw new Exception( "The specified parser node already exists" );
            }
            iNodes.Add( hash, aParser );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private SXILParserNode this[ string aName ]
        {
            get
            {
                SXILParserNode ret = null;
                //
                int hash = aName.GetHashCode();
                if ( iNodes.ContainsKey( hash ) )
                {
                    ret = iNodes[ hash ];
                }
                //
                return ret;
            }
        }

        internal bool XmlParse( XmlNode aNode, SXILParser aMasterParser )
        {
            bool handled = false;
            //
            SXILParserNode child = this[ aNode.Name ];
            if ( child != null )
            {
                child.Parser = aMasterParser;
                child.XmlParse( aNode );
                handled = true;
            }
            else
            {
                // No direct name-based match, check for multi entries
                foreach ( KeyValuePair<int, SXILParserNode> kvp in iNodes )
                {
                    SXILParserNode parserNode = kvp.Value;
                    bool isMulti = parserNode.IsMulti;
                    if ( isMulti )
                    {
                        bool canHandle = parserNode.CanHandle( aNode );
                        if ( canHandle )
                        {
                            parserNode.Parser = aMasterParser;
                            parserNode.XmlParse( aNode );
                            handled = true;
                            break;
                        }
                    }
                }
            }
            //
            return handled;
        }
        #endregion

        #region Internal methods
        private Dictionary<int, SXILParserNode> iNodes = new Dictionary<int, SXILParserNode>();
        #endregion
    }
}
