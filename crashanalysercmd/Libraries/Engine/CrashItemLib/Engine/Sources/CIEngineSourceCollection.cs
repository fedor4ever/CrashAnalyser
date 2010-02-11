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
using System.Collections;
using System.Collections.Generic;

namespace CrashItemLib.Engine.Sources
{
    public class CIEngineSourceCollection : IEnumerable<CIEngineSource>
    {
        #region Constructors
        public CIEngineSourceCollection( CIEngine aEngine )
		{
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iSources )
            {
                iSources.Clear();
            }
        }

        public void Add( CIEngineSource aEntry )
        {
            aEntry.Collection = this;
            lock ( iSources )
            {
                iSources.Add( aEntry );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iSources )
                {
                    return iSources.Count;
                }
            }
        }

        public CIEngineSource this[ int aIndex ]
        {
            get
            {
                lock ( iSources )
                {
                    return iSources[ aIndex ];
                }
            }
        }

        public CIEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal methods
        internal void OnSourceProgress( CIEngineSource aSource, int aProgress )
        {
            iEngine.OnSourceProgress( aSource, aProgress );
        }

        internal void OnSourceStateChanged( CIEngineSource aSource )
        {
            iEngine.OnSourceStateChanged( aSource );
        }

        internal void OnException( Exception aException )
        {
            iEngine.Trace( "[CIEngineSourceCollection] OnException() - aException: {0} / {1}", aException.Message, aException.StackTrace );
            iEngine.OnException( aException );
        }
        #endregion

        #region From IEnumerable<CIEngineSource>
        public IEnumerator<CIEngineSource> GetEnumerator()
        {
            foreach ( CIEngineSource entry in iSources )
            {
                yield return entry;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( CIEngineSource entry in iSources )
            {
                yield return entry;
            }
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private List<CIEngineSource> iSources = new List<CIEngineSource>();
        #endregion
    }
}
