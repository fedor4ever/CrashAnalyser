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
	internal class DExcExtractorList : IEnumerable<string>
	{
        #region Constructors
        public DExcExtractorList( DExcExtractor.TState aState, DExcExtractorListType aType )
		{
            iType = aType;
            iState = aState;
		}
		#endregion

		#region API
        public void AddExpression( DExcExtractorEntry aExpression )
        {
            iEntries.Add( aExpression );
        }

        public void AddExpressions( params DExcExtractorEntry[] aExpressions )
        {
            foreach ( DExcExtractorEntry exp in aExpressions )
            {
                AddExpression( exp );
            }
        }

        public virtual void Add( string aLine )
        {
            iLines.Add( aLine );
        }

        public virtual bool Offer( string aLine, long aLineNumber, DExcExtractor aInterpreter )
        {
            bool handled = false;
            //
            foreach ( DExcExtractorEntry interpreter in iEntries )
            {
                handled = interpreter.Offer( aLine, aLineNumber, this, aInterpreter );
                if ( handled )
                {
                    break;
                }
            }
            //
            return handled;
        }
		#endregion

		#region Properties
        public int Count
		{
			get { return iLines.Count; }
		}

        public string[] Lines
        {
            get { return iLines.ToArray(); }
        }

        public DExcExtractorListType Type
        {
            get { return iType; }
        }

        public DExcExtractor.TState State
        {
            get { return iState; }
        }
		#endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach ( string line in iLines )
            {
                ret.AppendLine( line );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region From IEnumerable<string>
        public IEnumerator<string> GetEnumerator()
        {
            foreach ( string line in iLines )
            {
                yield return line;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( string line in iLines )
            {
                yield return line;
            }
        }
        #endregion

        #region Data members
        private readonly DExcExtractorListType iType;
        private readonly DExcExtractor.TState iState;
        private List<string> iLines = new List<string>();
        private List<DExcExtractorEntry> iEntries = new List<DExcExtractorEntry>();
        #endregion
    }
}
