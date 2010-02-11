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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Events;
using CrashItemLib.Crash.Header;
using CrashItemLib.Crash.InfoHW;
using CrashItemLib.Crash.InfoSW;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Summarisable;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Traces;
using CrashItemLib.Crash.Reports;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Engine;
using SymbianDebugLib.Engine;

namespace CrashItemLib.Crash.Container
{
    public sealed class CIContainer : CIElement, ICISymbolManager
    {
        #region Enumerations
        internal enum TCIElementEventType
        {
            ECIEventChildAdded = 0,
            ECIEventChildRemoved,
        }

        public enum TStatus
        {
            EStatusDefault = 0,
            EStatusErrorContainer
        }
        #endregion

        #region Delegates & events
        internal delegate void ElementEventHandler( CIContainer aContainer, CIElement aElement, TCIElementEventType aType );
        internal event ElementEventHandler ElementEvents;
        #endregion

        #region Static constructors
        internal static CIContainer New( CIEngine aEngine, CISource aSource )
        {
            CIContainer ret = new CIContainer( aEngine, aSource );
            return ret;
        }

        public static CIContainer NewErrorContainer( CIEngine aEngine, CISource aSource )
        {
            CIContainer ret = new CIContainer( aEngine, aSource );
            ret.Status = TStatus.EStatusErrorContainer;
            return ret;
        }
        #endregion

        #region Constructors
        private CIContainer( CIEngine aEngine, CISource aSource )
            : base( KRootElementId )
		{
            iEngine = aEngine;

            // Immediately set up container association (to point to ourself) 
            // just incase...
            base.Container = this;

            // And indicate, that since we *are* the container, we want all our children
            // to automatically be "in it" too.
            base.IsInContainer = true;

            // Add source descriptor
            CISourceElement source = new CISourceElement( this, aSource );
            AddChild( source );

            // Add other mandatory elements
            AddMandatoryElements();
		}
		#endregion

        #region API
        public IEnumerable<CISummarisableEntity> GetSummarisableEnumerator()
        {
            CIElementList<CISummarisableEntity> list = base.ChildrenByType<CISummarisableEntity>();
            return list;
        }
        #endregion

        #region Constants
        public const int KRootElementId = CIElementIdProvider.KInitialStartingValue;
        #endregion

        #region Properties
        public object Tag
		{
			get { return iTag; }
			set { iTag = value; }
		}

        public TStatus Status
        {
            get { return iStatus; }
            set { iStatus = value; }
        }

        public string[] FileNames
        {
            get
            {
                List<string> files = new List<string>();
                if ( iFileNames != null )
                {
                    files.AddRange( iFileNames );
                }
                return files.ToArray();
            }
        }

        public override CIEngine Engine
        {
            get { return iEngine; }
        }

        public CISummarisableEntity PrimarySummary
        {
            get
            {
                CISummarisableEntity ret = null;

                // The primary summary is the first summary we can locate
                // that relates to a crash.
                CISummarisableEntityList summaries = Summaries;
                foreach ( CISummarisableEntity entity in summaries )
                {
                    bool isCrash = entity.IsAbnormalTermination;
                    if ( isCrash )
                    {
                        // Prefer threads to raw stack items.
                        if ( ret != null )
                        {
                            // If the 'best match' so far is just a stack, then replace it with whatever
                            // we've just found. This means we could replace a raw stack with another raw
                            // stack. We could never replace a thread entity with a stack entity though.
                            if ( ret.IsAvailable( CISummarisableEntity.TElement.EElementThread ) == false )
                            {
                                ret = entity;
                            }
                        }
                        else
                        {
                            ret = entity;
                        }
                    }
                }

                return ret;
            }
        }
        #endregion

        #region Mandatory elements
        public CIHeader Header
        {
            get { return (CIHeader) ChildByType( typeof( CIHeader ) ); }
        }

        public CIEventList Events
        {
            get { return (CIEventList) ChildByType( typeof( CIEventList ) ); }
        }

        public CITraceData Traces
        {
            get { return (CITraceData) ChildByType( typeof( CITraceData ) ); }
        }

        public CISourceElement Source
        {
            get { return (CISourceElement) ChildByType( typeof( CISourceElement ) ); }
        }

        public CISymbolDictionary Symbols
        {
            get { return (CISymbolDictionary) ChildByType( typeof( CISymbolDictionary ) ); }
        }

        public CIMessageDictionary Messages
        {
            get { return (CIMessageDictionary) ChildByType( typeof( CIMessageDictionary ) ); }
        }

        public CISummarisableEntityList Summaries
        {
            get { return (CISummarisableEntityList) ChildByType( typeof( CISummarisableEntityList ) ); }
        }

        public CIRegisterListCollection Registers
        {
            get
            {
                return (CIRegisterListCollection) ChildByType( typeof( CIRegisterListCollection ) );
            }
        }

        public CIReportInfo ReportInfo
        {
            get { return (CIReportInfo) ChildByType( typeof( CIReportInfo ) ); }
        }
        #endregion

