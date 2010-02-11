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

namespace SymbianUtils.Mmp
{
    public class MmpFileReader : AsyncTextFileReader
    {
        #region Constructor & destructor
        public MmpFileReader( string aFileName )
            : base( aFileName )
        {
            iFileInfo = new MmpFileInfo( aFileName );
        }
        #endregion

        #region API
        public void Read()
        {
            base.SyncRead();
        }
        #endregion

        #region Properties
        public MmpFileInfo FileInfo
        {
            get { return iFileInfo; }
        }
        #endregion

        #region Internal methods
        private string[] ExtractElements( string aLine )
        {
            List<string> ret = new List<string>();
            //
            string line = aLine.Trim();
            line = line.Replace( '\t', ' ' );
            //
            string[] elements = line.Split( new char[] { ' ' } );
            foreach ( string element in elements )
            {
                if ( element.Length > 0 && element != " " )
                {
                    ret.Add( element );
                }
            }
            //
            return ret.ToArray();
        }
        #endregion

        #region From AsyncTextFileReader
        protected override void HandleFilteredLine( string aLine )
        {
            string[] elements = ExtractElements( aLine );
            //
            if ( elements.Length >= 2 )
            {
                string line = elements[ 0 ].ToUpper();
                //
                if ( line == "TARGET" )
                {
                    iFileInfo.Target = elements[ 1 ];

                    // Use extension as a means of guessing target type.
                    string extension = System.IO.Path.GetExtension( iFileInfo.Target ).ToUpper();
                    if ( extension == ".EXE" && iFileInfo.TargetType == MmpFileInfo.TTargetType.ETargetTypeUnsupported )
                    {
                        iFileInfo.TargetType = MmpFileInfo.TTargetType.ETargetTypeEXE;
                    }
                }
                else if ( line == "UID" )
                {
                    for ( int i = 1; i < elements.Length; i++ )
                    {
                        try
                        {
                            string uidString = elements[ i ];

                            uint val = 0;
                            if ( uint.TryParse( uidString, out val ) )
                            {
                                iFileInfo.Uids.Add( val );
                            }
                            else
                            {
                                // Try again, skipping any possible leading 0x prefix and using
                                // hex number formatting.
                                if ( uidString.StartsWith( "0x" ) )
                                {
                                    uidString = uidString.Substring( 2 );
                                    if ( uint.TryParse( uidString, System.Globalization.NumberStyles.HexNumber, null, out val ) )
                                    {
                                        iFileInfo.Uids.Add( val );
                                    }
                                }
                            }
                        }
                        catch ( Exception )
                        {
                        }
                    }
                }
                else if ( line == "TARGETTYPE" )
                {
                    // These are the only target types we need to support at the moment.
                    string targetType = elements[ 1 ].ToUpper();
                    //
                    if ( targetType == "EXE" )
                    {
                        iFileInfo.TargetType = MmpFileInfo.TTargetType.ETargetTypeEXE;
                    }
                    else if ( targetType == "EXEXP" )
                    {
                        iFileInfo.TargetType = MmpFileInfo.TTargetType.ETargetTypeEXE;
                    }
                    else if ( targetType == "DLL" )
                    {
                        iFileInfo.TargetType = MmpFileInfo.TTargetType.ETargetTypeDLL;
                    }
                }
            }
        }
        #endregion

        #region Data members
        private readonly MmpFileInfo iFileInfo;
        #endregion
    }
}
