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
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Parsers.State.Implementation.Helpers;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal abstract class StateContainerBase : State
    {
        #region Constructors
        protected StateContainerBase( CrashDebuggerParser aParser )
            : base( aParser )
        {
            DObject temp = CreateNewObject();
            iObjectType = temp.Type;
        }
        #endregion

        #region API
        public override void Prepare()
        {
            DObjectCon container = CrashDebugger.ContainerByType( iObjectType );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Container %d at %08x contains %d %S:\r\n" );
            l1[ 0 ].SetTargetProperty( container, "Index" );
            l1[ 1 ].SetTargetProperty( container, "KernelAddress" );
            l1[ 2 ].SetTargetProperty( container, "ExpectedCount" );
            l1.ElementComplete += new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( ContainerCountInfoComplete );
            //
            ParserParagraph para = new ParserParagraph( "CONTAINER [" + container.TypeDescription + "]" );
            para.Add( l1 );
            ParserEngine.Add( para );
            //
            CreateEntryDetector();
        }

        public override void Finalise()
        {
        }
        #endregion

        #region Abstract API
        protected abstract DObject CreateNewObject();

        protected virtual bool IsObjectReadyForSaving( DObject aObject )
        {
            bool ret = ( aObject.KernelAddress != 0 );
            return ret;
        }

        protected virtual void CreateEntryParagraphs( DObject aObject )
        {
            string name = "ENTRY [" + Container.TypeDescription + "]";
            ParserParagraph para = iHelperDObject.CreateMonitorObjectParagraph( name, aObject );
            ParserEngine.Add( para );
        }

        protected virtual void CreateEntryParser()
        {
            // Remove all the old entries
            ParserEngine.Reset();

            // Save last thread if it looks valid
            if ( iCurrentObject != null  )
            {
                bool ready = IsObjectReadyForSaving( iCurrentObject );
                if ( ready )
                {
                    Container.Add( iCurrentObject );
                    iCurrentObject = null;
                }
            }

            // Create a new object which will contain the next set of parser data
            iCurrentObject = CreateNewObject();
            
            // Use the helper to prepare next paragraphs
            CreateEntryParagraphs( iCurrentObject );

            // Catch the next new entry
            CreateEntryDetector();
        }

        protected virtual void CreateEntryDetector()
        {
            string containerType = Container.TypeDescription;
            ParserParagraph para = iHelperDObject.CreateEntryDetector( containerType, new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( NewElementDetected ) );
            ParserEngine.Add( para );
        }
        #endregion

        #region Properties
        protected DObjectCon Container
        {
            get { return CrashDebugger.ContainerByType( iObjectType ); }
        }

        protected DObject.TObjectType ObjectType
        {
            get
            {
                return iObjectType;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Event handlers
        void NewElementDetected( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            CreateEntryParser();
        }

        void ContainerCountInfoComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
        }
        #endregion

        #region Data members
        private DObject iCurrentObject = null;
        private HelperDObject iHelperDObject = new HelperDObject();
        private readonly DObject.TObjectType iObjectType;
        #endregion
    }
}
