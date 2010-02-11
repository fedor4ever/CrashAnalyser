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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SymbianStructuresLib.Security
{
    public enum TCapability
    {
        [Description( "TCB" )]
        ECapabilityTCB = 0,

        [Description( "CommDD" )]
        ECapabilityCommDD = 1,

        [Description( "PowerMgmt" )]
        ECapabilityPowerMgmt = 2,

        [Description( "MultimediaDD" )]
        ECapabilityMultimediaDD = 3,

        [Description( "ReadDeviceData" )]
        ECapabilityReadDeviceData = 4,

        [Description( "WriteDeviceData" )]
        ECapabilityWriteDeviceData = 5,

        [Description( "DRM" )]
        ECapabilityDRM = 6,

        [Description( "TrustedUI" )]
        ECapabilityTrustedUI = 7,

        [Description( "ProtServ" )]
        ECapabilityProtServ = 8,

        [Description( "DiskAdmin" )]
        ECapabilityDiskAdmin = 9,

        [Description( "NetworkControl" )]
        ECapabilityNetworkControl = 10,

        [Description( "AllFiles" )]
        ECapabilityAllFiles = 11,

        [Description( "SwEvent" )]
        ECapabilitySwEvent = 12,

        [Description( "NetworkServices" )]
        ECapabilityNetworkServices = 13,

        [Description( "LocalServices" )]
        ECapabilityLocalServices = 14,

        [Description( "ReadUserData" )]
        ECapabilityReadUserData = 15,

        [Description( "WriteUserData" )]
        ECapabilityWriteUserData = 16,

        [Description( "Location" )]
        ECapabilityLocation = 17,

        [Description( "SurroundingsDD" )]
        ECapabilitySurroundingsDD = 18,

        [Description( "UserEnvironment" )]
        ECapabilityUserEnvironment = 19,

        [Description( "" )]
        ECapability_Limit,

        [Description( "" )]
        ECapability_HardLimit = 255,

        [Description( "" )]
        ECapability_None = -1,	            // Special value used to specify 'do not care' or 'no capability'

        [Description( "" )]
        ECapability_Denied = -2	            // Special value used to indicate a capability that is never granted
    }
}
