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
using System.Xml;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace SymbianUtils.Settings
{
    public interface IXmlSettingsSimple
	{
        string XmlSettingsPersistableName { get; }

        string XmlSettingPersistableValue { get; set; }
    }

    public interface IXmlSettingsExtended
    {
        void XmlSettingsSave( XmlSettings aSettings, string aCategory );

        void XmlSettingsLoad( XmlSettings aSettings, string aCategory );
    }
}


