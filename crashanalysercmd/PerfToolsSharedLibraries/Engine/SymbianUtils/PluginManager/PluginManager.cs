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
using System.Reflection;

namespace SymbianUtils.PluginManager
{
    public class PluginManager<T> : DisposableObject, IEnumerable<T>
    {
        #region Events
        public delegate void PluginLoadedHandler( T aPlugin );
        public event PluginLoadedHandler PluginLoaded;
        #endregion

        #region Constructors
        public PluginManager()
        {
        }

        public PluginManager( bool aDiagnostics )
        {
            iDiagnostics = aDiagnostics;
        }

        public PluginManager( int aMinimumNumberOfExpectedPlugins )
        {
            iMinimumNumberOfExpectedPlugins = aMinimumNumberOfExpectedPlugins;
        }

        public PluginManager( bool aDiagnostics, int aMinimumNumberOfExpectedPlugins )
            : this( aDiagnostics )
        {
            iMinimumNumberOfExpectedPlugins = aMinimumNumberOfExpectedPlugins;
        }
        #endregion

        #region API - loading
        public void Load( object[] aPluginConstructorParameters )
        {
            Trace( "Load() - invoked by: " + System.Environment.StackTrace );
            Assembly assembly = Assembly.GetCallingAssembly();
            string path = Path.GetDirectoryName( assembly.Location );
            FindPluginsWithinPath( path, KDefaultSearchSpecification, aPluginConstructorParameters, assembly );
        }

        public void LoadFromCallingAssembly()
        {
            Trace( "LoadFromCallingAssembly(1) - invoked by: " + System.Environment.StackTrace );
            Assembly assembly = Assembly.GetCallingAssembly();
            FindPluginsWithinAssembly( assembly, true, new object[] {} );
            Rationalise();
        }

        public void LoadFromCallingAssembly( object[] aPluginConstructorParameters )
        {
            Trace( "LoadFromCallingAssembly(2) - invoked by: " + System.Environment.StackTrace );
            Assembly assembly = Assembly.GetCallingAssembly();
            FindPluginsWithinAssembly( assembly, true, aPluginConstructorParameters );
            Rationalise();
        }

        public void LoadFromPath( string aPath, object[] aPluginConstructorParameters )
        {
            FindPluginsWithinPath( aPath, KDefaultSearchSpecification, aPluginConstructorParameters, null );
        }

        public void LoadFromPath( string aPath, string aMatchSpec, object[] aPluginConstructorParameters )
        {
            FindPluginsWithinPath( aPath, aMatchSpec, aPluginConstructorParameters, null );
        }
        #endregion

        #region API - misc
        public void Unload()
        {
            foreach ( T plugin in iPlugins )
            {
                IDisposable disposable = plugin as IDisposable;
                if ( disposable != null )
                {
                    disposable.Dispose();
                }
            }
            //
            iPlugins.Clear();
        }

        public void Sort( IComparer<T> aComparer )
        {
            iPlugins.Sort( aComparer );
        }

        public void Sort( Comparison<T> aComparer )
        {
            iPlugins.Sort( aComparer );
        }

        public bool Predicate( Predicate<T> aPredicate )
        {
            bool ret = iPlugins.Exists( aPredicate );
            return ret;
        }

        public void Rationalise()
        {
            List<T> plugins = iPlugins;
            iPlugins = new List<T>();
            //
            int count = plugins.Count;
            for ( int i = count - 1; i >= 0; i-- )
            {
                T pluginToCheck = plugins[ i ];
                plugins.RemoveAt( i );
                Trace( "Rationalise() - checking hierarchy for derivates of: " + pluginToCheck.GetType().ToString() );
                //
                bool isDerviedFromByOtherPlugin = IsClassDerivedFrom( plugins, pluginToCheck );
                Trace( "Rationalise() - is base class for other plugin: " + isDerviedFromByOtherPlugin );
                //
                if ( !isDerviedFromByOtherPlugin )
                {
                    iPlugins.Add( pluginToCheck );
                    Trace( "Rationalise() - found good plugin: " + pluginToCheck.GetType().ToString() );
                    OnPluginLoaded( pluginToCheck );
                }
            }

            // Check that we loaded the expected minimum amount
            count = iPlugins.Count;
            SymbianUtils.SymDebug.SymDebugger.Assert( iMinimumNumberOfExpectedPlugins < 0 || count >= iMinimumNumberOfExpectedPlugins );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iPlugins.Count; }
        }

