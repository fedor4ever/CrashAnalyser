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
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Visualization;
using CrashItemLib.Crash.Registers.Visualization.Bits;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.ValueInterpretations
{
    internal class CXmlValueInterpretation : CXmlNode
	{
		#region Constructors
        public CXmlValueInterpretation( CIRegisterVisualization aValue )
            : base( SegConstants.ValueInterpretation_Entry )
		{
            iValue = aValue;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iValue, aParameters.Writer );

            aParameters.Writer.WriteElementString( SegConstants.ValueInterpretation_Entry_Hex, iValue.Value.ToString("x8") );
            aParameters.Writer.WriteElementString( SegConstants.ValueInterpretation_Entry_Binary, iValue.Binary );
            aParameters.Writer.WriteElementString( SegConstants.CmnSize, iValue.Size.ToString() );

            // Endianness
            aParameters.Writer.WriteStartElement( SegConstants.ValueInterpretation_Entry_Endianness );
            aParameters.Writer.WriteAttributeString( SegConstants.CmnType, SegConstants.ValueInterpretation_Entry_Endianness_Little );
            aParameters.Writer.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Endianness_Bit0, SegConstants.ValueInterpretation_Entry_Endianness_Bit0_Right );
            aParameters.Writer.WriteEndElement();

            // Associated register
            CXmlNode.WriteLink( iValue.Register.Id, SegConstants.Registers, aParameters.Writer );
      
            // Description
            aParameters.Writer.WriteElementString( SegConstants.ValueInterpretation_Entry_Description, iValue.Description );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( SegConstants.ValueInterpretation_Entry_Interpretation );

            foreach ( CIElement child in iValue )
            {
                if ( child is CIRegisterVisBit )
                {
                    WriteBit( aParameters.Writer, child as CIRegisterVisBit );
                }
                else if ( child is CIRegisterVisBitGroup )
                {
                    WriteBitGroup( aParameters.Writer, child as CIRegisterVisBitGroup );
                }
                else if ( child is CIRegisterVisBitRange )
                {
                    WriteBitRange( aParameters.Writer, child as CIRegisterVisBitRange );
                }
            }
            
            aParameters.Writer.WriteEndElement();
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void WriteBit( XmlWriter aWriter, CIRegisterVisBit aItem )
        {
            aWriter.WriteStartElement( SegConstants.ValueInterpretation_Entry_Interpretation_Bit );

            // Index
            aWriter.WriteAttributeString( SegConstants.CmnIndex, aItem.Index.ToString() );

            // Value
            aWriter.WriteAttributeString( SegConstants.CmnValue, aItem.ToString() );

            // Category
            aWriter.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Category, aItem.Category );

            // Reserved
            if ( aItem.IsReserved )
            {
                aWriter.WriteAttributeString( SegConstants.CmnType, SegConstants.ValueInterpretation_Entry_Reserved );
            }

            // Single character interpretation
            aWriter.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Interpretation_Bit_Char, aItem.ValueCharacter );
 
            // Interpretation
            if ( !string.IsNullOrEmpty( aItem.Interpretation ) )
            {
                aWriter.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Interpretation, aItem.Interpretation );
            }

            aWriter.WriteEndElement();
        }

        private void WriteBitGroup( XmlWriter aWriter, CIRegisterVisBitGroup aItem )
        {
            aWriter.WriteStartElement( SegConstants.ValueInterpretation_Entry_Interpretation_BitGroup );

            foreach ( CIRegisterVisBit bit in aItem )
            {
                WriteBit( aWriter, bit );
            }

            aWriter.WriteEndElement();
        }

        private void WriteBitRange( XmlWriter aWriter, CIRegisterVisBitRange aItem )
        {
            aWriter.WriteStartElement( SegConstants.ValueInterpretation_Entry_Interpretation_BitRange );

            // Start & end
            aWriter.WriteAttributeString( SegConstants.CmnStart, aItem.Range.Min.ToString() );
            aWriter.WriteAttributeString( SegConstants.CmnEnd, aItem.Range.Max.ToString() );

            // Value
            aWriter.WriteAttributeString( SegConstants.CmnValue, aItem.ToString() );
            
            // Category
            aWriter.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Category, aItem.Category );

            // Reserved
            if ( aItem.IsReserved )
            {
                aWriter.WriteAttributeString( SegConstants.CmnType, SegConstants.ValueInterpretation_Entry_Reserved );
            }

            // Interpretation
            string interpretation = aItem.Interpretation;
            if ( !string.IsNullOrEmpty( interpretation ) )
            {
                aWriter.WriteAttributeString( SegConstants.ValueInterpretation_Entry_Interpretation, interpretation );
            }

            aWriter.WriteEndElement();
        }
        #endregion

        #region Data members
        private readonly CIRegisterVisualization iValue;
		#endregion
	}
}
