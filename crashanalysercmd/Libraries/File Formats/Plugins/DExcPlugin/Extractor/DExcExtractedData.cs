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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using EM=DExcPlugin.ExpressionManager.DExcExpressionManager;

namespace DExcPlugin.Extractor
{
	internal class DExcExtractedData
	{
        #region Constructors
        public DExcExtractedData()
		{
        }
		#endregion

        #region API
        public void Add( DExcExtractorList aList )
        {
            iLists.Add( aList.Type, aList );
        }
        #endregion

        #region Properties
        public long LineNumber
        {
            get { return iLineNumber; }
            set { iLineNumber = value; }
        }

        public DExcExtractorList this[ DExcExtractorListType aType ]
        {
            get
            {
                DExcExtractorList ret = null;
                //
                if ( iLists.ContainsKey( aType ) )
                {
                    ret = iLists[ aType ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach ( KeyValuePair<DExcExtractorListType, DExcExtractorList> kvp in iLists )
            {
                DExcExtractorList list = kvp.Value;
                string lines = list.ToString();
                if ( lines.Length > 0 )
                {
                    ret.AppendLine( lines );
                }
            }
            //
            return ret.ToString();
        }
        #endregion

		#region Data members
        private long iLineNumber = 0;
        private Dictionary<DExcExtractorListType, DExcExtractorList> iLists = new Dictionary<DExcExtractorListType, DExcExtractorList>();
		#endregion
	}
}
