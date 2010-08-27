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
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.Entity;
using SymbianDebugLib.Entity.Manager;
using SymbianDebugLib.Entity.Descriptors;
using SymbianDebugLib.Entity.Configurations;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianDebugLib.PluginAPI.Types.Trace;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianDebugLib.PluginAPI.Types.KeyBindings;
using SymbianDebugLib.PluginAPI.Types.MetaDataConfig;
using SymbianDebugLib.ValidationRules;

namespace SymbianDebugLib.Engine
{
    public class DbgEngine : DisposableObject, IEnumerable<DbgEntity>, ITracer
    {
        #region Enumerations
        public enum TEvent
        {
            EPrimingStarted = 0,
            EPrimingComplete
        }
        #endregion

        #region Delegates & events
        public delegate void OperationHandler( DbgEngine aEngine, TEvent aEvent );
        public delegate void EventHandler( DbgEngine aEngine, DbgEntity aEntity, object aContext );
        //
        public event EventHandler EntityAdded;
        public event EventHandler EntityRemoved;
        public event EventHandler EntitiesCleared;
        public event EventHandler EntityPrimingStarted;
        public event EventHandler EntityPrimingProgress;
        public event EventHandler EntityPrimingComplete;
        public event OperationHandler EngineOperation;
        #endregion

        #region Constructors
        public DbgEngine()
            : this( null )
        {
        }

        public DbgEngine( ITracer aTracer )
        {
            iTracer = aTracer;
 
            // Initialise the settings object
            iSettings = new XmlSettings( KDbgEngineXmlSettingsFileName );
            iSettings.Restore();

            iEntityManager = new DbgEntityManager( this );
            iPluginManager = new DbgPluginManager( this );
            iConfigManager = new DbgEntityConfigManager( this );
            iValidationManager = new DbgValidationManager( this );
            iDescriptorManager = new DbgEntityDescriptorManager( this );
        }
        #endregion

        #region API - setup phase
        public void Clear()
        {
            iCurrentConfig = null;
            iEntityManager.Clear();
        }

        public DbgEntity Add( string aEntityFullName )
        {
            return Add( new FileInfo( aEntityFullName ) );
        }

        public DbgEntity Add( FileInfo aFile )
        {
            return iEntityManager.AddFile( aFile );
        }

        public void AddRange( IEnumerable<FileInfo> aEntities )
        {
            foreach ( FileInfo entity in aEntities )
            {
                Add( entity );
            }
        }

        public void AddRange( IEnumerable<string> aFiles )
        {
            foreach ( string file in aFiles )
            {
                Add( file );
            }
        }

        public bool Contains( string aEntityFullName )
        {
            FSEntity entity = FSEntity.New( aEntityFullName );
            return Contains( entity );
        }

        public bool Contains( FSEntity aFSEntity )
        {
            return iEntityManager.Contains( aFSEntity );
        }

        public void Remove( DbgEntity aEntity )
        {
            EntityManager.Remove( aEntity );
        }

        public void AddActiveRomId(uint aRomId)
        {
            if (! iActiveRomIds.Contains(aRomId))
                iActiveRomIds.Add(aRomId);
        }

        public void AddSymbolRomId(uint aRomId)
        {
            if (!iSymbolRomIds.Contains(aRomId))
                iSymbolRomIds.Add(aRomId);
        }

        // Returns true we are decoding files related to this romid
        public bool IsActiveRomId(uint aRomId)
        {
            if (iActiveRomIds.Count == 0)
                return true;
            else
                return iActiveRomIds.Contains(aRomId);
        }

        // Returns true if this romid needs symbol files
        public bool IsSymbolDataNeeded(uint aRomId)
        {
            if (iSymbolRomIds.Count == 0)
                return true;
            else
                return iSymbolRomIds.Contains(aRomId);
        }

        public void Prime(TSynchronicity aSynchronicity)
        {
            if ( EngineOperation != null )
            {
                EngineOperation( this, TEvent.EPrimingStarted );
            }

            // Reset the plugins
            Code.Clear();
            Symbols.Clear();

            // Categorise the prime list by plugin
            Dictionary<DbgPluginEngine, DbgEntityList> list = new Dictionary<DbgPluginEngine, DbgEntityList>();
            foreach ( DbgEntity entity in iEntityManager )
            {
                // Might be null.
                DbgPluginEngine plugin = entity.PluginEngine;
                if ( plugin != null )
                {
                    // Find correct list
                    DbgEntityList pluginEntityList = null;
                    if ( list.ContainsKey( plugin ) )
                    {
                        pluginEntityList = list[ plugin ];
                    }
                    else
                    {
                        pluginEntityList = new DbgEntityList( this );
                        list.Add( plugin, pluginEntityList );
                    }

                    // Now add the entry
                    pluginEntityList.Add( entity );
                }
            }

            // Finally, we can tell all the plugins about the files they are about to receive
            foreach ( KeyValuePair<DbgPluginEngine, DbgEntityList> kvp in list )
            {
                kvp.Key.PrepareToPrime( kvp.Value );
            }

            // Now prime the individual entities
            foreach ( DbgEntity entity in iEntityManager )
            {
                entity.Prime( aSynchronicity );
            }

            if ( EngineOperation != null )
            {
                EngineOperation( this, TEvent.EPrimingComplete );
            }
        }

        public bool IsReadyToPrime( out string aErrorList )
        {
            bool valid = iValidationManager.IsValid( DbgValidationRule.TOperation.EOperationPrime, out aErrorList );
            //
            if ( valid )
            {
                valid = EntityManager.IsReadyToPrime( out aErrorList );
            }
            //
            return valid;
        }
        #endregion

