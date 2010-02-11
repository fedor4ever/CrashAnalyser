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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using SymbianUtils.Utilities;

namespace SymbianStackLib.Utilities
{
	public class BinaryFileToStackFormatConverter : AsyncBinaryFileReader
	{
		#region Constructors
		public BinaryFileToStackFormatConverter( string aFileName )
		:	base( aFileName )
		{
		}
		#endregion

        #region API
        public void Convert()
        {
            base.AsyncRead();
        }
        #endregion

        #region Properties
        public string ConvertedData
		{
			get
			{
				return iOutput.ToString();
			}
		}
		#endregion

		#region From AsyncTextReader
		protected override void HandleReadBytes( byte[] aData )
		{
			foreach( byte b in aData )
			{
				iBytes.Enqueue( b );
			}

			string data = RawByteUtility.ConvertDataToText( iBytes, false, ref iCurrentAddress );
			iOutput.Append( data );
		}

		protected override void HandleReadCompleted()
		{
			base.HandleReadCompleted();

            string data = RawByteUtility.ConvertDataToText( iBytes, true, ref iCurrentAddress );
			iOutput.Append( data );
		}
		#endregion

		#region Data members
		private uint iCurrentAddress = 0;
		private StringBuilder iOutput = new StringBuilder();
        private Queue<byte> iBytes = new Queue<byte>();
		#endregion
	}
}
