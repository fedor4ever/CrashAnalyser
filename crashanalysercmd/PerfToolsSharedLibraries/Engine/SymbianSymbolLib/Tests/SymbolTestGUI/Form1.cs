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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbols;

namespace SymbolTestGUI
{
    public partial class Form1 : Form, ITracer
    {
        #region Constructors
        public Form1()
        {
            InitializeComponent();

            iDebugEngine = new DbgEngine( this );
            iDebugEngine.UiMode = SymbianDebugLib.TDbgUiMode.EUiDisabled;
            iDebugEngine.EntityPrimingStarted += new DbgEngine.EventHandler( DebugEngine_EntityPrimingStarted );
            iDebugEngine.EntityPrimingProgress += new DbgEngine.EventHandler( DebugEngine_EntityPrimingProgress );
            iDebugEngine.EntityPrimingComplete += new DbgEngine.EventHandler( DebugEngine_EntityPrimingComplete );
        }
        #endregion

        #region Event handlers
        private void DebugEngine_EntityPrimingStarted( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( this.InvokeRequired )
            {
                DbgEngine.EventHandler observer = new DbgEngine.EventHandler( DebugEngine_EntityPrimingStarted );
                this.BeginInvoke( observer, new object[] { aEngine, aEntity, aContext } );
            }
            else
            {
                label1.Text = "Reading...";
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;
                progressBar1.Value = 0;
            }
        }

        private void DebugEngine_EntityPrimingProgress( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( this.InvokeRequired )
            {
                DbgEngine.EventHandler observer = new DbgEngine.EventHandler( DebugEngine_EntityPrimingProgress );
                this.BeginInvoke( observer, new object[] { aEngine, aEntity, aContext } );
            }
            else
            {
                if ( aContext != null && aContext is int )
                {
                    progressBar1.Value = (int) aContext;
                }
            }
        }

        private void DebugEngine_EntityPrimingComplete( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( this.InvokeRequired )
            {
                DbgEngine.EventHandler observer = new DbgEngine.EventHandler( DebugEngine_EntityPrimingComplete );
                this.BeginInvoke( observer, new object[] { aEngine, aEntity, aContext } );
            }
            else
            {
                DateTime endTime = DateTime.Now;
                TimeSpan span = endTime - iStartTime;
                label1.Text = "Done - " + span.ToString();
                progressBar1.Value = progressBar1.Maximum;

                if ( AreAllEntitiesPrimed )
                {
                    RunTests();
                }
            }
        }

        private void button1_Click( object sender, EventArgs e )
        {
            iDebugEngine.Add( @"Z:\epoc32\rom\S60_3_2_200846_RnD_merlin_emulator_hw.rom.symbol" );
            //iDebugEngine.Add( @"z:\epoc32\rom\S60_3_2_200846_RnD_merlin_emulator_hw.rofs1.oby" );
            //iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File43\CoreImage\MemSpy.EXE.map" );
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File44\Platform_wk49\Symbols\mapfiles.zip" );
            //iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File55\alarmserver.exe.map" );

            iStartTime = DateTime.Now;
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.EAsynchronous );
        }
        #endregion

        #region Internal methods
        private bool AreAllEntitiesPrimed
        {
            get
            {
                bool ret = true;
                //
                foreach ( DbgEntity entity in iDebugEngine )
                {
                    if ( !entity.IsPrimed )
                    {
                        ret = false;
                        break;
                    }
                }
                //
                return ret;
            }
        }

        private void RunTests()
        {
            //RunObyMapViewTest();
            RunZipMapFileTest();
        }

        private void RunObyMapViewTest()
        {
            CodeSegDefinitionCollection codeSegs = new CodeSegDefinitionCollection();
            //
            codeSegs.Add( new CodeSegDefinition( @"Z:\sys\bin\WidgetLauncher.exe", 0x7A120000, 0x7A170000 ) );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView", codeSegs ) )
            {
            }
        }

        private void CheckNoOverlaps( SymbolCollection aCollection )
        {
            int count = aCollection.Count;
            for ( int i = 0; i < count - 1; i++ )
            {
                Symbol s1 = aCollection[ i + 0 ];
                Symbol s2 = aCollection[ i + 1 ];
                //
                System.Diagnostics.Debug.WriteLine( "Comparing: " + s1 + " vs " + s2 );
                //
                System.Diagnostics.Debug.Assert( s1.AddressRange.Min <= s1.AddressRange.Max );
                System.Diagnostics.Debug.Assert( s1.AddressRange.Min < s2.AddressRange.Min );
                System.Diagnostics.Debug.Assert( s1.AddressRange.Max < s2.AddressRange.Min );
            }
        }

