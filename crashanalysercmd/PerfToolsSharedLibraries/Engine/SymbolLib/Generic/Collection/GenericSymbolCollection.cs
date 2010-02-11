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
using System.Collections;
using System.Collections.Generic;
using SymbolLib.Utils;
using SymbianUtils.Range;

namespace SymbolLib.Generics
{
	public abstract class GenericSymbolCollection : IEnumerable<GenericSymbol>, IComparable<GenericSymbolCollection>
	{
		#region Construct & destruct
		protected GenericSymbolCollection( string aHostBinaryFileName )
		{
            HostBinaryFileName = aHostBinaryFileName;
		}
		#endregion

        #region Virtual API
        public virtual void ClearTag()
        {
            Tagged = false;
        }

        public virtual void WriteToStream( StreamWriter aWriter )
        {
            // First write the binary name
            aWriter.WriteLine( string.Empty );
            aWriter.WriteLine( "From    " + HostBinaryFileName );
            aWriter.WriteLine( string.Empty );

            foreach ( GenericSymbol symbol in this )
            {
                aWriter.WriteLine( symbol.ToStringForStream() );
            }
        }
        #endregion

        #region Abstract API - methods
        public abstract void Add( GenericSymbolEngine aEngine, GenericSymbol aSymbol );

		public abstract void Remove( GenericSymbol aSymbol );

		public abstract void RemoveAt( int aIndex );

        public abstract IEnumerator CreateEnumerator();

        public abstract IEnumerator<GenericSymbol> CreateGenericEnumerator();

        public abstract GenericSymbol SymbolForAddress( long aAddress );
        #endregion

		#region Abstract API - properties
		public abstract int Count
		{
			get;
		}

		public abstract GenericSymbol this[ int aIndex ]
		{
			get;
		}

		public abstract void Sort();
		#endregion

		#region API
		public bool AddressFallsWithinRange( long aAddress )
		{
            bool found = false;
            //
            if ( this.Count == 1 && this[ 0 ].IsUnknownSymbol )
            {
                int x = 0;
                x++;
            }

            if ( aAddress > 0 )
            {
                if ( iAddresses == null )
                {
                    RebuildAddressRange();
                }
                //
                found = iAddresses.IsWithinRange( aAddress );
            }
            //
			return found;
		}

#if DEBUG
        public void Dump()
        {
            int i = 0;
            string line = string.Empty;
            foreach ( GenericSymbol entry in this )
            {
                line = i.ToString( "d8" ) + "   [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Symbol.ToString();
                System.Diagnostics.Debug.WriteLine( line );
                i++;
            }
        }

        public void Dump( long aAddress )
        {
            int i = 0;
            string line = string.Empty;
            foreach ( GenericSymbol entry in this )
            {
                if ( entry.FallsWithinDomain( aAddress ) )
                {
                    line = i.ToString( "d8" ) + " * [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Symbol.ToString();
                }
                else
                {
                    line = i.ToString( "d8" ) + "   [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Symbol.ToString();
                }
                System.Diagnostics.Debug.WriteLine( line );
                i++;
            }
        }
#endif
        #endregion

		#region Symbol Properties
		public GenericSymbol FirstSymbol
		{
			get
			{
				GenericSymbol ret = null;
				if ( Count > 0 )
				{
					ret = this[ 0 ];
				}
				return ret;
			}
		}

		public GenericSymbol LastSymbol
		{
			get
			{
				GenericSymbol ret = null;
				if ( Count > 0 )
				{
					ret = this[ Count - 1 ];
				}
				return ret;
			}
		}
		#endregion

		#region Misc Properties
        public bool Tagged
        {
            get { return iTagged; }
            set { iTagged = value; }
        }

        public long BaseAddress
        {
            get { return iBaseAddress; }
            internal set { iBaseAddress = value; }
        }

		public string HostBinaryFileName
		{
            get { return iHostBinaryFileName; }
			set
            {
                System.Diagnostics.Debug.Assert( !value.ToLower().Contains( ".map" ) );
                iHostBinaryFileName = value; 
            }
		}

		public bool HostBinaryExists
		{
            get { return File.Exists( HostBinaryFileName ); }
		}

		public virtual long AddressRangeStart
		{
			get
			{
				long ret = 0;
				//
				if ( Count > 0 )
				{
					ret = FirstSymbol.Address;
				}
				//
				return ret;
			}
		}

		public virtual long AddressRangeEnd
		{
			get
			{
				long ret = 0xffffffff;
				//
				if ( Count > 0 )
				{
					ret = LastSymbol.EndAddress;
				}
				//
				return ret;
			}
		}

        public virtual AddressRange AddressRange
        {
            get { return new AddressRange( AddressRangeStart, AddressRangeEnd ); }
        }

        public string SourceFile
        {
            get { return iSourceFile; }
            internal set { iSourceFile = value; }
        }
		#endregion

        #region Internal methods
        protected void RebuildAddressRange()
        {
            iAddresses = new SymbolAddressRange( this );
        }
        #endregion

        #region From IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return CreateEnumerator();
        }

        IEnumerator<GenericSymbol> IEnumerable<GenericSymbol>.GetEnumerator()
        {
            return CreateGenericEnumerator();
        }
        #endregion

		#region From IComparable
        public int CompareTo( GenericSymbolCollection aOther )
		{
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = string.Compare( HostBinaryFileName, aOther.HostBinaryFileName, true );
                //
                if ( aOther is GenericSymbolCollection )
                {
                    GenericSymbolCollection otherCol = (GenericSymbolCollection) aOther;
                    //
                    if ( BaseAddress == otherCol.BaseAddress )
                    {
                        ret = 0;
                    }
                    else if ( BaseAddress > otherCol.BaseAddress )
                    {
                        ret = 1;
                    }
                    else
                    {
                        ret = -1;
                    }
                }
            }
            
            // Debug check
            if ( aOther == this )
            {
                System.Diagnostics.Debug.Assert( ret == 0 );
            }
            //
            return ret;
		}
		#endregion

        #region From System.Object
        public override string ToString()
        {
            return string.Format( "{0} {1}", AddressRange, iHostBinaryFileName );
        }
        #endregion
        
        #region Data members
        private bool iTagged = false;
        private long iBaseAddress = 0;
        private string iHostBinaryFileName = string.Empty;
        private string iSourceFile = string.Empty;
        private SymbolAddressRange iAddresses = null;
		#endregion
    }
}
