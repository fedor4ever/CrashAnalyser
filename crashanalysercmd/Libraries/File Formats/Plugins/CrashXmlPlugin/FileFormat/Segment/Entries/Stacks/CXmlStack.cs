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
using CrashXmlPlugin.FileFormat.Node;
using MobileCrashLib;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Stacks
{
    internal class CXmlStack : CXmlNode
	{
		#region Constructors
        public CXmlStack( CIStack aStack )
            : base( SegConstants.Stacks_Stack )
		{
            iStack = aStack;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iStack, aParameters.Writer );

            if ( iStack.Registers != null )
            {
                CXmlNode.WriteLink( iStack.Registers.Id, SegConstants.Registers, aParameters.Writer );
            }
            if ( iStack.OwningThread != null )
            {
                CXmlNode.WriteLink( iStack.OwningThread.Id, SegConstants.Threads, aParameters.Writer );
            }

            aParameters.Writer.WriteElementString( SegConstants.CmnBase, iStack.Base.ToString("x8") );
            aParameters.Writer.WriteElementString( SegConstants.CmnSize, iStack.Size.ToString( "x" ) );
            aParameters.Writer.WriteElementString( SegConstants.CmnRange, iStack.Range.ToString() );

            // Write defect hash
            try
            {
                MobileCrashHashBuilder hashBuilder = MobileCrashHashBuilder.New(MobileCrashHashBuilder.TConfiguration.EDefault, iStack);
                if (hashBuilder != null)
                {
                    aParameters.Writer.WriteElementString(SegConstants.Stacks_Stack_Hash, hashBuilder.GetHash());
                }

                // Write detailed hash
                hashBuilder = MobileCrashHashBuilder.New(MobileCrashHashBuilder.TConfiguration.EDetailed, iStack, MobileCrashHashBuilder.KDetailedNumberOfStackEntriesToCheckForSymbols);
                
                if (hashBuilder != null)
                {
                    aParameters.Writer.WriteElementString(SegConstants.Stacks_Stack_Detailed_Hash, hashBuilder.GetHash());
                }
            }
            catch (Exception)
            {       
                // Could not create MobileCrashHashBuilder, ignore.
            }

            // Write any messages
            CXmlSegBase.XmlSerializeMessages( aParameters, iStack );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Algorithm name directly maps onto an attribute
            string attributeAlgorithm = string.Empty;
            if ( iStack.Algorithm.ToUpper().Contains( "ACCURATE" ) )
            {
                attributeAlgorithm = SegConstants.Stacks_Stack_Attributes_Accurate;
            }
            else if ( iStack.Algorithm.ToUpper().Contains( "HEURISTIC" ) )
            {
                attributeAlgorithm = SegConstants.Stacks_Stack_Attributes_Heuristic;
            }

            // Attributes
            if ( attributeAlgorithm != string.Empty )
            {
                aParameters.Writer.WriteStartElement( SegConstants.CmnAttributes );
                aParameters.Writer.WriteElementString( attributeAlgorithm, string.Empty );
                aParameters.Writer.WriteEndElement();
            }

            // Entries
            if ( aParameters.DetailLevel != CrashItemLib.Sink.CISinkSerializationParameters.TDetailLevel.ESummary )
            {
                System.Diagnostics.Debug.Assert( iStack.IsStackOutputAvailable );
                //
                aParameters.Writer.WriteStartElement( SegConstants.Stacks_Stack_Data );
                //
                foreach ( CIStackEntry entry in iStack )
                {
                    CXmlStackEntry xmlEntry = new CXmlStackEntry( entry );
                    xmlEntry.XmlSerialize( aParameters );
                }
                //
                aParameters.Writer.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIStack iStack;
		#endregion
	}
}
