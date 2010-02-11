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
using System.Collections;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;

namespace SymBuildParsingLib.Parser.Framework.Workers
{
	public abstract class SymParserWorker
	{
		#region Enumerations
		public enum TTokenConsumptionType
		{
			ETokenNotConsumed = 0,
			ETokenConsumed
		};
		#endregion

		#region Constructors & destructor
		public SymParserWorker( SymParserWorkerContext aContext )
			: this( aContext, 0 )
		{
		}

		public SymParserWorker( SymParserWorkerContext aContext, int aPriority )
		{
			iWorkerContext = aContext;
			iPriority = aPriority;
		}
		#endregion

		#region API
		public virtual SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType consumptionType = TTokenConsumptionType.ETokenNotConsumed;
			//
			int count = iChildren.Count;
			for( int i=0; i<count; i++ )
			{
				SymParserWorker worker = this[ i ];
				consumptionType = worker.OfferToken( aToken );
				//
				if	( consumptionType == SymParserWorker.TTokenConsumptionType.ETokenConsumed )
				{
					break;
				}
			}
			//
			return consumptionType;
		}
		#endregion

		#region Worker related
		public virtual void RemoveSelf()
		{
			if	( WorkerContext.Parent != null )
			{
				WorkerContext.Parent.RemoveChild( this );
			}
		}

		public virtual void AddChild( SymParserWorker aWorker )
		{
			iChildren.Add( aWorker );
		}

		public virtual void RemoveChild( SymParserWorker aWorker )
		{
			iChildren.Remove( aWorker );
		}
		
		public int ChildCount
		{
			get { return iChildren.Count; }
		}

		public SymParserWorker this[ int aIndex ]
		{
			get { return (SymParserWorker) iChildren[ aIndex ]; }
		}
		#endregion

		#region Properties
		public int Priority
		{
			get { return iPriority; }
		}

		public SymParserWorkerContext WorkerContext
		{
			get { return iWorkerContext; }
		}
		#endregion

		#region Data members
		private readonly SymParserWorkerContext iWorkerContext;
		private int iPriority = 0;
		private ArrayList iChildren = new ArrayList(2);
		#endregion
	}
}
