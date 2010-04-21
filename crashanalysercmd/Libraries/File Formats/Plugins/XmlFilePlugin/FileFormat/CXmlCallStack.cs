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
* The class CCrashInfoCallStack is part of CrashAnalyser CrashInfoFile plugin.
* It is a temporary container for thread call stack.
* Call stack is stored in fully decoded form as lines of text
* 
*/

using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Symbols;
using XmlFilePlugin.PluginImplementations.FileFormat;

namespace XmlFilePlugin.FileFormat
{
    class CXmlCallStack
    {

        #region Constructors
        public CXmlCallStack()
        {

        }
        #endregion



        public void Read(CIStack aStack)
        {
            foreach (CIStackEntry entry in aStack)
            {
                CCrashInfoCSItem csItem = new CCrashInfoCSItem();
                if (entry.IsCurrentStackPointerEntry)
                {
                    iStackPointerLocation = iCallStack.Count;
                    csItem.iIsStackPointer = true;
                }

                CIRegister register = entry.Register;
                if (register != null) //entry is from registers
                {
                    csItem.iIsRegisterEntry = true;
                    if (register.Name == "PC")
                    {
                        csItem.iRegisterName = "Program counter";
                    }
                    else if (register.Name == "LR")
                    {
                        csItem.iRegisterName = "Link register";
                    }
                    else //other register
                    {
                        csItem.iRegisterName = register.Name;
                    }
                }
                else //entry is from memory (normal case)
                {
                    csItem.iMemoryAddress = entry.Address;
                }

                //Add data contained in the memory location
                csItem.iItemData = entry.Data;
                csItem.iItemDataString = entry.DataAsString;

                if (entry.Symbol != null) //add symbol if possible
                {
                    csItem.iHasSymbol = true;
                    csItem.iSymbolName = entry.Symbol.Symbol.Name;
                    csItem.iSymbolOffset = entry.FunctionOffset;
                    csItem.iSymbolObject = entry.Symbol.Symbol.Object;

                
                }
                // else symbol is not available

                iCallStack.Add(csItem);

            }

        }
        public void CleanStack()
        {   
            //Clean elements far above stack pointer
            if (iStackPointerLocation != null)
            {
                int removeAmount = (int)iStackPointerLocation - XmlConsts.KMaxItemAboveSP;
                               
                if (removeAmount > 0)
                {
                    iCallStack.RemoveRange(0, removeAmount);
                }

                //Clean symbolless items far below stack pointer
                for (int i = (int)iStackPointerLocation + XmlConsts.KNonSymbolItemsAfterSP; i < iCallStack.Count; )
                {
                    if (!iCallStack[i].iHasSymbol)
                    {                        
                        iCallStack.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
          
        
        public override string ToString()
        {
            MakeWritableStack();
            
            System.Text.StringBuilder output = new System.Text.StringBuilder();
            foreach (string line in iTextContent)
            {
                if (output.Length + line.Length + XmlConsts.Kxml_EOL.Length >= XmlConsts.KMaxStackSize)
                {
                    break;
                }

                output.AppendLine(line);
            }
            return output.ToString();

        }
              
        private void MakeWritableStack()
        {

            foreach (CCrashInfoCSItem csItem in iCallStack)
            {
                System.Text.StringBuilder line = new System.Text.StringBuilder();
                if (csItem.iIsStackPointer)
                {
                    line.Append("This is current stack pointer ");
                }


                if (csItem.iIsRegisterEntry) //entry is from registers
                {
                    line.Append(csItem.iRegisterName + " ");
                }
                else //entry is from memory (normal case)
                {
                    line.Append(csItem.iMemoryAddress.ToString("X").PadLeft(8, '0'));
                }

                line.Append(" " + csItem.iItemData.ToString("X").PadLeft(8, '0'));



               if (csItem.iHasSymbol) //symbol if available
               {
                   line.Append(" " + csItem.iSymbolName);

                   line.Append(" " + csItem.iSymbolOffset.ToString("X").PadLeft(4, '0'));

                   line.Append(" " + csItem.iSymbolObject);
               }
               else //symbol is not available, print content in ascii (may contain some readable text)
               {
                   line.Append(" " + csItem.iItemDataString);
               }               
               iTextContent.Add(line.ToString());
                
           }
        }

        #region Data Members


        private List<string> iTextContent = new List<string>();

        private List<CCrashInfoCSItem> iCallStack = new List<CCrashInfoCSItem>();
        private int? iStackPointerLocation = null;
        #endregion

        #region Nested Classes
        private class CCrashInfoCSItem
        {
            #region Constructors
            public CCrashInfoCSItem()
            {

            }
            #endregion

            #region Data Members

            public uint iMemoryAddress = 0;
            public uint iItemData = 0;
            public string iItemDataString = string.Empty;

            public bool iIsStackPointer = false;
            public bool iIsRegisterEntry = false;
            public string iRegisterName = string.Empty;
            
            public bool iHasSymbol = false;
            public string iSymbolName = string.Empty;
            public string iSymbolObject = string.Empty;
            public uint iSymbolOffset = 0;

            #endregion
        }

        #endregion
   
    }

    

}