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
using SymbianDebugLib.Engine;
using SymbianStackLib.Data.Output.Entry;
using SymbianStackLib.Engine;
using SymbianStackLib.Interfaces;
using SymbianUtils;
using SymbianUtils.PluginManager;
using SymbianUtils.Range;

namespace SymbianStackLib.Algorithms
{
    internal sealed class StackAlgorithmManager : DisposableObject, IStackAlgorithmObserver, IStackAlgorithmManager
    {
        #region Delegates & events
        public enum TEvent
        {
            EAlgorithmStarted = 0,
            EAlgorithmProgress,
            EAlgorithmComplete
        }

        public delegate void AlgorithmEventHandler( StackAlgorithmManager aAlgManager, TEvent aEvent );
        public event AlgorithmEventHandler EventHandler;

        public delegate void AlgorithmExceptionHandler( StackAlgorithmManager aAlgManager, Exception aException );
        public event AlgorithmExceptionHandler ExceptionHandler;
        #endregion

        #region Constructors
        public StackAlgorithmManager( StackEngine aEngine )
		{
            iEngine = aEngine;
            
            // Make the view name based upon the stack address range
            AddressRange range = aEngine.AddressInfo.Range;
            StringBuilder viewName = new StringBuilder();
            viewName.AppendFormat( "StackReconstructor_{0}_{1}", range, DateTime.Now.Ticks );
            iView = iEngine.DebugEngine.CreateView( viewName.ToString(), iEngine.CodeSegments );
        }
		#endregion

		#region API
        public void Reconstruct( TSynchronicity aSynchronicity )
        {
            iSynchronicity = aSynchronicity;
            try
            {
                PrepareForExecution();
                ExecuteHeadAlgorithm();
            }
            catch ( Exception e )
            {
                // The only reason an exception should occur is if none of the algorithms indicate that
                // they are ready to process stack data (probably because symbols were not provided). 
                //
                // In this situation, we must report the exception to our event handler
                if ( ExceptionHandler != null && EventHandler != null )
                {
                    // Make sure we sent the 'start' and 'end' events as well - we cannot send
                    // these events twice so it's okay to try to send them (will be ignored if
                    // already sent).
                    ReportEvent( TEvent.EAlgorithmStarted );
                    
                    // Now report the exception
                    ExceptionHandler( this, e );

                    // Indicate completion since we're not going to be able to do anything anymore
                    ReportEvent( TEvent.EAlgorithmComplete );
                }
                else
                {
                    // No exception handler so just rethrow...
                    throw e;
                }
            }
        }
		#endregion

		#region Properties
        public int Progress
        {
            get
            {
                // Scale the progress back to a fraction of the total
                lock ( this )
                {
                    int totalProgressSoFar = iProgressValueCompleted + iProgressValueCurrent;
                    float prog = ( (float) totalProgressSoFar ) / ( (float) iProgressValueMax );
                    prog *= 100.0f;
                    return (int) prog;
                }
            }
        }
        #endregion

        #region From IStackAlgorithmObserver
        public void StackBuildingStarted( StackAlgorithm aAlg )
        {
            Trace( "[SBAlgManager] StackBuildingStarted() - aAlg: {0}", aAlg );
            ReportEvent( TEvent.EAlgorithmStarted );
        }

        public void StackBuldingProgress( StackAlgorithm aAlg, int aPercent )
        {
            Trace( "[SBAlgManager] StackBuldingProgress() - aAlg: {0}, aPercent: {1}", aAlg, aPercent );
            lock ( this )
            {
                iProgressValueCurrent = aPercent;
            }
            //
            ReportEvent( TEvent.EAlgorithmProgress );
        }

        public void StackBuildingComplete( StackAlgorithm aAlg )
        {
            // If the algorithm is still queued then everything went okay. If not, then we
            // had an exception and we should therefore ignore the completion as the algorithm
            // terminated unexpectedly.
            bool stillExists = iExecutionQueue.Contains( aAlg );
            Trace( "[SBAlgManager] StackBuildingComplete() - aAlg: {0}, stillExists: {1}", aAlg, stillExists );
            if ( stillExists )
            {
                iExecutionQueue.Clear();
                ReportEvent( TEvent.EAlgorithmComplete );
            }
        }

        public void StackBuildingElementConstructed( StackAlgorithm aAlg, StackOutputEntry aEntry )
        {
            lock ( this )
            {
                iEngine.DataOutput.InsertAsFirstEntry( aEntry );
            }
        }

        public void StackBuildingException( StackAlgorithm aAlg, Exception aException )
        {
            Trace( "[SBAlgManager] STACK ALG EXCEPTION: " + aException.Message );
            Trace( "[SBAlgManager] {0}", aException.StackTrace );

            StackAlgorithm alg = CurrentAlgorithm;

            // If we're executing using the fallback entry, then we're in trouble. 
            // There is nothing we can do besides report the problem upwards.
            if ( iExecutionQueue.Count == 1 )
            {
                if ( ExceptionHandler != null )
                {
                    ExceptionHandler( this, aException );
                }
            }
            else
            {
                // Report event
                string message = string.Format( LibResources.StackAlgorithmManager_FailedAlgorithmWarning + System.Environment.NewLine + "{1}", 
                                                alg.Name, aException.Message.ToString() 
                                                );
                iEngine.ReportMessage( StackEngine.TMessageType.ETypeWarning, message );

                // The primary algorithm has failed, let's roll back to the secondary
                // by dumping the primary algorithm and starting again.
                iExecutionQueue.Dequeue();
                iEngine.DataOutput.Clear();

                // Reset progress.
                iProgressValueCompleted += 100;
                iProgressValueCurrent = 0;

                // Start next algorithm...
                ExecuteHeadAlgorithm();
            }
        }
        #endregion

