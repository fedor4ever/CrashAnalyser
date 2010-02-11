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
	public abstract class AsyncTextReader : AsyncTextReaderBase
	{
		#region Constructors
		protected AsyncTextReader()
            : this( new AsyncTextReaderPrefix() )
        {
		}

        protected AsyncTextReader( ITracer aTracer )
		:   this( new AsyncTextReaderPrefix(), aTracer )
		{
		}

        protected AsyncTextReader( AsyncTextReaderPrefix aPrefixes )
		:	this( aPrefixes, null )
		{
		}
   
        protected AsyncTextReader( AsyncTextReaderPrefix aPrefixes, ITracer aTracer )
            : this( aPrefixes, false, aTracer )
        {
        }

        protected AsyncTextReader( AsyncTextReaderPrefix aPrefixes, bool aRouteBlankLines, ITracer aTracer )
            : base( aTracer )
		{
			iPrefixes = aPrefixes;
			iRouteBlankLines = aRouteBlankLines;
		}
		#endregion

		#region API
		public void AddFilter( AsyncTextReaderFilter aFilter )
		{
			iFilters.Add( aFilter );
		}
		#endregion

		#region Properties
		public AsyncTextReaderPrefix PrefixDefinition
		{
			get { return iPrefixes; }
		}

		public AsyncTextReaderFilterCollection Filters
		{
			get { return iFilters; }
		}

		public bool RouteBlankLines
		{
			get { return iRouteBlankLines; }
		}

        public string OriginalLine
        {
            get { return iOriginalLine; }
        }
		#endregion

		#region New Framework
		protected abstract void HandleFilteredLine( string aLine );
		#endregion

		#region From AsyncTextReaderBase
		protected override void HandleReadLine( string aLine )
		{
            iOriginalLine = ( aLine != null ? aLine : string.Empty );
			bool isBlankLine = ( aLine != null && aLine.Length == 0 );
			//
			if ( aLine != null )
			{
				if	( ( isBlankLine && RouteBlankLines ) || aLine.Length > 0 )
				{
					// Clean it
					if	( !isBlankLine )
					{
						iPrefixes.Clean( ref aLine );
					}

					// Process filter on line
					bool propagateLine = true;
					if	( iFilters != null && !isBlankLine )
					{
						propagateLine = iFilters.ProcessFilters( ref aLine );
					}
                
					// Notify derived classes
					if	( propagateLine )
					{
						HandleFilteredLine( aLine );
					}
				}
			}
		}
		#endregion

		#region Data members
		private readonly AsyncTextReaderPrefix iPrefixes;
		private readonly bool iRouteBlankLines;
        private string iOriginalLine = string.Empty;
		private AsyncTextReaderFilterCollection iFilters = new AsyncTextReaderFilterCollection();
		#endregion
	}
}
