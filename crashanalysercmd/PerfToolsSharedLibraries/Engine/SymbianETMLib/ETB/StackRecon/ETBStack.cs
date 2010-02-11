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
using SymbianETMLib.Common.Engine;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Utilities;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;

namespace SymbianETMLib.ETB.StackRecon
{
    internal class ETBStack
    {
        #region Constructors
        public ETBStack( ETEngineBase aEngine, uint aContextId )
        {
            iEngine = aEngine;
            iContextId = aContextId;
        }
        #endregion

        #region API
        internal void HandleBranch( ETMBranch aBranch )
        {
            TETBStackEntry last = LastEntry;
            if ( last != null )
            {
                TETBStackEntry entry = new TETBStackEntry( aBranch );
                //
                if ( last.SymbolAddress == entry.SymbolAddress )
                {
                    if ( entry.SymbolOffset > last.SymbolOffset )
                    {
                        // Internal branch? E.g. if statement, or loop?
                    }
                    else
                    {
                        PushBranch( entry );
                    }
                }
                else if ( aBranch.SymbolAddressOffset == 0 && aBranch.Symbol != null )
                {
                    // Guess: calling a new function
                    PushBranch( entry );
                }
                else
                {
                    bool save = true;

                    // Check if we have branched back to an earlier function without
                    // popping back through the call stack
                    int count = iEntries.Count;
                    for ( int i = count - 2; i >= 0; i-- )
                    {
                        last = iEntries[ i ];
                        if ( last.SymbolAddress == entry.SymbolAddress )
                        {
                            if ( entry.SymbolOffset > last.SymbolOffset )
                            {
                                // We appear to have jumped back to a calling function - so
                                // discard later items on stack.
                                int deleteCount = count - i - 1;
                                iEntries.RemoveRange( i + 1, deleteCount );
                                save = false;
                                break;
                            }
                        }
                    }

                    if ( save )
                    {
                        PushBranch( entry );
                    }
                }
            }
            else
            {
                PushBranch( aBranch );
            }
        }
        #endregion

        #region Properties
        public uint ContextId
        {
            get { return iContextId; }
        }

        public string ContextName
        {
            get { return iEngine.Config.GetContextID( iContextId ); }
        }
        #endregion

        #region Internal class
        private class TETBStackEntry
        {
            #region Constructors
            public TETBStackEntry( ETMBranch aBranch )
            {
                iBranch = aBranch;
            }

            #endregion

            #region API
            public void Print()
            {
                string text = iBranch.ToString( iDepth );
                System.Console.WriteLine( text );
            }
            #endregion

            #region Properties
            public uint SymbolAddress
            {
                get
                { 
                    uint ret = 0;
                    //
                    if ( iBranch.Symbol != null )
                    {
                        ret = (uint) iBranch.Symbol.Address;
                    }
                    //
                    return ret;
                }
            }

            public uint SymbolOffset
            {
                get { return iBranch.SymbolAddressOffset; }
            }

            public int Depth
            {
                get { return iDepth; }
                set { iDepth = value; }
            }

            public bool IsUnknown
            {
                get
                {
                    return ( iBranch.Symbol == null );
                }
            }
            #endregion

            #region From System.Object
            public override string ToString()
            {
                StringBuilder t = new StringBuilder();
                t.AppendFormat( "0x{0} [+{1:x4}] {2}", iBranch.Address.AddressHex, iBranch.SymbolAddressOffset, iBranch.SymbolText );
                return t.ToString();
            }
            #endregion

            #region Data members
            private readonly ETMBranch iBranch;
            private int iDepth = 0;
            #endregion
        }
        #endregion

        #region Internal methods
        private void PushBranch( TETBStackEntry aEntry )
        {
            if ( aEntry.IsUnknown == false )
            {
                int lastDepth = 0;
                //
                TETBStackEntry lastEntry = LastEntry;
                if ( lastEntry != null )
                {
                    lastDepth = lastEntry.Depth + 1;
                }
                //
                aEntry.Depth = lastDepth;
            }
            iEntries.Add( aEntry );
            aEntry.Print();
        }

        private void PushBranch( ETMBranch aBranch )
        {
            TETBStackEntry entry = new TETBStackEntry( aBranch );
            PushBranch( entry );
        }

        private TETBStackEntry LastEntry
        {
            get
            {
                TETBStackEntry ret = null;
                //
                int count = iEntries.Count;
                if ( count > 0 )
                {
                    ret = iEntries[ count - 1 ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private readonly ETEngineBase iEngine;
        private readonly uint iContextId;
        private List<TETBStackEntry> iEntries = new List<TETBStackEntry>();
        #endregion
    }
}
