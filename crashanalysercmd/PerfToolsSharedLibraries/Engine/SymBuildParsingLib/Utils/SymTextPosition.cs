/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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

namespace SymBuildParsingLib.Utils
{
	public class SymTextPosition
	{
		#region Constructors & destructor
		public SymTextPosition()
		{
			iLine = 1;
			iColumn = 1;
		}

		public SymTextPosition( long aLine, long aColumn )
		{
			iLine = aLine;
			iColumn = aColumn;
		}

		public SymTextPosition( SymTextPosition aCopy )
		{
			iLine = aCopy.Line;
			iColumn = aCopy.Column;
		}
		#endregion

		#region API
		public void Inc()
		{
			iColumn++;
		}

		public void Inc( int aColumnAmount )
		{
			iColumn += aColumnAmount;
		}

		public void NewLine()
		{
			iLine++;
			iColumn = 1;
		}
		#endregion

		#region Properties
		public long Line
		{
			get { return iLine; }
			set { iLine = value; }
		}

		public long Column
		{
			get { return iColumn; }
			set { iColumn = value; }
		}
		#endregion

		#region Data members
		private long iLine;
		private long iColumn;
		#endregion
	}
}
