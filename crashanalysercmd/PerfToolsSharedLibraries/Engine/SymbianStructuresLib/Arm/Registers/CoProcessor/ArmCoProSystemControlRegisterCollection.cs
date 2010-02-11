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

namespace SymbianStructuresLib.Arm.Registers.CoProcessor
{
    public class ArmCoProSystemControlRegisterCollection : ArmRegisterCollection
    {
        #region Constructors
        public ArmCoProSystemControlRegisterCollection()
            : base( TArmRegisterBank.ETypeCoProSystemControl )
        {
            AddDefaults();
        }
        #endregion

        #region From ArmRegisterCollection
        public override void AddDefaults()
        {
            base.Clear();
            //
            Array items = Enum.GetValues( typeof( TArmRegisterType ) );
            foreach ( object enumEntry in items )
            {
                TArmRegisterType value = (TArmRegisterType) enumEntry;
                if ( value >= TArmRegisterType.EArmReg_SysCon_Control && value <= TArmRegisterType.EArmReg_SysCon_Control )
                {
                    Add( value, 0 );
                }
            }
        }
        #endregion
    }
}
