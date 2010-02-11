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
    internal class TRofsDir : IEnumerable<TRofsEntry>
    {
        #region Constructors
        public TRofsDir( string aName, uint aFileAddress, SymbianStreamReaderLE aReader, ITracer aTracer )
        {
            //aTracer.Trace( "[Dir ] @ 0x{0:x8} {1}", aFileAddress, aName );
            //
            iName = aName;
            //
            aReader.Seek( aFileAddress, SeekOrigin.Begin );
            //
            iStructSize = aReader.ReadUInt16();
            iPadding = aReader.ReadUInt8();
            iFirstEntryOffset = aReader.ReadUInt8();
            iFileBlockAddress = aReader.ReadUInt32();
            iFileBlockSize = aReader.ReadUInt32();
            //
            if ( iFileBlockAddress != 0 )
            {
                // Directory has files in it
                AddFiles( aReader, aTracer, iFileBlockAddress, iFileBlockSize );
            }
            if ( iStructSize > KMinimumDirectoryEntrySize )
            {
                // Directory has subdirectories
                AddSubDirs( aReader, aTracer, aFileAddress + iStructSize );
            }
        }
        #endregion

        #region API
        #endregion

        #region Constants
        // The minimum standard entry size, irrespective of whether the directory 
        // contains subdirs or files.
        public const uint KMinimumDirectoryEntrySize = 2 + 1 + 1 + 4 + 4;
        #endregion

        #region Properties
        public uint Size
        {
            get { return iStructSize; }
        }

        public string Name
        {
            get { return iName; }
        }

        public uint FirstEntryOffset
        {
            get { return iFirstEntryOffset; }
        }

        public uint FileBlockAddress
        {
            get { return iFileBlockAddress; }
        }

        public uint FileBlockSize
        {
            get { return iFileBlockSize; }
        }

        public TRofsDir[] SubDirectories
        {
            get
            {
                return iSubdirectories.ToArray();
            }
        }
        #endregion

        #region Internal methods
        private void AddFiles( SymbianStreamReaderLE aReader, ITracer aTracer, long aStartPosition, uint aSize )
        {
            long originalPosition = aReader.Position;
            long endPos = aStartPosition + aSize;
            //
            long filePosition = aStartPosition;
            while ( filePosition < endPos )
            {
                // Seek to start of file and read it
                aReader.Seek( filePosition, SeekOrigin.Begin );
                TRofsEntry entry = new TRofsEntry( aReader, aTracer );
                iFiles.Add( entry );
                //aTracer.Trace( "[File] @ 0x{0:x8} {1:d8} {2}", filePosition, entry.FileSize, entry.Name );

                // Move to next file
                filePosition += entry.Size;
            }

            // Restore original position
            aReader.Seek( originalPosition, SeekOrigin.Begin );
        }

        private void AddSubDirs( SymbianStreamReaderLE aReader, ITracer aTracer, long aEndDirPos )
        {
            long originalPosition = aReader.Position;
            //
            long filePosition = aReader.BaseStream.Position;
            while ( filePosition < aEndDirPos )
            {
                // Seek to start of subdir and read basic properties
                aReader.Seek( filePosition, SeekOrigin.Begin );
                TRofsEntry entry = new TRofsEntry( aReader, aTracer );

                // Create a new subdirectory that we'll process recursively.
                TRofsDir subdir = new TRofsDir( entry.Name, entry.FileAddress, aReader, aTracer );
                iSubdirectories.Add( subdir );

                // Move to next subdir, rounding to 4 bytes
                long nextSubDirOffset = entry.Size;
                nextSubDirOffset += ( ( 4 - nextSubDirOffset ) & 0x3 );
                filePosition += nextSubDirOffset;
            }

            // Restore original position
            aReader.Seek( originalPosition, SeekOrigin.Begin );
        }
        #endregion

        #region From IEnumerable<TRofsEntry>
        public IEnumerator<TRofsEntry> GetEnumerator()
        {
            foreach ( TRofsEntry entry in iFiles )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( TRofsEntry entry in iFiles )
            {
                yield return entry;
            }
        }
        #endregion

        #region Data members
        private readonly string iName;
        private ushort iStructSize;		    // Total size of this directory block including padding
        private byte iPadding;
        private byte iFirstEntryOffset;	    // offset to first entry
        private uint iFileBlockAddress;	    // address of associated file block
        private uint iFileBlockSize;		// size of associated file block

        private List<TRofsEntry> iFiles = new List<TRofsEntry>();
        private List<TRofsDir> iSubdirectories = new List<TRofsDir>();
        #endregion
    }
}
