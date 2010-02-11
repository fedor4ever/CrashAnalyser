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
using SymbianUtils.Tracer;

namespace SymbianUtils
{
	public abstract class AsyncTextReaderBase : AsyncReaderBase
	{
		#region Constructors
		protected AsyncTextReaderBase()
            : this( null )
		{
		}
        
        protected AsyncTextReaderBase( ITracer aTracer )
            : base( aTracer )
        {
        }
        #endregion

        #region Properties
        protected bool TrimLine
        {
            get { return iTrimLine; }
            set { iTrimLine = value; }
        }
        #endregion

        #region Read handlers
        protected virtual bool ContinueProcessing()
		{
			return false;
		}

		protected virtual bool ImmediateAbort()
		{
			return false;
		}

		protected virtual void HandleReadLine( string aLine )
		{
		}
		#endregion

		#region Framework API
		protected abstract string ProvideReadLine();

        protected virtual void OnProgressChanged( int aProgress )
        {
            NotifyEvent( TEvent.EReadingProgress );
        }
		#endregion

        #region Internal constants
        private const int KProgressCheckGranularity = 500;
        #endregion

        #region Internal methods
        protected override void PerformOperation()
		{
			bool forcedContinue = false;
			bool immediateAbort = false;
			string line;
			//
			lock( this )
			{
				line = ProvideReadLine();
				forcedContinue = ContinueProcessing();
				immediateAbort = ImmediateAbort();
			}

            System.DateTime tenPercentTime = DateTime.Now;

            int newProgress = 0;
            int oldProgress = 0;
            int progressTracker = 0;

			while ( ( line != null || forcedContinue ) && !immediateAbort )
			{
                if ( line != null && TrimLine )
				{
					line = line.Trim();
				}
				//
				lock( this )
				{
					HandleReadLine( line );
				}
				//
				lock( this )
				{
                    if ( progressTracker == KProgressCheckGranularity )
                    {
                        newProgress = Progress;
                        progressTracker = 0;
                    }

                    ++progressTracker;
				}
                if ( newProgress != oldProgress )
                {
#if DEBUG
                    if ( newProgress > 0 && ( newProgress % 10 ) == 0 )
                    {
                        System.DateTime now = DateTime.Now;
                        long intermediateTickDuration = ( ( now.Ticks - tenPercentTime.Ticks ) / 100 );
                        System.Diagnostics.Debug.WriteLine( newProgress.ToString( "d2" ) + " % COMPLETE - " + intermediateTickDuration.ToString( "d12" ) );
                        tenPercentTime = now;
                    }
#endif
                    OnProgressChanged( newProgress );
                    oldProgress = newProgress;
                }
                //
				lock( this )
				{
					line = ProvideReadLine();
                    forcedContinue = ContinueProcessing();
                    immediateAbort = ImmediateAbort();
                }
			}

            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - iOperationStartTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "TEXT READ COMPLETE - " + tickDuration.ToString( "d12" ) );
        }
		#endregion

        #region Data members
        private bool iTrimLine = true;
        #endregion
    }
}
