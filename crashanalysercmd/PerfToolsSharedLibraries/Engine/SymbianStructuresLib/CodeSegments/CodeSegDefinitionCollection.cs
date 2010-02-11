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

namespace SymbianStructuresLib.CodeSegments
{
	public class CodeSegDefinitionCollection : IEnumerable<CodeSegDefinition>
	{
		#region Constructors
		public CodeSegDefinitionCollection()
			: this( 10 )
		{
		}

		public CodeSegDefinitionCollection( int aGranularity )
		{
			iEntries = new List<CodeSegDefinition>( aGranularity );
		}

        public CodeSegDefinitionCollection( IEnumerable<CodeSegDefinition> aCopy )
        {
            iEntries = new List<CodeSegDefinition>();
            iEntries.AddRange( aCopy );
        }
		#endregion

		#region API
		public void Reset()
		{
            lock ( iEntries )
            {
                iEntries.Clear();
            }
		}

        public void Add( CodeSegDefinition aEntry )
		{
            if ( string.IsNullOrEmpty( aEntry.FileName ) )
            {
                throw new ArgumentException( "File name invalid" );
            }

            lock ( iEntries )
            {
                if ( !Contains( aEntry ) )
                {
                    iEntries.Add( aEntry );
                }
            }
		}

        public void AddRange( IEnumerable<CodeSegDefinition> aItems )
        {
            foreach ( CodeSegDefinition entry in aItems )
            {
                Add( entry );
            }
        }

        public void Remove( CodeSegDefinition aEntry )
        {
            lock ( iEntries )
            {
                int index = IndexOf( aEntry );
                if ( index >= 0 )
                {
                    iEntries.RemoveAt( index );
                }
            }
        }

		public void SortByFileName()
		{
            lock ( iEntries )
            {
                iEntries.Sort( new Internal.CSDCompareByFileName() );
            }
		}

		public void SortByAddress()
		{
            lock ( iEntries )
            {
                iEntries.Sort( new Internal.CSDCompareByAddress() );
            }
        }

        public bool Contains( string aFileName )
        {
            lock ( iEntries )
            {
                CodeSegDefinition temp = new CodeSegDefinition( aFileName );
                return Contains( temp );
            }
        }

        public bool Contains( CodeSegDefinition aEntry )
        {
            lock ( iEntries )
            {
                int index = IndexOf( aEntry );
                bool ret = ( index >= 0 );
                return ret;
            }
        }

        public int IndexOf( CodeSegDefinition aEntry )
        {
            Predicate<CodeSegDefinition> predicate = delegate( CodeSegDefinition search )
            { 
                return search.Equals( aEntry ); 
            };
            //
            lock ( iEntries )
            {
                int ret = iEntries.FindIndex( predicate );
                return ret;
            }
        }

        public CodeSegDefinition DefinitionByAddress( uint aAddress )
        {
            Predicate<CodeSegDefinition> predicate = delegate( CodeSegDefinition validate )
            { 
                return validate.Contains( aAddress );
            };
            //
            lock ( iEntries )
            {
                CodeSegDefinition ret = iEntries.Find( predicate );
                return ret;
            }
        }
		#endregion

		#region Properties
		public int Count
		{
			get
            {
                lock ( iEntries )
                {
                    return iEntries.Count;
                }
            }
		}

		public CodeSegDefinition this[ int aIndex ]
		{
            get
            {
                lock ( iEntries )
                {
                    return iEntries[ aIndex ];
                }
            }
		}

        public CodeSegDefinition this[ uint aAddress ]
        {
            get
            {
                CodeSegDefinition ret = null;
                //
                foreach ( CodeSegDefinition entry in iEntries )
                {
                    if ( entry.Contains( aAddress ) )
                    {
                        ret = entry;
                        break;
                    }
                }
                //
                return ret;
            }
        }
		#endregion

        #region IEnumerable Members
        public IEnumerator<CodeSegDefinition> GetEnumerator()
        {
            foreach ( CodeSegDefinition entry in iEntries )
            {
                yield return entry;
            }
        }
 
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( CodeSegDefinition entry in iEntries )
            {
                yield return entry;
            }
        }
        #endregion

		#region Data members
		private readonly List<CodeSegDefinition> iEntries;
		#endregion
    }
}
