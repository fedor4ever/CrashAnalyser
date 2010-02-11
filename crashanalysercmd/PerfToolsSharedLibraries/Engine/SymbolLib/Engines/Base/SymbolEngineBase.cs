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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbolLib.CodeSegDef;
using SymbolLib.Generics;

namespace SymbolLib.Engines.Common
{
    public abstract class SymbolEngineBase : GenericSymbolEngine
    {
        #region Events
        public delegate void ParsingStarted( SymbolEngineBase aEngine, string aFileName );
        public event ParsingStarted ParsingStartedHandler;

        public delegate void ParsingProgress( SymbolEngineBase aEngine, string aFileName, int aProgress );
        public event ParsingProgress ParsingProgressHandler;

        public delegate void ParsingCompleted( SymbolEngineBase aEngine, string aFileName );
        public event ParsingCompleted ParsingCompletedHandler;

        public delegate void CollectionCreated( SymbolEngineBase aEngine, GenericSymbolCollection aCollection );
        public event CollectionCreated CollectionCreatedHandler; 
        #endregion

        #region Construct & destruct
        protected internal SymbolEngineBase( ITracer aTracer )
            : base( aTracer )
        {
        }
        #endregion

        #region API
        public abstract bool AddressInRange( long aAddress );
 
        public virtual int FileNameCount
        {
            get { return 0; }
        }

        public virtual string FileName( int aIndex )
        {
            return string.Empty;
        }

        public virtual void LoadFromFile( string aFileName, TSynchronicity aSynchronicity )
        {
            throw new NotSupportedException();
        }

        public virtual bool LoadFromDefinition( CodeSegDefinition aDefinition, TSynchronicity aSynchronicity )
        {
            throw new NotSupportedException();
        }

        public virtual void LoadFromDefinitionCollection( CodeSegDefinitionCollection aCollection, TSynchronicity aSynchronicity )
        {
            foreach ( CodeSegDefinition definition in aCollection )
            {
                LoadFromDefinition( definition, aSynchronicity );
            }
        }

        public virtual void UnloadAll()
        {
        }

        public virtual bool Unload( CodeSegDefinition aDefinition )
        {
            return false;
        }

        public virtual bool IsLoaded( CodeSegDefinition aDefinition )
        {
            return false;
        }
        #endregion

        #region Internal event dispatchers
        protected void OnParsingStarted( string aFileName )
        {
            if  ( ParsingStartedHandler != null )
            {
                ParsingStartedHandler( this, aFileName );
            }
        }

        protected void OnParsingProgress( string aFileName, int aProgress )
        {
            if  ( ParsingProgressHandler != null )
            {
                ParsingProgressHandler( this, aFileName, aProgress );
            }
        }
        
        protected void OnParsingCompleted( string aFileName )
        {
            if  ( ParsingCompletedHandler != null )
            {
                ParsingCompletedHandler( this, aFileName );
            }
        }

        protected void OnCollectionCreated( GenericSymbolCollection aCollection )
        {
            if  ( CollectionCreatedHandler != null )
            {
                CollectionCreatedHandler( this, aCollection );
            }
        }
        #endregion

        #region Data members
        #endregion
    }
}
