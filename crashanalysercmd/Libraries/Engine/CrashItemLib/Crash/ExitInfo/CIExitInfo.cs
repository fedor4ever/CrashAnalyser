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
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Special;
using SymbianUtils.Range;
using SymbianUtils.Enum;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.ExitInfo
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIExitInfo : CIElement
    {
        #region Enumerations
        public enum TExitType
        {
            EExitTypePending = 0,
            EExitTypeKill,
            EExitTypePanic,
            EExitTypeTerminate,
            EExitTypeException
        }
        #endregion

        #region Constructors
        public CIExitInfo( CIElement aParent )
            : base( aParent.Container, aParent )
		{
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public TExitType Type
        {
            get
            {
                TExitType ret = iType;
                //
                if ( !iTypeWasExplicitlySet && OwningThread != null )
                {
                    // A client did not specify an explict exit reason, so in this case
                    // we'll try to work out if we're dealing with an exception based upon
                    // register values. 
                    //
                    // We can only do this if the owning object is available.
                    CIThread thread = OwningThread;
                    CIThreadRegisterListCollection regs = thread.Registers;
                    if ( thread.Registers.Contains( TArmRegisterBank.ETypeException ) )
                    {
                        // Best guess
                        ret = TExitType.EExitTypeException;
                    }
                }
                //
                return iType; 
            }
            set
            {
                iTypeWasExplicitlySet = true;
                iType = value; 
            }
        }

        public string TypeDescription
        {
            get 
            {
                switch( Type )
                {
                case TExitType.EExitTypeException:
                    return "Exception";
                case TExitType.EExitTypePending:
                    return "Pending";
                case TExitType.EExitTypePanic:
                    return "Panic";
                case TExitType.EExitTypeTerminate:
                    return "Terminate";
                case TExitType.EExitTypeKill:
                    return "Kill";
                default:
                    return "Unknown";
                }
            }
        }

        public bool IsAbnormalTermination
        {
            get
            {
                bool ret = false;
                //
                switch ( Type )
                {
                case TExitType.EExitTypeException:
                case TExitType.EExitTypePanic:
                case TExitType.EExitTypeTerminate:
                    ret = true;
                    break;
                default:
                case TExitType.EExitTypeKill:
                case TExitType.EExitTypePending:
                    break;
                }
                //
                return ret;
            }
        }

        public string Category
        {
            get
            {
                string ret = iCategory;
                //
                if ( Type == TExitType.EExitTypeException )
                {
                    CIRegisterExcCode code = RegisterExcCode;
                    if ( code != null )
                    {
                        ret = EnumUtils.ToString( ExceptionCode );
                    }
                }
                //
                return ret; 
            }
            set { iCategory = value; }
        }

        public int Reason
        {
            get
            {
                int ret = iReason;
                //
                if ( Type == TExitType.EExitTypeException )
                {
                    ret = (int) ExceptionCode;
                }
                //
                return ret; 
            }
            set { iReason = value; }
        }

        public CIRegisterFSR RegisterFSR
        {
            get
            {
                CIRegisterFSR ret = null;
                //
                CIRegisterList list = RegListCoProcessor;
                if ( list != null && list.Contains( TArmRegisterType.EArmReg_FSR ) )
                {
                    CIRegister reg = list[ TArmRegisterType.EArmReg_FSR ];
                    ret = reg as CIRegisterFSR;
                }
                //
                return ret;
            }
        }

        public CIRegisterExcCode RegisterExcCode
        {
            get
            {
                CIRegisterExcCode ret = null;
                //
                CIRegisterList list = RegListException;
                if ( list != null && list.Contains( TArmRegisterType.EArmReg_EXCCODE ) )
                {
                    CIRegister reg = list[ TArmRegisterType.EArmReg_EXCCODE ];
                    ret = reg as CIRegisterExcCode;
                }
                //
                return ret;
            }
        }

        public CIRegisterExcCode.TExceptionCode ExceptionCode
        {
            get
            {
                if ( Type != TExitType.EExitTypeException )
                {
                    throw new InvalidOperationException();
                }

                CIRegisterExcCode.TExceptionCode code = CIRegisterExcCode.TExceptionCode.EExceptionCodeUnknown;
                CIRegisterExcCode excCode = RegisterExcCode;
                //
                if ( excCode != null )
                {
                    code = excCode;
                }
                //
                return code;
            }
        }

        public string ExceptionDescription
        {
            get
            {
                string ret = string.Empty;
                //
                if ( Type == TExitType.EExitTypeException )
                {
                    CIRegisterExcCode reg = RegisterExcCode;
                    if ( reg != null )
                    {
                        ret = reg.ExceptionCodeDescription;
                    }
                }
                //
                return ret;
            }
        }

        public CIThread OwningThread
        {
            get { return base.Parent as CIThread; }
        }

        public CIProcess OwningProcess
        {
            get
            { 
                CIProcess ret = base.Parent as CIProcess;
                //
                if ( ret == null && OwningThread != null )
                {
                    ret = OwningThread.OwningProcess;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private CIRegisterList RegListException
        {
            get
            {
                System.Diagnostics.Debug.Assert( OwningThread != null );
                //
                CIThread thread = (CIThread) OwningThread;
                CIThreadRegisterListCollection registers = thread.Registers;
                //
                if ( registers.Contains( TArmRegisterBank.ETypeException ) )
                {
                    CIRegisterList list = registers[ TArmRegisterBank.ETypeException ];
                    return list;
                }
                //
                return null;
            }
        }

        private CIRegisterList RegListCoProcessor
        {
            get
            {
                System.Diagnostics.Debug.Assert( OwningThread != null );
                //
                CIThread thread = (CIThread) OwningThread;
                CIThreadRegisterListCollection registers = thread.Registers;
                //
                if ( registers.Contains( TArmRegisterBank.ETypeCoProcessor ) )
                {
                    CIRegisterList list = registers[ TArmRegisterBank.ETypeCoProcessor ];
                    return list;
                }
                //
                return null;
            }
        }
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();
            
            // Type is common
            CIDBRow rowType= new CIDBRow();
            rowType.Add( new CIDBCell( "Type" ) );
            rowType.Add( new CIDBCell( TypeDescription ) );
            DataBindingModel.Add( rowType );
   
            // We must prepare them by hand because we show
            // different content depending on the type of exit.
            if ( Type == TExitType.EExitTypePending )
            {
                // Nothing to add
            }
            else if ( Type == TExitType.EExitTypeException )
            {
                string code = ExceptionDescription;
                if ( code != string.Empty )
                {
                    CIDBRow rowExcCode = new CIDBRow();
                    rowExcCode.Add( new CIDBCell( "Exception Code" ) );
                    rowExcCode.Add( new CIDBCell( code ) );
                    DataBindingModel.Add( rowExcCode );
                }

                CIRegisterFSR fsr = RegisterFSR;
                if ( fsr != null )
                {
                    CIDBRow rowFSR = new CIDBRow();
                    rowFSR.Add( new CIDBCell( "Fault Type" ) );
                    rowFSR.Add( new CIDBCell( fsr.FaultDescription ) );
                    DataBindingModel.Add( rowFSR );
                }
            }
            else
            {
                // Panic, terminate
                CIDBRow rowCategory = new CIDBRow();
                rowCategory.Add( new CIDBCell( "Category" ) );
                rowCategory.Add( new CIDBCell( Category ) );
                DataBindingModel.Add( rowCategory );

                CIDBRow rowReason = new CIDBRow();
                rowReason.Add( new CIDBCell( "Reason" ) );
                rowReason.Add( new CIDBCell( Reason.ToString() ) );
                DataBindingModel.Add( rowReason );

                CIDBRow rowFullPanic = new CIDBRow();
                rowFullPanic.Add( new CIDBCell( "In Full" ) );
                rowFullPanic.Add( new CIDBCell( string.Format( "{0}-{1}", Category, Reason ) ) );
                DataBindingModel.Add( rowFullPanic );
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( Type == TExitType.EExitTypePending )
            {
                ret.Append( "Pending" );
            }
            else if ( Type == TExitType.EExitTypeException )
            {
                ret.Append( "Exception" );
            }
            else
            {
                ret.AppendFormat( "{0}-{1}", Category, Reason );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private int iReason = 0;
        private string iCategory = string.Empty;
        private bool iTypeWasExplicitlySet = false;
        private TExitType iType = TExitType.EExitTypePending;
        #endregion
    }
}
