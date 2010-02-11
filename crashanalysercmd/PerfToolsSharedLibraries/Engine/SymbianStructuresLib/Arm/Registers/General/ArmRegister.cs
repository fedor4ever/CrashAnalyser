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
using SymbianUtils.Enum;

namespace SymbianStructuresLib.Arm.Registers
{
    public class ArmRegister
    {
        #region Delegates & events
        public delegate void ValueChangedHandler( ArmRegister aReg );
        public event ValueChangedHandler ValueChanged;
        #endregion

        #region Constructors
        public ArmRegister( ArmRegister aCopy )
            : this( aCopy.OriginalName, aCopy.Value )
        {
        }

        public ArmRegister( string aOriginalName )
            : this( aOriginalName, 0 )
        {
        }

        public ArmRegister( TArmRegisterType aType )
            : this( aType, 0 )
        {
        }

        public ArmRegister( TArmRegisterType aType, uint aValue )
            : this( GetTypeName( aType ), aValue )
        {
        }

        public ArmRegister( string aOriginalName, uint aValue )
            : this( GetTypeByName( aOriginalName ), aOriginalName, aValue )
        {
        }

        public ArmRegister( TArmRegisterType aType, string aOriginalName, uint aValue )
        {
            Value = aValue;

            // Save original name - used for UI presentation only
            iOriginalName = aOriginalName;

            // First map the name to a native type (if possible)
            RegType = aType;

            // Then map that type back onto a name, replacing
            // what the client originally supplied. This is because 
            // we want to keep names consistent, e.g. R09 everwhere
            // instead of R9 and R09 depending on client behaviour.
            Name = GetTypeName( RegType );
        }
        #endregion

