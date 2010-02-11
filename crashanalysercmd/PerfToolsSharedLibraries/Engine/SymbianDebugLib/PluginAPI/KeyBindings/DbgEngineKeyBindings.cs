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
using System.Text.RegularExpressions;
using System.IO;
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.PluginAPI.Types.KeyBindings
{
    public abstract class DbgEngineKeyBindings : DbgPluginEngine
    {
        #region Factory function
        public static DbgEngineKeyBindings New( DbgEngine aEngine )
        {
            PluginManager<DbgEngineKeyBindings> loader = new PluginManager<DbgEngineKeyBindings>( 1 );
            loader.Load( new object[] { aEngine } );
            //
            DbgEngineKeyBindings ret = null;
            foreach ( DbgEngineKeyBindings engine in loader )
            {
                if ( engine is DbgEngineKeyBindingsStub && loader.Count > 1 )
                {
                    continue;
                }
                else
                {
                    ret = engine;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected DbgEngineKeyBindings( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region API
        public void LoadFromFile( string aFileName )
        {
            using ( StreamReader reader = new StreamReader( new FileStream( aFileName, FileMode.Open, FileAccess.Read ) ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    Match m = KRegEx.Match( line );
                    if ( m.Success )
                    {
                        int scanCode = int.Parse( m.Groups[ "ScanCode" ].Value, System.Globalization.NumberStyles.HexNumber );
                        string value = m.Groups[ "Interpretation" ].Value;
                        //
                        if ( iTable.ContainsKey( scanCode ) == false )
                        {
                            iTable.Add( scanCode, value );
                        }
                    }
                    line = reader.ReadLine();
                }
            }
        }
        #endregion

        #region From DbgPluginEngine
        protected override void DoClear()
        {
            iTable.Clear();
        }
        #endregion

        #region Properties
        public bool IsKeyBindingTableAvailable
        {
            get { return iTable.Count > 0; }
        }

        public string this[ int aScanCode ]
        {
            get
            {
                string ret = KUnknownKey;
                //
                if ( iTable.TryGetValue( aScanCode, out ret ) == false )
                {
                    ret = KUnknownKey;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KUnknownKey = "Unknown Key";
        private static readonly Regex KRegEx = new Regex(
              "0x(?<ScanCode>[0-9a-fA-F]{4})\\=(?<Interpretation>.+)\r\n",
            RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
        private Dictionary<int, string> iTable = new Dictionary<int, string>();
        #endregion
    }
}
