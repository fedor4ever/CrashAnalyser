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
using SymbianTree;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Document;
using SymBuildParsingLib.Parser.PreProcessor.Parser;
using SymBuildParsingLib.Parser.PreProcessor.Workers;
using SymBuildParsingLib.Parser.BuildFile.Workers;
using SymBuildParsingLib.Common.Objects;


namespace SymBuildParsingLib.Parser.BuildFile.Parser
{
	public class SymBuildFileParser : SymPreProcessorParser
	{
		public SymBuildFileParser( SymParserDocument aDocument )
			: base( aDocument )
		{
		}

		#region From SymParserBase
		protected override string ParserName
		{
			get { return this.ToString(); }
		}

		protected override void PrepareInitialWorkers()
		{
			SymParserWorkerContext context = new SymParserWorkerContext( Document.Context );
			SymWorkerBuildFileMain mainWorker = new SymWorkerBuildFileMain( context );
			//
			QueueWorker( mainWorker );

			// Make a base call
			base.PrepareInitialWorkers();
		}
		#endregion
	}
}
