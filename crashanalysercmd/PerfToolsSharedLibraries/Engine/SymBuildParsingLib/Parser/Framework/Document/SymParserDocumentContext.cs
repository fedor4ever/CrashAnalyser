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
using SymBuildParsingLib.Common.Objects;
using SymBuildParsingLib.Parser.Framework.Parser;

namespace SymBuildParsingLib.Parser.Framework.Document
{
	public class SymParserDocumentContext
	{
		#region Constructors & destructor
		public SymParserDocumentContext( string aFileName )
		{
			iFileName = aFileName;

			// Construct empty directories
			iDefineDirectory = new SymDefineDirectory();
			iIncludeDirectory = new SymIncludeDirectory();
		}

		public SymParserDocumentContext( string aFileName, SymDefineDirectory aDefineDirectory, SymIncludeDirectory aIncludeDirectory )
		{
			iFileName = aFileName;
			//
			iDefineDirectory = aDefineDirectory;
			iIncludeDirectory = aIncludeDirectory;
		}

		public SymParserDocumentContext( SymParserDocumentContext aContext )
		{
			iFileName = aContext.FileName;
			//
			iDefineDirectory = aContext.DefineDirectory;
			iIncludeDirectory = aContext.IncludeDirectory;
			//
			iDocument = aContext.Document;
			iParser = aContext.Parser;
		}

		public SymParserDocumentContext( string aFileName, SymParserDocumentContext aContext )
		{
			iFileName = aFileName;
			//
			iDefineDirectory = aContext.DefineDirectory;
			iIncludeDirectory = aContext.IncludeDirectory;
			//
			iDocument = aContext.Document;
			iParser = aContext.Parser;
		}
		#endregion

		#region Properties
		public SymNode CurrentNode
		{
			get { return iDocument.CurrentNode; }
			set { iDocument.CurrentNode = value; }
		}

		public SymDefineDirectory DefineDirectory
		{
			get { return iDefineDirectory; }
		}

		public SymIncludeDirectory IncludeDirectory
		{
			get { return iIncludeDirectory; }
		}

		public string FileName
		{
			get { return iFileName; }
		}

		public SymParserDocument Document
		{
			get { return iDocument; }
			set { iDocument = value; }
		}

		public SymParserBase Parser
		{
			get { return iParser; }
			set { iParser = value; }
		}
		#endregion

		#region Data members
		private readonly string iFileName;
		private SymParserDocument iDocument;
		private readonly SymDefineDirectory iDefineDirectory;
		private readonly SymIncludeDirectory iIncludeDirectory;
		private SymParserBase iParser;
		#endregion
	}
}
