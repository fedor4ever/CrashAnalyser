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
using System.IO;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianCodeLib.QueryAPI;
using SymbianCodeLib.Relocator;
using SymbianCodeLib.DbgEnginePlugin;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;

namespace SymbianCodeLib.DbgEnginePlugin
{
    public class CodeView : DbgViewCode
    {
        #region Constructors
        internal CodeView( string aToken, CodePlugin aPlugin )
            : base( aToken, aPlugin )
		{
            iRelocator = new CodeRelocator( aPlugin );
            iQueryAPI = new CodeQueryAPI( iRelocator );
        }
		#endregion

        #region From DbgPluginView
        public override bool Contains( uint aAddress )
        {
            bool ret = iQueryAPI.Contains( aAddress );
            return ret;
        }

        public override bool Activate( CodeSegDefinition aCodeSegment )
        {
            bool activated = ActivateAndGetCollection( aCodeSegment ) != null;
            return activated;
        }

        public override bool Deactivate( CodeSegDefinition aCodeSegment )
        {
            return iRelocator.Deactivate( aCodeSegment );
        }

        public override bool IsReady
        {
            get
            {
                // For a view to be ready we must have at least one
                // activated, i.e. 'ready' symbol source.
                int count = 0;

                // Check with dynamic activations
                foreach ( CodeSourceAndCollection pair in iRelocator )
                {
                    if ( pair.Source.TimeToRead == CodeSource.TTimeToRead.EReadWhenPriming )
                    {
                        ++count;
                        break; // No need to count anymore
                    }
                }

                // Try to find any fixed activation entries
                if ( count == 0 )
                {
                    CodeSourceManager allSources = this.SourceManager;
                    foreach ( CodeSource source in allSources )
                    {
                        count += source.CountActivated;
                        if ( count > 0 )
                        {
                            break;
                        }
                    }
                }

                return ( count > 0 );
            }
        }
        #endregion

        #region From DbgViewCode
        public override CodeCollection ActivateAndGetCollection( CodeSegDefinition aCodeSegment )
        {
            CodeCollection ret = iRelocator.Activate( aCodeSegment );
            return ret;
        }

        protected override bool DoGetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            bool ret = iQueryAPI.GetInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
            return ret;
        }

        public override IArmInstruction ConvertToInstruction( uint aAddress, TArmInstructionSet aInstructionSet, uint aRawValue )
        {
            CodePlugin plugin = this.Plugin;
            IArmInstruction[] ret = plugin.ProvisioningManager.InstructionLibrary.ConvertToInstructions( aInstructionSet, new uint[ 1 ] { aRawValue }, aAddress );
            System.Diagnostics.Debug.Assert( ret != null && ret.Length == 1 );
            return ret[ 0 ];
        }
        #endregion

        #region API
        #endregion

		#region Properties
        internal CodeSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }
        #endregion

        #region Internal methods
        internal CodePlugin Plugin
        {
            get { return base.Engine as CodePlugin; }
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
                iRelocator.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly CodeQueryAPI iQueryAPI;
        private readonly CodeRelocator iRelocator;
        #endregion
    }
}
