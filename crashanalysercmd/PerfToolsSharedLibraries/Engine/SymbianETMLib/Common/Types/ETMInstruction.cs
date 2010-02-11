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
using SymbianUtils.BasicTypes;
using SymbianETMLib.Common.Utilities;
using SymbianStructuresLib.Arm.Instructions;

namespace SymbianETMLib.Common.Types
{
    internal class ETMInstruction
    {
        #region Constructors
        public ETMInstruction()
            : this( 0 )
        {
        }

        public ETMInstruction( uint aAddress )
            : this( aAddress, null )
        {
        }

        public ETMInstruction( uint aAddress, IArmInstruction aInstruction )
        {
            iAddress = new SymAddress( aAddress );
            iInstruction = aInstruction;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = string.Empty;
            if ( HaveInstruction )
            {
                ret = iInstruction.ToString();
            }
            return ret;
        }
        #endregion

        #region Properties
        public bool HaveInstruction
        {
            get { return iInstruction != null; }
        }

        public SymAddress Address
        {
            get { return iAddress; }
            set { iAddress = value; }
        }

        public uint AIRawValue
        {
            get
            {
                uint ret = 0;
                if ( HaveInstruction )
                {
                    ret = iInstruction.AIRawValue;
                }
                return ret;
            }
        }

        public IArmInstruction Instruction
        {
            get { return iInstruction; }
            set { iInstruction = value; }
        }
        #endregion

        #region Operators
        public static implicit operator uint( ETMInstruction aInstruction )
        {
            uint ret = aInstruction.AIRawValue;
            return ret;
        }
        #endregion

        #region Data members
        private SymAddress iAddress = new SymAddress();
        private IArmInstruction iInstruction = null;
        #endregion
    }
}
