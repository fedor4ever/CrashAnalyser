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
using System.Text;
using System.IO;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Uids;

namespace CrashItemLib.Crash.Memory
{
    public class CIMemoryInfoCollection : CIElement, IEnumerable<CIMemoryInfo>
	{
		#region Constructors
        public CIMemoryInfoCollection( CIContainer aContainer )
            : base( aContainer, TAutoPopulateType.EAutoPopulateEnabled )
		{
		}
		#endregion

        #region API
        public override void AddChild( CIElement aChild )
        {
            if ( aChild.GetType() != typeof( CIMemoryInfo ) )
            {
                throw new ArgumentException( "Child must be a CIMemoryInfo" );
            }

            base.AddChild( aChild );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<CIMemoryInfo>
        public new IEnumerator<CIMemoryInfo> GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIMemoryInfo )
                {
                    CIMemoryInfo ret = (CIMemoryInfo) element;
                    yield return ret;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIMemoryInfo )
                {
                    CIMemoryInfo ret = (CIMemoryInfo) element;
                    yield return ret;
                }
            }
        }
        #endregion

        #region Data members
        private List<CIMemoryInfo> iEntries = new List<CIMemoryInfo>();
        #endregion
    }
}
