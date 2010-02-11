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
using System.Text.RegularExpressions;
using System.IO;
using SymbianStructuresLib.Arm.Registers;

namespace SymbianInstructionLib.Arm.Instructions.Common
{
    internal static class ArmInstructionUtils
    {
        public static int SignExtend24BitTo32Bit( uint aImmediate )
        {
            int offset;
            //
            unchecked
            {
                if ( ( aImmediate & 0x00800000 ) == 0x00800000 )
                {
                    offset = (int) ( 0xff000000 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= 2;
            offset += 8; // pipeline
            return offset;
        }

        public static List<TArmRegisterType> ExtractGPRegisterList( uint aEncodedList )
        {
            List<TArmRegisterType> ret = new List<TArmRegisterType>();
            //
            for ( int i = 15; i >= 0; i-- )
            {
                uint mask = (uint) ( 1 << i );
                if ( ( aEncodedList & mask ) == mask )
                {
                    ret.Add( (TArmRegisterType) i );
                }
            }
            //
            return ret;
        }
    }

    internal static class ThumbInstructionUtils
    {
        public static int SignExtend11BitTo32Bit( uint aImmediate )
        {
            int offset = SignExtend11BitTo32Bit( aImmediate, 1 );
            offset += 4; // pipeline
            return offset;
        }

        public static int SignExtend11BitTo32Bit( uint aImmediate, int aLeftShiftCount )
        {
            int offset;
            //
            unchecked
            {
                //  10  9  8  7  6  5  4  3  2  1  0
                // ----------------------------------
                //   1  0  0  0  0  0  0  0  0  0  0
                if ( ( aImmediate & 0x00000400 ) == 0x00000400 )
                {
                    // 11111111111111111111100000000000
                    //                      10000000000
                    offset = (int) ( 0xFFFFF800 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= aLeftShiftCount;
            return offset;
        }

        public static int SignExtend8BitTo32Bit( uint aImmediate )
        {
            int offset;
            //
            unchecked
            {
                //  7  6  5  4  3  2  1  0
                // ------------------------
                //  1  0  0  0  0  0  0  0
                if ( ( aImmediate & 0x00000080 ) == 0x00000080 )
                {
                    // 11111111111111111111111100000000
                    //                         10000000
                    offset = (int) ( 0xFFFFFF00 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= 1;
            offset += 4; // pipeline
            return offset;
        }
    }
}
