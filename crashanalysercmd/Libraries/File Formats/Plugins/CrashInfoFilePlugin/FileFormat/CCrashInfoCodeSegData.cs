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
* The class CCrashInfoCodeSegItem is part of CrashAnalyser CrashInfoFile plugin.
* Stores information about one code segment, also known as crash time loaded dll.
* 
* 
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    internal class CCrashInfoCodeSegItem
    {
        #region Constructors
        public CCrashInfoCodeSegItem()
        {

        }
        public CCrashInfoCodeSegItem(uint aStart, uint aEnd, string aName)
        {
            iStart = aStart;
            iEnd = aEnd;
            iName = aName;
        }
        #endregion

        #region Properties
        public uint Start
        {
            get { return iStart; }
            set { iStart = value; }
        }

        public uint End
        {
            get { return iEnd; }
            set { iEnd = value; }
        }

        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        #endregion

        #region Data Members

        private uint iStart = 0;       
        private uint iEnd = 0;       
        private string iName = string.Empty;

      

        #endregion
    }
}
