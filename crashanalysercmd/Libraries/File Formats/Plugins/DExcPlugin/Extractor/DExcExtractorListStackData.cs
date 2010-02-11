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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Primer;

namespace DExcPlugin.Extractor
{
	internal class DExcExtractorListStackData : DExcExtractorList
    {
        #region Delegates & events
        public delegate void StackDataChangeHandler( DExcExtractorListStackData aSelf );
        public event StackDataChangeHandler StackChanged;
        #endregion

        #region Constructors
        public DExcExtractorListStackData( DExcExtractor.TState aState, DExcExtractorListType aType )
            : base( aState, aType )
		{
            iPrimer = new DataBufferPrimer( iBuffer );
		}
		#endregion

		#region API
        public override void Add( string aLine )
        {
            base.Add( aLine );
            //
            iPrimer.PrimeLine( aLine );
            if ( StackChanged != null )
            {
                StackChanged( this );
            }
        }

        public void Prime( string aFileName )
        {
            using ( FileStream stream = new FileStream( aFileName, FileMode.Open ) )
            {
                long len = stream.Length;
                byte[] bytes = new byte[ len ];
                stream.Read( bytes, 0, (int) len );
                iPrimer.Prime( bytes, 0 );
            }
        }
		#endregion

		#region Properties
        public DataBuffer StackData
        {
            get { return iBuffer; }
        }
		#endregion

        #region Internal methods
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly DataBufferPrimer iPrimer;
        private DataBuffer iBuffer = new DataBuffer();
        #endregion
    }
}
