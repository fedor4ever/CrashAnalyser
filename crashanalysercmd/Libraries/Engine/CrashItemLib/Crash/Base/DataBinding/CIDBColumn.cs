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
using SymbianDebugLib.Engine;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Crash.Base.DataBinding
{
	public class CIDBColumn
	{
		#region Constructors
        public CIDBColumn()
        {
        }

        public CIDBColumn( string aCaption )
        {
            iCaption = aCaption;
 		}
		#endregion

        #region API
        #endregion

        #region Properties
        public string Caption
        {
            get { return iCaption; }
            set { iCaption = value; }
        }

        public int Width
        {
            get
            {
                int ret = iWidth;
                return ret; 
            }
            set { iWidth = value; }
        }

        public bool WidthSet
        {
            get { return iWidthSet; }
            set { iWidthSet = value; }
        }

        public bool TakesUpSlack
        {
            get { return iTakesUpSlack; }
            set { iTakesUpSlack = value; }
        }

        public CIElement Element
        {
            get { return Model.Element; }
        }

        internal CIDBModel Model
        {
            get { return iModel; }
            set { iModel = value; }
        }
        #endregion

        #region Constants
        public const int KDefaultWidth = 24;
        #endregion

        #region Data members
        private bool iTakesUpSlack = false;
        private int iWidth = KDefaultWidth;
        private bool iWidthSet = false;
        private string iCaption = string.Empty;
        private CIDBModel iModel = null;
		#endregion
	}
}
