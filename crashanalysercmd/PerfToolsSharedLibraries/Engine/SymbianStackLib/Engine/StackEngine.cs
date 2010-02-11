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
using SymbianDebugLib.Engine;
using SymbianStackLib.AddressInfo;
using SymbianStackLib.Algorithms;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Data.Source.Primer;
using SymbianStackLib.Prefixes;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.CodeSegments;
using SymbianUtils;
using SymbianUtils.Tracer;

namespace SymbianStackLib.Engine
{
	public sealed class StackEngine : ITracer
    {
        #region Delegates & events
        public enum TEvent
        {
            EStackBuildingStarted = 0,
            EStackBuildingProgress,
            EStackBuildingComplete
        }

        public enum TMessageType
        {
            ETypeWarning = 0,
            ETypeError
        }

        public delegate void StackEngineEventHandler( TEvent aEvent, StackEngine aEngine );
        public event StackEngineEventHandler EventHandler;

        public delegate void StackEngineExceptionHandler( Exception aException, StackEngine aEngine );
        public event StackEngineExceptionHandler ExceptionHandler;

        public delegate void StackEngineMessageHandler( TMessageType aType, string aMessage, StackEngine aEngine );
        public event StackEngineMessageHandler MessageHandler;
        #endregion

        #region Constructors
        public StackEngine( DbgEngine aDebugEngine )
        {
            // Construction order important - must make primer after data source
            iPrefixes = new StackPrefixManager( this );
            iPrimer = new StackEnginePrimer( this );
            //
            iDebugEngine = aDebugEngine;
        }
		#endregion

		#region API
        public void Reconstruct( TSynchronicity aSynchronicity )
        {
            StackAlgorithmManager algorithmManager = new StackAlgorithmManager( this );
            algorithmManager.EventHandler += new StackAlgorithmManager.AlgorithmEventHandler( AlgorithmManager_EventHandler );
            algorithmManager.ExceptionHandler += new StackAlgorithmManager.AlgorithmExceptionHandler( AlgorithmManager_ExceptionHandler );
            algorithmManager.Reconstruct( aSynchronicity );
        }

        public void ReconstructAsync()
        {
            Reconstruct( TSynchronicity.EAsynchronous );
        }

        public void ReconstructSync()
        {
            Reconstruct( TSynchronicity.ESynchronous );
        }
		#endregion

		#region Properties
        public int Progress
        {
            get { return iProgress; }
        }

        public bool Verbose
        {
            get
            {
                bool ret = iVerbose;
                if ( System.Diagnostics.Debugger.IsAttached )
                {
                    ret = true;
                }
                return ret; 
            }
            set { iVerbose = value; }
        }

        public DbgEngine DebugEngine
        {
            get { return iDebugEngine; }
        }

        public StackEnginePrimer Primer
        {
            get { return iPrimer; }
        }

        public StackPrefixManager Prefixes
        {
            get { return iPrefixes; }
        }

        public StackAddressInfo AddressInfo
        {
            get { return iAddressInfo; }
        }

        public StackSourceData DataSource
        {
            get { return iDataSource; }
            set { iDataSource = value; }
        }

        public StackOutputData DataOutput
        {
            get { return iDataOutput; }
        }

        public ArmRegisterCollection Registers
        {
            get { return iRegisters; }
            set
            {
                iRegisters = value;

                // Update SP in the address info area if not already set
                if ( iAddressInfo.HaveSetStackPointer == false && iRegisters.Contains( TArmRegisterType.EArmReg_SP ) )
                {
                    iAddressInfo.Pointer = iRegisters[ TArmRegisterType.EArmReg_SP ].Value;
                }
            }
        }

        public CodeSegDefinitionCollection CodeSegments
        {
            get { return iCodeSegments; }
            set { iCodeSegments = value; }
        }
        #endregion

        #region Event handlers
        private void AlgorithmManager_ExceptionHandler( StackAlgorithmManager aAlgManager, Exception aException )
        {
            if ( ExceptionHandler != null )
            {
                ExceptionHandler( aException, this );
            }
        }

        private void AlgorithmManager_EventHandler( StackAlgorithmManager aAlgManager, StackAlgorithmManager.TEvent aEvent )
        {
            if ( EventHandler != null )
            {
                switch ( aEvent )
                {
                case StackAlgorithmManager.TEvent.EAlgorithmStarted:
                    EventHandler( TEvent.EStackBuildingStarted, this );
                    break;
                case StackAlgorithmManager.TEvent.EAlgorithmProgress:
                    iProgress = aAlgManager.Progress;
                    EventHandler( TEvent.EStackBuildingProgress, this );
                    break;
                case StackAlgorithmManager.TEvent.EAlgorithmComplete:
                    // Stop listening to algorithm manager events now that the reconstruction
                    // process is complete.
                    aAlgManager.EventHandler -= new StackAlgorithmManager.AlgorithmEventHandler( AlgorithmManager_EventHandler );
                    aAlgManager.ExceptionHandler -= new StackAlgorithmManager.AlgorithmExceptionHandler( AlgorithmManager_ExceptionHandler );

                    // Get rid of alg manager
                    aAlgManager.Dispose(); 

                    // Report that we're done!
                    EventHandler( TEvent.EStackBuildingComplete, this );
                    break;
                }
            }
        }
        #endregion

		#region Internal properties
		#endregion

        #region Internal methods
        internal void ReportMessage( TMessageType aType, string aMessage )
        {
            if ( MessageHandler != null )
            {
                MessageHandler( aType, aMessage, this );
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iDebugEngine.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iDebugEngine.Trace( aFormat, aParams );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();

            // CodeSegs
            foreach ( CodeSegDefinition entry in CodeSegments )
            {
                ret.Append( Prefixes.CodeSegment );
                ret.Append( entry.ToString() );
                ret.Append( System.Environment.NewLine );
            }

            // Current SP
            if ( AddressInfo.Pointer != 0 )
            {
                ret.AppendLine( Prefixes.Pointer + "0x" + AddressInfo.Pointer.ToString("x8") );
            }

            // Stack data
            ret.AppendLine( DataSource.ToString() );

            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly StackEnginePrimer iPrimer;
        private readonly StackPrefixManager iPrefixes;
        private readonly DbgEngine iDebugEngine;
        private bool iVerbose = false;
        private int iProgress = 0;
        private StackOutputData iDataOutput = new StackOutputData();
        private StackSourceData iDataSource = new StackSourceData();
        private StackAddressInfo iAddressInfo = new StackAddressInfo();
        private ArmRegisterCollection iRegisters = new ArmRegisterCollection();
        private CodeSegDefinitionCollection iCodeSegments = new CodeSegDefinitionCollection();
		#endregion
    }
}
