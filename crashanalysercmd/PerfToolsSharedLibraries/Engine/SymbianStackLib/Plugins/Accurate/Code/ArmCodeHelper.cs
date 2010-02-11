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
using SymbianUtils.Tracer;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStackAlgorithmAccurate.Engine;
using SymbianStackAlgorithmAccurate.Instructions;

namespace SymbianStackAlgorithmAccurate.Code
{
    internal class ArmCodeHelper
    {
        #region Constructors
        public ArmCodeHelper( DbgViewCode aDebugViewCode, ITracer aTracer )
        {
            iCodeView = aDebugViewCode;
            iTracer = aTracer;
        }
        #endregion

        #region API
        public uint LoadData( uint aAddress )
        {
            uint ret = iCodeView.GetDataUInt32( aAddress );
            return ret;
        }

        public AccInstructionList LoadInstructions( uint aAddress, int aCount, TArmInstructionSet aType )
        {
            AccInstructionList ret = new AccInstructionList();
            
            // Get list of instructions from code engine
            IArmInstruction[] basicInstructions = null;
            bool available = iCodeView.GetInstructions( aAddress, aType, aCount, out basicInstructions );
            if ( available )
            {
                // Convert the basic instructions into the different types
                // we can handle
                ret.AddRange( basicInstructions );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        private readonly DbgViewCode iCodeView;
        #endregion
    }
}