        #region API
        public static TArmRegisterType GetTypeByName( string aName )
        {
            TArmRegisterType ret = TArmRegisterType.EArmReg_Other;
            //
            switch ( aName.ToUpper() )
            {
            case "R0":
            case "R00":
                ret = TArmRegisterType.EArmReg_00;
                break;
            case "R1":
            case "R01":
                ret = TArmRegisterType.EArmReg_01;
                break;
            case "R2":
            case "R02":
                ret = TArmRegisterType.EArmReg_02;
                break;
            case "R3":
            case "R03":
                ret = TArmRegisterType.EArmReg_03;
                break;
            case "R4":
            case "R04":
                ret = TArmRegisterType.EArmReg_04;
                break;
            case "R5":
            case "R05":
                ret = TArmRegisterType.EArmReg_05;
                break;
            case "R6":
            case "R06":
                ret = TArmRegisterType.EArmReg_06;
                break;
            case "R7":
            case "R07":
                ret = TArmRegisterType.EArmReg_07;
                break;
            case "R8":
            case "R08":
            case "R8_FIQ":
            case "R08_FIQ":
                ret = TArmRegisterType.EArmReg_08;
                break;
            case "R9":
            case "R09":
            case "R9_FIQ":
            case "R09_FIQ":
                ret = TArmRegisterType.EArmReg_09;
                break;
            case "R10":
            case "R10_FIQ":
                ret = TArmRegisterType.EArmReg_10;
                break;
            case "R11":
            case "R11_FIQ":
                ret = TArmRegisterType.EArmReg_11;
                break;
            case "R12":
            case "R12_FIQ":
                ret = TArmRegisterType.EArmReg_12;
                break;
            case "R13":
            case "R13_USR":
            case "R13_FIQ":
            case "R13_SVC":
            case "R13_SYS":
            case "R13_IRQ":
            case "R13_ABT":
            case "R13_UND":
            case "SP":
                ret = TArmRegisterType.EArmReg_SP;
                break;
            case "R14":
            case "R14_USR":
            case "R14_FIQ":
            case "R14_SVC":
            case "R14_IRQ":
            case "R14_ABT":
            case "R14_UND":
            case "R14_SYS":
            case "LR":
                ret = TArmRegisterType.EArmReg_LR;
                break;
            case "R15":
            case "R15_USR":
            case "R15_FIQ":
            case "R15_SVC":
            case "R15_IRQ":
            case "R15_ABT":
            case "R15_UND":
            case "R15_SYS":
            case "PC":
                ret = TArmRegisterType.EArmReg_PC;
                break;
            case "CPSR":
                ret = TArmRegisterType.EArmReg_CPSR;
                break;
            case "SPSR":
            case "SPSR_USR":
            case "SPSR_SVC":
            case "SPSR_IRQ":
            case "SPSR_FIQ":
            case "SPSR_ABT":
            case "SPSR_UND":
            case "SPSR_SYS":
                ret = TArmRegisterType.EArmReg_SPSR;
                break;
            case "DACR":
                ret = TArmRegisterType.EArmReg_DACR;
                break;
            case "FAR":
                ret = TArmRegisterType.EArmReg_FAR;
                break;
            case "FSR":
                ret = TArmRegisterType.EArmReg_FSR;
                break;
            case "CAR":
                ret = TArmRegisterType.EArmReg_CAR;
                break;
            case "MMUID":
                ret = TArmRegisterType.EArmReg_MMUID;
                break;
            case "MMUCR":
                ret = TArmRegisterType.EArmReg_MMUCR;
                break;
            case "AUXCR":
                ret = TArmRegisterType.EArmReg_AUXCR;
                break;
            case "FPEXC":
                ret = TArmRegisterType.EArmReg_FPEXC;
                break;
            case "CTYPE":
                ret = TArmRegisterType.EArmReg_CTYPE;
                break;
            case "EXC_CODE":
                ret = TArmRegisterType.EArmReg_EXCCODE;
                break;
            case "EXC_PC":
                ret = TArmRegisterType.EArmReg_EXCPC;
                break;

            /////////////////////////////////
            // CO-PROCESSOR SYSTEM CONTROL
            /////////////////////////////////
            case "SYSCON_CONTROL":
                ret = TArmRegisterType.EArmReg_SysCon_Control;
                break;

            /////////////////////
            // ETM
            /////////////////////
            case "ETM_CONTROL":
                ret = TArmRegisterType.EArmReg_ETM_Control;
                break;
            case "ETM_ID":
                ret = TArmRegisterType.EArmReg_ETM_Id;
                break;

            /////////////////////
            // ETB
            /////////////////////
            case "ETB_RAM_DEPTH":
                ret = TArmRegisterType.EArmReg_ETB_RamDepth;
                break;
            case "ETB_RAM_WIDTH":
                ret = TArmRegisterType.EArmReg_ETB_RamWidth;
                break;
            case "ETB_STATUS":
                ret = TArmRegisterType.EArmReg_ETB_Status;
                break;
            case "ETB_RAM_WRITE_POINTER":
                ret = TArmRegisterType.EArmReg_ETB_RamWritePointer;
                break;
            case "ETB_TRIGGER_COUNTER":
                ret = TArmRegisterType.EArmReg_ETB_TriggerCounter;
                break;
            case "ETB_CONTROL":
                ret = TArmRegisterType.EArmReg_ETB_Control;
                break;
            case "ETB_ID":
                ret = TArmRegisterType.EArmReg_ETB_Id;
                break;

            /////////////////////
            // CATCH ALL
            /////////////////////
            default:
                break;
            }
            //
            return ret;
        }

        public static string GetTypeName( TArmRegisterType aType )
        {
            string ret = "??";
            //
            try
            {
                ret = EnumUtils.ToString( aType );
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public uint Value
        {
            get { return iValue; }
            set
            {
                if ( iValue != value )
                {
                    iValue = value;

                    if ( ValueChanged != null )
                    {
                        ValueChanged( this );
                    }
                }
            }
        }

        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public string OriginalName
        {
            get { return iOriginalName; }
        }

        public string TypeName
        {
            get { return GetTypeName( RegType ); }
        }

        public TArmRegisterType RegType
        {
            get { return iType; }
            set { iType = value; }
        }

        public ArmRegisterCollection Parent
        {
            get { return iParent; }
            set { iParent = value; }
        }

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }
        #endregion

        #region Operators
        public static implicit operator ArmRegister( uint aValue )
        {
            return new ArmRegister( TArmRegisterType.EArmReg_Other, aValue );
        }

        public static implicit operator uint( ArmRegister aRegister )
        {
            return aRegister.Value;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.AppendFormat( "{0,-12} = 0x{1:x8}", OriginalName, Value );
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly string iOriginalName;
        private uint iValue = 0;
        private string iName = string.Empty;
        private TArmRegisterType iType = TArmRegisterType.EArmReg_00;
        private ArmRegisterCollection iParent = null;
        private object iTag = null;
        #endregion
    }
}
