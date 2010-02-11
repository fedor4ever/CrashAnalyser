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
using System.Xml;
using System.IO;
using CrashItemLib.Sink;
using SymbianUtils;

namespace CrashXmlPlugin.FileFormat.Document
{
    internal class CXmlDocumentSerializationParameters : CISinkSerializationParameters
    {
        #region Constructors
        public CXmlDocumentSerializationParameters( CISinkSerializationParameters aCopy, CXmlDocumentRoot aDocument )
            : base( aCopy )
        {
            PrepareDefaultExtensions();
            iDocument = aDocument;
            //
            Stream stream = base.CreateFile( out iFileName );
            //
            XmlWriterSettings settings = CreateWriterSettings();
            settings.CloseOutput = true;
            //
            iWriter = XmlWriter.Create( stream, settings );
        }

        public CXmlDocumentSerializationParameters( CISinkSerializationParameters aCopy, CXmlDocumentRoot aDocument, StringBuilder aBuilder )
            : base( aCopy )
        {
            PrepareDefaultExtensions();
            iDocument = aDocument;
            //
            XmlWriterSettings settings = CreateWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.OmitXmlDeclaration = true;
            //
            iWriter = XmlWriter.Create( aBuilder, settings );
            iFileName = string.Empty;
        }
        #endregion

        #region From 
        protected override void PrepareDefaultExtensions()
        {
            if ( string.IsNullOrEmpty( base.FileExtensionFailed ) )
            {
                base.FileExtensionFailed = KXmlDefaultExtensionFailure;
            }
            if ( string.IsNullOrEmpty( base.FileExtensionSuccess ) )
            {
                base.FileExtensionSuccess = KXmlDefaultExtensionSuccess;
            }
        }
        #endregion

        #region Properties
        public CXmlDocumentRoot Document
        {
            get { return iDocument; }
        }

        public XmlWriter Writer
        {
            get { return iWriter; }
        }

        public string FileName
        {
            get { return iFileName; }
        }
        #endregion

        #region Internal constants
        private const string KXmlDefaultExtensionSuccess = ".xml";
        private const string KXmlDefaultExtensionFailure = ".failedxml";
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                iWriter.Flush();
                iWriter.Close();
            }
        }
        #endregion

        #region Internal methods
        private static XmlWriterSettings CreateWriterSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "   ";
            settings.NewLineHandling = NewLineHandling.None;
            settings.Encoding = Encoding.UTF8;
            //
            return settings;
        }
        #endregion

        #region Data members
        private readonly string iFileName;
        private readonly XmlWriter iWriter;
        private readonly CXmlDocumentRoot iDocument;
        #endregion
    }
}
