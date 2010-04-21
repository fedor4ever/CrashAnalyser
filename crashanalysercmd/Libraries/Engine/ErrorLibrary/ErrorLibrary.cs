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
* The class XmlErrorLibrary provides methods to read panic description and error description.
* If the information is not yet parsed from the XML files, it will be read when the error/panic
* description is asked first time.
* 
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Specialized;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Globalization;

namespace ErrorLibrary
{
    public static class XmlErrorLibrary
    {

        /**
         * Returns panic description based on panic category and panic id.
         * @param category Panic category.
         * @param id Panic id.
         * @return Panic description.
         */
        public static string GetPanicDescription(string category, string id)
        {
            lock (createLock)
            {
                if (iPanics == null)
                {
                    // Data not loaded, load it:
                    LoadData();
                }
            }
            
            if (iPanics != null)
            {
                string searchKey = category + id;
                foreach (ErrorLibraryError error in iPanics)
                {
                    if (error.GetSearchKey().Equals(searchKey))
                    {
                        return error.GetDescription();
                    }
                }
            }
            return string.Empty;
        }

        /**
         * Loads panic description data from resources (xml-files).
         */
        private static void LoadData()
        {

            ResourceManager resMgr = ErrorLibrary.Properties.Resources.ResourceManager;


            if (resMgr == null)
            {
                // Resource manager not available, not possible to read resources.
                return;
            }

            ResourceSet resSet = resMgr.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            if (resSet == null)
            {
                // Resources not available, can not read them.
                return;
            }

            IDictionaryEnumerator enumerator = resSet.GetEnumerator();

            createDataStructures();

            while (enumerator.MoveNext())
            {
                object obj = enumerator.Value;
                if (obj is string)
                {
                    try
                    {
                        loadResources((string)obj);
                    }
                    catch (Exception)
                    {
                        // Exception reading one file. Try still to read others.
                    }
                }
            }
        }

