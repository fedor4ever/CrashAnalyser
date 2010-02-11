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
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;

namespace SymbianDebugLib.PluginAPI
{
    public abstract class DbgPluginEngine : DisposableObject, ITracer
    {
        #region Constructors
        protected DbgPluginEngine( DbgEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Clear()
        {
            foreach ( DbgPluginView view in iViews )
            {
                view.Dispose();
            }
            System.Diagnostics.Debug.Assert( iViews.Count == 0 );
            iViews.Clear();
            //
            DoClear();
        }

        public bool IsSupported( string aFileName )
        {
            string notNeeded;
            return IsSupported( aFileName, out notNeeded );
        }

        public DbgPluginView CreateView( string aName )
        {
            DbgPluginView ret = this.DoCreateView( aName );
            return ret;
        }
        #endregion

        #region API - framework
        public abstract bool IsReady
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract DbgPluginPrimer CreatePrimer();

        public abstract bool IsSupported( string aFileName, out string aType );

        public virtual object CustomOperation( string aName, object aParam1, object aParam2 )
        {
            throw new NotSupportedException();
        }

        public virtual void PrepareToPrime( DbgEntityList aEntities )
        {
        }

        protected abstract DbgPluginView DoCreateView( string aName );

        protected abstract void DoClear();
        #endregion

        #region API - views
        internal void CloseView( DbgPluginView aView )
        {
            iViews.Remove( aView );
        }
        #endregion

        #region Properties
        public DbgEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            iEngine.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iEngine.Trace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private List<DbgPluginView> iViews = new List<DbgPluginView>();
        #endregion
    }
}
