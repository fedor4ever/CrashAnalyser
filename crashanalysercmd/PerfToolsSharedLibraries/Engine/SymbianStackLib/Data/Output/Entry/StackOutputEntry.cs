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
using System.Drawing;
using System.IO;
using System.Text;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Constants;
using SymbianUtils.Range;
using SymbianUtils.Utilities;

namespace SymbianStackLib.Data.Output.Entry
{
    public class StackOutputEntry
    {
        #region Constructors
        public StackOutputEntry( uint aAddress, uint aData, DbgViewSymbol aSymbolView )
            : this( aAddress, aData )
        {
            FindNearestSymbol( aSymbolView );
        }

        public StackOutputEntry( uint aAddress, uint aData )
        {
            iAddress = aAddress;
            iData = aData;
            iDataAsString = RawByteUtility.CreateCharacterisedData( aData );
        }

        internal StackOutputEntry( StackOutputEntry aCopy )
        {
            iAddress = aCopy.Address;
            iData = aCopy.Data;
            iDataAsString = aCopy.DataAsString;
            iSymbol = aCopy.Symbol;
            iFlags = aCopy.iFlags;
            iAssociatedBinary = aCopy.AssociatedBinary;
            iAssociatedRegister = aCopy.AssociatedRegister;
            iTag = aCopy.Tag;
        }
        #endregion

        #region API
        public void FindNearestSymbol( DbgViewSymbol aSymbolView )
        {
            SymbolCollection collection = null;
            iSymbol = aSymbolView.Lookup( this.Data, out collection );
            //
            if ( collection != null )
            {
                // FIXME: should this be device file name if available, and host file name if all else fails?
                AssociatedBinary = Path.GetFileName( collection.FileName.FileNameInHost );
            }
        }

        public string GetSuggestedToolTipText()
        {
            string ret = string.Empty;
            //
            if ( IsCurrentStackPointerEntry )
            {
                ret = "Current stack pointer address";
            }
            else if ( IsRegisterBasedEntry )
            {
                ret = "Value obtained from register " + AssociatedRegisterName;
            }
            else if ( Symbol != null )
            {
                ret = "From: " + Symbol.Object;
            }
            else if ( AssociatedBinary != string.Empty )
            {
                ret = "From: " + AssociatedBinary;
            }
            //
            return ret;
        }

        public void GetSuggestedColours( out Color aFore, out Color aBack )
        {
            aFore = Color.Black;
            aBack = Color.Transparent;
            //
            if ( IsRegisterBasedEntry )
            {
                aBack = Color.LightSteelBlue;
            }
            else if ( IsCurrentStackPointerEntry )
            {
                aBack = Color.Pink;
            }
            else if ( IsOutsideCurrentStackRange )
            {
                aFore = Color.DarkGray;
            }
            else if ( IsGhost )
            {
                aFore = Color.DarkGray;
            }
            else if ( Symbol != null )
            {
                aFore = Color.DarkBlue;
            }
        }
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
        }

        public AddressRange AddressRange
        {
            get { return new AddressRange( Address, Address + 3 ); }
        }

        public uint Data
        {
            get { return iData; }
        }

        public string DataAsString
        {
            get { return iDataAsString; }
        }

        public Symbol Symbol
        {
            get { return iSymbol; }
        }

        public TArmRegisterType AssociatedRegister
        {
            get { return iAssociatedRegister; }
            set { iAssociatedRegister = value; }
        }

        public string AssociatedRegisterName
        {
            get { return ArmRegister.GetTypeName( iAssociatedRegister ); }
        }

        public string AssociatedBinary
        {
            get { return iAssociatedBinary; }
            set { iAssociatedBinary = value; }
        }

        public string Object
        {
            get
            {
                string ret = AssociatedBinary;
                //
                if ( Symbol != null )
                {
                    ret = Symbol.Object;
                }
                //
                return ret;
            }
        }

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        public uint FunctionOffset
        {
            get
            {
                uint ret = 0;
                //
                if ( iSymbol != null )
                {
                    ret = iSymbol.Offset( this.Data );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Flag query properties
        public bool IsCurrentStackPointerEntry
        {
            get { return ( iFlags & TFlags.EFlagsIsCurrentStackPointerEntry ) == TFlags.EFlagsIsCurrentStackPointerEntry; }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsCurrentStackPointerEntry;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsCurrentStackPointerEntry;
                }
            }
        }

        public bool IsOutsideCurrentStackRange
        {
            get { return ( iFlags & TFlags.EFlagsIsOutsideCurrentStackRange ) == TFlags.EFlagsIsOutsideCurrentStackRange; }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsOutsideCurrentStackRange;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsOutsideCurrentStackRange;
                }
            }
        }

        public bool IsAccurate
        {
            get { return ( iFlags & TFlags.EFlagsIsAccurate ) == TFlags.EFlagsIsAccurate; }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsAccurate;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsAccurate;
                }
            }
        }

        public bool IsRegisterBasedEntry
        {
            get { return ( iFlags & TFlags.EFlagsIsRegisterBasedEntry ) == TFlags.EFlagsIsRegisterBasedEntry; }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsRegisterBasedEntry;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsRegisterBasedEntry;
                }
            }
        }

        public bool IsGhost
        {
            get { return ( iFlags & TFlags.EFlagsIsGhost ) == TFlags.EFlagsIsGhost; }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsGhost;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsGhost;
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            const int KNormalAddressWidth = 10;

            StringBuilder ret = new StringBuilder();
            if ( IsRegisterBasedEntry )
            {
                string prefix = "[ " + AssociatedRegisterName;
                prefix = prefix.PadRight( KNormalAddressWidth - 2 );
                prefix += " ]";
                ret.AppendFormat( "{0} = {1:x8} {2} ", prefix, Data, DataAsString );
            }
            else
            {
                ret.AppendFormat( "[{0:x8}] = {1:x8} {2} ", Address, Data, DataAsString );
            }
            //
            if ( iSymbol != null && !iSymbol.IsDefault )
            {
                string baseAddressOffset = Symbol.ToStringOffset( Data );
                ret.AppendFormat( "{0} {1}", baseAddressOffset, Symbol.Name );
            }
            else if ( AssociatedBinary != string.Empty )
            {
                ret.AppendFormat( "{0} {1}", SymbolConstants.KUnknownOffset, AssociatedBinary );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal flags
        [Flags]
        private enum TFlags
        {
            EFlagsNone = 0,
            EFlagsIsCurrentStackPointerEntry = 1,
            EFlagsIsOutsideCurrentStackRange = 2,
            EFlagsIsAccurate = 4,
            EFlagsIsRegisterBasedEntry = 8,
            EFlagsIsGhost = 16
        }
        #endregion

        #region Data members
        private readonly uint iAddress;
        private readonly uint iData;
        private readonly string iDataAsString;
        private Symbol iSymbol = null;
        private TFlags iFlags = TFlags.EFlagsNone;
        private string iAssociatedBinary = string.Empty;
        private TArmRegisterType iAssociatedRegister = TArmRegisterType.EArmReg_Other;
        private object iTag = null;
        #endregion
    }
}
