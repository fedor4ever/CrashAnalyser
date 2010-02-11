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
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;

namespace SymbianUtils.Environment
{
    public class SymbianCommandLineEnvValidator
    {
        #region API
        public static bool CheckEnvVarsValidForSymbianOSCompilation( bool aShowWarningDialogs )
        {
            bool valid = false;
            //
            ProcessStartInfo info = new ProcessStartInfo( "calc.exe" );
            info.UseShellExecute = false;

            // Check env vars and make sure standard epoc32 paths are included 
            StringDictionary envVars = info.EnvironmentVariables;

            string path = envVars[ "PATH" ];
            string epocRoot = envVars[ "EPOCROOT" ];

            if ( path.IndexOf( @"\epoc32\tools" ) < 0 )
            {
                if ( aShowWarningDialogs )
                {
                    System.Windows.Forms.MessageBox.Show( @"Your path doesn't appear to support the necessary" + System.Environment.NewLine +
                                                          @"\Epoc32\... environment variables.",
                                                          "EPOC Environment Variables Undefined" );
                }
            }
            else if ( epocRoot == string.Empty )
            {
                if ( aShowWarningDialogs )
                {
                    System.Windows.Forms.MessageBox.Show( @"You need to define EPOCROOT, e.g. to the value '\'",
                                                          "EPOCROOT Undefined" );
                }
            }
            else if ( epocRoot != @"\" )
            {
                if ( aShowWarningDialogs )
                {
                    System.Windows.Forms.MessageBox.Show( @"The Active Object toolkit requires EPOCROOT be set to '\'",
                                                          "EPOCROOT Value Not Supported" );
                }
            }
            else
            {
                valid = true;
            }
            //
            return valid;
        }
        #endregion
    }
}
