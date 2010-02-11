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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework;
using SymBuildParsingLib.Parser.Framework.Utils;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.PreProcessor.Nodes;
using SymBuildParsingLib.Common.Objects;
using SymbianTree;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public sealed class SymPreProcessorWorkerDefine : SymParserWorkerConsumer
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerDefine( SymParserWorkerContext aContext )
			: base( aContext, SymToken.TClass.EClassNewLine )
		{
			iFunctionParser.RegisterFunctionParserTokens();

			// Set up event handlers
			iFunctionParser.EventArgumentAvailableHandler += new SymBuildParsingLib.Parser.Framework.Utils.SymFunctionParser.ArgumentAvailable( FunctionParserEventArgumentAvailableHandler );
			iFunctionParser.EventLevelStarted += new SymBuildParsingLib.Token.SymTokenBalancer.LevelChangeEventHandler( TokenBalancerEventLevelStarted );
			iFunctionParser.EventLevelFinished += new SymBuildParsingLib.Token.SymTokenBalancer.LevelChangeEventHandler( TokenBalancerEventLevelFinished );
			iFunctionParser.EventLevelsBalanced += new SymBuildParsingLib.Token.SymTokenBalancer.LevelsBalancedEventHandler( TokenBalancerEventLevelsBalanced );
			iFunctionParser.EventLevelsImbalanced += new SymBuildParsingLib.Token.SymTokenBalancer.LevelsImbalancedEventHandler( TokenBalancerEventLevelsImbalanced );
		}
		#endregion

		#region Internal enumerations
		private enum TState
		{
			EStateInitialWhiteSpace = 0,
			EStateDefineName,
			EStateDefineArguments,
			EStateMiddleWhiteSpace,
			EStateDefineValue
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			while( ret == SymParserWorker.TTokenConsumptionType.ETokenNotConsumed )
			{
				TState currentState = State;
				switch( State )
				{
					case TState.EStateInitialWhiteSpace:
						ret = OnStateInitialWhiteSpace( aToken );
						break;
					case TState.EStateDefineName:
						ret = OnStateDefineName( aToken );
						break;
					case TState.EStateDefineArguments:
						ret = OnStateDefineArguments( aToken );
						break;
					case TState.EStateMiddleWhiteSpace:
						ret = OnStateMiddleWhiteSpace( aToken );
						break;
					case TState.EStateDefineValue:
						ret = OnStateDefineValue( aToken );
						break;
					default:
						break;
				}

				TState newState = State;
				bool statesDidNotChange = ( currentState == newState );
				if	( statesDidNotChange )
				{
					// If the state handlers didn't want the token, then we 
					// offer it to the base class instead
					if	( ret == SymParserWorker.TTokenConsumptionType.ETokenNotConsumed )
					{
						ret = base.OfferToken( aToken );
					}
				}
			}

			return ret;
		}
		#endregion

		#region From SymParserWorkerConsumer
		protected override void HandleTerminatingConditionMatch( SymToken aToken )
		{
			switch( State )
			{
			case TState.EStateInitialWhiteSpace:
				break;
			case TState.EStateDefineName:
				MakeDefineName();
				break;
			case TState.EStateDefineArguments:
				break;
			case TState.EStateMiddleWhiteSpace:
				break;
			case TState.EStateDefineValue:
				MakeDefineValue();
				break;
			default:
				break;
			}

			// Do we have a valid define?
			if	( iDefine.IsValid )
			{
				SymNodePreProcessorDefine defineNode = new SymNodePreProcessorDefine();
				defineNode.DefineDefinition = iDefine;
				//
				WorkerContext.CurrentNode.Add( defineNode );
				WorkerContext.DefineDirectory.Add( iDefine );
			}
		}
		#endregion

		#region Internal properties
		private TState State
		{
			get { return iState; }
			set { iState = value; }
		}
		#endregion

		#region Internal state handlers
		private SymParserWorker.TTokenConsumptionType OnStateInitialWhiteSpace( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			if	( aToken.Class == SymToken.TClass.EClassWhiteSpace )
			{
				// Skip leading whitespace
				ret = SymParserWorker.TTokenConsumptionType.ETokenConsumed;
			}
			else
			{
				// Change state
				State = TState.EStateDefineName;
				ResetTokenContainer();
			}
			//
			return ret;
		}

		private SymParserWorker.TTokenConsumptionType OnStateDefineName( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			if	( aToken.Class == SymToken.TClass.EClassWhiteSpace )
			{
				// Got some whitespace - so we're going to bail
				MakeDefineName();
				State = TState.EStateMiddleWhiteSpace;
			}
			else if ( aToken.Class == SymToken.TClass.EClassNewLine || aToken.Class == SymToken.TClass.EClassContinuation )
			{
				// Do nothing - new line is handled by base class (so we must not consume it)
				// and continuations are ignored.
			}
			else
			{
				// Keep reading tokens until we hit some whitespace
				bool consumed = iFunctionParser.OfferToken( aToken );
				if	( consumed == true )
				{
					ret = SymParserWorker.TTokenConsumptionType.ETokenConsumed;
				}
			}
			//
			return ret;
		}

		private SymParserWorker.TTokenConsumptionType OnStateDefineArguments( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			if	( aToken.Class == SymToken.TClass.EClassWhiteSpace && iFunctionParser.CurrentLevelNumber == 0 )
			{
				// Got some whitespace - so we're going to bail
				MakeDefineArgument();
				State = TState.EStateMiddleWhiteSpace;
			}
			else if ( aToken.Class == SymToken.TClass.EClassNewLine || aToken.Class == SymToken.TClass.EClassContinuation )
			{
				// Do nothing - new line is handled by base class (so we must not consume it)
				// and continuations are ignored.
			}
			else
			{
				// Keep reading tokens until we hit some whitespace
				bool consumed = iFunctionParser.OfferToken( aToken );
				if	( consumed == true )
				{
					ret = SymParserWorker.TTokenConsumptionType.ETokenConsumed;
				}
			}
			//
			return ret;
		}

		private SymParserWorker.TTokenConsumptionType OnStateMiddleWhiteSpace( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			if	( aToken.Class == SymToken.TClass.EClassWhiteSpace )
			{
				// Skip leading whitespace
				ret = SymParserWorker.TTokenConsumptionType.ETokenConsumed;
			}
			else
			{
				// Change state
				State = TState.EStateDefineValue;
				ResetTokenContainer();
			}
			//
			return ret;
		}

		private SymParserWorker.TTokenConsumptionType OnStateDefineValue( SymToken aToken )
		{
			SymParserWorker.TTokenConsumptionType ret = SymParserWorker.TTokenConsumptionType.ETokenNotConsumed;
			//
			if	( aToken.Class != SymToken.TClass.EClassNewLine )
			{
				iTokens.Append( aToken );
				ret = SymParserWorker.TTokenConsumptionType.ETokenConsumed;
			}
			//
			return ret;
		}
		#endregion

		#region Internal token balancer/function parser event handlers
		private void FunctionParserEventArgumentAvailableHandler( SymArgument aArgument, SymToken aDelimitingToken )
		{
			MakeDefineArgument( aArgument );
		}

		private void TokenBalancerEventLevelStarted( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel )
		{
			switch( State )
			{
			case TState.EStateDefineName:
				MakeDefineName();
				State = TState.EStateDefineArguments;
				break;
			case TState.EStateDefineArguments:
				break;
			case TState.EStateDefineValue:
			case TState.EStateInitialWhiteSpace:
			case TState.EStateMiddleWhiteSpace:
			default:
				System.Diagnostics.Debug.Assert( false );
				break;
			}
		}

		private void TokenBalancerEventLevelFinished( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel )
		{
			switch( State )
			{
				case TState.EStateDefineName:
					MakeDefineName();
					State = TState.EStateDefineArguments;
					break;
				case TState.EStateDefineArguments:
					if	( aLevelCount == 0 )
					{
						MakeDefineArgument();
						State = TState.EStateMiddleWhiteSpace;
					}
					break;
				case TState.EStateDefineValue:
				case TState.EStateInitialWhiteSpace:
				case TState.EStateMiddleWhiteSpace:
				default:
					System.Diagnostics.Debug.Assert( false );
					break;
			}
		}

		private void TokenBalancerEventLevelsBalanced()
		{
		}

		private void TokenBalancerEventLevelsImbalanced()
		{
			//TODO
		}
		#endregion

		#region Internal temp buffer related methods
		public void ResetTokenContainer()
		{
			iTokens.Reset();
			iFunctionParser.Reset();
		}

		public void AddToken( SymToken aToken )
		{
			iTokens.Append( aToken );
		}
		#endregion

		#region Misc internal helpers
		private void MakeDefineName()
		{
			iDefine.Name = iFunctionParser.MakeFunctionName();
		}

		private void MakeDefineArgument()
		{
			SymArgument argument = iFunctionParser.MakeArgument();
			MakeDefineArgument( argument );
		}

		private void MakeDefineArgument( SymArgument aArgument )
		{
			// Convert generic argument into define-specific argument
			SymDefineArgument defineArgument = new SymDefineArgument( aArgument );
			iDefine.AddArgument( defineArgument );
		}

		private void MakeDefineValue()
		{
			iDefine.Value = iTokens;
			iTokens = new SymTokenContainer();
		}
		#endregion

		#region Data members
		private TState iState = TState.EStateInitialWhiteSpace;
		private SymDefineDefinition iDefine = new SymDefineDefinition();
		private SymTokenContainer iTokens = new SymTokenContainer();
		private SymFunctionParser iFunctionParser = new SymFunctionParser();
		#endregion
	}
}
