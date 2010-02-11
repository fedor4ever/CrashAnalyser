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
using SymbianUtils.Tracer;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianETMLib.Common.Types;

namespace SymbianETMLib.Common.State
{
    public abstract class ETMDecodeState : ITracer
    {
        #region Constructors
        protected ETMDecodeState( ETMStateData aStateData )
        {
            iStateData = aStateData;
        }
        #endregion

        #region API
        public virtual ETMDecodeState PrepareToHandleByte( SymByte aByte )
        {
            iCurrentRawByte = aByte;
            //
            ETMDecodeState ret = HandleByte( aByte );
            return ret;
        }

        public abstract ETMDecodeState HandleByte( SymByte aByte );

        public static string MakeInstructionSetPrefix( TArmInstructionSet aInstructionSet )
        {
            string instSet;
            switch ( aInstructionSet )
            {
            default:
            case TArmInstructionSet.EARM:
                instSet = "[A]";
                break;
            case TArmInstructionSet.ETHUMB:
                instSet = "[T]";
                break;
            case TArmInstructionSet.EJAZELLE:
                instSet = "[J]";
                break;
            }
            return instSet;
        }
        #endregion

        #region Properties
        public byte CurrentRawByte
        {
            get { return iCurrentRawByte; }
        }

        public string Binary
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                ret.Append( Convert.ToString( CurrentRawByte, 2 ).PadLeft( 8, '0' ) );
                return ret.ToString();
            }
        }

        protected ETMStateData StateData
        {
            get { return iStateData; }
        }
        #endregion

        #region Internal methods
        protected StringBuilder MakeTracePacketPrefix( string aPacketName )
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( " {0} [PKT] {1}", MakeInstructionSetPrefix( StateData.CurrentInstructionSet ), aPacketName.ToUpper().PadRight( 16, ' ' ) );
            return ret;
        }

        protected virtual void DbgTrace( string aPrefix, string aPostfix )
        {
            StringBuilder trace = MakeTracePacketPrefix( aPrefix );
            trace.AppendFormat( " - 0x{0:x8} [{1:d2}] {2} {3} {4}{5}",
                StateData.CurrentAddress,
                StateData.LastBranch.KnownBits,
                StateData.CurrentAddress.AddressBinary,
                StateData.LastBranch.IsKnown ? StateData.Engine.LookUpSymbol( StateData.CurrentAddress ) : string.Empty,
                aPostfix,
                System.Environment.NewLine
              );
            //
            Trace( trace.ToString() );
        }

        protected virtual void DbgTrace( string aPrefix )
        {
            DbgTrace( aPrefix, string.Empty );
        }
        #endregion

        #region From System.Object
        #endregion

        #region From ITracer
        public void Trace( string aText )
        {
            iStateData.Engine.Trace( aText );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iStateData.Engine.Trace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly ETMStateData iStateData;
        private byte iCurrentRawByte = 0;
        #endregion
    }
}
