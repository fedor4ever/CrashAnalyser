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

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Stacks
{
    internal class CXmlStackEntry : CXmlNode
	{
		#region Constructors
        public CXmlStackEntry( CIStackEntry aEntry )
            : base( SegConstants.Stacks_Stack_Data_Entry )
		{
            iEntry = aEntry;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            if ( !iEntry.IsRegisterBasedEntry )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnAddress, iEntry.Address.ToString( "x8" ) );
            }
            //
            aParameters.Writer.WriteElementString( SegConstants.CmnValue, iEntry.Data.ToString( "x8" ) );
            aParameters.Writer.WriteElementString( SegConstants.CmnText, iEntry.DataAsString );
            if ( iEntry.FunctionOffset != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.Stacks_Stack_Data_Offset, iEntry.FunctionOffset.ToString( "x4" ) );
            }
            //
            if ( iEntry.IsRegisterBasedEntry )
            {
                // Get the corresponding register from the stack
                CIStack stack = iEntry.Stack;
                CIRegister register = iEntry.Register;
                if ( register != null )
                {
                    CXmlNode.WriteLink( register.Id, SegConstants.Registers, aParameters.Writer );
                }
            }

            if ( iEntry.Symbol != null )
            {
                CXmlNode.WriteLink( iEntry.Symbol.Id, SegConstants.Symbols, aParameters.Writer );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( SegConstants.CmnAttributes );
            //
            if ( iEntry.IsCurrentStackPointerEntry )
            {
                aParameters.Writer.WriteElementString( SegConstants.Stacks_Stack_Data_Entry_Attributes_CurrentStackPointer, string.Empty );
            }
            if ( iEntry.IsAccurate )
            {
                aParameters.Writer.WriteElementString( SegConstants.Stacks_Stack_Data_Entry_Attributes_Accurate, string.Empty );
            }
            if ( iEntry.IsRegisterBasedEntry )
            {
                aParameters.Writer.WriteElementString( SegConstants.Stacks_Stack_Data_Entry_Attributes_FromRegister, string.Empty );
            }
            else if ( iEntry.IsOutsideCurrentStackRange )
            {
                aParameters.Writer.WriteElementString( SegConstants.Stacks_Stack_Data_Entry_Attributes_OutsideBounds, string.Empty );
            }
            //
            aParameters.Writer.WriteEndElement();
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIStackEntry iEntry;
		#endregion
	}
}
