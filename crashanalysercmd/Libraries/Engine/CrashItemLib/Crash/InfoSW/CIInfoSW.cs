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

namespace CrashItemLib.Crash.InfoSW
{
    [CIDBAttributeColumn( "Name", 0, 100 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIInfoSW : CIElement, IEnumerable<CIVersionInfo>
    {
        #region Constructors
        public CIInfoSW( CIContainer aContainer )
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

        public void AddVersion( string aName, string aValue )
        {
            CIVersionInfo version = new CIVersionInfo( aName, aValue );
            AddVersion( version );
        }

        internal void AddVersion( CIVersionInfo aVersion )
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

        [CIDBAttributeCell( "(XIP-ROM) Image Checksum", 3, "x8", 0u )]
        public uint ImageCheckSum
        {
            get { return iImageCheckSum; }
            set { iImageCheckSum = value; }
        }

        [CIDBAttributeCell( "Image Timestamp", 2 )]
        public DateTime ImageTimeStamp
        {
            get { return iImageTimeStamp; }
            set { iImageTimeStamp = value; }
        }

        [CIDBAttributeCell( "Platform", 0, "" )]
        public string Platform
        {
            get { return iPlatform; }
            set { iPlatform = value; }
        }

        [CIDBAttributeCell( "Language", 1, "" )]
        public string Language
        {
            get { return iLanguage; }
            set { iLanguage = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            base.PrepareRows();

            // Need to add the version information
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
        private uint iImageCheckSum = 0;
        private DateTime iImageTimeStamp = new DateTime();
        private string iPlatform = string.Empty;
        private string iLanguage = string.Empty;
        private List<CIVersionInfo> iVersions = new List<CIVersionInfo>();
        #endregion
    }
}
