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
using System.Collections.Generic;
using SymbianUtils.Tracer;

namespace SymbianUtils.TextUtilities.Readers.Types.Array
{
	public abstract class AsyncEnumerableReader<TType> : AsyncReaderBase
	{
		#region Constructors
        protected AsyncEnumerableReader( ITracer aTracer )
            : base( aTracer )
        {
        }

        protected AsyncEnumerableReader( int aCount, IEnumerator<TType> aEnumerator )
            : this( aCount, aEnumerator, null )
        {
        }

        protected AsyncEnumerableReader( int aCount, IEnumerator<TType> aEnumerator, ITracer aTracer )
            : base( aTracer )
		{
            Setup( aCount, aEnumerator );
		}
		#endregion

        #region API
        protected void Setup( long aCount, IEnumerator<TType> aEnumerator )
        {
            iCount = aCount;
            iEnumerator = aEnumerator;
        }
        #endregion

        #region Abstract reading framework
        protected abstract void HandleObject( TType aObject, long aIndex, long aCount );
		#endregion

        #region From AsyncReaderBase
        protected override long Size
        {
            get { return iCount; }
        }

        protected override long Position
        {
            get { return iCurrentIndex; }
        }
        #endregion

        #region Properties
        #endregion

		#region Internal methods
		protected override void PerformOperation()
		{
#if DEBUG
            System.DateTime tenPercentTime = DateTime.Now;
#endif
            iCurrentIndex = -1;
            int newProgress = 0;
            int oldProgress = 0;
            //
            bool hasEntry = iEnumerator.MoveNext();
            while ( hasEntry )
            {
                ++iCurrentIndex;
                TType entry = iEnumerator.Current;
                HandleObject( entry, iCurrentIndex, iCount );
                newProgress = Progress;
                //
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
                    NotifyEvent( TEvent.EReadingProgress );
                    oldProgress = newProgress;
                }

                // Move to next item
                hasEntry = iEnumerator.MoveNext();
            }

#if DEBUG
            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - iOperationStartTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "ARRAY READ COMPLETE - " + tickDuration.ToString( "d12" ) );
#endif
        }
		#endregion

        #region Data members
        private long iCount = 0;
        private IEnumerator<TType> iEnumerator = null;
        private long iCurrentIndex = -1;
        #endregion
	}
}
