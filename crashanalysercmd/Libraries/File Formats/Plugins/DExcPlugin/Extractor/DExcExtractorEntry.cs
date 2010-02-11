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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DExcPlugin.Extractor
{
	internal class DExcExtractorEntry
    {
        #region Constructors
        public static DExcExtractorEntry NewMatchAndSave( Regex aExpression )
        {
            DExcExtractorEntry ret = new DExcExtractorEntry( aExpression );
            return ret;
        }

        public static DExcExtractorEntry NewMatchAndTransition( Regex aExpression, DExcExtractor.TState aNewState )
        {
            DExcExtractorEntry ret = new DExcExtractorEntry( aExpression, TType.ETypeMatchAndTransition, aNewState );
            return ret;
        }

        public static DExcExtractorEntry NewMatchSaveAndTransition( Regex aExpression, DExcExtractor.TState aNewState )
        {
            DExcExtractorEntry ret = new DExcExtractorEntry( aExpression, TType.ETypeMatchSaveAndTransition, aNewState );
            return ret;
        }
        #endregion

        #region Internal constructors
        private DExcExtractorEntry( Regex aExpression )
            : this( aExpression, TType.ETypeMatchAndSave, DExcExtractor.TState.EStateIdle )
		{
		}

        private DExcExtractorEntry( Regex aExpression, TType aType, DExcExtractor.TState aNewState )
        {
            iType = aType;
            iNewState = aNewState;
            iExpression = aExpression;
        }
        #endregion

		#region API
        public bool Offer( string aLine, long aLineNumber, DExcExtractorList aList, DExcExtractor aInterpreter )
        {
            Match m = iExpression.Match( aLine );
            //
            if ( m.Success )
            {
                if ( Type == TType.ETypeMatchAndTransition || Type == TType.ETypeMatchSaveAndTransition )
                {
                    aInterpreter.State = iNewState;
                }
                if ( Type == TType.ETypeMatchAndSave )
                {
                    aList.Add( m.Value );
                }
                else if ( Type == TType.ETypeMatchSaveAndTransition )
                {
                    // We have just transitioned state and we must add the line
                    // to the new state's list
                    if ( aInterpreter.CurrentList != null )
                    {
                        aInterpreter.CurrentList.Add( m.Value );
                    }
                }
            }
            //
            return m.Success;
		}
		#endregion

		#region Properties
        private TType Type
        {
            get { return iType; }
        }
		#endregion

        #region Internal enumerations
        private enum TType
        {
            ETypeMatchAndSave,
            ETypeMatchAndTransition,
            ETypeMatchSaveAndTransition
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iExpression.ToString();
        }
        #endregion

        #region Data members
        private readonly TType iType;
        private readonly Regex iExpression;
        private readonly DExcExtractor.TState iNewState;
        #endregion
    }
}
