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

namespace CrashItemLib.Crash.Registers.Visualization.Bits
{
    public abstract class CIRegisterVisBitList : CIElementList<CIRegisterVisBit>
    {
        #region Constructors
        protected CIRegisterVisBitList( CIContainer aContainer )
            : base( aContainer )
        {
        }
		#endregion

        #region API
        public void AddBit( int aIndex, TBit aValue, string aCategory )
        {
            AddBit( aIndex, aValue, aCategory, string.Empty );
        }

        public void AddBit( int aIndex, TBit aValue, string aCategory, string aInterpretation )
        {
            CIRegisterVisBit bit = new CIRegisterVisBit( Container );
            bit.Index = aIndex;
            bit.Value = aValue;
            bit.Category = aCategory;
            bit.Interpretation = aInterpretation;
            //
            Add( bit );
        }
        
        public new void Add( CIRegisterVisBit aBit )
        {
            base.Add( aBit );
        }
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
            get { return iCategory; }
            set
            {
                iCategory = value;
            }
        }

        public AddressRange Range
        {
            get { return iRange; }
        }

        public bool IsReserved
        {
            get { return iIsReserved; }
            set { iIsReserved = value; }
        }

        public string Interpretation
        {
            get
            {
                if ( IsReserved )
                {
                    return string.Empty;
                }

                return iInterpretation; 
            }
            set { iInterpretation = value; }
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
            foreach ( CIRegisterVisBit bit in this )
            {
                ret.Append( bit.ToString() );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private string iCategory = string.Empty;
        private string iInterpretation = string.Empty;
        private AddressRange iRange = new AddressRange();
        private bool iIsReserved = false;
        #endregion
    }
}
