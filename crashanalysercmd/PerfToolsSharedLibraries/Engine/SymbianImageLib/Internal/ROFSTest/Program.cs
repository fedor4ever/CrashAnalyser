using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianImageLib.E32Image;
using SymbianImageLib.ROFS.Image;
using SymbianImageLib.ROFS.File;

namespace ROFSTest
{
    class Program : ITracer
    {
        static void Main( string[] args )
        {
            Program p = new Program();
        }

        #region Constructor
        public Program()
        {
            SymbianE32Image memSpyDeflated = new SymbianE32Image( new FileInfo( @"C:\Tool Demo Files\2. Crash Data\IvaloImage\MemSpyDriverClient.epoc32_include.dll" ), this );
            memSpyDeflated.Decompress( TSynchronicity.ESynchronous );
            byte[] deflateCode = memSpyDeflated.GetAllData();

            using ( FileStream stream = new FileStream( @"C:\Tool Demo Files\2. Crash Data\IvaloImage\ivalo\CoreImage\sp_rnd_image.rofs1.img", FileMode.Open ) )
            {
                SymbianImageROFS rofsImage = new SymbianImageROFS( this, stream );

                ISymbianFileROFS trkRofsFile = rofsImage[ @"\Trk.ini" ];
                System.Diagnostics.Debug.Assert( trkRofsFile != null );
                byte[] trkData = trkRofsFile.GetAllData();
                string trkIniText = SymbianUtils.Strings.StringParsingUtils.BytesToString( trkData );

                ISymbianFileROFS memSpyRofsFile = rofsImage[ @"\Sys\Bin\MemSpyDriverClient.dll" ];
                System.Diagnostics.Debug.Assert( memSpyRofsFile != null );
                memSpyRofsFile.PrepareContent( TSynchronicity.ESynchronous );

                byte[] bytePairCode = memSpyRofsFile.GetAllData();

                System.Diagnostics.Debug.Assert( bytePairCode.Length == deflateCode.Length );
                for ( int i = 0; i < bytePairCode.Length; i++ )
                {
                    System.Diagnostics.Debug.Assert( bytePairCode[ i ] == deflateCode[ i ] );
                }
            }
        }
        #endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            System.Diagnostics.Debug.WriteLine( aMessage );
            System.Console.WriteLine( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string text = string.Format( aFormat, aParams );
            Trace( text );
        }
        #endregion
    }
}
