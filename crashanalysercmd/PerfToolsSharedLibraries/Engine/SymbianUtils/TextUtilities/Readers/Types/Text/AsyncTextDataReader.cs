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
using System.Threading;
using System.Collections;
using SymbianUtils.Tracer;

namespace SymbianUtils
{
	public abstract class AsyncTextDataReader : AsyncTextReader
	{
		#region Enumerations
		public enum TReadDirection
		{
			EReadDirectionForwards = 0,
			EReadDirectionBackwards
		}
		#endregion

		#region Construct & destruct
		protected AsyncTextDataReader( string[] aLines )
			: this( aLines, new AsyncTextReaderPrefix() )
		{
		}

        protected AsyncTextDataReader( string[] aLines, ITracer aTracer )
            : this( aLines, new AsyncTextReaderPrefix(), aTracer )
        {
        }

        protected AsyncTextDataReader( string[] aLines, AsyncTextReaderPrefix aPrefixes )
			: this( aLines, TReadDirection.EReadDirectionForwards, aPrefixes )
		{
		}

        protected AsyncTextDataReader( string[] aLines, AsyncTextReaderPrefix aPrefixes, ITracer aTracer )
            : this( aLines, TReadDirection.EReadDirectionForwards, aPrefixes, aTracer )
        {
        }

        protected AsyncTextDataReader( string[] aLines, TReadDirection aReadDirection )
			: this( aLines, TReadDirection.EReadDirectionForwards, new AsyncTextReaderPrefix(), null )
		{
		}

        protected AsyncTextDataReader( string[] aLines, TReadDirection aReadDirection, ITracer aTracer )
            : this( aLines, TReadDirection.EReadDirectionForwards, new AsyncTextReaderPrefix(), aTracer )
		{
		}

        protected AsyncTextDataReader( string[] aLines, TReadDirection aReadDirection, AsyncTextReaderPrefix aPrefixes )
            : this( aLines, aReadDirection, aPrefixes, null )
        {
		}

        protected AsyncTextDataReader( string[] aLines, TReadDirection aReadDirection, AsyncTextReaderPrefix aPrefixes, ITracer aTracer )
			: base( aPrefixes, aTracer )
		{
			iLines = aLines;
			iReadDirection = aReadDirection;
			//
			switch( iReadDirection )
			{
			default:
			case TReadDirection.EReadDirectionForwards:
				iLineIndex = 0;
				break;
			case TReadDirection.EReadDirectionBackwards:
				iLineIndex = iLines.Length - 1;
				break;
			}
		}
		#endregion

        #region Properties
        public int LineNumber
		{
			get { return iLineIndex; }
		}
		#endregion

		#region Abstract reading framework
		protected override int CalculateProgress()
		{
			int prog = 0;
			//
			switch( iReadDirection )
			{
			default:
			case TReadDirection.EReadDirectionForwards:
				{
				prog = base.CalculateProgress();
				break;
				}
			case TReadDirection.EReadDirectionBackwards:
				{
				float positionAsFloat = (float)Position;
				float sizeAsFloat = (float)Size;
				prog = (int)((positionAsFloat / sizeAsFloat) * 100.0);
				prog = System.Math.Max(1, System.Math.Min(100, prog));
				break;
				}
			}
			//
			return prog;
		}

		protected override string ProvideReadLine()
		{
			string ret = null;
			//
			switch( iReadDirection )
			{
			default:
			case TReadDirection.EReadDirectionForwards:
				if	( iLineIndex < iLines.Length )
				{
					ret = iLines[ iLineIndex++ ];
				}
				break;
			case TReadDirection.EReadDirectionBackwards:
				if	( iLineIndex >= 0 )
				{
					ret = iLines[ iLineIndex-- ];
				}
				break;
			}
			//
			return ret;
		}

		protected override long Size
		{
			get
			{
				long size = iLines.LongLength;
				return size;
			}
		}

		protected override long Position
		{
			get
			{
				long position = (long) iLineIndex;
				return position;
			}
		}
		#endregion

		#region Data members
		private readonly string[] iLines;
		private readonly TReadDirection iReadDirection;
		private int iLineIndex = 0;
		#endregion
	}
}
