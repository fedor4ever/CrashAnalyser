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
using SymbianDebugLib.Engine;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Engine;
using SymbianStackLib.Interfaces;
using SymbianUtils;

namespace SymbianStackLib.Algorithms
{
    public abstract class StackAlgorithm : AsyncReaderBase
    {
        #region Constructors
        protected StackAlgorithm( IStackAlgorithmManager aManager, IStackAlgorithmObserver aObserver )
            : base( aManager )
        {
            iManager = aManager;
            iAlgorithmObserver = aObserver;
        }
        #endregion

        #region API
        public abstract string Name { get; }
        public abstract int Priority { get; }
        
        public virtual bool IsAvailable()
        {
            // We must have symbols
            bool ret = DebugEngineView.Symbols.IsReady;
            //
            base.Trace( "[StackAlgorithm] IsAvailable() - ret: {0}", ret );
            return ret;
        }

        public virtual void BuildStack( TSynchronicity aSynchronicity )
        {
            base.StartRead( aSynchronicity );
        }
        #endregion

        #region Properties
        protected IStackAlgorithmManager Manager
        {
            get { return iManager; }
        }

        protected IStackAlgorithmObserver StackObserver
        {
            get { return iAlgorithmObserver; }
        }

        protected StackEngine Engine
        {
            get { return Manager.Engine; }
        }

        protected DbgEngineView DebugEngineView
        {
            get { return iManager.DebugEngineView; }
        }

        protected StackSourceData SourceData
        {
            get { return Engine.DataSource; }
        }

        protected StackOutputData OutputData
        {
            get { return Engine.DataOutput; }
        }
        #endregion

        #region From AsyncReaderBase
        protected override void HandleReadException( Exception aException )
        {
            iAlgorithmObserver.StackBuildingException( this, aException );
        }

        protected override void NotifyEvent( AsyncReaderBase.TEvent aEvent )
        {
            switch ( aEvent )
            {
            case TEvent.EReadingStarted:
                iAlgorithmObserver.StackBuildingStarted( this );
                break;
            case TEvent.EReadingProgress:
                {
                    int progress = base.Progress;
                    if ( progress != iLastProgress )
                    {
                        iAlgorithmObserver.StackBuldingProgress( this, base.Progress );
                        iLastProgress = progress;
                    }
                    break;
                }
            case TEvent.EReadingComplete:
                iAlgorithmObserver.StackBuildingComplete( this );
                break;
            }
            //
            base.NotifyEvent( aEvent );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private readonly IStackAlgorithmManager iManager;
        private readonly IStackAlgorithmObserver iAlgorithmObserver;
        private int iLastProgress = 0;
        #endregion
    }
}
