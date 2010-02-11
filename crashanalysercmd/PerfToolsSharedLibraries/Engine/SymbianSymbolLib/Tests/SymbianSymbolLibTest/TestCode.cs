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
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbols;


namespace SymbianSymbolLibTest
{
    class TestCode : DisposableObject, ITracer
    {
        #region Constructors
        public TestCode()
        {
            iDebugEngine = new DbgEngine( this );
            iDebugEngine.UiMode = SymbianDebugLib.TDbgUiMode.EUiDisabled;
            iDebugEngine.EntityPrimingStarted += new DbgEngine.EventHandler( DebugEngine_EntityPrimingStarted );
            iDebugEngine.EntityPrimingProgress += new DbgEngine.EventHandler( DebugEngine_EntityPrimingProgress );
            iDebugEngine.EntityPrimingComplete += new DbgEngine.EventHandler( DebugEngine_EntityPrimingComplete );
        }
        #endregion

        #region API
        public void Clear()
        {
            iDebugEngine.Clear();
        }

        public void RunTests()
        {
            TestUDACode();
            TestCodeSegmentResolutionOBY();
            TestOBY();
            TestBigMapFile();
            TestCodeSegmentResolutionROFS();
            TestMapRVCT();
            TestMapGCCE();
            TestBigDsoData();
            TestZipMapFiles();
            TestHeapCellSymbolLookup();
            TestRofsDllAtDifferentBaseAddresses();
        }
        #endregion

