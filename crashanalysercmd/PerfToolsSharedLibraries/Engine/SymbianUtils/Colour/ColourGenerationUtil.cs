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
using System.Collections.Generic;
using System.Drawing;

namespace SymbianUtils.Colour
{
    public class ColourGenerationUtil
    {
        #region Constructors
        public ColourGenerationUtil()
        {
            iStandardColors = CreateStandardColors();
            iBannedColors = CreateBannedColors();
        }
        #endregion

        #region API
        public Color GenerateRandomColour( Random aRandomNumberGenerator )
        {
            Color ret = Color.White;
            //
            bool blackListed = false;
            do
            {
                int colItemIndex = aRandomNumberGenerator.Next( (int) KnownColor.Aqua, (int) KnownColor.YellowGreen );
                KnownColor col = (KnownColor) colItemIndex;
                //
                blackListed = BannedColors.Contains( col );
                ret = Color.FromKnownColor( col );
            }
            while ( blackListed );
            //
            return ret;
        }

        public Color GenerateRandomColourAndRemoveFromList()
        {
            int count = iStandardColors.Count;
            System.Random random = new Random();
            int colItemIndex = random.Next( 0, count );
            Color ret = Color.FromKnownColor( iStandardColors[ colItemIndex ] );
            iStandardColors.RemoveAt( colItemIndex );
            return ret;
        }

        public void SuplimentStandardColoursWithAdditionalEntries( int aRequiredEntryCount )
        {
            // Pick colours at random from the standard colours until we have used them all
            System.Random random = new Random();

            // How many objects do we have to colourise?
            int iterations = 0;
            int maxIterations = (int) KnownColor.MenuHighlight - iBannedColors.Count;
            while ( iStandardColors.Count < aRequiredEntryCount && iterations < maxIterations )
            {
                int colItemIndex = random.Next( (int) KnownColor.Aqua, (int) KnownColor.YellowGreen );
                KnownColor col = (KnownColor) colItemIndex;
                //
                bool alreadyExists = iStandardColors.Contains( col );
                bool isBanned = iBannedColors.Contains( col );
                if ( !alreadyExists && !isBanned )
                {
                    iStandardColors.Add( col );
                }

                ++iterations;
            }

            // If we still dont' have enough colours at this point, then just duplicate some of the
            // known colours
            iterations = 0;
            maxIterations = iStandardColors.Count;
            while ( iStandardColors.Count < aRequiredEntryCount )
            {
                KnownColor col = iStandardColors[ iterations ];
                iStandardColors.Add( col );
                //
                if ( iterations >= maxIterations )
                {
                    iterations = 0;
                }
            }
        }

        public List<KnownColor> CreateStandardColors()
        {
            List<KnownColor> list = new List<KnownColor>();
            //
            list.Add( KnownColor.Red );
            list.Add( KnownColor.IndianRed );
            list.Add( KnownColor.Tomato );
            list.Add( KnownColor.SandyBrown );
            list.Add( KnownColor.Moccasin );
            list.Add( KnownColor.Gold );
            list.Add( KnownColor.Yellow );
            list.Add( KnownColor.YellowGreen );
            list.Add( KnownColor.LawnGreen );
            list.Add( KnownColor.ForestGreen );
            list.Add( KnownColor.Aquamarine );
            list.Add( KnownColor.LightSeaGreen );
            list.Add( KnownColor.Cyan );
            list.Add( KnownColor.SkyBlue );
            list.Add( KnownColor.DodgerBlue );
            list.Add( KnownColor.LightSteelBlue );
            list.Add( KnownColor.SlateBlue );
            list.Add( KnownColor.DarkOrchid );
            list.Add( KnownColor.Fuchsia );
            list.Add( KnownColor.Pink );
            //
            return list;
        }

        public List<KnownColor> CreateBannedColors()
        {
            List<KnownColor> list = new List<KnownColor>();
            //
            list.Add( KnownColor.Black );
            list.Add( KnownColor.LightGray );
            list.Add( KnownColor.White );
            list.Add( KnownColor.Gray );
            list.Add( KnownColor.GrayText );
            list.Add( KnownColor.DarkGray );
            list.Add( KnownColor.DimGray );
            list.Add( KnownColor.Azure );
            list.Add( KnownColor.Silver );
            list.Add( KnownColor.GhostWhite );
            list.Add( KnownColor.DarkKhaki );
            list.Add( KnownColor.DarkOliveGreen );
            list.Add( KnownColor.NavajoWhite );
            list.Add( KnownColor.Ivory );
            list.Add( KnownColor.Cornsilk );
            list.Add( KnownColor.Honeydew );
            list.Add( KnownColor.AliceBlue );
            list.Add( KnownColor.Gainsboro );
            list.Add( KnownColor.Beige );
            list.Add( KnownColor.Lavender );
            list.Add( KnownColor.FloralWhite );
            //
            list.Add( KnownColor.Desktop );
            list.Add( KnownColor.AppWorkspace );
            list.Add( KnownColor.Transparent );
            list.Add( KnownColor.Bisque );
            list.Add( KnownColor.Control );
            list.Add( KnownColor.ControlText );
            list.Add( KnownColor.ControlDark );
            list.Add( KnownColor.ControlDarkDark );
            list.Add( KnownColor.ControlLight );
            list.Add( KnownColor.ControlLightLight );
            list.Add( KnownColor.ButtonFace );
            list.Add( KnownColor.ButtonHighlight );
            list.Add( KnownColor.ButtonShadow );
            list.Add( KnownColor.GradientActiveCaption );
            list.Add( KnownColor.GradientInactiveCaption );
            list.Add( KnownColor.HotTrack );
            list.Add( KnownColor.Menu );
            list.Add( KnownColor.MenuBar );
            list.Add( KnownColor.MenuHighlight );
            list.Add( KnownColor.MenuText );
            list.Add( KnownColor.ActiveBorder );
            list.Add( KnownColor.ActiveCaption );
            list.Add( KnownColor.ActiveCaptionText );
            list.Add( KnownColor.InactiveBorder );
            list.Add( KnownColor.InactiveCaption );
            list.Add( KnownColor.InactiveCaptionText );
            list.Add( KnownColor.Highlight );
            list.Add( KnownColor.HighlightText );
            list.Add( KnownColor.BlanchedAlmond );
            list.Add( KnownColor.Info );
            list.Add( KnownColor.InfoText );
            list.Add( KnownColor.Window );
            list.Add( KnownColor.WindowText );
            list.Add( KnownColor.WindowFrame );
            list.Add( KnownColor.ScrollBar );
            list.Add( KnownColor.LightBlue );
            //
            return list;
        }
        #endregion

        #region Properties
        public List<KnownColor> StandardColors
        {
            get { return iStandardColors; }
        }

        public List<KnownColor> BannedColors
        {
            get { return iBannedColors; }
        }
        #endregion

        #region Data members
        private readonly List<KnownColor> iStandardColors;
        private readonly List<KnownColor> iBannedColors;
        #endregion
    }
}
