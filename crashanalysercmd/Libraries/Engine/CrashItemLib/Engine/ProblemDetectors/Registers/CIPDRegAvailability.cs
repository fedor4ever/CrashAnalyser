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

namespace CrashItemLib.Engine.ProblemDetectors.Registers
{
    internal class CIPDRegAvailability : CIProblemDetector
    {
        #region Constructors
        public CIPDRegAvailability()
		{
        }
		#endregion

        #region From CIProblemDetector
        public override void Check( CIContainer aContainer )
        {
            CISummarisableEntityList list = aContainer.Summaries;
            foreach ( CISummarisableEntity entry in list )
            {
                // Check that each stack contains some registers and at least the SP.
                bool stackAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementStack );
                bool regsAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementRegisters );
                //
                if ( stackAvailable )
                {
                    CIStack stack = entry.Stack;
                    //
                    if ( regsAvailable )
                    {
                        CIRegisterList regs = stack.Registers;

                        // Check that SP, LR and PC are available
                        bool pointerAvailable = regs.Contains( TArmRegisterType.EArmReg_SP );
                        if ( !pointerAvailable )
                        {
                            base.CreateWarning( aContainer, stack,
                               LibResources.CIPDRegAvailability_MissingSP_Title,
                               string.Format( LibResources.CIPDRegAvailability_MissingSP_Description, base.CreateIdentifierText( entry ) )
                               );
                        }
                        //
                        bool lrAvailable = regs.Contains( TArmRegisterType.EArmReg_LR );
                        if ( !lrAvailable )
                        {
                            base.CreateWarning( aContainer, stack,
                               LibResources.CIPDRegAvailability_MissingLR_Title,
                               string.Format( LibResources.CIPDRegAvailability_MissingLR_Description, base.CreateIdentifierText( entry ) )
                               );
                        }
                        //
                        bool pcAvailable = regs.Contains( TArmRegisterType.EArmReg_PC );
                        if ( !pcAvailable )
                        {
                            base.CreateWarning( aContainer, stack,
                               LibResources.CIPDRegAvailability_MissingPC_Title,
                               string.Format( LibResources.CIPDRegAvailability_MissingPC_Description, base.CreateIdentifierText( entry ) )
                               );
                        }
                     
                        // If R0 is available, check if it is 0 and check whether an exception occurred - if so, it was possibly
                        // caused by de-referencing a NULL this pointer.
                        bool threadAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementThread );
                        if ( threadAvailable )
                        {
                            if ( regs.Contains( TArmRegisterType.EArmReg_00 ) )
                            {
                                CIRegister r0 = regs[ TArmRegisterType.EArmReg_00 ];
                                //
                                bool r0WasNull = ( r0.Value == 0 );
                                bool wasException = entry.IsAbnormalTermination && ( entry.Thread.ExitInfo.Type == CrashItemLib.Crash.ExitInfo.CIExitInfo.TExitType.EExitTypeException );
                                bool wasKernExec3 = entry.IsAbnormalTermination && ( entry.Thread.ExitInfo.Type == CrashItemLib.Crash.ExitInfo.CIExitInfo.TExitType.EExitTypePanic && 
                                                                                     entry.Thread.ExitInfo.Category.ToUpper() == "KERN-EXEC" && 
                                                                                     entry.Thread.ExitInfo.Reason == 3 );
                                //
                                if ( r0WasNull && ( wasException || wasKernExec3 ) )
                                {
                                    base.CreateWarning( aContainer, r0,
                                       LibResources.CIPDRegAvailability_NullThisPointer_Title,
                                       string.Format( LibResources.CIPDRegAvailability_NullThisPointer_Description, base.CreateIdentifierText( entry ) )
                                       );
                                }
                            }
                        }
                    }
                    else
                    {
                        base.CreateWarning( aContainer, stack, 
                               LibResources.CIPDRegAvailability_NoRegsForStack_Title,
                               string.Format( LibResources.CIPDRegAvailability_NoRegsForStack_Description, base.CreateIdentifierText( entry ) )
                               );
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
