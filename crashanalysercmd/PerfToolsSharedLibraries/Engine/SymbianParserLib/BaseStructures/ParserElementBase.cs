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

namespace SymbianParserLib.BaseStructures
{
    public abstract class ParserElementBase
    {
        #region Internal enumerations
        [Flags]
        internal enum TElementBaseFlags
        {
            EElementBaseFlagsNone = 0,
            EElementBaseFlagsIsDisabled = 1,
            EElementBaseFlagsIsNeverEnding = 2,
            EElementBaseFlagsIsComplete = 4,
            EElementBaseFlagsSetDisabledWhenComplete = 8,
        }
        #endregion

        #region Delegates and events
        public delegate void ElementCompleteHandler( ParserElementBase aElement );
        public event ElementCompleteHandler ElementComplete;
        #endregion

        #region Constructors
        protected ParserElementBase()
            : this( string.Empty )
        {
        }

        protected ParserElementBase( string aName )
            : this( aName, null )
        {
        }

        protected ParserElementBase( string aName, object aTag )
            : this( aName, null, aTag )
        {
        }

        protected ParserElementBase( ParserElementBase aParent )
            : this( string.Empty, aParent, null )
        {
        }

        protected ParserElementBase( string aName, ParserElementBase aParent, object aTag )
        {
            iName = aName;
            iParent = aParent;
            iTag = aTag;

            // Default behaviour is to disable the item when it is complete 
            DisableWhenComplete = true;
        }
        #endregion

        #region Abstract API
        internal abstract ParserResponse Offer( ref string aLine );

        internal virtual string CurrentLine
        {
            get { return string.Empty; }
        }

        internal virtual void OnDisableWhenComplete()
        {
        }

        internal virtual void OnNeverEnding()
        {
        }
        #endregion

        #region API
        public string GetCurrentLine()
        {
            string line = string.Empty;
            //
            ParserElementBase element = this;
            while ( line == string.Empty && element != null )
            {
                line = element.CurrentLine;
                element = element.Parent;
            }
            //
            return line;
        }

        public void SetRepetitions( int aValue )
        {
            iRepetitions = aValue;
            IsComplete = ( aValue == 0 ) && !IsNeverEnding;
            IsDisabled = ( IsComplete && DisableWhenComplete );
        }
        #endregion

        #region Properties
        public bool DisableWhenComplete
        {
            get { return ( iElementFlags & TElementBaseFlags.EElementBaseFlagsSetDisabledWhenComplete ) == TElementBaseFlags.EElementBaseFlagsSetDisabledWhenComplete; }
            set
            {
                if ( value )
                {
                    iElementFlags |= TElementBaseFlags.EElementBaseFlagsSetDisabledWhenComplete;
                }
                else
                {
                    iElementFlags &= ~TElementBaseFlags.EElementBaseFlagsSetDisabledWhenComplete;
                }

                OnDisableWhenComplete();
            }
        }

        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        public int Repetitions
        {
            get { return iRepetitions; }
        }

        public bool IsDisabled
        {
            get { return ( iElementFlags & TElementBaseFlags.EElementBaseFlagsIsDisabled ) == TElementBaseFlags.EElementBaseFlagsIsDisabled; }
            set
            {
                if ( value )
                {
                    iElementFlags |= TElementBaseFlags.EElementBaseFlagsIsDisabled;
                }
                else
                {
                    iElementFlags &= ~TElementBaseFlags.EElementBaseFlagsIsDisabled;
                }
            }
        }

        public bool IsNeverEnding
        {
            get { return ( iElementFlags & TElementBaseFlags.EElementBaseFlagsIsNeverEnding ) == TElementBaseFlags.EElementBaseFlagsIsNeverEnding; }
            set
            {
                if ( value )
                {
                    iElementFlags |= TElementBaseFlags.EElementBaseFlagsIsNeverEnding;
                }
                else
                {
                    iElementFlags &= ~TElementBaseFlags.EElementBaseFlagsIsNeverEnding;
                }

                OnNeverEnding();
            }
        }

        public bool IsComplete
        {
            get { return ( iElementFlags & TElementBaseFlags.EElementBaseFlagsIsComplete ) == TElementBaseFlags.EElementBaseFlagsIsComplete; }
            set
            {
                bool isComplete = false;

                // Decrement the number of repetitions and if the final value is zero
                // then the line is 'really' complete.
                if ( value == true )
                {
                    if ( !IsNeverEnding )
                    {
                        --iRepetitions;
                    }

                    isComplete = ( iRepetitions <= 0 ) || IsNeverEnding;
                }
                else
                {
                    isComplete = value;
                }

                // Final value update
                if ( isComplete )
                {
                    iElementFlags |= TElementBaseFlags.EElementBaseFlagsIsComplete;
                }
                else
                {
                    iElementFlags &= ~TElementBaseFlags.EElementBaseFlagsIsComplete;
                }

                if ( isComplete )
                {
                    if ( DisableWhenComplete )
                    {
                        IsDisabled = true;
                    }

                    OnElementComplete();
                }

                // If we're never ending, then the item is never really complete
                if ( IsNeverEnding )
                {
                    iElementFlags &= ~TElementBaseFlags.EElementBaseFlagsIsComplete;
                }
            }
        }

        public ParserElementBase Parent
        {
            get { return iParent; }
            set { iParent = value; }
        }
        #endregion

        #region Internal methods
        protected void OnElementComplete()
        {
            if ( ElementComplete != null )
            {
                ElementComplete( this );
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Data members
        private string iName;
        private object iTag = null;
        private TElementBaseFlags iElementFlags = TElementBaseFlags.EElementBaseFlagsNone;
        private int iRepetitions = 1;
        private ParserElementBase iParent = null;
        #endregion
    }
}
