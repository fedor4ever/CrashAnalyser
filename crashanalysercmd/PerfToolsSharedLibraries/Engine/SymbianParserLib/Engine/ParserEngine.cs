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
using SymbianParserLib.Enums;
using SymbianParserLib.Elements;
using SymbianParserLib.Exceptions;
using SymbianParserLib.BaseStructures;
using SymbianParserLib.RegExTranslators;

namespace SymbianParserLib.Engine
{
    public class ParserEngine : BaseStructures.ParserElementBase, IEnumerable<ParserParagraph>
    {
        #region Delegates and events
        public event BaseStructures.ParserElementBase.ElementCompleteHandler ParagraphComplete;
        #endregion

        #region Constructors
        public ParserEngine()
        {
        }
        #endregion

        #region API
        public bool OfferLine( ref string aLine )
        {
            iCurrentLine = aLine;
            //
            ParserResponse response = Offer( ref aLine );
            bool ret = response.WasHandled;
            //
            return ret;
        }

        public void Reset()
        {
            iParagraphs.Clear();
        }

        public void Remove( ParserParagraph aParagraph )
        {
            aParagraph.ElementComplete -= new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( Paragraph_ElementComplete );
            iParagraphs.Remove( aParagraph );
        }

        public void RemoveRange( int aStartAt )
        {
            int count = iParagraphs.Count - aStartAt;
            iParagraphs.RemoveRange( aStartAt, count );
        }

        public void Add( params ParserParagraph[] aParagraphs )
        {
            foreach ( ParserParagraph para in aParagraphs )
            {
                Add( para );
            }
        }

        public void Add( ParserParagraph aParagraph )
        {
            aParagraph.Parent = this;
            aParagraph.ElementComplete += new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( Paragraph_ElementComplete );
            iParagraphs.Add( aParagraph );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iParagraphs.Count; }
        }

        public ParserParagraph this[ int aIndex ]
        {
            get { return iParagraphs[ aIndex ]; }
        }
        #endregion

        #region From ParserElementBase
        internal override string CurrentLine
        {
            get { return iCurrentLine; }
        }

        internal override ParserResponse Offer( ref string aLine )
        {
            ++iLineNumber;
            //
            ParserResponse ret = new ParserResponse();
            //
            do
            {
                ret = TryToConsumeLine( ref aLine );
            }
            while ( ret.Type == ParserResponse.TResponseType.EResponseTypeHandledByRequiresReProcessing );
            //
            return ret;
        }
        #endregion

        #region Internal methods
        private ParserResponse TryToConsumeLine( ref string aLine )
        {
            ParserResponse ret = new ParserResponse();
            //
            foreach ( ParserParagraph paragraph in iParagraphs )
            {
                if ( !paragraph.IsDisabled )
                {
                    ret = paragraph.Offer( ref aLine );
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

        #region Internal constants
        #endregion

        #region From IEnumerable<ParserParagraph>
        public IEnumerator<ParserParagraph> GetEnumerator()
        {
            return new ParserEngineEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ParserEngineEnumerator( this );
        }
        #endregion

        #region Internal event handlers
        void Paragraph_ElementComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            if ( ParagraphComplete != null )
            {
                ParagraphComplete( aElement );
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private long iLineNumber = 0;
        private string iCurrentLine = string.Empty;
        private List<ParserParagraph> iParagraphs = new List<ParserParagraph>();
        #endregion
    }

    #region Enumerator
    internal class ParserEngineEnumerator : IEnumerator<ParserParagraph>
    {
        #region Constructors
        public ParserEngineEnumerator( ParserEngine aObject )
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

        #region From IEnumerator<ParserParagraph>
        ParserParagraph IEnumerator<ParserParagraph>.Current
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
        private readonly ParserEngine iObject;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
