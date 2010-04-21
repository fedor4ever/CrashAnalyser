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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Drawing;

namespace SymbianUtils
{
	#region Enumerations
	public enum TNotUsingThread
	{
		ENotUsingThread
	}
	#endregion

	public abstract class AsyncTextWriterBase : DisposableObject
	{
		#region Events
		public enum TEvent
		{
			EWritingStarted = 0,
			EWritingProgress,
			EWritingComplete
		}

		public delegate void Observer( TEvent aEvent, AsyncTextWriterBase aObject );
		public event Observer iObserver;
		#endregion

		#region Construct & destruct
		public AsyncTextWriterBase()
			: this( System.Threading.ThreadPriority.BelowNormal )
		{
		}

		public AsyncTextWriterBase( System.Threading.ThreadPriority aPriority )
		{
			iWorkerThread = new Thread( new System.Threading.ThreadStart( WorkerThreadFunction ) );
			iWorkerThread.Name = "WorkerThreadFunction_" + this.ToString();
			iWorkerThread.Priority = aPriority;
			iWorkerThread.IsBackground = true;
		}

		public AsyncTextWriterBase( TNotUsingThread aNotUsingThread )
		{
		}
		#endregion

		#region API
		public void AsyncWrite()
		{
			lock( this )
			{
				if	( iWorkerThread != null )
				{
					iWorkerThread.Start();
				}
			}
		}
		#endregion

		#region From DisposableObject - Cleanup Framework
		protected override void CleanupManagedResources()
		{
            try
            {
            }
            finally
            {
                base.CleanupManagedResources();
            }
		}

		protected override void CleanupUnmanagedResources()
		{
            try
            {
            }
            finally
            {
                base.CleanupUnmanagedResources();
            }
		}
		#endregion

		#region Properties
		public bool IsReady
		{
			get
			{
				lock(this)
				{
					return iReady;
				}
			}
			set
			{
				lock( this )
				{
					iReady = value;
				}
			}
		}

        public int Progress
		{
			get
			{
				lock(this)
				{
					if (Size == 0)
						return 0;
					else
					{
						float positionAsFloat = (float)Position;
						float sizeAsFloat = (float)Size;
						int progress = (int)((positionAsFloat / sizeAsFloat) * 100.0);
						//
						return System.Math.Max(1, System.Math.Min(100, progress));
					}
				}
			}
		}
		#endregion

		#region Write handlers
		protected virtual bool ContinueProcessing()
		{
			return false;
		}

		protected virtual void HandleWriteStarted()
		{
		}

		protected virtual void HandleWriteCompleted()
		{
		}

		protected virtual void HandleWriteException( Exception aException )
		{
		}
		#endregion

		#region Abstract writing framework
		public abstract void ExportData();
        public abstract long Size { get; }
        public abstract long Position { get; }
		#endregion

		#region Internal methods
		private void NotifyEvent( TEvent aEvent )
		{
			if	( iObserver != null )
			{
				iObserver( aEvent, this );
			}
		}

		private void AsyncWriteLines()
		{
			bool forcedContinue = false;
			lock( this )
			{
				forcedContinue = ContinueProcessing();
			}

			while( Progress != KAllWorkCompletedPercentage || forcedContinue )
			{
				ExportData();

				lock( this )
				{
					NotifyEvent( TEvent.EWritingProgress );
					forcedContinue = ContinueProcessing();
				}
			}
		}

		private void WorkerThreadFunction()
		{
			try
			{
				lock( this )
				{
					iReady = false;
					HandleWriteStarted();
					NotifyEvent( TEvent.EWritingStarted );
				}

				AsyncWriteLines();
			}
			catch( Exception exception )
			{
				lock( this )
				{
					HandleWriteException( exception );
				}
			}
			finally
			{
				lock( this )
				{
					HandleWriteCompleted();
					iReady = true;
					NotifyEvent( TEvent.EWritingComplete );
				}
			}
		}
		#endregion

		#region Internal constants
		protected const int KAllWorkCompletedPercentage = 100;
		#endregion

		#region Data members
		protected bool iReady = true;
		private readonly Thread iWorkerThread;
		#endregion
	}

