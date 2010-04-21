/*
* Copyright (c) 2010 Nokia Corporation and/or its subsidiary(-ies). 
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
* The class HtmlFormatter is used to format text to HTML format.
* 
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace ErrorLibrary
{
    class HtmlFormatter
    {
        /**
         * Formats a description for a panic category in html format
         * @param categoryName category name
         * @param categoryDescription category description
         * @return category description in html format
         */
        public static string formatCategoryDescription(string categoryName, string categoryDescription)
        {
            return BOLD + categoryName + BOLD_END + BREAK + BREAK + categoryDescription;
        }

        /**
         * Formats a description for an error in html format
         * @param errorName error name
         * @param errorDescription error description 
         * @return error description in html format
         */
        public static string formatErrorDescription(string errorName, string errorDescription)
        {
            return BOLD + errorName + "  " + errorDescription + BOLD_END + BREAK + BREAK;
        }

        /**
         * Formats a description for a panic in html format
         * @param panicName panic name
         * @param panicDescription panic description
         * @return panic description in html format
         */
        public static string formatPanicDescription(string panicName, string panicDescription)
        {
            return BOLD + panicName + BOLD_END + BREAK + panicDescription + BREAK + BREAK;
        }

        /**
         * Formats a panic name to html format
         * @param categoryName category name
         * @param panicId panic id
         * @return panic name in html format
         */
        public static string formatPanicName(string categoryName, string panicId)
        {
            return BREAK + BREAK + BOLD + categoryName + " - " + panicId + BOLD_END + BREAK;
        }

        /**
         * 
         * @param component
         * @return
         */
        public static string formatErrorComponent(string component)
        {
            if (component.Trim().Length < 1)
                return "";
            return BREAK + BREAK + BOLD + "Error Component:" + BOLD_END + BREAK + component;
        }

	    private static string BOLD = "<b>";
	    private static string BOLD_END = "</b>";
	    private static string BREAK = "<br>";
    }
}
