/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianParserLib.Engine;
using SymbianParserLib.BaseStructures;
using CrashDebuggerLib.Structures.Scheduler;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.UserContextTable;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateInfoUserContextTable : State
    {
        #region Constructors
        public StateInfoUserContextTable( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            ParserParagraph para = new ParserParagraph( "USERCONTEXTTABLE_INFO" );
            
            // The format of the actual entry specification is the same for each
            // line
            StringBuilder lineFormat = new StringBuilder(  );
            int count = UserContextTable.EntryCount;
            for( int i=0; i<count; i++ )
            {
                lineFormat.Append( "[%02x, %02x]" );
            }

            // Create one line per table
            int tableCount = CrashDebugger.UserContextTableManager.Count;
            for( int i=0; i<tableCount; i++ )
            {
                // Create line based upon dynamic format string
                string format = String.Format( KTablePrefixFormat, i, lineFormat.ToString() );
                ParserLine line = ParserLine.NewSymFormat( format );

                // Save the context table type - we need this in the callback later on
                line.Tag = (TUserContextType) i;

                // Make sure each field stores it's value internally, so that we can extract it
                // ourselves in the callback.
                line.SetTargetObject();

                // Get a callback when all fields are ready.
                line.ElementComplete += new ParserElementBase.ElementCompleteHandler( LineComplete );

                para.Add( line );
            }

            ParserEngine.Add( para );
        }

        public override void Finalise()
        {
            // Go through each field
            CrashDebugger.UserContextTableManager.Dump();
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        void LineComplete( ParserElementBase aElement )
        {
            ParserLine line = (ParserLine) aElement;
            TUserContextType tableType = (TUserContextType) line.Tag;
            UserContextTable table = CrashDebugger.UserContextTableManager[ tableType ];

            // Each line should have a known number of entries stored within it's field collection.
            int expectedCount = UserContextTable.EntryCount * 2; // 2 fields per table entry
            int actualCount = line.Count;

            if ( expectedCount == actualCount )
            {
                for ( int i = 0; i < expectedCount; i += 2 )
                {
                    ParserField fieldType = line[ i + 0 ];
                    ParserField fieldValue = line[ i + 1 ];
                    //
                    if ( fieldType.IsUint && fieldValue.IsUint )
                    {
                        UserContextTable.TArmRegisterIndex reg = (UserContextTable.TArmRegisterIndex) ( i / 2 );
                        UserContextTableEntry entry = table[ reg ];
                        //
                        UserContextTableEntry.TType type = (UserContextTableEntry.TType) fieldType.AsUint;
                        byte value = (byte) fieldValue.AsUint;
                        //
                        entry.Type = type;
                        entry.Offset = value;
                    }
                }
            }
            else
            {
                throw new Exception( "User Context Table Corruption" );
            }
        }
        #endregion

        #region Internal constants
        private const string KTablePrefixFormat = "Table[{0:d2}] = {1}";
        #endregion

        #region Data members
        private List<ParserLine> iLines = new List<ParserLine>();
        #endregion
    }
}
