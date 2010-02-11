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
	public abstract class AsyncReaderBase : DisposableObject, ITracer
	{
		#region Events
		public enum TEvent
		{
			EReadingStarted = 0,
			EReadingProgress,
			EReadingComplete
		}

		public delegate void Observer( TEvent aEvent, AsyncReaderBase aSender );
		public event Observer iObserver;

        public delegate void ExceptionHandler( Exception aException, AsyncReaderBase aSender );
        public event ExceptionHandler iExceptionHandler;
        #endregion

		#region Construct & destruct
        protected AsyncReaderBase()
            : this( null )
        {
        }

		protected AsyncReaderBase( ITracer aTracer )
		{
            iTracer = aTracer;

            // Make sure the thread has a unique name...
            iWorkerThread = new Thread( new System.Threading.ThreadStart( WorkerThreadFunction ) );
            iWorkerThread.Name = "WorkerThreadFunction_" + SymbianUtils.Strings.StringUtils.MakeRandomString();
			iWorkerThread.Priority = System.Threading.ThreadPriority.BelowNormal;
			iWorkerThread.IsBackground = true;
		}
		#endregion

		#region API
        protected virtual void StartRead( TSynchronicity aSynchronicity )
        {
            switch ( aSynchronicity )
            {
            default:
            case TSynchronicity.EAsynchronous:
                AsyncRead();
                break;
            case TSynchronicity.ESynchronous:
                SyncRead();
                break;
            }
        }

		protected virtual void AsyncRead()
		{
			lock( this )
			{
				iWorkerThread.Start();
			}
		}

        protected virtual void SyncRead()
		{
			WorkerThreadFunction();
		}
		#endregion

		#region From DisposableObject - Cleanup Framework
		protected override void CleanupManagedResources()
		{
            try
            {
            }
            finally
            {
                base.CleanupManagedResources();
            }
		}

		protected override void CleanupUnmanagedResources()
		{
            try
            {
            }
            finally
            {
                base.CleanupUnmanagedResources();
            }
		}
		#endregion

		#region Properties
		public bool IsReady
		{
			get
			{
				lock(this)
				{
					return iReady;
				}
			}
		}

		public int Progress
		{
			get
			{
				lock(this)
				{
					int progress = 0;
					//
					if	( Size == 0 )
					{
						progress = 0;
					}
					else
					{
						progress = CalculateProgress();
					}
					//
					return progress;
				}
			}
		}

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }
		#endregion

		#region Read handlers
		protected virtual void HandleReadStarted()
		{
		}

		protected virtual void HandleReadCompleted()
		{
		}

		protected virtual void HandleReadException( Exception aException )
		{
            if  ( iExceptionHandler != null )
            {
                iExceptionHandler( aException, this );
            }
		}
		#endregion

		#region Abstract reading framework
		protected abstract void PerformOperation();
		protected abstract long Size { get; }
		protected abstract long Position { get; }
		#endregion

		#region Framework methods
		protected virtual int CalculateProgress()
		{
			float positionAsFloat = (float)Position;
			float sizeAsFloat = (float)Size;
			int progress = (int)((positionAsFloat / sizeAsFloat) * 100.0);
			//
			return System.Math.Max(1, System.Math.Min(100, progress));
		}

		protected virtual void NotifyEvent( TEvent aEvent )
		{
            // Prevents reporting the same progress repeatedly...
            if ( aEvent == TEvent.EReadingProgress )
            {
                int progress = Progress;
                if ( progress == iLastProgress )
                {
                    return;
                }
                iLastProgress = progress;
            }

			if	( iObserver != null )
			{
				iObserver( aEvent, this );
			}
		}
		#endregion

		#region Internal methods
		private void WorkerThreadFunction()
		{
			try
			{
				lock( this )
				{
                    iLastProgress = -1;
					iReady = false;
					HandleReadStarted();
					NotifyEvent( TEvent.EReadingStarted );
				}

                // Record start time
                iOperationStartTime = DateTime.Now;

                PerformOperation();
			}
			catch( Exception exception )
			{
				lock( this )
				{
					HandleReadException( exception );
				}
			}
			finally
			{
				Dispose();
				//
				lock( this )
				{
					try
					{
						HandleReadCompleted();
					}
					catch( Exception exception )
					{
						HandleReadException( exception );
					}
					iReady = true;
					NotifyEvent( TEvent.EReadingComplete );
				}
			}
		}
		#endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            Trace( string.Format( aFormat, aParams ) );
        }
        #endregion

        #region Internal data members
        protected bool iReady = true;
        protected DateTime iOperationStartTime;
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        private readonly Thread iWorkerThread;
        private object iTag = null;
        private int iLastProgress = -1;
		#endregion
    }
}
