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
using System.ComponentModel;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Memory
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1 )]
    public class CIMemoryInfo : CIElement
    {
        #region Type
        public enum TType
        {
            [Description( "Drive" )]
            ETypeDrive = 0,

            [Description( "RAM" )]
            ETypeRAM
        }
        #endregion

        #region Constructors
        public CIMemoryInfo( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public int DriveNumber
        {
            get { return iDriveNumber; }
            set { iDriveNumber = value; }
        }

        public string DriveLetter
        {
            get 
            { 
                int driveLetterCharNumber = ( (int) 'A' ) + DriveNumber;
                char driveLetter = (char) driveLetterCharNumber;
                //
                return string.Format( "{0}:", driveLetter );
            }
        }

        public ulong Capacity
        {
            get { return iCapacity; }
            set { iCapacity = value; }
        }

        public ulong Free
        {
            get { return iFree; }
            set { iFree = value; }
        }

        public ulong UID
        {
            get { return iUID; }
            set { iUID = value; }
        }

        public string VolumeName
        {
            get { return iVolumeName; }
            set { iVolumeName = value; }
        }

        public string Vendor
        {
            get { return iVendor; }
            set { iVendor = value; }
        }

        public TType Type
        {
            get { return iType; }
            set { iType = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = SymbianUtils.Enum.EnumUtils.ToString( Type );
            if ( Type == TType.ETypeDrive )
            {
                ret = DriveLetter;
            }
            return ret;
        }
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();

            DataBindingModel.Add( new CIDBRow( new CIDBCell( "Type" ), new CIDBCell( SymbianUtils.Enum.EnumUtils.ToString( this.Type ) ) ) );
            
            if ( Type == TType.ETypeDrive )
            {
                DataBindingModel.Add( new CIDBRow( new CIDBCell( "Drive Letter" ), new CIDBCell( DriveLetter ) ) );
                if ( VolumeName.Length > 0 )
                {
                    DataBindingModel.Add( new CIDBRow( new CIDBCell( "Volume" ), new CIDBCell( VolumeName ) ) );
                }
                if ( Vendor.Length > 0 )
                {
                    DataBindingModel.Add( new CIDBRow( new CIDBCell( "Vendor" ), new CIDBCell( Vendor ) ) );
                }
                if ( UID != 0 )
                {
                    DataBindingModel.Add( new CIDBRow( new CIDBCell( "UID" ), new CIDBCell( UID.ToString() ) ) );
                }
            }

            DataBindingModel.Add( new CIDBRow( new CIDBCell( "Free" ), new CIDBCell( Free.ToString() ) ) );
            if ( Capacity != 0 )
            {
                DataBindingModel.Add( new CIDBRow( new CIDBCell( "Capacity" ), new CIDBCell( Capacity.ToString() ) ) );
            }
        }
        #endregion

        #region Data members
        private int iDriveNumber = 0;
        private ulong iCapacity = 0;
        private ulong iFree = 0;
        private ulong iUID = 0;
        private string iVolumeName = string.Empty;
        private string iVendor = string.Empty;
        private TType iType = TType.ETypeDrive;
        #endregion
    }
}
