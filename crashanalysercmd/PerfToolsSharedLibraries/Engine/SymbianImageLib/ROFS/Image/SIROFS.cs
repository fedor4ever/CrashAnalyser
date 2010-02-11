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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using SymbianUtils;
using SymbianUtils.Streams;
using SymbianUtils.Tracer;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Content;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.ROFS.Content;
using SymbianImageLib.ROFS.Header;
using SymbianImageLib.ROFS.Structures;

namespace SymbianImageLib.ROFS.Image
{
    public class SIROFS : SIImage
    {
        #region Constructors
        public SIROFS( ITracer aTracer, Stream aStream, string aName )
            : base( aTracer, new SIStream( aStream ), aName )
        {
            base.Trace( "[SymbianImageROFS] Ctor() - START" );
            iHeader = SIHeaderROF.New( this, aStream );

            ReadFiles();
            base.Trace( "[SymbianImageROFS] Ctor() - END" );
        }
        #endregion

        #region From SIImage
        public override SIHeader Header
        {
            get { return iHeader; }
        }
        #endregion

        #region API
        public static bool IsROFS( Stream aStream )
        {
            bool ret = false;
            //
            try
            {
                ret = SIHeaderROF.IsROFS( aStream );
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void ReadFiles()
        {
            base.Trace( "[SymbianImageROFS] ReadFiles() - reading directory tree..." );
            using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( (Stream) base.Stream ) )
            {
                reader.Seek( iHeader.InternalHeader.DirTreeOffset, SeekOrigin.Begin );
                TRofsDir rootDirectory = new TRofsDir( string.Empty, (uint) reader.Position, reader, this );

                base.Trace( "[SymbianImageROFS] ReadFiles() - converting directory tree to full full paths..." );
                MakeFilesForDirectory( rootDirectory, string.Empty );
            }
        }

        private void MakeFilesForDirectory( TRofsDir aDirectory, string aParentDirectoryName )
        {
            string name = aParentDirectoryName + @"\";

            // Create files
            StringBuilder fullName = new StringBuilder();
            foreach ( TRofsEntry entry in aDirectory )
            {
                fullName.Length = 0;
                fullName.Append( name );
                fullName.Append( entry.Name );
                //
                SIContent file = SIContentFactoryROFS.New( this, fullName.ToString(), entry.FileSize, entry.FileAddress, entry.Uids );
                base.RegisterFile( file );
            }

            // Create files in any subdirs
            foreach ( TRofsDir subdir in aDirectory.SubDirectories )
            {
                MakeFilesForDirectory( subdir, name + subdir.Name );
            }
        }
        #endregion

        #region Data members
        private readonly SIHeaderROF iHeader;
        #endregion
    }
}
