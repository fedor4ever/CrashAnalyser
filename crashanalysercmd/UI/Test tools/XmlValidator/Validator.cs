using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;        
using System.Xml.Schema; 
	

namespace XmlValidator
{
    class Validator
    {

        static void Main(string[] args)
        {
            string dtdFile = string.Empty;
            string xmlFile = string.Empty;

            if (args.Length == 2)
            {
                dtdFile = args[0];
                xmlFile = args[1];
            }
            else
            {
                Console.WriteLine("Usage: XmlValidator.exe dtdfile.dtd xmlfile.xml");
                return;
            }
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.DTD;
                settings.ValidationEventHandler += XmlValidationEventHandler;
                settings.ProhibitDtd = false;
                XmlReader xmlReader = XmlReader.Create(xmlFile, settings);

                while (xmlReader.Read())
                {
                    // Do nothing
                }
                xmlReader.Close();
            }
            catch (XmlException /* xmle */)
            {
                Console.Write("Not valid");
                return;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine("File not found!");
                Console.WriteLine(ex.Message);
                return;
            }
            
            if (isValid)
            {
                Console.Write("Valid");
            }
            else
            {
                Console.Write("Not valid");
            }

        }

        public static void XmlValidationEventHandler(object sender, ValidationEventArgs args)
        {
            isValid = false;
//            Console.WriteLine("Validation event\n" + args.Message);
        }
        private static bool isValid = true;
    }
}
