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
using System.Text;
using System.IO;
using SymbianUtils.Tracer;
using SymbianUtils.Streams;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Version;

namespace SymbianImageLib.ROFS.Structures
{
    internal class TRofsEntry
    {
        #region Constructors
        public TRofsEntry( SymbianStreamReaderLE aReader, ITracer aTracer )
        {
            long startPos = aReader.BaseStream.Position;

            iStructSize = aReader.ReadUInt16();
            iUids = new TCheckedUid( aReader );
            
            // Skip name offset - not useful
            aReader.ReadUInt8();
            //
            iAtt = aReader.ReadUInt8();
            iFileSize = aReader.ReadUInt32();
            iFileAddress = aReader.ReadUInt32();
            iAttExtra = aReader.ReadUInt8();

            // Read name - the length is in unicode characters.
            byte nameLength = aReader.ReadUInt8();
            iName = aReader.ReadStringUTF16( nameLength );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Size
        {
            get { return iStructSize; }
        }

        public TCheckedUid Uids
        {
            get
            {
                return iUids;
            }
        }

        public uint Att
        {
            get { return iAtt; }
        }

        public uint FileSize
        {
            get { return iFileSize; }
        }

        public uint FileAddress
        {
            get { return iFileAddress; }
        }

        public uint AttExtra
        {
            get { return iAttExtra; }
        }

        public string Name
        {
            get { return iName; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iName;
        }

        public override int GetHashCode()
        {
            return iName.GetHashCode();
        }
        #endregion

        #region Data members
        private readonly ushort iStructSize;	    // Total size of entry, header + name + any padding
        private readonly TCheckedUid iUids;		    // A copy of all the UID info
        private readonly byte iAtt;			        // standard file attributes
        private readonly uint iFileSize;		    // real size of file in bytes (may be different from size in image)
						                            // for subdirectories this is the total size of the directory
						                            // block entry excluding padding
        private readonly uint iFileAddress;	        // address in image of file start
        private readonly byte iAttExtra;		    // extra ROFS attributes (these are inverted so 0 = enabled)
        private readonly string iName;
        #endregion
    }
}