        #region From IStackAlgorithmManager
        public StackEngine Engine
        {
            get { return iEngine; }
        }

        public DbgEngineView DebugEngineView
        {
            get { return iView; }
        }

        public void Trace( string aMessage )
        {
            iEngine.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iEngine.Trace( aFormat, aParams );
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iMasterAlgorithmTable != null )
                {
                    iMasterAlgorithmTable.Dispose();
                    iMasterAlgorithmTable = null;
                }
                
                if ( iView != null )
                {
                    iView.Dispose();
                    iView = null;
                }
            }
        }
        #endregion

        #region Internal properties
        private StackAlgorithm CurrentAlgorithm
        {
            get
            {
                if ( iExecutionQueue.Count == 0 )
                {
                    throw new Exception( "No stack algorithms available" );
                }
                //
                return iExecutionQueue.Peek();
            }
        }
		#endregion

        #region Internal methods
        private void PrepareForExecution()
        {
            Trace( "[SBAlgManager] PrepareForExecution() - START" );

            // Validate address info
            iEngine.AddressInfo.Validate();

            // Clear data output
            iEngine.DataOutput.Clear();

            // Reset master list
            IStackAlgorithmManager manager = (IStackAlgorithmManager) this;
            IStackAlgorithmObserver observer = (IStackAlgorithmObserver) this;
            iMasterAlgorithmTable.Load( new object[] { manager, observer } );
            SortAlgorithms();

            // Build list of algorithms we'll try to use
            PrepareExecutionQueue();

            // Work out maximum progress value for this operation
            int numberOfPendingAlgs = iExecutionQueue.Count;
            iProgressValueMax = numberOfPendingAlgs * 100;

            // Reset current progress
            iProgressValueCurrent = 0;
            iProgressValueCompleted = 0;

            Trace( "[SBAlgManager] PrepareForExecution() - END" );
        }

        private void PrepareExecutionQueue()
        {
            iExecutionQueue.Clear();
            
            // Find most appropriate algorithm...
            StackAlgorithm primary = FindPrimaryAlgorithm();
            if ( primary != null )
            {
                iExecutionQueue.Enqueue( primary );

                // Find backup algorithms
                EnqueueBackupAlgorithms( primary );
            }
            else
            {
                throw new Exception( "No valid stack algorithms available" );
            }
        }

        private StackAlgorithm FindPrimaryAlgorithm()
        {
            StackAlgorithm ret = null;
            //
            foreach ( StackAlgorithm alg in iMasterAlgorithmTable )
            {
                if ( alg.IsAvailable() )
                {
                    ret = alg;
                    break;
                }
            }
            //
            Trace( "[SBAlgManager] FindPrimaryAlgorithm() - ret: {0}", ret );
            return ret;
        }

        private void EnqueueBackupAlgorithms( StackAlgorithm aExclude )
        {
            foreach ( StackAlgorithm alg in iMasterAlgorithmTable )
            {
                string name = alg.Name;
                bool available = alg.IsAvailable();
                Trace( "[SBAlgManager] EnqueueBackupAlgorithms() - name: {0}, available: {1}", name, available );

                if ( available && name != aExclude.Name )
                {
                    iExecutionQueue.Enqueue( alg );
                }
            }
        }

        private void ExecuteHeadAlgorithm()
        {
            Trace( "[SBAlgManager] ExecuteHeadAlgorithm() - iSynchronicity: {0}", iSynchronicity );
            StackAlgorithm alg = CurrentAlgorithm;
            alg.BuildStack( iSynchronicity );
        }

        private void SortAlgorithms()
        {
            Comparison<StackAlgorithm> sorter = delegate( StackAlgorithm aLeft, StackAlgorithm aRight )
            {
                int ret = 1;
                //
                if ( aLeft == null )
                {
                    ret = -1;
                }
                else if ( aRight == null )
                {
                }
                else
                {
                    ret = aLeft.Priority.CompareTo( aRight.Priority );
                }
                //
                return ret;
            };
            iMasterAlgorithmTable.Sort( sorter );
        }

        private void ReportEvent( TEvent aEvent )
        {
            // Ensure we only report significant events once
            if ( EventHandler != null )
            {
                switch ( aEvent )
                {
                case TEvent.EAlgorithmStarted:
                    if ( ( iFlags & TFlags.EFlagsReportedStarted ) != TFlags.EFlagsReportedStarted )
                    {
                        iFlags |= TFlags.EFlagsReportedStarted;
                        EventHandler( this, aEvent );
                    }
                    break;
                case TEvent.EAlgorithmComplete:
                    if ( ( iFlags & TFlags.EFlagsReportedComplete ) != TFlags.EFlagsReportedComplete )
                    {
                        iFlags |= TFlags.EFlagsReportedComplete;
                        EventHandler( this, aEvent );
                    }
                    break;
                default:
                    EventHandler( this, aEvent );
                    break;
                }
            }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags
        {
            EFlagsNone = 0,
            EFlagsReportedStarted = 1,
            EFlagsReportedComplete = 2
        }
        #endregion

        #region Data members
        private readonly StackEngine iEngine;
        private DbgEngineView iView;
        private PluginManager<StackAlgorithm> iMasterAlgorithmTable = new PluginManager<StackAlgorithm>( 2 );
        
        // Transient variables
        private TSynchronicity iSynchronicity = TSynchronicity.EAsynchronous;
        private Queue<StackAlgorithm> iExecutionQueue = new Queue<StackAlgorithm>();
        private int iProgressValueMax = 0;
        private int iProgressValueCompleted = 0;
        private int iProgressValueCurrent = 0;
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
