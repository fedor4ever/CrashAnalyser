/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Entry;

namespace CrashDebuggerLib.Structures.UserContextTable
{
    internal class UserContextTableEntry
    {
        #region Enumerations
        public enum TType
		{
		    EUndefined,				/**< register is not available */
		    EOffsetFromSp,			/**< iOffset is offset from stack pointer */
		    EOffsetFromStackTop,	/**< iOffset is offset from stack top */
		    ESpPlusOffset,			/**< value = SP + offset */
        }
        #endregion

        #region Constructors
        public UserContextTableEntry()
        {
        }
        #endregion

        #region API
        public bool IsAvailable( bool aIsCurrentThread )
        {
            bool ret = false;
            //
            switch ( Type )
            {
            case TType.EOffsetFromSp:
                // Not allowed when it's the current thread
                ret = ( !aIsCurrentThread );
                break;
            case TType.EOffsetFromStackTop:
                // Always allowed
                ret = true;
                break;
            case TType.ESpPlusOffset:
                // Not allowed when it's the current thread
                ret = ( !aIsCurrentThread );
                break;
            default:
            case TType.EUndefined:
                break;
            }
            //
            return ret;
        }

        public uint Process( ArmRegister aSp, DataBuffer aStackData )
        {
            uint ret = 0;
            //
            switch ( Type )
            {
            case TType.EUndefined:
                throw new NotSupportedException();
            case TType.EOffsetFromSp:
                ret = UpdateUsingOffsetFromSp( aSp, aStackData );
                break;
            case TType.EOffsetFromStackTop:
                ret = UpdateUsingOffsetFromStackTop( aSp, aStackData );
                break;
            case TType.ESpPlusOffset:
                ret = UpdateUsingSpPlusOffset( aSp, aStackData );
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
            set { iType = value; }
        }

        public byte Offset
        {
            get { return iOffset; }
            set { iOffset = value; }
        }

        public uint OffsetAsDWord
        {
            get { return (uint) Offset * 4; }
        }
        #endregion

        #region Internal methods
        private uint UpdateUsingOffsetFromSp( ArmRegister aSp, DataBuffer aStackData )
        {
            uint sp = aSp;
            uint offset = OffsetAsDWord;
            uint fetchAddr = offset + sp;
            DataBufferUint val = aStackData[ fetchAddr ];
            return val;
        }

        private uint UpdateUsingOffsetFromStackTop( ArmRegister aSp, DataBuffer aStackData )
        {
            uint stackTop = aStackData.Last.Address + 1;
            uint offset = OffsetAsDWord;
            uint fetchAddr = stackTop - offset;
            DataBufferUint val = aStackData[ fetchAddr ];
            return val;
        }

        private uint UpdateUsingSpPlusOffset( ArmRegister aSp, DataBuffer aStackData )
        {
            uint sp = aSp;
            uint offset = OffsetAsDWord;
            uint val = offset + sp;
            return val;
        }
        #endregion

        #region Data members
        private TType iType = TType.EUndefined;
        private byte iOffset = 0;
        #endregion
    }
}
