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
* The class ErrorLibraryError holds information of the one error in error library.
* 
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace ErrorLibrary
{
    class ErrorLibraryError
    {
        /**
         * Constructor
         * @param name error name
         * @param description error description
         */
        public ErrorLibraryError(string key, string name, string description)
        {
            searchKey = key;
            errorName = name;
            errorDescription = description;
        }

        /**
         * Constructor
         */
        public ErrorLibraryError()
        {
            // for empty ErrorLibraryError creation
        }

        /**
         * Set name for an error
         * @param name error name
         */
        public void SetName(string name)
        {
            errorName = name;
        }

        /**
         * Set description for an error.
         * @param description error description
         */
        public void SetDescription(string description)
        {
            errorDescription = description;
        }

        /**
         * Adds information to an error description
         * @param description error description
         */
        public void AddToDescription(string description)
        {
            errorDescription += description;
        }

        /**
         * Overrides ToString. Returns the name of the error.
         * @return Error name
         */
        public override string ToString()
        {
            return errorName;
        }

        /**
         * Returns the description of the error.
         * @return Error description
         */
        public string GetDescription()
        {
            return errorDescription;
        }

        /**
         * Sets key which is used in searching.
         */
        public void SetSearchKey(string key)
        {
            searchKey = key;
        }

        /**
         * Gets key which is used in searching.
         */
        public string GetSearchKey()
        {
            return searchKey;
        }

        private string searchKey = string.Empty;
        private string errorName = string.Empty;
        private string errorDescription = string.Empty;

    }
}
