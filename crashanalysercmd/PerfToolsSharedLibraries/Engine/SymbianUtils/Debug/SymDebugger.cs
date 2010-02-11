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
using System.Diagnostics;

namespace SymbianUtils.SymDebug
{
    public static class SymDebugger
    {
        public static void Assert( bool aAssertionResult )
        {
            if ( !aAssertionResult )
            {
                System.Diagnostics.Debug.WriteLine( "ASSERTION FAILED" );
                System.Diagnostics.Debug.WriteLine( "================" );
                //
                string trace = System.Environment.StackTrace;
                System.Diagnostics.Debug.WriteLine( trace );
                //
                Break();
            } 
        }

        public static void Break()
        {
            if ( System.Diagnostics.Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }
    }
}
