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
using System.Reflection;
using System.Collections.Generic;
using SymbianUtils;
using SymbianDebugLib.Engine;
using CrashItemLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Base.DataBinding;

namespace CrashItemLib.Crash.Base
{
	public abstract class CIElement : IEnumerable<CIElement>
    {
        #region Delegates & events
        public delegate void CIElementEventHandler( CIElement aElement );
        public delegate void CIMultiElementEventHandler( CIElement aSelf, CIElement aOther );

        public event CIMultiElementEventHandler ChildAdded = null;
        #endregion

        #region Enumerations
        internal enum TAutoPopulateType
        {
            EAutoPopulateDisabled = 0,
            EAutoPopulateEnabled
        }

        public enum TChildSearchType
        {
            EDirectChildrenOnly = 0,
            EEntireHierarchy
        }
        #endregion

        #region Constructors
        internal CIElement( int aId )
            : this( aId, false )
        {
            // NB: to be called only by the container itself since everything else
            // must specify a valid container object
            System.Diagnostics.Debug.Assert( this is CIContainer );
        }

        internal CIElement( CIContainer aContainer )
            : this( aContainer, TAutoPopulateType.EAutoPopulateDisabled )
		{
		}

        internal CIElement( CIContainer aContainer, CIElement aParent )
            : this( aContainer )
        {
            iParent = aParent;
        }

        internal CIElement( CIContainer aContainer, TAutoPopulateType aDataBindingAutoPopulate )
            : this( aContainer.GetNextElementId(), aDataBindingAutoPopulate == TAutoPopulateType.EAutoPopulateEnabled )
        {
            iContainer = aContainer;
        }

        internal CIElement( CIContainer aContainer, CIElement aParent, TAutoPopulateType aDataBindingAutoPopulate )
            : this( aContainer, aDataBindingAutoPopulate )
        {
            iParent = aParent;
        }
        
        private CIElement( long aId, bool aDataBindingAutoPopulate )
        {
            iId = new CIElementId( aId );
            iDataBindingModel = new CIDBModel( this, aDataBindingAutoPopulate );
        }
        #endregion

        #region API - Framework Properties
        public virtual string Name
        {
            get { return string.Empty; }
            set { }
        }

        public virtual CIElementId Id
        {
            get { return iId; }
            set 
            { 
                iId = value;
                IsIdExplicit = true;
            }
        }
        #endregion

        #region API - Children
        public virtual int Count
        {
            get 
            {
                int ret = 0;
                //
                lock ( iSyncLock )
                {
                    if ( iChildren != null )
                    {
                        ret = iChildren.Count;
                    }
                }
                //
                return ret;
            }
        }

        public bool HasChildren
        {
            get
            {
                bool ret = false;
                //
                lock ( iSyncLock )
                {
                    if ( iChildren != null )
                    {
                        ret = ( iChildren.Count > 0 );
                    }
                }
                //
                return ret;
            }
        }

        public virtual void AddChild( CIElement aChild )
        {
            if ( aChild != null )
            {
                // If we have been restricted to a specific 
                // type of child element, then check aChild against
                // it...
                Type t = aChild.GetType();
                ValidateChildType( t );

                lock ( iSyncLock )
                {
                    if ( iChildren == null )
                    {
                        iChildren = new CIElementList<CIElement>( Container );
                        iChildren.IsInContainer = this.IsInContainer;
                    }

                    if ( aChild.Parent == null )
                    {
                        aChild.Parent = this;
                    }

                    iChildren.Add( aChild );
                }

                OnElementAddedToSelf( aChild );
            }
        }

        public virtual void AddChildren( CIElement[] aChildren )
        {
            for ( int i = aChildren.Length - 1; i >= 0; i-- )
            {
                AddChild( aChildren[ i ] );
            }
        }

        public virtual void RemoveChild( CIElement aChild )
        {
            if ( aChild == null )
            {
                throw new ArgumentException( "Child is null" );
            }

            if ( HasChildren && iChildren.Count > 0 )
            {
                lock ( iSyncLock )
                {
                    iChildren.Remove( aChild );
                }
            }
        }

