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
using SymbianUtils.Range;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;

namespace SymbianDebugLib.PluginAPI.Types.Code
{
    public abstract class DbgViewCode : DbgPluginView, IArmInstructionProvider
    {
        #region Constructors
        protected DbgViewCode( string aName, DbgPluginEngine aEngine )
            : base( aName, aEngine )
        {
        }
        #endregion

        #region From IArmInstructionProvider
        public uint GetDataUInt32( uint aAddress )
        {
            uint ret = 0;
            IArmInstruction[] inst = null;
            //
            bool available = GetInstructions( aAddress, TArmInstructionSet.EARM, 1, out inst );
            if ( available && inst.Length >= 1 )
            {
                ret = inst[ 0 ].AIRawValue;
            }
            //
            return ret;
        }

        public ushort GetDataUInt16( uint aAddress )
        {
            ushort ret = 0;
            IArmInstruction[] inst = null;
            //
            bool available = GetInstructions( aAddress, TArmInstructionSet.ETHUMB, 1, out inst );
            if ( available && inst.Length >= 1 )
            {
                ret = inst[ 0 ].AIRawValue;
            }
            //
            return ret;
        }

        public bool IsInstructionAddressValid( uint aAddress )
        {
            return Contains( aAddress );
        }

        public bool GetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            return DoGetInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
        }
        #endregion

        #region Framework API
        public abstract CodeCollection ActivateAndGetCollection( CodeSegDefinition aCodeSegment );

        public abstract IArmInstruction ConvertToInstruction( uint aAddress, TArmInstructionSet aInstructionSet, uint aRawValue );

        protected abstract bool DoGetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions );
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
