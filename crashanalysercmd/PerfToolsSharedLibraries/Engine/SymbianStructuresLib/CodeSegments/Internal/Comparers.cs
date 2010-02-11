/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
* All rights reserved.
* This component and the accompanying materials are made available
* under the terms of the License "Symbian Foundation License v1.0"
* which accompanies this distribution, and is available
* at the URL "http://www.symbianfoundation.org/legal/sfl-v10.html".
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
using System.Collections;
using System.Collections.Generic;

namespace SymbianStructuresLib.CodeSegments.Internal
{
	internal class CSDCompareByFileName : IComparer<CodeSegDefinition>
	{
		#region IComparer Members
		public int Compare( object aLeft, object aRight )
		{
			CodeSegDefinition left = (CodeSegDefinition) aLeft;
			CodeSegDefinition right = (CodeSegDefinition) aRight;
			//
            return Compare( left, right );
		}
		#endregion

        #region From IComparer<CodeSegDefinition>
        int IComparer<CodeSegDefinition>.Compare( CodeSegDefinition aLeft, CodeSegDefinition aRight )
        {
            return string.Compare( aLeft.FileName, aRight.FileName, StringComparison.CurrentCultureIgnoreCase );
        }
        #endregion
    }

    internal class CSDCompareByAddress : IComparer<CodeSegDefinition>
	{
        #region IComparer Members
        public int Compare( object aLeft, object aRight )
        {
            CodeSegDefinition left = (CodeSegDefinition) aLeft;
            CodeSegDefinition right = (CodeSegDefinition) aRight;
            //
            return Compare( left, right );
        }
        #endregion

        #region IComparer Members
        int IComparer<CodeSegDefinition>.Compare( CodeSegDefinition aLeft, CodeSegDefinition aRight )
        {
			int ret = -1;
            if ( aLeft.Base == aRight.Base && aLeft.Limit == aRight.Limit )
			{
				ret = 0;
			}
            else if ( aLeft.Limit == aRight.Base )
			{
                System.Diagnostics.Debug.Assert( aLeft.Base < aRight.Base );
                System.Diagnostics.Debug.Assert( aRight.Limit >= aLeft.Limit );
				//
				ret = -1;
			}
            else if ( aLeft.Base == aRight.Limit )
			{
                System.Diagnostics.Debug.Assert( aRight.Base < aLeft.Base );
                System.Diagnostics.Debug.Assert( aLeft.Limit >= aRight.Limit );
				//
				ret = 1;
			}
            else if ( aLeft.Base > aRight.Limit )
			{
                System.Diagnostics.Debug.Assert( aLeft.Limit > aRight.Limit );
                System.Diagnostics.Debug.Assert( aLeft.Limit > aRight.Base );
				ret = 1;
			}
            else if ( aLeft.Limit < aRight.Base )
			{
                System.Diagnostics.Debug.Assert( aLeft.Base < aRight.Limit );
                System.Diagnostics.Debug.Assert( aRight.Limit > aLeft.Limit );
				ret = -1;
			}
			//
			return ret;
		}
		#endregion
    }
}
