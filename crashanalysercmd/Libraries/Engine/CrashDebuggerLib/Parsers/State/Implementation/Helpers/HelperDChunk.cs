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
using CrashDebuggerLib.Structures.Chunk;
using CrashDebuggerLib.Structures.CodeSeg;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperDChunk : HelperDObject
    {
        #region Constructors
        public HelperDChunk()
        {
        }
        #endregion

        #region API
        public void CreateMonitorChunk( ParserEngine aEngine, string aName, DChunk aChunk )
        {
            ParserParagraph para0 = base.CreateMonitorObjectParagraph( aName, aChunk );
            aEngine.Add( para0 );
            ParserParagraph para1 = CreateChunkMultiple( aName, aChunk );
            aEngine.Add( para1 );
 
            // TODO: add support for older memory models?
        }
        #endregion

        #region Call-back methods
        #endregion

        #region Internal methods
        private ParserParagraph CreateChunkMultiple( string aName, DChunk aChunk )
        {
            ParserParagraph para = new ParserParagraph( aName );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Owning Process %08x OS ASIDS %08x\r\n" );
            l1.SetTargetProperties( aChunk, "OwningProcessAddress", "OSAsids" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "Size %x, MaxSize %x, Base %08x\r\n" );
            l2.SetTargetProperties( aChunk, "Size", "MaxSize", "Base" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "Attrib %x, StartPos %x\r\n" );
            l3.SetTargetProperties( aChunk, "Attributes", "StartPos" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "Type %d\r\n" );
            l4.SetTargetProperty( aChunk, "ChunkType" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "PTE: %08x, PDE: %08x\r\n" );
            l5.SetTargetProperties( aChunk.Permissions, "Pte", "Pde" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "PageTables=%08x, PageBitMap=%08x\r\n" );
            l6.SetTargetProperties( aChunk, "PageTables", "PageBitMap" );

            para.Add( l1, l2, l3, l4, l5, l6 );
            return para;
        }
        #endregion
    }
}
