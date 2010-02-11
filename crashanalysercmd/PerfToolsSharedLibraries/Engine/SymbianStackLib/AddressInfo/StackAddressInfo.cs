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
using System.Text.RegularExpressions;
using System.IO;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.Range;
using SymbianStackLib.Exceptions;

namespace SymbianStackLib.AddressInfo
{
    /// <summary>
    /// Stack flows from the base upwards:
    /// 
    /// -------------- TOP ---------------- 0x402000
    /// |
    /// |           Unused Stack
    /// |
    /// |----------- CURRENT SP ----------- 0x4033b8
    /// |
    /// |                ^
    /// |                |
    /// |
    /// |           Used Stack [Stack 
    /// |                       Pointer 
    /// |                |      Range]
    /// |                |
    /// |                |
    /// |
    /// -------------- BASE --------------- 0x407000
    /// </summary>
    public sealed class StackAddressInfo
    {
        #region Constructors
        internal StackAddressInfo()
        {
        }
        #endregion

        #region API
        public void Set( uint aSP, uint aTop, uint aBase )
        {
            Pointer = aSP;
            Top = aTop;
            Base = aBase;
        }

        internal void Validate()
        {
            if ( Top != 0 && Base < Top )
            {
                throw new StackAddressException( StackAddressException.TType.ETypeBaseAddressBeforeTopAddress );
            }
            else if ( Base != 0 && Top > Base )
            {
                throw new StackAddressException( StackAddressException.TType.ETypeTopAddressAfterBaseAddress );
            }
            else if ( Pointer == 0 ) 
            {
                throw new StackAddressException( StackAddressException.TType.ETypePointerIsNull );
            }
            else if ( Pointer > Base )
            {
                throw new StackAddressException( StackAddressException.TType.ETypePointerOutOfBounds );
            }
            else if ( Pointer < Top )
            {
                // Not an error as possibly a stack overflow...
            }
        }

        internal bool IsWithinStackDomain( DataBufferByte aEntry )
        {
            bool ret = Range.Contains( aEntry.Address );
            return ret;
        }

        internal bool IsWithinCurrentStackDomain( DataBufferByte aEntry )
        {
            bool ret = StackPointerRange.Contains( aEntry.Address );
            return ret;
        }
        #endregion

        #region Properties
        public uint Base
        {
            get { return iBase; }
            set
            {
                iBase = value; 
            }
        }

        public uint Top
        {
            get { return iTop; }
            set
            {
                iTop = value; 
            }
        }

        public uint Pointer
        {
            get { return iPointer; }
            set
            {
                iPointer = value;
                
                // Don't record that we've set the stack pointer if somebody is just
                // zeroing it out.
                if ( value != 0 )
                {
                    HaveSetStackPointer = true;
                }
            }
        }

        public AddressRange Range
        {
            get { return new AddressRange( Top, Base ); }
            set
            {
                Top = value.Min;
                Base = value.Max;
            }
        }

        public AddressRange StackPointerRange
        {
            get
            {
                AddressRange ret = new AddressRange( Pointer, Base );

                // If we don't have a valid SP value, then use the entire stack
                if ( Pointer == 0 )
                {
                    ret = Range;
                }
                //
                return ret;
            }
        }

        public AddressRange StackPointerRangeWithExtensionArea
        {
            get
            {
                AddressRange ret;
                //
                uint pointer = Pointer;
                uint pointerMinusExtension = pointer - KStackRangeExtension;
                if ( pointer == 0 || pointerMinusExtension < 0 || pointerMinusExtension < Top )
                {
                    ret = Range;
                }
                else
                {
                    ret = StackPointerRange;
                    ret.Min = pointerMinusExtension;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        internal bool HaveSetStackPointer
        {
            get { return iHaveSetSP; }
            set { iHaveSetSP = value; }
        }
        #endregion

        #region Internal constants
        private const int KStackRangeExtension = 12 * 4;
        #endregion

        #region Data members
        private uint iBase = 0;
        private uint iTop = 0;
        private uint iPointer = 0;
        private bool iHaveSetSP = false;
        #endregion
    }
}
