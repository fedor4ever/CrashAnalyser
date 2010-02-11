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
using System.Text;
using System.Collections.Generic;
using SymbianUtils;
using SymbianDebugLib.Engine;
using SymbianStackLib.Engine;
using SymbianStackLib.Exceptions;
using CrashItemLib.Engine.Operations;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Engine;

namespace CrashItemLib.Crash.Stacks
{
	internal class CIStackBuilder
    {
        #region Constructors
        public CIStackBuilder( CIStack aStack, DbgEngine aDebugEngine )
        {
            iStack = aStack;
            iStackEngine = new StackEngine( aDebugEngine );
            iStackEngine.AddressInfo.Pointer = aStack.PointerValue;
            iStackEngine.AddressInfo.Range = aStack.Range;
            iStackEngine.Registers = aStack.Registers;
            iStackEngine.DataSource = aStack.RawSourceData;

            // Get the code segments for the process
            bool isThreadStack = aStack.IsThreadAvailable;
            if ( isThreadStack )
            {
                CIProcess process = OwningProcess;
                System.Diagnostics.Debug.Assert( process != null );

                // Seed stack engine with relevant code segments
                iStackEngine.CodeSegments = process.CodeSegments;
            }
        }
        #endregion

        #region API
        public void Build( TSynchronicity aSynchronicity )
        {
            StackEventsSubscribe();
            iStackEngine.Reconstruct( aSynchronicity );
        }
        #endregion

        #region Properties
        public bool IsReady
        {
            get { return iIsReady; }
            private set
            {
                lock ( this )
                {
                    iIsReady = value;
                }
            }
        }

        public CIProcess OwningProcess
        {
            get
            {
                CIProcess ret = iStack.OwningProcess;
                return ret;
            }
        }

        public StackEngine StackEngine
        {
            get { return iStackEngine; }
        }
        #endregion

        #region Event handlers
        private void StackEngine_EventHandler( StackEngine.TEvent aEvent, StackEngine aEngine )
        {
            if ( aEvent == StackEngine.TEvent.EStackBuildingComplete )
            {
                StackEventsUnsubscribe();
                IsReady = true;
            }
        }

        private void StackEngine_ExceptionHandler( Exception aException, StackEngine aEngine )
        {
            // Operation failed, but we must mark ourselves as ready or else UI clients will block forever...
            IsReady = true;

            // We'll deal with the public exceptions ourselves. Any kind of exception we cannot handle
            // will just get treated as a generic error.
            string msg = string.Empty;
            bool recognized = false;
            //
            if ( aException is StackAddressException )
            {
                recognized = true;
                StackAddressException exception = (StackAddressException) aException;
                switch ( exception.Type )
                {
                case StackAddressException.TType.ETypePointerIsNull:
                    msg = LibResources.CIStackBuilder_AddressInfoException_PointerMissing;
                    break;
                case StackAddressException.TType.ETypePointerOutOfBounds:
                    msg = LibResources.CIStackBuilder_AddressInfoException_PointerOutOfBounds;
                    break;
                case StackAddressException.TType.ETypeBaseAddressBeforeTopAddress:
                    msg = LibResources.CIStackBuilder_AddressInfoException_BaseAddressBeforeTopAddress;
                    break;
                case StackAddressException.TType.ETypeTopAddressAfterBaseAddress:
                    msg = LibResources.CIStackBuilder_AddressInfoException_TopAddressAfterBaseAddress;
                    break;
                default:
                    recognized = false;
                    break;
                }
            }

            // Not recognized? Unfortunately have to leak the original underlying .NET exception details
            if ( recognized == false )
            {
                msg = aException.Message;
            }

            if ( string.IsNullOrEmpty( msg ) == false )
            {
                // Treat exceptions as fatal errors
                CIMessageError error = new CIMessageError( iStack.Container, LibResources.CIStackBuilder_Error_Title );
                error.AddLine( msg );
                iStack.AddChild( error );
            }
        }

        private void StackEngine_MessageHandler( StackEngine.TMessageType aType, string aMessage, StackEngine aEngine )
        {
            switch ( aType )
            {
            default:
                break;
            case StackEngine.TMessageType.ETypeError:
                {
                CIMessageError error = new CIMessageError( iStack.Container, LibResources.CIStackBuilder_Error_Title );
                error.AddLine( aMessage );
                iStack.AddChild( error );
                break;
                }
            case StackEngine.TMessageType.ETypeWarning:
                {
                CIMessageWarning warning = new CIMessageWarning( iStack.Container, LibResources.CIStackBuilder_Warning_Title );
                warning.AddLine( aMessage );
                iStack.AddChild( warning );
                break;
                }
            }
        }
        #endregion

        #region Internal methods
        private void StackEventsSubscribe()
        {
            StackEngine.MessageHandler += new StackEngine.StackEngineMessageHandler( StackEngine_MessageHandler );
            StackEngine.EventHandler += new StackEngine.StackEngineEventHandler( StackEngine_EventHandler );
            StackEngine.ExceptionHandler += new StackEngine.StackEngineExceptionHandler( StackEngine_ExceptionHandler );
        }

        private void StackEventsUnsubscribe()
        {
            StackEngine.MessageHandler -= new StackEngine.StackEngineMessageHandler( StackEngine_MessageHandler );
            StackEngine.EventHandler -= new StackEngine.StackEngineEventHandler( StackEngine_EventHandler );
            StackEngine.ExceptionHandler -= new StackEngine.StackEngineExceptionHandler( StackEngine_ExceptionHandler );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "CIStackBuilder_{0}_{1}", iStack.Id, iStack.Name );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly CIStack iStack;
        private readonly StackEngine iStackEngine;
        private bool iIsReady = false;
        #endregion
    }
}
