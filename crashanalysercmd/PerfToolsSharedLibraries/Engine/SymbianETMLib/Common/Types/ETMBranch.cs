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
using SymbianStructuresLib.Debug.Symbols;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.SecurityMode;
using SymbianETMLib.Common.Utilities;

namespace SymbianETMLib.Common.Types
{
    public class ETMBranch
    {
        #region Constructors
        public ETMBranch( SymAddress aAddress, int aNumber, TETMBranchType aType, TArmInstructionSet aInstructionSet, TArmExceptionType aExceptionType )
        {
            iAddress = aAddress;
            iNumber = aNumber;
            iType = aType;
            iInstructionSet = aInstructionSet;
            iExceptionType = aExceptionType;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public SymAddress Address
        {
            get { return iAddress; }
        }

        public int Number
        {
            get { return iNumber; }
        }

        public TETMBranchType Type
        {
            get { return iType; }
        }

        public TArmInstructionSet InstructionSet
        {
            get { return iInstructionSet; }
        }

        public TArmExceptionType ExceptionType
        {
            get { return iExceptionType; }
        }

        public Symbol Symbol
        {
            get { return iSymbol; }
            set
            {
                if ( iSymbol != value )
                {
                    iFlags &= ~TFlags.EFlagsSymbolTextSetExplicitly;
                    iSymbol = value;
                    BuildSymbolText();
                }
            }
        }

        public uint SymbolAddressOffset
        {
            get
            {
                uint ret = 0;
                if ( iSymbol != null )
                {
                    ret = iSymbol.Offset( iAddress );
                }
                return ret;
            }
        }

        public string SymbolText
        {
            get { return iSymbolText; }
            set
            {
                iSymbolText = value;
                iFlags |= TFlags.EFlagsSymbolTextSetExplicitly;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = ToString( 0 );
            return ret;
        }
 
        public string ToString( int aDepth )
        {
            StringBuilder text = new StringBuilder();

            // Counter
            text.AppendFormat( "[{0:d4}] ", iNumber );

            // Instruction set
            text.Append( ETMEnumToTextConverter.ToString( this.InstructionSet ) );

            // Exception mode
            string processorState = ETMEnumToTextConverter.ToString( this.ExceptionType );
            text.AppendFormat( string.Format( "    [{0}]", processorState ).PadRight( 33, ' ' ) );

            // Address
            text.AppendFormat( " @ 0x{0} ", iAddress.AddressHex );

            // Symbol
            if ( this.Symbol != null )
            {
                text.AppendFormat( "[0x{0:x8} +0x{1:x4}] ", this.Symbol.Address, this.SymbolAddressOffset );
            }
            else
            {
                text.Append( "[  ????????   +????] " );
            }
            
            // Add padding for depth
            for ( int i = 0; i < aDepth; i++ )
            {
                text.Append( "  " );
            }

            // Finally, add the symbol
            text.Append( this.SymbolText );

            return text.ToString();
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags : byte
        {
            EFlagsNone = 0,
            EFlagsSymbolTextSetExplicitly
        }
        #endregion

        #region Internal constants
        private const string KUnknownSymbol = "????";
        #endregion

        #region Internal methods
        private void BuildSymbolText()
        {
            if ( ( iFlags & TFlags.EFlagsSymbolTextSetExplicitly ) != TFlags.EFlagsSymbolTextSetExplicitly )
            {
                if ( iSymbol != null )
                {
                    StringBuilder ret = new StringBuilder();
                    ret.AppendFormat( "{0}     [{1}]", iSymbol.Name, iSymbol.Object );
                    iSymbolText = ret.ToString();
                }
                else
                {
                    iSymbolText = KUnknownSymbol;
                }
            }
        }
        #endregion

        #region Data members
        private readonly int iNumber;
        private readonly SymAddress iAddress;
        private readonly TETMBranchType iType;
        private readonly TArmInstructionSet iInstructionSet;
        private readonly TArmExceptionType iExceptionType;
        private Symbol iSymbol = null;
        private string iSymbolText = string.Empty;
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
