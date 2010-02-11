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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianStackLib.Data.Output.Entry;

namespace SymbianStackLib.Data.Output
{
    public class StackOutputData : IEnumerable<StackOutputEntry>
    {
        #region Delegates & events
        public delegate void EntryCreatedHandler( StackOutputEntry aEntry );
        public event EntryCreatedHandler EntryCreated;
        #endregion

        #region Constructors
        public StackOutputData()
		{
		}
		#endregion

		#region API
        public void Clear()
        {
            iEntries.Clear();
            IsAccurate = false;
            AlgorithmName = string.Empty;
        }

        public string ToString( bool aHideNonSymbols, bool aHideGhosts )
        {
            StringBuilder text = new StringBuilder();
            //
            StackOutputEntry lastElement = null;
            foreach ( StackOutputEntry element in this )
            {
                bool includeEntry = IncludeEntry( element, aHideNonSymbols, aHideGhosts );
                if ( includeEntry )
                {
                    if ( lastElement != null && lastElement.IsOutsideCurrentStackRange && element.IsOutsideCurrentStackRange == false && !element.IsRegisterBasedEntry )
                    {
                        text.Append( System.Environment.NewLine );
                        text.Append( " >>>> Current Stack Pointer >>>> " + System.Environment.NewLine );
                        text.Append( System.Environment.NewLine );
                    }

                    string line = element.ToString() + Environment.NewLine;
                    text.Append( line );
                    //
                    lastElement = element;
                }
            }
            //
            return text.ToString();
        }

        internal void InsertAsFirstEntry( StackOutputEntry aEntry )
        {
            iEntries.Insert( 0, aEntry );

            if ( EntryCreated != null )
            {
                EntryCreated( aEntry );
            }
        }

        internal void Add( StackOutputEntry aEntry )
        {
            iEntries.Add( aEntry );
            //
            if ( EntryCreated != null )
            {
                EntryCreated( aEntry );
            }
        }
		#endregion

		#region Properties
        public int Count
        {
            get { return iEntries.Count; }
        }

        public bool IsAccurate
        {
            get { return iIsAccurate; }
            set { iIsAccurate = value; }
        }

        public string AlgorithmName
        {
            get { return iAlgorithmName; }
            set { iAlgorithmName = value; }
        }

        public StackOutputEntry this[ int aIndex ]
        {
            get { return iEntries[ aIndex ]; }
        }
		#endregion

		#region Internal methods
        private static bool IncludeEntry( StackOutputEntry aElement, bool aHideNonSymbols, bool aHideGhosts )
        {
            bool passedGhostCheck = true;
            bool passedNonSymbolCheck = true;
         
            // Check whether we should exclude ghosts
            if ( aHideGhosts )
            {
                if ( aElement.IsGhost )
                {
                    // We definitely hide these...
                    passedGhostCheck = false;
                }
                else if ( aElement.Symbol == null )
                {
                    // We also hide all of these
                    passedGhostCheck = false;
                }
            }

            // Check whether we should exclude symbols which are NULL
            if ( aHideNonSymbols )
            {
                if ( aElement.Symbol == null && aElement.AssociatedBinary == string.Empty )
                {
                    passedNonSymbolCheck = false;
                }
            }

            // Some entries override everything
            bool ret = ( passedGhostCheck && passedNonSymbolCheck );
            if ( aElement.IsCurrentStackPointerEntry || aElement.IsRegisterBasedEntry )
            {
                ret = true;
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<StackOutputEntry>
        public IEnumerator<StackOutputEntry> GetEnumerator()
        {
            foreach ( StackOutputEntry entry in iEntries )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( StackOutputEntry entry in iEntries )
            {
                yield return entry;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach ( StackOutputEntry entry in this )
            {
                ret.AppendLine( entry.ToString() );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private bool iIsAccurate = false;
        private string iAlgorithmName = string.Empty;
        private List<StackOutputEntry> iEntries = new List<StackOutputEntry>();
        #endregion
    }
}