	public abstract class AsyncTextFileWriter : AsyncTextWriterBase
	{
		#region Construct & destruct
		public AsyncTextFileWriter( string aFileName )
		{
			iSourceFileName = aFileName;
		}

		public AsyncTextFileWriter( string aFileName, TNotUsingThread aNotUsingThread )
			: this( aNotUsingThread )
		{
			iSourceFileName = aFileName;
		}

		public AsyncTextFileWriter( TNotUsingThread aNotUsingThread )
			: base( aNotUsingThread )
		{
		}
		#endregion

		#region API
		public void ConstructWriter()
		{
			iWriter = new StreamWriter( FileName );
		}

		public void WriteLine( string aLine )
		{
			iWriter.WriteLine( aLine );
		}
		#endregion

		#region Properties
		public StreamWriter Writer
		{
			get { return iWriter; }
		}

		public string FileName
		{
			get { return iSourceFileName; }
		}
		#endregion

		#region From DisposableObject - Cleanup Framework
		protected override void CleanupManagedResources()
		{
			try
			{
				Cleanup();
			}
			finally
			{
				base.CleanupManagedResources();
			}
		}
		#endregion

		#region From AsyncTextWriterBase
		protected override void HandleWriteStarted()
		{
			if	( iWriter == null )
			{
				ConstructWriter();
			}

			base.HandleWriteStarted();
		}

		protected override void HandleWriteCompleted()
		{
			try
			{
				Cleanup();
			}
			finally
			{
				base.HandleWriteCompleted();
			}
		}
		#endregion

		#region Internal methods
		private void Cleanup()
		{
			lock( this )
			{
				if	( iWriter != null )
				{
					iWriter.Close();
					iWriter = null;
				}
			}
		}
		#endregion

		#region Data members
		private StreamWriter iWriter;
		private readonly string iSourceFileName;
		#endregion
	}

	public abstract class AsyncHTMLFileWriter : AsyncTextFileWriter
	{
		#region Construct & destruct
		public AsyncHTMLFileWriter( string aFileName )
            : base( aFileName )
		{
		}

		public AsyncHTMLFileWriter( TNotUsingThread aNotUsingThread )
			: base( aNotUsingThread )
		{
		}

		public AsyncHTMLFileWriter( string aFileName, TNotUsingThread aNotUsingThread )
            : base( aFileName )
		{
		}
		#endregion

		#region Enumerations
		public enum TAlignment
		{
			EAlignNone = -1,
			EAlignLeft = 0,
			EAlignRight,
			EAlignCenter,
			EAlignJustify
		}
		#endregion

		#region Helper methods - document 
		public void WriteDocumentBegin()
		{
			WriteLine( "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" );
			WriteLine( "<HTML>" );
		}

		public void WriteDocumentEnd()
		{
			WriteLine( "</HTML>" );
		}
		#endregion

		#region Helper methods - header
		public void WriteHeadBegin()
		{
			WriteLine( "<HEAD>" );
		}

		public void WriteHeadEnd()
		{
			Writer.WriteLine( "</HEAD>" );
		}

		public void WriteTitle( string aTitle )
		{
			Writer.WriteLine( "<TITLE>" + aTitle + "</TITLE>" );
		}

		public void WriteStyleBegin()
		{
			WriteLine( "<META HTTP-EQUIV=\"Content-Style-Type\" CONTENT=\"text/css\">" );
			WriteLine( "<STYLE TYPE=\"text/css\" MEDIA=\"screen, print, projection\">" );
		}

		public void WriteStyleName( string aName )
		{
			WriteLine( aName );
		}

		public void WriteStyleBodyBegin()
		{
			WriteLine( "{" );
		}

		public void WriteStyleBody( string aStyle )
		{
			WriteLine( "     " + aStyle );
		}

		public void WriteStyleBodyEnd()
		{
			WriteLine( "}" );
		}

		public void WriteStyleEnd()
		{
			Writer.WriteLine( "</STYLE>" );
		}
		#endregion

		#region Helper methods - body
		public void WriteBodyBegin()
		{
			WriteLine( "<BODY onload=\"initialize();\" onmousemove=\"mouseMove(event);\">" );
		}

