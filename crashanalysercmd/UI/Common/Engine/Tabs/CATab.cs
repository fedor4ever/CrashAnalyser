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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CrashAnalyserEngine.Tabs
{
    public partial class CATab : UserControl
    {
        #region Constructors
        protected CATab()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        [Category( "Tab" )]
        public string Title
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
        #endregion

        #region Framework methods
        protected virtual void RegisterMenuItems()
        {
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private string iTitle = string.Empty;
        #endregion
    }
}
