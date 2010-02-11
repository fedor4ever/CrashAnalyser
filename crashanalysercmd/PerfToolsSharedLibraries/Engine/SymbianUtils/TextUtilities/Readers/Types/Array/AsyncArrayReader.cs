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

namespace SymbianUtils.TextUtilities.Readers.Types.Array
{
    public interface AsyncArrayObjectSupplier<TType>
    {
        #region AsyncArrayObjectSupplier interface declaration
        int ObjectCount { get; }
        TType this[ int aIndex ] { get; }
        #endregion
    }

	public abstract class AsyncArrayReader<TType> : AsyncReaderBase
	{
		#region Constructors
        protected AsyncArrayReader( AsyncArrayObjectSupplier<TType> aObjectSupplier )
            : this( aObjectSupplier, null )
        {
        }

        protected AsyncArrayReader( AsyncArrayObjectSupplier<TType> aObjectSupplier, ITracer aTracer )
            : base( aTracer )
		{
            iObjectSupplier = aObjectSupplier;
		}
		#endregion

        #region Abstract reading framework
        protected abstract void HandleObject( TType aObject, int aIndex, int aCount );
		#endregion

        #region From AsyncReaderBase
        protected override long Size
        {
            get
            {
                long size = 0;
                //
                lock( this )
                {
                    size = iObjectSupplier.ObjectCount;
                }
                //
                return size;
            }
        }

        protected override long Position
        {
            get
            {
                long position = 0;
                //
                lock( this )
                {
                    position = iCurrentIndex;
                }
                //
                return position;
            }
        }
        #endregion

        #region Properties
        public int CurrentIndex
        {
            get
            {
                lock( this )
                {
                    return iCurrentIndex;
                }
            }
        }

        protected AsyncArrayObjectSupplier<TType> ObjectSupplier
        {
            get { return iObjectSupplier; }
        }
        #endregion

		#region Internal methods
		protected override void PerformOperation()
		{
            int count = 0;
            lock( this )
            {
                count = iObjectSupplier.ObjectCount;
                iCurrentIndex = -1;
            }

            System.DateTime tenPercentTime = DateTime.Now;

            int newProgress = 0;
            int oldProgress = 0;
            while( CurrentIndex < count - 1 )
            {
                lock( this )
                {
                    ++iCurrentIndex;
                }
                //
                lock( this )
                {
                    TType obj = iObjectSupplier[ iCurrentIndex ];
                    HandleObject( obj, iCurrentIndex-1, count );
                    newProgress = Progress;
                }
                //
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
            }

            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - iOperationStartTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "ARRAY READ COMPLETE - " + tickDuration.ToString( "d12" ) );
        }
		#endregion

        #region Data members
        private int iCurrentIndex = -1;
        private readonly AsyncArrayObjectSupplier<TType> iObjectSupplier;
        #endregion
	}
}