        public virtual void RemoveChildren( Type aType )
        {
            if ( HasChildren && iChildren.Count > 0 )
            {
                lock ( iSyncLock )
                {
                    int count = iChildren.Count;
                    for ( int i = count - 1; i >= 0; i-- )
                    {
                        CIElement child = iChildren[ i ];
                        if ( aType.IsAssignableFrom( aType ) )
                        {
                            iChildren.Remove( child );
                        }
                    }
                }
            }
        }

        public virtual void Clear()
        {
            lock ( iSyncLock )
            {
                if ( iChildren != null )
                {
                    iChildren.Clear();
                }
            }
        }

        public virtual bool Contains( CIElement aElement )
        {
            return Contains( aElement.Id );
        }

        public virtual bool Contains( CIElementId aId )
        {
            bool ret = false;
            //
            lock ( iSyncLock )
            {
                if ( iChildren != null )
                {
                    ret = iChildren.Contains( aId );
                }
            }
            //
            return ret;
        }

        public CIElement this[ CIElementId aId ]
        {
            get
            {
                CIElement ret = null;
                //
                lock ( iSyncLock )
                {
                    if ( Contains( aId ) )
                    {
                        System.Diagnostics.Debug.Assert( iChildren != null );
                        ret = iChildren[ aId ];
                    }
                }
                //
                return ret;
            }
        }

        public CIElement this[ int aIndex ]
        {
            get
            {
                CIElement ret = null;
                //
                lock ( iSyncLock )
                {
                    if ( iChildren != null )
                    {
                        ret = iChildren[ aIndex ];
                    }
                }
                //
                return ret;
            }
        }
        
        public CIElement ChildByType( Type aType )
        {
            CIElement ret = null;
            //
            lock ( iSyncLock )
            {
                foreach ( CIElement element in this )
                {
                    if ( aType.IsAssignableFrom( element.GetType() ) )
                    {
                        ret = element;
                        break;
                    }
                }
            }
            //
            return ret;
        }

        public CIElementList<T> ChildrenByType<T>() where T: CIElement
        {
            CIElementList<T> ret = ChildrenByType<T>( TChildSearchType.EDirectChildrenOnly );
            return ret;
        }

        public CIElementList<T> ChildrenByType<T>( Predicate<T> aPredicate ) where T : CIElement
        {
            CIElementList<T> ret = ChildrenByType<T>( TChildSearchType.EDirectChildrenOnly, aPredicate );
            return ret;
        }

        public CIElementList<T> ChildrenByType<T>( TChildSearchType aType ) where T : CIElement
        {
            return ChildrenByType<T>( aType, null );
        }

        public CIElementList<T> ChildrenByType<T>( TChildSearchType aType, Predicate<T> aPredicate ) where T : CIElement
        {
            CIElementList<T> ret = new CIElementList<T>( Container );
            GetChildrenByType<T>( ret, aType, aPredicate );
            return ret;
        }