        #region Tests
        private void TestRofsDllAtDifferentBaseAddresses()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view1 = iDebugEngine.CreateView( "TestView" ) )
            {
                SymbolCollection colPageScalerAt70000000 = view1.Symbols.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\PageScaler.dll", 0x70000000, 0x7A000000 ) );
                System.Diagnostics.Debug.Assert( colPageScalerAt70000000 != null );
                SymbolCollection col1 = view1.Symbols.CollectionByAddress( 0x70000000 );
                System.Diagnostics.Debug.Assert( col1 != null );

                // Make a second view
                using ( DbgEngineView view2 = iDebugEngine.CreateView( "TestView" ) )
                {
                    SymbolCollection colPageScalerAt75000000 = view2.Symbols.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\PageScaler.dll", 0x75000000, 0x7A000000 ) );
                    System.Diagnostics.Debug.Assert( colPageScalerAt75000000 != null );
                    SymbolCollection col2 = view2.Symbols.CollectionByAddress( 0x75000000 );
                    System.Diagnostics.Debug.Assert( col2 != null );

                    // Check invalid requests
                    Symbol symTemp = null;

                    symTemp = view1.Symbols[ 0x80240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );
                    symTemp = view1.Symbols[ 0x74240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );
                    symTemp = view1.Symbols[ 0x78240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );

                    symTemp = view2.Symbols[ 0x80240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );
                    symTemp = view2.Symbols[ 0x74240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );
                    symTemp = view2.Symbols[ 0x78240000 ];
                    System.Diagnostics.Debug.Assert( symTemp == null );

                    // Check offsets are maintained
                    int count = col1.Count;
                    for( int i=0; i<count; i++ )
                    {
                        Symbol sym1 = col1[ i ];
                        Symbol sym2 = col2[ i ];
                        //
                        System.Diagnostics.Debug.Assert( sym1.Name == sym2.Name );
                        System.Diagnostics.Debug.Assert( sym1.Object == sym2.Object );
                        System.Diagnostics.Debug.Assert( sym1.Type == sym2.Type );
                        //
                        uint delta = sym2.Address - sym1.Address;
                        System.Diagnostics.Debug.Assert( delta == ( 0x75000000 - 0x70000000 ) );
                    }
                }
            }
        }

        private void TestMulitThreadedLookup()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;

                SymbolCollection col = null;

                // Should be possible to activate a file within a zip
                SymbolCollection colPageScaler = symView.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\PageScaler.dll", 0x70000000, 0x7A000000 ) );
                System.Diagnostics.Debug.Assert( colPageScaler != null );

                // Verify that the symbols were really read.
                col = view.Symbols.CollectionByAddress( 0x70000000 );
                System.Diagnostics.Debug.Assert( col != null );
                System.Diagnostics.Debug.WriteLine( col.ToString( "full", null ) );

                // Multithreaded symbol lookup times
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter1, col, 10000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter2, col, 5000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter3, col, 8000 ) );
                ThreadPool.QueueUserWorkItem( new WaitCallback( MultiThreadedLookup ), new AsyncData( symView, iWaiter4, col, 20000 ) );

                // Wait
                using ( iWaiter4 )
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
            }
        }

        private void TestZipMapFiles()
        {
            // So we can spot it in the profiler...
            //Thread.Sleep( 2000 );
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\8. For SymbianSymbolLib Test Code\S60_3_2_200846_RnD_merlin_emulator_hw.rom.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File44\Platform_wk49\Symbols\mapfiles.zip" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;

                // Should be possible to activate a file within a zip
                SymbolCollection activatedCol = symView.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\AcCmOnItOr.dll", 0x70000000, 0x7A000000 ) );
                System.Diagnostics.Debug.Assert( activatedCol != null );

                // Verify that the symbols were really read.
                SymbolCollection col = view.Symbols.CollectionByAddress( 0x70000000 );
                System.Diagnostics.Debug.Assert( col != null );
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

                // Deactivate an activated dll
                deactivated = symView.Deactivate( new CodeSegDefinition( @"Z:\sys\bin\ACCMONITOR.DLL" ) );
                System.Diagnostics.Debug.Assert( deactivated == true );

                // symbol shouldn't be available anymore
                sym = symView.Lookup( 0x70000000, out col );
                System.Diagnostics.Debug.Assert( sym == null && col == null );
            }
        }

        private void TestMapRVCT()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File55\RVCT\alarmserver.exe.map" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;
                SymbolCollection col = null;

                // Should be possible to activate 
                col = symView.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\alarmserver.exe", 0x70000000, 0x7FFFFFFF ) );
                System.Diagnostics.Debug.Assert( col != null );
                System.Diagnostics.Debug.WriteLine( col.ToString( "full", null ) );

                // Check invalid address
                col = view.Symbols.CollectionByAddress( 0x700090a5 );
                System.Diagnostics.Debug.Assert( col == null );

                // Verify that the symbols were really read.
                col = view.Symbols.CollectionByAddress( 0x700090a4 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70000000 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70000001 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70002001 );
                System.Diagnostics.Debug.Assert( col != null );

                // Check for overlaps
                CheckNoOverlaps( col );

                // Perform some lookup tests
                string text = string.Empty;

                text = view.Symbols.PlainText[ 0x70000000 ];
                System.Diagnostics.Debug.Assert( text == "_E32Startup" );
                text = view.Symbols.PlainText[ 0x70000001 ];
                System.Diagnostics.Debug.Assert( text == "_E32Startup" );
                text = view.Symbols.PlainText[ 0x7000006f ];
                System.Diagnostics.Debug.Assert( text == "_E32Startup" );
                text = view.Symbols.PlainText[ 0x70000070 ];
                System.Diagnostics.Debug.Assert( text == "__cpp_initialize__aeabi_" );
                text = view.Symbols.PlainText[ 0x700090a4 ];
                System.Diagnostics.Debug.Assert( text == ".ARM.exidx$$Limit" );
            }
        }

        private void TestMapGCCE()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File55\GCCE\alarmserver.exe.map" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;
                SymbolCollection col = null;

                // Should be possible to activate 
                col = symView.ActivateAndGetCollection( new CodeSegDefinition( @"Z:\sys\bin\alarmserver.exe", 0x70000000, 0x7FFFFFFF ) );
                System.Diagnostics.Debug.Assert( col != null );
                System.Diagnostics.Debug.WriteLine( col.ToString( "full", null ) );

                // Check invalid address
                col = view.Symbols.CollectionByAddress( 0x7000bcc8 );
                System.Diagnostics.Debug.Assert( col == null );

                // Verify that the symbols were really read.
                col = view.Symbols.CollectionByAddress( 0x7000bcc7 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70000000 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70000001 );
                System.Diagnostics.Debug.Assert( col != null );
                col = view.Symbols.CollectionByAddress( 0x70002001 );
                System.Diagnostics.Debug.Assert( col != null );

                // Check for overlaps
                CheckNoOverlaps( col );

                // Perform some lookup tests
                string text = string.Empty;

                text = view.Symbols.PlainText[ 0x70000000 ];
                System.Diagnostics.Debug.Assert( text == "_xxxx_call_user_invariant" );
                text = view.Symbols.PlainText[ 0x70000001 ];
                System.Diagnostics.Debug.Assert( text == "_xxxx_call_user_invariant" );
                text = view.Symbols.PlainText[ 0x70000007 ];
                System.Diagnostics.Debug.Assert( text == "_xxxx_call_user_invariant" );
                text = view.Symbols.PlainText[ 0x70000008 ];
                System.Diagnostics.Debug.Assert( text == "_xxxx_call_user_handle_exception" );
                text = view.Symbols.PlainText[ 0x7000000f ];
                System.Diagnostics.Debug.Assert( text == "_xxxx_call_user_handle_exception" );
                text = view.Symbols.PlainText[ 0x70000070 ];
                System.Diagnostics.Debug.Assert( text == "CASSrvServer::CASSrvServer()" );
                text = view.Symbols.PlainText[ 0x7000bcc7 ];
                System.Diagnostics.Debug.Assert( text == "typeinfo name for CASAltRequestQuietPeriodEnd" );
            }
        }

        private void TestBigDsoData()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            CodeSegDefinitionCollection codeSegs = new CodeSegDefinitionCollection();
            using ( StringReader reader = new StringReader( KTestBigDsoDataCodeSegList ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    CodeSegDefinition def = CodeSegDefinitionParser.ParseDefinition( line );
                    if ( def != null )
                    {
                        codeSegs.Add( def );
                    }
                    line = reader.ReadLine();
                }
            }

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView", codeSegs ) )
            {
                // MemMan.dll contains a dodgy symbol:
                // 
                // 000031b4    0000    Image$$ER_RO$$Limit                       anon$$obj.o(linker$$defined$$symbols)
                // 003f8024    0004    __dso_handle                              ucppfini.o(.data)
                SymbolCollection colMemManDll = view.Symbols.CollectionByAddress( 0x79E18000 );
                System.Diagnostics.Debug.Assert( colMemManDll != null );

                // Verify it doesn't include the big dso object
                Symbol bigDsoData = FindByName( "__dso_handle", colMemManDll );
                System.Diagnostics.Debug.Assert( bigDsoData == null );

                // Widget engine would otherwise overlap with memman.dll
                SymbolCollection colWidgetEngineDll = view.Symbols.CollectionByAddress( 0x7A0C0000 );
                System.Diagnostics.Debug.Assert( colMemManDll != null );

                // Check no overlaps
                CheckNoOverlaps( colMemManDll, colWidgetEngineDll );

            }
        }

        private void TestHeapCellSymbolLookup()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rom.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs2.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs3.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            DoTestHeapCellSymbolLookup();
        }

        private void TestCodeSegmentResolutionROFS()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rom.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs2.symbol" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs3.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            DoTestCodeSegmentResolution();
        }

        private void TestCodeSegmentResolutionOBY()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs2.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs3.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rom.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            DoTestCodeSegmentResolution();
        }

        private void TestOBY()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rofs1.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs2.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\Variant03\RM505_widgetui_rheap_rnd.V03.rofs3.oby" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\Rom_images_widgetui_rheap\ivalo\CoreImage\RM505_widgetui_rheap_rnd_rom.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );
              
            DoTestHeapCellSymbolLookup();
        }

        private void TestBigMapFile()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\epoc32\release\armv5\urel\browserengine.dll.map" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\epoc32\release\armv5\urel\smart2go.exe.map" );
            iDebugEngine.Add( @"C:\Tool Demo Files\4. Heap Sample Data\11. Browser heap\epoc32\release\armv5\urel\avkon.dll.map" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );
        }

        private void TestUDACode()
        {
            Clear();
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File62\Ivalo_RM-505\Wk12\DebugMetaData\RM-505_52.50.2009.12_rnd.rofs1.symbol" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            CodeSegDefinition csOnC = new CodeSegDefinition( @"C:\sys\bin\btaccesshost.exe", 0x80ef3ae8, 0x80efa988 );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;
                SymbolCollection col = null;

                // This won't activate
                col = symView.ActivateAndGetCollection( csOnC );
                System.Diagnostics.Debug.Assert( col == null );
            }

            // Now merge in the zip file containing lots of maps...
            iDebugEngine.Add( @"C:\Tool Demo Files\2. Crash Data\File62\Ivalo_RM-505\Wk12\DebugMetaData\mapfiles.zip" );
            iDebugEngine.Prime( SymbianUtils.TSynchronicity.ESynchronous );

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView" ) )
            {
                DbgViewSymbols symView = view.Symbols;
                SymbolCollection col = null;

                // This should activate now
                col = symView.ActivateAndGetCollection( csOnC );
                System.Diagnostics.Debug.Assert( col != null );
            }
        }
        #endregion

        #region Constants
        private const string KTestBigDsoDataCodeSegList = @"CodeSegs - 807C4268-807D3844 z:\Sys\Bin\audiopolicyserver.dll
