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
    public class TraceTimeStamp : IFormattable
    {
        #region Constructors
        public TraceTimeStamp( ulong aNanoSeconds )
        {
            iValue = aNanoSeconds;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public ulong NanoSeconds
        {
            get { return iValue; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = ToString( "full", null );
            return ret;
        }
        #endregion

        #region From IFormattable
        public string ToString( string aFormat, IFormatProvider aFormatProvider )
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( aFormat == "nanoseconds" )
            {
                ret.AppendFormat( "[{0:d}]", iValue );
            }
            else
            {
                try
                {
                    ulong milliseconds = iValue / KNanosecondsToMilliseconds;
                    ulong ticks = milliseconds * (ulong) TimeSpan.TicksPerMillisecond;
                    
                    // Now we have a time span, but it includes the rather ugly number of days
                    // and also an inaccurate number of microseconds.
                    TimeSpan ts = new TimeSpan( (long) ticks );
                    
                    // First with discard all the milli seconds
                    ts = ts.Subtract( new TimeSpan( 0, 0, 0, 0, ts.Milliseconds ) );

                    // Next, we calculate the microseconds fraction
                    ulong oneNsInSeconds = KNanosecondsToMilliseconds * 1000;
                    ulong totalNsInSeconds = (ulong) ( ts.TotalSeconds * oneNsInSeconds );
                    ulong nsLeftOver = iValue % totalNsInSeconds;
                    float usFraction = (float) nsLeftOver / (float) oneNsInSeconds;

                    // Now discard the number of days - we're not interested in this at all.
                    ts = ts.Subtract( new TimeSpan( ts.Days, 0, 0, 0, 0 ) );

                    // Finally, we can assemble the time stamp. First we'll add in the
                    // hh:mm:ss part, and there will be no ms, so the decimal aspect is
                    // entirely missing at this stage.
                    ret.Append( ts.ToString() );

                    // Next, we'll add back in the fractional part, ignoring the leading 0 prefix
                    // (i.e. the non fraction part of the string representation of a fractional number, 
                    // e.g. in 0.05, we throw away the leading 0, to leave .05).
                    string fraction = usFraction.ToString( "0.00000000" ).Substring( 1 );
                    ret.Append( fraction );
                }
                catch
                {
                    // Fall back
                    ret.Append( this.ToString() );
                }
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Internal constants
        private const ulong KNanosecondsToMilliseconds = 1000000;

	    private const int TEN = 10;
	    private const int THOUSAND = 1000;
	    private const int HOURS_IN_DAY = 24;
	    private const int MINUTES_IN_HOUR = 60;
	    private const int SECONDS_IN_MINUTE = 60;
	    private const int MILLISECS_IN_SECOND = THOUSAND;
	    private const ulong MILLISECS_IN_MINUTE = MILLISECS_IN_SECOND * SECONDS_IN_MINUTE;
	    private const ulong MILLISECS_IN_HOUR = MILLISECS_IN_MINUTE * MINUTES_IN_HOUR;
	    private const ulong MILLISECS_IN_DAY = MILLISECS_IN_HOUR * HOURS_IN_DAY;
        #endregion

        #region Data members
        private ulong iValue;
        #endregion
    }
}