        private Symbol FindByName( string aName, SymbolCollection aCollection )
        {
            Symbol ret = null;
            //
            foreach ( Symbol sym in aCollection )
            {
                if ( sym.Name == aName )
                {
                    ret = sym;
                    break;
                }
            }
            //
            return ret;
        }

        private void RunZipMapFileTest()
        {
            // So we can spot it in the profiler...
            //Thread.Sleep( 2000 );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;

                // Should be possible to activate a file within a zip
                SymbolCollection activatedCol = symView.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\AcCmOnItOr.dll", 0x70000000, 0x7A000000 ) );
                System.Diagnostics.Debug.Assert( activatedCol != null );
                
                // Verify that the symbols were really read.
                SymbolCollection col = view.Symbols.CollectionByAddress( 0x70000000 );
                //System.Diagnostics.Debug.Assert( col.Count == 0xaa );
                System.Diagnostics.Debug.WriteLine( col.ToString( "full", null ) );

                // Verify that the collections are the same
                System.Diagnostics.Debug.Assert( activatedCol.Count == col.Count );
                System.Diagnostics.Debug.Assert( activatedCol.FileName == col.FileName );
                System.Diagnostics.Debug.Assert( activatedCol == col );

                // Cannot activate the same dll twice
                bool activated = symView.Activate( new CodeSegDefinition( @"Z:\sys\bin\AcCmOnItOr.dll", 0x80000000, 0x8A000000 ) );
                System.Diagnostics.Debug.Assert( activated == false );

                // Cannot activate an overlapping area.
                activated = symView.Activate( new CodeSegDefinition( @"Z:\sys\bin\AIFW.dll", 0x70000000, 0x70040000 ) );
                System.Diagnostics.Debug.Assert( activated == false );

                // Cannot deactivate a non-activated dll
                bool deactivated = symView.Deactivate( new CodeSegDefinition( @"Z:\sys\bin\AIUTILS.DLL" ) );
                System.Diagnostics.Debug.Assert( deactivated == false );

                // Cannot deactivate a missing dll
                deactivated = symView.Deactivate( new CodeSegDefinition( @"Z:\sys\bin\THIS_DOES_NOT_EXIST.EXE" ) );
                System.Diagnostics.Debug.Assert( deactivated == false );

                // Look up first symbol
                Symbol sym = null;
                Symbol sym2 = null;
                sym = symView.Lookup( 0x70000000, out col );
                System.Diagnostics.Debug.Assert( sym != null && col == activatedCol && sym.Name == "_E32Dll" );
                sym = symView.Lookup( 0x70000027, out col );
                System.Diagnostics.Debug.Assert( sym != null && col == activatedCol && sym.Name == "_E32Dll" );

                // For the following sequence, ensure that we discard the CAccMonitor::~CAccMonitor__sub_object()
                // line and keep the CAccMonitor::~CAccMonitor() line instead. Ensure that the size of the 
                // CAccMonitor::~CAccMonitor() entry has been calculated using the data from the sub_object entry which
                // we threw away.
                //
                //     CAccMonitor::~CAccMonitor__deallocating() 0x00009195   Thumb Code    16  accmonitor.in(i._ZN11CAccMonitorD0Ev)
                //     CAccMonitor::~CAccMonitor()              0x000091a5   Thumb Code     0  accmonitor.in(i._ZN11CAccMonitorD2Ev)
                //     CAccMonitor::~CAccMonitor__sub_object()  0x000091a5   Thumb Code     8  accmonitor.in(i._ZN11CAccMonitorD2Ev)
                //     CAccMonitorInfo::Reset()                 0x000091ad   Thumb Code    28  accmonitor.in(i._ZN15CAccMonitorInfo5ResetEv)
                //
                sym = FindByName( "CAccMonitor::~CAccMonitor__sub_object()", col );
                System.Diagnostics.Debug.Assert( sym == null );
                sym = FindByName( "CAccMonitor::~CAccMonitor()", col );
                System.Diagnostics.Debug.Assert( sym != null && sym.Size == 8 );

                // For the following sequence, ensure that we discard the sub object and keep the destructor.
                //
                //      RArray<unsigned long>::RArray()          0x00009289   Thumb Code    10  accmonitor.in(t._ZN6RArrayImEC1Ev)
                //      RArray<unsigned long>::RArray__sub_object() 0x00009289   Thumb Code     0  accmonitor.in(t._ZN6RArrayImEC1Ev)
                sym = FindByName( "RArray<unsigned long>::RArray__sub_object()", col );
                System.Diagnostics.Debug.Assert( sym == null );
                sym = FindByName( "RArray<unsigned long>::RArray()", col );
                System.Diagnostics.Debug.Assert( sym != null && sym.Size == 10 );

                // For the following sequence, ensure that the end of the first entry doesn't overlap with the start of the second.
                //
                //      typeinfo name for CAccMonitorCapMapper   0x000094a8   Data          23  accmonitor.in(.constdata__ZTS20CAccMonitorCapMapper)
                //      typeinfo name for CAccMonitorContainer   0x000094bf   Data          23  accmonitor.in(.constdata__ZTS20CAccMonitorContainer)
                //
                sym = FindByName( "typeinfo name for CAccMonitorCapMapper", col );
                System.Diagnostics.Debug.Assert( sym != null );
                sym2 = FindByName( "typeinfo name for CAccMonitorContainer", col );
                System.Diagnostics.Debug.Assert( sym2 != null );
                System.Diagnostics.Debug.Assert( sym.AddressRange.Max + 1 == sym2.Address );

                // Check no overlap
                CheckNoOverlaps( col );
 
                // Second symbol
                sym = symView.Lookup( 0x70000028, out col );
                System.Diagnostics.Debug.Assert( sym != null && col == activatedCol && sym.Name == "__cpp_initialize__aeabi_" );

                // Multithreaded symbol lookup times
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter1, col, 10000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter2, col, 5000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter3, col, 8000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter4, col, 20000 ) );

                // Wait
                using( iWaiter4 )
                {
                    iWaiter4.WaitOne();
                }
                using ( iWaiter3 )
                {
                    iWaiter3.WaitOne();
                }
                using ( iWaiter2 )
                {
                    iWaiter2.WaitOne();
                }
                using ( iWaiter1 )
                {
                    iWaiter1.WaitOne();
                }

                // Deactivate an activated dll
                deactivated = symView.Deactivate( new CodeSegDefinition( @"Z:\sys\bin\ACCMONITOR.DLL" ) );
                System.Diagnostics.Debug.Assert( deactivated == true );
            
                // symbol shouldn't be available anymore
                sym = symView.Lookup( 0x70000000, out col );
                System.Diagnostics.Debug.Assert( sym == null && col == null );
            }
        }

        #region Internal classes
        private class AsyncData
        {
            public AsyncData( DbgViewSymbols aView, AutoResetEvent aWaiter, SymbolCollection aCollection, int aIterations )
            {
                iView = aView;
                iWaiter = aWaiter;
                iCollection = aCollection;
                iIterations = aIterations;
            }

            public readonly DbgViewSymbols iView;
            public readonly AutoResetEvent iWaiter;
            public readonly SymbolCollection iCollection;
            public readonly int iIterations;
        }
        #endregion

        private void MultiThreadedLookup( object aState )
        {
            AsyncData data = (AsyncData) aState;
            Random rand = new Random( 666 );
            AddressRange colRange = data.iCollection.SubsumedPrimaryRange;
            //
            for ( int i = 0; i < data.iIterations; i++ )
            {
                uint offset = (uint) rand.Next( (int) colRange.Size );
                uint address = colRange.Min + offset;
                //
                SymbolCollection col = null;
                Symbol sym = data.iView.Lookup( address, out col );
            }

            data.iWaiter.Set();
        }
        #endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            System.Diagnostics.Debug.WriteLine( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string t = string.Format( aFormat, aParams );
            Trace( t );
        }
        #endregion

        #region Data members
        private DateTime iStartTime = new DateTime();
        private readonly DbgEngine iDebugEngine;
        private AutoResetEvent iWaiter1 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter2 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter3 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter4 = new AutoResetEvent( false );
        #endregion
    }
}
