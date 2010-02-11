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
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Reports
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIReportInfo : CIElement, IEnumerable<CIReportParameter>
    {
        #region Constructors
        public CIReportInfo( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        public void AddParameter( string aName, uint aValue )
        {
            CIReportParameter param = new CIReportParameter( this, aName, aValue );
            iParameters.Add( param );
        }
        #endregion

        #region Properties
        /// <summary>
        /// Type of crash report
        /// </summary>
        [CIDBAttributeCell( "Type", 1, "", "" )]
        public string Type
        {
            get { return iType; }
            set { iType = value; }
        }

        /// <summary>
        /// Reporter name
        /// </summary>
        [CIDBAttributeCell( "Name", 2, "", "" )]
        public override string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        [CIDBAttributeCell( "Category", 3, "", "" )]
        public string Category
        {
            get { return iCategory; }
            set { iCategory = value; }
        }

        [CIDBAttributeCell( "Success Count", 4, "", "" )]
        public uint CountSuccess
        {
            get { return iCountSuccess; }
            set { iCountSuccess = value; }
        }

        [CIDBAttributeCell( "Fail Count", 5, "", "" )]
        public uint CountFail
        {
            get { return iCountFail; }
            set { iCountFail = value; }
        }

        /// <summary>
        /// Misc. reporter comments
        /// </summary>
        [CIDBAttributeCell( "Comments", 6, "", "" )]
        public string Comments
        {
            get { return iComments; }
            set { iComments = value; }
        }

        public int ParameterCount
        {
            get { return iParameters.Count; }
        }
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            base.PrepareRows();

            // Need to add the version information
            foreach ( CIReportParameter rp in iParameters )
            {
                DataBindingModel.Add( rp );
            }
        }
        #endregion

        #region From IEnumerable<CIReportParameter>
        public new IEnumerator<CIReportParameter> GetEnumerator()
        {
            foreach ( CIReportParameter rp in iParameters )
            {
                yield return rp;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIReportParameter rp in iParameters )
            {
                yield return rp;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private string iType = string.Empty;
        private string iName = string.Empty;
        private string iCategory = string.Empty;
        private string iComments = string.Empty;
        private uint iCountSuccess = 0;
        private uint iCountFail = 0;
        private List<CIReportParameter> iParameters = new List<CIReportParameter>();
        #endregion
    }
}
