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
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using CrashItemLib.PluginAPI;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Special;
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Summarisable;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Telephony;
using CrashItemLib.Crash.Memory;
using CrashItemLib.Crash.InfoHW;
using CrashItemLib.Crash.InfoSW;
using CrashItemLib.Crash.Events;
using CrashItemLib.Crash.Header;
using CrashItemLib.Crash.Container;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Primer;
using SymbianUtils.Range;
using DExcPlugin.Extractor;
using EM=DExcPlugin.ExpressionManager.DExcExpressionManager;

namespace DExcPlugin.Transformer
{
	internal class DExcTransformer
	{
		#region Constructors
        public DExcTransformer( CFFSource aDescriptor, CFFDataProvider aDataProvider, DExcExtractedData aData )
		{
            iData = aData;
            iDescriptor = aDescriptor;
            iDataProvider = aDataProvider;
        }
        #endregion

        #region API
        public CIContainer Transform()
        {
            try
            {
                iContainer = iDataProvider.CreateContainer( iDescriptor );

                SaveInputData();

                CreateHeader();
                CIProcess process = CreateProcess();
                CreateThread( process );
            }
            catch ( Exception e )
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine( "DEXC READER QUEUE EXCEPTION: " + e.Message );
                System.Diagnostics.Debug.WriteLine( "DEXC READER QUEUE STACK: " + e.StackTrace );
#endif
                //
                iContainer = iDataProvider.CreateErrorContainer( iDescriptor );
                CIMessageError error = new CIMessageError( iContainer, "Error" );
                error.AddLine( e.Message );
                iContainer.Messages.Add( error );
            }
            //
            return iContainer;
        }
        #endregion

        #region Internal methods
        private void SaveInputData()
        {
            string inputData = iData.ToString();

            // Convert the entire text data into binary
            List<byte> inputDataAsBytes = new List<byte>();

            // We just want to map the raw unicode character onto a single byte.
            // ASCII range is probably not sufficient (guess?) so this is why we
            // do not use System.Text.ASCIIEncoding, but rather roll our own.
            foreach ( char c in inputData )
            {
                byte b = System.Convert.ToByte( c );
                inputDataAsBytes.Add( b );
            }

            CISourceElement source = iContainer.Source;
            source.InputDataClear();
            source.InputDataAdd( inputDataAsBytes.ToArray() );
        }

        private void CreateHeader()
        {
            CIHeader header = iContainer.Header;
            header.CrashTime = iDescriptor.MasterFile.LastWriteTime;
            header.FileFormatVersion = "D_EXC for Symbian OS EKA2";
        }

        private CIProcess CreateProcess()
        {
            CIProcess process = new CIProcess( iContainer );

            ExtractProcess( process );
            ExtractProcessCodeSegs( process );

            iContainer.AddChild( process );
            return process;
        }

        private CIThread CreateThread( CIProcess aProcess )
        {
            // Make a new thread
            CIThread thread = aProcess.CreateThread();

            // Extract items
            ExtractThread( thread );
            ExtractThreadExitReason( thread );
            ExtractThreadRegisters( thread );
            ExtractThreadStack( thread );

            iContainer.AddChild( thread );
            return thread;
        }
        #endregion

        #region Internal constants
        #endregion

        #region Helpers - process
        private void ExtractProcess( CIProcess aProcess )
        {
            // Extract process info from thread full name.
            DExcExtractorList threadInfo = iData[ DExcExtractorListType.EListThread ];
            foreach ( string line in threadInfo )
            {
                Match m = EM.ThreadName.Match( line );
                if ( m.Success )
                {
                    CIFullNameUtils parser = new CIFullNameUtils( m.Groups[ 1].Value );
                    parser.GetProcessInfo( aProcess );
                    
                    return;
                }
            }
        }

