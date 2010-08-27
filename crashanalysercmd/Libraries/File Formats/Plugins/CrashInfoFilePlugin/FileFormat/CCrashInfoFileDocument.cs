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
* The class CCrashInfoFileDocument is part of CrashAnalyser CrashInfoFile plugin.
* Container and output implementation for data in Crash Info File format.
* CI format is an intermediate file used in the MobileCrash server
* CCrashInfoFileSink creates an instance of this class and uses it to output
* crash data to file in CI format. 
* 
*/

using System;
using System.Collections.Generic;
using System.Text;

using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using System.IO;
using CrashItemLib.Sink;

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    internal class CCrashInfoFileDocument
    {
        #region Constructors
        public CCrashInfoFileDocument()
           
        {
        }

        #endregion

        /** Creates a new datablock and inputs data from container to the datablock */
        public void ReadDataFromContainer(CISinkSerializationParameters aParams)
        {
            CIContainer container = aParams.Container;

            //Create a datablock for this container's contents
            CCrashInfoDataBlock datablock = new CCrashInfoDataBlock();
           
            //Read all interesting data from container to the datablock
            datablock.AddHeader(container);
            datablock.AddSWInfos(container);
            datablock.AddThreadAndExitInfo(container);
            datablock.AddPanicedProcess(container);
            datablock.AddRegisterLists(container);
            datablock.AddStacks(container);
            datablock.AddCodeSegments(container);
            datablock.AddMemoryInfo(container);
            datablock.AddHWInfo(container);
            datablock.AddTelephony(container);
            datablock.AddEnvInfo(container);
            datablock.AddReportParameters(container);
            datablock.AddMessages(container);
            datablock.AddCrashHash(container);

            string archivedFileName = (String)aParams.OperationData1;
            datablock.AddFileNames(container, archivedFileName);
            datablock.AddEventlog(container);

            datablock.AddOstTraces(container);

            //If all went well, we will add datablock to stored datablocks
            iDatablocks.Add(datablock);
        }

        /** Writes datablock contents to stream in CrashInfoFile format. Makes a complete .ci file */ 
        public void WriteToStream(StreamWriter aOutput)
        {            
            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kversion));
            aOutput.Write(CrashInfoConsts.KVersionNumber.ToString().PadLeft(8, '0'));
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kversion));

            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Knum_datablocks));
            aOutput.Write(iDatablocks.Count.ToString().PadLeft(8, '0'));
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Knum_datablocks));

            foreach (CCrashInfoDataBlock datablock in iDatablocks)
            {
                datablock.WriteTimeStamp(aOutput);
                datablock.WriteRomID(aOutput);
                datablock.WriteSWVersion(aOutput);
                datablock.WriteVariantID(aOutput);
                datablock.WriteHWVersion(aOutput);
                datablock.WritePanicID(aOutput);
                datablock.WritePanicCategory(aOutput);
                datablock.WritePanicDescription(aOutput);
                datablock.WriteLanguage(aOutput);
                datablock.WritePanicedProcess(aOutput);
                datablock.WriteProgramCounter(aOutput);
                datablock.WriteModuleName(aOutput);
                datablock.WriteRegisterList(aOutput);
                datablock.WriteLoadedDLLs(aOutput);
                datablock.WriteAvailableMemory(aOutput);
                datablock.WriteUserComment(aOutput);
                datablock.WriteMemoryInfo(aOutput);
                datablock.WriteMiscInfo(aOutput);
                datablock.WriteReporter(aOutput);
                datablock.WriteArchive(aOutput);                
                datablock.WriteProductType(aOutput);
                datablock.WriteCrashSource(aOutput);
                datablock.WriteProductionMode(aOutput);
                datablock.WriteImei(aOutput);
                datablock.WriteResetreason(aOutput);
                datablock.WriteUptime(aOutput);
                datablock.WriteTestset(aOutput);
                datablock.WriteIMSI(aOutput);
                datablock.WriteNetworkCountry(aOutput);
                datablock.WriteNetworkIdentity(aOutput);
                datablock.WriteLocInfo(aOutput);
                datablock.WriteNetworkCell(aOutput);                
                datablock.WriteSerialNumber(aOutput);
                datablock.WriteS60Version(aOutput);
                datablock.WriteProductCode(aOutput);
                datablock.WriteVariantVersion(aOutput);
                datablock.WriteMMCInfo(aOutput);
                datablock.WriteUID(aOutput);
                datablock.WriteDiskInfo(aOutput);
                datablock.WriteFileType(aOutput);

                datablock.WriteReportType(aOutput);
                datablock.WriteReportCategory(aOutput);
                datablock.WriteReportOK(aOutput);
                datablock.WriteReportFail(aOutput);
                datablock.WriteReportParam1(aOutput);                
                datablock.WriteReportParam2(aOutput);                
                datablock.WriteReportParam3(aOutput);               
                datablock.WriteReportComments(aOutput);

                datablock.WriteRegisterExtraList(aOutput);

                datablock.WriteCrashHash(aOutput);
                datablock.WriteDetailedCrashHash(aOutput);

                datablock.WriteBinFileName(aOutput);
                datablock.WriteSymbolFileNames(aOutput);
                
                datablock.WriteCallstacks(aOutput);
                datablock.WriteEventlog(aOutput);

                datablock.WriteOstTraces(aOutput);


            }
        }

        #region Data members
        private List<CCrashInfoDataBlock> iDatablocks = new List<CCrashInfoDataBlock>();
        #endregion
    }
}
