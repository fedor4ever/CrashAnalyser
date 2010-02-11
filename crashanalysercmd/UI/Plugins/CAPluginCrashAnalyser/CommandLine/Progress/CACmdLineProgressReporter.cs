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
using System.Text;
using System.Xml;
using System.Collections.Generic;
using CrashItemLib.Crash.Container;

namespace CAPCrashAnalysis.CommandLine.Progress
{
    internal class CACmdLineProgressReporter
    {
        #region Constructors
        public CACmdLineProgressReporter()
        {
        }
        #endregion

        #region API
        public void StepBegin( string aMessage, string aKey )
        {
            StepBegin( aMessage, aKey, KNotDefined );
        }

        public void StepBegin( string aMessage, string aKey, int aSubSteps )
        {
            // Check preconditions
            if ( string.IsNullOrEmpty( aMessage ) )
            {
                throw new ArgumentException( "Step title cannot be undefined" );
            }
            else if ( string.IsNullOrEmpty( aKey ) )
            {
                throw new ArgumentException( "Step key cannot be undefined" );
            }
            //
            lock ( iSyncRoot )
            {
                Step step = FindStep( aKey );
                if ( step != null )
                {
                    throw new ArgumentException( "The specified key already exists: " + aKey );
                }

                step = new Step( this, aKey, ++iCurrentStep, aSubSteps );
                step.StepBegin( aMessage );
                iActiveSteps.Add( aKey, step );
            }
        }

        public void StepProgress( string aMessage, int aValue, string aKey )
        {
            if (iDetailed)
            {
                lock (iSyncRoot)
                {
                    Step step = FindStep(aKey);
                    if (step == null)
                    {
                        throw new ArgumentException("A step with the specified key was not found: " + aKey);
                    }
                    step.StepProgress(aMessage, aValue);
                }
            }
        }

        public void StepEnd( string aMessage, string aKey )
        {
            if ( string.IsNullOrEmpty( aKey ) )
            {
                throw new ArgumentException( "Step key cannot be undefined" );
            }
            lock ( iSyncRoot )
            {
                Step step = FindStep( aKey );
                if ( step == null )
                {
                    throw new ArgumentException( "A step with the specified key was not found: " + aKey );
                }
                step.StepEnd( aMessage );
                iActiveSteps.Remove( aKey );
            }
        }

        public void PrintProgress( string aText, int aStepNumber )
        {
            StringBuilder line = new StringBuilder();
            line.Append( "[CA PROGRESS]" );

            lock ( iSyncRoot )
            {
                line.Append( " " );
                line.Append( "{" );

                // Put step count if available
                if ( iTotalStepCount != KNotDefined )
                {
                    line.AppendFormat( "{0:d3}/{1:d3}", aStepNumber, iTotalStepCount );
                }
                else
                {
                    line.AppendFormat( "{0:d3}/???", aStepNumber, iTotalStepCount );
                }

                line.Append( "}" );
                line.AppendFormat( " {0}", aText );
            }
            //
            string text = line.ToString();
            PrintRaw( text );
        }
        #endregion

        #region Properties
        public bool Enabled
        {
            get { return iEnabled; }
            set { iEnabled = value; }
        }

        public bool Detailed
        {
            get { return iDetailed; }
            set { iDetailed = value; iEnabled = value; }
        }

        public int TotalNumberOfSteps
        {
            get
            {
                lock ( iSyncRoot )
                {
                    return iTotalStepCount;
                }
            }
            set
            {
                lock ( iSyncRoot )
                {
                    iTotalStepCount = value;
                }
            }
        }
        #endregion

        #region Internal methods
        private Step FindStep( string aKey )
        {
            Step ret = null;
            iActiveSteps.TryGetValue( aKey, out ret );
            return ret;
        }

        private void PrintRaw( string aText )
        {
            System.Console.WriteLine( aText );
        }
        #endregion

        #region Internal constants
        internal const int KNotDefined = -1;
        #endregion

        #region Internal classes
        private class Step
        {
            #region Constructors
            public Step( CACmdLineProgressReporter aParent, string aKey, int aStepNumber, int aSubStepCount )
            {
                iKey = aKey;
                iParent = aParent;
                iStepNumber = aStepNumber;
                iSubStepCount = aSubStepCount;
            }
            #endregion

            #region API
            public void StepBegin( string aMessage )
            {
                StringBuilder message = new StringBuilder();
                message.Append( KPrefixStepBegin );
                //
                if ( string.IsNullOrEmpty( aMessage ) == false )
                {
                    message.AppendFormat( " - {0}", aMessage );
                }
                //
                PrintProgress( message.ToString() );
            }

            public void StepProgress( string aMessage, int aValue )
            {
                float newProgress = ( ( (float) aValue ) / (float) iSubStepCount ) * 100.0f;
                if ( (int) newProgress != iLastProgressPercentage || aValue == iSubStepCount )
                {
                    iLastProgressPercentage = (int) newProgress;
                    iLastProgressMessage = aMessage;
                    ++iNumberOfProgressReportPackets;

                    StringBuilder text = new StringBuilder();
                    text.AppendFormat( "{0:d3}%", iLastProgressPercentage );

                    if ( string.IsNullOrEmpty( aMessage ) == false )
                    {
                        text.Append( " " + aMessage );
                    }

                    PrintProgress( text.ToString() );
                }
            }

            public void StepEnd( string aMessage )
            {
                // If we didn't hit 100% completion, then emit a dummy
                // progress event.
                if ( iNumberOfProgressReportPackets > 0 && iSubStepCount != KNotDefined && iLastProgressPercentage < 100 )
                {
                    StepProgress( iLastProgressMessage, iSubStepCount );
                }

                // Now output end marker
                StringBuilder message = new StringBuilder();
                message.Append( KPrefixStepEnd );
                //
                if ( string.IsNullOrEmpty( aMessage ) == false )
                {
                    message.AppendFormat( " - {0}", aMessage );
                }
                //
                PrintProgress( message.ToString() );
                iParent.PrintRaw( string.Empty );
            }
            #endregion

            #region Internal constants
            private const string KPrefixStepBegin = "START";
            private const string KPrefixStepEnd = "END";
            #endregion

            #region Internal methods
            private void PrintProgress( string aText )
            {
                iParent.PrintProgress( aText, iStepNumber );
            }
            #endregion

            #region Data members
            private readonly string iKey;
            private readonly int iStepNumber;
            private readonly CACmdLineProgressReporter iParent;
            private readonly int iSubStepCount;
            private string iLastProgressMessage = string.Empty;
            private int iLastProgressPercentage = KNotDefined;
            private int iNumberOfProgressReportPackets = 0;
            #endregion
        }
        #endregion

        #region Data members
        private bool iEnabled = false;
        private bool iDetailed = false;
        private object iSyncRoot = new object();
        //
        private int iTotalStepCount = KNotDefined;
        private int iCurrentStep = 0;
        //
        private Dictionary<string, Step> iActiveSteps = new Dictionary<string, Step>();
        #endregion
    }
}
