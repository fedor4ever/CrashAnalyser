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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework;
using SymBuildParsingLib.Parser.Framework.Workers;

namespace SymBuildParsingLib.Parser.BuildFile.Workers
{
	public class SymWorkerBuildFileMain : SymParserWorker
	{
		#region Constructors & destructor
		public SymWorkerBuildFileMain( SymParserWorkerContext aContext )
			: base( aContext )
		{
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			// Offer the token to the base class first. If it doesn't want it, we'll
			// check it.
			TTokenConsumptionType ret = base.OfferToken( aToken );
			if	( ret == TTokenConsumptionType.ETokenNotConsumed )
			{
				if	( aToken.Class != SymToken.TClass.EClassComment )
				{
					// Try to find a new child worker to handle this kind
					// of data.
					SymParserWorker worker = CreateWorkerByTokenType( aToken );
					if	( worker != null )
					{
						System.Diagnostics.Debug.WriteLine( "SymWorkerBuildFileMain.OfferToken() - FOUND HANDLER FOR: " + aToken.Value );

						AddChild( worker );
						ret = TTokenConsumptionType.ETokenConsumed;
					}
				}
			}
			//
			return ret;
		}
		#endregion

		#region Internal methods
		private SymParserWorker CreateWorkerByTokenType( SymToken aToken )
		{
			// Find a worker to handle the token type
			SymParserWorkerContext context = new SymParserWorkerContext( WorkerContext.Document.Context, this, aToken );
			//
			SymParserWorker worker = null;
			switch( aToken.Value.ToLower() )
			{
				// Simple preprocessor operations
				case "prj_platforms":
					break;
				case "prj_exports":
					break;
				case "prj_testexports":
					break;
				case "prj_mmpfiles":
					break;
				case "prj_testmmpfiles":
					break;

				// Skip unhandled preprocessor directives
				default:
					break;
			}
			//
			return worker;
		}
		#endregion

		#region Data members
		#endregion
	}
}
