/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
* All rights reserved.
* This component and the accompanying materials are made available
* under the terms of the License "Symbian Foundation License v1.0"
* which accompanies this distribution, and is available
* at the URL "http://www.symbianfoundation.org/legal/sfl-v10.html".
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
using System.Reflection;
using SymbianUtils.PluginManager;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Engine;

namespace CrashItemLib.PluginAPI
{
    public class CFFPluginRegistry : CFFDataProvider, IEnumerable<CFFPlugin>
	{
		#region Constructors
        public CFFPluginRegistry( CIEngine aEngine )
		{
            iEngine = aEngine;
            LoadPlugins();
		}
		#endregion

        #region API
        public string GetSupportedCrashFileTypes()
        {
            StringBuilder ret = new StringBuilder();

            // First get a list of all supported extensions
            List<CFFFileSpecification> initialList = new List<CFFFileSpecification>();
            foreach ( CFFPlugin plugin in this )
            {
                plugin.GetSupportedFileTypes( initialList );
            }

            // Identify duplicate entries and filter them out
            Dictionary<string, CFFFileSpecification> finalList = new Dictionary<string, CFFFileSpecification>();
            foreach ( CFFFileSpecification entry in initialList )
            {
                if ( !finalList.ContainsKey( entry.Description ) )
                {
                    finalList.Add( entry.Description, entry );
                }
            }

            // Convert each entry into dialog-like format list specification.
            foreach ( KeyValuePair<string, CFFFileSpecification> kvp in finalList )
            {
                CFFFileSpecification spec = kvp.Value;
                //
                ret.AppendFormat( "{0} ({1})|{2}", spec.Description, spec.Extensions, spec.Extensions );
                ret.Append( "|" );
            }

            // Remove last trailing pipe
            if ( ret.Length > 0 )
            {
                ret.Remove( ret.Length - 1, 1 );
            }

            string finalVal = ret.ToString();
            return finalVal;
        }

        public CFFSourceAndConfidence[] GetHandlers( FileInfo aFile, CFFFileList aOtherFiles )
        {
            List<CFFSourceAndConfidence> ret = new List<CFFSourceAndConfidence>();
            //
            foreach ( CFFPlugin plugin in iPlugins )
            {
                try
                {
                    CFFSourceAndConfidence conf = plugin.GetConfidence( aFile, aOtherFiles );
                    //
                    if ( conf.IsSupported )
                    {
                        ret.Add( conf );
                    }
                }
                catch ( Exception e )
                {
                    iEngine.Trace( "CFFPrimerRegistry.GetHandlers() - aFile: {0}, message: {1}, stack: {2}", aFile.FullName, e.Message, e.StackTrace );
                }
            }
            //
            return ret.ToArray();
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iPlugins.Count; }
        }

        public CFFPlugin this[ int aIndex ]
        {
            get { return iPlugins[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        private void LoadPlugins()
        {
            object[] parameters = new object[ 1 ];
            parameters[ 0 ] = this;
            //
            iPlugins.Load( parameters );
        }
        #endregion

        #region From CFFDataProvider
        public override CIEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region From IEnumerable<CFFPlugin>
        public IEnumerator<CFFPlugin> GetEnumerator()
        {
            return iPlugins.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return iPlugins.GetEnumerator();
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private PluginManager<CFFPlugin> iPlugins = new PluginManager<CFFPlugin>();
		#endregion
    }
}
