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
using System.Xml;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Registers.EmbeddedTrace;
using SymbianStructuresLib.Arm.Registers.CoProcessor;
using SymbianETMLib.Common.Buffer;
using SymbianETMLib.Common.State;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Utilities;
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.Config
{
    public abstract class ETConfigBase
    {
        #region Constructors
        protected ETConfigBase( ETBufferBase aBuffer )
        {
            iBuffer = aBuffer;
            //
            RegisterETMControl = 0xC000; // 4 byte context id
            RegisterSysConControl = 1 << 13; // Assume HIVECS;
            //
            SetExceptionVector( TArmExceptionVector.EReset, 0 );
            SetExceptionVector( TArmExceptionVector.EUndefinedInstruction, 0 );
            SetExceptionVector( TArmExceptionVector.ESVC, 0 );
            SetExceptionVector( TArmExceptionVector.EPrefetchAbort, 0 );
            SetExceptionVector( TArmExceptionVector.EDataAbort, 0 );
            SetExceptionVector( TArmExceptionVector.EIRQ, 0 );
            SetExceptionVector( TArmExceptionVector.EFIQ, 0 );
        }
        #endregion

        #region API
        public void AddContextIdMapping( uint aId, string aName )
        {
            // Clean the context id so that it matches the kind of data 
            // we see on the phone.
            uint id = aId & 0x0FFFFFFF;

            if ( !iContextIDs.ContainsKey( id ) )
            {
                iContextIDs.Add( id, aName );
            }
        }

        public void SetExceptionVector( TArmExceptionVector aVector, uint aInstruction )
        {
            if ( !iExceptionVectors.ContainsKey( aVector ) )
            {
                iExceptionVectors.Add( aVector, aInstruction );
            }
            else
            {
                iExceptionVectors[ aVector ] = aInstruction;
            }
        }

        public string GetContextID( uint aID )
        {
            string ret = "Unknown Thread";
            //
            uint lastKey = 0;
            //
            foreach ( KeyValuePair<uint, string> kvp in iContextIDs )
            {
                if ( kvp.Key > aID )
                {
                    if ( lastKey != 0 )
                    {
                        ret = kvp.Value;//iContextIDs[ lastKey ];
                    }
                    break;
                }
                else
                {
                    lastKey = kvp.Key;
                }
            }
            //
            return ret;
        }
        #endregion

        #region API - internal
        internal uint GetExceptionVector( TArmExceptionVector aVector )
        {
            uint ret = iExceptionVectors[ aVector ];
            return ret;
        }

        internal bool IsExceptionVector( uint aAddress )
        {
            bool ret = false;
            
            // Get current vector setting and also the vector address range
            TArmExceptionVectorLocation type = ExceptionVectorLocation;
            uint min = (uint) type;
            uint max = min + (uint) TArmExceptionVector.EFIQ;
            //
            ret = ( aAddress >= min && aAddress <= max );
            //
            return ret;
        }

        internal TArmExceptionVector MapToExceptionVector( uint aAddress )
        {
            System.Diagnostics.Debug.Assert( IsExceptionVector( aAddress ) );
            // 
            TArmExceptionVector ret = TArmExceptionVector.EUndefinedInstruction;
            //
            uint baseAddress = (uint) ExceptionVectorLocation;
            uint delta = aAddress - baseAddress;
            switch ( delta )
            {
            case (uint) TArmExceptionVector.EReset:
            case (uint) TArmExceptionVector.EUndefinedInstruction:
            case (uint) TArmExceptionVector.ESVC:
            case (uint) TArmExceptionVector.EPrefetchAbort:
            case (uint) TArmExceptionVector.EDataAbort:
            case (uint) TArmExceptionVector.EIRQ:
            case (uint) TArmExceptionVector.EFIQ:
                ret = (TArmExceptionVector) delta;
                break;
            default:
                throw new ETMException( "ERROR - specified address is an unsupported vector location" );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public bool Verbose
        {
            get { return iVerbose; }
            set { iVerbose = value; }
        }

        public bool BBCModeEnabled
        {
            get
            {
                //                        10 0 0 000 00 01
                SymMask mask = new SymMask( "1 # ### ## ##" );
                uint val = mask.Apply( RegisterETMControl );
                return ( val != 0 );
            }
        }

        public int ContextIDSize
        {
            get
            {
                // Bits [15:14] define the context id size.
                uint ret = RegisterETMControl & 0xC000; // b11000000 00000000
                ret >>= 14;
                if ( ret > 0 )
                {
                    ++ret;
                }
                return (int) ret;
            }
        }

        public uint RegisterETMControl
        {
            get { return iRegistersETM[ TArmRegisterType.EArmReg_ETM_Control ].Value; }
            set { iRegistersETM[ TArmRegisterType.EArmReg_ETM_Control ].Value = value; }
        }

        public uint RegisterETMId
        {
            get { return iRegistersETM[ TArmRegisterType.EArmReg_ETM_Id ].Value; }
            set { iRegistersETM[ TArmRegisterType.EArmReg_ETM_Id ].Value = value; }
        }

        public uint RegisterSysConControl
        {
            get { return iRegistersCoProSystemControl[ TArmRegisterType.EArmReg_SysCon_Control ].Value; }
            set { iRegistersCoProSystemControl[ TArmRegisterType.EArmReg_SysCon_Control ].Value = value; }
        }

        public TArmExceptionVectorLocation ExceptionVectorLocation
        {
            get
            {
                TArmExceptionVectorLocation ret = TArmExceptionVectorLocation.ENormal;
                uint mask = (uint) ( 1 << 13 );
                if ( ( RegisterSysConControl & mask ) == mask )
                {
                    ret = TArmExceptionVectorLocation.EHigh;
                }
                return ret;
            }
        }

        internal TETMBranchCompressionScheme BranchCompressionScheme
        {
            get
            {
                TETMBranchCompressionScheme ret = TETMBranchCompressionScheme.EOriginal;
                                // 101011 100 1 0000 00001111 00001111
                SymMask mask = new SymMask( "1 #### ######## ########" );
                if ( mask.IsMatch( RegisterETMId ) )
                {
                    ret = TETMBranchCompressionScheme.EAlternative;
                }
                return ret;
            }
        }
        #endregion

        #region Internal methods
        protected ETBufferBase Buffer
        {
            get { return iBuffer; }
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly ETBufferBase iBuffer;
        private bool iVerbose = false;
        private ArmETMRegisterCollection iRegistersETM = new ArmETMRegisterCollection();
        private ArmCoProSystemControlRegisterCollection iRegistersCoProSystemControl = new ArmCoProSystemControlRegisterCollection();
        private Dictionary<TArmExceptionVector, uint> iExceptionVectors = new Dictionary<TArmExceptionVector, uint>();
        private SortedDictionary<uint, string> iContextIDs = new SortedDictionary<uint, string>();
        #endregion
    }
}
