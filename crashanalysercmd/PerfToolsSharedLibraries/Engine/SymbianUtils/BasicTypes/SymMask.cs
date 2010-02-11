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

namespace SymbianUtils.BasicTypes
{
    public class SymMask
    {
        #region Enumerations
        public enum TShiftDirection
        {
            ELeft = 0,
            ERight
        }
        #endregion

        #region Constructors
        public SymMask( string aBinary )
        {
            iMaskingBits = SymBitUtils.CreateMask( aBinary, out iExpectedValueAfterMasking );
        }

        public SymMask( string aBinary, TShiftDirection aDirection, uint aCount )
            : this( aBinary )
        {
            if ( aDirection == TShiftDirection.ELeft )
            {
                iShift = -Convert.ToInt32( aCount );
            }
            else
            {
                iShift = Convert.ToInt32( aCount );
            }
        }

        public SymMask( uint aMask )
            : this( aMask, aMask, TShiftDirection.ELeft, 0 )
        {
        }

        public SymMask( uint aMask, TShiftDirection aDirection, int aCount )
            : this( aMask, aMask, aDirection, aCount )
        {
        }

        public SymMask( uint aMask, uint aValue, TShiftDirection aDirection, int aCount )
        {
            iMaskingBits = aMask;
            iExpectedValueAfterMasking = aValue;
            if ( aDirection == TShiftDirection.ELeft )
            {
                iShift = -Convert.ToInt32( aCount );
            }
            else
            {
                iShift = Convert.ToInt32( aCount );
            }
        }
        #endregion

        #region API
        public bool IsMatch( uint aValue )
        {
            //      101 0 000000000000000000000000
            //      111 0 000000000000000000000000
            // 1110 010 1 100111110000000000111100
            bool ret = ( aValue & iMaskingBits ) == iExpectedValueAfterMasking;
            return ret;
        }

        public uint Apply( uint aValue )
        {
            uint ret = ( iMaskingBits & aValue );
            if ( iShift != 0 )
            {
                if ( iShift < 0 )
                {
                    ret <<= iShift;
                }
                else
                {
                    ret >>= iShift;
                }
            }
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Operators
        #endregion

        #region From System.Object
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private int iShift = 0;
        private uint iMaskingBits = 0;
        private uint iExpectedValueAfterMasking = 0;
        #endregion
    }
}
