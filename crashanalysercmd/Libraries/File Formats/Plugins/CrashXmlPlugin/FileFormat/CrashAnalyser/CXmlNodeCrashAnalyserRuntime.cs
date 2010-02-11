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
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using CrashItemLib.Sink;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Versions;
using SymbianUtils.Enum;

namespace CrashXmlPlugin.FileFormat.CrashAnalyser
{
    internal class CXmlNodeCrashAnalyserRuntime : CXmlNode
	{
		#region Constructors
        public CXmlNodeCrashAnalyserRuntime()
            : base( Constants.CrashAnalyser_Runtime )
        {
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // UI Version
            CXmlVersionExtended version = new CXmlVersionExtended( aParameters.UIVersion );
            version.XmlSerialize( aParameters );

            // Analysis type
            string analysisType = EnumUtils.ToString( aParameters.DetailLevel );
            aParameters.Writer.WriteElementString( Constants.CrashAnalyser_Runtime_AnalysisType, analysisType );

            // Command line
            string commandLine = aParameters.UICommandLineArguments;
            aParameters.Writer.WriteElementString( Constants.CrashAnalyser_Runtime_CommandLine, commandLine );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( Constants.CrashAnalyser_Runtime_InputFiles );
            
            // Get the file names & sort them
            List<string> files = new List<string>();
            files.AddRange( aParameters.Container.FileNames );
            files.Sort();

            // Output them
            foreach ( string file in files )
            {
                aParameters.Writer.WriteElementString( Constants.CrashAnalyser_Runtime_InputFiles_File, file );
            }
            //
            aParameters.Writer.WriteEndElement();
        }
        #endregion
            
        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
