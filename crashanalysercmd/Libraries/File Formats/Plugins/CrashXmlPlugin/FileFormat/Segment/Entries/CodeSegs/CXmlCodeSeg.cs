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
using System.Collections.Generic;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.CodeSegs;
using SymbianStructuresLib.CodeSegments;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.CodeSegs
{
    internal class CXmlCodeSeg : CXmlNode
	{
		#region Constructors
        public CXmlCodeSeg( CICodeSeg aCodeSeg )
            : base( SegConstants.CodeSegs_CodeSeg )
		{
            iCodeSeg = aCodeSeg;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iCodeSeg, aParameters.Writer );
            aParameters.Writer.WriteElementString( SegConstants.CmnName, iCodeSeg.Name );
            aParameters.Writer.WriteElementString( SegConstants.CmnBase, iCodeSeg.Base.ToString( "x8" ) );
            aParameters.Writer.WriteElementString( SegConstants.CmnSize, iCodeSeg.Size.ToString( "x8" ) );
            aParameters.Writer.WriteElementString( SegConstants.CmnRange, iCodeSeg.Range.ToString() );

            if ( iCodeSeg.Checksum != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnChecksum, iCodeSeg.Checksum.ToString( "x8" ) );
            }

            // Write any messages
            CXmlSegBase.XmlSerializeMessages( aParameters, iCodeSeg );
        }
        
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( SegConstants.CmnAttributes );
            //
            CodeSegDefinition codeSegDef = (CodeSegDefinition) iCodeSeg;
            if ( ( codeSegDef.Attributes & CodeSegDefinition.TAttributes.EAttributeXIP ) == CodeSegDefinition.TAttributes.EAttributeXIP )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnXIP, string.Empty );
            }
            else if ( ( codeSegDef.Attributes & CodeSegDefinition.TAttributes.EAttributeRAM ) == CodeSegDefinition.TAttributes.EAttributeRAM )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnRAM, string.Empty );
            }
            if ( !iCodeSeg.IsResolved )
            {
                aParameters.Writer.WriteElementString( SegConstants.CodeSegs_CodeSeg_Attributes_NoSymbols, string.Empty );
            }
            if ( iCodeSeg.IsSpeculative )
            {
                aParameters.Writer.WriteElementString( SegConstants.CodeSegs_CodeSeg_Attributes_Speculative, string.Empty );
            }
            if ( iCodeSeg.IsMismatched )
            {
                aParameters.Writer.WriteElementString( SegConstants.CodeSegs_CodeSeg_Attributes_Mismatch, string.Empty );
            }
            //
            aParameters.Writer.WriteEndElement();
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CICodeSeg iCodeSeg;
		#endregion
	}
}