        internal CIElementList<CIElement> Children
        {
            get
            {
                CIElementList<CIElement> ret = null;
                //
                lock ( iSyncLock )
                {
                    ret = iChildren;
                    //
                    if ( ret == null )
                    {
                        ret = new CIElementList<CIElement>( Container );
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region API - Data Binding framework
        public virtual void PrepareColumns()
        {
            DataBindingModel.ClearColumns();
            DataBindingModel.TryAutoPopulateColumns( this );
        }

        public virtual void PrepareRows()
        {
            DataBindingModel.ClearRows();
            DataBindingModel.TryAutoPopulateCells( this );
        }
        #endregion

        #region API - Parentage
        public bool HasParent
        {
            get
            {
                lock ( iSyncLock )
                {
                    return iParent != null;
                }
            }
        }

        public virtual CIElement Parent
        {
            get
            {
                lock ( iSyncLock )
                {
                    return iParent;
                }
            }
            internal set 
            {
                lock ( iSyncLock )
                {
                    iParent = value;
                }
            }
        }
        #endregion

        #region Properties
        public virtual bool IsInContainer
        {
            get
            {
                lock ( iSyncLock )
                {
                    return ( iFlags & TFlags.EFlagsIsInContainer ) == TFlags.EFlagsIsInContainer;
                }
            }
            internal set
            {
                // Don't allow it to change if we're locked down.
                if ( IsFinalized == false )
                {
                    bool oldValue = IsInContainer;
                    if ( oldValue != value )
                    {
                        // Set new flag value
                        lock ( iSyncLock )
                        {
                            if ( value )
                            {
                                iFlags |= TFlags.EFlagsIsInContainer;
                            }
                            else
                            {
                                iFlags &= ~TFlags.EFlagsIsInContainer;
                            }
                        }

                        // Fire internal call that reflects new state. This notifies the
                        // container about our registration status and in turn, the container
                        // can notify it's observers about our presence/remove within container.
                        OnIsInContainerChanged();

                        // Ensure children are also in/out of container
                        lock ( iSyncLock )
                        {
                            if ( iChildren != null )
                            {
                                iChildren.IsInContainer = value;
                            }
                        }
                    }
                }
            }
        }

        public bool IsIdExplicit
        {
            get
            {
                lock ( iSyncLock )
                {
                    return ( iFlags & TFlags.EFlagsIdWasExplicitlySet ) == TFlags.EFlagsIdWasExplicitlySet;
                }
            }
            private set 
            {
                lock ( iSyncLock )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIdWasExplicitlySet;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIdWasExplicitlySet;
                    }
                }
            }
        }

        public virtual CIEngine Engine
        {
            get { return Container.Engine; }
        }

        public CIContainer Container
        {
            get
            {
                lock ( iSyncLock )
                {
                    return iContainer;
                }
            }
            internal set 
            {
                lock ( iSyncLock )
                {
                    iContainer = value;
                }
            }
        }

        public CIDBModel DataBindingModel
        {
            get { return iDataBindingModel; }
        }

        protected bool IsToBeFinalizedLast
        {
            get
            {
                lock ( iSyncLock )
                {
                    return ( iFlags & TFlags.EFlagsIsToBeFinalizedLast ) == TFlags.EFlagsIsToBeFinalizedLast;
                }
            }
            set
            {
                lock ( iSyncLock )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIsToBeFinalizedLast;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIsToBeFinalizedLast;
                    }
                }
            }
        }

