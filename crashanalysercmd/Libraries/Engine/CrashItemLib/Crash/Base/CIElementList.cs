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
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Uids;

namespace CrashItemLib.Crash.Base
{
    public class CIElementList<T> : CIElementDictionary<T>, IEnumerable<T> where T : CIElement
    {
        #region Constructors
        public CIElementList( CIContainer aContainer )
            : base( aContainer )
        {
        }
        #endregion

        #region API
        public void Sort( Comparison<T> aComparisonFunction )
        {
            iList.Sort( aComparisonFunction );
        }

        public override void Clear()
        {
            base.Clear();
            iList.Clear();
        }

        /// <summary>
        /// Add the specified element to this list
        /// </summary>
        /// <param name="aEntry"></param>
        /// <returns>true if the item was added to the list, false if the item was not added (for example, it already exists)</returns>
        public override bool Add( T aEntry )
        {
            bool added = base.Add( aEntry );
            if ( added )
            {
                iList.Add( aEntry );
            }
            return added;
        }

        public override void Remove( T aEntry )
        {
            base.Remove( aEntry );
            iList.Remove( aEntry );
        }

        public void RemoveAt( int aIndex )
        {
            iList.RemoveAt( aIndex );
        }

        public virtual T[] ToArray()
        {
            return iList.ToArray();
        }
        #endregion

        #region Properties
        public new T this[ int aIndex ]
        {
            get { return iList[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<T>
        public new IEnumerator<T> GetEnumerator()
        {
            foreach ( T element in iList )
            {
                yield return element;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( T element in iList )
            {
                yield return element;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "[{0}] {1} item{2}", typeof( T ).Name, iList.Count, iList.Count == 0 || iList.Count > 1 ? "s" : string.Empty );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private List<T> iList = new List<T>();
        #endregion
    }
}
