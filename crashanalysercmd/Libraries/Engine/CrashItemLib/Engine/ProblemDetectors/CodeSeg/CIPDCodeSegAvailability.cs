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
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Summarisable;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.Range;

namespace CrashItemLib.Engine.ProblemDetectors.CodeSeg
{
    internal class CIPDCodeSegAvailability : CIProblemDetector
    {
        #region Constructors
        public CIPDCodeSegAvailability()
		{
        }
		#endregion

        #region From CIProblemDetector
        public override void Check( CIContainer aContainer )
        {
            CIElementList<CICodeSeg> allCodeSegs = aContainer.ChildrenByType<CICodeSeg>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CICodeSeg codeSeg in allCodeSegs )
            {
                bool isResolved = codeSeg.IsResolved;
                if ( !isResolved )
                {
                    CreateMissingWarning( codeSeg );
                }
                if ( codeSeg.IsMismatched )
                {
                    CreateMismatchWarning( codeSeg );
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void CreateMismatchWarning( CICodeSeg aCodeSeg )
        {
            CIMessageWarning warning = new CIMessageWarning( aCodeSeg.Container, LibResources.CIPDCodeSegAvailability_CodeSegMisMatch_Title );
            warning.AddLineFormatted( LibResources.CIPDCodeSegAvailability_CodeSegMisMatch_Description_L1, aCodeSeg, aCodeSeg.OwningProcess.Name );
            warning.AddLineFormatted( LibResources.CIPDCodeSegAvailability_CodeSegMisMatch_Description_L2, aCodeSeg.Base, aCodeSeg.MismatchAddress );
            //
            aCodeSeg.AddChild( warning );
        }

        private void CreateMissingWarning( CICodeSeg aCodeSeg )
        {
            CIMessageWarning warning = new CIMessageWarning( aCodeSeg.Container, LibResources.CIPDCodeSegAvailability_NoSymbols_Title );
            warning.AddLineFormatted( LibResources.CIPDCodeSegAvailability_NoSymbols_Description, aCodeSeg, aCodeSeg.OwningProcess.Name );
            //
            aCodeSeg.AddChild( warning );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
		#endregion
    }
}
