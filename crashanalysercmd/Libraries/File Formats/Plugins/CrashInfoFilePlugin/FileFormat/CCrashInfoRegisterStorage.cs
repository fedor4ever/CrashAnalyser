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
* The class CCrashInfoRegisterStorage is part of CrashAnalyser CrashInfoFile plugin.
* Temporary container for arm register list information conversion from
* CrashAnalyser data to CrashInfoFile document.
* Reads register info from CIRegisterList structures and outputs formatted
* CrashInfo file rows.
* 
*/

using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Crash.Registers;

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    internal class CCrashInfoRegisterStorage
    {
        #region Constructors
        public CCrashInfoRegisterStorage()           
		{
        
		}
		#endregion

        public void ReadRegisterData(CIRegisterList aRegList)
        {
            //If long enough reglist starts with R0, it is assumed to be the "basic" register list (R0-R15)
            if (aRegList.Count > 15 && aRegList[0].Type == SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_00)
            {
                iBasicRegs.Name = aRegList.Name;
                foreach (CIRegister register in aRegList)
                {
                    string regName = GetRegisterName(register);      

                    CCrasInfoRegisterItem regItem = new CCrasInfoRegisterItem(regName, register.Value);

                    if (register.Symbol != null && CCrashInfoFileUtilities.IsSymbolSerializable(register.Symbol))
                    {                        
                        regItem.Symbol = register.Symbol.Name;
                    }

                    iBasicRegs.Registers.Add(regItem);

                    //Check if this is PC and save it separately
                    if (register.Type == SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_PC)
                    {
                        iProgramCounter.Value = register.Value;
                        iProgramCounter.Symbol = register.Symbol.Name;
                    }
                }
            }
            else //all other registers as their own list
            {
                CCrashInfoRegisterList regs = new CCrashInfoRegisterList();
                regs.Name = aRegList.Name;
                bool hasRealData = false;
                foreach (CIRegister register in aRegList)
                {
                    string regName = GetRegisterName(register);
                    CCrasInfoRegisterItem regItem = new CCrasInfoRegisterItem(regName, register.Value);
                    if (register.Symbol != null && CCrashInfoFileUtilities.IsSymbolSerializable(register.Symbol))
                    {
                        regItem.Symbol = register.Symbol.Name;
                    }

                    regs.Registers.Add(regItem);

                    if (register.Value != 0)
                    {
                        hasRealData = true;
                    }
                }

                if (hasRealData)
                {
                    iOtherRegLists.Add(regs);
                }
            }
        }

        public void WriteBasicRegisters(System.IO.StreamWriter aOutput)
        {
            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kregister));
            if (iBasicRegs.Registers.Count == 0)
            {
                aOutput.Write("NotFound|");

            }
            else
            {
                aOutput.Write(iBasicRegs.Name);
                foreach (CCrasInfoRegisterItem reg in iBasicRegs.Registers)
                {
                    aOutput.Write(CrashInfoConsts.KSeparator);
                    aOutput.Write(reg.Name);
                    aOutput.Write(CrashInfoConsts.KSeparator);
                    aOutput.Write(reg.Value);
                    if (reg.Symbol != string.Empty)
                    {
                        aOutput.Write(":" +reg.Symbol);
                    }
                }
            }
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kregister));   
        }

        internal void WriteOtherRegisters(System.IO.StreamWriter aOutput)
        {
            if (iOtherRegLists.Count > 0)
            {
                aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kregister_extra));
                foreach (CCrashInfoRegisterList regList in iOtherRegLists)
                {

                    aOutput.Write(regList.Name);
                    aOutput.Write(CrashInfoConsts.KEOL);
                    foreach (CCrasInfoRegisterItem reg in regList.Registers)
                    {
                        aOutput.Write(reg.Name + " 0x" + reg.Value.ToString("X8"));
                        if (reg.Symbol != string.Empty)
                        {
                            aOutput.Write(" " + reg.Symbol);
                        }
                        aOutput.Write(CrashInfoConsts.KEOL);
                    }
                    aOutput.Write(CrashInfoConsts.KEOL);

                }
                aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kregister_extra));

            }
        }

        internal void WriteProgramCounter(System.IO.StreamWriter aOutput)
        {
          aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kprogram_counter));
          aOutput.Write(iProgramCounter.Value);
            aOutput.Write(CrashInfoConsts.KSeparator);
            if (iProgramCounter.Symbol != string.Empty)
            {
                aOutput.Write(iProgramCounter.Symbol);
            }
            else
            {
                aOutput.Write("-"); //missing pc symbol is marked with - in ci file.
            }
                

          aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kprogram_counter));
        }

        /** Return CrashInfo compatible register name - R0-R15 in short numeric form, other as they are */
        public static string GetRegisterName(CIRegister aRegister)
        {
            string ret = aRegister.Name;
            //
            switch (aRegister.Type)
            {
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_00:
                    ret = "R0";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_01:
                    ret = "R1";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_02:
                    ret = "R2";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_03:
                    ret = "R3";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_04:
                    ret = "R4";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_05:
                    ret = "R5";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_06:
                    ret = "R6";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_07:
                    ret = "R7";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_08:
                    ret = "R8";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_09:
                    ret = "R9";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_10:
                    ret = "R10";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_11:
                    ret = "R11";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_12:
                    ret = "R12";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_SP:
                    ret = "R13";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_LR:
                    ret = "R14";
                    break;
                case SymbianStructuresLib.Arm.Registers.TArmRegisterType.EArmReg_PC:
                    ret = "R15";
                    break;
                default:
                    break;
            }
            //
            return ret;
        }

        #region Data Members       
        private CCrashInfoRegisterList iBasicRegs = new CCrashInfoRegisterList(); //R0-R15
        private List<CCrashInfoRegisterList> iOtherRegLists = new List<CCrashInfoRegisterList>(); //Other registers
        private CCrasInfoRegisterItem iProgramCounter = new CCrasInfoRegisterItem("PC");

        #endregion

        #region Nested Classes
        private class CCrashInfoRegisterList
        {
            #region Constructors
            public CCrashInfoRegisterList()
            {

            }
            #endregion

            #region Properties
            public List<CCrasInfoRegisterItem> Registers
            {
                get { return iRegisters; }
                set { iRegisters = value; }
            }
            public string Name
            {
                get { return iName; }
                set { iName = value; }
            }

            #endregion

            private List<CCrasInfoRegisterItem> iRegisters = new List<CCrasInfoRegisterItem>();            
            private string iName;

           
        }

        private class CCrasInfoRegisterItem
        {
            #region Constructors
            public CCrasInfoRegisterItem(string aName)
            {
                iName = aName;
            }

            public CCrasInfoRegisterItem(string aName, uint aValue)
            {
                iName = aName;
                iValue = aValue;
            }

            #endregion

            #region Properties
            public string Name
            {
                get { return iName; }
                set { iName = value; }
            }

            public uint Value
            {
                get { return iValue; }
                set { iValue = value; }
            }

            public string Symbol
            {
                get { return iSymbol; }
                set { iSymbol = value; }
            }

            #endregion


            #region Data Members

            private uint iValue = 0;
            private string iName = string.Empty;
            private string iSymbol = string.Empty;

            #endregion
        }

        #endregion





    }

    
}
