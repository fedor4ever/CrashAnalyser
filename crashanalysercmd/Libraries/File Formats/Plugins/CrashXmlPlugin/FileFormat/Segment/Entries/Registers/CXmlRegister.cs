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
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Special;
using CrashItemLib.Crash.Registers.Visualization;
using CrashItemLib.Crash.Registers.Visualization.Bits;
using CrashItemLib.Crash.Symbols;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Segment.Entries.Symbols;
using SymbianUtils.Enum;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Registers
{
    internal class CXmlRegister : CXmlNode
	{
		#region Constructors
        public CXmlRegister( CIRegister aRegister )
            : base( SegConstants.Registers_RegisterSet_Register )
		{
            iRegister = aRegister;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iRegister, aParameters.Writer );

            // Map the ArmRegister name onto the register names we expose via XML. In reality, the only register names
            // that need remapping are R13, R14 and R15.
            string regName = RemapRegisterName();
            aParameters.Writer.WriteElementString( SegConstants.CmnName, regName );
            aParameters.Writer.WriteElementString( SegConstants.CmnValue, iRegister.Value.ToString("x8") );

            if ( iRegister.Symbol != null && CXmlSymbol.IsSerializable( iRegister.Symbol ) )
            {
                CXmlNode.WriteLink( iRegister.Symbol.Id, SegConstants.Symbols, aParameters.Writer );
            }

            // Write any messages
            CXmlSegBase.XmlSerializeMessages( aParameters, iRegister );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIRegisterVisualization> visList = iRegister.ChildrenByType<CIRegisterVisualization>();
            foreach ( CIRegisterVisualization vis in visList )
            {
                CXmlNode.WriteLink( vis.Id, SegConstants.ValueInterpretation, aParameters.Writer );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private string RemapRegisterName()
        {
            string ret = iRegister.Name;
            //
            switch ( iRegister.Type )
            {
            case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_SP:
                ret = "R13";
                break;
            case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_LR:
                ret = "R14";
                break;
            case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_PC:
                ret = "R15";
                break;
            default:
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly CIRegister iRegister;
		#endregion
	}
}
