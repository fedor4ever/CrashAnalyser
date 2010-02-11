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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Summarisable
{
	public class CISummarisableEntityList : CIElement
    {
        #region Constructors
        [CIElementAttributeMandatory()]
        public CISummarisableEntityList( CIContainer aContainer )
            : base( aContainer )
        {
            // Restrict children to summaries
            base.AddSupportedChildType( typeof( CISummarisableEntity ) );
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
        }

        internal CISummarisableEntityList( CIContainer aContainer, IEnumerable<CISummarisableEntity> aArray )
            : this( aContainer )
        {
            foreach ( CISummarisableEntity entry in aArray )
            {
                base.AddChild( entry );
            }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public CISummarisableEntity this[ CIThread aEntry ]
        {
            get
            {
                CISummarisableEntity ret = null;
                //
                foreach ( CISummarisableEntity entry in this )
                {
                    if ( entry.IsAvailable( CISummarisableEntity.TElement.EElementThread ) )
                    {
                        if ( entry.Thread.Id == aEntry.Id )
                        {
                            ret = entry;
                            break;
                        }
                    }
                }
                //
                return ret;
            }
        }

        public CISummarisableEntity this[ CIStack aEntry ]
        {
            get
            {
                CISummarisableEntity ret = null;
                //
                foreach ( CISummarisableEntity entry in this )
                {
                    if ( entry.IsAvailable( CISummarisableEntity.TElement.EElementStack ) )
                    {
                        if ( entry.Stack.Id == aEntry.Id )
                        {
                            ret = entry;
                            break;
                        }
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From CIElement
        internal override void DoFinalize( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize )
        {
            CIContainer container = base.Container;

            // Find all the stacks and add them as children before we call the base class
            // method, since the base class will then take care of finalizing the new dynamically created
            // summarisable objects.
            CIElementList<CIStack> stacks = container.ChildrenByType<CIStack>( TChildSearchType.EEntireHierarchy );
            if ( stacks != null && stacks.Count > 0 )
            {
                foreach( CIStack stack in stacks )
                {
                    CISummarisableEntity entity = this[ stack ];
                    if ( entity == null )
                    {
                        entity = new CISummarisableEntity( stack );
                        AddChild( entity );
                    }
                }
            }

            // Now, make sure there are summarisable wrappers created for any threads which do not have associated
            // stacks. Call stacks will be unavailable, but there's still plenty of useful information at the thread
            // process and register levels.
            CIElementList<CIThread> threads = container.ChildrenByType<CIThread>( TChildSearchType.EEntireHierarchy );
            if ( threads != null && threads.Count > 0 )
            {
                foreach ( CIThread thread in threads )
                {
                    CISummarisableEntity entity = this[ thread ];
                    if ( entity == null )
                    {
                        entity = new CISummarisableEntity( thread );
                        AddChild( entity );
                    }
                }
            }

            // Now run finalizers on children et al.
            base.DoFinalize( aParams, aCallBackLast, aForceFinalize );
        }
        #endregion

        #region Data members
        #endregion
    }
}
