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
using System.Threading;
using System.Collections;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Lexer;

namespace SymBuildParsingLib.Grouper
{
	public class SymGrouperMastermind
	{
		#region Enumerations
		public enum TEvent
		{
			EEventGroupTokenReady = 0
		};
		#endregion

		#region Observer interface
		public delegate void MastermindObserver( TEvent aEvent, SymToken aGroupedToken );
		#endregion

		#region Events
		public event MastermindObserver MastermindObservers;
		#endregion

		#region Constructors & destructor
		public SymGrouperMastermind()
		{
		}
		#endregion

		#region Internal enumerations
		private enum TGroupingAction
		{
			ETokenIgnore = -1,
			ETokenMerge = 0,
			ETokenEnqueue,
			ETokenFlushQueue
		};

		[Flags]
		private enum TStateFlag
		{
			EStateFlagUnspecified = 0,
			EStateFlagInQuotation = 1,
			EStateFlagInComment = 2,
			EStateFlagInPreProcessorDirective = 4
		};
		#endregion

		#region API
		public void PerformGrouping()
		{
			SymToken token = NextInputToken();
			//
			while( token != null )
			{
				ProcessToken( token );
				token = NextInputToken();
			}
		}

		public void EnqueueLexedToken( SymToken aToken )
		{
			lock( iLexedTokens )
			{
				iLexedTokens.Enqueue( aToken );
			}
		}
		#endregion

		#region Internal token processors
		private void ProcessToken( SymToken aToken )
		{
			aToken.RefineTokenClass();
			aToken.RefineTokenType();

			if	( InQuotation )
			{
				ProcessTokenDuringQuotation( aToken );
			}
			else if ( InComment )
			{
				ProcessTokenDuringComment( aToken );
			}
			else if ( InPreProcessorDirective )
			{
				ProcessTokenDuringPreProcessorDirective( aToken );
			}
			else
			{
				ProcessTokenDuringNormalOperations( aToken );
			}
		}

