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
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.KernelObjects;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateContainerThreads : State
    {
        #region Constructors
        public StateContainerThreads( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            DObjectCon container = CrashDebugger.ContainerByType( DObject.TObjectType.EThread );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Container %d at %08x contains %d %S:\r\n" );
            l1[ 0 ].SetTargetProperty( container, "Index" );
            l1[ 1 ].SetTargetProperty( container, "KernelAddress" );
            l1[ 2 ].SetTargetProperty( container, "ExpectedCount" );
            l1.ElementComplete += new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( HeaderLine_ElementComplete );
            //
            ParserParagraph para = new ParserParagraph( "CONTAINER [" + container.TypeDescription + "]" );
            para.Add( l1 );
            ParserEngine.Add( para );
        }

        public override void Finalise()
        {
            
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void PrepareEntryParser()
        {
            // Junk the old paragraphs
            ParserEngine.Reset();

            // Get a handle to our destination container
            DObjectCon container = CrashDebugger.ContainerByType( DObject.TObjectType.EThread );

            // Save last thread if it looks valid
            if ( iCurrentObject != null && iCurrentObject.KernelAddress != 0 )
            {
                bool alreadyExists = container.Contains( iCurrentObject );
                if ( !alreadyExists )
                {
                    container.Add( iCurrentObject );
                }
                //
                iCurrentObject = null;
            }

            // Create a new object which will contain the next set of parser data
            iCurrentObject = new DThread( CrashDebugger );

            // Use the helper to prepare next paragraphs
            iHelper.CreateMonitorThread( ParserEngine, "ENTRY [" + container.TypeDescription + "]", iCurrentObject, new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( PC_Register_ElementComplete ) );
        }
        #endregion

        #region Event handlers
        void PC_Register_ElementComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            PrepareEntryParser();
        }

        void HeaderLine_ElementComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            PrepareEntryParser();
        }
        #endregion

        #region Data members
        private DThread iCurrentObject = null;
        private Helpers.HelperDThread iHelper = new CrashDebuggerLib.Parsers.State.Implementation.Helpers.HelperDThread();
        #endregion
    }
}
