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
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.DataBuffer.Primer;
using SymbianStackLib.Engine;
using SymbianStackLib.Data.Source;

namespace SymbianStackLib.Data.Source.Primer
{
    public sealed class StackEnginePrimer : DataBufferPrimer
    {
        #region Constructors
        internal StackEnginePrimer( StackEngine aEngine )
            : base( aEngine.DataSource )
        {
            iEngine = aEngine;
            //
            base.LineNotHandled += new DataBufferPrimerUnhandledLine( StackEnginePrimer_LineNotHandled );
            base.PrimerComplete += new DataBufferPrimerCompleteHandler( StackEnginePrimer_PrimerComplete );
        }
        #endregion

        #region Event handlers
        void StackEnginePrimer_PrimerComplete( DataBufferPrimer aPrimer, DataBuffer aBuffer, uint aFirstByteAddress, uint aLastByteAddress )
        {
            SeedAddressRangeBasedUponData();
        }

        void StackEnginePrimer_LineNotHandled( DataBufferPrimer aPrimer, DataBuffer aBuffer, string aLine )
        {
            iEngine.Prefixes.TryAgainstPrefixes( aLine );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void SeedAddressRangeBasedUponData()
        {
            uint top = iEngine.DataSource.First.Address;
            if ( top != 0 && iEngine.AddressInfo.Top == 0 )
            {
                iEngine.AddressInfo.Top = top;
            }
            //
            uint baseAddr = iEngine.DataSource.Last.Address;
            if ( baseAddr != 0 && iEngine.AddressInfo.Base == 0 )
            {
                iEngine.AddressInfo.Base = baseAddr;
            }
        }
        #endregion

        #region Data members
        private readonly StackEngine iEngine;
        #endregion
    }
}
