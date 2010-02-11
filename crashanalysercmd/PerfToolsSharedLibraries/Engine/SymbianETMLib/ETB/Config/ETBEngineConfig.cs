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
﻿using System;
using System.Collections.Generic;
using System.Text;
using DecompressETB.Buffer;
using DecompressETB.State;
using DecompressETB.Types;

namespace DecompressETB.Config
{
    public class ETBEngineConfig
    {
        #region Constructors
        public ETBEngineConfig()
        {
            SetExceptionVector( TETBExceptionVector.EReset, 0 );
            SetExceptionVector( TETBExceptionVector.EUndefinedInstruction, 0 );
            SetExceptionVector( TETBExceptionVector.ESWI, 0 );
            SetExceptionVector( TETBExceptionVector.EPrefetchAbort, 0 );
            SetExceptionVector( TETBExceptionVector.EDataAbort, 0 );
            SetExceptionVector( TETBExceptionVector.EIRQ, 0 );
            SetExceptionVector( TETBExceptionVector.EFIQ, 0 );
        }
        #endregion

        #region API
        // <summary>
        // Use this to seed the context id to thread name mapping
        // </summary>
        public void SetRegisterContextID( uint aID, string aName )
        {
            if ( !iContextIDs.ContainsKey( aID ) )
            {
                iContextIDs.Add( aID, aName );
            }
        }

        public void SetExceptionVector( TETBExceptionVector aVector, uint aInstruction )
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
            if ( iContextIDs.ContainsKey( aID ) )
            {
                ret = iContextIDs[ aID ];
            }
            //
            return ret;
        }

        internal uint GetExceptionVector( TETBExceptionVector aVector )
        {
            uint ret = iExceptionVectors[ aVector ];
            return ret;
        }

        internal TETBExceptionVector MapToExceptionVector( uint aAddress )
        {
            System.Diagnostics.Debug.Assert( IsExceptionVector( aAddress ) );
            // 
            TETBExceptionVector ret = TETBExceptionVector.EUndefinedInstruction;
            //
            uint baseAddress = (uint) ExceptionVectorLocation;
            uint delta = aAddress - baseAddress;
            switch ( delta )
            {
            case (uint) TETBExceptionVector.EReset:
            case (uint) TETBExceptionVector.EUndefinedInstruction:
            case (uint) TETBExceptionVector.ESWI:
            case (uint) TETBExceptionVector.EPrefetchAbort:
            case (uint) TETBExceptionVector.EDataAbort:
            case (uint) TETBExceptionVector.EIRQ:
            case (uint) TETBExceptionVector.EFIQ:
                ret = (TETBExceptionVector) delta;
                break;
            default:
                throw new NotSupportedException( "Specified address is an unsupported vector location" );
                break;
            }
            //
            return ret;
        }

        internal bool IsExceptionVector( uint aAddress )
        {
            bool ret = false;
            
            // Get current vector setting and also the vector address range
            TETBExceptionVectorLocation type = ExceptionVectorLocation;
            uint min = (uint) type;
            uint max = min + (uint) TETBExceptionVector.EFIQ;
            //
            ret = ( aAddress >= min && aAddress <= max );
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

        public uint ETMControlRegister
        {
            get { return iETMControlRegister; }
            set { iETMControlRegister = value; }
        }

        public uint SystemControlRegister
        {
            get { return iSystemControlRegister; }
            set { iSystemControlRegister = value; }
        }

        public int ContextIDSize
        {
            get
            {
                // Bits [15:14] define the context id size.
                uint ret = iETMControlRegister & 0xC000; // b11000000 00000000
                ret >>= 14;
                if ( ret > 0 )
                {
                    ++ret;
                }
                return (int) ret;
            }
        }

        public TETBExceptionVectorLocation ExceptionVectorLocation
        {
            get
            {
                TETBExceptionVectorLocation ret = TETBExceptionVectorLocation.ENormal;
                uint mask = (uint) ( 1 << 13 );
                if ( ( iSystemControlRegister & mask ) == mask )
                {
                    ret = TETBExceptionVectorLocation.EHigh;
                }
                return ret;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private bool iVerbose = false;
        private uint iETMControlRegister = 0xC000; // 4 byte context id
        private uint iSystemControlRegister = 1 << 13;
        private Dictionary<TETBExceptionVector, uint> iExceptionVectors = new Dictionary<TETBExceptionVector, uint>();
        private Dictionary<uint, string> iContextIDs = new Dictionary<uint, string>();
        #endregion
    }
}
