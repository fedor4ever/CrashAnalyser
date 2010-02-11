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
	public abstract class AsyncBinaryReaderBase : AsyncReaderBase
	{
		#region Construct & destruct
		protected AsyncBinaryReaderBase()
            : this( null )
		{
		}
        
        protected AsyncBinaryReaderBase( ITracer aTracer )
            : base( aTracer )
        {
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

		protected virtual void HandleReadBytes( byte[] aData )
		{
		}
		#endregion

		#region Abstract reading framework
		protected abstract byte[] ProvideReadBytes();
		#endregion

		#region Internal methods
		protected override void PerformOperation()
		{
			bool forcedContinue = false;
			bool immediateAbort = false;
			byte[] bytes;
			//
			lock( this )
			{
				bytes = ProvideReadBytes();
				forcedContinue = ContinueProcessing();
				immediateAbort = ImmediateAbort();
			}

            System.DateTime tenPercentTime = DateTime.Now;

			int oldProgress = 0;
            int newProgress = 0;
			while ( ( bytes != null && bytes.Length > 0 ) || forcedContinue && !immediateAbort )
			{
				lock( this )
				{
					HandleReadBytes( bytes );
				}
				//
				lock( this )
				{
					newProgress = Progress;
					if	( newProgress != oldProgress )
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
                        NotifyEvent( TEvent.EReadingProgress );
                        oldProgress = newProgress;
					}
					bytes = ProvideReadBytes();
				}
				//
				lock( this )
				{
					forcedContinue = ContinueProcessing();
				}
			}

            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - iOperationStartTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "BINARY READ COMPLETE - " + tickDuration.ToString( "d12" ) );
        }

		#endregion
	}
}
