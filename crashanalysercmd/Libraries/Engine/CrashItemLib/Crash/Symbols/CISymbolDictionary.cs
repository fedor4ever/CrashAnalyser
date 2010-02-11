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
using System.IO;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Stacks;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;

namespace CrashItemLib.Crash.Symbols
{
    public class CISymbolDictionary : CIElementList<CISymbol>
    {
        #region Constructors
        internal CISymbolDictionary( CIContainer aContainer )
            : base( aContainer )
		{
            base.IsToBeFinalizedLast = true;
		}
		#endregion

        #region API
        internal CISymbol Register( uint aSymbolAddress )
        {
            // First try to find an existing CISymbol that matches the address.
            CISymbol symbol = this[ aSymbolAddress ];
            if ( symbol == null )
            {
                // No existing entry defined, so create a new symbol which is just based upon
                // an address. 
                symbol = new CISymbol( base.Container, aSymbolAddress );
                DoRegister( symbol );
            }
            else
            {
                // Found an existing symbol object - increment reference count
                symbol.ReferenceCountIncrement();
            }
            //
            return symbol;
        }

        internal CISymbol Register( Symbol aSymbol )
        {
            // First try to find an existing CISymbol that matches the address.
            CISymbol symbol = this[ aSymbol.Address ];
            if ( symbol == null )
            {
                // No existing entry defined, so create a new symbol with explicit symbol definition
                symbol = new CISymbol( base.Container, aSymbol );
                DoRegister( symbol );
            }
            else
            {
                // Found an existing symbol object - however, does it have an associated
                // symbol yet?
                symbol.AssignPermanentSymbol( aSymbol );

                // Also, we must increment the reference count because it's now shared by another client
                symbol.ReferenceCountIncrement();
            }
            //
            return symbol;
        }

        internal CISymbol RefreshRegistration( CIElement aElement, uint aSymbolAddress, CISymbol aOldSymbolOrNull )
        {
            CISymbol ret = aOldSymbolOrNull;
            bool needToRegister = false;

            if ( aOldSymbolOrNull != null )
            {
                // We only need to do this if the symbol address changed.
                if ( aSymbolAddress != aOldSymbolOrNull.Address )
                {
                    // We must unregister the symbol from the parent element because potentially we might
                    // replace the symbol with an entirely new one.
                    aElement.RemoveChild( aOldSymbolOrNull );

                    // However, we only do this if there are no other client of the same symbol.
                    int newRefCount = aOldSymbolOrNull.ReferenceCountDecrement();
                    if ( newRefCount <= 0 )
                    {
                        // Remove old references to dead symbol
                        DoDeregister( aOldSymbolOrNull );
                    }

                    // Since the address of the symbol has changed, we will need to 
                    // re-register a new symbol
                    needToRegister = true;
                }
                else
                {
                    // The symbol address hasn't changed
                }
            }
            else if ( aSymbolAddress == 0 && aOldSymbolOrNull == null )
            {
                // Err, nothing to do here.
            }
            else
            {
                // We weren't supplied with an original symbol object which means we will
                // definitely need to register a symbol (if we don't find a pre-existing match).
                needToRegister = true;
            }

            // Do we need to register a new symbol?
            if ( needToRegister )
            {
                // Create new entry (or look up existing reference). This will also
                // ensure any new symbol is registered with this dictionary.
                ret = Register( aSymbolAddress );

                // Associate with parent element
                aElement.AddChild( ret );
            }

            return ret;
        }
        #endregion

        #region Properties
        internal CISymbol this[ uint aAddress ]
        {
            get
            {
                CISymbol ret = null;
                //
                if ( iAddressLUT.ContainsKey( aAddress ) )
                {
                    ret = iAddressLUT[ aAddress ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void DoRegister( CISymbol aSymbol )
        {
            CISymbol check = this[ aSymbol.Address ];
            System.Diagnostics.Debug.Assert( check == null );
            aSymbol.ReferenceCountIncrement();
            Add( aSymbol );
        }

        private void DoDeregister( CISymbol aSymbol )
        {
            bool alreadyRegistered = base.Contains( aSymbol );
            if ( alreadyRegistered )
            {
                Remove( aSymbol );
            }
        }
        #endregion

        #region From CIElementDictionary
        public override bool Add( CISymbol aEntry )
        {
            bool okayToAdd = iAddressLUT.ContainsKey( aEntry.Address ) == false;
            if ( okayToAdd )
            {
                base.Add( aEntry );
                iAddressLUT.Add( aEntry.Address, aEntry );
            }

            return okayToAdd;
        }

        public override void Remove( CISymbol aEntry )
        {
            bool alreadyExists = iAddressLUT.ContainsKey( aEntry.Address );
            if ( alreadyExists )
            {
                iAddressLUT.Remove( aEntry.Address );
            }
            base.Remove( aEntry );
        }
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            try
            {
                base.OnFinalize( aParams );
            }
            finally
            {
            }
        }
        #endregion

        #region Data members
        private Dictionary<uint, CISymbol> iAddressLUT = new Dictionary<uint, CISymbol>();
        #endregion
    }
}