        private void ExtractProcessCodeSegs( CIProcess aProcess )
        {
            DExcExtractorList codeSegInfo = iData[ DExcExtractorListType.EListCodeSegments ];
            foreach ( string line in codeSegInfo )
            {
                Match m = EM.CodeSegmentsEntry.Match( line );
                if ( m.Success )
                {
                    GroupCollection groups = m.Groups;
                    //
                    uint codeSegBase = uint.Parse( groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber );
                    uint codeSegLimit = uint.Parse( groups[ 2 ].Value, System.Globalization.NumberStyles.HexNumber );
                    string codeSegName = groups[ 3 ].Value;
                    //
                    aProcess.CreateCodeSeg( codeSegName, codeSegBase, codeSegLimit );
                }
            }
        }
        #endregion

        #region Helpers - thread
        private void ExtractThread( CIThread aThread )
        {
            // Extract process info from thread full name.
            DExcExtractorList threadInfo = iData[ DExcExtractorListType.EListThread ];
            foreach ( string line in threadInfo )
            {
                Match m = EM.ThreadName.Match( line );
                if ( m.Success )
                {
                    CIFullNameUtils parser = new CIFullNameUtils( m.Groups[ 1 ].Value );
                    parser.GetThreadInfo( aThread );
                }
                else
                {
                    m = EM.ThreadId.Match( line );
                    if ( m.Success )
                    {
                        aThread.Id = int.Parse( m.Groups[ 1 ].Value );
                    }
                }
            }
        }

        private void ExtractThreadExitReason( CIThread aThread )
        {
            aThread.ExitInfo.Type = CrashItemLib.Crash.ExitInfo.CIExitInfo.TExitType.EExitTypeException;

            // Extract process info from thread full name.
            DExcExtractorList threadInfo = iData[ DExcExtractorListType.EListThread ];
            foreach ( string line in threadInfo )
            {
                Match m = EM.ThreadPanicDetails.Match( line );
                if ( m.Success )
                {
                    aThread.ExitInfo.Type = CrashItemLib.Crash.ExitInfo.CIExitInfo.TExitType.EExitTypePanic;
                    aThread.ExitInfo.Category = m.Groups[ 1 ].Value;
                    aThread.ExitInfo.Reason = int.Parse( m.Groups[ 2 ].Value );
                }
            }
        }