        internal bool IsFinalized
        {
            get
            {
                lock ( iSyncLock )
                {
                    return ( iFlags & TFlags.EFlagsIsFinalized ) == TFlags.EFlagsIsFinalized;
                }
            }
            set
            {
                lock ( iSyncLock )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIsFinalized;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIsFinalized;
                    }
                }
            }
        }
        #endregion

        #region Internal methods
        internal void Trace( string aMessage )
        {
            Container.Engine.Trace( aMessage );
        }

        internal void Trace( string aFormat, params object[] aParams )
        {
            Container.Engine.Trace( aFormat, aParams );
        }

        protected void AddSupportedChildType( Type aType )
        {
            lock ( iSyncLock )
            {
                if ( iSupportedChildTypes == null )
                {
                    iSupportedChildTypes = new List<Type>();
                }
                iSupportedChildTypes.Add( aType );
            }
        }

        private void ValidateChildType( Type aType )
        {
            lock ( iSyncLock )
            {
                if ( iSupportedChildTypes != null )
                {
                    StringBuilder typeNames = new StringBuilder();
                    //
                    foreach ( Type t in iSupportedChildTypes )
                    {
                        typeNames.Append( t.ToString() + ", " );
                        //
                        if ( aType == t || aType.IsSubclassOf( t ) )
                        {
                            return;
                        }
                    }
                    //
                    string names = typeNames.ToString();
                    if ( names.Length > 2 )
                    {
                        names = names.Substring( 0, names.Length - 2 );
                    }
                    //
                    throw new ArgumentException( "Child is not of type: [" + names + "]" );
                }
            }
        }

        internal virtual void GetChildrenByType<T>( CIElementList<T> aList, TChildSearchType aType, Predicate<T> aPredicate ) where T : CIElement
        {
            // Get all direct children, and if recusion enabled, then fetch the
            // entire tree.
            Type t = typeof( T );
            foreach ( CIElement element in Children )
            {
                if ( t.IsAssignableFrom( element.GetType() ) )
                {
                    // Get entry of correct type
                    T objectEntry = (T) ( (object) element );

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
                    element.GetChildrenByType<T>( aList, aType, aPredicate );
                }
            }
        }
        #endregion

        #region Internal flags
        [Flags]
        private enum TFlags : short
        {
            EFlagsNone = 0,
            EFlagsIsInContainer = 1,
            EFlagsIdWasExplicitlySet = 2,
            EFlagsIsReadOnly = 4,
            EFlagsIsToBeFinalizedLast = 8,
            EFlagsIsFinalized = 16,
        }
        #endregion

        #region Internal framework methods
        /// <summary>
        /// Called when the element is to finish its construction. At this point
        /// it is assumed that the crash container data structure is largely fully
        /// populated. This function call should typically perform any final 
        /// post-processing, such as looking up symbols etc.
        /// </summary>
        /// <param name="aParams"></param>
        internal virtual void OnFinalize( CIElementFinalizationParameters aParams )
        {
        }

        internal void DoFinalizeElements( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize, IEnumerable<CIElement> aElements )
        {
            foreach( CIElement element in aElements )
            {
                if ( element.IsToBeFinalizedLast )
                {
                    aCallBackLast.Enqueue( element );
                }
                //
                element.DoFinalize( aParams, aCallBackLast, aForceFinalize );
            }
        }

        internal virtual void DoFinalize( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize )
        {
            lock ( iSyncLock )
            {
                // Finalize children
                if ( iChildren != null )
                {
                    DoFinalizeElements( aParams, aCallBackLast, aForceFinalize, iChildren );
                }
            }

            // Finalize ourself
            if ( ( aForceFinalize || IsToBeFinalizedLast == false ) && IsFinalized == false )
            {
                try
                {
                    OnFinalize( aParams );
                }
                finally
                {
                    IsFinalized = true;
                }
            }
        }

        /// <summary>
        /// Called by CIElement.AddChild() whenever a new element is
        /// added as a child of this element.
        /// 
        /// Notify the container that a child entry was added
        /// so that it can notify any elements that listen to
        /// all additions to the container (direct or indirect).
        /// </summary>
        protected virtual void OnElementAddedToSelf( CIElement aElement )
        {
            // aElement inherits our container state
            bool inContainer = IsInContainer;

            // Doing this will fire an added/removed event in accordance
            // with what our state currently is and what it has just become.
            aElement.IsInContainer = inContainer;

            // Report event
            if ( ChildAdded != null )
            {
                ChildAdded( this, aElement );
            }
        }

        protected virtual void OnIsInContainerChanged()
        {
            // If we're now in the container, either directly or indirectly (we don't care)
            // then make sure we notify the container so that it can inform other elements
            // that may be listening.
            //
            // Because "IsInContainer" cascades changes to our children, they will also
            // notify the container themselves in due course.
            //
            // Obviously don't cascade when the container itself changes its state.
            if ( this != Container )
            {
                if ( IsInContainer )
                {
                    Container.OnContainerElementRegistered( this );
                }
                else
                {
                    Container.OnContainerElementUnregistered( this );
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region From IEnumerable<CIElement>
        public IEnumerator<CIElement> GetEnumerator()
        {
            lock ( iSyncLock )
            {
                CIElementList<CIElement> children = iChildren;
                if ( iChildren == null )
                {
                    children = new CIElementList<CIElement>( Container );
                }
                return children.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock ( iSyncLock )
            {
                CIElementList<CIElement> children = iChildren;
                if ( iChildren == null )
                {
                    children = new CIElementList<CIElement>( Container );
                }
                return children.GetEnumerator();
            }
        }
        #endregion

        #region Data members
        private readonly CIDBModel iDataBindingModel;
        private object iSyncLock = new object();
        private CIElement iParent = null;
        private List<Type> iSupportedChildTypes = null;
        private CIContainer iContainer;
        private CIElementList<CIElement> iChildren;
        private CIElementId iId = new CIElementId();
        private TFlags iFlags = TFlags.EFlagsNone;
		#endregion
    }
}
