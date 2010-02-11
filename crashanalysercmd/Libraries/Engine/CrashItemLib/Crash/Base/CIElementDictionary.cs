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
using System.ComponentModel;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Uids;

namespace CrashItemLib.Crash.Base
{
    public class CIElementDictionary<T> : CIElement, IEnumerable<T> where T: CIElement
	{
		#region Constructors
        public CIElementDictionary( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        [Browsable( false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void AddChild( CIElement aChild )
        {
            throw new NotSupportedException( "Use Add() instead" );
        }

        [Browsable(false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void AddChildren( CIElement[] aChildren )
        {
            throw new NotSupportedException( "Use Add() instead" );
        }

        [Browsable(false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void RemoveChild( CIElement aChild )
        {
            throw new NotSupportedException( "Use Remove() instead" );
        }

        public override void Clear()
        {
            base.Clear();
            iDictionary.Clear();
        }

        public virtual bool Contains( T aEntry )
        {
            CIElement element = CheckValid( aEntry );
            return iDictionary.ContainsKey( element.Id );
        }

        public override bool Contains( CIElement aElement )
        {
            return this.Contains( aElement.Id );
        }

        public override bool Contains( CIElementId aId )
        {
            return iDictionary.ContainsKey( aId );
        }

        public virtual bool Add( T aEntry )
        {
            CIElement element = CheckValid( aEntry );

            bool needsAdd = iDictionary.ContainsKey( element.Id ) == false;
            if ( needsAdd )
            {
                iDictionary.Add( element.Id, aEntry );

                // Treat as though it was added as a child
                base.OnElementAddedToSelf( element );
            }

            return needsAdd;
        }

        public virtual void AddRange( IEnumerable<T> aEnumerable )
        {
            foreach ( T t in aEnumerable )
            {
                Add( t );
            }
        }

        public virtual void AddRange( T[] aArray )
        {
            AddRange( (IEnumerable<T>) aArray );
        }

        public virtual void Remove( T aEntry )
        {
            CIElement element = CheckValid( aEntry );
            if ( iDictionary.ContainsKey( element.Id ) )
            {
                iDictionary.Remove( element.Id );
            }
        }

        public override bool IsInContainer
        {
            get { return base.IsInContainer; }
            internal set
            {
                base.IsInContainer = value;
                //
                foreach ( KeyValuePair<CIElementId, T> kvp in iDictionary )
                {
                    CIElement element = kvp.Value as CIElement;
                    if ( element != null )
                    {
                        element.IsInContainer = value;
                    }
                }
            }
        }
        #endregion

        #region Properties
        public new int Count
        {
            get { return iDictionary.Count; }
        }

        public new T this[ CIElementId aId ]
        {
            get
            {
                T ret = default(T);
                //
                if ( iDictionary.ContainsKey( aId ) )
                {
                    ret = iDictionary[ aId ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region From CIElement
        internal override void DoFinalize( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize )
        {
            DoFinalizeElements( aParams, aCallBackLast, aForceFinalize, this );
            base.DoFinalize( aParams, aCallBackLast, aForceFinalize );
        }

        internal override void GetChildrenByType<ChildType>( CIElementList<ChildType> aList, TChildSearchType aType, Predicate<ChildType> aPredicate )
        {
 	        // Get all direct children, and if recusion enabled, then fetch the
            // entire tree.
            Type t = typeof( ChildType );
            foreach( T entry in this )
            {
                CIElement element = entry;
                //
                if ( t.IsAssignableFrom( element.GetType() ) )
                {
                    // Get entry of correct type
                    ChildType objectEntry = (ChildType) ( (object) element );

                    // Check whether it is suitable for inclusion via our predicate
                    bool addEntry = true;
                    if ( aPredicate != null )
                    {
                        addEntry = aPredicate( objectEntry );
                    }

                    // Is it okay to take the entry?
                    if ( addEntry )
                    {
                        aList.Add( objectEntry );
                    }
                }

                // Get the element's children
                if ( aType == TChildSearchType.EEntireHierarchy )
                {
                    element.GetChildrenByType<ChildType>( aList, aType, aPredicate );
                }
            }
        } 
        #endregion

        #region Internal methods
        protected virtual CIElement CheckValid( T aEntry )
        {
            CIElement element = aEntry as CIElement;
            if ( element == null )
            {
                throw new NotSupportedException( "CIElementDictionary can only contain CIElement derived objects" );
            }
            return element;
        }
        #endregion

        #region From IEnumerable<T>
        public new IEnumerator<T> GetEnumerator()
        {
            foreach ( KeyValuePair<CIElementId, T > kvp in iDictionary )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<CIElementId, T> kvp in iDictionary )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region Data members
        private SortedDictionary<CIElementId, T> iDictionary = new SortedDictionary<CIElementId, T>();
        #endregion
    }
}
