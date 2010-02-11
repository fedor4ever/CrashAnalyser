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
using System.Xml;
using System.Collections.Generic;
using CrashItemLib.Crash.Container;

namespace CAPCrashAnalysis.CommandLine
{
	internal class CACmdLineManifestWriter
	{
        #region Constructors
        public CACmdLineManifestWriter( CACmdLineFSEntityList<CACmdLineFileSource> aSourceFiles )
		{
            iSourceFiles = aSourceFiles;
		}
        #endregion

		#region API
        public string Create()
        {
            // This is where the XML will be stored
            StringBuilder backBuffer = new StringBuilder();

            // Create XML writer
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "   ";
            settings.NewLineChars = System.Environment.NewLine;
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.Encoding = Encoding.UTF8;

            using ( XmlWriter writer = XmlWriter.Create( backBuffer, settings ) )
            {
                // Serialise manifest for each file
                writer.WriteStartElement( KXmlRoot );

                foreach ( CACmdLineFileSource file in iSourceFiles )
                {
                    System.Diagnostics.Debug.Assert( file.ContainerCount >= 1 );

                    foreach ( CACmdLineFileSource.OutputEntry outputEntry in file.Outputs )
                    {
                        CIContainer container = outputEntry.Container;
                        string outputFile = outputEntry.OutputFileName;

                        writer.WriteStartElement( KXmlNodeReport );

                        // Input element
                        writer.WriteStartElement( KXmlNodeReportInput );
                        writer.WriteAttributeString( KXmlCmnName, file.Name );
                        writer.WriteEndElement();

                        // Output element
                        if ( !string.IsNullOrEmpty( outputFile ) )
                        {
                            writer.WriteStartElement( KXmlNodeReportOutput );
                            writer.WriteAttributeString( KXmlCmnName, outputEntry.OutputFileName );
                            writer.WriteEndElement();
                        }

                        // Status
                        string status = KXmlNodeReportStatusSuccess;
                        if ( outputEntry.Status == TOutputStatus.EFailed )
                        {
                            status = KXmlNodeReportStatusFailure;
                        }
                        writer.WriteElementString( KXmlNodeReportStatus, status );

                        // Messages
                        foreach ( CACmdLineMessage msg in outputEntry )
                        {
                            string typeName = KXmlNodeReportMsgMessage;
                            switch ( msg.Type )
                            {
                            case CACmdLineMessage.TType.ETypeDiagnostic:
                                typeName = KXmlNodeReportMsgDiagnostic;
                                break;
                            case CACmdLineMessage.TType.ETypeWarning:
                                typeName = KXmlNodeReportMsgWarning;
                                break;
                            case CACmdLineMessage.TType.ETypeError:
                                typeName = KXmlNodeReportMsgError;
                                break;
                            default:
                                break;
                            }
                            writer.WriteStartElement( typeName );
                            writer.WriteAttributeString( KXmlNodeReportMsgTitle, msg.Title );
                            writer.WriteString( msg.Description );
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }

            return backBuffer.ToString();
        }
        #endregion

		#region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KXmlRoot = "summary";
        private const string KXmlCmnName = "name";

        private const string KXmlNodeReport = "report";
        private const string KXmlNodeReportInput = "input";
        private const string KXmlNodeReportOutput = "output";
        private const string KXmlNodeReportStatus = "status";
        private const string KXmlNodeReportStatusSuccess = "OK";
        private const string KXmlNodeReportStatusFailure = "ERROR";
        private const string KXmlNodeReportMsgTitle = "title";
        private const string KXmlNodeReportMsgError = "error";
        private const string KXmlNodeReportMsgWarning = "warning";
        private const string KXmlNodeReportMsgMessage= "message";
        private const string KXmlNodeReportMsgDiagnostic = "diagnostic";
        #endregion

        #region Data members
        private readonly CACmdLineFSEntityList<CACmdLineFileSource> iSourceFiles;
        #endregion
	}
}
