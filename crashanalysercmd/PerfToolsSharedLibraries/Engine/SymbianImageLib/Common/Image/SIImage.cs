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
using System.ComponentModel;
using System.Threading;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Content;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.Common.Factory;

namespace SymbianImageLib.Common.Image
{
    public abstract class SIImage : DisposableObject, ITracer, IEnumerable<SIContent>
    {
        #region Factory
        public static SIImage New( ITracer aTracer, string aFileName )
        {
            return SIImage.New( aTracer, new FileInfo( aFileName ) );
        }

        public static SIImage New( ITracer aTracer, FileInfo aFileInfo )
        {
            SIImage ret = null;
            //
            if ( aFileInfo.Exists )
            {
                Stream fileStream = aFileInfo.OpenRead();
                try
                {
                    // If creating the image succeeds then we transfer ownership
                    // of the file stream
                    ret = SIImage.New( aTracer, fileStream, aFileInfo.FullName );
                    if ( ret == null )
                    {
                        fileStream.Close();
                    }
                }
                catch( Exception )
                {
                    fileStream.Close();
                }
            }
            //
            return ret;
        }

        public static SIImage New( ITracer aTracer, Stream aStream, string aName )
        {
            SIImage ret = null;
            //
            PluginManager<SIFactory> imageFactories = new PluginManager<SIFactory>();
            imageFactories.LoadFromCallingAssembly();
            foreach ( SIFactory factory in imageFactories )
            {
                ret = factory.CreateImage( aTracer, aStream, aName );
                if ( ret != null )
                {
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        internal SIImage( ITracer aTracer, SIStream aStream, string aName )
        {
            iName = aName;
            iTracer = aTracer;
            iStream = aStream;
        }
        #endregion

        #region Framework API
        public abstract SIHeader Header
        {
            get;
        }
        #endregion

        #region API
        public bool Contains( SIContent aContent )
        {
            lock ( iContentList )
            {
                bool ret = iContentList.Contains( aContent );
                return ret;
            }
        }

        internal void Remove( SIContent aContent )
        {
            lock ( iContentList )
            {
                iContentList.Remove( aContent );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iContentList )
                {
                    return iContentList.Count;
                }
            }
        }

        public TSymbianCompressionType CompressionType
        {
            get { return Header.CompressionType; }
        }

        public SIContent this[ int aIndex ]
        {
            get 
            {
                lock ( iContentList )
                {
                    return iContentList[ aIndex ];
                }
            }
        }

        public SIContent this[ string aFileName ]
        {
            get
            {
                lock ( iContentList )
                {
                    return iContentList[ aFileName ];
                }
            }
        }

        internal SIStream Stream
        {
            get { return iStream; }
            set
            {
                System.Diagnostics.Debug.Assert( iStream != value );
                //
                if ( iStream != null )
                {
                    iStream.Close();
                }
                iStream = value; 
            }
        }

        public string Name
        {
            get { return iName; }
        }
        #endregion

        #region Internal methods
        protected void RegisterFile( SIContent aFile )
        {
            iContentList.Add( aFile );
        }

        private SIContentList FileList
        {
            get { return iContentList; }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iTracer.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iTracer.Trace( aFormat, aParams );
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iStream != null )
                {
                    iStream.Close();
                    iStream = null;
                }
            }
        }
        #endregion

        #region From IEnumerable<SymbianImageContentFile>
        public IEnumerator<SIContent> GetEnumerator()
        {
            return iContentList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return iContentList.GetEnumerator();
        }
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        private readonly string iName;
        private SIStream iStream = null;
        private SIContentList iContentList = new SIContentList();
        #endregion
    }
}
