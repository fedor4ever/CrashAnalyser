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

namespace SymbianUtils.Range
{
	public class AddressRange : IComparable<AddressRange>
    {
        #region Delegates & events
        public delegate void ChangeHandler( AddressRange aAddressRange );
        public event ChangeHandler RangeChanged; 
        #endregion

        #region Constructors
        public AddressRange()
		{
		}

        public AddressRange( string aSpecifier )
        {
            this.Parse( aSpecifier );
        }

        public AddressRange( uint aMin, uint aMax )
        {
            iMin = aMin;
            iMax = aMax;
        }

        public AddressRange( long aMin, long aMax )
            : this( (uint) aMin, (uint) aMax )
        {
        }

        public AddressRange( AddressRange aCopy )
            : this( aCopy.Min, aCopy.Max )
        {
        }
        #endregion

        #region API
        public void Set( uint aMin, uint aMax )
        {
            iMax = aMax;
            iMin = aMin;
        }

        public bool Contains( uint aAddress )
        {
            bool ret = ( aAddress >= iMin && aAddress <= iMax );
            return ret;
        }

        public bool Contains( long aAddress )
        {
            bool ret = Contains( (uint) aAddress );
            return ret;
        }

        public bool Contains( AddressRange aRange )
        {
            bool ret = Contains( aRange.Min ) && Contains( aRange.Max );
            return ret;
        }

        public void Reset()
        {
            iMin = uint.MaxValue;
            iMax = uint.MinValue;
            //
            OnChanging();
        }

        public void Update( AddressRange aRange )
        {
            AddressRange transaction = TransactionBegin();
            //
            UpdateMin( aRange.Min );
            UpdateMax( aRange.Max );
            //
            TransactionEnd( transaction );
        }

        public void UpdateMin( uint aProposedMin )
        {
            uint old = iMin;
            iMin = Math.Min( iMin, aProposedMin );
            //
            if ( old != iMin )
            {
                OnChanging();
            }
        }

        public void UpdateMax( uint aProposedMax )
        {
            uint old = iMax;
            iMax = Math.Max( iMax, aProposedMax );
            //
            if ( old != iMax )
            {
                OnChanging();
            }
        }

        public void UpdateMin( long aProposedMin )
        {
            UpdateMin( (uint) aProposedMin );
        }

        public void UpdateMax( long aProposedMax )
        {
            UpdateMax( (uint) aProposedMax );
        }

        public void Parse( string aValue )
        {
            Match m = KRegEx.Match( aValue );
            if ( m.Success )
            {
                string min = m.Groups[ "Min" ].Value.Replace( "0x", string.Empty );
                string max = m.Groups[ "Max" ].Value.Replace( "0x", string.Empty );

                // Also need to know if they are hex or decimal specifiers
                int baseMin = ( m.Groups[ 0 ].Value == "0x" ? 16 : 10 );
                int baseMax = ( m.Groups[ 1 ].Value == "0x" ? 16 : 10 );

                // Now convert
                uint valueMin = System.Convert.ToUInt32( min, baseMin );
                uint valueMax = System.Convert.ToUInt32( max, baseMax );

                // Make range...
                AddressRange transaction = TransactionBegin();
                //
                UpdateMin( valueMin );
                UpdateMin( valueMax );
                //
                TransactionEnd( transaction );
            }
            else
            {
                throw new ArgumentException( "Invalid range format - expected [0x]AAA - [0x]BBB" );
            }
        }
        #endregion

        #region Properties
        public uint Min
        {
            get { return iMin; }
            set 
            {
                if ( iMin != value )
                {
                    iMin = value;
                    OnChanging();
                }
            }
        }

        public uint Max
        {
            get { return iMax; }
            set
            {
                if ( iMax != value )
                {
                    iMax = value;
                    OnChanging();
                }
            }
        }

        public uint Size
        {
            get 
            { 
                uint ret = Max - Min + 1;
                return ret;
            }
        }

        public bool IsValid
        {
            get
            {
                bool goodMin = ( iMin != uint.MaxValue );
                bool goodMax = ( iMax != uint.MinValue );
                //
                return ( goodMin && goodMax );
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0:x8}-{1:x8}", Min, Max );
            return ret.ToString();
        }

        public override int GetHashCode()
        {
            return (int) ( Min ^ Max );
        }

        public override bool Equals( object aObject )
        {
            bool ret = false;
            //
            if ( aObject != null )
            {
                if ( aObject is AddressRange )
                {
                    AddressRange other = (AddressRange) aObject;
                    //
                    int compareMin = other.Min.CompareTo( this.Min );
                    int compareMax = other.Max.CompareTo( this.Max );
                    //
                    ret = ( compareMin == 0 && compareMax == 0 );
                }
                else
                {
                    ret = base.Equals( aObject );
                }
            }
            //
            return ret;
        }
        #endregion

        #region Operators
        public static implicit operator AddressRange( string aSpecifier )
        {
            AddressRange ret = new AddressRange( aSpecifier );
            return ret;
        }
        #endregion

        #region Internal regular expressions
        private static readonly Regex KRegEx = new Regex(
              "(?<Min>(?((0x))0x[A-Fa-f0-9]{1,8}|[0-9]{1,8}))\\s*\\-\\s*(?<"+
              "Max>(?((0x))0x[A-Fa-f0-9]{1,8}|[0-9]{1,8}))",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region From IComparable<AddressRange5>
        public int CompareTo( AddressRange aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = this.Min.CompareTo( aOther.Min );
            }
            //
            return ret;
        }
        #endregion

        #region Internal methods
        private void OnChanging()
        {
            if ( !InTransaction )
            {
                OnChanged();
            }
        }

        protected virtual void OnChanged()
        {
            if ( RangeChanged != null )
            {
                RangeChanged( this );
            }
        }

        private bool InTransaction
        {
            get { return iTransactionCount > 0; }
        }

        private AddressRange TransactionBegin()
        {
            ++iTransactionCount;
            //
            AddressRange ret = new AddressRange( this.Min, this.Min );
            return ret;
        }

        private void TransactionEnd( AddressRange aTransaction )
        {
            System.Diagnostics.Debug.Assert( iTransactionCount > 0 );
            --iTransactionCount;
            //
            if ( aTransaction.CompareTo( this ) != 0 )
            {
                OnChanging();
            }
        }
        #endregion

        #region Data members
        private uint iMin = uint.MaxValue;
        private uint iMax = uint.MinValue;
        private int iTransactionCount = 0;
        #endregion
    }
}