		public void WriteBodyEnd()
		{
			WriteLine( "</BODY>" );
		}
		#endregion

		#region Helper methods - tables
		public void WriteTableBegin()
		{
			WriteTableBegin( 100 );
		}

		public void WriteTableBegin( int aWidthPercentage )
		{
			WriteLine( "<TABLE WIDTH=\"" + aWidthPercentage.ToString() + "%\">" );
		}

		public void WriteTableBeginWidthPixels( int aWidthPixels, Color aColor )
		{
			string color = ColorString( aColor );
			WriteLine( "<TABLE WIDTH=\"" + aWidthPixels.ToString() + "px\" bgcolor=\"" + color + "\">" );
		}

		public void WriteTableEnd()
		{
			Writer.WriteLine( "</TABLE>" );
		}

		public void WriteTableRowBegin()
		{
			WriteLine( "<TR>" );
		}

		public void WriteTableRowEnd()
		{
			WriteLine( "</TR>" );
		}

		public void WriteTableColumnBegin()
		{
			WriteTableColumnBegin( TAlignment.EAlignNone, string.Empty );
		}

		public void WriteTableColumnBegin( TAlignment aAlignment, string aStyle )
		{
			WriteTableColumnBegin( aAlignment, aStyle, -1 );
		}

		public void WriteTableColumnBegin( TAlignment aAlignment, string aStyle, int aPixelWidth )
		{
			StringBuilder line = new StringBuilder();
			line.Append( "<TD" );
			line.Append( FormattedClass( aStyle ) );
			line.Append( FormattedAlignment( aAlignment ) );
			line.Append( FormattedWidthPixel( aPixelWidth ) );
			line.Append( ">" );
			WriteLine( line.ToString() );
		}

		public void WriteTableColumnEnd()
		{
			WriteLine( "</TD>" );
		}

		public void WriteTableColumn( string aValue )
		{
			WriteTableColumn( aValue, TAlignment.EAlignNone, string.Empty );
		}

		public void WriteTableColumn( string aValue, TAlignment aAlignment )
		{
			WriteTableColumn( aValue, aAlignment, string.Empty );
		}

		public void WriteTableColumn( string aValue, string aStyle )
		{
			WriteTableColumn( aValue, TAlignment.EAlignNone, aStyle );
		}

		public void WriteTableColumn( string aValue, TAlignment aAlignment, string aStyle )
		{
			WriteTableColumnBegin( aAlignment, aStyle );
			Writer.Write( aValue );
			WriteTableColumnEnd();
		}

		public void WriteTableColumnFormatted( long aNumber, string aFormat )
		{
			WriteTableColumnFormatted( aNumber, aFormat, TAlignment.EAlignNone );
		}

		public void WriteTableColumnFormatted( long aNumber, string aFormat, TAlignment aAlignment )
		{
			WriteTableColumnFormatted( aNumber, aFormat, aAlignment, string.Empty );
		}

		public void WriteTableColumnFormatted( long aNumber, string aFormat, TAlignment aAlignment, string aStyle )
		{
			WriteTableColumn( aNumber.ToString( aFormat ), aAlignment, aStyle );
		}

		public void WriteTableColumn( long aNumber )
		{
			WriteTableColumn( aNumber, "d" );
		}

		public void WriteTableColumn( long aNumber, string aStyle )
		{
			WriteTableColumnFormatted( aNumber, "d", TAlignment.EAlignNone, aStyle );
		}

		public void WriteTableColumn( long aNumber, TAlignment aAlignment )
		{
			WriteTableColumnFormatted( aNumber, "d", aAlignment, string.Empty );
		}

		public void WriteTableColumn( long aNumber, TAlignment aAlignment, string aStyle )
		{
			WriteTableColumnFormatted( aNumber, "d", aAlignment, aStyle );
		}

		public void WriteTableColumnHex( long aNumber )
		{
			WriteTableColumnHex( aNumber, string.Empty );
		}

		public void WriteTableColumnHex( long aNumber, string aStyle )
		{
			WriteTableColumnHex( aNumber, TAlignment.EAlignNone, aStyle );
		}

