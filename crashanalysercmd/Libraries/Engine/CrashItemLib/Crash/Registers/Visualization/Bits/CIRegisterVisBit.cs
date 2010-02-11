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

namespace CrashItemLib.Crash.Registers.Visualization.Bits
{
    public class CIRegisterVisBit : CIElement
    {
        #region Constructors
        public CIRegisterVisBit( CIContainer aContainer )
            : base( aContainer )
        {
        }
        
        public CIRegisterVisBit( CIContainer aContainer, int aIndex, TBit aValue, string aCategory, string aInterpretation )
            : base( aContainer )
        {
            Index = aIndex;
            Value = aValue;
            Category = aCategory;
            Interpretation = aInterpretation;
        }
        #endregion

        #region Constants
        public const string KReserved = "Reserved";
        #endregion

        #region API
        #endregion

        #region Properties
        public override string Name
        {
            get { return Category; }
            set
            {
                Category = value;
            }
        }

        public string Category
        {
            get
            {
                // Return the "Reserved" category if we are reserved
                if ( IsReserved )
                {
                    return KReserved;
                }

                return iCategory; 
            }
            set
            {
                iCategory = value;
            }
        }

        public int Index
        {
            get { return iIndex; }
            set { iIndex = value; }
        }

        public TBit Value
        {
            get { return iValue; }
            set { iValue = value; }
        }

        public string ValueString
        {
            get
            {
                switch ( Value )
                {
                default:
                case TBit.EBitClear:
                    return KBitClear;
                case TBit.EBitSet:
                    return KBitSet;
                }
            }
        }

        public string Interpretation
        {
            get
            {
                // Don't return any interpretation if the bit is reserved
                if ( IsReserved )
                {
                    return string.Empty;
                }

                return iInterpretation; 
            }
            set { iInterpretation = value; }
        }

        public bool IsReserved
        {
            get { return iIsReserved; }
            set { iIsReserved = value; }
        }

        public string ValueCharacter
        {
            get
            {
                if ( IsReserved )
                {
                    return KBitNotApplicable;
                }
                else
                {
                    switch ( Value )
                    {
                    case TBit.EBitSet:
                        return iValueCharacters[ 0 ];
                    default:
                    case TBit.EBitClear:
                        return iValueCharacters[ 1 ];
                    }
                }
            }
        }

        public string this[ TBit aBit ]
        {
            get
            {
                switch( aBit )
                {
                case TBit.EBitSet:
                    return iValueCharacters[ 0 ];
                default:
                case TBit.EBitClear:
                    return iValueCharacters[ 1 ];
                }
            }
            set
            {
                string bitVal = value;
                //
                if ( bitVal.Length > 1 )
                {
                    throw new ArgumentException( "Bit value must be a maximum of one character" );
                }
                //
                switch ( aBit )
                {
                case TBit.EBitSet:
                    iValueCharacters[ 0 ] = bitVal;
                    break;
                default:
                case TBit.EBitClear:
                    iValueCharacters[ 1 ] = bitVal;
                    break;
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KBitNotApplicable = "-";
        private const string KBitClear = "0";
        private const string KBitSet = "1";
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return ValueString;
        }
        #endregion

        #region Data members
        private string iCategory = string.Empty;
        private string iInterpretation = string.Empty;
        private int iIndex = 0;
        private TBit iValue = TBit.EBitClear;
        private bool iIsReserved = false;
        private string[] iValueCharacters = new string[] { "1", "" };
        #endregion
    }
}
