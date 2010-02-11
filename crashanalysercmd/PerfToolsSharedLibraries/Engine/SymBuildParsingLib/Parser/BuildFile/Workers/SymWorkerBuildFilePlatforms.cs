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
	public class SymWorkerBuildFilePlatforms : SymParserWorkerConsumer
	{
		#region Constructors & destructor
		public SymWorkerBuildFilePlatforms( SymParserWorkerContext aContext )
			: base( aContext, SymToken.TClass.EClassNewLine )
		{
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			return TTokenConsumptionType.ETokenConsumed;
		}
		#endregion

		#region Data members
		#endregion
	}
}
