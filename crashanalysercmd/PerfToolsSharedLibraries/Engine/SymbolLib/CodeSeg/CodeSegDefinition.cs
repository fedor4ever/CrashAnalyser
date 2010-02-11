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
using System.Text;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;

namespace SymbolLib.CodeSegDef
{
    public class CodeSegDefinition : CodeSegResolverEntry
	{
        #region Enumerations
        [Flags]
        public enum TAttributes
        {
            EAttributeNone = 0,
            EAttributeXIP = 1,
            EAttributeRAM = 2,
        }
        #endregion

        #region Constructors & destructor
        public CodeSegDefinition()
            : this( string.Empty )
		{
		}

		public CodeSegDefinition( string aFileName )
            : this( aFileName, string.Empty )
		{
		}

		public CodeSegDefinition( string aEnvFileName, string aImageFileName )
            : base( aEnvFileName, aImageFileName )
        {
		}
		#endregion

		#region Constants
		public const string KMapFileExtension = ".map";
        public const string KSysBinPath = @"\sys\bin\";
        public const string KROMBinaryPath = "z:" + KSysBinPath;
        #endregion

        #region API
        #endregion

        #region Properties
        public string MapFileName
		{
			get
			{
                string ret = string.Empty;
                //
                if ( !String.IsNullOrEmpty( EnvironmentFileNameAndPath ) )
                {
                    ret = EnvironmentFileNameAndPath + KMapFileExtension;
                }
                //
				return ret;
			}
		}

		public bool MapFileExists
		{
			get
			{
				bool ret = false;
				//
				try
				{
					string fileName = MapFileName;
					ret = File.Exists( fileName );
				}
				finally
				{
				}
				//
				return ret;
			}
		}

        public bool AddressValid
        {
            get
            {
                bool good = iRange.IsValid;
                return good;
            }
        }

		public uint AddressStart
		{
			get { return (uint) iRange.Min; }
			set
            { 
                iRange.UpdateMin( value );
                UpdateAttributes();
            }
		}

        public uint AddressEnd
		{
            get { return (uint) iRange.Max; }
            set 
            { 
                iRange.UpdateMax( value );
                UpdateAttributes();
            }
        }

        public uint Checksum
        {
            get { return iChecksum; }
            set { iChecksum = value; }
        }

        public AddressRange AddressRange
        {
            get { return iRange; }
        }

        public TAttributes Attributes
        {
            get { return iAttributes; }
        }
		#endregion

        #region From System.Object
		public override string ToString()
		{
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( iRange.ToString() );
            ret.Append( " " );
            ret.Append( base.ToString() );
            //
            return ret.ToString();
		}
        #endregion

        #region Internal methods
        private void UpdateAttributes()
        {
            // Remove XIP/RAM attrib before we work out the type...
            iAttributes &= ~TAttributes.EAttributeRAM;
            iAttributes &= ~TAttributes.EAttributeXIP;

            MemoryModel.TMemoryModelType type = MemoryModel.TypeByAddress( AddressStart );
            if ( type != MemoryModel.TMemoryModelType.EMemoryModelUnknown )
            {
                MemoryModel.TMemoryModelRegion region = MemoryModel.RegionByAddress( AddressStart, type );
                //
                if ( region == MemoryModel.TMemoryModelRegion.EMemoryModelRegionROM )
                {
                    iAttributes |= TAttributes.EAttributeXIP;
                }
                else if ( region == MemoryModel.TMemoryModelRegion.EMemoryModelRegionRAMLoadedCode )
                {
                    iAttributes |= TAttributes.EAttributeRAM;
                }
            }
        }
		#endregion

		#region Data members
        private readonly AddressRange iRange = new AddressRange();
        private uint iChecksum = 0;
        private TAttributes iAttributes = TAttributes.EAttributeNone;
		#endregion
	}
}
