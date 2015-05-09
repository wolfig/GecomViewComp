using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;


namespace GedcomViewCompare
{
    interface IDataManipulator
    {
        XmlDocument theXmlDocument1 { set; get; }
        XmlDocument theXmlDocument2 { set; get; }
        XDocument theXDocument1 { set; get; }
        XDocument theXDocument2 { set; get; }

        XmlDocument compareDocumentContent(XDocument masterXDocument, XDocument slaveXDocument, RichTextBox masterDocumentTextBox);
        CConstants.dataManipulatorStructure createDifferenceData();
        CConstants.dataManipulatorStructure createIntersectionData();
        CConstants.dataManipulatorStructure createUnionData();
        string getPersonCountByCheckAttr(XDocument theXDocument, string checkAttrValue);
        void sortDocumentBySurnames(RichTextBox theRichTextBox, XDocument theXDocument);
    }
}