        private void ExtractThreadRegisters( CIThread aThread )
        {
            CIThreadRegisterListCollection threadRegs = aThread.Registers;
            CIRegisterList regListUser = threadRegs[ TArmRegisterBank.ETypeUser ];
            CIRegisterList regListEXC = threadRegs[ TArmRegisterBank.ETypeException ];
            CIRegisterList regListCOP = threadRegs[ TArmRegisterBank.ETypeCoProcessor ];
            CIRegisterList regListSVC = threadRegs[ TArmRegisterBank.ETypeSupervisor ];

            #region User registers
            foreach ( string line in iData[ DExcExtractorListType.EListRegistersUser ] )
            {
                Match m = EM.RegistersUserSet.Match( line );
                if ( m.Success )
                {
                    GroupCollection groups = m.Groups;
                    int firstReg = int.Parse( groups[ 1 ].Value );
                    for ( int i = firstReg; i < firstReg + 4; i++ )
                    {
                        Group gp = groups[ 2 + ( i - firstReg ) ];
                        uint value = uint.Parse( gp.Value, System.Globalization.NumberStyles.HexNumber );
                        TArmRegisterType regType = (TArmRegisterType) i;
                        regListUser[ regType ].Value = value;
                    }
                }
                else
                {
                    m = EM.RegistersUserCPSR.Match( line );
                    if ( m.Success )
                    {
                        // Get CPSR value and set it
                        uint cpsrValue = uint.Parse( m.Groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber );
                        threadRegs.CPSR = cpsrValue;
                    }
                }
            }
            #endregion

            #region Exception registers
            foreach ( string line in iData[ DExcExtractorListType.EListRegistersException ] )
            {
                Match m = EM.RegistersExceptionSet1.Match( line );
                if ( m.Success )
                {
                    GroupCollection groups = m.Groups;
                    //
                    regListEXC[ TArmRegisterType.EArmReg_EXCCODE ].Value = uint.Parse( m.Groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber );
                    regListEXC[ TArmRegisterType.EArmReg_EXCPC ].Value = uint.Parse( m.Groups[ 2 ].Value, System.Globalization.NumberStyles.HexNumber );
                    //
                    regListCOP[ TArmRegisterType.EArmReg_FAR ].Value = uint.Parse( m.Groups[ 3 ].Value, System.Globalization.NumberStyles.HexNumber );
                    regListCOP[ TArmRegisterType.EArmReg_FSR ].Value = uint.Parse( m.Groups[ 4 ].Value, System.Globalization.NumberStyles.HexNumber );

                    if ( regListEXC.Contains( TArmRegisterType.EArmReg_EXCCODE ) )
                    {
                        CIRegister reg = regListEXC[ TArmRegisterType.EArmReg_EXCCODE ];
                        System.Diagnostics.Debug.Assert( reg is CIRegisterExcCode );
                        CIRegisterExcCode excReg = (CIRegisterExcCode) reg;
                        //
                        excReg.ExpandToFullExceptionRange();
                    }

                    // It also means it was an exception
                    aThread.ExitInfo.Type = CrashItemLib.Crash.ExitInfo.CIExitInfo.TExitType.EExitTypeException;
                }
                else
                {
                    m = EM.RegistersExceptionSet2.Match( line );
                    if ( m.Success )
                    {
                        GroupCollection groups = m.Groups;
                        //
                        regListSVC[ TArmRegisterType.EArmReg_SP ].Value = uint.Parse( m.Groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber );
                        regListSVC[ TArmRegisterType.EArmReg_LR ].Value = uint.Parse( m.Groups[ 2 ].Value, System.Globalization.NumberStyles.HexNumber );
                        regListSVC[ TArmRegisterType.EArmReg_SPSR ].Value = uint.Parse( m.Groups[ 3 ].Value, System.Globalization.NumberStyles.HexNumber );
                    }
                }
            }
            #endregion
        }

        private void ExtractThreadStack( CIThread aThread )
        {
            DExcExtractorListStackData stackDataList = (DExcExtractorListStackData) iData[ DExcExtractorListType.EListStackData ];
            DataBuffer stackData = stackDataList.StackData;

            // Get stack range details
            DExcExtractorListThreadInfo threadInfo = (DExcExtractorListThreadInfo) iData[ DExcExtractorListType.EListThread ];
            AddressRange stackRange = threadInfo.StackRange;

            // If we didn't get the stack range, we cannot create a stack entry
            if ( !stackRange.IsValid || stackRange.Max == 0 || stackRange.Min == 0 )
            {
                CIMessageWarning warning = new CIMessageWarning( aThread.Container, "Stack Address Range Unavailable" );
                warning.AddLine( "The stack address range details are invalid." );
                aThread.AddChild( warning );
            }
            else if ( stackData.Count == 0 )
            {
                // No stack data
                CIMessageWarning warning = new CIMessageWarning( aThread.Container, "Stack Data Unavailable" );
                warning.AddLine( "The crash details contain no stack data." );
                aThread.AddChild( warning );
            }
            else
            {
                // Set base address of data buffer if not already set
                if ( stackData.AddressOffset == 0 )
                {
                    stackData.AddressOffset = stackRange.Min;
                }

                // In theory, D_EXC only ever captures user-side crashes (panics/exceptions) therefore
                // we should always be able to assume that the stack data goes with a user-side thread.
                CIRegisterList userRegs = aThread.Registers[ TArmRegisterBank.ETypeUser ];
                if ( userRegs != null )
                {
                    CIStack stack = aThread.CreateStack( userRegs, stackData, stackData.AddressOffset, stackRange );

                    // Register it as a specific crash instance
                    iContainer.AddChild( stack );
                }
            }
        }
        #endregion

        #region Data members
        private readonly CFFDataProvider iDataProvider;
        private readonly CFFSource iDescriptor;
        private readonly DExcExtractedData iData = null;
        private CIContainer iContainer = null;
		#endregion
	}
}