		public void WriteTableColumnHex( long aNumber, TAlignment aAlignment, string aStyle )
		{
			WriteTableColumn( aNumber.ToString( "x8" ), aAlignment, aStyle );
		}

		public void WriteTableColumnHexAddress( long aNumber )
		{
			WriteTableColumnHexAddress( aNumber, TAlignment.EAlignNone );
		}

		public void WriteTableColumnHexAddress( long aNumber, TAlignment aAlignment )
		{
			WriteTableColumnHexAddress( aNumber, aAlignment, string.Empty );
		}

		public void WriteTableColumnHexAddress( long aNumber, TAlignment aAlignment, string aStyle )
		{
			WriteTableColumn( "0x" + aNumber.ToString( "x8" ), aAlignment, aStyle );
		}

		public void WriteTableColumnSpace()
		{
			WriteTableColumnBegin();
			WriteLine( "&nbsp;" );
			WriteTableColumnEnd();
		}

		public void WriteTableColumnSpace( int aPixelWidth )
		{
			WriteTableColumnBegin();
			WriteLine( "&nbsp;" );
			WriteTableColumnEnd();
		}

		public void WriteTableColumnEmpty()
		{
			WriteTableColumnBegin();
			WriteTableColumnEnd();
		}
		#endregion

		#region Helper methods - blocks/paragraphs
		public void WriteDivisionBegin()
		{
			WriteLine( "<DIV>" );
		}

		public void WriteDivisionBegin( string aStyle )
		{
			WriteDivisionBegin( TAlignment.EAlignNone, aStyle );
		}

		public void WriteDivisionBeginWithId( string aId )
		{
			WriteDivisionBegin( TAlignment.EAlignNone, string.Empty, aId );
		}

		public void WriteDivisionBegin( TAlignment aAlignment )
		{
			WriteDivisionBegin( aAlignment, string.Empty );
		}

		public void WriteDivisionBegin( TAlignment aAlignment, string aStyle )
		{
			WriteDivisionBegin( aAlignment, aStyle, string.Empty );
		}

		public void WriteDivisionBegin( TAlignment aAlignment, string aStyle, string aId )
		{
			StringBuilder line = new StringBuilder();
			line.Append( "<DIV" );
			line.Append( FormattedClass( aStyle ) );
			line.Append( FormattedAlignment( aAlignment ) );
			line.Append( FormattedId( aId ) );
			line.Append( ">" );
			//
			WriteLine( line.ToString() );
		}

		public void WriteDivisionEnd()
		{
			WriteLine( "</DIV>" );
		}

		public void WriteParagraphBegin()
		{
			WriteLine( "<P>" );
		}

		public void WriteParagraphBegin( string aClass )
		{
			WriteLine( "<P CLASS=\"" + aClass + "\">" );
		}
		
		public void WriteParagraphEnd()
		{
			WriteLine( "</P>" );
		}

		public void WriteSpanBegin()
		{
			WriteSpanBegin( string.Empty );
		}

		public void WriteSpanBegin( string aClass )
		{
			WriteSpanBegin( aClass, string.Empty );
		}

		public void WriteSpanBegin( string aClass, string aId )
		{
			Writer.Write( "<SPAN" );
			Writer.Write( FormattedClass( aClass ) );
			Writer.Write( FormattedId( aId ) );
			Writer.Write( ">" );
		}
		
		public void WriteSpanEnd()
		{
			WriteLine( "</SPAN>" );
		}

		public void WriteNewLine()
		{
			WriteLine( "<BR>" );
		}
		#endregion

		#region Helper methods - text
		public void WriteText( string aText )
		{
			Writer.Write( aText );
		}

		public void WriteText( string aText, string aStyle )
		{
			Writer.Write( "<SPAN CLASS=\"" + aStyle + "\">" );
			Writer.Write( aText );
			Writer.Write( "</SPAN>" );
		}

		public void WriteLine( string aText, string aStyle )
		{
			StringBuilder line = new StringBuilder();
			//
			line.Append( "<SPAN CLASS=\"" + aStyle + "\">" );
			line.Append( aText );
			line.Append( "</SPAN>" );
			//
			WriteLine( line.ToString() );
		}