        public T this[ int aIndex ]
        {
            get { return iPlugins[ aIndex ]; }
        }

        public Type PluginType
        {
            get { return typeof( T ); }
        }
        #endregion
        
        #region Internal constants
        private const string KDefaultSearchSpecification = "*.plugin.dll";
        #endregion

        #region Event propgation
        private void OnPluginLoaded( T aPlugin )
        {
            if ( PluginLoaded != null )
            {
                PluginLoaded( aPlugin );
            }
        }
        #endregion

        #region Internal methods
        private void Trace( string aMessage )
        {
            System.Diagnostics.Debug.WriteLineIf( iDiagnostics, "PluginLoader<" + typeof(T).Name + "> " + aMessage );
        }

        private void Trace( Exception aException, string aFunction )
        {
            Trace( string.Format( "PluginLoader.{0}() - exception: {1}", aFunction, aException.Message ) );
            Trace( string.Format( "PluginLoader.{0}() - stack:     {1}", aFunction, aException.StackTrace ) );
            //
            if ( aException is ReflectionTypeLoadException )
            {
                ReflectionTypeLoadException refEx = (ReflectionTypeLoadException) aException;
                foreach ( Exception l in refEx.LoaderExceptions )
                {
                    Trace( string.Format( "     loader exception: {0}", l.Message ) );
                    Trace( string.Format( "     loader stack:     {0}", l.StackTrace ) );
                }
            }
            else if ( aException is FileNotFoundException )
            {
                FileNotFoundException fnf = (FileNotFoundException) aException;
                Trace( string.Format( "     file name:  {0}", fnf.FileName ) );
                Trace( string.Format( "     fusion log: {0}", fnf.FusionLog ) );
            }
        }

