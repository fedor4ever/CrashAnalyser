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
using System.IO;
using System.Text;

namespace SymbianUtils.Environment
{
	public class EnvLocator
	{
		#region Constructors
		public EnvLocator()
		{
			LocateEnvironmentDrives();
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iEnvironments.Count; }
		}

        public EnvEntryBase this[ int aIndex ]
		{
			get { return iEnvironments[ aIndex ]; }
		}
		#endregion

		#region Internal methods
		private void LocateEnvironmentDrives()
		{
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach ( DriveInfo drive in drives )
            {
                if ( drive.DriveType == DriveType.Fixed )
                {
                    EnvEntryBase environment = EnvEntryBase.New( drive );
                    bool valid = environment.IsValid;
                    if ( valid )
                    {
                        iEnvironments.Add( environment );
                    }
                }
            }
		}
		#endregion

		#region Internal constants
		private const int Removable = 2; 
		private const int LocalDisk = 3; 
		private const int Network = 4; 
		private const int CD = 5; 
		private const int RAMDrive = 6;
		#endregion

		#region Data members
        private List<EnvEntryBase> iEnvironments = new List<EnvEntryBase>( 3 );
		#endregion
	}
}
