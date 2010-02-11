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
using System.Text;
using System.Collections;
using SymBuildParsingLib.Tree;
using SymbianTree;

namespace SymBuildParsingLib.Token
{
	#region Enumerations
	public enum TLevelExpectations
	{
		ELevelExpectationsAny = 0,
		ELevelExpectationsAtLevel,
		ELevelExpectationsBelowLevelNumber,
		ELevelExpectationsAboveLevelNumber
	}

	[Flags]
	public enum TAssociatedBehaviour
	{
		EBehaviourNone = 0,
		EBehaviourRemoveReduntantBracketing = 1,
		EBehaviourCreateSubTree = 2
	}
	#endregion

	public class SymTokenBalancerMatchCriteria
	{
		#region Constructors & destructor
		public SymTokenBalancerMatchCriteria( SymToken aDiametricToken, bool aEmit, bool aChangesLevel, TLevelExpectations aLevelExpectations, int aAssociatedLevel, TAssociatedBehaviour aAssociatedBehaviour )
		{
			iDiametricToken = aDiametricToken;
			iEmit = aEmit;
			iChangesLevel = aChangesLevel;
			iLevelExpectations = aLevelExpectations;
			iAssociatedLevel = aAssociatedLevel;
			iAssociatedBehaviour = aAssociatedBehaviour;
		}
		#endregion

		#region API
		public bool Matches( int aLevel )
		{
			bool matches = false;
			//
			switch( iLevelExpectations )
			{
			default:
			case TLevelExpectations.ELevelExpectationsAny:
				matches = true;
				break;
			case TLevelExpectations.ELevelExpectationsAtLevel:
				matches = ( aLevel == iAssociatedLevel );
				break;
			case TLevelExpectations.ELevelExpectationsBelowLevelNumber:
				matches = ( aLevel < iAssociatedLevel );
				break;
			case TLevelExpectations.ELevelExpectationsAboveLevelNumber:
				matches = ( aLevel > iAssociatedLevel );
				break;
			}
			//
			return matches;
		}
		#endregion

		#region Properties
		public SymToken DiametricToken
		{
			get { return iDiametricToken; }
		}

		public bool Emit
		{
			get { return iEmit; }
		}

		public bool ChangesLevel
		{
			get { return iChangesLevel; }
		}

		public int AssociatedLevel
		{
			get { return iAssociatedLevel; }
		}

		public TLevelExpectations LevelExpectations
		{
			get { return iLevelExpectations; }
		}

		public TAssociatedBehaviour AssociatedBehaviour
		{
			get { return iAssociatedBehaviour; }
		}
		#endregion

		#region Properties - associated behaviour bitflag helpers
		public bool IsAssociatedBehaviourRemoveRedundantBracketing
		{
			get { return ( iAssociatedBehaviour & TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing ) == TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing; }
		}

		public bool IsAssociatedBehaviourCreateSubTree
		{
			get { return ( iAssociatedBehaviour & TAssociatedBehaviour.EBehaviourCreateSubTree ) == TAssociatedBehaviour.EBehaviourCreateSubTree; }
		}
		#endregion

		#region From System.Object
		public override bool Equals( object aObject )
		{
			bool same = false;
			//
			if	( aObject is SymTokenBalancerMatchCriteria )
			{
				SymTokenBalancerMatchCriteria otherInfo = (SymTokenBalancerMatchCriteria) aObject;
				//
				same = ( Emit == otherInfo.Emit ) &&
					   ( ChangesLevel == otherInfo.ChangesLevel ) &&
					   ( LevelExpectations == otherInfo.LevelExpectations ) &&
					   ( AssociatedLevel == otherInfo.AssociatedLevel ) &&
					   ( AssociatedBehaviour == otherInfo.AssociatedBehaviour );
			}
			//
			return same;
		}
		#endregion

		#region Constants
		public const int KAssociatedLevelDefault = 1;
		#endregion

		#region Data members
		private readonly SymToken iDiametricToken;
		private readonly bool iEmit;
		private readonly bool iChangesLevel;
		private readonly TLevelExpectations iLevelExpectations = TLevelExpectations.ELevelExpectationsAny;
		private readonly int iAssociatedLevel = KAssociatedLevelDefault;
		private readonly TAssociatedBehaviour iAssociatedBehaviour;
		#endregion
	}
}
