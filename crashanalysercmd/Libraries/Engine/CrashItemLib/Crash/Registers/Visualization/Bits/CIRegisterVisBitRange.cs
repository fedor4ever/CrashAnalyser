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
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Registers;
using SymbianUtils.Range;
using CrashItemLib.Crash.Registers.Visualization.Utilities;

namespace CrashItemLib.Crash.Registers.Visualization.Bits
{
    public class CIRegisterVisBitRange : CIRegisterVisBitList
    {
        #region Constructors
        public CIRegisterVisBitRange( CIContainer aContainer, uint aIndexMin, uint aIndexMax )
            : base( aContainer )
        {
            base.Range.Set( aIndexMin, aIndexMax );
        }

        public CIRegisterVisBitRange( CIContainer aContainer, uint aIndexMin, uint aIndexMax, string aCategory )
            : this( aContainer, aIndexMin, aIndexMax )
        {
            this.Category = aCategory;
        }

        public CIRegisterVisBitRange( CIContainer aContainer, uint aIndexMin, uint aIndexMax, string aCategory, string aValues )
            : this( aContainer, aIndexMin, aIndexMax, aCategory )
        {
            if ( aValues.Length != base.Range.Size )
            {
                throw new ArgumentException( "Values are outside of range" );
            }
            //
            int index = (int) aIndexMax;
            foreach ( char c in aValues )
            {
                TBit value = TBit.EBitClear;
                if ( c == VisUtilities.KBitIsSet )
                {
                    value = TBit.EBitSet;
                }
                else if ( c == VisUtilities.KBitIsClear )
                {
                }
                else
                {
                    throw new ArgumentException( "Invalid bit value" );
                }

                base.AddBit( index, value, aCategory );
                --index;
            }
        }
         #endregion

        #region API
        public void ExtractBits( uint aExtractFrom, string aMask )
        {
            foreach ( CIRegisterVisBit bit in VisUtilities.ExtractBits( Container, aExtractFrom, aMask ) )
            {
                this.Add( bit );
            }
        }

        public void ExtractBits( uint aExtractFrom, string aByte3Mask, string aByte2Mask, string aByte1Mask, string aByte0Mask )
        {
            foreach ( CIRegisterVisBit bit in VisUtilities.ExtractBits( Container, aExtractFrom, aByte3Mask, aByte2Mask, aByte1Mask, aByte0Mask ) )
            {
                this.Add( bit );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region Data members
        #endregion
    }
}
