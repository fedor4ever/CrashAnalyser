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
using SymbianETMLib.Common.Engine;
using SymbianETMLib.Common.Types;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;

namespace SymbianETMLib.ETB.StackRecon
{
    public class ETBStackReconManager
    {
        #region Constructors
        public ETBStackReconManager( ETEngineBase aEngine )
        {
            iEngine = aEngine;
            iEngine.Branch += new ETEngineBase.BranchHandler( ETB_Branch );
            iEngine.ContextSwitch += new ETEngineBase.ContextSwitchHandler( ETB_ContextSwitch );

            // Make the initial "unknown" context id stack.
            SwitchContext( 0 );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        private void ETB_ContextSwitch( uint aContextId, string aThreadName )
        {
            // Make sure we're using the new current stack
            SwitchContext( aContextId );
        }

        private void ETB_Branch( ETMBranch aBranch )
        {
            if ( iCurrentStack != null )
            {
                iCurrentStack.HandleBranch( aBranch );
            }
        }
        #endregion

        #region Internal methods
        private void SwitchContext( uint aId )
        {
            if ( !iStacks.ContainsKey( aId ) )
            {
                ETBStack stack = new ETBStack( iEngine, aId );
                iStacks.Add( aId, stack );
            }
            //
            iCurrentStack = iStacks[ aId ];
        }
        #endregion

        #region Data members
        private readonly ETEngineBase iEngine;
        private ETBStack iCurrentStack = null;
        private Dictionary<uint, ETBStack> iStacks = new Dictionary<uint, ETBStack>();
        #endregion
    }
}
