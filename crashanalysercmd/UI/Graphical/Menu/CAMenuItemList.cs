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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using SymbianUtils.Settings;

namespace CrashAnalyser.Menu
{
	internal class CAMenuItemList
	{
		#region Constructors
        public CAMenuItemList( CAMenuManager aManager )
		{
            iManager = aManager;
		}
		#endregion

		#region API
        public void Add( CAMenuItem aItem )
        {
            iItems.Add( aItem );
        }

        public void Activate()
        {
            foreach ( CAMenuItem item in iItems )
            {
                item.Activate();
            }
        }

        public void Deactivate()
        {
            foreach ( CAMenuItem item in iItems )
            {
                item.Deactivate();
            }
        }
        #endregion

		#region Properties
		#endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CAMenuManager iManager;
        private List<CAMenuItem> iItems = new List<CAMenuItem>();
		#endregion
    }
}