		private void ProcessTokenDuringNormalOperations( SymToken aToken )
		{
			// By default we will just add the input token to the
			// pending queue (i.e. no combining/grouping)
			TGroupingAction action = TGroupingAction.ETokenEnqueue;
			//
			if	( iCache.Count == 0 )
			{
				#region The cache is empty - enqueue the token.
				// Starting a new token batch, so just push the token. If
				// its a quotation, then it will be handled during
				// the enqueuing. Pragma symbols must appear as the first
				// item on a line, and will be picked up similarly to quotes.
				if	( aToken.Class == SymToken.TClass.EClassNewLine )
				{
					// If we're adding a new blank line as the first
					// token, we just want to flush it out immediately.
					EnqueueNewOutputToken( aToken );
					action = TGroupingAction.ETokenFlushQueue;
				}
				else
				{
					action = TGroupingAction.ETokenEnqueue;
				}
				#endregion
			}
			else
			{
				#region The cache already has some tokens...
				SymToken previousToken = PreviousOutputToken;
				SymToken.TClass previousTokenClass = previousToken.Class;
				//
				if	( aToken.Class == SymToken.TClass.EClassNewLine )
				{
					#region New line detected...

					// Checking for continuations...
					if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
					{
						// Because of the continuation character, we don't 
						// flush the cache. 

						// Discard new line
						previousToken.Class = SymToken.TClass.EClassContinuation;
						action = TGroupingAction.ETokenIgnore;
					}
					else
					{
						// We never allow new lines to be combined. In fact,
						// they are the signal that we should flush whatever we have
						// cached so far. We must add the new line token
						// first though.
						EnqueueNewOutputToken( aToken );
						action = TGroupingAction.ETokenFlushQueue;
					}
					#endregion
				}
				else if	( previousTokenClass == aToken.Class )
				{
					#region Tokens are the same class - check for combining
					// We group almost all tokens, but some are not permitted
					// to be combined, for example, brackets.
					bool combiningAllowed = previousToken.CombiningAllowed;
					if	( combiningAllowed && aToken.CombiningAllowed )
					{
						// Merge the two tokens
						action = TGroupingAction.ETokenMerge;
					}
					else
					{
						// Treat it as a separate token.
						action = TGroupingAction.ETokenEnqueue;
					}
					#endregion
				}
				else
				{
					#region Handling some other type of token...
					if	( previousTokenClass == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
					{
						// If the last token was a single escaped character, and this next
						// character is not an asterisk or another back slash, then
						// we can try to combine the two.
						if	( !(aToken.Value == "*" || aToken.Value == @"\" ) )
						{
							action = TGroupingAction.ETokenMerge;
						}
						else
						{
							System.Diagnostics.Debug.Assert( false );
						}
					}
					else
					{
						action = TGroupingAction.ETokenEnqueue;
					}
					#endregion
				}
				#endregion
			}

			#region Now perform the action
			switch( action )
			{
				case TGroupingAction.ETokenEnqueue:
					EnqueueNewOutputToken( aToken );
					break;
				case TGroupingAction.ETokenMerge:
					MergeWithPreviousToken( aToken );
					break;
				case TGroupingAction.ETokenFlushQueue:
					FlushCache();
					break;
				default:
				case TGroupingAction.ETokenIgnore:
					break;
			}
			#endregion
		}

		private void ProcessTokenDuringQuotation( SymToken aToken )
		{
//			System.Diagnostics.Debug.Write( "[" + aToken.Value + "] " );
			System.Diagnostics.Debug.Assert( iCache.Count > 0 );

			#region Quotation examples
			//	1)	""
			//	2)	"\""
			//	3)	"\"\""
			//	4)	''
			//	5)	'\''
			//	6)	'\'\''
			//	7)	"\'\'\'\"\""
			//	8)	"abc def ghi"
			//
			//	9)	#define WIBBLE " this is a test string \
			//		This too" " - and this!"
			//
			//	10)	#define WIBBLE2 " this is a test string \\ abc \
			//		This too" " - and this!"
			//
			//  11) #pragma message("Quotation with brackets (;') and other \'nasty\' things! inside it__\\");
			//
			#endregion

			if	( aToken.Class == SymToken.TClass.EClassQuotation )
			{
				#region Token is a quotation ...
				// Quotation symbol whilst already in a quotation.
				// We should check whether we have reached
				// the closing quotation symbol, or then whether
				// this is possibly just an escaped character?
				//
				// See examples 2,3,5,6,7,10,11

				SymToken previousToken = PreviousOutputToken;
				if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
				{
					// Combine the \' or \" with any previous token
					previousToken.ForceCombine( aToken );
					System.Diagnostics.Debug.Assert( iCache.Count > 0 );
				}
				else
				{
					// The last token was not an escape marker, so this
					// is a quotation character all on its own. Since
					// we always start a new cache run when we first see
					// a quotation (during "normal" state), then the
					// first token in the cache forms the basis for the
					// search character.
					// 
					// If the number of tokens in the cache with the same
					// type (as the first token) is even, then we have
					// reached the end of a quotation. If its odd, then
					// we're still inside one.

					SymToken initialQuotationToken = iCache.PeekHead;
					System.Diagnostics.Debug.Assert( initialQuotationToken.Value.Length == 1 );
					System.Diagnostics.Debug.Assert( initialQuotationToken.Class == SymToken.TClass.EClassQuotation );
					System.Diagnostics.Debug.Assert( initialQuotationToken.Type == SymToken.TType.ETypeQuotationDouble || initialQuotationToken.Type == SymToken.TType.ETypeQuotationSingle );

					if	( initialQuotationToken.Value == aToken.Value )
					{
						// Need to check for a closing quotation. The count in the cache
						// should be odd (so that adding aToken makes a balanced set of
						// quotation characters). 
						int count = iCache.CountByType( initialQuotationToken );
						int remainder = count % 2;
						if	( remainder == 1 )
						{
							// Odd number which means that the quotation is treated as complete
							System.Diagnostics.Debug.Assert( aToken.Value == initialQuotationToken.Value );
							EnqueueNewOutputToken( aToken );

							#region Try to group all of the text into a logical string

							// No sense in doing this unless we have more than 3 tokens
							count = iCache.Count;
							if	( count > 3 )
							{
								// Assume we have the following string:
								// "marker.h"
								//
								// This is actually represented as 5 tokens:-
								//
								//	0 ["] => EClassQuotation
								//	1 [marker] => EClassQuotation
								//	2 [.] => EClassQuotation
								//	3 [h] => EClassQuotation
								//	4 ["] => EClassQuotation
								//
								// We need to merge tokens at indicies 1, 2 and 3 into a 
								// single token. 

								iCache.MergeAllTokensWithinRange( 1, count - 1, false, true );
							}
							#endregion

							FlushCache();
						}
						else
						{
							EnqueueNewOutputToken( aToken );
						}
					}
					else
					{
						// It wasn't the closing quotation, so just queue it up
						EnqueueNewOutputToken( aToken );
					}
				}
				#endregion
			}
			else
			{
				#region Token is not a quotation...
				// We'll try to combine the tokens as much as is possible.
				if	( aToken.Class == SymToken.TClass.EClassNewLine )
				{
					#region Handle new line during quotation...
					// Checking for continuations...
					//
					// If the last token was not a backshash marker, then
					// we should flush the cache (reset state).
					SymToken previousToken = PreviousOutputToken;
					if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
					{
						// The last token was an backslash. This means we
						// are dealing with a similar case to examples 9 & 10.

						// Discard new line
						previousToken.Class = SymToken.TClass.EClassContinuation;
					}
					else
					{
						// The last token wasn't a continuation character
						// which means this is a "normal" EOL scenario.
						// Just add the token and flush the cache. Mind you, this actually
						// means the content is invalid.
						EnqueueNewOutputToken( aToken );
						FlushCache();
					}
					#endregion
				}
				else if	( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == @"\" )
				{
					SymToken previousToken = PreviousOutputToken;
					if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
					{
						// Example 10 - an escaped backslash. Combine the 
						// previous token (a backslash) with the new token
						// then join this new combined token with the previous.
						// Phew.
						MergeWithPreviousToken( aToken );
						PreviousOutputToken.Class = SymToken.TClass.EClassQuotation;
					}
					else
					{
						// This should not be combined until we know
						// what the next character is.
						EnqueueNewOutputToken( aToken );
					}
				}
				else
				{
					// Irrespective of what class the token is
					// currently, we treat it as part of a quotation.
					aToken.Class = SymToken.TClass.EClassQuotation;

					// If the previous character wasn't a quotation, 
					EnqueueNewOutputToken( aToken );
				}
				#endregion
			}
		}

		private void ProcessTokenDuringComment( SymToken aToken )
		{
			#region Comment examples
			//		// this is a comment
			//		/* this is also a comment */
			//		// "This is another comment"
			//		// This is a comment with a continuation \
			//		   and here's the rest.
			#endregion

			System.Diagnostics.Debug.Assert( iCache.Count > 0 );

			if	( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == "*" )
			{
				#region Ensure asterisk is not merged with other comments
				// The asterisk character is separated from 
				// the rest of the comment in order that we can
				// ascertain when the end of a block comment has
				// been reached.
				EnqueueNewOutputToken( aToken );
				#endregion
			}
			else if ( aToken.Class == SymToken.TClass.EClassNewLine )
			{
				#region New line during comment...

				// Checking for continuations...
				SymToken previousToken = PreviousOutputToken;
				if	( previousToken.Value == @"\" )
				{
					// Discard new line
					previousToken.Class = SymToken.TClass.EClassContinuation;
				}
				else
				{
					// If we're in a block comment, then we don't flush when we
					// see a new line token.
					SymToken firstToken = iCache.PeekHead;
					EnqueueNewOutputToken( aToken );
					//
					if	( firstToken.Type == SymToken.TType.ETypeCommentFullLine )
					{
						// Flushing the cache resets the flags...
						FlushCache();
					}
					else if ( firstToken.Type == SymToken.TType.ETypeCommentBlock )
					{
						// Don't end the comment until we see the closing block token.
					}
				}
				#endregion
			}
			else if ( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == "/" )
			{
				#region Handle Closing Comment Block [ */ ]
				// For ending a comment region, we must have at least one token
				// already in the cache.
				SymToken previousToken = PreviousOutputToken;

				// Check whether previous token was a "*" - we might be closing a block comment
				if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == "*" )
				{
					// Check whether first token was an opening block
					SymToken firstToken = iCache.PeekHead;
					if	( firstToken.Type == SymToken.TType.ETypeCommentBlock && firstToken.Value == "/*" )
					{
						// End of a block reached. Combine the closing "/" with the asterisk we already
						// have in order to form a closing "*/" block token. 
						previousToken.Combine( aToken );
						previousToken.Class = SymToken.TClass.EClassComment;
						previousToken.Type = SymToken.TType.ETypeCommentBlock;

						// No longer in a comment
						InComment = false;
					}
				}
				#endregion
			}
			else if ( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == @"\" )
			{
				#region Handle possible continuation during comment
				// We treat the possible continuation character as a comment.
				// If the next character that arrives is really a new line, then we change
				// the class to continuation and handle the situation accordingly...
				aToken.Class = SymToken.TClass.EClassComment;
				EnqueueNewOutputToken( aToken );
				#endregion
			}
			else
			{
				aToken.Class = SymToken.TClass.EClassComment;

				if	( PreviousOutputToken.Class == SymToken.TClass.EClassContinuation )
				{
					// In this scenario, we don't want to try to merge the specified token with the previous
					// new line character, since new lines must be left intact. Just enque it, ensuring
					// that the token class is suitably updated.
					EnqueueNewOutputToken( aToken );
				}
				else if ( iCache.Count == 1 )
				{
					// We don't want to merge this token with the first token in the
					// cache, or else we won't be able to successfully identify closing
					// block comments
					EnqueueNewOutputToken( aToken );
				}
				else
				{
					System.Diagnostics.Debug.Assert( PreviousOutputToken.CombiningAllowed );
					ForceMergeWithPreviousToken( aToken );
				}
			}
		}

		private void ProcessTokenDuringPreProcessorDirective( SymToken aToken )
		{
			#region PreProcessor examples
			// 1)		#_ pragma "This is invalid"
			//			
			// 2)		#\
			//			pragma message("hello")
			//			
			// 3)		#\
			//			define TEST
			//			
			// 4)		# \
			//			 define TEST
			//			
			// 5)		# \\
			//			 define INVALID_DEFINE
			//			
			// 6)		# pragma "This is a valid \
			//							pragma which contains a quotation"
			//
			// 7)		#define LOG_FUNC XLeaveDetector __instrument; \ 
			//											TCleanupItem __cleanupItem(XLeaveDetector::LeaveOccurred, &__instrument); \ 
			//											CleanupStack::PushL(__cleanupItem); 
			#endregion

			// NB. We only stay in "preprocessor mode" until we've identified
			// the preprocessor type,i.e. the first non-whitespace word that
			// appears after the initial hash sign.
			bool validPreProcessorDirective = true;
			int cacheCount = iCache.Count;
			System.Diagnostics.Debug.Assert( cacheCount > 0 );
			System.Diagnostics.Debug.Assert( iCache.PeekHead.Class == SymToken.TClass.EClassPreProcessor && iCache.PeekHead.Value == "#" );
			
			// Handle case 5 first of all. If the previous token was a possible
			// continuation, then this next token must be a new line. If its not,
			// then the PP statement is invalid.
			SymToken previousToken = PreviousOutputToken;
			if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
			{
				#region Handle new line character - checking for continuations
				if	( aToken.Class == SymToken.TClass.EClassNewLine )
				{
					previousToken.Class = SymToken.TClass.EClassContinuation;
				}
				else
				{
					// Borked.
					validPreProcessorDirective = false;
				}
				#endregion
			}
			else
			{
				// The next token HAS to be an alphanumeric or then a whitespace.
				// If its not, we're borked.
				if	( aToken.Class == SymToken.TClass.EClassAlphaNumeric && aToken.Type == SymToken.TType.ETypeAlphaNumericNormal )
				{
					#region Handle identified preprocessor command
					// Token was okay - and we can switch back to normal mode
					// now as we've grabbed our preprocessor command.
					aToken.Class = SymToken.TClass.EClassPreProcessor;
					EnqueueNewOutputToken( aToken );
					InPreProcessorDirective = false;
					#endregion
				}
				else if ( aToken.Class == SymToken.TClass.EClassWhiteSpace )
				{
					// Token is okay, but don't change mode yet. We still need an alphanumeric word.
				}
				else if ( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == @"\" )
				{
					#region Handle possible continuation
					// Possibly a valid continuation character prior to seeing the first
					// preprocessor command. For this to be really valid, we must only
					// have seen whitespace between the first token and now.
					bool everythingExceptFirstTokenIsWhiteSpace = iCache.CheckTokensAreOfClass( SymToken.TClass.EClassWhiteSpace, 1 );
					if	( everythingExceptFirstTokenIsWhiteSpace )
					{
						// Could be a continuation character, but only if the next char is a new line
						EnqueueNewOutputToken( aToken );
					}
					else
					{
						// Borked - we've seen non-whitespace. Actually I don't think we can
						// ever come here anyway
						System.Diagnostics.Debug.Assert( false );
						validPreProcessorDirective = false;
					}
					#endregion
				}
				else
				{
					// Something else -> borked.
					validPreProcessorDirective = false;
				}
			}

			#region Handle detection of invalid preprocessor line
			if	( validPreProcessorDirective == false )
			{
				// Token is not valid - this isn't a valid preprocessor directive. 
				// Reset state, update previous character so that its marked as a symbol
				// and bail out.
				InPreProcessorDirective = false;
				iCache.PeekHead.Class = SymToken.TClass.EClassSymbol;
				EnqueueNewOutputToken( aToken );
			}
			#endregion
		}
		#endregion

		#region Internal cache manipulation methods
		private void FlushCache()
		{
#if SHOW_FLUSHED_TOKENS
			StringBuilder debugListing = new StringBuilder();
			foreach( SymToken token in iCache )
			{
				if	( token.Class == SymToken.TClass.EClassNewLine )
				{
					debugListing.Append( "[NL] " );
				}
				else
				{
					debugListing.Append( "[" + token.Value + "] ");
				}
			}
			if	( debugListing.Length > 0 )
			{
				System.Diagnostics.Debug.WriteLine( debugListing.ToString() );
			}
#endif

			foreach( SymToken token in iCache )
			{
				if	( MastermindObservers != null )
				{
					MastermindObservers( TEvent.EEventGroupTokenReady, token );
				}
			}

			iCache.Reset();
			ResetState();
		}

		private SymToken NextInputToken()
		{
			SymToken ret = null;
			//
			lock( iLexedTokens )
			{
				if	( iLexedTokens.Count > 0 )
				{
					ret = iLexedTokens.Dequeue();
				}
			}
			//
			return ret;
		}

		private SymToken PreviousOutputToken
		{
			get
			{
				SymToken ret = SymToken.NullToken();
				if	( iCache.Count > 0 )
				{
					SymToken previousToken = (SymToken) iCache.PeekTail;
					ret = previousToken;
				}
				return ret;
			}
		}

		private void EnqueueNewOutputToken( SymToken aToken )
		{
			if	( CheckIfStateChangeRequiredForEnqueuedToken( aToken ) == false )
			{
				//System.Console.WriteLine( "Enqueue [" + aToken.Value + "]" );
				iCache.Append( aToken );
			}
		}

		private void MergeWithPreviousTwoTokens( SymToken aNewToken, SymToken.TClass aNewClassType )
		{
			System.Diagnostics.Debug.Assert( iCache.Count > 0 );

			SymToken previousToken = iCache.PopTail();

			// Combine it with the new token...
			previousToken.Combine( aNewToken );
			previousToken.Class = aNewClassType;

			// And combine any previous previous token
			MergeWithPreviousToken( previousToken );
		}

		private void MergeWithPreviousToken( SymToken aNewToken )
		{
			if	( iCache.Count > 0 )
			{
				if	( CheckIfStateChangeRequiredForEnqueuedToken( aNewToken ) == false )
				{
					SymToken previousOutputToken = PreviousOutputToken;
					previousOutputToken.Combine( aNewToken );
				}
			}
			else
			{
				EnqueueNewOutputToken( aNewToken );
			}
		}

		private void ForceMergeWithPreviousToken( SymToken aNewToken )
		{
			if	( iCache.Count > 0 )
			{
				if	( CheckIfStateChangeRequiredForEnqueuedToken( aNewToken ) == false )
				{
					SymToken previousOutputToken = PreviousOutputToken;
					previousOutputToken.ForceCombine( aNewToken );
				}
			}
			else
			{
				EnqueueNewOutputToken( aNewToken );
			}
		}
		#endregion

		#region Internal state related methods
		private void ResetState()
		{
			iFlags = TStateFlag.EStateFlagUnspecified;
		}

		private bool CheckIfStateChangeRequiredForEnqueuedToken( SymToken aToken )
		{
			// NB. This method is called before aToken has been enqueued
			// or in the case of combining, before the token has been combined
			// with any prior token.
			bool tokenProcessed = false;

			if	( InQuotation )
			{
			}
			else if ( InComment )
			{
			}
			else if ( InPreProcessorDirective )
			{
			}
			else
			{
				if	( aToken.Class == SymToken.TClass.EClassQuotation )
				{
					#region Handle start of quotation
					if	( iCache.Count > 0 )
					{
						// Check whether the previous symbol was a backslash. If it was
						// then this must be an escaped " or ' character, in which case
						// we don't change state.
						SymToken previousToken = PreviousOutputToken;

						if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == @"\" )
						{
							// Last character was an escape marker. Combine it
							// with the quotation
							previousToken.Combine( aToken );

							// Already handled the token
							tokenProcessed = true;
						}
						else
						{
							// Really are starting a quotation.
							FlushCache();
							InQuotation = true;
						}
					}
					#endregion
				}
				else if ( aToken.Class == SymToken.TClass.EClassSymbol )
				{
					if ( aToken.Value == "*" ) 
					{
						#region Handle Opening comment block [ /* ]
						if	( iCache.Count > 0 )
						{
							SymToken previousToken = PreviousOutputToken;
							//
							if	( previousToken.Class == SymToken.TClass.EClassSymbol && previousToken.Value == "/" ) 
							{
								// "/*" case
								//
								// In this scenario, in order to ensure that we do not
								// flush the first character of our comment marker, we must
								// dequeue the tail item, then flush, then enqueue. 
								SymToken tailToken = iCache.PopTail(); // -> this is the initial "/" that we pop...
								FlushCache();

								// Forward slash and asterisk are combined
								tailToken.Combine( aToken );

								// Mark the token as a full line comment
								tailToken.Class = SymToken.TClass.EClassComment;
								tailToken.Type = SymToken.TType.ETypeCommentBlock;

								// ...and re-added to the cache
								iCache.Append( tailToken );

								// aToken was already combined so we don't want the caller
								// to add it twice.
								tokenProcessed = true;

								// We're now in a full line comment. 
								InComment = true;
							}
						}
						#endregion
					}
					else if ( aToken.Value == "/" )
					{
						#region Handle Full-Line comment [ // ]
						if	( iCache.Count > 0 )
						{
							SymToken previousToken = PreviousOutputToken;
							//
							if	( previousToken.Value == aToken.Value ) 
							{
								// "//" case
								//
								// In this scenario, in order to ensure that we do not
								// flush the first character of our comment marker, we must
								// dequeue the tail item, then flush, then enqueue. 
								SymToken tailToken = iCache.PopTail(); // -> this is the initial "/" that we pop...
								FlushCache();

								// Two forward slashes are combined into one.
								tailToken.Combine( aToken );

								// Mark the token as a full line comment
								tailToken.Class = SymToken.TClass.EClassComment;
								tailToken.Type = SymToken.TType.ETypeCommentFullLine;

								// ...and re-added to the cache
								iCache.Append( tailToken );

								// aToken was already combined so we don't want the caller
								// to add it twice.
								tokenProcessed = true;

								// We're now in a full line comment. 
								InComment = true;
							}
						}
						#endregion
					}
				}
				else if ( aToken.Class == SymToken.TClass.EClassPreProcessor )
				{
					#region Handle start of preprocessor directive
					// Preprocessor directives must only appear on a line
					// after whitespace. If there was any non-whitespace
					// characters before the preprocessor directive, then its illegal.
					bool tokensAreAllWhiteSpace = iCache.CheckTokensAreOfEitherClass( SymToken.TClass.EClassWhiteSpace, SymToken.TClass.EClassNewLine );
					if	( aToken.Value == "#" && tokensAreAllWhiteSpace )
					{
						// Starting a preprocess directive
						FlushCache();
						InPreProcessorDirective = true;
					}
					#endregion
				}
			}

			return tokenProcessed;
		}

		#endregion

		#region Internal state properties
		private bool InQuotation
		{
			get
			{
				bool ret = ( ( iFlags & TStateFlag.EStateFlagInQuotation ) == TStateFlag.EStateFlagInQuotation );
				return ret;
			}
			set
			{
				if	( value )
				{
					iFlags |= TStateFlag.EStateFlagInQuotation;
				}
				else
				{
					iFlags &= ~TStateFlag.EStateFlagInQuotation;
				}
			}
		}

		private bool InComment
		{
			get
			{
				bool ret = ( ( iFlags & TStateFlag.EStateFlagInComment ) == TStateFlag.EStateFlagInComment );
				return ret;
			}
			set
			{
				if	( value )
				{
					iFlags |= TStateFlag.EStateFlagInComment;
				}
				else
				{
					iFlags &= ~TStateFlag.EStateFlagInComment;
				}
			}
		}

		private bool InPreProcessorDirective
		{
			get
			{
				bool ret = ( ( iFlags & TStateFlag.EStateFlagInPreProcessorDirective ) == TStateFlag.EStateFlagInPreProcessorDirective );
				return ret;
			}
			set
			{
				if	( value )
				{
					iFlags |= TStateFlag.EStateFlagInPreProcessorDirective;
				}
				else
				{
					iFlags &= ~TStateFlag.EStateFlagInPreProcessorDirective;
				}
			}
		}
		#endregion

		#region Data members
		private SymLexedTokens iLexedTokens = new SymLexedTokens();
		private SymGrouperMastermindCache iCache = new SymGrouperMastermindCache();
		private TStateFlag iFlags = TStateFlag.EStateFlagUnspecified;
		#endregion
	}
}
