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

namespace SymbolLib.CodeSegDef
{
	internal class CodeSegDefinitionCollectionCompareByFileName : IComparer<CodeSegDefinition>
	{
		#region IComparer Members
		public int Compare( object aLeft, object aRight )
		{
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
                CodeSegDefinition left = (CodeSegDefinition) aLeft;
                CodeSegDefinition right = (CodeSegDefinition) aRight;
                //
                ret = Compare( left, right );
            }
            //
            return ret;
		}
		#endregion

        #region IComparer<CodeSegDefinition> Members
        int IComparer<CodeSegDefinition>.Compare( CodeSegDefinition aLeft, CodeSegDefinition aRight )
        {
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
                ret = string.Compare( aLeft.ImageFileNameAndPath, aRight.ImageFileNameAndPath, true );
            }
            //
            return ret;
        }
        #endregion
    }

    internal class CodeSegDefinitionCollectionCompareByAddress : IComparer<CodeSegDefinition>
	{
        #region IComparer Members
        public int Compare( object aLeft, object aRight )
        {
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
                CodeSegDefinition left = (CodeSegDefinition) aLeft;
                CodeSegDefinition right = (CodeSegDefinition) aRight;
                //
                ret = Compare( left, right );
            }
            //
            return ret;
        }
        #endregion

        #region IComparer Members
        int IComparer<CodeSegDefinition>.Compare( CodeSegDefinition aLeft, CodeSegDefinition aRight )
        {
			int ret = -1;
            if ( aLeft.AddressStart == aRight.AddressStart && aLeft.AddressEnd == aRight.AddressEnd )
			{
				ret = 0;
			}
            else if ( aLeft.AddressEnd == aRight.AddressStart )
			{
                System.Diagnostics.Debug.Assert( aLeft.AddressStart < aRight.AddressStart );
                System.Diagnostics.Debug.Assert( aRight.AddressEnd >= aLeft.AddressEnd );
				//
				ret = -1;
			}
            else if ( aLeft.AddressStart == aRight.AddressEnd )
			{
                System.Diagnostics.Debug.Assert( aRight.AddressStart < aLeft.AddressStart );
                System.Diagnostics.Debug.Assert( aLeft.AddressEnd >= aRight.AddressEnd );
				//
				ret = 1;
			}
            else if ( aLeft.AddressStart > aRight.AddressEnd )
			{
                System.Diagnostics.Debug.Assert( aLeft.AddressEnd > aRight.AddressEnd );
                System.Diagnostics.Debug.Assert( aLeft.AddressEnd > aRight.AddressStart );
				ret = 1;
			}
            else if ( aLeft.AddressEnd < aRight.AddressStart )
			{
                System.Diagnostics.Debug.Assert( aLeft.AddressStart < aRight.AddressEnd );
                System.Diagnostics.Debug.Assert( aRight.AddressEnd > aLeft.AddressEnd );
				ret = -1;
			}
			//
			return ret;
		}
		#endregion
    }
}
