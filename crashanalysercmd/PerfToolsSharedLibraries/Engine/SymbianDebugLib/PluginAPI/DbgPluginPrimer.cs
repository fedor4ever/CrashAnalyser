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
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;

namespace SymbianDebugLib.PluginAPI
{
    public abstract class DbgPluginPrimer
    {
        #region Enumerations
        public enum TPrimeEvent
        {
            EEventPrimingStarted = 0,
            EEventPrimingProgress,
            EEventPrimingComplete
        }
        #endregion

        #region Delegates & events
        public delegate void PrimeEventHandler( TPrimeEvent aEvent, object aData );
        public event PrimeEventHandler EventHandler;
        #endregion

        #region Constructors
        protected DbgPluginPrimer( DbgPluginEngine aEngine )
		{
            iEngine = aEngine;
		}
		#endregion

		#region Framework API
        public abstract void Add( DbgEntity aEntity );

        public abstract void Prime( TSynchronicity aSynchronicity );

        protected abstract int Count
        {
            get;
        }
        #endregion

        #region Properties
        public bool ResetEngineBeforePriming
        {
            get { return iResetEngineBeforePriming; }
            set { iResetEngineBeforePriming = value; }
        }

        protected bool IsComplete
        {
            get
            {
                int completedCount = 0;
                lock ( iCompleted )
                {
                    completedCount = iCompleted.Count;
                }
                int totalCount = Count;
                bool primeCompleted = ( completedCount == totalCount );
                return primeCompleted;
            }
        }
        #endregion

        #region Framework API
        protected DbgPluginEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal framework methods
        protected virtual void OnPrepareToPrime()
        {
            lock ( iCompleted )
            {
                iCompleted.Clear();
            }
            lock ( iSourceProgressValues )
            {
                iSourceProgressValues.Clear();
            }
            iLastReportedProgress = -1;
        }

        protected virtual void OnPrimeComplete()
        {
            lock ( iSourceProgressValues )
            {
                iSourceProgressValues.Clear();
            }
            lock ( iCompleted )
            {
                iCompleted.Clear();
            }
            //
            ReportEvent( TPrimeEvent.EEventPrimingComplete, null );
        }

        protected void ReportProgressIfNeeded( bool aEntireOperationComplete )
        {
            int newProgress = ( aEntireOperationComplete ? 100 : TotalProgress );
            if ( newProgress > iLastReportedProgress )
            {
                // Set to max completion value if all sources are finished.
                if ( aEntireOperationComplete )
                {
                    newProgress = 100;
                }

                iLastReportedProgress = newProgress;
                ReportEvent( TPrimeEvent.EEventPrimingProgress, newProgress );
            }
        }

        protected bool AddToCompleted( object aEntity )
        {
            lock ( iCompleted )
            {
                iCompleted.Add( aEntity );
            }
            //
            bool primeCompleted = primeCompleted = this.IsComplete;
            SaveLatestProgress( aEntity, 100 );
            return primeCompleted;
        }

        protected void RemoveFromCompleted( object aEntity )
        {
            lock ( iCompleted )
            {
                iCompleted.Remove( aEntity );
            }
            lock ( iSourceProgressValues )
            {
                if ( iSourceProgressValues.ContainsKey( aEntity ) )
                {
                    iSourceProgressValues.Remove( aEntity );
                }
            }
        }

        protected void SaveLatestProgress( object aEntity, int aProgress )
        {
            lock ( iSourceProgressValues )
            {
                if ( !iSourceProgressValues.ContainsKey( aEntity ) )
                {
                    iSourceProgressValues.Add( aEntity, aProgress );
                }
                else
                {
                    iSourceProgressValues[ aEntity ] = aProgress;
                }
            }
        }

        protected void ReportEvent( TPrimeEvent aEvent, object aData )
        {
            if ( EventHandler != null )
            {
                EventHandler( aEvent, aData );
            }
        }

        protected int TotalProgress
        {
            get
            {
                long combined = 0;
                //
                lock ( iSourceProgressValues )
                {
                    foreach ( KeyValuePair<object, int> kvp in iSourceProgressValues )
                    {
                        combined += kvp.Value;
                    }
                }
                
                // Scale by number of objects that are reporting progress
                int count = Count;
                long maxCompletionValue = ( count * 100 );
                //
                float scaled = ((float) combined ) / (float) maxCompletionValue;
                return (int) ( scaled * 100.0f );
            }
        }
        #endregion

        #region Data members
        private readonly DbgPluginEngine iEngine;
        private List<object> iCompleted = new List<object>();
        private Dictionary<object, int> iSourceProgressValues = new Dictionary<object, int>();
        private int iLastReportedProgress = -1;
        private bool iResetEngineBeforePriming = false;
        #endregion
    }
}
