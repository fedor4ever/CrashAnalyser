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
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianUtils.FileSystem.FilePair;
using SymbianStructuresLib.CodeSegments;

namespace SymbianDebugLib.PluginAPI
{
    public abstract class DbgPluginView : DisposableObject
    {
        #region Constructors
        protected DbgPluginView( string aName, DbgPluginEngine aEngine )
		{
            iName = aName;
            iEngine = aEngine;
		}
		#endregion

        #region API - frameowork
        public abstract bool Contains( uint aAddress );

        public virtual void Activate( IEnumerable<CodeSegDefinition> aCodeSegments )
        {
            foreach ( CodeSegDefinition codeSeg in aCodeSegments )
            {
                Activate( codeSeg );
            }
        }

        public abstract bool Activate( CodeSegDefinition aCodeSegment );

        public virtual void Deactivate( IEnumerable<CodeSegDefinition> aCodeSegments )
        {
            foreach ( CodeSegDefinition cs in aCodeSegments )
            {
                Deactivate( cs );
            }
        }

        public abstract bool Deactivate( CodeSegDefinition aCodeSegment );

        public virtual object CustomOperation( string aName, object aParam1, object aParam2 )
        {
            throw new NotSupportedException();
        }

        public virtual bool SerializeTaggedCollections( FileNamePairCollection aFilesToSave )
        {
            return false;
        }

        public abstract bool IsReady
        {
            get;
        }
        #endregion

		#region Properties
        public string Name
        {
            get { return iName; }
        }

        protected DbgPluginEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal methods
        internal static int GetHashCode( string aName )
        {
            int ret = aName.GetHashCode();
            return ret;
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return iName.GetHashCode();
        }

        public override string ToString()
        {
            string ret = string.Format( "DbgPluginView: [{0}]", iName );
            return ret;
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
                iEngine.CloseView( this );
            }
        }
        #endregion

        #region Data members
        private readonly string iName;
        private readonly DbgPluginEngine iEngine;
        #endregion
    }
}
