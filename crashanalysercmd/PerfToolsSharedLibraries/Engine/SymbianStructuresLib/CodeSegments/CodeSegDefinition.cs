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
using SymbianStructuresLib.MemoryModel;

namespace SymbianStructuresLib.CodeSegments
{
    public class CodeSegDefinition : AddressRange
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

        #region Constructors
		public CodeSegDefinition()
            : this( string.Empty )
		{
		}

		public CodeSegDefinition( string aFileName )
            : this( aFileName, 0, 0 )
        {
		}

        public CodeSegDefinition( string aFileName, uint aBase, uint aLimit )
            : base( aBase, aLimit )
        {
            iFileName = aFileName;
        }
        #endregion

        #region Properties
        public uint Checksum
        {
            get { return iChecksum; }
            set { iChecksum = value; }
        }

        public string FileName
        {
            get { return iFileName; }
            set { iFileName = value; }
        }

        public uint Base
        {
            get { return base.Min; }
            set { base.Min = value; }
        }

        public uint Limit
        {
            get { return base.Max; }
            set { base.Max = value; }
        }

        public TAttributes Attributes
        {
            get { return iAttributes; }
        }
		#endregion

        #region From AddressRange
        protected override void OnChanged()
        {
            base.OnChanged();
            //
            UpdateAttributes();
        }
        #endregion

        #region From System.Object
		public override string ToString()
		{
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( base.ToString() );
            ret.Append( " " );
            ret.Append( iFileName );
            //
            return ret.ToString();
		}

        public override int GetHashCode()
        {
            return iFileName.ToUpper().GetHashCode();
        }

        public override bool Equals( object aObject )
        {
            bool ret = false;
            //
            if ( aObject is CodeSegDefinition )
            {
                CodeSegDefinition other = (CodeSegDefinition) aObject;
                //
                string myName = this.FileName;
                string otherName = other.FileName;
                //
                ret = string.Compare( myName, otherName, StringComparison.CurrentCultureIgnoreCase ) == 0;
            }
            else
            {
                ret = base.Equals( aObject );
            }
            //
            return ret;
        }
        #endregion

        #region Internal methods
        private void UpdateAttributes()
        {
            // Remove XIP/RAM attrib before we work out the type...
            iAttributes &= ~TAttributes.EAttributeRAM;
            iAttributes &= ~TAttributes.EAttributeXIP;

            MemoryModel.TMemoryModelType type = MMUtilities.TypeByAddress( Base );
            if ( type != MemoryModel.TMemoryModelType.EMemoryModelUnknown )
            {
                MemoryModel.TMemoryModelRegion region = MMUtilities.RegionByAddress( Base, type );
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
        private string iFileName = string.Empty;
        private uint iChecksum = 0;
        private TAttributes iAttributes = TAttributes.EAttributeNone;
		#endregion
	}
}
