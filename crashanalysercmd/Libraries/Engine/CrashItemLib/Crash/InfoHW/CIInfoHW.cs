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
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.InfoHW
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIInfoHW : CIElement, IEnumerable<CIVersionInfo>
    {
        #region Constructors
        public CIInfoHW( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        public void ClearVersions()
        {
            iVersions.Clear();
        }

        public void AddVersion( string aVersionText )
        {
            CIVersionInfo version = new CIVersionInfo( aVersionText );
            AddVersion( version );
        }

        public void AddVersion( CIVersionInfo aVersion )
        {
            if ( aVersion.IsValid )
            {
                iVersions.Add( aVersion );
            }
        }
        #endregion

        #region Properties
        public int VersionCount
        {
            get { return iVersions.Count; }
        }

        [CIDBAttributeCell( "Product Type", 1 )]
        public string ProductType
        {
            get { return iProductType; }
            set { iProductType = value; }
        }

        [CIDBAttributeCell( "Product Code", 0 )]
        public string ProductCode
        {
            get { return iProductCode; }
            set { iProductCode = value; }
        }

        [CIDBAttributeCell( "Serial Number", 2 )]
        public string SerialNumber
        {
            get { return iSerialNumber; }
            set { iSerialNumber = value; }
        }

        [CIDBAttributeCell("Production Mode", 3)]
        public int ProductionMode
        {
            get { return iPhoneMode; }
            set { iPhoneMode = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iProductType;
        }
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            base.PrepareRows();
            foreach ( CIVersionInfo ver in iVersions )
            {
                DataBindingModel.Add( ver );
            }
        }
        #endregion

        #region From IEnumerable<CIVersionInfo>
        public new IEnumerator<CIVersionInfo> GetEnumerator()
        {
            foreach ( CIVersionInfo v in iVersions )
            {
                yield return v;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIVersionInfo v in iVersions )
            {
                yield return v;
            }
        }
        #endregion

        #region Data members
        private string iProductType = string.Empty;
        private string iProductCode = string.Empty;
        private string iSerialNumber = string.Empty;
        private List<CIVersionInfo> iVersions = new List<CIVersionInfo>();
        private int iPhoneMode = -1; // production mode = 1, R&D mode = 0

        #endregion
    }
}
