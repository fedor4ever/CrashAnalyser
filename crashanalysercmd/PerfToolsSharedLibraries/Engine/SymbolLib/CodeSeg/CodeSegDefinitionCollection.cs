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
	public class CodeSegDefinitionCollection : IEnumerable<CodeSegDefinition>
	{
		#region Constructors & destructor
		public CodeSegDefinitionCollection()
			: this( 10 )
		{
		}

		public CodeSegDefinitionCollection( int aGranularity )
		{
			iEntries = new List<CodeSegDefinition>( aGranularity );
		}
		#endregion

		#region API
		public void Reset()
		{
			iEntries.Clear();
		}

		public void Add( CodeSegDefinition aNewEntry )
		{
            if ( aNewEntry.ImageFileName.Length == 0 )
            {
                throw new ArgumentException( "Invalid code seg definition entry" );
            }

            // Our check-for-exists predicate
            Predicate<CodeSegDefinition> existsPredicate = delegate( CodeSegDefinition aEntry )
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

        public void Remove( CodeSegDefinition aRemoveEntry )
        {
            if ( aRemoveEntry.ImageFileName.Length == 0 )
            {
                throw new ArgumentException( "Invalid code seg definition entry" );
            }

            // Our check-for-match predicate
            Predicate<CodeSegDefinition> matchPredicate = delegate( CodeSegDefinition aEntry )
            {
                return aEntry.ImageFileName.ToLower() == aRemoveEntry.ImageFileName.ToLower();
            };

            // Remove any matching entries
            iEntries.RemoveAll( matchPredicate );
        }

        public int IndexOf( CodeSegDefinition aEntry )
        {
            int index = -1;
            int i = 0;
            //
            foreach ( CodeSegDefinition entry in iEntries )
            {
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

		public void SortByFileName()
		{
			iEntries.Sort( new CodeSegDefinitionCollectionCompareByFileName() );
		}

		public void SortByAddress()
		{
			iEntries.Sort( new CodeSegDefinitionCollectionCompareByAddress() );
		}

        public CodeSegDefinition DefinitionByAddress( long aAddress )
        {
            CodeSegDefinition ret = null;
            //
            foreach ( CodeSegDefinition entry in iEntries )
            {
                if ( entry.AddressRange.Contains( aAddress ) )
                {
                    ret = entry;
                    break;
                }
            }
            //
            return ret;
        }

		public CodeSegDefinition FindByCodeSegmentFileNameAndPath( string aImageCodeSegmentFileNameAndPath )
		{
			CodeSegDefinition temp = new CodeSegDefinition( string.Empty, aImageCodeSegmentFileNameAndPath );
			int index = iEntries.IndexOf( temp );

			int x = 0;
			if	( x != 0 )
			{
				int count = iEntries.Count;
				for( int i=0; i<count; i++ )
				{
					CodeSegDefinition e = this[ i ];
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

        public void MergeInto( CodeSegDefinitionCollection aItems )
        {
            foreach ( CodeSegDefinition entry in aItems )
            {
                Add( entry );
            }
        }
		#endregion

		#region Properties
		public int Count
		{
			get { return iEntries.Count; }
		}

		public CodeSegDefinition this[ int aIndex ]
		{
			get { return iEntries[ aIndex ]; }
		}
		#endregion

        #region IEnumerable Members
        public IEnumerator GetEnumerator()
		{
			return new CodeSegDefinitionCollectionEnumerator( this );
		}

        IEnumerator<CodeSegDefinition> IEnumerable<CodeSegDefinition>.GetEnumerator()
        {
            return new CodeSegDefinitionCollectionEnumerator( this );
        }
		#endregion

		#region Data members
		private readonly List<CodeSegDefinition> iEntries;
		#endregion
    }
}
