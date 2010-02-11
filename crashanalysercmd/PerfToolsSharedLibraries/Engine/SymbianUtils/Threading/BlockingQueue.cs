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
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SymbianUtils.Threading
{
    public class BlockingQueue<T> : ICollection, IEnumerable 
    {
        #region Constructors
        public BlockingQueue()
            : this( -1 )
        {
        }

        public BlockingQueue( int aMaxSize )
        {
            iMaxSize = aMaxSize;
            iQueue = new Queue<T>();
        }
        #endregion

        #region API
        public void Enqueue( T aItem )
        {
            lock ( this.SyncRoot )
            {
                // We want to prevent the queue from growing beyond it's own
                // bounds, unless the creator requested an unbounded queue.
                if ( iMaxSize > 0 )
                {
                    while ( this.Count >= iMaxSize )
                    {
                        try
                        {
                            Monitor.Wait( this.SyncRoot );
                        }
                        catch
                        {
                            Monitor.PulseAll( this.SyncRoot );
                            throw;
                        }
                    }
                }

                // Now it's okay to add the item
                iQueue.Enqueue( aItem );

                // If the count is now one, then we've just added the first
                // item, in which case we must pulse the monitor because
                // there could be blocked threads that are stuck inside the Dequeue()
                // method, waiting for published content.
                int count = this.Count;
                if ( count == 1 )
                {
                    Monitor.PulseAll( this.SyncRoot );
                }
            }
        }

        public bool TryToDequeue( out T aItem )
        {
            bool ret = false;
            aItem = default( T );
            //
            lock ( this.SyncRoot )
            {
                if ( this.Count > 0 )
                {
                    aItem = iQueue.Dequeue();
                    ret = true;
                }
            }
            //
            return ret;
        }

        public void Clear()
        {
            lock ( this.SyncRoot )
            {
                iQueue.Clear();

                // Pulse, since clearing the items might allow a thread blocked
                // inside Enqueue() to push something to the head of the list
                Monitor.PulseAll( this.SyncRoot );
            }
        }

        public T Dequeue()
        {
            lock ( this.SyncRoot )
            {
                // Wait until the queue contains some content.
                while ( this.Count == 0 )
                {
                    try
                    {
                        Monitor.Wait( this.SyncRoot );
                    }
                    catch
                    {
                        Monitor.PulseAll( this.SyncRoot );
                        throw;
                    }
                }

                T ret = iQueue.Dequeue();

                // We dequeue the item and then check to see if we have
                // just opened up the first free slot in the queue.
                // If so, we must pulse the monitor because there could be
                // threads blocked inside the Enqueue() method that are waiting
                // for space to become available.
                int count = this.Count;
                if ( iMaxSize > 0 && count == iMaxSize - 1 )
                {
                    Monitor.PulseAll( this.SyncRoot );
                }
                //
                return ret;
            }
        }

        public T Peek()
        {
            lock ( this.SyncRoot )
            {
                return iQueue.Peek();
            }
        }

        public bool Contains( T aItem )
        {
            lock ( this.SyncRoot )
            {
                bool ret = iQueue.Contains( aItem );
                return ret;
            }
        }

        public T[] ToArray()
        {
            lock ( this.SyncRoot )
            {
                T[] ret = iQueue.ToArray();
                return ret;
            }
        }
        #endregion

        #region From IEnumerable
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException( "You cannot enumerate a Blocking Queue - get the values and enumerate those instead" );
        }
        #endregion

        #region From ICollection
        public void CopyTo( Array aArray, int aIndex )
        {
            lock ( this.SyncRoot )
            {
                ICollection baseCol = (ICollection) iQueue;
                baseCol.CopyTo( aArray, aIndex );
            }
        }

        public int Count
        {
            get
            {
                lock ( this.SyncRoot )
                {
                    return iQueue.Count;
                }
            }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get 
            {
                if ( this.iSyncRoot == null )
                {
                    Interlocked.CompareExchange( ref this.iSyncRoot, new object(), null );
                }
                return iSyncRoot; 
            }
        }
        #endregion

        #region Data members
        private readonly Queue<T> iQueue;
        private object iSyncRoot = null;
        private readonly int iMaxSize;
        #endregion
    }
}
