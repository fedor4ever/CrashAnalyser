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
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianCodeLib.DbgEnginePlugin;
using SymbianCodeLib.SourceManagement.Source;
using SymbianInstructionLib.Arm.Library;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianCodeLib.SourceManagement.Provisioning
{
    public abstract class CodeSourceProvider : DisposableObject
    {
        #region Constructors
        protected CodeSourceProvider( CodeSourceProviderManager aManager )
        {
            iManager = aManager;
        }
        #endregion

        #region Framework API
        public virtual bool IsSupported( string aFileName )
        {
            SymFileTypeList fileTypes = FileTypes;
            string extension = Path.GetExtension( aFileName );
            //
            bool ret = fileTypes.IsSupported( extension );
            return ret;
        }

        public virtual CodeSourceCollection CreateSources( string aName )
        {
            throw new NotSupportedException( "Support not implemented by " + this.GetType().ToString() );
        }

        public abstract SymFileTypeList FileTypes
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public virtual string GetFileName( CodeSource aSource )
        {
            string ret = aSource.URI;
            return ret;
        }

        public virtual void PrepareToCreateSources( IEnumerable<string> aFileNames )
        {
        }
        #endregion

        #region API
        public IArmInstruction[] ConvertToInstructions( TArmInstructionSet aInstructionSet, uint[] aRawInstructions, uint aStartingAddress )
        {
            IArmInstruction[] ret = iManager.InstructionLibrary.ConvertToInstructions( aInstructionSet, aRawInstructions, aStartingAddress );
            return ret;
        }

        public bool SourceRemove( CodeSource aSource )
        {
            CodeSourceManager manager = iManager.Plugin.SourceManager;
            bool removed = manager.Remove( aSource );
            return removed;
        }

        public bool SourceAdd( CodeSource aSource )
        {
            CodeSourceManager manager = iManager.Plugin.SourceManager;
            bool added = manager.Add( aSource );
            return added;
        }
        #endregion

        #region Properties
        public ITracer Tracer
        {
            get { return iManager; }
        }

        public IPlatformIdAllocator IdAllocator
        {
            get { return iManager.IdAllocator; }
        }

        public ArmLibrary InstructionLibrary
        {
            get { return iManager.InstructionLibrary; }
        }

        public int SourceCount
        {
            get { return iManager.Plugin.SourceManager.Count; }
        }

        protected CodeSourceProviderManager ProvisioningManager
        {
            get { return iManager; }
        }
        #endregion

        #region Data members
        private readonly CodeSourceProviderManager iManager;
        #endregion
    }
}
