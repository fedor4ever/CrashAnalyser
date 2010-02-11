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
using System.IO;
using System.ComponentModel;
using System.Threading;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.Common.Factory;

namespace SymbianImageLib.Common.Content
{
    public abstract class SIContent : DisposableObject, ITracer
    {
        #region Enumerations
        public enum TDecompressionEvent
        {
            EEventDecompressionStarting = 0,
            EEventDecompressionProgress,
            EEventDecompressionComplete
        }
        #endregion

        #region Delegates & events
        public delegate void DecompressionEventHandler( TDecompressionEvent aEvent, SIContent aFile, object aData );
        public event DecompressionEventHandler DecompressionEvent;
        #endregion

        #region Constructors
        protected SIContent( SIImage aImage )
        {
            iImage = aImage;
        }
        #endregion

        #region Framework API
        public abstract TSymbianCompressionType CompressionType
        {
            get;
        }

        public abstract string FileName
        {
            get;
        }

        public abstract uint FileSize
        {
            get;
        }

        public abstract uint ContentSize
        {
            get;
        }

        public virtual bool IsRelocationSupported
        {
            get { return true; }
        }

        public virtual TCheckedUid Uid
        {
            get { return new TCheckedUid(); }
        }

        public virtual byte[] GetAllData()
        {
            return new byte[ 0 ];
        }

        public virtual uint ProvideDataUInt32( uint aAddress )
        {
            uint ret = 0;
            //
            if ( IsContentPrepared == false )
            {
                // Cannot provide data if we've not prepared it yet
            }
            else if ( iDecompressionException == false )
            {
                WaitForAsyncOperationCompletion();
                uint resolvedAddress = FixupAddress( aAddress );
                ret = DoProvideDataUInt32( resolvedAddress );
            }
            //
            return ret;
        }

        public virtual ushort ProvideDataUInt16( uint aAddress )
        {
            ushort ret = 0;
            //
            if ( IsContentPrepared == false )
            {
                // Cannot provide data if we've not prepared it yet
            }
            else if ( iDecompressionException == false )
            {
                WaitForAsyncOperationCompletion();
                uint resolvedAddress = FixupAddress( aAddress );
                ret = DoProvideDataUInt16( resolvedAddress );
            }
            //
            return ret;
        }

        public virtual bool IsCode
        {
            get { return true; }
        }

        protected virtual void OnRelocationAddressChanged( uint aOld, uint aNew )
        {
        }

        protected virtual void DoDecompress()
        {
        }

        protected virtual uint DoProvideDataUInt32( uint aTranslatedAddress )
        {
            throw new NotImplementedException();
        }

        protected virtual ushort DoProvideDataUInt16( uint aTranslatedAddress )
        {
            throw new NotImplementedException();
        }

        protected abstract bool GetIsContentPrepared();
        #endregion

        #region API
        public void PrepareContent( TSynchronicity aSynchronicity )
        {
            if ( CompressionType == TSymbianCompressionType.ENone )
            {
                // No content preparation required, so just indicate completion
                // immediately.
                ReportDecompressionEvent( TDecompressionEvent.EEventDecompressionStarting );
                ReportDecompressionEvent( TDecompressionEvent.EEventDecompressionComplete );
            }
            else
            {
                switch ( aSynchronicity )
                {
                case TSynchronicity.ESynchronous:
                    RunDecompressor();
                    break;
                case TSynchronicity.EAsynchronous:
                    // Must take the lock to either create or destroy waiter
                    lock ( iWaiterSyncRoot )
                    {
                        if ( iWaiter == null )
                        {
                            iWaiter = new AutoResetEvent( false );
                            ThreadPool.QueueUserWorkItem( new WaitCallback( RunDecompressionInBackgroundThread ), null );
                            break;
                        }
                        else
                        {
                            // Wait is active, so we are presumably busy...
                            throw new Exception( "Content is already in preparation" );
                        }
                    }
                }
            }
        }
 
        public uint RelocationAddress
        {
            get { return iRelocationAddress; }
            set
            {
                if ( value != iRelocationAddress )
                {
                    uint old = iRelocationAddress;
                    iRelocationAddress = value;
                    OnRelocationAddressChanged( old, iRelocationAddress );
                }
            }
        }

        public virtual bool IsContentPrepared
        {
            get
            {
                bool ret = true;
                //
                if ( CompressionType != TSymbianCompressionType.ENone )
                {
                    ret = GetIsContentPrepared();
                }
                //
                return ret;
            }
        }
        #endregion

        #region Properties
        public SIImage Image
        {
            get { return iImage; }
        }
        
        public SIHeader ImageHeader
        {
            get { return iImage.Header; }
        }

        internal SIStream ImageStream
        {
            get 
            {
                return iImage.Stream; 
            }
            set { iImage.Stream = value; }
        }
        #endregion

        #region Event handlers
        private void RunDecompressionInBackgroundThread( object aNotUsed )
        {
            System.Diagnostics.Debug.Assert( iWaiter != null );

            // Call derived class to do decompression. Must not throw.
            RunDecompressor();

            // Must do this last so that anybody that is waiting for code to become
            // ready is resumed.
            //
            // Also, we might have been disposed of as a result of the "completion" callback
            // so in that case, we will have already set the auto-reset-event during the cleanup.
            if ( iWaiter != null )
            {
                iWaiter.Set();
            }
        }
        #endregion

        #region Internal methods
        protected void ReportDecompressionEvent( TDecompressionEvent aEvent )
        {
            ReportDecompressionEvent( aEvent, null );
        }

        protected void ReportDecompressionEvent( TDecompressionEvent aEvent, object aData )
        {
            if ( DecompressionEvent != null )
            {
                DecompressionEvent( aEvent, this, aData );
            }
        }

        private void RunDecompressor()
        {
            Trace( "[SIImageContent] RunDecompressor() - START - this: " + FileName );
            ReportDecompressionEvent( TDecompressionEvent.EEventDecompressionStarting );
            //
            try
            {
                DoDecompress();
            }
            catch ( Exception e )
            {
                Trace( "[SIImageContent] RunDecompressor() - Exception - this: {0}, message: {1}", this.FileName, e.Message );
                Trace( "[SIImageContent] RunDecompressor() - Exception - this: {0}, stack  : {1}", this.FileName, e.StackTrace );
                iDecompressionException = true;
            }

            // Doing this might cause the disposal of the object
            ReportDecompressionEvent( TDecompressionEvent.EEventDecompressionComplete );
            Trace( "[SIImageContent] RunDecompressor() - END - this: " + FileName );
        }

        private void WaitForAsyncOperationCompletion()
        {
            // If the worker doesn't exist then we've already completed the operation
            if ( iWaiter != null )
            {
                // Just wait on the auto-reset event. If it's already been signalled, then
                // we will continue immediately.
                iWaiter.WaitOne();

                // Must take the lock to either create or destroy waiter
                lock ( iWaiterSyncRoot )
                {
                    iWaiter.Close();
                    iWaiter = null;
                }
            }
        }

        private uint FixupAddress( uint aAddress )
        {
            uint ret = aAddress - iRelocationAddress;
            return ret;
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iImage.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iImage.Trace( aFormat, aParams );
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
                iImage.Remove( this );
                //
                if ( iWaiter != null )
                {
                    iWaiter.Set();
                    iWaiter.Close();
                    iWaiter = null;
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return FileName;
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
        #endregion

        #region Data members
        private readonly SIImage iImage;
        private uint iRelocationAddress = 0;
        private bool iDecompressionException = false;
        private AutoResetEvent iWaiter = null;
        private object iWaiterSyncRoot = new object();
        #endregion
    }
}
