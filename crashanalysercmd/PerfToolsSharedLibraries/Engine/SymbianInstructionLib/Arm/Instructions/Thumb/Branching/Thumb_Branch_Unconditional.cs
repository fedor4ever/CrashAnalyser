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
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Thumb.Branching
{
    public class Thumb_Branch_Unconditional : Thumb_Branch_Immediate
    {
        #region Enumerations
        public enum TType
        {
            ETypeMultipartFirstPartOfBranch = -1,
            ETypeB = 0,
            ETypeBL,
            ETypeBLX
        }
        #endregion

        #region Constructors
        public Thumb_Branch_Unconditional()
        {
            //                      H      offset_11
            base.SetMask( "111" + "##" + "###########" );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override int BranchOffset
        {
            get
            {
                int ret = ThumbInstructionUtils.SignExtend11BitTo32Bit( base.AIRawValue & 0x7FF );
                return ret;
            }
        }

        public TType Type
        {
            get
            {
                TType ret = TType.ETypeB;
                //
                byte val = base.AIRawValue[ 12, 11 ];
                if ( val == 1 )
                {
                    ret = TType.ETypeBLX;
                }
                else if ( val == 3 )
                {
                    ret = TType.ETypeBL;
                }
                else if ( val == 2 )
                {
                    ret = TType.ETypeMultipartFirstPartOfBranch;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

