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
using CrashDebuggerLib.Structures.KernelObjects;

namespace CrashDebuggerLib.Structures.Common
{
    public class Capability
    {
        #region Enumerations
        [Flags, System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TCapability : ulong
        {
            ECapabilityNone                 = 0x00000000000,
            ECapabilityTCB                  = 0x00000000001,
            ECapabilityCommDD               = 0x00000000002,
            ECapabilityPowerMgmt            = 0x00000000004,
            ECapabilityMultimediaDD         = 0x00000000008,
            ECapabilityReadDeviceData       = 0x00000000010,
            ECapabilityWriteDeviceData      = 0x00000000020,
            ECapabilityDRM                  = 0x00000000040,
            ECapabilityTrustedUI            = 0x00000000080,
            ECapabilityProtServ             = 0x00000000100,
            ECapabilityDiskAdmin            = 0x00000000200,
            ECapabilityNetworkControl       = 0x00000000400,
            ECapabilityAllFiles             = 0x00000000800,
            ECapabilitySwEvent              = 0x00000001000,
            ECapabilityNetworkServices      = 0x00000002000,
            ECapabilityLocalServices        = 0x00000004000,
            ECapabilityReadUserData         = 0x00000008000,
            ECapabilityLocation             = 0x00000010000,
            ECapabilityWriteUserData        = 0x00000020000,
            ECapabilitySurroundingsDD       = 0x00000040000,
            ECapabilityUserEnvironment      = 0x00000080000,
        }
        #endregion

        #region Constructors
        public Capability()
            : this( TCapability.ECapabilityNone )
        {
        }

        public Capability( TCapability aCapability )
        {
            iCaps = aCapability;
        }

        public Capability( long aValue )
        {
            TCapability cap = (TCapability) aValue;
            iCaps = cap;
        }
        #endregion

        #region API
        public void Add( TCapability aCapability )
        {
            ulong addVal = (ulong) aCapability;
            ulong curVal = (ulong) iCaps;
            ulong combin = addVal | curVal;
            //
            iCaps = (TCapability) combin;
        }

        public void Remove( TCapability aCapability )
        {
            ulong remVal = (ulong) aCapability;
            ulong combin = (ulong) iCaps;
            combin &= remVal;
            //
            iCaps = (TCapability) combin;
        }

        /*
        public static string ToString( TCapability aCapability )
        {

            string ret = KCapabilityNames[ (int) aCapability ];
            return ret;
        }*/
        #endregion

        #region Properties
        public uint LowDWord
        {
            get
            {
                ulong val = (ulong) iCaps;
                ulong masked = val & 0x00000000FFFFFFFF;
                uint ret = (uint) masked;
                return ret;
            }
            set
            {
                ulong val = HighDWord | value;
                iCaps = (TCapability) val;
            }
        }

        public uint HighDWord
        {
            get
            {
                ulong val = (ulong) iCaps;
                ulong masked = val & 0xFFFFFFFF00000000;
                uint ret = (uint) ( masked >> 8 );
                return ret;
            }
            set
            {
                ulong val = ( value << 8 ) | LowDWord;
                iCaps = (TCapability) val;
            }
        }

        public ulong RawValue
        {
            get { return (ulong) iCaps; }
            set { iCaps = (TCapability) value; }
        }

        public TCapability Value
        {
            get { return iCaps; }
            set { iCaps = value; }
        }
        #endregion

        #region Internal methods
        /*private static long MakeValue( TCapability aCapability )
        {
            int shiftAmount = (int) aCapability;
            long ret = 1L << shiftAmount;
            return ret;
        }

        private static void ValidateCapability( TCapability aCapability )
        {
            if ( aCapability < 0 || aCapability >= TCapability.ECapability_Limit )
            {
                throw new ArgumentException( "Capability is out of range: " + aCapability.ToString() );
            }
        }*/
        #endregion

        #region Internal constants
        private const int KMaxBitIndex = 64;
        private static readonly string[] KCapabilityNames = new string[]
        {
	        "TCB",
	        "CommDD",
	        "PowerMgmt",
	        "MultimediaDD",
	        "ReadDeviceData",
	        "WriteDeviceData",
	        "DRM",
	        "TrustedUI",
	        "ProtServ",
	        "DiskAdmin",
	        "NetworkControl",
	        "AllFiles",
	        "SwEvent",
	        "NetworkServices",
	        "LocalServices",
	        "ReadUserData",
	        "WriteUserData",
	        "Location",
	        "SurroundingsDD",
	        "UserEnvironment",
        };
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            for( int i=0; i<KMaxBitIndex; i++ )
            {
                ulong checkVal = (ulong) 1 << i;
                if ( ( checkVal & RawValue ) == checkVal )
                {
                    string name = KCapabilityNames[ i ];
                    //
                    if ( ret.Length > 0 )
                    {
                        ret.Append( ", " );
                    }
                    //
                    ret.Append( name  );
                }

                /*
                TCapability cap = (TCapability) i;
                long mask = MakeValue( cap );
                long remainder = ( iValue & mask );
                if ( remainder != 0 )
                {
                    string name = ToString( cap );
                }
             */
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private TCapability iCaps = TCapability.ECapabilityNone;
        #endregion
    }
}
