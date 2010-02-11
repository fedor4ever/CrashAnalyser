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

namespace SymbianUtils
{
	public class TracePrefixAnalyser
	{
		#region Constructors
		public TracePrefixAnalyser()
		{
		}
		#endregion

		#region API
		public bool CleanLine( ref string aLine )
		{
			bool matched = false;
			string prefix = IdentifyTracePrefixFromLine( aLine );
			//
			if	( prefix.Length > 0 )
			{
				int prefixPos = aLine.IndexOf( prefix );
				aLine = aLine.Substring( prefixPos + prefix.Length );
				matched = true;
			}
			//
			return matched;
		}

		public StringBuilder CleanFile( string aFileName )
		{
			StringBuilder lines = new StringBuilder();
			//
			using( StreamReader reader = new StreamReader( aFileName ) )
			{
				string line = reader.ReadLine();
				while( line != null )
				{
					CleanLine( ref line );
					lines.Append( line + System.Environment.NewLine );
					//
					line = reader.ReadLine();
				}
			}
			//
			return lines;
		}

		public string IdentifyTracePrefixFromLine( string aLine )
		{
			string prefix = string.Empty;
			//
            if ( aLine.IndexOf( KFinalPrefix ) >= 0 )
            {
                prefix = KFinalPrefix;
            }
			//
			return prefix;
		}

		public string IdentifyTracePrefix( string aFileName )
		{
			string prefix = string.Empty;
			//
			try
			{
				using( StreamReader reader = new StreamReader( aFileName ) )
				{
					int lineCounter = 0;
					string line = string.Empty;
					//
					while( lineCounter < KNumberOfLinesToCheck && line != null )
					{
						try
						{
							line = reader.ReadLine();
							//
                            if ( line != null )
                            {
                                string tmp = IdentifyTracePrefixFromLine( line );
                                if ( tmp.Length > 0 )
                                {
                                    prefix = tmp;
                                    break;
                                }
                            }
						}
						catch(Exception)
						{
							line = null;
						}
                        //
                        ++lineCounter;
					}
				}
			}
			catch( Exception )
			{
			}
			//
			return prefix;
		}
		#endregion

		#region Internal constants
		private const int KNumberOfLinesToCheck = 500;
		#endregion

		#region Data members
		private const int KLineLookAheadCount = 200;
		private const string KFinalPrefix = "msg:";
		#endregion
	}
}
