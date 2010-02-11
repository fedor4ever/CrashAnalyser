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
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Summarisable;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.Range;

namespace CrashItemLib.Engine.ProblemDetectors.Stack
{
    internal class CIPDThreadValidity : CIProblemDetector
    {
        #region Constructors
        public CIPDThreadValidity()
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
                bool threadAvailable = entry.IsAvailable( CISummarisableEntity.TElement.EElementThread );
                //
                if ( threadAvailable && !stackAvailable )
                {
                    base.CreateWarning( aContainer, entry.Thread,
                        LibResources.CIPDThreadValidity_NoStack_Title,
                        string.Format( LibResources.CIPDThreadValidity_NoStack_Description, base.CreateIdentifierText( entry ) )
                        );
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
