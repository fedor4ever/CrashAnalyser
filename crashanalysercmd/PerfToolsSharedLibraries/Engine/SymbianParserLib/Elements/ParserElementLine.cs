//#define SHOW_EACH_LINE
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
using System.Text.RegularExpressions;
using System.Reflection;
using SymbianParserLib.Utilities;
using SymbianParserLib.RegExTranslators;
using SymbianParserLib.BaseStructures;
using SymbianParserLib.ValueStores;

namespace SymbianParserLib.Elements
{
    public class ParserLine : ParserElementBaseWithValueStore, IEnumerable<ParserField>
    {
        #region Enumerations
        public enum TLineType
        {
            ELineTypeSimpleStringMatch = 0,
            ELineTypeSymbianFormatString,
            ELineTypeRegEx
        }

        [Flags]
        internal enum TLineFlags
        {
            ELineFlagNone = 0,
            ELineFlagDequeueIfComplete = 1,
            ELineFlagNeverConsumesLine = 2
        }
        #endregion

        #region Static constructors
        public static ParserLine New( string aText )
        {
            ParserLine self = new ParserLine( TLineType.ELineTypeSimpleStringMatch );
            self.OriginalValue = ParserUtils.RemoveLineEndings( aText );
            return self;
        }
        
        public static ParserLine NewSymFormat( string aFormat )
        {
            // First check with cache
            ParserLine self = RegExTranslatorManager.PreCachedCompiledEntry( aFormat );
            if ( self == null )
            {
                // Wasn't cached so we need to parse and create a new entry
                self = new ParserLine( TLineType.ELineTypeSymbianFormatString );
                self.OriginalValue = aFormat;
                //
                RegExTranslatorManager.CompileToRegularExpression( self );
            }
            //
            return self;
        }

        public static ParserLine NewRegEx( string aRegEx )
        {
            throw new NotSupportedException();
        }

        internal static ParserLine NewCopy( ParserLine aLine )
        {
            ParserLine ret = new ParserLine( aLine );
            return ret;
        }
        #endregion

        #region Constructors
        private ParserLine( TLineType aLineType )
        {
            iLineType = aLineType;
        }

        private ParserLine( ParserLine aLine )
        {
            iLineType = aLine.LineType;
            iOriginalValue = aLine.OriginalValue;
            iFinalValue = new StringBuilder( aLine.FinalValue );
            iFlags = aLine.iFlags;

            foreach ( ParserField templateField in aLine )
            {
                ParserField copy = new ParserField( templateField );
                Add( copy );
            }

            CreateRegEx();
        }
        #endregion

        #region API
        public void Add( ParserField aField )
        {
            aField.Parent = this;
            aField.DisableWhenComplete = this.DisableWhenComplete;
            iFields.Add( aField );
        }

        internal void Finalise()
        {
            if ( Count != 0 )
            {
                FinalValue = OriginalValue;

                // Fixup the final string to be a new "dynamically" generated regular expression. Work
                // backwards since we adjust the string by index and therefore we must not affect earlier
                // indexes when forming the replacement.
                for( int i=Count-1; i>=0; i-- )
                {
                    ParserField field = this[ i ];
                    //
                    string regex = field.FormatSpecifier.RegularExpressionString;
                    int origPos = field.FormatSpecifier.OriginalLocation;
                    int origLen = field.FormatSpecifier.OriginalLength;
                    iFinalValue.Remove( origPos, origLen );
                    iFinalValue.Insert( origPos, regex );
                }

                CreateRegEx();
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iFields.Count; }
        }

        public ParserField this[ int aIndex ]
        {
            get { return iFields[ aIndex ]; }
        }

        public ParserParagraph Paragraph
        {
            get
            {
                ParserParagraph ret = null;
                //
                if ( Parent != null && Parent is ParserParagraph )
                {
                    ret = Parent as ParserParagraph;
                }
                //
                return ret;
            }
        }

        public bool DequeueIfComplete
        {
            get { return ( iFlags & TLineFlags.ELineFlagDequeueIfComplete ) == TLineFlags.ELineFlagDequeueIfComplete; }
            set
            {
                if ( value )
                {
                    iFlags |= TLineFlags.ELineFlagDequeueIfComplete;
                }
                else
                {
                    iFlags &= ~TLineFlags.ELineFlagDequeueIfComplete;
                }
            }
        }

        public bool NeverConsumesLine
        {
            get { return ( iFlags & TLineFlags.ELineFlagNeverConsumesLine ) == TLineFlags.ELineFlagNeverConsumesLine; }
            set
            {
                if ( value )
                {
                    iFlags |= TLineFlags.ELineFlagNeverConsumesLine;
                }
                else
                {
                    iFlags &= ~TLineFlags.ELineFlagNeverConsumesLine;
                }
            }
        }