        /**
         * Adds the data from one XML file (i.e. resourceString) to 
         * data structures.
         * 
         * @param resourceString One XML-file as string.
         */
        private static void loadResources(string resourceString)
        {
            ErrorLibraryError error = null;
            ErrorLibraryError error2 = null;
            ErrorLibraryError panic = null;

            string nodeText = "";
            string categoryDescription = "";
            string categoryName = "";
            string panicKey = "";
            string errorKey1 = "";
            string errorKey2 = "";
            string errorComponent = "";

            byte[] xmlByteArray = Encoding.ASCII.GetBytes(resourceString);
            MemoryStream memoryStream = new MemoryStream(xmlByteArray);
            XmlTextReader textReader = new XmlTextReader(memoryStream);
            while (textReader.Read())
            {
                XmlNodeType nType = textReader.NodeType;
                if (nType == XmlNodeType.Text)
                {
                    nodeText = textReader.Value;
                    switch (tagType)
                    {
                        case TagType.TagTypeCategoryName:
                            categoryName = nodeText;
                            break;
                        case TagType.TagTypeCategoryDescription:
                            categoryDescription = HtmlFormatter.formatCategoryDescription(categoryName, nodeText);
                            break;
                        case TagType.TagTypePanicId:
                            panic = new ErrorLibraryError();
                            panic.SetName(categoryName + " " + nodeText);
                            panicKey = categoryName + nodeText;
                            break;
                        case TagType.TagTypePanicDescription:
                            panic.SetDescription(HtmlFormatter.formatPanicDescription(panic.ToString(), nodeText));
                            panic.AddToDescription(categoryDescription);
                            // iPanics.put(panicKey, panic);
                            panic.SetSearchKey(panicKey);
                            iPanics.Add(panic);
                            panic = null;
                            panicKey = "";
                            break;
                        case TagType.TagTypePanicCategory:
                            // Do nothing
                            break;
                        case TagType.TagTypeErrorName:
                            error = new ErrorLibraryError();
                            error.SetName(nodeText);
                            errorKey1 = nodeText;
                            break;
                        case TagType.TagTypeErrorValue:
                            error.SetDescription(HtmlFormatter.formatErrorDescription(error.ToString(), nodeText));
                            error2 = new ErrorLibraryError();
                            error2.SetName(nodeText);
                            error2.SetDescription(HtmlFormatter.formatErrorDescription(nodeText, error.ToString()));
                            errorKey2 = nodeText;
                            break;
                        case TagType.TagTypeErrorComponent:
                            errorComponent = nodeText;
                            break;
                        case TagType.TagTypeErrorText:
                            error.AddToDescription(nodeText);
                            error.AddToDescription(HtmlFormatter.formatErrorComponent(errorComponent));
                            error2.AddToDescription(nodeText);
                            error2.AddToDescription(HtmlFormatter.formatErrorComponent(errorComponent));
                            error.SetSearchKey(errorKey1);
                            iErrors.Add(error);
                            error = null;
                            errorKey1 = "";
                            error2.SetSearchKey(errorKey2);
                            iErrors.Add(error2);
                            error2 = null;
                            errorKey2 = "";
                            break;
                    }
                }
                // if node type is an element
                else if (nType == XmlNodeType.Element)
                {
                    string name = textReader.Name.ToString();
                    nodeText = textReader.Value;

                    if (TAG_CATEGORY_NAME.Equals(name))
                    {
                        tagType = TagType.TagTypeCategoryName;
                    }
                    else if (TAG_CATEGORY_DESCRIPTION.Equals(name))
                    {
                        tagType = TagType.TagTypeCategoryDescription;
                    }
                    else if (TAG_PANIC_ID.Equals(name))
                    {
                        tagType = TagType.TagTypePanicId;
                    }
                    else if (TAG_PANIC_DESCRIPTION.Equals(name))
                    {
                        tagType = TagType.TagTypePanicDescription;
                    }
                    else if (TAG_PANIC_CATEGORY.Equals(name))
                    {
                        tagType = TagType.TagTypePanicCategory;
                    }
                    else if (TAG_ERROR_NAME.Equals(name))
                    {
                        tagType = TagType.TagTypeErrorName;
                    }
                    else if (TAG_ERROR_VALUE.Equals(name))
                    {
                        tagType = TagType.TagTypeErrorValue;
                    }
                    else if (TAG_ERROR_COMPONENT.Equals(name))
                    {
                        tagType = TagType.TagTypeErrorComponent;
                    }
                    else if (TAG_ERROR_TEXT.Equals(name))
                    {
                        tagType = TagType.TagTypeErrorText;
                    }

                }
            }

        }

        /**
         * Create data structures for panics/errors.
         */
        private static void createDataStructures()
        {
            iErrors = new ArrayList();
            iPanics = new ArrayList();
        }


        #region Data members

        private enum TagType
        {
            TagTypeCategoryName,
            TagTypeCategoryDescription,
            TagTypePanicId,
            TagTypePanicDescription,
            TagTypePanicCategory,
            TagTypeErrorName,
            TagTypeErrorValue,
            TagTypeErrorText,
            TagTypeErrorComponent
        }

        private static string TAG_CATEGORY_NAME = "category_name";
        private static string TAG_CATEGORY_DESCRIPTION = "category_description";
        private static string TAG_PANIC_ID = "panic_id";
        private static string TAG_PANIC_DESCRIPTION = "panic_description";
        private static string TAG_PANIC_CATEGORY = "panic_category";
        private static string TAG_ERROR_NAME = "error_name";
        private static string TAG_ERROR_VALUE = "error_value";
        private static string TAG_ERROR_TEXT = "error_text";
        private static string TAG_ERROR_COMPONENT = "error_component";
        
        private static TagType tagType;
        private static ArrayList iErrors = null;
        private static ArrayList iPanics = null;

        private static readonly object createLock = new object();

        #endregion

    }
}
