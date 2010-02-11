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
using SymbianUtils;
using SymbianUtils.Settings;
using SymbianUtils.FileSystem;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity.Primer;
using SymbianDebugLib.Entity.Descriptors;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity
{
    public abstract class DbgEntity : DisposableObject
    {
        #region Enumerations
        public enum TEvent
        {
            EEventPrimingStarted = 0,
            EEventPrimingProgress,
            EEventPrimingComplete,
            EEventRemoved
        }

        public enum TConfigurationSuccess
        {
            ESuccessful = 0, 
            EFailed
        }
        #endregion

        #region Delegates & events
        public delegate void EventHandler( DbgEntity aEntity, string aCategory, TEvent aEvent, object aContext1, object aContext2 );
        public event EventHandler EventObserver;
        #endregion

        #region Constructors
        protected DbgEntity( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
        {
            iDescriptor = aDescriptor;
            iEntity = aFSEntity;
            iResult = new DbgEntityPrimerResult( this );
        }
        #endregion

        #region API
        internal void Prime( TSynchronicity aSynchronicity )
        {
            iIsPrimed = false;
            iDescriptor.Prime( this, aSynchronicity );
        }
        #endregion

        #region Framework API
        public virtual bool Exists
        {
            get
            {
                bool ret = false;
                //
                if ( iEntity != null )
                {
                    ret = iEntity.Exists;
                }
                //
                return ret;
            }
        }

        public virtual bool IsConfigurable
        {
            get { return false; }
        }

        public virtual bool IsReadyToPrime( out string aErrorList )
        {
            aErrorList = string.Empty;
            return true;
        }

        public virtual TConfigurationSuccess Configure()
        {
            return TConfigurationSuccess.ESuccessful;
        }

        public virtual object CustomOperation( string aName, object aParam1, object aParam2 )
        {
            throw new NotSupportedException();
        }

        public virtual void Save( XmlSettingCategory aCategory )
        {
        }

        public virtual void Load( XmlSettingCategory aCategory )
        {
        }

        public virtual string FullName
        {
            get { return FSEntity.FullName; }
        }

        public virtual void OnRemoved()
        {
        }

        public virtual DbgPluginEngine PluginEngine
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region Properties
        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        public bool IsPrimed
        {
            get { return iIsPrimed; }
         }

        public FSEntity FSEntity
        {
            get { return iEntity; }
        }

        public bool IsUnsupported
        {
            get
            {
                return ( this is SymbianDebugLib.Entity.BuiltIn.Unsupported.UnsupportedEntity );
            }
        }

        public bool WasAddedExplicitly
        {
            get { return iWasAddedExplicitly; }
            set { iWasAddedExplicitly = value; }
        }

        public string CategoryName
        {
            get { return iDescriptor.CategoryName; }
        }

        public DbgEntityDescriptor Descriptor
        {
            get { return iDescriptor; }
        }

        public DbgEntityPrimerResult PrimerResult
        {
            get { return iResult; }
            internal set { iResult = value; }
        }

        public TDbgUiMode UiMode
        {
            get { return Descriptor.UiMode; }
        }
        #endregion

        #region Priming event propgation
        internal void OnPrimeStart( IDbgEntityPrimer aPrimer )
        {
            // First tell engine
            Descriptor.Engine.OnPrimingStarted( this );
            try
            {
                // Next, tell observer
                if ( EventObserver != null )
                {
                    EventObserver( this, this.CategoryName, TEvent.EEventPrimingStarted, null, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnPrimeProgress( IDbgEntityPrimer aPrimer, int aValue )
        {
            // First tell engine
            Descriptor.Engine.OnPrimingProgress( this, aValue );
            try
            {
                if ( EventObserver != null )
                {
                    EventObserver( this, this.CategoryName, TEvent.EEventPrimingProgress, aValue, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnPrimeComplete( IDbgEntityPrimer aPrimer )
        {
            // We are now considered "primed"
            iIsPrimed = true;

            // Next tell engine
            Descriptor.Engine.OnPrimingComplete( this );
            try
            {
                // Next, tell observer
                if ( EventObserver != null )
                {
                    EventObserver( this, this.CategoryName, TEvent.EEventPrimingComplete, aPrimer.PrimeErrorMessage, aPrimer.PrimeException );
                }
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region Operators
        public static implicit operator FSEntity( DbgEntity aEntity )
        {
            FSEntity ret = null;
            //
            if ( aEntity.FSEntity != null )
            {
                ret = aEntity.FSEntity;
            }
            //
            return ret;
        }
        #endregion

        #region Internal properties
        protected DbgEngine Engine
        {
            get { return Descriptor.Engine; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return this.FSEntity.GetHashCode();
        }

        public override bool Equals( object aObject )
        {
            if ( aObject != null )
            {
                if ( aObject is DbgEntity )
                {
                    DbgEntity other = (DbgEntity) aObject;
                    //
                    return ( other.FSEntity == this.FSEntity );
                }
            }
            //
            return base.Equals( aObject );
        }

        public override string ToString()
        {
            return this.FSEntity.FullName;
        }
        #endregion

        #region Data members
        private readonly DbgEntityDescriptor iDescriptor;
        private DbgEntityPrimerResult iResult;
        private object iTag = null;
        private FSEntity iEntity = null;
        private bool iIsPrimed = false;
        private bool iWasAddedExplicitly = true;
        #endregion
    }
}