        public string OriginalValue
        {
            get { return iOriginalValue; }
            set { iOriginalValue = value; }
        }

        public TLineType LineType
        {
            get { return iLineType; }
        }

        internal string FinalValue
        {
            get { return FinalValueBuilder.ToString(); }
            set { iFinalValue = new StringBuilder( value ); }
        }

        internal StringBuilder FinalValueBuilder
        {
            get { return iFinalValue; }
            set { iFinalValue = value; }
        }
        #endregion

        #region From ParserElementBase
        internal override ParserResponse Offer( ref string aLine )
        {
            ParserResponse ret = new ParserResponse();
            //
            if ( iLineType == TLineType.ELineTypeSimpleStringMatch )
            {
                bool match = ( aLine.Contains( OriginalValue ) );
                if ( match )
                {
#if SHOW_EACH_LINE
                    System.Diagnostics.Debug.WriteLine( aLine );
#endif
                    ret = new ParserResponse( ParserResponse.TResponseType.EResponseTypeHandled );
                }
            }
            else if ( iLineType == TLineType.ELineTypeSymbianFormatString )
            {
                if ( iFinalRegEx != null )
                {
                    Match m = iFinalRegEx.Match( aLine );
                    bool match = m.Success;
                    if ( match )
                    {
#if SHOW_EACH_LINE
                        System.Diagnostics.Debug.WriteLine( aLine );
#endif

                        GroupCollection groups = m.Groups;
                        ExtractValues( groups );

                        ret = new ParserResponse( ParserResponse.TResponseType.EResponseTypeHandled );
                    }
                }
            }

            // Update completion - will trigger observers
            IsComplete = ret.WasHandled;

            if ( IsComplete )
            {
                // Dequeue the line from parent paragraph if it is
                // complete and so adorned
                if ( DequeueIfComplete )
                {
                    Paragraph.Remove( this );
                }

                // If this object never consumes the input string, then
                // instead throw a nonconsuming exception, and the engine
                // will re-offer it to all objects
                if ( NeverConsumesLine )
                {
                    ret = new ParserResponse( ParserResponse.TResponseType.EResponseTypeHandledByRequiresReProcessing );
                }
            }

            return ret;
        }

        internal override void OnDisableWhenComplete()
        {
            base.OnDisableWhenComplete();
            //
            foreach( ParserField f in iFields )
            {
                f.DisableWhenComplete = this.DisableWhenComplete;
            }
        }

        internal override void OnNeverEnding()
        {
            base.OnNeverEnding();
            //
            foreach ( ParserField f in iFields )
            {
                f.IsNeverEnding = this.IsNeverEnding;
            }
        }
        #endregion

        #region From ParserElementBaseWithValueStore
        public override void SetTargetObject()
        {
            base.SetTargetObject();
            foreach ( ParserField field in this )
            {
                field.SetTargetObject();
            }
        }

        internal override void SetTargetProperty( object aPropertyObject, string aPropertyName, int aIndex )
        {
            if ( aIndex == ParserElementBaseWithValueStore.KGloballyApplicable )
            {
                // Applicable to all
                iValueStore = new ValueStore();
                iValueStore.SetTargetProperty( aPropertyObject, aPropertyName );
            }
            else
            {
                // Specific to a field
                if ( aIndex < 0 || aIndex >= Count )
                {
                    throw new ArgumentOutOfRangeException( "aIndex" );
                }

                this[ aIndex ].SetTargetProperty( aPropertyObject, aPropertyName );
            }
        }
        #endregion

        #region Internal methods
        private void CreateRegEx()
        {
            iFinalRegEx = new Regex( FinalValue, RegexOptions.Singleline );
        }

        private void ExtractValues( GroupCollection aGroups )
        {
            for ( int i = 1; i < aGroups.Count; i++ )
            {
                Group group = aGroups[ i ];
                if ( group.Success )
                {
                    int pos = group.Index;
                    string value = group.Value;
                    //
                    if ( i <= Count )
                    {
                        ParserField field = this[ i - 1 ];
                        field.ExtractValue( group );
                    }
                }
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region From IEnumerable<ParserField>
        public IEnumerator<ParserField> GetEnumerator()
        {
            foreach ( ParserField field in iFields )
            {
                yield return field;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( ParserField field in iFields )
            {
                yield return field;
            }
        }
        #endregion

        #region Data members
        private readonly TLineType iLineType;
        private string iOriginalValue = string.Empty;
        private StringBuilder iFinalValue = new StringBuilder();
        private Regex iFinalRegEx = null;
        private TLineFlags iFlags = TLineFlags.ELineFlagNone;
        private List<ParserField> iFields = new List<ParserField>();
        #endregion
    }
}
