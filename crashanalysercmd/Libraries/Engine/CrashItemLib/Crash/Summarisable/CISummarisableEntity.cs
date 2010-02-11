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
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Summarisable
{
	public class CISummarisableEntity : CIElement
    {
        #region Enumerations
        public enum TContext
        {
            /// <summary>
            /// A thread-specific entity, the normal case when the crash occurs within a Symbian
            /// OS kernel or user side thread context.
            /// </summary>
            EContextTypeThread = 0,

            /// <summary>
            /// An exception mode entity. For example, an object that represents the associated
            /// stack et al for an IRQ handler or FIQ handler crash.
            /// </summary>
            EContextTypeException
        }

        public enum TElement
        {
            EElementThread = 0,
            EElementProcess,
            EElementCodeSegments,
            EElementStack,
            EElementRegisters
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Preferred constructor that is called by CISummarisableEntityList during finalization.
        /// This constructor is used to create a summary wrapper for any stack-level objects that
        /// were created during container preparation. Those stack objects may be stand-alone exception
        /// mode stacks (IRQ, FIQ, ABT, UND et al) or then they may be associated with a specific
        /// Symbian OS thread (user or supervisor).
        /// </summary>
        /// <param name="aStack"></param>
        internal CISummarisableEntity( CIStack aStack )
            : base( aStack.Container )
        {
            // NB: If the stack has an associated register list,
            // and the register list's owner is a thread, then we
            // automatically know we're dealing with a thread-based stack.
            AddChild( aStack );
        }

        /// <summary>
        /// Fall back constructor which is called when the thread in question has no associated
        /// stack. This means that stack data is unavailable and therefore stack reconstruction is
        /// obviously impossible. However, the thread (and by implication the process) may well still
        /// contain useful information.
        /// </summary>
        /// <param name="aThread"></param>
        internal CISummarisableEntity( CIThread aThread )
            : base( aThread.Container )
        {
            AddChild( aThread );
        }
		#endregion

        #region API
        public bool IsAvailable( TElement aElement )
        {
            bool ret = false;
            //
            switch ( aElement )
            {
            case TElement.EElementThread:
                ret = ( Thread != null );
                break;
            case TElement.EElementProcess:
                ret = ( Process != null );
                break;
            case TElement.EElementCodeSegments:
                ret = ( CodeSegments != null );
                break;
            case TElement.EElementStack:
                ret = ( Stack != null );
                break;
            case TElement.EElementRegisters:
                ret = ( Registers != null );
                break;
            }
            //
            return ret;
        }

        public CIMessageDictionary GetMessages()
        {
            CIElementList<CIMessage> list = base.ChildrenByType<CIMessage>( TChildSearchType.EEntireHierarchy );
            CIMessageDictionary ret = new CIMessageDictionary( list );
            return ret;
        }
        #endregion

        #region Properties
        public TContext Context
        {
            get
            {
                TContext ret = TContext.EContextTypeException;
                //
                if ( IsAvailable( TElement.EElementThread ) )
                {
                    ret = TContext.EContextTypeThread;
                }
                //
                return ret;
            }
        }

        public bool IsAbnormalTermination
        {
            get
            {
                // A crash can be an exception stack (i.e. without a thread)
                // or then a thread that has panics/crashed in some way.
                bool ret = false;
                //
                if ( IsAvailable( TElement.EElementThread ) )
                {
                    ret = Thread.IsAbnormalTermination;
                }
                else if ( IsAvailable( TElement.EElementStack ) )
                {
                    // All stand-alone stacks are treated as crashes.
                    ret = true;
                }
                //
                return ret;
            }
        }

        public CIThread Thread
        {
            get
            {
                CIThread ret = null;
                
                // Use the stack object to find associated thread.
                CIStack stack = this.Stack;
                if ( stack != null )
                {
                    ret = stack.OwningThread;
                }

                // Did we find a thread? If not, fall back to trying our children.
                if ( ret == null )
                {
                    // Register set unavailable - check child nodes for
                    // presence of thread object. Implies no stack data for
                    // this summary wrapper object.
                    ret = base.ChildByType( typeof( CIThread ) ) as CIThread;
                }
                //
                return ret;
            }
        }

        public CIProcess Process
        {
            get
            {
                CIProcess ret = null;
                //
                if ( Thread != null )
                {
                    ret = Thread.OwningProcess;
                }
                //
                return ret;
            }
        }

        public CICodeSegList CodeSegments
        {
            get
            {
                CICodeSegList ret = null;
                //
                CIProcess process = this.Process;
                if ( process != null )
                {
                    ret = process.CodeSegments;
                }
                //
                return ret;
            }
        }

        public CIRegisterList Registers
        {
            get 
            {
                CIRegisterList ret = null;
                //
                CIStack stack = this.Stack;
                if ( stack != null )
                {
                    // Use registers associated with stack
                    ret = stack.Registers;
                }
                else
                {
                    // No stack, if we have a thread then try to
                    // find the thread register list that is current.
                    CIThread thread = this.Thread;
                    if ( thread != null )
                    {
                        ret = thread.CurrentProcessorModeRegisters;
                    }
                }
                //
                return ret;
            }
        }

        public CIStack Stack
        {
            get 
            {
                // Might return null
                CIStack ret = base.ChildByType( typeof( CIStack ) ) as CIStack;
                return ret;
            }
        }

        public TArmRegisterBank Bank
        {
            get
            {
                TArmRegisterBank ret = TArmRegisterBank.ETypeUnknown;
                //
                if ( IsAvailable( TElement.EElementStack ) )
                {
                    ret = Stack.Type;
                }
                //
                return ret;
            }
        }

        public override string Name
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                //
                if ( IsAvailable( TElement.EElementThread ) )
                {
                    // Just use thread name
                    ret.Append( Thread.FullName );
                }
                else if ( IsAvailable( TElement.EElementStack ) )
                {
                    // Get associated bank type
                    TArmRegisterBank bank = Stack.Registers.Bank;
                    string type = ArmRegisterBankUtils.BankAsStringLong( bank );
                    ret.AppendFormat( "{0} mode", type );
                }
                //
                return ret.ToString();
            }
            set
            {
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
