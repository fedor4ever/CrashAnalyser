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
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Visualization.Bits;
using CrashItemLib.Crash.Registers.Visualization.Utilities;

namespace CrashItemLib.Crash.Registers.Visualization
{
    public class CIRegisterVisualization : CIElement
	{
		#region Constructors
        public CIRegisterVisualization( CIRegister aRegister, ICIRegisterVisualizerVisitor aVisitor, string aDescription )
            : base( aRegister.Container, aRegister )
        {
            iVisitor = aVisitor;
            iRegister = aRegister;
            iDescription = aDescription;
        }
		#endregion

        #region API
        public void Refresh()
        {
            iVisitor.Build( this );
        }
        #endregion

        #region Properties
        public override string Name
        {
            get { return Description; }
            set { }
        }

        public string Description
        {
            get { return iDescription; }
        }

        public string Binary
        {
            get { return VisUtilities.ToBinary( Value ); }
        }

        public CIRegister Register
        {
            get { return iRegister; }
        }

        public int Size
        {
            get { return KRegisterSizeInBits; }
        }

        public uint Value
        {
            get { return Register.Value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const int KRegisterSizeInBits = 32;
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iRegister.ToString();
        }
        #endregion

        #region Data members
        private readonly CIRegister iRegister;
        private readonly ICIRegisterVisualizerVisitor iVisitor;
        private readonly string iDescription;
        #endregion
    }
}