        #region API - child engine & views
        public DbgEngineCode Code
        {
            get { return iPluginManager.Code; }
        }

        public DbgEngineSymbol Symbols
        {
            get { return iPluginManager.Symbols; }
        }

        public DbgEngineTrace TraceDictionaries
        {
            get { return iPluginManager.TraceDictionaries; }
        }

        public DbgEngineKeyBindings KeyBindings
        {
            get { return iPluginManager.KeyBindings; }
        }

        public DbgEngineMetaDataConfig MetaDataConfig
        {
            get { return iPluginManager.MetaDataConfig; }
        }

        public DbgEngineView CreateView( string aName )
        {
            return CreateView( aName, new CodeSegDefinitionCollection() );
        }

        public DbgEngineView CreateView( string aName, CodeSegDefinitionCollection aCodeSegments )
        {
            return CreateView( aName, aCodeSegments, TDbgViewDeactivationType.EDoNothing );
        }

        public DbgEngineView CreateView( string aName, CodeSegDefinitionCollection aCodeSegments, TDbgViewDeactivationType aDeactivationType )
        {
            DbgEngineView ret = new DbgEngineView( this, aName, aCodeSegments, aDeactivationType );
            return ret;
        }
        #endregion

        #region API - settings
        public void XmlSettingsSave()
        {
            iSettings.Save( KDbgEngineXmlSettingsRootNodeName, iEntityManager );
            iSettings.Store();
        }

        public void XmlSettingsLoad()
        {
            iSettings.Load( KDbgEngineXmlSettingsRootNodeName, iEntityManager );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iEntityManager.Count; }
        }

        public bool Verbose
        {
            get { return iVerbose; }
            set { iVerbose = value; }
        }

        public DbgEntity this[ int aIndex ]
        {
            get { return iEntityManager[ aIndex ]; }
        }

        public DbgEntityManager EntityManager
        {
            get { return iEntityManager; }
        }

        public DbgEntityConfigManager ConfigManager
        {
            get { return iConfigManager; }
        }

        public DbgEntityDescriptorManager DescriptorManager
        {
            get { return iDescriptorManager; }
        }

        public bool IsUsingConfiguration
        {
            get { return iCurrentConfig != null; }
        }

        public DbgEntityConfig CurrentConfiguration
        {
            get { return iCurrentConfig; }
            internal set { iCurrentConfig = value; }
        }

        public TDbgUiMode UiMode
        {
            get { return iDescriptorManager.UiMode; }
            set
            {
                iDescriptorManager.UiMode = value;
            }
        }

        public FSExtensionList FileTypeExtensions
        {
            get { return DescriptorManager.Extensions; }
        }

        public XmlSettings Settings
        {
            get { return iSettings; }
        }

        public FSEntity[] FileSystemEntities
        {
            get { return EntityManager.FileSystemEntities; }
        }

        public IPlatformIdAllocator IdAllocator
        {
            get { return iIdAllocator; }
        }
        #endregion

        #region Internal constants
        private const string KDbgEngineXmlSettingsFileName = "DbgEngineSettings.xml";
        private const string KDbgEngineXmlSettingsRootNodeName = "DbgEngine";
        #endregion

        #region Internal event propagation methods
        internal void OnAdded( DbgEntity aEntity )
        {
            try
            {
                if ( EntityAdded != null )
                {
                    EntityAdded( this, aEntity, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnRemoved( DbgEntity aEntity )
        {
            try
            {
                if ( EntityRemoved != null )
                {
                    EntityRemoved( this, aEntity, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnPrimingStarted( DbgEntity aEntity )
        {
            try
            {
                if ( EntityPrimingStarted != null )
                {
                    EntityPrimingStarted( this, aEntity, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnPrimingProgress( DbgEntity aEntity, int aValue )
        {
            try
            {
                if ( EntityPrimingProgress != null )
                {
                    EntityPrimingProgress( this, aEntity, aValue );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnPrimingComplete( DbgEntity aEntity )
        {
            try
            {
                if ( EntityPrimingComplete != null )
                {
                    EntityPrimingComplete( this, aEntity, null );
                }
            }
            catch ( Exception )
            {
            }
        }

        internal void OnCleared()
        {
            try
            {
                if ( EntitiesCleared != null )
                {
                    EntitiesCleared( this, null, null );
                }
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region From IEnumerable<DbgEntity>
        public IEnumerator<DbgEntity> GetEnumerator()
        {
            foreach ( DbgEntity e in iEntityManager )
            {
                yield return e;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( DbgEntity e in iEntityManager )
            {
                yield return e;
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            Trace( string.Format( aFormat, aParams ) );
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
                iPluginManager.Dispose();
                iConfigManager.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        private readonly XmlSettings iSettings;
        private readonly DbgEntityManager iEntityManager;
        private readonly DbgPluginManager iPluginManager;
        private readonly DbgEntityConfigManager iConfigManager;
        private readonly DbgValidationManager iValidationManager;
        private readonly DbgEntityDescriptorManager iDescriptorManager;
        private PlatformIdAllocator iIdAllocator = new PlatformIdAllocator();
        private DbgEntityConfig iCurrentConfig = null;
        private List<uint> iActiveRomIds = new List<uint>();
        private List<uint> iSymbolRomIds = new List<uint>();
        private bool iVerbose;
        #endregion
    }
}