		public void WriteSpace()
		{
			Writer.Write( "&nbsp;" );
		}
		#endregion

		#region Helper methods - links (anchors)
		public void WriteAnchorBegin( string aPageAddress )
		{
			WriteAnchorBeginWithStyle( aPageAddress, string.Empty );
		}

		public void WriteAnchorBeginWithStyle( string aPageAddress, string aStyle )
		{
			StringBuilder line = new StringBuilder();
			line.Append( "<A " );
			line.Append( FormattedClass( aStyle ) );
			line.Append( "HREF=\"" );
			line.Append( aPageAddress );
			line.Append( "\"" );
			//
			line.Append( ">" );
			Writer.Write( line.ToString() );
		}

		public void WriteAnchorEnd()
		{
			WriteLine( "</A>" );
		}

		public void WriteAnchorWithName( string aName )
		{
			WriteAnchorWithName( aName, string.Empty );
		}

		public void WriteAnchorWithName( string aName, string aValue )
		{
			StringBuilder line = new StringBuilder();
			line.Append( "<A " );
			line.Append( "NAME=\"#" );
			line.Append( aName );
			line.Append( "\"" );
			line.Append( ">" );
			line.Append( aValue );
			line.Append( "</A>" );
			Writer.Write( line.ToString() );
		}

		public void WriteAnchorWithTarget( string aTargetName, string aURL, string aText )
		{
			StringBuilder line = new StringBuilder();
			line.Append( "<A " );
			line.Append( "TARGET=\"" );
			line.Append( aTargetName );
			line.Append( "\" " );
			line.Append( "HREF=\"" );
			line.Append( aURL );
			line.Append( "\"" );
			line.Append( ">" );
			line.Append( aText );
			line.Append( "</A>" );
			Writer.Write( line.ToString() );
		}
		#endregion

		#region Helper methods - colors
		public static string ColorString( Color aColor )
		{
			StringBuilder ret = new StringBuilder();
			//
			ret.Append( "#" );
			ret.Append( aColor.R.ToString("x2") );
			ret.Append( aColor.G.ToString("x2") );
			ret.Append( aColor.B.ToString("x2") );
			//
			return ret.ToString();
		}
		#endregion

		#region Internal methods
		static string FormattedId( string aId )
		{
			string ret = string.Empty;
			//
			if	( aId != string.Empty ) 
			{
				ret = " ID=\"" + aId + "\" ";
			}
			//
			return ret;
		}

		static string FormattedClass( string aStyleName )
		{
			string ret = string.Empty;
			//
			if	( aStyleName != string.Empty ) 
			{
				ret = " CLASS=\"" + aStyleName + "\"";
			}
			//
			return ret;
		}

		static string FormattedWidthPixel( int aWidth )
		{
			string ret = string.Empty;
			//
			if	( aWidth > 0 ) 
			{
				ret = " WIDTH=\"" + aWidth.ToString() + "px\"";
			}
			//
			return ret;
		}

		static string FormattedWidthPercent( int aWidth )
		{
			string ret = string.Empty;
			//
			if	( aWidth > 0 ) 
			{
				ret = " WIDTH=\"" + aWidth.ToString() + "%\"";
			}
			//
			return ret;
		}

		static string FormattedAlignment( TAlignment aAlignment )
		{
			StringBuilder line = new StringBuilder();
			//
			if	( aAlignment != TAlignment.EAlignNone )
			{
				line.Append( " ALIGN=\"" );
				switch( aAlignment )
				{
					case TAlignment.EAlignLeft:
						line.Append( "LEFT" );
						break;
					case TAlignment.EAlignRight:
						line.Append( "RIGHT" );
						break;
					case TAlignment.EAlignCenter:
						line.Append( "CENTER" );
						break;
					case TAlignment.EAlignJustify:
						line.Append( "JUSTIFY" );
						break;
					default:
					case TAlignment.EAlignNone:
						break;
				}
				line.Append( "\"" );
			}
			//
			return line.ToString();
		}
		#endregion
	}
}
