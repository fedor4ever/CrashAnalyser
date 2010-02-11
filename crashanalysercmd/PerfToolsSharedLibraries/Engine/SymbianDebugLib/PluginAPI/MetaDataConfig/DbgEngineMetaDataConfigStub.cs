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
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;

namespace SymbianDebugLib.PluginAPI.Types.MetaDataConfig
{
    internal class DbgEngineMetaDataConfigStub : DbgEngineMetaDataConfig
    {
        #region Constructors
        public DbgEngineMetaDataConfigStub( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region From DbgCodeEngine
        public override bool IsReady
        {
            get { return false; }
        }
        
        public override string Name
        {
            get { return "Meta-Data Config Engine Stub"; }
        }

        public override bool IsSupported( string aFileName, out string aType )
        {
            aType = string.Empty;
            return false;
        }

        public override DbgPluginPrimer CreatePrimer()
        {
            throw new NotImplementedException();
        }

        protected override DbgPluginView DoCreateView( string aName )
        {
            return null;
        }

        protected override void DoClear()
        {
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
