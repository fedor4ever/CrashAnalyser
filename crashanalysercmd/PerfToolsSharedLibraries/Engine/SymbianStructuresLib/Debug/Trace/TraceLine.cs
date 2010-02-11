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
using System.IO;
using System.Text;
using SymbianUtils;
using SymbianUtils.Threading;

namespace SymbianStructuresLib.Debug.Trace
{
    public class TraceLine : IFormattable
    {
        #region Enumerations
        public enum TType
        {
            /// <summary>
            /// Plain text trace data
            /// </summary>
            ETypeText = 0,

            /// <summary>
            /// Binary trace data that has been decoded back to text
            /// </summary>
            ETypeBinary,

            /// <summary>
            /// Raw trace data is (usually) binary data that could not be decoded
            /// </summary>
            ETypeRaw
        }
        #endregion

        #region Factory
        public static TraceLine NewText( TraceTimeStamp aTimeStamp, string aPayload )
        {
            TraceLine ret = new TraceLine( aTimeStamp, aPayload, TType.ETypeText );
            return ret;
        }

        public static TraceLine NewRaw( TraceTimeStamp aTimeStamp, byte[] aRawData )
        {
            TraceLine ret = new TraceLine( aTimeStamp, aRawData );
            return ret;
        }

        public static TraceLine NewBinary( TraceTimeStamp aTimeStamp, string aPayload )
        {
            TraceLine ret = new TraceLine( aTimeStamp, aPayload, TType.ETypeBinary );
            return ret;
        }
        #endregion

        #region Constructors
        static TraceLine()
        {
            TraceIdentifier temp = new TraceIdentifier( 0, 0, 0 );
            iTraceIdentifierWidthInCharacters = temp.ToString().Length;
        }

        private TraceLine( TraceTimeStamp aTimeStamp, string aPayload, TType aType )
        {
            iTimeStamp = aTimeStamp;
            iPayload = aPayload;
            iType = aType;
        }

        private TraceLine( TraceTimeStamp aTimeStamp, byte[] aRawData )
            : this( aTimeStamp, string.Empty, TType.ETypeRaw )
        {
            StringBuilder payload = new StringBuilder();
            //
            foreach ( byte b in aRawData )
            {
                payload.AppendFormat( "{0:x2} ", b );
            }
            //
            iPayload = payload.ToString();
        }
        #endregion

        #region API
        public void AddPrefix( int aOrder, string aValue )
        {
            if ( string.IsNullOrEmpty( aValue ) == false )
            {
                TXffix item = new TXffix( aOrder, aValue );
                InsertInSortedOrder( ref iPrefixes, item );
            }
        }

        public void AddSuffix( int aOrder, string aValue )
        {
            if ( string.IsNullOrEmpty( aValue ) == false )
            {
                TXffix item = new TXffix( aOrder, aValue );
                InsertInSortedOrder( ref iSuffixes, item );
            }
        }
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
        }

        public string Prefix
        {
            get
            {
                string ret = XffixListToString( iPrefixes );
                return ret;
            }
        }

        public string Suffix
        {
            get
            {
                string ret = XffixListToString( iSuffixes );
                return ret;
            }
        }

        public string Payload
        {
            get { return iPayload; }
        }

        public uint ContextId
        {
            get { return iContextId; }
            set { iContextId = value; }
        }

        public bool HasIdentifier
        {
            get { return iIdentifier != null; }
        }

        public TraceTimeStamp TimeStamp
        {
            get { return iTimeStamp; }
        }

        public TraceIdentifier Identifier
        {
            get { return iIdentifier; }
            set { iIdentifier = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();

            // First comes time stamp
            ret.Append( iTimeStamp.ToString() + " " );

            // Prefix with type
            switch ( iType )
            {
            case TType.ETypeBinary:
                ret.Append( "[BIN]" );
                break;
            case TType.ETypeRaw:
                ret.Append( "[RAW]" );
                break;
            case TType.ETypeText:
                ret.Append( "[ASC]" );
                break;
            default:
                ret.Append( "[???]" );
                break;
            }
            ret.Append( " " );

            // Context Id (if available)
            if ( iContextId != 0 )
            {
                ret.AppendFormat( "THD: [{0:x8}]", iContextId );
            }
            else
            {
                ret.Append( "              " );
            }
            ret.Append( " " );

            // Add identifier if present
            if ( iIdentifier != null )
            {
                ret.Append( "ID: " + iIdentifier.ToString() );
            }
            else
            {
                ret.Append( "     " );
                ret.Append( "".PadRight( iTraceIdentifierWidthInCharacters, ' ' ) );
            }

            // Text aspect
            ret.Append( this.GetText() );

            // Done
            return ret.ToString();
        }
        #endregion

        #region From IFormattable
        public string ToString( string aFormat, IFormatProvider formatProvider )
        {
            string ret = string.Empty;
            //
            if ( aFormat.ToUpper() == "TEXT" )
            {
                ret = this.GetText();
            }
            else
            {
                ret = this.ToString();
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        #endregion

        #region Internal class
        private class TXffix : IComparer<TXffix>
        {
            #region Constructors
            public TXffix( int aOrder, string aValue )
            {
                iOrder = aOrder;
                iValue = aValue;
            }
            #endregion

            #region From System.Object
            public override string ToString()
            {
                return iValue;
            }
            #endregion

            #region From IComparer<TXffix>
            public int Compare( TXffix aLeft, TXffix aRight )
            {
                int ret = aLeft.iOrder.CompareTo( aRight.iOrder );
                return ret;
            }
            #endregion

            #region Data members
            private readonly int iOrder;
            private readonly string iValue;
            #endregion
        }
        #endregion

        #region Internal methods
        private void InsertInSortedOrder( ref List<TXffix> aList, TXffix aValue )
        {
            if ( aList == null )
            {
                aList = new List<TXffix>();
                aList.Add( aValue );
            }
            else
            {
                int pos = aList.BinarySearch( aValue, aValue as IComparer<TXffix> );
                if ( pos < 0 )
                {
                    pos = ~pos;
                }
                aList.Insert( pos, aValue );
            }
        }

        private string XffixListToString( List<TXffix> aList )
        {
            string ret = string.Empty;
            //
            if ( aList != null )
            {
                StringBuilder sb = new StringBuilder();
                //
                foreach ( TXffix item in aList )
                {
                    sb.Append( item );
                    sb.Append( " " );
                }
                //
                ret = sb.ToString();
            }
            //
            return ret;
        }

        private string GetText()
        {
            StringBuilder ret = new StringBuilder();

            // Now prefix, payload, suffix
            string prefix = XffixListToString( iPrefixes );
            string suffix = XffixListToString( iSuffixes );
            
            ret.AppendFormat( "    {0}{1}{2}", prefix, iPayload, suffix );

            return ret.ToString();
        }
        #endregion

        #region Data members
        private static int iTraceIdentifierWidthInCharacters;
        private readonly TraceTimeStamp iTimeStamp;
        private readonly string iPayload;
        private readonly TType iType;
        private uint iContextId = 0;
        private List<TXffix> iPrefixes = null;
        private List<TXffix> iSuffixes = null;
        private TraceIdentifier iIdentifier = null;
        #endregion
    }
}
