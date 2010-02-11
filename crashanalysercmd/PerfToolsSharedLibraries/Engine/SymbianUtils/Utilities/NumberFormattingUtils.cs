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

namespace SymbianUtils
{
	public class NumberFormattingUtils
	{
		public static string NumberAsPercentageInt( long aValue, long aOneHundredPercentValue )
		{
			float percent = (float) aValue / (float) aOneHundredPercentValue;
			percent *= (float) 100.0;
			//
			int percentAsInt = Math.Min( 100, ( (int) ( percent + 0.5 ) ) );
			string ret = percentAsInt.ToString();
			return ret;
		}

		public static string NumberAsPercentageOneDP( long aValue, long aOneHundredPercentValue )
		{
			double percent = (double) aValue / (double) aOneHundredPercentValue;
			percent *= (double) 100.0;
			//
			string ret = percent.ToString("##0.0");
			return ret;
		}

		public static string NumberAsPercentageTwoDP( long aValue, long aOneHundredPercentValue )
		{
			double percent = (double) aValue / (double) aOneHundredPercentValue;
			percent *= (double) 100.0;
			//
			string ret = percent.ToString("##0.00");
			return ret;
		}

		public static string NumberAsPercentageThreeDP( long aValue, long aOneHundredPercentValue )
		{
			double percent = (double) aValue / (double) aOneHundredPercentValue;
			percent *= (double) 100.0;
			//
			string ret = percent.ToString("##0.000");
			return ret;
		}
	}

}
