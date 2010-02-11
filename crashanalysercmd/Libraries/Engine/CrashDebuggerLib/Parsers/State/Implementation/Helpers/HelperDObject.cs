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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.CodeSeg;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperDObject
    {
        #region Constructors
        public HelperDObject()
        {
        }
        #endregion

        #region API
        public ParserParagraph CreateEntryDetector( string aObjectName, ParserElementBase.ElementCompleteHandler aNewEntryHandler )
        {
            ParserParagraph para = new ParserParagraph( "MONITOR_ENTRY_DETECTOR" );

            // If the "new entry handler" object is utilised, the client wishes to detect when a new entry is created.
            // This detection will act as a transient call-back to the client - and won't be used to gather any specific
            // information besides that a new entry has been detected. Furthermore, the entry will be non-consuming and
            // dequeue itself once it "fires" since it's purpose has then been served.
            if ( aNewEntryHandler == null )
            {
                throw new ArgumentException( "aNewEntryHandler cannot be NULL" );
            }

            ParserLine newEntryLine = ParserLine.NewSymFormat( aObjectName.ToUpper() + " at %08x VPTR=%08x AccessCount=%d Owner=%08x\r\n" );
            newEntryLine.ElementComplete += new ParserElementBase.ElementCompleteHandler( aNewEntryHandler );
            newEntryLine.DequeueIfComplete = true;
            newEntryLine.NeverConsumesLine = true;
            para.Add( newEntryLine );
            return para;
        }

        public ParserParagraph CreateMonitorObjectParagraph( string aName, DObject aObject )
        {
            ParserParagraph para = new ParserParagraph( aName );

            // This is a real line that will gather and save information for the client...
            ParserLine l1 = ParserLine.NewSymFormat( "%S at %08x VPTR=%08x AccessCount=%d Owner=%08x\r\n" );
            l1.SetTargetProperties( aObject, "<dummy>", "KernelAddress", "VTable", "AccessCount", "OwnerAddress" );
            l1[ 0 ].SetTargetObject();
            //
            ParserLine l2 = ParserLine.NewSymFormat( "Full name %S\r\n" );
            l2.SetTargetProperties( aObject, "Name" );
            //
            para.Add( l1, l2 );
            return para;
        }
        #endregion

        #region Call-back methods
        #endregion

        #region Internal methods
        #endregion
    }
}
