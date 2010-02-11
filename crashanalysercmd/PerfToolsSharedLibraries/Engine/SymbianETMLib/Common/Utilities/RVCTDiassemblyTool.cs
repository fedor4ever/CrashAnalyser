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
﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DecompressETB.Types;
using SymbianUtils.Range;

namespace DecompressETB.Utilities
{
    public class RVCTDiassemblyUtility
    {
        #region Constructor
        public RVCTDiassemblyUtility()
        {
        }
        #endregion

        #region API
        public string Disassemble( uint aAddress )
        {
            string ret = "Unknown instruction";
            //
            RVCTFunctionComparer comparer = new RVCTFunctionComparer();
            RVCTDisassemblyFunction temp = new RVCTDisassemblyFunction( aAddress );
            int pos = iFunctions.BinarySearch( temp, comparer );
            if ( pos >= 0 && pos < iFunctions.Count )
            {
                temp = iFunctions[ pos ];
                System.Diagnostics.Debug.Assert( temp.AddressRange.Contains( aAddress ) );
                //
                ret = temp[ aAddress ];
            }
            //
            return ret;
        }

        public void Read( string aFileName, uint aGlobalBaseAddress )
        {
            using ( StreamReader reader = new StreamReader( aFileName ) )
            {
                RVCTDisassemblyFunction fn = LastFunction;
                //
                string line = reader.ReadLine();
                while ( line != null )
                {
                    Match m = null;
                    //
                    m = KRegExFunction.Match( line );
                    if ( m.Success )
                    {
                        if ( fn != null )
                        {
                            fn.Finalise();

                            // Only save functions that contain disassembly
                            if ( fn.Count > 0 )
                            {
                                iFunctions.Add( fn );
                            }
                        }

                        fn = new RVCTDisassemblyFunction( m.Groups[ "Name" ].Value, aGlobalBaseAddress );
                    }
                    else if ( fn != null )
                    {
                        fn.Offer( line );
                    }
                    //
                    line = reader.ReadLine();
                }
                
                // Finalise any pending function
                if ( fn != null )
                {
                    fn.Finalise();
                }
            }
        }
        #endregion

        #region Properties
        private RVCTDisassemblyFunction LastFunction
        {
            get
            {
                RVCTDisassemblyFunction ret = null;
                int count = iFunctions.Count;
                if ( count > 0 )
                {
                    ret = iFunctions[ count - 1 ];
                }
                return ret;
            }
        }
        #endregion

        #region Internal classes
        class RVCTFunctionComparer : IComparer<RVCTDisassemblyFunction>
        {
            public int Compare( RVCTDisassemblyFunction aLeft, RVCTDisassemblyFunction aRight )
            {
                int ret = -1;
                //
                AddressRange lr = aLeft.AddressRange;
                AddressRange rr = aRight.AddressRange;
                //
                if ( lr.Contains( rr ) || rr.Contains( lr ) )
                {
                    ret = 0;
                }
                else
                {
                    ret = lr.CompareTo( rr );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal constants
        private static readonly Regex KRegExFunction = new Regex(
              @"^\s{4}(?<Name>[A-Za-z_0-9]+)$",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
        private List<RVCTDisassemblyFunction> iFunctions = new List<RVCTDisassemblyFunction>();
        #endregion
    }

    internal class RVCTDisassemblyFunction
    {
        #region Constructor
        public RVCTDisassemblyFunction( string aName, uint aBaseAddress )
        {
            iName = aName;
            iBaseAddress = aBaseAddress;
        }

        public RVCTDisassemblyFunction( uint aAddress )
        {
            iName = string.Empty;
            iBaseAddress = aAddress;
            iAddressRange = new AddressRange( aAddress, aAddress );
        }
        #endregion

        #region API
        internal void Finalise()
        {
            int size = InstructionSize;
            iAddressRange = new AddressRange( iFirstInstructionAddress, iFirstInstructionAddress + ( iInstructions.Count * size ) - 1 );
        }

        internal void Offer( string aLine )
        {
            Match m = KRegExInstruction.Match( aLine );
            if ( m.Success )
            {
                // get offset address
                Group addressGroup = m.Groups[ "Address" ];
                uint address = uint.Parse( addressGroup.Value, System.Globalization.NumberStyles.HexNumber ) - KCodeBase;

                // Convert to global address
                address += iBaseAddress;
                if ( iFirstInstructionAddress == 0 )
                {
                    iFirstInstructionAddress = address;
                }

                // Create record
                string text = aLine.Substring( addressGroup.Index + addressGroup.Length + 1 ).Trim();
                iInstructions.Add( address, text );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iInstructions.Count; }
        }

        public int InstructionSize
        {
            get
            {
                int ret = 4;
                //
                if ( iInstructions.Count >= 2 )
                {
                    List<uint> addresses = new List<uint>();
                    //
                    foreach ( KeyValuePair<uint, string> i in iInstructions )
                    {
                        addresses.Add( i.Key );
                        if ( addresses.Count == 2 )
                        {
                            break;
                        }
                    }
                    //
                    if ( addresses.Count == 2 )
                    {
                        int diff = (int) ( addresses[ 1 ] - addresses[ 0 ] );
                        ret = diff;
                    }
                }
                //
                return ret;
            }
        }

        public AddressRange AddressRange
        {
            get
            {
                return iAddressRange;
            }
        }

        internal string this[ uint aAddress ]
        {
            get
            {
                string ret = "??? No exact match";
                //
                if ( iInstructions.ContainsKey( aAddress ) )
                {
                    ret = iInstructions[ aAddress ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal constants
        private const uint KCodeBase = 0x8000;
        private static readonly Regex KRegExInstruction = new Regex(
              "^\\s{8}\r\n0x(?<Address>.{8}):\r\n\\s{4}\r\n(?<Instruction>.{8})\r\n" +
              "\\s{4}\r\n(?<Ascii>.{4})\r\n\\s{4}\r\n(?<Memnonic>\\p{Lu}*)\r\n\\s+\r\n" +
              "(?<Params>.+)\r\n$",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
        private readonly string iName;
        private readonly uint iBaseAddress;
        private uint iFirstInstructionAddress;
        private AddressRange iAddressRange;
        private Dictionary<uint, string> iInstructions = new Dictionary<uint, string>();
        #endregion
    }
}
