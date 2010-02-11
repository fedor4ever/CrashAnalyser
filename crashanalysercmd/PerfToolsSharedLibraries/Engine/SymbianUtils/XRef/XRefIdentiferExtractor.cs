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
using SymbianUtils.Strings;

namespace SymbianUtils.XRef
{
    public class XRefIdentiferExtractor
    {
        #region Constructors
        public XRefIdentiferExtractor( string aFunction )
        {
            iIdentifiers = ExtractSearchableElements( aFunction );
        }
        #endregion

        #region Properties
        public List<XRefIdentifer> Identifiers
        {
            get { return iIdentifiers; }
        }
        #endregion

        #region Internal methods
        private static bool ContainsParameters( string aText )
        {
            // Search initiall for '(' - if that is found, then
            // we should also find a closing bracket.
            bool parameters = false;
            int openingBracketPos = aText.IndexOf( "(" );
            //
            if ( openingBracketPos > 0 )
            {
                // Should also be a closing bracket and it should
                // appear after the opening bracket
                int closingBracketPos = aText.LastIndexOf( ")" );
                parameters = ( closingBracketPos > openingBracketPos );
            }
            //
            return parameters;
        }

        private static string ExtractParameters( ref string aText )
        {
            const string KOperatorChevronText = "operator <<";

            // DoAppendFormatList<TDes16, (int)2>(T1&, const T3&, std::__va_list, T2*)
            // DoAppendFormatList<TDes16, (int)2>(T1&, TBuf<(int)256>, std::__va_list, T2*)
            // Method<TDes16>::Wibble( something )
            // Method::Wibble( RPointerArray<HBufC> )
            // RTest::operator ()(int, int, const unsigned short*)
            // TDesC16::Left(int) const
            // CObjectCon::AtL(int) const
            // User::Panic(const TDesC16&, int)
            // operator <<(RWriteStream&, const unsigned char&)

            // Handle special case of "operator <<" confusing matters
            string workingText = aText;
            int operatorOpeningChevronPos = aText.IndexOf( KOperatorChevronText );
            if ( operatorOpeningChevronPos >= 0 )
            {
                aText = aText.Substring( 0, operatorOpeningChevronPos + KOperatorChevronText.Length );
                workingText = workingText.Substring( operatorOpeningChevronPos + KOperatorChevronText.Length );
            }
            else
            {
                aText = string.Empty;
            }

            string ret = string.Empty;
            //
            int closingPos = 0;
            int openingPos = 0;
            int templatePos = 0;
            //
            while ( openingPos >= 0 )
            {
                if ( templatePos >= 0 )
                    templatePos = workingText.IndexOf( "<", templatePos );
                openingPos = workingText.IndexOf( "(", openingPos );

                if ( templatePos >= 0 && templatePos < openingPos )
                {
                    // Template region appears before the next bracket. Skip
                    // over all characters until we hit the end of the template
                    // section
                    int endingPos = templatePos;
                    StringParsingUtils.SkipToEndOfSection( ref workingText, ref endingPos, '<', '>' );

                    if ( endingPos < 0 )
                    {
                        // Matching closing brace was never found - dealing with operator << ?
                        templatePos = -1;
                    }
                    else
                    {
                        // Something like DoAppendFormatList<TDes16, (int)2>(T1&, const T3&, std::__va_list, T2*) ???
                        templatePos = endingPos;
                        openingPos = endingPos;
                    }
                }
                else if ( openingPos >= 0 )
                {
                    // Skipped over any template nonsense. Work backward from the end 
                    // in order to locate start of parameters.
                    closingPos = workingText.LastIndexOf( ')' );
                    openingPos = closingPos;
                    StringParsingUtils.SkipToBeginningOfSection( ref workingText, ref openingPos, '(', ')' );

                    string parameters = workingText.Substring( openingPos + 1, ( closingPos - openingPos ) - 1 ).Trim();
                    ret = parameters;
                    workingText = workingText.Substring( 0, openingPos + 1 ) + workingText.Substring( closingPos );
                    aText = aText + workingText;
                    break;
                }
            }
            //
            return ret;
        }