        private static bool IsClassDerivedFrom( List<T> aPluginList, T aPlugin )
        {
            bool ret = false;
            Type checkType = aPlugin.GetType();
            //
            foreach ( T plugin in aPluginList )
            {
                Type type = plugin.GetType();
                if ( type.IsSubclassOf( checkType ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        private void CreateFromTypes( Type[] aTypes, object[] aPluginConstructorParameters )
        {
            Type pluginType = PluginType;
            Trace( "CreateFromTypes() - got " + aTypes.Length + " types. Searching for instances of: " + pluginType.Name );
            // 
            foreach ( Type type in aTypes )
            {
                if ( pluginType.IsAssignableFrom( type ) )
                {
                    Trace( "CreateFromTypes() - found type: " + type.Name );
                    if ( !type.IsAbstract )
                    {
                        Trace( "CreateFromTypes() - type \'" + type.Name + "\' is concrete implementation" );
                        CreateFromType( type, aPluginConstructorParameters );
                    }
                }
            }
        }

        private void CreateFromType( Type aType, object[] aPluginConstructorParameters )
        {
            try
            {
                Trace( "CreateFromType() - calling constructor: " + aType.Name );
                //
                object ret = CallConstructor( aType, aPluginConstructorParameters );
                if ( ret != null )
                {
                    T plugin = (T) ret;
                    Trace( "CreateFromType() - saving instance: " + plugin.GetType().ToString() );
                    iPlugins.Add( plugin );
                }
            }
            catch ( FileNotFoundException )
            {
            }
            catch ( Exception e )
            {
                Trace( e, "CreateFromType" );
            }
        }

        private object CallConstructor( Type aType, object[] aParameters )
        {
            object ret = null;
            //
            try
            {
                ret = Activator.CreateInstance( aType, aParameters );
            }
            catch ( Exception e )
            {
                Trace( e, "CallConstructor" );
            }
            //
            return ret;
        }

        private void LoadAssemblyAndCreatePlugins( string aFileName, object[] aPluginConstructorParameters, bool aExplicit )
        {
            try
            {
                Trace( "LoadAssemblyAndCreatePlugins() - Trying to load plugins from dll: " + aFileName );
                if ( SymbianUtils.Assemblies.AssemblyHelper.IsCLRAssembly( aFileName ) )
                {
                    Assembly pluginAssembly = Assembly.LoadFrom( aFileName );
                    if ( pluginAssembly != null )
                    {
                        FindPluginsWithinAssembly( pluginAssembly, aExplicit, aPluginConstructorParameters );
                    }
                }
            }
            catch ( BadImageFormatException )
            {
                // Not a managed dll - ignore error
            }
            catch ( Exception assemblyLoadException )
            {
                Trace( assemblyLoadException, "LoadAssemblyAndCreatePlugins" );
            }
        }

        private void FindPluginsWithinAssembly( Assembly aAssembly, bool aExplit, object[] aPluginConstructorParameters )
        {
            bool okayToGetTypes = aExplit;

#if BETTER_PERFORMANCE
            // If explit load requested, then don't check for plugin attribute. Otherwise, to avoid
            // enumerating all types we can check for our special "plugin" attribute
            object[] attributes = aAssembly.GetCustomAttributes( typeof( PluginAssemblyAttribute ), false) ;
            if ( attributes != null && attributes.Length > 0 ) 
            {
                okayToGetTypes = true;
            }
#else
            okayToGetTypes = true;
#endif

            // Now get types
            if ( okayToGetTypes )
            {
                Trace( "DoLoad() - getting types from assembly: " + aAssembly.Location );
                Type[] types = aAssembly.GetTypes();
                CreateFromTypes( types, aPluginConstructorParameters );
            }
        }

        private void FindPluginsWithinPath( string aPath, string aMatchSpec, object[] aPluginConstructorParameters, Assembly aAdditionalSearchAssembly )
        {
            Trace( string.Format( "FindPluginsWithinPath() - path: {0}, matchSpec: {1}, invoked by: {2}", aPath, aMatchSpec, System.Environment.StackTrace ) );
            Unload();
            
            // Find from path
            string[] dllNames = Directory.GetFiles( aPath, aMatchSpec );
            foreach ( string dll in dllNames )
            {
                string justFileName = Path.GetFileName( dll );
                LoadAssemblyAndCreatePlugins( dll, aPluginConstructorParameters, false );
            }

            // Check additional assembly
            if ( aAdditionalSearchAssembly != null )
            {
                FindPluginsWithinAssembly( aAdditionalSearchAssembly, true, aPluginConstructorParameters );
            }
   
            // Check hierarchy for derived plugins
            Rationalise();
        }
        #endregion

        #region From IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
        {
            foreach ( T p in iPlugins )
            {
                yield return p;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( T p in iPlugins )
            {
                yield return p;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "Found: {0} of type: {1}", iPlugins.Count, typeof(T).Name );
            if ( iMinimumNumberOfExpectedPlugins > 0 )
            {
                ret.AppendFormat( ", expected: {2}", iMinimumNumberOfExpectedPlugins );
            }
            return ret.ToString();
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
                foreach ( T obj in iPlugins )
                {
                    IDisposable disp = obj as IDisposable;
                    if ( disp != null )
                    {
                        disp.Dispose();
                    }
                }
                //
                iPlugins.Clear();
                iPlugins = null;
            }
        }
        #endregion

        #region Data members
        private bool iDiagnostics = false;
        private int iMinimumNumberOfExpectedPlugins = -1;
        private List<T> iPlugins = new List<T>();
        #endregion
    }
}
