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
using System.Reflection;
using SymbianParserLib.Engine;
using CrashDebuggerLib.Structures.Fault;
using SymbianParserLib.Elements;
using CrashDebuggerLib.Parsers.State.Implementation;

namespace CrashDebuggerLib.Parsers.State
{
    internal static class StateFactory
    {
        #region API
        public static State Create( TState aState, CrashDebuggerParser aParser )
        {
            CreateDictionary();
            //
            State ret = null;
            if ( iDictionary.ContainsKey( aState ) )
            {
                ret = CreateState( aState, aParser );
                if ( ret != null )
                {
                    ret.Prepare();
                }
            }
            //
            return ret;
        }

        public static void RegisterCommands( ParserEngine aEngine )
        {
            CreateDictionary();
            //
            foreach ( KeyValuePair<TState, TStateMapplet> keyVP in iDictionary )
            {
                string[] commandIds = keyVP.Value.CommandIdentifiers;
                foreach ( string commandId in commandIds )
                {
                    // Create paragraph and associate the state type with the tag so that
                    // we know what type of object to create later on when the line fires.
                    ParserParagraph command = new ParserParagraph( commandId );
                    command.Tag = keyVP.Key;

                    // Create line to match
                    ParserLine line = ParserLine.New( commandId );

                    // Make sure that the paragraph and line don't disable themselves whenever
                    // they see a matching line. Some of these objects are needed more than once!
                    command.DisableWhenComplete = false;
                    line.DisableWhenComplete = false;
                    //
                    command.Add( line );
                    aEngine.Add( command );
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private static void CreateDictionary()
        {
            if ( iDictionary.Count == 0 )
            {
                // State table
                iDictionary.Add( TState.EStateInfoFault, 
                                 new TStateMapplet( "*** CMD: Fault_Info", typeof( StateInfoFault ) ) );
                iDictionary.Add( TState.EStateInfoCpu, 
                                 new TStateMapplet( "*** CMD: Cpu_Info", typeof( StateInfoCpu ) ) );
                iDictionary.Add( TState.EStateInfoDebugMask,
                                 new TStateMapplet( "*** CMD: Debug_Mask_Info", typeof( StateInfoDebugMask ) ) );
                iDictionary.Add( TState.EStateInfoScheduler, 
                                 new TStateMapplet( "*** CMD: Scheduler_Info", typeof( StateInfoScheduler ) ) );

                // For compatibility reasons catch either of these entries
                iDictionary.Add( TState.EStateInfoUserContextTable,
                                 new TStateMapplet( typeof( StateInfoUserContextTable ),
                                                    "*** CMD: UserContextTable_Info",
                                                    "*** CMD: UserContextTables" ) );

                // Other entries...
                iDictionary.Add( TState.EStateTheCurrentProcess,
                                 new TStateMapplet( "*** CMD: The_Current_Process", typeof( StateTheCurrentProcess ) ) );
                iDictionary.Add( TState.EStateTheCurrentThread,
                                 new TStateMapplet( "*** CMD: The_Current_Thread", typeof( StateTheCurrentThread ) ) );
                iDictionary.Add( TState.EStateContainerCodeSegs,
                                 new TStateMapplet( "*** CMD: container[CODESEG]", typeof( StateContainerCodeSegs ) ) );
                iDictionary.Add( TState.EStateContainerThread,
                                 new TStateMapplet( "*** CMD: container[THREAD]", typeof( StateContainerThreads ) ) );
                iDictionary.Add( TState.EStateContainerProcess,
                                 new TStateMapplet( "*** CMD: container[PROCESS]", typeof( StateContainerProcesses ) ) );
                iDictionary.Add( TState.EStateContainerChunk,
                                 new TStateMapplet( "*** CMD: container[CHUNK]", typeof( StateContainerChunks ) ) );
                iDictionary.Add( TState.EStateContainerLibrary,
                                 new TStateMapplet( "*** CMD: container[LIBRARY]", typeof( StateContainerLibraries ) ) );
                iDictionary.Add( TState.EStateContainerSemaphore,
                                 new TStateMapplet( "*** CMD: container[SEMAPHORE]", typeof( StateContainerSemaphores ) ) );
                iDictionary.Add( TState.EStateContainerMutex,
                                 new TStateMapplet( "*** CMD: container[MUTEX]", typeof( StateContainerMutexes ) ) );
                iDictionary.Add( TState.EStateContainerTimer,
                                 new TStateMapplet( "*** CMD: container[TIMER]", typeof( StateContainerTimers ) ) );
                iDictionary.Add( TState.EStateContainerServer,
                                 new TStateMapplet( "*** CMD: container[SERVER]", typeof( StateContainerServers ) ) );
                iDictionary.Add( TState.EStateContainerSession,
                                 new TStateMapplet( "*** CMD: container[SESSION]", typeof( StateContainerSessions ) ) );
                iDictionary.Add( TState.EStateContainerLogicalDevice,
                                 new TStateMapplet( "*** CMD: container[LOGICAL DEVICE]", typeof( StateContainerLogicalDevices ) ) );
                iDictionary.Add( TState.EStateContainerPhysicalDevice,
                                 new TStateMapplet( "*** CMD: container[PHYSICAL DEVICE]", typeof( StateContainerPhysicalDevices ) ) );
                iDictionary.Add( TState.EStateContainerLogicalChannel,
                                 new TStateMapplet( "*** CMD: container[LOGICAL CHANNEL]", typeof( StateContainerLogicalChannels ) ) );
                iDictionary.Add( TState.EStateContainerChangeNotifier,
                                 new TStateMapplet( "*** CMD: container[CHANGE NOTIFIER]", typeof( StateContainerChangeNotifiers ) ) );
                iDictionary.Add( TState.EStateContainerUndertaker,
                                 new TStateMapplet( "*** CMD: container[UNDERTAKER]", typeof( StateContainerUndertakers ) ) );
                iDictionary.Add( TState.EStateContainerMessageQueue,
                                 new TStateMapplet( "*** CMD: container[MESSAGE QUEUE]", typeof( StateContainerMessageQueues ) ) );
                iDictionary.Add( TState.EStateContainerPropertyRef,
                                 new TStateMapplet( "*** CMD: container[PROPERTY REF]", typeof( StateContainerPropertyRefs ) ) );
                iDictionary.Add( TState.EStateContainerConditionalVariable,
                                 new TStateMapplet( "*** CMD: container[CONDITION VARIABLE]", typeof( StateContainerConditionalVariables ) ) );
                iDictionary.Add( TState.EStateThreadStacks,
                                 new TStateMapplet( "*** CMD: Stack_Data[", typeof( StateThreadStacks ) ) );
            }
        }

        private static State CreateState( TState aState, CrashDebuggerParser aParser )
        {
            TStateMapplet maplet = iDictionary[ aState ];
            State ret = maplet.CreateInstance( aParser );
            return ret;
        }
        #endregion

        #region Data members
        private static Dictionary<TState, TStateMapplet> iDictionary = new Dictionary<TState, TStateMapplet>();
        #endregion
    }

    #region Internal structure
    internal class TStateMapplet
    {
        #region Constructors
        public TStateMapplet( Type aType, params string[] aCommandIdentifiers )
        {
            iCommandIdentifiers = aCommandIdentifiers;
            iType = aType;
        }

        public TStateMapplet( string aCommandIdentifier, Type aType )
        {
            iCommandIdentifiers = new string[] { aCommandIdentifier };
            iType = aType;
        }
        #endregion

        #region API
        public State CreateInstance( CrashDebuggerParser aParser )
        {
            State state = null;
            //
            Binder binder = null;
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance;
            object[] args = { aParser };
            object ret = iType.InvokeMember( string.Empty, bindingFlags, binder, null, args );
            if ( ret != null )
            {
                state = (State) ret;
            }
            //
            return state;
        }
        #endregion

        #region Properties
        public string[] CommandIdentifiers
        {
            get { return iCommandIdentifiers; }
        }
        #endregion

        #region Data members
        private readonly string[] iCommandIdentifiers;
        private readonly Type iType;
        #endregion
    }
    #endregion
}
