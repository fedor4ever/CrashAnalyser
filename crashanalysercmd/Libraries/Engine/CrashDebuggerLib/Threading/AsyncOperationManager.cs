/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.ComponentModel;

namespace CrashDebuggerLib.Threading
{
    // <summary>
    // Serialises asynchronous requests - mainly because the symbol engine doesn't much
    // like having dynamically loaded codesegments unloaded underneath it - i.e. it doesn't
    // work multithreaded!
    // </summary>
    internal class AsyncOperationManager
    {
        #region Constructors
        public AsyncOperationManager()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( this )
            {
                iStarted = false;
                iQueue.Clear();
            }
        }

        public void Start()
        {
            lock ( this )
            {
                iStarted = true;
                StartNextOperation();
            }
        }

        public void Queue( AsyncOperation aOperation )
        {
            Queue( aOperation, false );
        }

        public void Queue( AsyncOperation aOperation, bool aHighPriority )
        {
            lock ( this )
            {
                if ( aHighPriority )
                {
                    iQueue.Insert( 0, aOperation );
                }
                else
                {
                    iQueue.Add( aOperation );
                }
            }

            //System.Diagnostics.Debug.WriteLine( "[AOP] - Add - Queue now contains " + iQueue.Count + " entries..." );
            StartNextOperation();
        }
        #endregion

        #region Event handlers
        void Operation_RunWorkerCompleted( object aSender, RunWorkerCompletedEventArgs aArgs )
        {
            lock ( this )
            {
                AsyncOperation op = (AsyncOperation) aSender;
                op.RunWorkerCompleted -= new RunWorkerCompletedEventHandler( Operation_RunWorkerCompleted );
                iPendingOperation = false;
            }
            //
            StartNextOperation();
            //System.Diagnostics.Debug.WriteLine( "[AOP] - Fin -Queue now contains " + iQueue.Count + " entries..." );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void StartNextOperation()
        {
            lock ( this )
            {
                if ( iStarted )
                {
                    if ( iQueue.Count > 0 && !iPendingOperation )
                    {
                        AsyncOperation op = iQueue[ 0 ];
                        iQueue.RemoveAt( 0 );
                        //
                        op.RunWorkerCompleted += new RunWorkerCompletedEventHandler( Operation_RunWorkerCompleted );
                        iPendingOperation = true;
                        op.RunWorkerAsync( op );
                        //
                        //System.Diagnostics.Debug.WriteLine( "[AOP] - Start - Starting op with " + iQueue.Count + " remaining..." );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( "[AOP] - Start - Is empty!" );
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( "[AOP] - Start - Queue is disabled!" );
                }
            }
        }
        #endregion

        #region Data members
        private List<AsyncOperation> iQueue = new List<AsyncOperation>();
        private bool iPendingOperation = false;
        private bool iStarted = false;
        #endregion
    }
}
