using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;

namespace GedcomViewCompare
{
    static class CHelper
    {
        public static XmlDocument initializeXmlDocument()
        {
            XmlDocument theXmlDocument = new XmlDocument();
            XmlDeclaration theDeclaration;
            XmlProcessingInstruction theProcessingInstruction;
            String theProcessingText;
            XmlElement catalogElement = null;

            // Create an Xml declaration (xml-header)
            theDeclaration = theXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            // Create a processing instruction.
            theProcessingText = getProcessingInstruction();
            theProcessingInstruction = theXmlDocument.CreateProcessingInstruction("xml-stylesheet", theProcessingText);

            theXmlDocument.AppendChild(theDeclaration);// Create the root element
            theXmlDocument.AppendChild(theProcessingInstruction);// Create the root element
            catalogElement = theXmlDocument.CreateElement("CATALOG");
            theXmlDocument.AppendChild(catalogElement);

            return theXmlDocument;
        }

        public static XDocument initalizeXDocument()
        {
            XmlDocument theXmlDocument = initializeXmlDocument();
            XDocument theXDocument = XDocument.Load(new XmlNodeReader(theXmlDocument));

            XmlDeclaration theDeclaration =
                theXmlDocument.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
            if (theDeclaration != null)
            {
                theXDocument.Declaration = new XDeclaration(theDeclaration.Version, theDeclaration.Encoding,
                    theDeclaration.Standalone);
            }

            XmlProcessingInstruction theXmlProcessingInstruction =
                theXmlDocument.ChildNodes.OfType<XmlProcessingInstruction>().FirstOrDefault();
            if (theXmlProcessingInstruction != null)
            {
                theXDocument.AddFirst(new XProcessingInstruction(theXmlProcessingInstruction.Target, theXmlProcessingInstruction.Data));
            }
            return theXDocument;
        }

        public static FlowDocument parseXmlToGEDCOM(XDocument theXDocument)
        {
            FlowDocument theFlowDocument = new FlowDocument();
            Paragraph textToInsert = new Paragraph();

            foreach (XElement theXElement in theXDocument.Element(CConstants.xmlElementCatalog).Descendants())
            {
                String[] levelAndNameArray = splitXmlNameIntoConstituents(theXElement);
                if (levelAndNameArray[1] == "0")
                {
                    textToInsert = new Paragraph();

                    textToInsert.Inlines.Add(levelAndNameArray[1]
                             + " "
                             + levelAndNameArray[0]
                             + " "
                             + levelAndNameArray[2]);

                    if (levelAndNameArray[0] == CConstants.particleHead)
                    {
                        textToInsert.Name = "header";
                    }
                    else if (levelAndNameArray[2] == CConstants.particleFam)
                    {
                        textToInsert.Name = "FAM_" + levelAndNameArray[2].Replace("@", "");
                    }
                    else if (levelAndNameArray[2] == CConstants.particleIndi)
                    {
                        textToInsert.Name = "INDI_" + levelAndNameArray[2].Replace("@", "");
                    }
                }
                else
                {
                    textToInsert.Inlines.Add("\r\n"
                                             + levelAndNameArray[1]
                                             + " "
                                             + levelAndNameArray[0]
                                             + " "
                                             + levelAndNameArray[2]);
                }
                theFlowDocument.Blocks.Add(textToInsert);
            }
            return theFlowDocument;
        }

        private static String[] splitXmlNameIntoConstituents(XElement theXElement)
        {
            String[] theStringArray = new String[3];

            if (theXElement.Name != CConstants.xmlElementPerson &&
                theXElement.Name != CConstants.xmlElementFamily)
            {
                string elementName = theXElement.Name.ToString();
                theStringArray = elementName.Split(new string[] { "_L" }, StringSplitOptions.None);
                Array.Resize(ref theStringArray, 3);
                if (theXElement.Attribute(CConstants.xmlAttributeValue) != null)
                {
                    theStringArray[2] = theXElement.Attribute(CConstants.xmlAttributeValue).Value;
                }
                else
                {
                    theStringArray[2] = "";
                }
            }
            else
            {
                string elementName = theXElement.Name.ToString();
                String[] theBufferArray = elementName.Split(new string[] { "_L" }, StringSplitOptions.None);
                //For individual's and family entries, value and identifier are reversed
                theStringArray[0] = theXElement.Attribute(CConstants.xmlAttributeOrigID).Value;
                theStringArray[1] = theBufferArray[1];
                theStringArray[2] = theBufferArray[0];
            }

            return theStringArray;
        }

        public static XmlDocument convertXDocToXmlDoc(XDocument theXDocument)
        {
            using (XmlReader theXmlReader = theXDocument.CreateReader())
            {
                XmlDocument theXmlDoc = new XmlDocument();
                theXmlDoc.Load(theXmlReader);
                if (theXDocument.Declaration != null)
                {
                    XmlDeclaration theDeclaration = theXmlDoc.CreateXmlDeclaration(theXDocument.Declaration.Version,
                        theXDocument.Declaration.Encoding, theXDocument.Declaration.Standalone);
                    theXmlDoc.InsertBefore(theDeclaration, theXmlDoc.FirstChild);
                }
                return theXmlDoc;
            }
        }

        public static string getApplicationPath()
        {
            return Path.GetDirectoryName(
                     Assembly.GetAssembly(typeof(CHelper)).CodeBase);
        }

        public static string getProcessingInstruction()
        {
            string styleFileLocation = getApplicationPath();
            styleFileLocation = styleFileLocation.Replace("file:\\", "file:///");
            styleFileLocation = styleFileLocation.Replace('\\', '/');
            return "type='text/xsl' href='" + styleFileLocation + "/Stylefile.xsl'";
        }
    }
}
