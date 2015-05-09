using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace GedcomViewCompare
{
    class CGedcomFileHandler
    {
        private double _progressPercentage;
        private double progressPercentage
        {
            get { return this._progressPercentage; }
            set { this._progressPercentage = value;  }
        }

        private string _encoding;
        private string encoding
        {
            get { return this._encoding; }
            set { this._encoding = value; }
        }

        private CConstants.fileHandlerReturnStruct output;

        public CGedcomFileHandler()
        {
            //Create structured output Element
            output = new CConstants.fileHandlerReturnStruct();
            output.xmlDoc = new XmlDocument();
            output.flowDoc = new FlowDocument();
            output.personCounter = 0;
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public CConstants.fileHandlerReturnStruct parseGedcomToXml(string fileName)
        {
            String sourceString;
            String uidString = CConstants.uidTemplate;//use this as template for well defined positions of entries
            String familyUidString = CConstants.familyUidTemplate;
            Paragraph textToInsert = new Paragraph();
            int lineCounter = 0;
            //***********************************************************
            // XML Element Definitions
            //***********************************************************
            XmlElement catalogElement = null;
            XmlElement l0_Element = null;
            XmlElement l1_Element = null;
            XmlElement l2_Element = null;
            XmlElement l3_Element = null;
            XmlElement unknowEntryElement = null;
            XmlElement unknowEntryElements = null;
            //***********************************************************
            // End XML Elements
            //***********************************************************
            //Call progress popup
            ProgressPopup thePopup = new ProgressPopup();
            thePopup.theProgressBar.Maximum = 100;
            thePopup.theProgressBar.Minimum = 0;
            thePopup.Show();

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(thePopup.theProgressBar.SetValue);
            UpdateProgressBarDelegate updateTextDelegate = new UpdateProgressBarDelegate(thePopup.progressText.SetValue);

            TextReader textFromFile = new StreamReader(fileName);
            sourceString = textFromFile.ReadToEnd();
            String[] sourceLines = sourceString.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int numberOfLines = sourceLines.Count();

            output.xmlDoc = CHelper.initializeXmlDocument();

            foreach (String sourceLine in sourceLines)
            {
                if (sourceLine.Contains(CConstants.debugIndicator))
                {
                    hardBreakpoint();
                }
                else
                {
                    //count lines and send updates to status popup
                    lineCounter++;
                    progressPercentage = (double)lineCounter * 100d / (double)numberOfLines;

                    if (lineCounter % 100 == 0)
                    {
                        string progressText = lineCounter.ToString() + " of " + numberOfLines.ToString() + " lines processed";
                        progressPercentage = (double)lineCounter * 100d / (double)numberOfLines;

                        Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate,
                                                            System.Windows.Threading.DispatcherPriority.Background,
                                                            new object[] { ProgressBar.ValueProperty, progressPercentage });
                        Dispatcher.CurrentDispatcher.Invoke(updateTextDelegate,
                                        System.Windows.Threading.DispatcherPriority.Background,
                                        new object[] { Label.ContentProperty, progressText });
                    }

                    //The real file to XML parsing starts here:
                    String[] informationConstituents = new String[3];
                    informationConstituents = splitLineIntoConstituents(sourceLine);

                    switch (determineNestingLevel(informationConstituents[0]))
                    {
                        case 0:

                            textToInsert = new Paragraph();

                            switch (informationConstituents[0])
                            {
                                case CConstants.xmlElementHeader:
                                    textToInsert.Name = "header";
                                    l0_Element = output.xmlDoc.CreateElement(informationConstituents[0]);
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    break;
                                case CConstants.xmlElementPerson:
                                    output.personCounter++;
                                    textToInsert.Name = "INDI_" + informationConstituents[1].Replace("@", "");

                                    //Generate own person ID (should be the same in both files)
                                    // This has to be done BEFORE new element is created
                                    //l0_Element = generatePersonUID(uidString, l0_Element, output.xmlDoc);

                                    //Create new element
                                    l0_Element = output.xmlDoc.CreateElement(informationConstituents[0]);
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    //Write old ID to element (differ in files, needed to reconstruct family tree)
                                    try { l0_Element = createNewAttribute(output.xmlDoc, l0_Element, CConstants.xmlAttributeOrigID, informationConstituents[1]); }
                                    catch { }
                                    try { l0_Element = createNewAttribute(output.xmlDoc, l0_Element, CConstants.xmlAttributeFile, fileName); }
                                    catch { }
                                    uidString = CConstants.uidTemplate;
                                    break;
                                case CConstants.xmlElementFamily:
                                    textToInsert.Name = "FAM_" + informationConstituents[1].Replace("@", "");
                                    //Generate own person ID (should be the same in both files)
                                    //Has to be done in Family-section for the last person in file
                                    /*
                                    if (l0_Element.Name == CConstants.xmlElementPerson)
                                    {
                                        l0_Element = generatePersonUID(uidString, l0_Element, output.xmlDoc);
                                    }
                                    */
                                    if (l0_Element.Name == CConstants.xmlElementFamily)
                                    {
                                        l0_Element = generatePersonUID(familyUidString, l0_Element, output.xmlDoc);
                                    }
                                    else
                                    {
                                        l0_Element = output.xmlDoc.CreateElement(informationConstituents[0]);
                                    } 
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    //Write old ID to element (differ in files, needed to reconstruct family tree)
                                    try { l0_Element = createNewAttribute(output.xmlDoc, l0_Element, "OrigID", informationConstituents[1]); }
                                    catch { }
                                    try { l0_Element = createNewAttribute(output.xmlDoc, l0_Element, CConstants.xmlAttributeFile, fileName); }
                                    catch { }

                                    familyUidString = CConstants.familyUidTemplate;
                                    break;
                                case CConstants.xmlElementRepository:
                                    try
                                    {
                                        textToInsert.Name = "REPO_" + informationConstituents[1].Replace("@", "");
                                    }
                                    catch (ArgumentException) { }
                                    if (l0_Element.Name == CConstants.xmlElementPerson)
                                    {
                                        //l0_Element = generatePersonUID(uidString, l0_Element, output.xmlDoc);
                                    }
                                    else if (l0_Element.Name == CConstants.xmlElementFamily)
                                    {
                                        l0_Element = generatePersonUID(familyUidString, l0_Element, output.xmlDoc);
                                    }
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    break;
                                case CConstants.xmlElementSourceL0:
                                    try
                                    {
                                        textToInsert.Name = "SOUR_" + informationConstituents[1].Replace("@", "");
                                    }
                                    catch (ArgumentException) { }
                                    if (l0_Element.Name == CConstants.xmlElementPerson)
                                    {
                                        //l0_Element = generatePersonUID(uidString, l0_Element, output.xmlDoc);
                                    }
                                    else if (l0_Element.Name == CConstants.xmlElementFamily)
                                    {
                                        l0_Element = generatePersonUID(familyUidString, l0_Element, output.xmlDoc);
                                    }
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    break;
                                case CConstants.debugIndicator:
                                    hardBreakpoint();
                                    //l0_Element = generatePersonUID(uidString, l0_Element, output.xmlDoc);
                                    l0_Element = output.xmlDoc.CreateElement(CConstants.xmlElementPerson);
                                    l0_Element = createNewAttribute(output.xmlDoc, l0_Element, CConstants.xmlAttributeOrigID, "");
                                    l0_Element.InnerText = CConstants.debugIndicator;
                                    output.xmlDoc.DocumentElement.AppendChild(l0_Element);
                                    break;
                            }
                            break;
                        case 1:
                            l1_Element = addChildElement(output.xmlDoc, l0_Element, informationConstituents[0]);
                            try { l1_Element = createNewAttribute(output.xmlDoc, l1_Element, CConstants.xmlAttributeValue, informationConstituents[1]); }
                            catch (IndexOutOfRangeException) { l1_Element = createNewAttribute(output.xmlDoc, l1_Element, CConstants.xmlAttributeValue, "N/A"); }

                            uidString = buildUidString(informationConstituents, uidString, l1_Element, output);
                            familyUidString = buildFamilyUidString(informationConstituents, familyUidString, l1_Element, output.xmlDoc);

                            //Determine encoding from header
                            if (informationConstituents[0] == CConstants.xmlElementCharacterEncoding) encoding = informationConstituents[1];

                            break;
                        case 2:
                            l2_Element = addChildElement(output.xmlDoc, l1_Element, informationConstituents[0]);
                            try { l2_Element = createNewAttribute(output.xmlDoc, l2_Element, CConstants.xmlAttributeValue, informationConstituents[1]); }
                            catch (IndexOutOfRangeException) { l2_Element = createNewAttribute(output.xmlDoc, l2_Element, CConstants.xmlAttributeValue, "N/A"); }
                            if (informationConstituents[1] != null &&
                                l2_Element.Name == CConstants.xmlElementDateL2)
                            {
                                uidString = createUidString(uidString, l2_Element.ParentNode.Name, informationConstituents[1].ToString());
                            }
                            break;
                        case 3:
                            l3_Element = addChildElement(output.xmlDoc, l2_Element, informationConstituents[0]);
                            try { l3_Element = createNewAttribute(output.xmlDoc, l3_Element, CConstants.xmlAttributeValue, informationConstituents[1]); }
                            catch (IndexOutOfRangeException) { l3_Element = createNewAttribute(output.xmlDoc, l3_Element, CConstants.xmlAttributeValue, "N/A"); }
                            break;
                        default:
                            if (unknowEntryElements == null) unknowEntryElements = addChildElement(output.xmlDoc, catalogElement, "UNKNOWN_ELEMENTS");
                            unknowEntryElement = addChildElement(output.xmlDoc, unknowEntryElements, "UNKNOWN");
                            unknowEntryElement.InnerText = sourceLine.ToString();
                            break;
                    }

                    if (determineNestingLevel(informationConstituents[0]) != 0)
                    {
                        textToInsert.Inlines.Add("\r\n");
                    }
                    textToInsert.Inlines.Add(sourceLine);
                    output.flowDoc.Blocks.Add(textToInsert);
                }
            } //foreach (String sourceLine in sourceLines)

            XDocument theXDocument = new XDocument();
            theXDocument = XDocument.Load(new XmlNodeReader(output.xmlDoc));
            foreach (XElement thePersonXElement in theXDocument.Descendants(CConstants.xmlElementPerson))
            {
                uidString = generatePersonUids(theXDocument, thePersonXElement);
                XAttribute newXAttribute = new XAttribute(CConstants.xmlAttributeUID, getMD5CodeFromString(uidString));
                thePersonXElement.Add(newXAttribute);
            }

            output.xmlDoc = CHelper.convertXDocToXmlDoc(theXDocument);

            thePopup.Hide();
            thePopup.Close();
            textFromFile.Close();
            return output;
        }

        private XmlDocument addRootElement(XmlDocument inXmlDocument, String elementName)
        {
            XmlElement theElement = inXmlDocument.CreateElement(elementName);
            inXmlDocument.AppendChild(theElement);
            return inXmlDocument;
        }

        private XmlElement addChildElement(XmlDocument inXmlDocument, XmlElement inXmlParentNode, String elementName)
        {
            XmlElement theElement = inXmlDocument.CreateElement(elementName);
            inXmlParentNode.AppendChild(theElement);
            return theElement;
        }

        private String[] splitLineIntoConstituents(String inSourceLine)
        {
            String[] lineConstituents = new String[3];

            String[] splitString = inSourceLine.Split(new char[] { ' ' });
            String helperString = splitString[0] + "_" + splitString[1];
            String theSubstring = "";
            int helperLength = helperString.Length;

            //**************************************** HEADER Elements ********************************************************************
            if (inSourceLine.Contains("@ INDI") ||
                inSourceLine.Contains("@ FAM") ||
                inSourceLine.Contains("@ REPO") ||
                inSourceLine.Contains("@ SOUR"))
            {
                String[] constituents = inSourceLine.Split(new char[] { ' ' }, 3);
                lineConstituents = createConstituents(constituents[0] + " " + constituents[2], constituents[1], "");
            }
            else if (inSourceLine.Contains("1 UID"))
            {
                //Some providers (e.g. Ancestry) use "UID" instead of "_UID" => normalize
                String[] constituents = inSourceLine.Split(new char[] { ' ' }, 3);
                lineConstituents = createConstituents(constituents[0] + " _" + constituents[1], constituents[2], "");
            }
            else
            {
                switch (helperLength)
                {
                    case 5:
                        // Short entries
                        theSubstring = inSourceLine.Substring(0, 5);
                        try { lineConstituents = createConstituents(theSubstring, inSourceLine.Remove(0, 6), ""); }
                        catch (ArgumentOutOfRangeException) { lineConstituents = createConstituents(theSubstring, "", ""); }
                        break;
                    case 6:
                        theSubstring = inSourceLine.Substring(0, 6);
                        try { lineConstituents = createConstituents(theSubstring, inSourceLine.Remove(0, 7), ""); }
                        catch (ArgumentOutOfRangeException) { lineConstituents = createConstituents(theSubstring, "", ""); }
                        break;
                    default:
                        //Wild try for those entries which do not match
                        String[] theConstituents = inSourceLine.Split(new char[] { ' ' }, 3);
                        lineConstituents = createConstituents(theConstituents[0] + " " + theConstituents[1], theConstituents[2], "");
                        break;
                }
            }

            return lineConstituents;
        }

        private String[] createConstituents(string inConstituent0Text, string inConstituent1Text, string inConstituent2Text)
        {
            String[] lineConstituents = new String[3];
            String[] splitArray = new String[2];

            splitArray = inConstituent0Text.Split(new Char[] {' '});
            if (inConstituent0Text != "") lineConstituents[0] = splitArray[1] + "_L" + splitArray[0];
            else lineConstituents[0] = null;
            if (inConstituent1Text != "") lineConstituents[1] = inConstituent1Text;
            else lineConstituents[1] = null;
            if (inConstituent2Text != "") lineConstituents[2] = inConstituent2Text;
            else lineConstituents[2] = null;
            return lineConstituents;
        }

        private XmlElement createNewAttribute(XmlDocument theXmlDoc, XmlElement ParentElement, string AttributeName, string AttributeValue)
        {
            XmlAttribute newAttribute = theXmlDoc.CreateAttribute(AttributeName);
            newAttribute.Value = AttributeValue;
            if (ParentElement.Attributes != null)
            {
                ParentElement.Attributes.Append(newAttribute);
            }
            else
            {
                newAttribute.Value = AttributeValue;
            }
            return ParentElement;
        }

        private int determineNestingLevel(string inputString)
        {
            int nestingLevel = 0;

            if (inputString.Contains("_L0")) nestingLevel = 0;
            else if (inputString.Contains("_L1")) nestingLevel = 1;
            else if (inputString.Contains("_L2")) nestingLevel = 2;
            else if (inputString.Contains("_L3")) nestingLevel = 3;

            return nestingLevel;
        }

        private XmlElement generatePersonUID(string uidString, XmlElement theXmlElement, XmlDocument theXmlDoc)
        {
            if (uidString != "" && theXmlElement != null)
            {

                theXmlElement = createNewAttribute(theXmlDoc, theXmlElement, "UID", getMD5CodeFromString(uidString));
            }

            return theXmlElement;
        }

        private string getMD5CodeFromString(string theString)
        {
            Byte[] uidBytes = new Byte[1];
            StringBuilder theStringBuilder = new StringBuilder();

            if (theString != "")
            {
                theString = theString.ToUpper();
                switch (encoding)
                {
                    case "UTF-8":
                        UTF8Encoding utf8Encoding = new UTF8Encoding();
                        uidBytes = utf8Encoding.GetBytes(theString);
                        break;
                    case "ASCII":
                        ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                        uidBytes = asciiEncoding.GetBytes(theString);
                        break;
                }

                MD5 md5 = MD5.Create();
                Byte[] hashArray = md5.ComputeHash(uidBytes);
                theString = CConstants.uidTemplate;
                foreach (Byte hashByte in hashArray)
                {
                    theStringBuilder.Append(hashByte.ToString("x2"));
                }
            }

            return theStringBuilder.ToString();
        }

        private string createUidString(string uidString, string xElementName, string textToAdd)
        {
            int indexOfString = 0;
            switch (xElementName)
            {
                case CConstants.xmlElementName:
                    uidString = uidString.Insert(3, textToAdd);
                    uidString.Replace(' ', '_');
                    break;
                case CConstants.xmlElementSex:
                    indexOfString = uidString.IndexOf("_S:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(' ', '_');
                    break;
                case CConstants.xmlElementBirth:
                    indexOfString = uidString.IndexOf("_B:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(' ', '_');
                    break;
                case CConstants.xmlElementDeath:
                    indexOfString = uidString.IndexOf("_D:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(" ", "");
                    break;
                case CConstants.xmlElementHusband:
                    indexOfString = uidString.IndexOf("_H:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(" ", "");
                    break;
                case CConstants.xmlElementWife:
                    indexOfString = uidString.IndexOf("_W:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(" ", "");
                    break;
                case CConstants.xmlElement_UID:
                    indexOfString = uidString.IndexOf("_U:");
                    uidString = uidString.Insert(indexOfString + 3, textToAdd);
                    uidString.Replace(" ", "");
                    break;
            }

            return uidString;
        }

        private string buildUidString(string[] informationConstituents, string uidString, XmlElement theXmlElement, CConstants.fileHandlerReturnStruct outputStructure)
        {
            //Information to be included in UID
            if (informationConstituents[1] != null &&
                (informationConstituents[0] == CConstants.xmlElementName ||
                 informationConstituents[0] == CConstants.xmlElementSex ||
                 informationConstituents[0] == CConstants.xmlElement_UID))
            {
                uidString = createUidString(uidString, theXmlElement.Name, informationConstituents[1].ToString());
            }

            return uidString;
        }

        private string buildFamilyUidString(string[] informationConstituents, string familyUidString, XmlElement theXmlElement, XmlDocument theXmlDocument)
        {
            XDocument bufferXDocument;
            string nameOfPerson = "";

            if (informationConstituents[1] != null &&
               (informationConstituents[0] == CConstants.xmlElementHusband ||
                informationConstituents[0] == CConstants.xmlElementWife))
            {
                //create a buffer linq theXDocument for more handy data handling
                bufferXDocument = XDocument.Load(new XmlNodeReader(theXmlDocument));
                // determine person names of family record
                IEnumerable<XElement> personRecordsToFamilyRecord =
                        bufferXDocument.Descendants(CConstants.xmlElementPerson)
                        .Where(test => test
                            .Attribute(CConstants.xmlAttributeOrigID)
                            .Value == informationConstituents[1]);

                if (personRecordsToFamilyRecord.Count() == 1)
                {
                    XElement personRecord = personRecordsToFamilyRecord.ElementAt(0);
                    if (personRecord
                        .Element(CConstants.xmlElementName) != null)
                    {
                        nameOfPerson = personRecord
                            .Element(CConstants.xmlElementName)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value;
                    }
                    else
                    {
                        nameOfPerson = "UNKNOWN";
                    }
                }


                familyUidString = createUidString(familyUidString, theXmlElement.Name, nameOfPerson);
            }
            return familyUidString;
        }

        private void hardBreakpoint()
        {
            if(System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        public void saveDataToFile(FlowDocument theFlowDocument, XmlDocument theXmlDocument)
        {
            Stream theDataStream; ;

            SaveFileDialog theSaveFileDialog = new SaveFileDialog();
            theSaveFileDialog.InitialDirectory = "C:\\";
            theSaveFileDialog.Filter = "GEDCOM File (*.ged)|*.ged|XML File (*.xml)|*.xml";
            theSaveFileDialog.FilterIndex = 1;

            if (theSaveFileDialog.ShowDialog() == true)
            {
                switch (theSaveFileDialog.FilterIndex)
                {
                    case 1:
                        theDataStream = new MemoryStream();
                        if ((theDataStream = theSaveFileDialog.OpenFile()) != null)
                        {
                            var dataForSave = new TextRange(theFlowDocument.ContentStart, theFlowDocument.ContentEnd);
                            StreamWriter theDataStreamWriter = new StreamWriter(theDataStream, Encoding.UTF8);
                            theDataStreamWriter.Write(dataForSave.Text.ToString());
                            theDataStreamWriter.Close();
                        }
                        break;
                    case 2:
                        theDataStream = new MemoryStream();
                        if ((theDataStream = theSaveFileDialog.OpenFile()) != null)
                        {
                            theXmlDocument.Save(theDataStream);
                        }
                        break;
                    case 3:
                        MessageBox.Show("Not yet implmented");
                        break;
                }
            }
        }

        private string generatePersonUids(XDocument theXDocument, XElement thePersonXElement)
        {
            string uidString; 
            IEnumerable<XElement> familyParentElements;
            IEnumerable<XElement> familySpouseElements;
            IEnumerable<XElement> spouseElements;
            IEnumerable<XElement> fatherElements;
            IEnumerable<XElement> motherElements;

            if (thePersonXElement.Value == CConstants.debugIndicator)
            {
                hardBreakpoint();
            }

            uidString = "_N:_B:_D:_SN:_SB:_SD:_FN:_FB:_FD:_MN:_MB:_MD:";
            //Add data of person him/herself
            try { uidString = addToUidString(uidString, thePersonXElement.Element(CConstants.xmlElementName), CConstants.markerSelf); }
            catch { }
            try { uidString = addToUidString(uidString, thePersonXElement.Element(CConstants.xmlElementBirth).Element(CConstants.xmlElementDateL2), CConstants.markerSelf); }
            catch { }
            try { uidString = addToUidString(uidString, thePersonXElement.Element(CConstants.xmlElementDeath).Element(CConstants.xmlElementDateL2), CConstants.markerSelf); }
            catch { }

            //Add spouse data
            if (thePersonXElement.Element(CConstants.xmlElementFamilySpouse) != null)
            {
                try
                {
                    familySpouseElements =
                        from XElement theSpouseFamilyElement in theXDocument.Descendants(CConstants.xmlElementFamily)
                        where
                        theSpouseFamilyElement
                        .Attribute(CConstants.xmlAttributeOrigID)
                        .Value ==
                        thePersonXElement
                        .Element(CConstants.xmlElementFamilySpouse)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value
                        select theSpouseFamilyElement;
                }
                catch { familySpouseElements = null; }

                if (familySpouseElements != null && familySpouseElements.Count() == 1)
                {
                    XElement familySpouseElement = familySpouseElements.ElementAt(0);
                    if (thePersonXElement.Element(CConstants.xmlElementSex).Value == "M")
                    {
                    
                        if (familySpouseElement.Element(CConstants.xmlElementHusband) != null)
                        {
                            try
                            {
                            spouseElements =
                                from XElement personElement in theXDocument.Descendants(CConstants.xmlElementPerson)
                                where
                                personElement
                                .Attribute(CConstants.xmlAttributeOrigID)
                                .Value ==
                                familySpouseElement
                                .Element(CConstants.xmlElementWife)
                                .Attribute(CConstants.xmlAttributeValue)
                                .Value
                                select personElement;
                            }
                            catch { spouseElements = null; }
                        }
                        else { spouseElements = null; }
                    }
                    else if (thePersonXElement.Element(CConstants.xmlElementSex).Value == "F")
                    {
                        if (familySpouseElement.Element(CConstants.xmlElementHusband) != null)
                        {
                            try
                            {
                                spouseElements =
                                    from XElement personElement in theXDocument.Descendants(CConstants.xmlElementPerson)
                                    where
                                    personElement
                                    .Attribute(CConstants.xmlAttributeOrigID)
                                    .Value ==
                                    familySpouseElement
                                    .Element(CConstants.xmlElementHusband)
                                    .Attribute(CConstants.xmlAttributeValue)
                                    .Value
                                    select personElement;
                            }
                            catch { spouseElements = null; }
                        }
                        else { spouseElements = null; }
                    }
                    else
                    {
                        spouseElements = null;
                    }

                    if (spouseElements != null && spouseElements.Count() == 1)
                    {
                        XElement spouseElement = spouseElements.ElementAt(0);
                        try { uidString = addToUidString(uidString, spouseElement.Element(CConstants.xmlElementName), CConstants.markerSpouse); }
                        catch { }
                        try { uidString = addToUidString(uidString, spouseElement.Element(CConstants.xmlElementBirth).Element(CConstants.xmlElementDateL2), CConstants.markerSpouse); }
                        catch { }
                        try { uidString = addToUidString(uidString, spouseElement.Element(CConstants.xmlElementDeath).Element(CConstants.xmlElementDateL2), CConstants.markerSpouse); }
                        catch { }
                    }
                }
            } //if (thePersonXElement.Element(CConstants.xmlElementFamilySpouse) != null)

            // Add Father Data
            if (thePersonXElement.Element(CConstants.xmlElementFamilyChild) != null)
            {
                try
                {
                    familyParentElements =
                        from XElement theChildFamilyElement in theXDocument.Descendants(CConstants.xmlElementFamily)
                        where
                        theChildFamilyElement
                        .Attribute(CConstants.xmlAttributeOrigID)
                        .Value ==
                        thePersonXElement
                        .Element(CConstants.xmlElementFamilyChild)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value
                        select theChildFamilyElement;
                }
                catch { familyParentElements = null; }

                if (familyParentElements != null && familyParentElements.Count() == 1)
                {
                    XElement familyChildElement = familyParentElements.ElementAt(0);

                    if (familyChildElement.Element(CConstants.xmlElementHusband) != null)
                    {
                        try
                        {
                            fatherElements =
                            from XElement personElement in theXDocument.Descendants(CConstants.xmlElementPerson)
                            where
                            personElement
                            .Attribute(CConstants.xmlAttributeOrigID)
                            .Value ==
                            familyChildElement
                            .Element(CConstants.xmlElementHusband)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value
                            select personElement;
                        }
                        catch { fatherElements = null; }
                    }
                    else { fatherElements = null; }

                    if (fatherElements != null && fatherElements.Count() == 1)
                    {
                        XElement fatherElement = fatherElements.ElementAt(0);
                        try { uidString = addToUidString(uidString, fatherElement.Element(CConstants.xmlElementName), CConstants.markerFather); }
                        catch { }
                        try { uidString = addToUidString(uidString, fatherElement.Element(CConstants.xmlElementBirth).Element(CConstants.xmlElementDateL2), CConstants.markerFather); }
                        catch { }
                        try { uidString = addToUidString(uidString, fatherElement.Element(CConstants.xmlElementDeath).Element(CConstants.xmlElementDateL2), CConstants.markerFather); }
                        catch { }
                    }
                }
            }

            // Add Father Data
            if (thePersonXElement.Element(CConstants.xmlElementFamilyChild) != null)
            {
                try
                {
                    familyParentElements =
                        from XElement theChildFamilyElement in theXDocument.Descendants(CConstants.xmlElementFamily)
                        where
                        theChildFamilyElement
                        .Attribute(CConstants.xmlAttributeOrigID)
                        .Value ==
                        thePersonXElement
                        .Element(CConstants.xmlElementFamilyChild)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value
                        select theChildFamilyElement;
                }
                catch { familyParentElements = null; }

                if (familyParentElements != null && familyParentElements.Count() == 1)
                {
                    XElement familyChildElement = familyParentElements.ElementAt(0);

                    if (familyChildElement.Element(CConstants.xmlElementWife) != null)
                    {
                        try
                        {
                            motherElements =
                                    from XElement personElement in theXDocument.Descendants(CConstants.xmlElementPerson)
                                    where
                                    personElement
                                    .Attribute(CConstants.xmlAttributeOrigID)
                                    .Value ==
                                    familyChildElement
                                    .Element(CConstants.xmlElementWife)
                                    .Attribute(CConstants.xmlAttributeValue)
                                    .Value
                                    select personElement;
                        }
                        catch { motherElements = null; }
                    }
                    else { motherElements = null; }

                    if (motherElements != null && motherElements.Count() == 1)
                    {
                        XElement motherElement = motherElements.ElementAt(0);
                        try { uidString = addToUidString(uidString, motherElement.Element(CConstants.xmlElementName), CConstants.markerFather); }
                        catch { }
                        try { uidString = addToUidString(uidString, motherElement.Element(CConstants.xmlElementBirth).Element(CConstants.xmlElementDateL2), CConstants.markerFather); }
                        catch { }
                        try { uidString = addToUidString(uidString, motherElement.Element(CConstants.xmlElementDeath).Element(CConstants.xmlElementDateL2), CConstants.markerFather); }
                        catch { }
                    }
                }
            }

            uidString = uidString.Replace(" ", "");
            uidString = uidString.Replace("/", "");
            return uidString;
        }

        private string addToUidString(string inputString, XElement theXElement, string theMarker)
        {
            string valueString = "";

            try
            {
                valueString = theXElement.Attribute(CConstants.xmlAttributeValue).Value;
                valueString = replaceSpecialCharacters(valueString);
                //Special case for dates: transform to English spelling
                if (theXElement.Name == CConstants.xmlElementDateL2)
                {
                    valueString = parseDateToEnglish(valueString);
                }
            }
            catch { valueString = "UNKNOWN"; }

            string elementName = "";
            if (theXElement.Name.ToString() == CConstants.xmlElementDateL2)
            {
                elementName = theXElement.Parent.Name.ToString();
            }
            else elementName = theXElement.Name.ToString();

            return createUidStringNew(inputString, elementName, valueString, theMarker);
        }

        private string createUidStringNew(string uidString, string xElementName, string textToAdd, string theMarker)
        {
            int indexOfString = 0;

            switch (theMarker)
            {
                case CConstants.markerSelf:
                    switch (xElementName)
                    {
                        case CConstants.xmlElementName:
                            uidString = uidString.Replace("_N:", textToAdd);
                            break;
                        case CConstants.xmlElementBirth:
                            uidString = uidString.Replace("_B:", textToAdd);
                            break;
                        case CConstants.xmlElementDeath:
                            uidString = uidString.Replace("_D:", textToAdd);
                            break;
                    }
                    break;
                case CConstants.markerSpouse:
                    switch (xElementName)
                    {
                        case CConstants.xmlElementName:
                            uidString = uidString.Replace("_SN:", textToAdd);
                            break;
                        case CConstants.xmlElementBirth:
                            uidString = uidString.Replace("_SB:", textToAdd);
                            break;
                        case CConstants.xmlElementDeath:
                            uidString = uidString.Replace("_SD:", textToAdd);
                            break;
                    }
                    break;
                case CConstants.markerMother:
                    switch (xElementName)
                    {
                        case CConstants.xmlElementName:
                            uidString = uidString.Replace("_MN:", textToAdd);
                            break;
                        case CConstants.xmlElementBirth:
                            uidString = uidString.Replace("_MB:", textToAdd);
                            break;
                        case CConstants.xmlElementDeath:
                            uidString = uidString.Replace("_MD:", textToAdd);
                            break;
                    }
                    break;
                case CConstants.markerFather:
                    switch (xElementName)
                    {
                        case CConstants.xmlElementName:
                            uidString = uidString.Replace("_FN:", textToAdd);
                            break;
                        case CConstants.xmlElementBirth:
                            uidString = uidString.Replace("_FB:", textToAdd);
                            break;
                        case CConstants.xmlElementDeath:
                            uidString = uidString.Replace("_FD:", textToAdd);
                            break;
                    }
                    break;
            }

            return uidString;
        }

        private string replaceSpecialCharacters(string inputString)
        {
            string bufferString;

            bufferString = inputString.Replace("ä", "ae");
            bufferString = bufferString.Replace("ö", "oe");
            bufferString = bufferString.Replace("ü", "ue");
            bufferString = bufferString.Replace("ß", "ss");

            return bufferString;
        }

        private string parseDateToEnglish(string inputString)
        {
            string dateString = inputString;

            if (inputString.Contains("Mär")) dateString = inputString.Replace("Mär","MAR");
            else if (inputString.Contains("MÄR")) dateString = inputString.Replace("MÄR", "MAR");
            else if (inputString.Contains("Mai")) dateString = inputString.Replace("Mai", "MAY");
            else if (inputString.Contains("MAI")) dateString = inputString.Replace("MAI", "MAY");
            else if (inputString.Contains("Okt")) dateString = inputString.Replace("Okt", "OCT");
            else if (inputString.Contains("OKT")) dateString = inputString.Replace("OKT", "OCT");
            else if (inputString.Contains("Dez")) dateString = inputString.Replace("Dez", "DEC");
            else if (inputString.Contains("DEZ")) dateString = inputString.Replace("DEZ", "DEC");

            return dateString;
        }
    }
}

