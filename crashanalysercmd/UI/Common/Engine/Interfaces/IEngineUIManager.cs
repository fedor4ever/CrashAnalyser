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
using System.Text;
using System.Windows.Forms;
using CrashAnalyserEngine.Tabs;

namespace CrashAnalyserEngine.Interfaces
{
    public interface IEngineUIManager
    {
        /// <summary>
        /// Add a menu item
        /// </summary>
        void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag );

        /// <summary>
        /// Add a menu item that is associated with a specific host element (usually an IToolStripHost)
        /// </summary>
        void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag, CATab aHost );

        /// <summary>
        /// Creates a new tab
        /// </summary>
        /// <param name="aControl"></param>
        void UIManagerContentAdd( CATab aTab );

        /// <summary>
        /// Removes a tab
        /// </summary>
        /// <param name="aControl"></param>
        void UIManagerContentClose( CATab aTab );

        /// <summary>
        /// Information about the runtime version
        /// </summary>
        Version UIVersion
        {
            get;
        }

        /// <summary>
        /// Command line arguments passed to runtime
        /// </summary>
        string UICommandLineArguments
        {
            get;
        }

        /// <summary>
        /// Whether the UI runs in silent mode
        /// </summary>
        bool UIIsSilent
        {
            get;
        }

        void UITrace( string aMessage );

        void UITrace( string aFormat, params object[] aParams );
    }
}
