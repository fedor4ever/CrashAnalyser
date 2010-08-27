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
using System.Drawing;
using SymbianUtils;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity.Primer;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity.Configurations
{
    public class DbgEntityConfig : IEnumerable<DbgEntityConfig.CfgSet>
    {
        #region Constructors
        public DbgEntityConfig( DbgEntityConfigManager aManager )
        {
            iManager = aManager;
        }
        #endregion

        #region Classes
        public sealed class CfgFile
        {
            #region Constructors
            public CfgFile( string aFileName )
            {
                iFileNameAndPath = aFileName;
            }
            #endregion

            #region API
            public void AddId( string aId )
            {
                iId.Add( aId );
            }

            public bool Contains( DbgEntityConfigIdentifier aId )
            {
                bool ret = iId.Contains( aId );
                return ret;
            }
            #endregion

            #region Properties
            public string FileNameAndPath
            {
                get { return iFileNameAndPath; }
            }
            #endregion

            #region Data members
            private string iFileNameAndPath = string.Empty;
            private DbgEntityConfigIdentifier iId = new DbgEntityConfigIdentifier();
            #endregion
        }

        public sealed class CfgSet : IEnumerable<CfgFile>
        {
            #region Constructors
            public CfgSet( string aSetName )
            {
                iSetName = aSetName;
            }
            #endregion

            #region API
            public void Add( CfgFile aFile )
            {
                iFiles.Add( aFile );
            }

            public bool Contains( DbgEntityConfigIdentifier aId )
            {
                bool ret = false;
                //
                foreach ( CfgFile file in iFiles )
                {
                    if ( file.Contains( aId ) )
                    {
                        ret = true;
                        break;
                    }
                }
                //
                return ret;
            }
            #endregion

            #region Properties
            public string Name
            {
                get { return iSetName; }
            }

            public int Count
            {
                get { return iFiles.Count; }
            }

            public CfgFile this[ int aIndex ]
            {
                get { return iFiles[ aIndex ]; }
            }
            #endregion

            #region From IEnumerable<CfgFile>
            public IEnumerator<CfgFile> GetEnumerator()
            {
                foreach ( CfgFile f in iFiles )
                {
                    yield return f;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach ( CfgFile f in iFiles )
                {
                    yield return f;
                }
            }
            #endregion
            
            #region Data members
            private readonly string iSetName;
            private List<CfgFile> iFiles = new List<CfgFile>();
            #endregion
        }
        #endregion

        #region API
        public bool Contains( DbgEntityConfigIdentifier aId )
        {
            bool ret = false;
            //
            foreach ( KeyValuePair<string, CfgSet> kvp in iSets )
            {
                if ( kvp.Value.Contains( aId ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public void Add( CfgSet aSet )
        {
            CfgSet ret = null;
            if ( iSets.TryGetValue( aSet.Name, out ret ) )
            {
                throw new ArgumentException( "Specified set already registered" );
            }
            //
            iSets.Add( aSet.Name, aSet );
        }
        #endregion

        #region Properties

        public bool SymbolDataNeeded
        {
            get { return iSymbolDataNeeded; }
            set { iSymbolDataNeeded = value; }
        }

        public DbgEngine Engine
        {
            get { return Manager.Engine; }
        }

        protected DbgEntityConfigManager Manager
        {
            get { return iManager; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<CfgSet>
        public IEnumerator<CfgSet> GetEnumerator()
        {
            foreach ( KeyValuePair< string, CfgSet> kvp in iSets )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, CfgSet> kvp in iSets )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region Data members
        private readonly DbgEntityConfigManager iManager;
        private Dictionary<string, CfgSet> iSets = new Dictionary<string, CfgSet>();
        private bool iSymbolDataNeeded = true;
        #endregion
    }
}
