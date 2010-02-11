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
using CrashItemLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Container
{
	public class CIContainerCollection : IEnumerable<CIContainer>
	{
		#region Constructor & destructor
        public CIContainerCollection()
		{
		}
		#endregion

		#region API
        public void Add( CIContainer aContainer )
        {
            lock ( iEntries )
            {
                iEntries.Add( aContainer );
            }
        }

        public void RemoveAt( int aIndex )
        {
            lock ( iEntries )
            {
                iEntries.RemoveAt( aIndex );
            }
        }

        public bool Contains( CIContainer aContainer )
        {
            lock ( iEntries )
            {
                return iEntries.Contains( aContainer );
            }
        }

        public void Remove( CIContainer aContainer )
        {
            lock ( iEntries )
            {
                iEntries.Remove( aContainer );
            }
        }

        public void Clear()
        {
            lock ( iEntries )
            {
                iEntries.Clear();
            }
        }
		#endregion

        #region Properties
        public int Count
		{
            get
            {
                int ret = 0;
                //
                lock ( iEntries )
                {
                    ret = iEntries.Count;
                }
                //
                return ret;
            }
		}

        public CIContainer this[ int aIndex ]
		{
			get
			{
                CIContainer ret = null;
                //
                lock ( iEntries )
                {
                    ret = iEntries[ aIndex ];
                }
                //
                return ret;
			}
		}
		#endregion

        #region From IEnumerable
        public IEnumerator<CIContainer> GetEnumerator()
        {
            lock ( iEntries )
            {
                foreach ( CIContainer item in iEntries )
                {
                    yield return item;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock ( iEntries )
            {
                foreach ( CIContainer item in iEntries )
                {
                    yield return item;
                }
            }
        }
        #endregion

		#region Data members
        private List<CIContainer> iEntries = new List<CIContainer>( 3 );
		#endregion
    }
}
