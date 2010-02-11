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
using SymbianXmlInputLib.Parser.Nodes;

namespace SymbianXmlInputLib.Parser
{
    public class SXILParser : DisposableObject
    {
        #region Constructors
        public SXILParser( string aFileName, string aRootNodeName, SXILDocument aDocument )
        {
            iFileName = aFileName;
            iDocument = aDocument;
            iRootNodeName = aRootNodeName;
            //
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.IgnoreComments = true;
            settings.CheckCharacters = true;
            settings.IgnoreWhitespace = true;
            //
            iReader = XmlReader.Create( iFileName, settings );
            iXmlDocument.Load( iReader );
        }
        #endregion

        #region API
        public void Parse()
        {
            ValidateRoot();
            ParseTopLevelNodes();
        }

        public void CategoryAdd( string aName, SXILParserNodeCollection aParsers )
        {
            SXILParserNodeCategory category = new SXILParserNodeCategory( aName );
            category.Children = aParsers;
            iParsers.Add( category );
        }

        public void CategoryAdd( string aName, params SXILParserNode[] aParserNodes )
        {
            SXILParserNodeCategory category = new SXILParserNodeCategory( aName );
            foreach ( SXILParserNode parserNode in aParserNodes )
            {
                category.Children.Add( parserNode );
            }
            iParsers.Add( category );
        }
        #endregion

        #region Properties
        internal SXILDocument Document
        {
            get { return iDocument; }
        }
        #endregion

        #region Internal methods
        private void ValidateRoot()
        {
            XmlElement root = iXmlDocument.DocumentElement;
            if ( root != null && root.Name != iRootNodeName )
            {
                throw new Exception( "Document Root Incorrect" );
            }
        }

        private void ParseTopLevelNodes()
        {
            foreach ( XmlNode child in iXmlDocument.DocumentElement )
            {
                bool handled = iParsers.XmlParse( child, this );
                if ( handled )
                {
                }
            }
        }

        private void OnElementStart()
        {
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            base.CleanupManagedResources();
            iReader.Close();
        }
        #endregion

        #region Data members
        private readonly string iFileName;
        private readonly string iRootNodeName;
        private readonly SXILDocument iDocument;
        private readonly XmlReader iReader;
        private XmlDocument iXmlDocument = new XmlDocument();
        private SXILParserNodeCollection iParsers = new SXILParserNodeCollection();
        #endregion
    }
}