        private static void SplitParameters( string aParams, ref List<string> aEntries )
        {
            /*
             * TPtrC16::TPtrC16(const unsigned short*) 
             * TPtrC16::TPtrC16(const TDesC16&) 
             * UserHal::MemoryInfo(TDes8&) 
             * RHandleBase::Close() 
             * TBufCBase16::Copy(const TDesC16&, int) 
             * CBufFlat::NewL(int) 
             * TBufCBase16::TBufCBase16() 
             * CServer2::RunL() 
             * CServer2::StartL(const TDesC16&) 
             * CServer2::DoCancel() 
             * CServer2::RunError(int) 
             * CServer2::DoConnect(const RMessage2&) 
             * CServer2::CServer2__sub_object(int, CServer2::TServerType) 
             */
            string paramType;
            while ( aParams.Length > 0 )
            {
                int commaPos = aParams.IndexOf( "," );
                //
                paramType = aParams;
                if ( commaPos > 0 )
                {
                    paramType = aParams.Substring( 0, commaPos ).Trim();
                    if ( commaPos < aParams.Length )
                        aParams = aParams.Substring( commaPos + 1 ).Trim();
                    else
                        aParams = string.Empty;
                }
                else
                {
                    // Everything was consumed
                    aParams = string.Empty;
                }

                // Add it
                aEntries.Add( paramType );
            }
        }

        private static string[] IdentifyFurtherClassAndMethodInformation( string aEntry )
        {
            string[] classAndMethodData = aEntry.Split( new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries );
            return classAndMethodData;
        }

        private static List<XRefIdentifer> ExtractSearchableElements( string aFunction )
        {
            List<string> workingData = new List<string>();

            // See if this entry contains parameter data?
            if ( ContainsParameters( aFunction ) )
            {
                // This call modifies aFunction so that the parameters are removed
                string parameters = ExtractParameters( ref aFunction );

                // Now we get the individual parameter elements from the arguments
                SplitParameters( parameters, ref workingData );
            }

            // Extract class & method names if present
            string[] classAndMethodData = aFunction.Split( new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries );
            if ( classAndMethodData.Length > 0 )
            {
                foreach ( string identifiedEntry in classAndMethodData )
                {
                    workingData.Add( identifiedEntry );
                }
            }
            else
            {
                // Not a class & method - so just take the entire text as a 
                // global function name
                workingData.Add( aFunction );
            }

            // Recursively check for any more class types, e.g. in the following entry:
            //
            // CServer2::CServer2__sub_object(int, CServer2::TServerType) 
            //
            // We also need to extract CServer2 and TServerType as individual elements
            for ( int i = workingData.Count - 1; i >= 0; i-- )
            {
                string entry = workingData[ i ];
                string[] furtherSplit = IdentifyFurtherClassAndMethodInformation( entry );
                if ( furtherSplit != null && furtherSplit.Length > 1 )
                {
                    foreach ( string furtherEntry in furtherSplit )
                    {
                        workingData.Add( furtherEntry );
                    }

                    // Remove defunct entry
                    workingData.RemoveAt( i );
                }
            }

            // Clean up phase
            List<string> cleanedEntries = new List<string>();
            foreach ( string identifiedEntry in workingData )
            {
                string entry = identifiedEntry;

                // Now go through the identified entries and ensure they don't include
                // any pointer (*), reference (&) or bracketry.
                int pos = entry.IndexOfAny( new char[] { '*', '+', '&', '[', ']', '<', '>', '(', ')' } );
                if ( pos >= 0 )
                {
                    entry = entry.Substring( 0, pos );
                }

                // Strip any reserved keywords
                if ( entry.Length > 0 )
                {
                    entry = entry.Replace( "const", string.Empty );
                    entry = entry.Replace( "static", string.Empty );
                    entry = entry.Replace( "public", string.Empty );
                    entry = entry.Replace( "protected", string.Empty );
                    entry = entry.Replace( "private", string.Empty );
                    entry = entry.Replace( "__sub_object", string.Empty );
                    //
                    if ( !cleanedEntries.Contains( entry ) )
                    {
                        cleanedEntries.Add( entry.Trim() );
                    }
                }
            }

            // Convert to XRefIdentifiers
            List<XRefIdentifer> finalEntries = new List<XRefIdentifer>();
            foreach ( string cleanedEntry in cleanedEntries )
            {
                XRefIdentifer item = new XRefIdentifer( cleanedEntry );
                finalEntries.Add( item );
            }
            //
            return finalEntries;
        }
        #endregion

        #region Data members
        private readonly List<XRefIdentifer> iIdentifiers;
        #endregion
    }
}
