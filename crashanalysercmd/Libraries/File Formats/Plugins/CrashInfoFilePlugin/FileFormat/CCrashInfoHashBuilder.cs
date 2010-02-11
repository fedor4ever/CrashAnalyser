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
﻿using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Special;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Summarisable;
using SymbianStructuresLib.Arm.Registers;


namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    public class CCrashInfoHashBuilder
    {
        #region Enumerations
        [Flags]
        public enum TConfiguration
        {
            // <summary>
            // Include stack data in hash
            // </summary>
            EIncludeStack = 1,

            // <summary>
            // Force inclusion of stack data in hash, even if hash builder
            // believes that it's inclusion is not needed in order to uniquely
            // pin-point the crash.
            // </summary>
            EIncludeStackForced = 2,

            // <summary>
            // By default, offset is only included in program counter symbol. 
            // This will force it to be incldued for all symbols (i.e. even
            // those within stack trace).
            // </summary>
            EIncludeOffsetsForAllSymbols = 4,

            // <summary>
            // Include the processor mode at the time of the crash within the hash.
            // </summary>
            EIncludeProcessorMode = 8,

            // <summary>
            // By default we output an MD5 hash, but you can enable plain text
            // output by using this option.
            // </summary>
            EOutputAsText = 16,
            
            // <summary>
            // A general purpose default configuration
            // </summary>
            EDefault = EIncludeStack | EIncludeStackForced | EIncludeProcessorMode
        }
        #endregion

        #region Factory
        public static CCrashInfoHashBuilder New( TConfiguration aConfig, CISummarisableEntity aEntity )
        {
            CCrashInfoHashBuilder ret = CCrashInfoHashBuilder.New( aConfig, aEntity, KDefaultNumberOfStackEntriesToCheckForSymbols );
            return ret;
        }

        public static CCrashInfoHashBuilder New( TConfiguration aConfig, CISummarisableEntity aEntity, int aNumberOfStackEntriesToCheck )
        {
            // By default we'll return an empty string for the hash if mandatory input
            // elements are not available
            CCrashInfoHashBuilder ret = null;
            //
            CIStack stack = aEntity.Stack;
            CIThread thread = aEntity.Thread;
            CIProcess process = aEntity.Process;
            CIRegisterList regs = aEntity.Registers;
            //
            if ( stack != null && regs != null )
            {
                if ( process == null || thread == null )
                {
                    ret = new CCrashInfoHashBuilder( aConfig, regs, stack, aNumberOfStackEntriesToCheck );
                }
                else
                {
                    ret = new CCrashInfoHashBuilder( aConfig, regs, stack, thread, process, aNumberOfStackEntriesToCheck );
                }
            }
            else
            {
                throw new ArgumentException( LibResources.KRes_CCrashInfoHashBuilder_BadSummarisable );
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        private CCrashInfoHashBuilder( TConfiguration aConfig, CIRegisterList aRegisters, CIStack aStack, int aNumberOfStackEntriesToCheck )
            : this( aConfig, aRegisters, aStack, null, null, aNumberOfStackEntriesToCheck )
        {
        }

        private CCrashInfoHashBuilder( TConfiguration aConfig, CIRegisterList aRegisters, CIStack aStack, CIThread aThread, CIProcess aProcess, int aNumberOfStackEntriesToCheck )
        {
            iConfig = aConfig;
            iRegisters = aRegisters;
            iStack = aStack;
            iProcess = aProcess;
            iThread = aThread;
            iNumberOfStackEntriesToCheck = aNumberOfStackEntriesToCheck;
        }
        #endregion

        #region API
        public string GetHash()
        {
            StringBuilder hash = new StringBuilder();
            //
            if ( ( iConfig & TConfiguration.EIncludeProcessorMode ) != 0 )
            {
                string processorMode = GetProcessorMode();
                hash.AppendFormat( "MODE: [{0}] ", processorMode );
            }
            //
            string moduleName = GetAppropriateBinaryModuleName();
            hash.AppendFormat( "MODN: [{0}] ", moduleName );
            //
            string programCounter = GetProgramCounter();
            hash.AppendFormat( "PC: [{0}] ", programCounter );
            //
            if ( ( iConfig & TConfiguration.EIncludeStack ) != 0 || ( iConfig & TConfiguration.EIncludeStackForced ) != 0 )
            {
                string stackSymbols = GetStackSymbols();
                hash.AppendFormat( "STK: [{0}] ", stackSymbols );
            }
            
            // Final stage is to MD5 hash the text, unless the caller requested
            // plain text output.
            string ret = hash.ToString();            
            if ( ( iConfig & TConfiguration.EOutputAsText ) == 0 )
            {
                ret = GetMD5( ret );
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private const int KDefaultNumberOfStackEntriesToCheckForSymbols = 4;
        #endregion

        #region Internal methods
        private string GetProcessorMode()
        {
            string ret = LibResources.KRes_CCrashInfoHashBuilder_NoCPUMode;
            //
            if ( iRegisters.IsCurrentProcessorMode )
            {
                CIRegisterCPSR cpsr = iRegisters[ TArmRegisterType.EArmReg_CPSR ] as CIRegisterCPSR;
                if ( cpsr != null )
                {
                    ret = ArmRegisterBankUtils.BankAsString( cpsr.ProcessorMode );
                }
            }
            //
            return ret;
        }

        private string GetAppropriateBinaryModuleName()
        {
            // We'll use the name of the binary associated with the program 
            // counter symbol (if present) or then if not, we'll fall back
            // to using the process name.
            string ret = string.Empty;
            //
            bool fallBack = false;
            if ( iRegisters.Contains( TArmRegisterType.EArmReg_PC ) )
            {
                CIRegister pc = iRegisters[ TArmRegisterType.EArmReg_PC ];
                if ( pc.Symbol.IsNull == false )
                {
                    // Symbol available - use the associated binary name
                    string binName = pc.Symbol.Symbol.Collection.FileName.EitherFullNameButDevicePreferred;
                    ret = Path.GetFileName( binName );
                }
                else
                {
                    fallBack = true;
                }
            }
            else
            {
                fallBack = true;
            }

            // Do we need to fallback because symbol et al was unavailable?
            if ( fallBack )
            {
                // No Symbol, then in this case we'll try to fall back
                // to the process name. 
                if ( iProcess != null )
                {
                    ret = iProcess.Name;
                }
                else
                {
                    // Must be e.g. IRQ, FIQ, ABT, etc
                    ret = LibResources.KRes_CCrashInfoHashBuilder_AbortModeStack;
                }
            }
            //
            return ret;
        }

        private string GetProgramCounter()
        {
            string ret = string.Empty;
            //
            if ( iRegisters.Contains( TArmRegisterType.EArmReg_PC ) )
            {
                CIRegister pc = iRegisters[ TArmRegisterType.EArmReg_PC ];
                ret = CleanSymbol( pc.Value, pc.Symbol, true );
            }
            //
            return ret;
        }

        private string GetStackSymbols()
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( iStack.IsStackOutputAvailable )
            {
                bool isOverflow = iStack.IsOverflow;
                bool isForced = ( iConfig & TConfiguration.EIncludeStackForced ) != 0;
                bool isNullDereference = ( iStack.Registers.Contains( TArmRegisterType.EArmReg_00 ) && iStack.Registers[ TArmRegisterType.EArmReg_00 ].Value == 0 );

                // If dealing with a stack overflow, then we don't, by default, include any
                // symbols from the stack, as they are likely to be entirely dirty or "ghosts" for
                // the most part. 
                //
                // Furthermore, if the 'crash' was caused by dereferencing a NULL this pointer
                // (which is somewhat of a guess on our part, but we ascertain this by inspecting
                // the value of R0) then we don't include stack either.
                //
                // However, the above behaviour can be overriden by forcing stack processing.
                bool processEntries = ( ( isOverflow == false && isNullDereference == false ) || isForced );
                if ( processEntries )
                {
                    // Get the entries and work out the number of items we should check for
                    // associated symbols.
                    CIElementList<CIStackEntry> stackEntries = iStack.ChildrenByType<CIStackEntry>();

                    // Discard all the entries that are outside of the stack pointer range.
                    // Once we see a register-based entry (for accurate decoding) or the current
                    // stack pointer entry, we've reached the start of the interesting stack data.
                    //
                    // However, if there has been a stack overflow then don't discard anything
                    // or else we'll end up with nothing left!
                    if ( iStack.IsOverflow == false && stackEntries.Count > 0 )
                    {
                        int i = 0;
                        while ( stackEntries.Count > 0 )
                        {
                            // If we see a register based entry then the stack reconstruction almost certainly
                            // used the accurate algorithm.
                            //
                            // Since we have already included the program counter in the hash text, it doesn't
                            // make sense to include it twice, so skip that.
                            // However, link register might be useful.
                            bool remove = true;
                            CIStackEntry entry = stackEntries[ i ];

                            if ( entry.IsCurrentStackPointerEntry )
                            {
                                break;
                            }
                            else if ( entry.IsRegisterBasedEntry )
                            {
                                // Preserve LR, skip PC
                                if ( entry.Register.Type == TArmRegisterType.EArmReg_LR )
                                {
                                    ++i;
                                    remove = false;
                                }
                            }

                            if ( remove )
                            {
                                stackEntries.RemoveAt( 0 );
                            }
                        }
                    }

                    // Did the caller also want offsets within the output? By default only the program
                    // counter receives this treatment, but it can be overridden.
                    bool includeOffset = ( iConfig & TConfiguration.EIncludeOffsetsForAllSymbols ) != 0;

                    // We should now have the stack entries directly relating to the crash call stack.
                    // Process them in turn, but only look at entries which happen to have associated
                    // symbols.
                    
                    int SymbolsNeeded = iNumberOfStackEntriesToCheck;
                    foreach (CIStackEntry entry in stackEntries)
                    {                       
                        if ( entry.Symbol != null && entry.Symbol.IsNull == false )
                        {
                            string txt = CleanSymbol( entry.Data, entry.Symbol, includeOffset );
                            ret.AppendFormat( "{0}, ", txt );
                            SymbolsNeeded--;
                        }     
                        if (SymbolsNeeded == 0)
                        {
                            break;
                        }
                    }
                   
                    // Remove trailing comma
                    string t = ret.ToString();
                    if ( t.EndsWith( ", " ) )
                    {
                        ret = ret.Remove( ret.Length - 2, 2 );
                    }
                }
            }
            //
            string final = ret.ToString().TrimEnd();
            return final;
        }

        private string GetMD5( string aMakeHash )
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes( aMakeHash );
            byte[] hash = md5.ComputeHash( inputBytes );
            //
            StringBuilder sb = new StringBuilder();
            //
            for ( int i = 0; i < hash.Length; i++ )
            {
                sb.Append( hash[ i ].ToString( "x2" ) );
            }
            //
            return sb.ToString();
        }

        private static string CleanSymbol( uint aAddress, CISymbol aSymbol, bool aAddOffset )
        {
            string ret = string.Empty;
            //
            if ( aSymbol.IsNull == false )
            {
                if ( aAddOffset )
                {
                    // Only include the name and offset, not the address
                    uint offset = aSymbol.Symbol.Offset( aAddress );
                    ret = string.Format( "|0x{0:x4}| {1}", offset, aSymbol.Symbol.Name );
                }
                else
                {
                    ret = aSymbol.Symbol.Name;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly TConfiguration iConfig;
        private readonly CIRegisterList iRegisters;
        private readonly CIStack iStack;
        private readonly CIThread iThread;
        private readonly CIProcess iProcess;
        private readonly int iNumberOfStackEntriesToCheck;
        #endregion
    }
}