        #region Internal methods
        internal void RunFinalizers( CIElementFinalizationParameters aParams )
        {
            Queue<CIElement> mustBeCalledLast = new Queue<CIElement>();
            base.DoFinalize( aParams, mustBeCalledLast, false );

            // Now call the elements that are to be finalized last
            while ( mustBeCalledLast.Count > 0 )
            {
                CIElement child = mustBeCalledLast.Dequeue();
                child.DoFinalize( aParams, mustBeCalledLast, true );
            }
        }
        
        internal int GetNextElementId()
        {
            return Engine.GetNextElementId();
        }

        private void AddMandatoryElements()
        {
            Type attribType = typeof( CIElementAttributeMandatory );
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            //
            foreach( Type t in types)
            {
                // Get all the constructors for the type
                if ( !t.IsAbstract && typeof( CIElement ).IsAssignableFrom( t ) )
                {
                    ConstructorInfo[] ctors = t.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                    foreach ( ConstructorInfo ctor in ctors )
                    {
                        // If the specified ctor is decorated with the "mandatory" attribute
                        // then it must be a mandatory class, so new an instance up...
                        object[] attribs = ctor.GetCustomAttributes( attribType, false );
                        if ( attribs.Length > 0 )
                        {
                            // Check that it has the expected signature.
                            // We expect mandatory constructors to take only a single parameter
                            // which is the container itself, i.e. this object
                            ParameterInfo[] parameters = ctor.GetParameters();
                            if ( parameters.Length == 1 && parameters[ 0 ].ParameterType == this.GetType() )
                            {
                                CIElement element = ctor.Invoke( new object[] { this } ) as CIElement;
                                if ( element != null )
                                {
                                    element.Parent = this;
                                    AddChild( element );
                                }
                            }
                        }
                    }
                }
            }
            //
            AddChild( new CISymbolDictionary( this ) );
        }

        private void CacheFileNames( DbgEngine aDebugEngine )
        {
            if ( iFileNames == null )
            {
                iFileNames = new List<string>();

                CISourceElement source = Source;
                foreach ( FileInfo file in source.AllFiles )
                {
                    iFileNames.Add( file.FullName );
                }

                // Meta-data files
                SymbianUtils.FileSystem.FSEntity[] entities = aDebugEngine.FileSystemEntities;
                foreach ( SymbianUtils.FileSystem.FSEntity e in entities )
                {
                    if ( e.IsFile )
                    {
                        FileInfo file = ( (SymbianUtils.FileSystem.FSEntityFile) e ).File;
                        iFileNames.Add( file.FullName );
                    }
                }
            }
        }
        #endregion

        #region Internal container event propagation
        internal void OnContainerElementRegistered( CIElement aElement )
        {
            if ( ElementEvents != null )
            {
                ElementEvents( this, aElement, TCIElementEventType.ECIEventChildAdded );
            }
        }

        internal void OnContainerElementUnregistered( CIElement aElement )
        {
            if ( ElementEvents != null )
            {
                ElementEvents( this, aElement, TCIElementEventType.ECIEventChildRemoved );
            }
        }
        #endregion

        #region From ICISymbolManager
        public CISymbolDictionary SymbolDictionary
        {
            get { return this.Symbols; }
        }
        #endregion

        #region From CIElement
        /// <summary>
        /// Ensure that we only allow single instances of some objects to be added
        /// as direct children.
        /// </summary>
        public override void AddChild( CIElement aChild )
        {
            bool exception = false;
            //
            if ( aChild is CIEventList && Events != null )
            {
                exception = true;
            }
            else if ( aChild is CISymbolDictionary && Symbols != null )
            {
                exception = true;
            }
            else if ( aChild is CIMessageDictionary && Messages != null )
            {
                exception = true;
            }
            else if ( aChild is CISourceElement && Source != null )
            {
                exception = true;
            }
            else if ( aChild is CISummarisableEntityList && Summaries != null )
            {
                exception = true;
            }
            else if ( aChild is CIHeader && Header != null )
            {
                exception = true;
            }
            else if ( aChild is CIReportInfo && ReportInfo != null )
            {
                exception = true;
            }
            else
            {
                // These aren't mandatory, but there should only be one...
                int count = -1;
                if ( aChild is CIInfoHW )
                {
                    count = base.ChildrenByType<CIInfoHW>().Count;
                }
                else if ( aChild is CIInfoSW )
                {
                    count = base.ChildrenByType<CIInfoSW>().Count;
                }

                if ( count > 1 )
                {
                    throw new ArgumentException( "An instance of the specified object has already been added to the container" );
                }
            }

            if ( exception )
            {
                throw new ArgumentException( "Can only add a single instance of " + aChild.GetType() + " to the container" );
            }

            base.AddChild( aChild );
        }

        /// <summary>
        /// Called by CIElement when an object is *directly* added
        /// as a child of the container.
        /// </summary>
        protected override void OnElementAddedToSelf( CIElement aElement )
        {
            // The master switch that ensures all elements and their children
            // are flagged as in the container.
            aElement.IsInContainer = true;
        }

        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            base.OnFinalize( aParams );

            // Cache file names
            CacheFileNames( aParams.DebugEngine );
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private object iTag = null;
        private TStatus iStatus = TStatus.EStatusDefault;
        private List<string> iFileNames = null;
		#endregion
    }
}