CodeSegs - 809194C8-8091B458 z:\Sys\Bin\eapdsp.dll
CodeSegs - 80698D48-806A4248 Z:\sys\bin\Cone.dll
CodeSegs - 78750000-78750B08 Z:\sys\bin\EikSrvc.dll
CodeSegs - 80742D38-80746288 Z:\sys\bin\gfxtrans.dll
CodeSegs - 78828000-78828D98 Z:\sys\bin\akntransitionutils.dll
CodeSegs - 78824000-78826F90 Z:\sys\bin\aknnotify.dll
CodeSegs - 78820000-78821164 Z:\sys\bin\akncapserverclient.dll
CodeSegs - 78834000-78838A18 Z:\sys\bin\cdlengine.dll
CodeSegs - 78830000-7883060C Z:\sys\bin\FontUtils.dll
CodeSegs - 7882C000-7882E204 Z:\sys\bin\FontProvider.dll
CodeSegs - 80D8C168-80D8DFB4 Z:\sys\bin\FepBase.dll
CodeSegs - 78894000-78894294 Z:\sys\bin\AknPictograph.dll
CodeSegs - 7894C000-7895713C Z:\sys\bin\cXmlParser.dll
CodeSegs - 78944000-7894820C Z:\sys\bin\XMLInterface.dll
CodeSegs - 78724000-78741DC0 Z:\sys\bin\imcm.dll
CodeSegs - 78710000-78720060 Z:\sys\bin\imut.dll
CodeSegs - 80624CB8-8062D428 Z:\sys\bin\MediaClientAudio.dll
CodeSegs - 78958000-78991F6C Z:\sys\bin\libopenvg.dll
CodeSegs - 78994000-789955CC Z:\sys\bin\libopenvgu.dll
CodeSegs - 78998000-78998570 Z:\sys\bin\libvgi.dll
CodeSegs - 788B0000-78941128 Z:\sys\bin\SVGEngine.dll
CodeSegs - 789BC000-789C0A24 Z:\sys\bin\COMMONENGINE.DLL
CodeSegs - 789C4000-789C7904 Z:\sys\bin\DrmRights.DLL
CodeSegs - 789D4000-789D41A8 Z:\sys\bin\DrmKeyStorage.dll
CodeSegs - 789D0000-789D2230 Z:\sys\bin\DrmCrypto.DLL
CodeSegs - 789C8000-789CE30C Z:\sys\bin\DrmServerInterfaces.DLL
CodeSegs - 789D8000-789DB13C Z:\sys\bin\DRMCOMMON.DLL
CodeSegs - 78A08000-78A09ED0 Z:\sys\bin\AknLayout2scalable.dll
CodeSegs - 78A04000-78A04A0C Z:\sys\bin\AknLayout2.dll
CodeSegs - 78A0C000-78A10908 Z:\sys\bin\lbs.dll
CodeSegs - 78A14000-78A147F0 Z:\sys\bin\aknlistloadertfx.dll
CodeSegs - 78A18000-78A19FDC Z:\sys\bin\touchfeedback.dll
CodeSegs - 789EC000-78A01C88 Z:\sys\bin\eikctl.dll
CodeSegs - 78A1C000-78A1E838 Z:\sys\bin\MediatorClient.dll
CodeSegs - 789DC000-789EBE08 Z:\sys\bin\eikdlg.dll
CodeSegs - 78A20000-78A21C50 Z:\sys\bin\DcfRep.dll
CodeSegs - 78A24000-78A28E80 Z:\sys\bin\servicehandler.dll
CodeSegs - 78A48000-78A4ECA8 Z:\sys\bin\cmmanagerdatabase.dll
CodeSegs - 78A2C000-78A46364 Z:\sys\bin\cmmanager.dll
CodeSegs - 789B0000-789B99E0 Z:\sys\bin\DRMHelper.dll
CodeSegs - 78A50000-78A517EC Z:\sys\bin\disknotifyhandler.dll
CodeSegs - 7899C000-789AD374 Z:\sys\bin\aknskinsrv.dll
CodeSegs - 788A4000-788AC154 Z:\sys\bin\AknIcon.dll
CodeSegs - 78A54000-78A6F26C Z:\sys\bin\aknskinrenderlib.dll
CodeSegs - 80C79038-80C79D68 Z:\sys\bin\HWRMLightClient.dll
CodeSegs - 78898000-788A3D30 Z:\sys\bin\aknskins.dll
CodeSegs - 78A70000-78A709EC Z:\sys\bin\jplangutil.dll
CodeSegs - 78A74000-78A75BF0 Z:\sys\bin\numbergrouping.dll
CodeSegs - 78A78000-78A7AD40 Z:\sys\bin\EikCoCtlLaf.dll
CodeSegs - 7883C000-788925B4 Z:\sys\bin\eikcoctl.dll
CodeSegs - 78A7C000-78A80FE8 Z:\sys\bin\phoneclient.dll
CodeSegs - 78A84000-78A86000 Z:\sys\bin\oommonitor.dll
CodeSegs - 78A88000-78A94CA0 Z:\sys\bin\ptiengine.dll
CodeSegs - 78758000-7881FF20 Z:\sys\bin\avkon.dll
CodeSegs - 78754000-787562F8 Z:\sys\bin\UikLaf.dll
CodeSegs - 78744000-7874EA34 Z:\sys\bin\EikCore.dll
CodeSegs - 78CAC000-78CAEF64 Z:\sys\bin\WEPSecuritySettingsUI.dll
CodeSegs - 78CB0000-78CB2BC4 Z:\sys\bin\WPASecuritySettingsUI.dll
CodeSegs - 78C98000-78CAA7B8 Z:\sys\bin\ApEngine.dll
CodeSegs - 78B2C000-78B2C800 Z:\sys\bin\directorylocalizer.dll
CodeSegs - 78B30000-78B30460 Z:\sys\bin\AknMemoryCardUi.DLL
CodeSegs - 78B38000-78B38B54 Z:\sys\bin\rsfwmountstore.dll
CodeSegs - 78B3C000-78B3C858 Z:\sys\bin\rsfwmountutils.dll
CodeSegs - 78B40000-78B405CC Z:\sys\bin\rsfwcontrol.dll
CodeSegs - 78B34000-78B35488 Z:\sys\bin\rsfwmountman.dll
CodeSegs - 78B20000-78B2A5AC Z:\sys\bin\commondialogs.dll
CodeSegs - 78B44000-78B4BBAC Z:\sys\bin\FavouritesEngine.dll
CodeSegs - 78AC4000-78AC5B18 Z:\sys\bin\mtur.dll
CodeSegs - 78B4C000-78B4FA68 Z:\sys\bin\Sendui.dll
CodeSegs - 78B70000-78B71718 Z:\sys\bin\mdccommon.dll
CodeSegs - 78B60000-78B6C498 Z:\sys\bin\mdeclient.dll
CodeSegs - 78B74000-78B74F30 Z:\sys\bin\HarvesterClient.dll
CodeSegs - 78B58000-78B5DE0C Z:\sys\bin\ContentListingFramework.dll
CodeSegs - 78B78000-78B7840C Z:\sys\bin\MediaCollectionManager.dll
CodeSegs - 78B54000-78B56DA4 Z:\sys\bin\MGXUtils.dll
CodeSegs - 78B50000-78B50BD0 Z:\sys\bin\MGXMediaFileApi.dll
CodeSegs - 78B7C000-78B7CE98 Z:\sys\bin\SWInstCli.dll
CodeSegs - 78B80000-78B80B94 Z:\sys\bin\aiwdialdata.dll
CodeSegs - 78B10000-78B1D3D8 Z:\sys\bin\commonui.dll
CodeSegs - 78D90000-78D945BC Z:\sys\bin\ConnectionUiUtilities.dll
CodeSegs - 79C34000-79C37138 Z:\sys\bin\ConnectionManager.dll
CodeSegs - 79528000-79528528 Z:\sys\bin\MGFetch.dll
CodeSegs - 796C4000-796CA388 Z:\sys\bin\BrowserDialogsProvider.dll
CodeSegs - 78F30000-78F30F20 Z:\sys\bin\WidgetRegistryClient.dll
CodeSegs - 795C0000-795C73DC Z:\sys\bin\SenXml.dll
CodeSegs - 795CC000-795CCF28 Z:\sys\bin\RTSecMgrUtil.dll
CodeSegs - 795C8000-795CA7D4 Z:\sys\bin\RTSecMgrClient.dll
CodeSegs - 795B4000-795BC558 Z:\sys\bin\Liwservicehandler.dll
CodeSegs - 79E18000-79E1B1B4 Z:\sys\bin\MemMan.dll
CodeSegs - 79E28000-79E80828 Z:\sys\bin\JavascriptCore.dll
CodeSegs - 7965C000-7965CD28 Z:\sys\bin\RECENTURLSTORE.DLL
CodeSegs - 79DC4000-79DF1518 Z:\sys\bin\WebKitUtils.dll
CodeSegs - 78CD0000-78CD1184 Z:\sys\bin\httpfiltercommon.dll
CodeSegs - 7A030000-7A036554 Z:\sys\bin\BrowserCache.dll
CodeSegs - 79C4C000-79C4CD7C Z:\sys\bin\WEBUTILS.dll
CodeSegs - 796A0000-796A28D8 Z:\sys\bin\PageScaler.dll
CodeSegs - 78CD4000-78CD6298 Z:\sys\bin\fotaengine.dll
CodeSegs - 78CC0000-78CCC408 Z:\sys\bin\HttpDMServEng.dll
CodeSegs - 78D08000-78D0ED30 Z:\sys\bin\DrmParsers.DLL
CodeSegs - 78D10000-78D10614 Z:\sys\bin\drmroapwbxmlparser.dll
CodeSegs - 78CF4000-78D06A74 Z:\sys\bin\RoapHandler.DLL
CodeSegs - 78CE4000-78CF1644 Z:\sys\bin\CodEng.dll
CodeSegs - 78D14000-78D16470 Z:\sys\bin\MultipartParser.dll
CodeSegs - 78CDC000-78CE2218 Z:\sys\bin\CodUi.dll
CodeSegs - 78CD8000-78CD8414 Z:\sys\bin\CodDownload.dll
CodeSegs - 78CB4000-78CBC878 Z:\sys\bin\DownloadMgr.dll
CodeSegs - 78F2C000-78F2C4FC Z:\sys\bin\aknnotifyplugin.dll
CodeSegs - 79478000-7948D470 Z:\sys\bin\CONNMON.DLL
CodeSegs - 79974000-7997E04C Z:\sys\bin\DownloadMgrUiLib.dll
CodeSegs - 79B90000-79BA06D8 Z:\sys\bin\backend.dll
CodeSegs - 79BA4000-79BB4D54 Z:\sys\bin\libm.dll
CodeSegs - 79B6C000-79B8DB8C Z:\sys\bin\libc.dll
CodeSegs - 79BB8000-79BBA31C Z:\sys\bin\libpthread.dll
CodeSegs - 79B2C000-79B2C480 Z:\sys\bin\ftokenclient.dll
CodeSegs - 79B30000-79B30334 Z:\sys\bin\aknlayout2hierarchy.dll
CodeSegs - 79B10000-79B2AEB0 Z:\sys\bin\alfclient.dll
CodeSegs - 79854000-79856620 Z:\sys\bin\rt_gesturehelper.dll
CodeSegs - 7A4CC000-7A722330 Z:\sys\bin\BrowserEngine.dll
CodeSegs - 793EC000-793F2E54 Z:\sys\bin\WidgetUi.exe
CodeSegs - 790A4000-7921DA68 Z:\sys\bin\10283389.dll
CodeSegs - 79220000-7922FFE0 Z:\sys\bin\10285D7B.dll
CodeSegs - 79264000-79264410 Z:\sys\bin\AknLayout2adaptation.dll
CodeSegs - 79244000-79263FD0 Z:\sys\bin\101fe2aa.dll
CodeSegs - 7926C000-7926EAC4 Z:\sys\bin\eikcdlg.dll
CodeSegs - 79268000-792685DC Z:\sys\bin\akninit.dll
CodeSegs - 79274000-79276BB0 Z:\sys\bin\102827CF.dll
CodeSegs - 79334000-79334514 Z:\sys\bin\aknfepuiinterface.dll
CodeSegs - 79338000-79338544 Z:\sys\bin\aknjapanesereading.dll
CodeSegs - 7933C000-7933E2BC Z:\sys\bin\peninputclient.dll
CodeSegs - 792F8000-79332F68 Z:\sys\bin\avkonfep.dll
CodeSegs - 79340000-793504F0 Z:\sys\bin\AknFepUiAvkonPlugin.dll
CodeSegs - 7935C000-79362600 Z:\sys\bin\101f84b9.dll
CodeSegs - 7937C000-7937CA78 Z:\sys\bin\PtiKeymappings_01.dll
CodeSegs - 79894000-7989738C Z:\sys\bin\httpfilterauthentication.dll
CodeSegs - 79390000-79395438 Z:\sys\bin\PtiZiCore.dll
CodeSegs - 80D43098-80D45818 Z:\sys\bin\httptransporthandler.dll
CodeSegs - 79C80000-79C80EDC Z:\sys\bin\uaproffilter.dll
CodeSegs - 79654000-796557E0 Z:\sys\bin\peninputimepluginitut.dll
CodeSegs - 80B9F5C8-80BA92C0 Z:\sys\bin\xmlparserplugin.dll
CodeSegs - 796AC000-796B08A4 Z:\sys\bin\cputils.dll
CodeSegs - 796A8000-796A9970 Z:\sys\bin\cpclient.dll
CodeSegs - 79E1C000-79E1C920 Z:\sys\bin\httpfilterIop.dll
CodeSegs - 7A0C0000-7A0C5E44 Z:\sys\bin\WidgetEngine.dll
CodeSegs - 7A158000-7A15DB88 Z:\sys\bin\jsdevice.dll
CodeSegs - 80D4B2A8-80D51F28 Z:\sys\bin\httpclient.dll
CodeSegs - 80D48928-80D4B224 Z:\sys\bin\HttpClientCodec.dll
CodeSegs - 80D45ED8-80D488AC Z:\sys\bin\tfcorefilters.dll
CodeSegs - 79C50000-79C51D08 Z:\sys\bin\HTTPFilterDRM.dll
CodeSegs - 79C54000-79C54FB4 Z:\sys\bin\httpfilterproxy.dll
CodeSegs - 79C5C000-79C5DB6C Z:\sys\bin\PnP.dll
CodeSegs - 79C58000-79C5A63C Z:\sys\bin\PnpPaosFilter.dll
CodeSegs - 78ED0000-78ED0CB4 Z:\sys\bin\wmdrmpkclient.dll
CodeSegs - 78ECC000-78ECD2D0 Z:\sys\bin\drmasf.dll
CodeSegs - 79C6C000-79C6CA94 Z:\sys\bin\wmdrmota.dll
CodeSegs - 79C68000-79C6A884 Z:\sys\bin\CameseUtility.dll
CodeSegs - 78DC0000-78DCA618 Z:\sys\bin\mpxcommon.dll
CodeSegs - 79724000-79728994 Z:\sys\bin\mpxcollectionutility.dll
CodeSegs - 79720000-79722344 Z:\sys\bin\mpxplaybackutility.dll
CodeSegs - 79B00000-79B0058C Z:\sys\bin\mpxviewplugin.dll
CodeSegs - 79AFC000-79AFDBF4 Z:\sys\bin\mpxviewutility.dll
CodeSegs - 79C64000-79C6704C Z:\sys\bin\cameseuicommon.dll
CodeSegs - 79C60000-79C6103C Z:\sys\bin\httpfiltercamese.dll
CodeSegs - 79C70000-79C70924 Z:\sys\bin\musicshophttpfilter.dll
CodeSegs - 79C74000-79C74DFC Z:\sys\bin\httpfilterconnhandler.dll
CodeSegs - 7A20C000-7A20C854 Z:\sys\bin\CookieFilter.dll
CodeSegs - 7A4BC000-7A4BD844 Z:\sys\bin\DeflateFilter.dll
CodeSegs - 80A799D8-80A829A4 Z:\sys\bin\pngcodec.dll
CodeSegs - 7A2EC000-7A300468 Z:\sys\bin\Zi8English.dll";
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            if ( System.Diagnostics.Debugger.IsAttached )
            {
                System.Diagnostics.Debug.WriteLine( aMessage );
            }
            else
            {
                System.Console.WriteLine( aMessage );
            }
            //
            CheckTrace( aMessage.ToUpper() );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string t = string.Format( aFormat, aParams );
            Trace( t );
        }
        #endregion

        #region Event handlers
        private void DebugEngine_EntityPrimingStarted( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            iTimePrimingStarted = DateTime.Now;
            Trace( "[Priming] Started : {0}, file: {1}", iTimePrimingStarted.ToString(), aEntity.FullName );
        }

        private void DebugEngine_EntityPrimingProgress( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            if ( aContext != null && aContext is int )
            {
                int prog = (int) aContext;
                if ( ( prog % 10 ) == 0 )
                {
                    DateTime time = DateTime.Now;
                    TimeSpan ts = time - iTimePrimingStarted;
                    int ms = (int) ts.TotalMilliseconds;
                    Trace( "[Priming] Progress: {0:d12}, file: {1}", ms, aEntity.FullName );
                }
            }
        }

        private void DebugEngine_EntityPrimingComplete( DbgEngine aEngine, DbgEntity aEntity, object aContext )
        {
            DateTime time = DateTime.Now;
            TimeSpan ts = time - iTimePrimingStarted;
            int ms = (int) ts.TotalMilliseconds;
            Trace( "[Priming] Complete: {0:d12}, file: {1}", ms, aEntity.FullName );
        }
        #endregion

        #region Internal methods
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

        private void CheckNoOverlaps( SymbolCollection aCollection1, SymbolCollection aCollection2 )
        {
            // Check collection1 against collection2
            int count1 = aCollection1.Count;
            for ( int i = 0; i < count1; i++ )
            {
                Symbol s = aCollection1[ i ];

                bool foundBase = aCollection2.Contains( s.Address );
                System.Diagnostics.Debug.Assert( foundBase == false );
                //
                bool foundLimit = aCollection2.Contains( s.AddressRange.Max );
                System.Diagnostics.Debug.Assert( foundLimit == false );
            }

            // Check collection2 against collection1
            int count2 = aCollection2.Count;
            for ( int i = 0; i < count2; i++ )
            {
                Symbol s = aCollection2[ i ];

                bool foundBase = aCollection1.Contains( s.Address );
                System.Diagnostics.Debug.Assert( foundBase == false );
                //
                bool foundLimit = aCollection1.Contains( s.AddressRange.Max );
                System.Diagnostics.Debug.Assert( foundLimit == false );
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

        private void CheckTrace( string aTrace )
        {
            bool overlap = aTrace.Contains( "OVERLAPS WITH EXISTING COLLECTION" );
            System.Diagnostics.Debug.Assert( overlap == false );
        }

        private void DoTestHeapCellSymbolLookup()
        {
            CodeSegDefinitionCollection codeSegs = new CodeSegDefinitionCollection();
            using ( StringReader reader = new StringReader( KTestBigDsoDataCodeSegList ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    CodeSegDefinition def = CodeSegDefinitionParser.ParseDefinition( line );
                    if ( def != null )
                    {
                        codeSegs.Add( def );
                    }
                    line = reader.ReadLine();
                }
            }
            codeSegs.SortByAddress();

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView", codeSegs ) )
            {
                foreach ( TSymLookupEntry entry in TSymLookupEntry.KHeapSymbols )
                {
                    SymbolCollection col = null;
                    Symbol sym = view.Symbols.Lookup( entry.iAddress, out col );
                    //
                    if ( sym != null )
                    {
                        string name = sym.Name;
                        System.Diagnostics.Debug.Assert( entry.iSymbol == name );
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert( entry.iSymbol == string.Empty );
                    }
                    //
                    if ( col != null )
                    {
                        string name = entry.iCollection.ToUpper();
                        bool match = col.FileName.Contains( name );
                        System.Diagnostics.Debug.Assert( match );
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert( entry.iCollection == string.Empty );
                    }
                    //
                    CodeSegDefinition def = codeSegs[ entry.iAddress ];
                    if ( def != null )
                    {
                        if ( entry.iSymbol == string.Empty )
                        {
                            // The original SymbolLib didn't find a symbolic match. It's okay
                            // if we did (or didn't) find a match using SymbianSymbolLib.
                        }
                        else if ( entry.iSymbol != string.Empty )
                        {
                            // SymbolLib found a match, SymbianSymbolLib must do too.
                            System.Diagnostics.Debug.Assert( sym != null );
                        }
                        if ( col == null )
                        {
                            // We didn't find a symbol collection for the specified address
                            // even though it falls within code segment range. Print a warning
                            // as this may be caused by dodgy symbol file content.
                            System.Diagnostics.Debug.WriteLine( string.Format( @"WARNING: couldn't find symbol for: 0x{0:x8}, offset: 0x{1:x8}, even though code seg match was found: {2}",
                                entry.iAddress,
                                entry.iAddress - def.Base,
                                def ) );
                        }
                    }
                }
            }
        }

        private void DoTestCodeSegmentResolution()
        {
            CodeSegDefinitionCollection codeSegs = new CodeSegDefinitionCollection();
            using ( StringReader reader = new StringReader( KTestBigDsoDataCodeSegList ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    CodeSegDefinition def = CodeSegDefinitionParser.ParseDefinition( line );
                    if ( def != null )
                    {
                        codeSegs.Add( def );
                    }
                    line = reader.ReadLine();
                }
            }
            codeSegs.SortByAddress();

            using ( DbgEngineView view = iDebugEngine.CreateView( "TestView", codeSegs ) )
            {
                foreach ( CodeSegDefinition def in codeSegs )
                {
                    SymbolCollection col = null;
                    Symbol sym = view.Symbols.Lookup( def.Base, out col );
                    System.Diagnostics.Debug.Assert( sym != null );
                }
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                iDebugEngine.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iDebugEngine;
        private DateTime iTimePrimingStarted;
        private AutoResetEvent iWaiter1 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter2 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter3 = new AutoResetEvent( false );
        private AutoResetEvent iWaiter4 = new AutoResetEvent( false );
        #endregion
    }
}
