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
using System.Collections;
using System.Collections.Generic;

namespace SymbolLib.CodeSegDef
{
	public class CodeSegResolverEntryCollection : IEnumerable<CodeSegResolverEntry>
	{
		#region Constructors & destructor
		public CodeSegResolverEntryCollection()
			: this( 10 )
		{
		}

        public CodeSegResolverEntryCollection( int aGranularity )
		{
            iEntries = new List<CodeSegResolverEntry>( aGranularity );
		}
		#endregion

		#region API
		public void Reset()
		{
			iEntries.Clear();
		}

        public void Add( CodeSegResolverEntry aNewEntry )
		{
            if ( aNewEntry.ImageFileName.Length == 0 )
            {
                throw new ArgumentException( "Invalid code seg definition entry" );
            }

            // Our check-for-exists predicate
            Predicate<CodeSegResolverEntry> existsPredicate = delegate( CodeSegResolverEntry aEntry )
            {
                return aEntry.ImageFileName.ToLower() == aNewEntry.ImageFileName.ToLower();
            };

            // Check whether there is already an entry that matches
            if ( !iEntries.Exists( existsPredicate ) )
            {
                iEntries.Add( aNewEntry );
            }
            else
            {
#if TRACE_RESOLVER
                System.Diagnostics.Debug.WriteLine( "IGNORING DUPE CodeSegDefinition: " + aNewEntry.ToString() );
#endif
            }
		}

        public void Remove( CodeSegResolverEntry aRemoveEntry )
        {
            if  ( String.IsNullOrEmpty( aRemoveEntry.ImageFileName ) )
            {
                // Remove by environment file name
                Predicate<CodeSegResolverEntry> matchPredicate = delegate( CodeSegResolverEntry aEntry )
                {
                    return aEntry.EnvironmentFileName.ToLower() == aRemoveEntry.EnvironmentFileName.ToLower();
                };
                iEntries.RemoveAll( matchPredicate );
            }
            else
            {
                // Remove by phone file name
                Predicate<CodeSegResolverEntry> matchPredicate = delegate( CodeSegResolverEntry aEntry )
                {
                    return aEntry.ImageFileName.ToLower() == aRemoveEntry.ImageFileName.ToLower();
                };
                iEntries.RemoveAll( matchPredicate );
            }
        }

        public int IndexOf( CodeSegResolverEntry aEntry )
        {
            int index = -1;
            //
            int count = iEntries.Count;
            for( int i=0; i<count; i++ )
            {
                CodeSegResolverEntry entry = iEntries[ i ];
                //
                if ( entry.EnvironmentFileNameAndPath.ToLower() == aEntry.EnvironmentFileNameAndPath.ToLower() )
                {
                    index = i;
                    break;
                }
                ++i;
            }
            //
            return index;
        }

        public void Sort()
        {
            iEntries.Sort( new Comparison<CodeSegResolverEntry>( Compare ) );
        }

        public CodeSegResolverEntry FindByCodeSegmentFileNameAndPath( string aImageCodeSegmentFileNameAndPath )
		{
            CodeSegResolverEntry temp = new CodeSegResolverEntry( string.Empty, aImageCodeSegmentFileNameAndPath );
			int index = iEntries.IndexOf( temp );

			int x = 0;
			if	( x != 0 )
			{
				int count = iEntries.Count;
				for( int i=0; i<count; i++ )
				{
                    CodeSegResolverEntry e = this[ i ];
#if TRACE_RESOLVER
					System.Diagnostics.Debug.WriteLine( "Entry = ENV[ " + e.EnvironmentFileName + " => " + e.EnvironmentFileNameAndPath + " ] IMG[ " + e.ImageFileName + " => " + e.ImageFileNameAndPath + " ]" );
#endif

				}
			}

			if	( index >= 0 && index < Count )
			{
				temp = this[ index ];
			}
			else
			{
				// Not found
				temp = null;
			}
			//
			return temp;
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iEntries.Count; }
		}

        public CodeSegResolverEntry this[ int aIndex ]
		{
			get { return iEntries[ aIndex ]; }
		}
		#endregion

        #region IEnumerable<CodeSegResolverEntry> Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( CodeSegResolverEntry entry in iEntries )
            {
                yield return entry;
            }
        }

        IEnumerator<CodeSegResolverEntry> IEnumerable<CodeSegResolverEntry>.GetEnumerator()
        {
            foreach ( CodeSegResolverEntry entry in iEntries )
            {
                yield return entry;
            }
        }
        #endregion

        #region Internal methods
        private static int Compare( CodeSegResolverEntry aLeft, CodeSegResolverEntry aRight )
        {
            return aLeft.EnvironmentFileNameAndPath.CompareTo( aRight.EnvironmentFileNameAndPath );
        }
        #endregion

        #region Data members
        private readonly List<CodeSegResolverEntry> iEntries;
		#endregion
    }
}
