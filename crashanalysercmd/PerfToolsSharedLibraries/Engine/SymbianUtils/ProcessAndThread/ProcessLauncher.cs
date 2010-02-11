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
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SymbianUtils.ProcessAndThread
{
    public class ProcessLauncher
    {
        #region API
        public static int Launch( string aFileName, string aCommandLineArguments, string aCurrentDirectory, bool aShowWindow )
        {
            return Launch( aFileName, aCommandLineArguments, aCurrentDirectory, aShowWindow, 0 );
        }

        public static int Launch( string aFileName, string aCommandLineArguments, string aCurrentDirectory, bool aShowWindow, int aWaitTimeInMs )
        {
            // See http://msdn.microsoft.com/en-us/library/ms682425.aspx
            // See http://msdn2.microsoft.com/en-us/library/ms682425.aspx
            // See http://www.pinvoke.net/default.aspx/kernel32/CreateProcess.html
            ProcessInformation pi = new ProcessInformation();
            StartupInformation si = new StartupInformation();
            if ( !aShowWindow )
            {
                si.dwFlags = STARTF_USESHOWWINDOW;
                si.wShowWindow = SW_SHOWMINIMIZED;
            }

            si.cb = Marshal.SizeOf( si );
            SecurityAttributes pSec = new SecurityAttributes();
            pSec.nLength = Marshal.SizeOf( pSec );
            SecurityAttributes tSec = new SecurityAttributes();
            tSec.nLength = Marshal.SizeOf( tSec );
            //
            int error = 0;
            bool suceeded = CreateProcess( aFileName, aCommandLineArguments, ref pSec, ref tSec, false, 0, IntPtr.Zero, aCurrentDirectory, ref si, out pi ) != false;
            if ( !suceeded )
            {
                error = Marshal.GetLastWin32Error();
            }
            else
            {
                if ( aWaitTimeInMs != 0 )
                {
                    // Wait
                    WaitForSingleObject( pi.hProcess, aWaitTimeInMs );
                }

                // Tidy up
                CloseHandle( pi.hThread );
                CloseHandle( pi.hProcess );

                int hResult = GetExitCodeProcess( pi.hProcess, ref error );
                if ( error != 0 )
                {
                    Win32Exception exception = new Win32Exception( error );
                    throw exception;
                }
            }
            //
            return error;
        }
        #endregion

        #region Internal constants
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        #endregion

        #region Internal PInvoke wrappers
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern int GetExitCodeProcess( IntPtr hProcess, ref int lpExitCode );

        [DllImport( "kernel32.dll", SetLastError = true )]
        internal static extern bool CreateProcess( string lpApplicationName,
                                          string lpCommandLine,
                                          ref SecurityAttributes lpProcessAttributes,
                                          ref SecurityAttributes lpThreadAttributes,
                                          bool bInheritHandles,
                                          uint dwCreationFlags,
                                          IntPtr lpEnvironment,
                                          string lpCurrentDirectory,
                                          ref StartupInformation lpStartupInfo,
                                          out ProcessInformation lpProcessInformation );

        [DllImport("kernel32", SetLastError=true, ExactSpelling=true)]
        internal static extern Int32 WaitForSingleObject( IntPtr handle, Int32 milliseconds );

        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool CloseHandle( IntPtr hObject );
        #endregion
   }

    public static class ProcessLauncherWithBlockingWait
    {
        #region API
        public static int Launch( string aFileName, string aCommandLineArguments, string aCurrentDirectory )
        {
            return Launch( aFileName, aCommandLineArguments, aCurrentDirectory, false );
        }

        public static int Launch( string aFileName, string aCommandLineArguments, string aCurrentDirectory, bool aShowWindow )
        {
            LaunchWorkerThread worker = new LaunchWorkerThread( aFileName, aCommandLineArguments, aCurrentDirectory );
            return worker.LaunchAndWait( aShowWindow );
        }
        #endregion
    }

    #region Internal class
    internal class LaunchWorkerThread
    {
        #region Constructors
        public LaunchWorkerThread( string aFileName, string aCommandLineArguments, string aCurrentDirectory )
        {
            iFileName = aFileName;
            iCommandLineArguments = aCommandLineArguments;
            iCurrentDirectory = aCurrentDirectory;
        }
        #endregion

        #region API
        public int LaunchAndWait( bool aShowWindow )
        {
            Thread thread = new Thread( new ParameterizedThreadStart( this.ThreadFunction ) );
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start( aShowWindow );
            //
            while ( thread.IsAlive )
            {
                Application.DoEvents();
                Thread.Sleep( 250 );
            }
            //
            return iError;
        }
        #endregion

        #region Internal methods
        private void ThreadFunction( object aData )
        {
            bool showWindow = (bool) aData;
            iError = ProcessLauncher.Launch( iFileName, iCommandLineArguments, iCurrentDirectory, showWindow, (int) WAIT_INFINITE );
        }
        #endregion

        #region Internal constants
        private static uint WAIT_INFINITE = 0xFFFFFFFF;
        #endregion

        #region Data members
        private int iError = -1;
        private readonly string iFileName;
        private readonly string iCommandLineArguments;
        private readonly string iCurrentDirectory;
        #endregion
    }
    #endregion
}
