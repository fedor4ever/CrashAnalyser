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
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Summarisable;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.Range;

namespace CrashItemLib.Engine.ProblemDetectors.Stack
{
    internal class CIPDStackBoundaryValidator : CIProblemDetector
    {
        #region Constructors
        public CIPDStackBoundaryValidator()
		{
        }
		#endregion

        #region From CIProblemDetector
        public override void Check( CIContainer aContainer )
        {
            CISummarisableEntityList list = aContainer.Summaries;
            foreach ( CISummarisableEntity entry in list )
            {
                bool stackAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementStack );
                bool regsAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementRegisters );
                //
                if ( stackAvailable && regsAvailable )
                {
                    CIStack stack = entry.Stack;
                    bool pointerAvailable = stack.Registers.Contains( TArmRegisterType.EArmReg_SP );
                    //
                    if ( pointerAvailable )
                    {
                        CIRegister regSP = stack.Pointer;
                        AddressRange stackRange = entry.Stack.Range;
                        //
                        if ( stack.IsOverflow )
                        {
                            base.CreateError( aContainer, stack, 
                                LibResources.CIPDStackBoundaryValidator_StackOverflow_Title,
                                string.Format( LibResources.CIPDStackBoundaryValidator_StackOverflow_Description, base.CreateIdentifierText( entry ) )
                                );
                        }
                        else if ( !stackRange.Contains( regSP ) )
                        {
                            base.CreateError( aContainer, stack,
                                LibResources.CIPDStackBoundaryValidator_StackUnderflow_Title,
                                string.Format( LibResources.CIPDStackBoundaryValidator_StackUnderflow_Description, base.CreateIdentifierText( entry ) )
                                );
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        #endregion

        #region Data members
		#endregion
    }
}
