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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SymbianUtils.Range;
using EM = DExcPlugin.ExpressionManager.DExcExpressionManager;

namespace DExcPlugin.Extractor
{
	internal class DExcExtractorListThreadInfo : DExcExtractorList
    {
        #region Constructors
        public DExcExtractorListThreadInfo( DExcExtractor.TState aState, DExcExtractorListType aType )
            : base( aState, aType )
		{
		}
		#endregion

		#region API
        public override void Add( string aLine )
        {
            base.Add( aLine );
            //
            Match m = EM.ThreadStackRange.Match( aLine );
            if ( m.Success )
            {
                iStackRange.Min = uint.Parse( m.Groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber );
                iStackRange.Max = uint.Parse( m.Groups[ 2 ].Value, System.Globalization.NumberStyles.HexNumber );
            }
        }
		#endregion

		#region Properties
        public AddressRange StackRange
        {
            get { return iStackRange; }
        }
		#endregion

        #region Internal methods
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private AddressRange iStackRange = new AddressRange();
        #endregion
    }
}
