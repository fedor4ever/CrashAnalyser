/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Reflection;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Structures.UserContextTable
{
    internal class UserContextTable : IEnumerable<UserContextTableEntry>
    {
        #region Enumerations
        public enum TArmRegisterIndex
        {
            EArmR0 = 0,
            EArmR1 = 1,
            EArmR2 = 2,
            EArmR3 = 3,
            EArmR4 = 4,
            EArmR5 = 5,
            EArmR6 = 6,
            EArmR7 = 7,
            EArmR8 = 8,
            EArmR9 = 9,
            EArmR10 = 10,
            EArmR11 = 11,
            EArmR12 = 12,
            EArmSp = 13,
            EArmLr = 14,
            EArmPc = 15,
            EArmFlags = 16,
            EArmDacr = 17
        }
        #endregion

        #region Constructors
        public UserContextTable( TUserContextType aType )
        {
            iType = aType;
            //
            List<UserContextTableEntry> entries = new List<UserContextTableEntry>();
            int count = EntryCount;
            for ( int i = 0; i < count; i++ )
            {
                UserContextTableEntry entry = new UserContextTableEntry();
                entries.Add( entry );
            }
            //
            iEntries = entries.ToArray();
        }
        #endregion

        #region API
        public static bool IsSupported( TArmRegisterType aType )
        {
            bool ret = false;
            //
            switch ( aType )
            {
            case TArmRegisterType.EArmReg_00:
            case TArmRegisterType.EArmReg_01:
            case TArmRegisterType.EArmReg_02:
            case TArmRegisterType.EArmReg_03:
            case TArmRegisterType.EArmReg_04:
            case TArmRegisterType.EArmReg_05:
            case TArmRegisterType.EArmReg_06:
            case TArmRegisterType.EArmReg_07:
            case TArmRegisterType.EArmReg_08:
            case TArmRegisterType.EArmReg_09:
            case TArmRegisterType.EArmReg_10:
            case TArmRegisterType.EArmReg_11:
            case TArmRegisterType.EArmReg_12:
            case TArmRegisterType.EArmReg_SP:
            case TArmRegisterType.EArmReg_LR:
            case TArmRegisterType.EArmReg_PC:
            case TArmRegisterType.EArmReg_CPSR:
            case TArmRegisterType.EArmReg_DACR:
                ret = true;
                break;
            default:
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public UserContextTableEntry this[ TArmRegisterIndex aIndex ]
        {
            get
            {
                int index = (int) aIndex;
                UserContextTableEntry ret = iEntries[ index ];
                return ret;
            }
        }

        public UserContextTableEntry this[ TArmRegisterType aReg ]
        {
            get
            {
                // Have to map to our internal type
                TArmRegisterIndex reg = Map( aReg );
                return this[ reg ];
            }
        }

        public static int EntryCount
        {
            get
            {
                Array vals = Enum.GetValues( typeof( TArmRegisterIndex ) );
                int count = vals.Length;
                return count;
            }
        }

        public TUserContextType Type
        {
            get { return iType; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach ( UserContextTableEntry entry in iEntries )
            {
                ret.AppendFormat( "[{0:x2}, {1:x2}]", (int) entry.Type, (int) entry.Offset );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Internal methods
        private static TArmRegisterIndex Map( TArmRegisterType aReg )
        {
            TArmRegisterIndex ret = TArmRegisterIndex.EArmR0;
            //
            switch ( aReg )
            {
            case TArmRegisterType.EArmReg_00:
                ret = TArmRegisterIndex.EArmR0;
                break;
            case TArmRegisterType.EArmReg_01:
                ret = TArmRegisterIndex.EArmR1;
                break;
            case TArmRegisterType.EArmReg_02:
                ret = TArmRegisterIndex.EArmR2;
                break;
            case TArmRegisterType.EArmReg_03:
                ret = TArmRegisterIndex.EArmR3;
                break;
            case TArmRegisterType.EArmReg_04:
                ret = TArmRegisterIndex.EArmR4;
                break;
            case TArmRegisterType.EArmReg_05:
                ret = TArmRegisterIndex.EArmR5;
                break;
            case TArmRegisterType.EArmReg_06:
                ret = TArmRegisterIndex.EArmR6;
                break;
            case TArmRegisterType.EArmReg_07:
                ret = TArmRegisterIndex.EArmR7;
                break;
            case TArmRegisterType.EArmReg_08:
                ret = TArmRegisterIndex.EArmR8;
                break;
            case TArmRegisterType.EArmReg_09:
                ret = TArmRegisterIndex.EArmR9;
                break;
            case TArmRegisterType.EArmReg_10:
                ret = TArmRegisterIndex.EArmR10;
                break;
            case TArmRegisterType.EArmReg_11:
                ret = TArmRegisterIndex.EArmR11;
                break;
            case TArmRegisterType.EArmReg_12:
                ret = TArmRegisterIndex.EArmR12;
                break;
            case TArmRegisterType.EArmReg_SP:
                ret = TArmRegisterIndex.EArmSp;
                break;
            case TArmRegisterType.EArmReg_LR:
                ret = TArmRegisterIndex.EArmLr;
                break;
            case TArmRegisterType.EArmReg_PC:
                ret = TArmRegisterIndex.EArmPc;
                break;
            case TArmRegisterType.EArmReg_CPSR:
                ret = TArmRegisterIndex.EArmFlags;
                break;
            case TArmRegisterType.EArmReg_DACR:
                ret = TArmRegisterIndex.EArmDacr;
                break;
            default:
                throw new NotSupportedException();
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<UserContextTableEntry>
        public IEnumerator<UserContextTableEntry> GetEnumerator()
        {
            foreach ( UserContextTableEntry entry in iEntries )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( UserContextTableEntry entry in iEntries )
            {
                yield return entry;
            }
        }
        #endregion

        #region Data members
        private readonly TUserContextType iType;
        private readonly UserContextTableEntry[] iEntries;
        #endregion
    }
}
