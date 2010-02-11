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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SymbianParserLib.RegExTranslators;
using SymbianParserLib.BaseStructures;

namespace SymbianParserLib.Elements
{
    public class ParserParagraph : ParserElementBaseWithValueStore, IEnumerable<ParserLine>
    {
        #region Constructors
        public ParserParagraph( string aName )
            : base( aName )
        {
        }
        #endregion

        #region API
        public void Add( ParserLine aLine )
        {
            aLine.Parent = this;
            aLine.ElementComplete += new ElementCompleteHandler( Line_ElementComplete );
            iLines.Add( aLine );
        }

        public void Add( params ParserLine[] aLines )
        {
            foreach ( ParserLine line in aLines )
            {
                Add( line );
            }
        }

        public void Remove( ParserLine aLine )
        {
            aLine.ElementComplete -= new ElementCompleteHandler( Line_ElementComplete );
            iLines.Remove( aLine );
        }
        #endregion

        #region Properties
        public ParserLine this[ int aIndex ]
        {
            get { return iLines[ aIndex ]; }
        }

        public int Count
        {
            get { return iLines.Count; }
        }
        #endregion

        #region From ParserElementBase
        internal override ParserResponse Offer( ref string aLine )
        {
            ParserResponse ret = new ParserResponse();
            //
            foreach ( ParserLine line in this )
            {
                if ( !line.IsDisabled )
                {
                    ret = line.Offer( ref aLine );
                    if ( ret.Type != ParserResponse.TResponseType.EResponseTypeUnhandled )
                    {
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region From ParserElementBaseWithValueStore
        internal override void SetTargetProperty( object aPropertyObject, string aPropertyName, int aIndex )
        {
            if ( aIndex == ParserElementBaseWithValueStore.KGloballyApplicable )
            {
            }
            else
            {
                int cumulativeFieldCount = 0;
                int count = iLines.Count;
                for ( int i = 0; i < count; i++ )
                {
                    ParserLine line = this[ i ];
                    int fieldCountForLine = line.Count;
                    int lastFieldIndexWithinLine = cumulativeFieldCount + fieldCountForLine;
                    //
                    if ( aIndex < fieldCountForLine )
                    {
                        int index = aIndex - cumulativeFieldCount;
                        line.SetTargetProperty( aPropertyObject, aPropertyName, index );
                        break;
                    }
                    //
                    cumulativeFieldCount += fieldCountForLine;
                }
            }
        }
        #endregion

        #region Event handlers
        void Line_ElementComplete( ParserElementBase aElement )
        {
            int completeCount = 0;
            foreach( ParserLine line in iLines )
            {
                if ( line.IsComplete && !line.IsNeverEnding )
                {
                    ++completeCount;
                }
            }
            //
            IsComplete = ( completeCount == Count );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From IEnumerable<ParserLine>
        public IEnumerator<ParserLine> GetEnumerator()
        {
            return new ParserParagraphEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ParserParagraphEnumerator( this );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private List<ParserLine> iLines = new List<ParserLine>();
        #endregion
    }

    #region Enumerator
    internal class ParserParagraphEnumerator : IEnumerator<ParserLine>
    {
        #region Constructors
        public ParserParagraphEnumerator( ParserParagraph aObject )
        {
            iObject = aObject;
        }
        #endregion

        #region IEnumerator Members
        public void Reset()
        {
            iCurrentIndex = -1;
        }

        public object Current
        {
            get
            {
                return iObject[ iCurrentIndex ];
            }
        }

        public bool MoveNext()
        {
            return ( ++iCurrentIndex < iObject.Count );
        }
        #endregion

        #region From IEnumerator<ParserLine>
        ParserLine IEnumerator<ParserLine>.Current
        {
            get { return iObject[ iCurrentIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

        #region Data members
        private readonly ParserParagraph iObject;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
