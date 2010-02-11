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
using SymbianTree;

namespace SymBuildParsingLib.Parser.BuildFile.Nodes
{
	public abstract class SymNodeBuildFileEntity : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymNodeBuildFileEntity()
		{
		}
		#endregion

		#region API
		#endregion
	}

	public sealed class SymNodeBuildFileEntityTest : SymNodeBuildFileEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileEntityTest()
		{
		}
		#endregion

		#region API
		#endregion
	}

	public sealed class SymNodeBuildFileEntityRelease : SymNodeBuildFileEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileEntityRelease()
		{
		}
		#endregion

		#region API
		#endregion
	}
}
