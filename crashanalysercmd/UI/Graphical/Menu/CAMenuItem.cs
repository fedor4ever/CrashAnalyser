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
using CrashAnalyserEngine.Interfaces;
using SymbianUtils.Settings;

namespace CrashAnalyser.Menu
{
	internal class CAMenuItem
	{
		#region Constructors
        public CAMenuItem( CAMenuManager aManager, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag )
		{
            iManager = aManager;
            iClickHandler = aClickHandler;
            iTag = aTag;
            //
            iItem = new ToolStripMenuItem( aCaption );
            iItem.Click += new EventHandler( Item_Click );
		}
		#endregion

		#region API
        public void Activate()
        {
            iItem.Visible = true;
        }

        public void Deactivate()
        {
            iItem.Visible = false;
        }
        #endregion

		#region Properties
		#endregion

        #region Operators
        public static implicit operator ToolStripMenuItem( CAMenuItem aItem )
        {
            return aItem.iItem;
        }
        #endregion

        #region Event handlers
        private void Item_Click( object sender, EventArgs e )
        {
            if ( iClickHandler != null )
            {
                iClickHandler( iTag, iItem.Text );
            }
        }
        #endregion

        #region Data members
        private readonly CAMenuManager iManager;
        private readonly object iTag;
        private readonly ToolStripMenuItem iItem;
        private readonly UIMenuItemClickHandler iClickHandler;
		#endregion
    }
}
