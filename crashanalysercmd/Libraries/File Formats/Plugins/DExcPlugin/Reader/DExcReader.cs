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
using System.IO;
using System.Collections.Generic;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Primer;
using SymbianUtils;
using DExcPlugin.Extractor;
using DExcPlugin.Descriptor;
using DExcPlugin.Plugin;
using DExcPlugin.Transformer;

namespace DExcPlugin.Reader
{
    internal class DExcReader : CFFReader
	{
		#region Constructors
        public DExcReader( DExcPluginImp aEngine, DExcDescriptor aDescriptor )
		:   base( aEngine, aDescriptor )
		{
            iTraceExtractor.StateChanged += new DExcExtractor.EventHandler( TraceExtractor_StateChanged );
        }
        #endregion

        #region From CFFReader
        public override void NativeReadInit()
        {
            throw new NotSupportedException();
        }

        public override void TraceReadInit()
        {
            // Check if there is an stk file with the same name
            string stackFile = DExcDescriptor.StackFileName;
            if ( File.Exists( stackFile ) )
            {
                iTraceExtractor.Init( stackFile );
            }
            else
            {
                iTraceExtractor.Init();
            }
        }

        public override bool TraceReadOffer( CFFTraceLine aLine )
        {
            bool consumed = iTraceExtractor.Offer( aLine, aLine.LineNumber );
            return consumed;
        }

        public override void TraceReadComplete()
        {
            // This flushes any pending raw data items in the interpreter.
            iTraceExtractor.Finialize();
            
            base.NotifyEvent( TEvent.EReadingComplete );
        }
        #endregion

        #region Properties
        public DExcPluginImp DExcEngine
        {
            get { return base.Plugin as DExcPluginImp; }
        }

        public DExcDescriptor DExcDescriptor
        {
            get { return base.Descriptor as DExcDescriptor; }
        }
        #endregion

        #region Observer - Trace interpreter
        private void TraceExtractor_StateChanged( DExcExtractor.TEvent aEvent, DExcExtractor aExtractor )
        {
            // This event is notified when the extractor has obtained one entire D_EXC crash
            if ( aEvent == DExcExtractor.TEvent.EEventExtractedAllData )
            {
                DExcExtractedData data = aExtractor.CurrentData;
                DExcTransformer transformer = new DExcTransformer( DExcDescriptor, base.Plugin.DataProvider, data );

                // Transform into crash container
                CIContainer container = transformer.Transform();
                if ( container != null )
                {
                    base.NotifyEvent( TEvent.EReadingContainerCreated, container );
                }

                // Get extractor ready for next file
                iTraceExtractor.Init();
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private DExcExtractor iTraceExtractor = new DExcExtractor();
        #endregion
    }
}
