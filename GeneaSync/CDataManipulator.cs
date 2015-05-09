using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;


namespace GedcomViewCompare
{
    public class CDataManipulator : IDataManipulator
    {
        #region Attribute Declaration
        private XmlDocument _theXmlDocument1;
        public XmlDocument theXmlDocument1
        {
            set 
            {
                _theXmlDocument1 = value;
                theXDocument1 = XDocument.Load(new XmlNodeReader(_theXmlDocument1));

                XmlDeclaration theDeclaration =
                    _theXmlDocument1.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
                if (theDeclaration != null)
                {
                    theXDocument1.Declaration = new XDeclaration(theDeclaration.Version, theDeclaration.Encoding,
                        theDeclaration.Standalone);
                }

                XmlProcessingInstruction theXmlProcessingInstruction =
                    _theXmlDocument1.ChildNodes.OfType<XmlProcessingInstruction>().FirstOrDefault();
                if (theXmlProcessingInstruction != null)
                {
                    theXDocument1.AddFirst(new XProcessingInstruction(theXmlProcessingInstruction.Target, theXmlProcessingInstruction.Data));
                }

                create_UidWhereNeeded(theXDocument1);
            }
            get { return _theXmlDocument1; }
        }

        private XmlDocument _theXmlDocument2;
        public XmlDocument theXmlDocument2
        {
            set 
            {
                _theXmlDocument2 = value;
                theXDocument2 = XDocument.Load(new XmlNodeReader(_theXmlDocument2));
                
                XmlDeclaration theDeclaration =
                    _theXmlDocument2.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
                if (theDeclaration != null)
                {
                    theXDocument2.Declaration = new XDeclaration(theDeclaration.Version, theDeclaration.Encoding,
                        theDeclaration.Standalone);
                }

                XmlProcessingInstruction theXmlProcessingInstruction =
                    _theXmlDocument2.ChildNodes.OfType<XmlProcessingInstruction>().FirstOrDefault();
                if (theXmlProcessingInstruction != null)
                {
                    theXDocument2.AddFirst(new XProcessingInstruction(theXmlProcessingInstruction.Target, theXmlProcessingInstruction.Data));
                }
                create_UidWhereNeeded(theXDocument2);
            }
            get { return _theXmlDocument2; }
        }

        private XDocument _theXDocument1;
        public XDocument theXDocument1
        {
            set { _theXDocument1 = value; }
            get { return _theXDocument1; }
        }

        private XDocument _theXDocument2;
        public XDocument theXDocument2
        {
            set { _theXDocument2 = value; }
            get { return _theXDocument2; }
        }

        delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        #endregion

        #region Public Methods
        public CDataManipulator()
        {
        }

        public XmlDocument compareDocumentContent(XDocument masterXDocument, XDocument slaveXDocument, RichTextBox masterDocumentTextBox)
        {
            //***************************************************************************************
            // Algorythm to find person elements which are the same in both files
            //***************************************************************************************
            // Loop over persons XML masterXDocument and flag person entries
            foreach (XElement personInMaster in masterXDocument.Descendants(CConstants.xmlElementPerson))
            {
                if (personInMaster.Attribute(CConstants.xmlAttributeOrigID).Value == CConstants.debugIndicator)
                {
                    hardBreakpoint();
                }
                // Check 1: Search for record with the same person hash code (Name and dates match)
                //getMatchesByOrigUID(personInMaster, slaveXDocument);

                // Check 2: For "missing" records check if they are maybe available by hash code
                //if (personInMaster.Attribute(CConstants.xmlAttributeChecked).Value == CConstants.xmlAttributeCheckedValueMiss)
                //{
                    getMatchesByHashCode(personInMaster, slaveXDocument);
                //}

                // Check 3: For "missing" records check if they are maybe available with other dates
                if (personInMaster.Attribute(CConstants.xmlAttributeChecked).Value == CConstants.xmlAttributeCheckedValueMiss)
                {
                    getMatchesByName(personInMaster, slaveXDocument);
                }

                // Color-Code text in UI by Checked Attribute
                colorCodeText(personInMaster, masterDocumentTextBox);
            }//foreach (XElement personInFile1 in theXDocument1.Descendants(CConstants.xmlElementPerson))

            //********************************************************************************************
            /* Debugging coding DO NOT REMOVE!!!
            if (bufferXDoc1 == null)
            {
                bufferXDoc1 = new XDocument(masterXDocument);
            }
            else if (bufferXDoc2 == null)
            {
                bufferXDoc2 = new XDocument(masterXDocument);
                if (bufferXDoc1 != null && bufferXDoc2 != null)
                {
                    XDocument bufferXDoc3 = new XDocument();
                    bufferXDoc3.Add(new XElement(CConstants.xmlElementCatalog));

                    IEnumerable<XElement> buffer1array = bufferXDoc2.Descendants(CConstants.xmlElementPerson)
                        .Where(test => test.Attribute(CConstants.xmlAttributeChecked)
                            .Value == CConstants.xmlAttributeCheckedValueMatch);

                    int testX = buffer1array.Count();

                    foreach (XElement element in buffer1array)
                    {
                        bufferXDoc3.Element(CConstants.xmlElementCatalog).Add(element);
                    }

                    buffer1array = bufferXDoc1.Descendants(CConstants.xmlElementPerson)
                        .Where(test => test.Attribute(CConstants.xmlAttributeChecked)
                            .Value == CConstants.xmlAttributeCheckedValueMatch);

                    int testY = buffer1array.Count();

                    foreach (XElement element2 in buffer1array)
                    {
                            IEnumerable<XElement> myTest =
                                from XElement testElement in bufferXDoc3
                                .Descendants(CConstants.xmlElementPerson)
                            where
                            testElement
                            .Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value == 
                            element2
                            .Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value
                             select testElement;

                            if (myTest.Count() > 1)
                            {
                                MessageBox.Show("Person: " +
                                    element2
                                    .Element(CConstants.xmlElementName)
                                    .Attribute(CConstants.xmlAttributeValue)
                                    .Value +
                                    " seems to exist with more than one entry in source file.\n" +
                                    "Please check entries in GEDCOM files.");
                            }

                            myTest =
                            from XElement testElement in bufferXDoc3
                                 .Descendants(CConstants.xmlElementPerson)
                             where
                             testElement
                             .Attribute(CConstants.xmlAttributeUID)
                             .Value ==
                             element2
                             .Attribute(CConstants.xmlAttributeUID)
                             .Value
                             select testElement;

                            if (myTest.Count() > 1)
                            {
                             MessageBox.Show(element2
                             .Attribute(CConstants.xmlAttributeOrigID)
                             .Value);
                            }
                    }
                }
            }
            */
            //********************************************************************************

            XmlDocument outputXmlDoc = new XmlDocument();
            outputXmlDoc = CHelper.convertXDocToXmlDoc(masterXDocument);
            return outputXmlDoc;
        }

        public CConstants.dataManipulatorStructure createIntersectionData()
        {
            CConstants.dataManipulatorStructure unionData = new CConstants.dataManipulatorStructure();
            unionData.flowDoc = new FlowDocument();
            unionData.XDoc = CHelper.initalizeXDocument();

            ProgressPopup thePopup = new ProgressPopup();
            thePopup.Title = "Creating Union Data..";
            thePopup.theProgressBar.Maximum = 100;
            thePopup.theProgressBar.Minimum = 0;
            thePopup.Show();

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(thePopup.theProgressBar.SetValue);
            UpdateProgressBarDelegate updateTextDelegate = new UpdateProgressBarDelegate(thePopup.progressText.SetValue);

            
            unionData = this.createNewDocumentsByCheckedValue(CConstants.xmlAttributeCheckedValueMatch, unionData);
            updateProgressBar(1, 4, 1, updatePbDelegate, updateTextDelegate);
            unionData = this.createNewDocumentsByCheckedValue(CConstants.xmlAttributeCheckedValueMiss, unionData);
            updateProgressBar(2, 4, 1, updatePbDelegate, updateTextDelegate);
            unionData = this.createNewDocumentsByCheckedValue(CConstants.xmlAttributeCheckedValueNaMa, unionData);
            updateProgressBar(3, 4, 1, updatePbDelegate, updateTextDelegate);
            unionData = this.createNewDocumentsByCheckedValue(CConstants.xmlAttributeCheckedValueAmbi, unionData);
            updateProgressBar(4, 4, 1, updatePbDelegate, updateTextDelegate);
            thePopup.Close();

            return unionData;
        }

        public CConstants.dataManipulatorStructure createDifferenceData()
        {
            CConstants.dataManipulatorStructure differenceData = new CConstants.dataManipulatorStructure();
            differenceData.flowDoc = new FlowDocument();
            differenceData.XDoc = CHelper.initalizeXDocument();
            return this.createNewDocumentsByCheckedValue(CConstants.xmlAttributeCheckedValueMiss, differenceData);
        }

        public string getPersonCountByCheckAttr(XDocument theXDocument, string checkAttrValue)
        {
            IEnumerable<XElement> personsByAttrib = getPersonsByAttribute(theXDocument, checkAttrValue);
            return personsByAttrib.Count().ToString();
        }

        public void sortDocumentBySurnames(RichTextBox theRichTextBox, XDocument theXDocument)
        {
            IEnumerable<XElement> persons;
            IEnumerable<XElement> families;
            FlowDocument sortedDocument = new FlowDocument();
            Block[] personArray = new Block[0];
            Block[] familyArray = new Block[0];
            int arrayIndex = 0;
            int stepNumber = 1;
            int numberOfSteps = 1;

            numberOfSteps = theXDocument.Descendants("CATALOG").Elements().Count();

            ProgressPopup thePopup = new ProgressPopup();
            thePopup.Title = "Sorting Progress...";
            thePopup.theProgressBar.Maximum = 100;
            thePopup.theProgressBar.Minimum = 0;
            thePopup.Show();

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(thePopup.theProgressBar.SetValue);
            UpdateProgressBarDelegate updateTextDelegate = new UpdateProgressBarDelegate(thePopup.progressText.SetValue);

            //********************************************************************************
            // Add header data to theXDocument
            //********************************************************************************
            IEnumerable<Block> headerBlock = theRichTextBox.Document.Blocks.Where(test => test.Name == "header");
            sortedDocument.Blocks.Add(headerBlock.First());

            //********************************************************************************
            // 13.07.2012: check if every individual has a last name and fill if needed
            // Start of insertion
            //********************************************************************************

            Int32 unknownCounter = 0;

            foreach (XElement personElement in theXDocument.Descendants(CConstants.xmlElementPerson))
            {
                if (personElement.Element(CConstants.xmlElementName) == null)
                {
                    unknownCounter++;
                    XElement newNameElement = new XElement(CConstants.xmlElementName);
                    XAttribute newNameValue = new XAttribute(CConstants.xmlAttributeValue, "UNKNOWN" + unknownCounter.ToString());
                    newNameElement.Add(newNameValue);
                    personElement.Add(newNameElement);
                }
            }

            //********************************************************************************
            // 13.07.2012: End of insertion
            //********************************************************************************
            //********************************************************************************
            // create a sorted XML list, sort by /Lastname/ in element INDI
            //********************************************************************************
            persons =
                theXDocument
                .Descendants(CConstants.xmlElementPerson)
                .OrderBy(personElement =>
                    personElement
                    .Elements(CConstants.xmlElementName)
                    .Attributes(CConstants.xmlAttributeValue)
                    .ElementAt(0)
                    .Value
                    .Substring(personElement
                                .Elements(CConstants.xmlElementName)
                                .Attributes(CConstants.xmlAttributeValue)
                                .ElementAt(0)
                                .Value
                                .IndexOf("/") + 1));
            //********************************************************************************
            // Sort text in UI
            //********************************************************************************
            foreach (XElement personElement in persons)
            {
                //Progress Bar update
                updateProgressBar(stepNumber++, numberOfSteps, 100, updatePbDelegate, updateTextDelegate);
                //Actual sorting
                Array.Resize(ref personArray, personArray.Length + 1);
                IEnumerable<Block> personBlock = getDocumentBlocksByINDI_ID(personElement, theRichTextBox.Document);
                if (personBlock.Count() == 1)
                {
                    sortedDocument.Blocks.Add(personBlock.ElementAt(0));
                }
                else
                {
                    RoutedEventArgs theEventArgs = new RoutedEventArgs();
                    //RaiseEvent(theEventArgs);
                }
                arrayIndex++;
            }

            arrayIndex = 0;
            //********************************************************************************
            // Append family and other data unsorted
            //********************************************************************************
            families =
                theXDocument
                .Descendants(CConstants.xmlElementFamily);

            foreach (XElement familyElement in families)
            {
                //Progress Bar update
                updateProgressBar(stepNumber++, numberOfSteps, 100, updatePbDelegate, updateTextDelegate);
                //Actual sorting
                Array.Resize(ref familyArray, familyArray.Length + 1);
                IEnumerable<Block> famliyBlock = getDocumentBlocksByINDI_ID(familyElement, theRichTextBox.Document);
                if (famliyBlock.Count() == 1)
                {
                    sortedDocument.Blocks.Add(famliyBlock.ElementAt(0));
                }
                else
                {
                    RoutedEventArgs theEventArgs = new RoutedEventArgs();
                    //RaiseEvent(theEventArgs);
                }
            }

            thePopup.Close();
            theRichTextBox.Document = sortedDocument;
        }

        public CConstants.dataManipulatorStructure createUnionData()
        {
            CConstants.dataManipulatorStructure theDataStructure = new CConstants.dataManipulatorStructure();
            theDataStructure.flowDoc = new FlowDocument();
            theDataStructure.XDoc = CHelper.initalizeXDocument();

            // Pupulate XDocument
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument1, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueMatch);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument2, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueMatch);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument1, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueNaMa);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument2, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueNaMa);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument1, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueAmbi);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument2, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueAmbi);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument1, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueMiss);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument2, theDataStructure.XDoc, CConstants.xmlAttributeCheckedValueMiss);

            //Add a Flow Document
            theDataStructure.flowDoc = CHelper.parseXmlToGEDCOM(theDataStructure.XDoc);
            return theDataStructure;
        }
        #endregion

        #region Private Methods

        private CConstants.dataManipulatorStructure createNewDocumentsByCheckedValue(string attribCheckedValue, CConstants.dataManipulatorStructure inputData)
        {
            CConstants.dataManipulatorStructure theDataStructure = new CConstants.dataManipulatorStructure();
            theDataStructure = inputData;

            //************************************************************************
            //Fill new theXDocument with persons from both files
            //************************************************************************
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument1, theDataStructure.XDoc, attribCheckedValue);
            fillXDocumentWithPersonsByCheckedAttribute(theXDocument2, theDataStructure.XDoc, attribCheckedValue);
            theDataStructure.XDoc = removeDuplicatePersonsFromXDocument(theDataStructure.XDoc);

            theDataStructure.flowDoc = CHelper.parseXmlToGEDCOM(theDataStructure.XDoc);
            return theDataStructure;
        }
        private void colorCodeText(XElement personElement, RichTextBox theTextBox)
        {
            IEnumerable<Block> paragraphArray = getDocumentBlocksByINDI_ID(personElement, theTextBox.Document);

            switch (personElement.Attribute(CConstants.xmlAttributeChecked).Value)
            {
                case CConstants.xmlAttributeCheckedValueMiss:
                    foreach (Block theBlock in paragraphArray)
                    {
                        Paragraph theParagraph = theBlock as Paragraph;
                        theParagraph.Background = Brushes.LightCoral;
                    }
                    break;
                case CConstants.xmlAttributeCheckedValueNaMa:
                    foreach (Block theBlock in paragraphArray)
                    {
                        Paragraph theParagraph = theBlock as Paragraph;
                        theParagraph.Background = Brushes.Yellow;
                    }
                    break;
                case CConstants.xmlAttributeCheckedValueAmbi:
                    foreach (Block theBlock in paragraphArray)
                    {
                        Paragraph theParagraph = theBlock as Paragraph;
                        theParagraph.Background = Brushes.SkyBlue;
                    }
                    break;
            }
        }

        private void displayResults()
        {
            comparisonResults theCompResolutWindow = new comparisonResults();

            theCompResolutWindow.textBlock_Ambi1.Text = getPersonCountByCheckAttr(theXDocument1, CConstants.xmlAttributeCheckedValueAmbi);
            theCompResolutWindow.textBlock_Ambi2.Text = getPersonCountByCheckAttr(theXDocument2, CConstants.xmlAttributeCheckedValueAmbi);
            theCompResolutWindow.textBlock_Matc1.Text = getPersonCountByCheckAttr(theXDocument1, CConstants.xmlAttributeCheckedValueMatch);
            theCompResolutWindow.textBlock_Matc2.Text = getPersonCountByCheckAttr(theXDocument2, CConstants.xmlAttributeCheckedValueMatch);
            theCompResolutWindow.textBlock_Miss1.Text = getPersonCountByCheckAttr(theXDocument1, CConstants.xmlAttributeCheckedValueMiss);
            theCompResolutWindow.textBlock_Miss2.Text = getPersonCountByCheckAttr(theXDocument2, CConstants.xmlAttributeCheckedValueMiss);
            theCompResolutWindow.textBlock_NaMa1.Text = getPersonCountByCheckAttr(theXDocument1, CConstants.xmlAttributeCheckedValueNaMa);
            theCompResolutWindow.textBlock_NaMa2.Text = getPersonCountByCheckAttr(theXDocument2, CConstants.xmlAttributeCheckedValueNaMa);

            theCompResolutWindow.Show();
        }

        private void fillXDocumentWithPersonsByCheckedAttribute(XDocument oldXDocument, XDocument newXDocument, string attribCheckedValue)
        {
            IEnumerable<XElement> personXElements;

            personXElements = getPersonsByAttribute(oldXDocument, attribCheckedValue);
            try
            {
                foreach (XElement personXElement in personXElements)
                {
                    string personOrigID = personXElement.Attribute(CConstants.xmlAttributeOrigID).Value;
                    int personCount = getNumberOfPersonOccurrencesInXDocument(newXDocument, personOrigID);
                    if (personCount == 0)
                    {
                        newXDocument.Element(CConstants.xmlElementCatalog).Add(personXElement);
                    }
                    fillXDocumentWithPersonsByFamilyQualifier(personXElement, oldXDocument, newXDocument, CConstants.xmlElementFamilySpouse);
                    fillXDocumentWithPersonsByFamilyQualifier(personXElement, oldXDocument, newXDocument, CConstants.xmlElementFamilyChild);

                }
            }
            catch (NullReferenceException) { }
        }

        private void fillXDocumentWithPersonsByFamilyQualifier(XElement personXElement, XDocument oldXDocument, XDocument newXDocument, string xElementName)
        {
            IEnumerable<XElement> familyRecords;
            IEnumerable<XElement> familyXElements;

            familyRecords = getFamilyRecordsByPersonQualifier(personXElement, oldXDocument, xElementName);

            //********************************************************************
            // With the family records check if the persons in the records are 
            // already in the flowdoc (Check only for Husband & Wife records)
            //********************************************************************
            if (familyRecords != null)
            {
                foreach (XElement familyRecord in familyRecords)
                {
                    //Check if family record already exists in the new theXDocument an append if needed
                    familyXElements = null;
                    familyXElements =
                        newXDocument
                        .Descendants(CConstants.xmlElementFamily)
                        .Where(test => test
                            .Attribute(CConstants.xmlAttributeOrigID)
                            .Value ==
                            familyRecord
                            .Attribute(CConstants.xmlAttributeOrigID)
                            .Value);

                    if (familyXElements.Count() == 0)
                    {
                        newXDocument.Element(CConstants.xmlElementCatalog).Add(familyRecord);
                    }

                    //Get all family records besides the record of current person
                    IEnumerable<XElement> personsFromfamilyRecord = familyRecord
                                                                .Descendants()
                                                                .Where(test => (test.Name == CConstants.xmlElementChild ||
                                                                                test.Name == CConstants.xmlElementHusband ||
                                                                                test.Name == CConstants.xmlElementWife) &&
                                                                                test.Attribute(CConstants.xmlAttributeValue).Value
                                                                                != personXElement.Attribute(CConstants.xmlAttributeOrigID).Value);

                    //Now Check if the persons in family record are already in XML structure
                    //if not add them to structure
                    foreach (XElement personFromFamilyRecord in personsFromfamilyRecord)
                    {
                        //Find those persons who do not have a "missing" attribute
                        IEnumerable<XElement> personRecords =
                            from XElement personRecord in oldXDocument.Descendants(CConstants.xmlElementPerson)
                            where
                            personRecord.Attribute(CConstants.xmlAttributeOrigID).Value ==
                            personFromFamilyRecord.Attribute(CConstants.xmlAttributeValue).Value &&
                            personRecord.Attribute(CConstants.xmlAttributeChecked).Value !=
                            CConstants.xmlAttributeCheckedValueMiss
                            select personRecord;

                        if (personRecords.Count() == 1)
                        {
                            XElement currentPersonXElement = personRecords.ElementAt(0);
                            string personOrigID = currentPersonXElement.Attribute(CConstants.xmlAttributeOrigID).Value;
                            int personCount = getNumberOfPersonOccurrencesInXDocument(newXDocument, personOrigID);

                            if (personCount == 0)
                            {
                                newXDocument.Element(CConstants.xmlElementCatalog).Add(personRecords.ElementAt(0));
                            }
                        }
                    }
                }//foreach (XElement familyRecordSpouse in familyRecordsSpouse)
            }//if (familyRecords != null)
        }

        private IEnumerable<Block> getDocumentBlocksByINDI_ID(XElement personElement, FlowDocument theDocument)
        {
            IEnumerable<Block> paragraphArray;
            string oldPersonID = personElement.Attribute(CConstants.xmlAttributeOrigID).Value.ToString();
            oldPersonID = oldPersonID.Replace("@", "");
            return paragraphArray = theDocument.Blocks.Where(test => test.Name == "INDI_" + oldPersonID);
        }

        private IEnumerable<XElement> getFamilyRecordsByPersonQualifier(XElement personXElement, XDocument theXDocument, string xmlElementName)
        {
            IEnumerable<XElement> familyRecords;
            //********************************************************************
            // Find parents and children family records
            //********************************************************************
            if (personXElement.Element(xmlElementName) != null)
            {
                try
                {
                    familyRecords =
                        from familyElements in theXDocument.Descendants(CConstants.xmlElementFamily)
                        where
                        familyElements.Attribute(CConstants.xmlAttributeOrigID).Value ==
                        personXElement
                        .Element(xmlElementName)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value
                        select familyElements;
                }
                catch { familyRecords = null; }
            }
            else { familyRecords = null; }

            return familyRecords;
        }

        private void getMatchesByHashCode(XElement personElement, XDocument theXDocument)
        {
            IEnumerable<XElement> personElementCollection = null;
            string personUID = "";
            bool error = false;

            try { personUID = personElement.Attribute(CConstants.xmlAttributeUID).Value; }
            catch { error = true; }

            if (error == false)
            {
                // Try to find the persons in theXDocument 1 as well in theXDocument 2 (right window)
                personElementCollection =
                    from personElements in theXDocument.Descendants(CConstants.xmlElementPerson)
                    where
                    personElements.Attribute(CConstants.xmlAttributeUID).Value == personUID
                    select personElements;
            }

            setCheckAttribute(personElement, personElementCollection);
        }

        private void getMatchesByName(XElement personElement, XDocument theXDocument)
        {

            IEnumerable<XElement> personElementCollection;
            string personName;
            int counter;

            personName = personElement.Element(CConstants.xmlElementName).Attribute(CConstants.xmlAttributeValue).Value.ToString();

            personElementCollection =
                from personElements in theXDocument.Descendants(CConstants.xmlElementPerson)
                where
                personElements.Elements(CConstants.xmlElementName).Attributes(CConstants.xmlAttributeValue).Any(nameValue => nameValue.Value.ToString() == personName)
                select personElements;

            counter = personElementCollection.Count();
            if (counter == 1)
            {
                personElement.SetAttributeValue(CConstants.xmlAttributeChecked, CConstants.xmlAttributeCheckedValueNaMa);
            }
        }

        private void getMatchesByOrigUID(XElement personElement, XDocument theXDocument)
        {
            IEnumerable<XElement> personElementCollection = null;
            string personUID = "";
            bool error = false;

            try { personUID = personElement.Element(CConstants.xmlElement_UID).Attribute(CConstants.xmlAttributeValue).Value; }
            catch { error = true; }

            if (error == false)
            {
                // Try to find the persons in theXDocument 1 as well in theXDocument 2 (right window)
                personElementCollection =
                    from personElements in theXDocument.Descendants(CConstants.xmlElementPerson)
                    where
                    personElements.Elements(CConstants.xmlElement_UID).Any(test => test.Attribute(CConstants.xmlAttributeValue).Value == personUID)
                    select personElements;
            }

            setCheckAttribute(personElement, personElementCollection);

        }

        private int getNumberOfPersonOccurrencesInXDocument(XDocument theXDocument, string personOrigID)
        {
            IEnumerable<XElement> personTestRecords =
                from XElement personTestRecord in theXDocument.Descendants(CConstants.xmlElementPerson)
                where
                personTestRecord
                .Attribute(CConstants.xmlAttributeOrigID)
                .Value ==
                personOrigID
                select personTestRecord;

            return personTestRecords.Count();
        }

        private IEnumerable<XElement> getPersonsByCheckValue(XDocument inXDocument, string inAttrValue)
        {
            IEnumerable<XElement> personStructure =
                from personElements in inXDocument.Descendants(CConstants.xmlElementPerson)
                where
                personElements.Attributes(CConstants.xmlAttributeChecked).Any(test => test.Value.Equals(inAttrValue))
                select personElements;

            return personStructure;
        }

        private IEnumerable<XElement> getPersonsByAttribute(XDocument theXDocument, string checkAttrValue)
        {
            IEnumerable<XElement> personsByAttrib = null;

            if (!theXDocument.Descendants(CConstants.xmlElementPerson).Any(test => test.Attribute(CConstants.xmlAttributeChecked) == null ) )
            {
                personsByAttrib =
                    from personElements in theXDocument.Descendants(CConstants.xmlElementPerson)
                    where personElements.Attribute(CConstants.xmlAttributeChecked).Value.ToString() == checkAttrValue
                    select personElements;
            }
            else
            {
                MessageBox.Show(": Command aborted. Please execute the \"Highlight\" command first");
            }

            return personsByAttrib;
        }

        private CConstants.personInFlowDocStruct personInFamilyRecordExistsInFlowDoc(XElement familyRecord, FlowDocument theFlowDoc, string familyMemberQualifier)
        {
            CConstants.personInFlowDocStruct returnStruct = new CConstants.personInFlowDocStruct();
            returnStruct.isPersonInFlowDoc = true;
            returnStruct.personID = "";

            if (familyRecord.Element(familyMemberQualifier) != null)
            {
                returnStruct.personID = familyRecord
                                        .Element(familyMemberQualifier)
                                        .Attribute(CConstants.xmlAttributeValue)
                                        .Value
                                        .ToString()
                                        .Replace("@", "");
                try { theFlowDoc.Blocks.Select(test => test.Name == "INDI_" + returnStruct.personID); }
                catch { returnStruct.isPersonInFlowDoc = false; }
            }

            return returnStruct;
        }

        private void setCheckAttribute(XElement personElement, IEnumerable<XElement> matchElements)
        {
            int counter = matchElements.Count();

            switch (counter)
            {
                case 0:
                    personElement.SetAttributeValue(CConstants.xmlAttributeChecked, CConstants.xmlAttributeCheckedValueMiss);
                    break;
                case 1:
                    personElement.SetAttributeValue(CConstants.xmlAttributeChecked, CConstants.xmlAttributeCheckedValueMatch);
                    string matchPartner = matchElements.ElementAt(0).Attribute(CConstants.xmlAttributeOrigID).Value;
                    personElement.SetAttributeValue(CConstants.xmlAttributeMatchPartner, matchPartner);
                    break;
                default:
                    personElement.SetAttributeValue(CConstants.xmlAttributeChecked, CConstants.xmlAttributeCheckedValueAmbi);
                    break;
            }

            //return personElement;
        }

        private void updateProgressBar(int stepNumber, int numberOfSteps, int moduloNumber, UpdateProgressBarDelegate updateNumberDelegate, UpdateProgressBarDelegate updateTextDelegate)
        {
            double progressPercentage;

            progressPercentage = (double)stepNumber * 100d / (double)numberOfSteps;

            if (stepNumber % moduloNumber == 0)
            {
                string progressText = stepNumber.ToString() + " of " + numberOfSteps.ToString() + " steps processed";
                progressPercentage = (double)stepNumber * 100d / (double)numberOfSteps;

                Dispatcher.CurrentDispatcher
                    .Invoke(updateNumberDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, progressPercentage });
                Dispatcher.CurrentDispatcher
                    .Invoke(updateTextDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { Label.ContentProperty, progressText });
            }
        }

        private XDocument removeDuplicatePersonsFromXDocument(XDocument theXDocument)
        {
            IEnumerable<XElement> personsWithUID;
            IEnumerable<XElement> familiesWithUID;
            XDocument bufferXDocument = new XDocument();
            XDeclaration theDeclaration = new XDeclaration(theXDocument.Declaration.Version, theXDocument.Declaration.Encoding, theXDocument.Declaration.Standalone);
            bufferXDocument.Declaration = theDeclaration;
            bufferXDocument.Add(new XProcessingInstruction("xml-stylesheet", CHelper.getProcessingInstruction()));
            bufferXDocument.Add(new XElement(CConstants.xmlElementCatalog));
            bufferXDocument.Element(CConstants.xmlElementCatalog).Add(createStandardGEDCOMHeader());
            bool userStop = false;
            //************************************************************************
            //Clean XML from duplicate persons (duplicate "match" entries)
            //************************************************************************
            foreach (XElement personXElement in theXDocument.Descendants(CConstants.xmlElementPerson))
            {
                int[] numberOfNodes = new int[0];
                int arraySize = 0;
                int personCount = 0;

                try
                {
                    IEnumerable<XElement> descendantsPerson = theXDocument.Descendants(CConstants.xmlElementPerson);
                    foreach (XElement testElement in descendantsPerson)
                    {
                        //XElement testXElement = testElement.Element(CConstants.xmlElement_UID);
                        string testValue = testElement.Element(CConstants.xmlElement_UID).Attribute(CConstants.xmlAttributeValue).Value;
                        //string testValue = testAttrib.Value;
                        if (testValue == personXElement.Element(CConstants.xmlElement_UID)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value)
                        {
                        }
                    }

                    personsWithUID =
                        from XElement personWithUID in descendantsPerson
                        where
                        personWithUID.Element(CConstants.xmlElement_UID)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value ==
                        personXElement.Element(CConstants.xmlElement_UID)
                        .Attribute(CConstants.xmlAttributeValue)
                        .Value
                        select personWithUID;

                    personCount = personsWithUID.Count();
                }
                catch { personCount = 0; personsWithUID = null; }

                if (personCount == 0)
                {
                    string warningMessage = "Person with ID: " +
                                    personXElement
                                    .Attribute(CConstants.xmlAttributeOrigID)
                                    .Value +
                                    " does not have a unique identifier (UID).\n" +
                                    "Please check GEDCOM file and assign one if possible.";
                    string warningBoxCaption = "UID Missing";
                    MessageBoxButton theMessageBoxButtons = MessageBoxButton.OKCancel;
                    MessageBoxResult theResult = MessageBox.Show(warningMessage, warningBoxCaption, theMessageBoxButtons);

                    if (theResult == MessageBoxResult.Cancel)
                    {
                        userStop = true;
                        break;
                    }

                }
                else if (personCount == 1)
                {
                    bufferXDocument.Element(CConstants.xmlElementCatalog).Add(personXElement);
                }
                else if (personCount > 1)
                {
                    //Find records with the largest number of elements
                    // - this should be the one with the most information
                    foreach (XElement personWithUID in personsWithUID)
                    {
                        arraySize++;
                        Array.Resize(ref numberOfNodes, arraySize);
                        numberOfNodes[arraySize - 1] = personWithUID.DescendantsAndSelf().Count();
                    }
                    int indexMaxOfElements = Array.IndexOf(numberOfNodes, numberOfNodes.Max());

                    //Check if an entry with this UID already exists
                    IEnumerable<XElement> testList =
                        from XElement bufferXElement in bufferXDocument.Descendants(CConstants.xmlElementPerson)
                        where
                            bufferXElement
                            .Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value ==
                            personXElement
                            .Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value
                        select bufferXElement;

                    if (testList.Count() == 0) bufferXDocument.Element(CConstants.xmlElementCatalog).Add(personXElement);
                }
            }

            if (userStop == false)
            {
                foreach (XElement familyXElement in theXDocument.Descendants(CConstants.xmlElementFamily))
                {

                    int familyCount = 0;
                    try
                    {
                        familiesWithUID =
                            from XElement familyWithUID in theXDocument.Descendants(CConstants.xmlElementFamily)
                            where
                            familyWithUID.Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value ==
                            familyXElement.Element(CConstants.xmlElement_UID)
                            .Attribute(CConstants.xmlAttributeValue)
                            .Value
                            select familyWithUID;

                        familyCount = familiesWithUID.Count();
                    }
                    catch { familyCount = 0; familiesWithUID = null; }

                    //The family record does not have a UID -> issue message
                    if (familyCount == 0)
                    {
                        string warningMessage = "Family with ID: " +
                                        familyXElement
                                        .Attribute(CConstants.xmlAttributeOrigID)
                                        .Value +
                                        " does not have a unique identifier (UID).\n" +
                                        "Please check GEDCOM file and assign one if possible.";
                        string warningBoxCaption = "UID Missing";
                        MessageBoxButton theMessageBoxButtons = MessageBoxButton.OKCancel;
                        MessageBoxResult theResult = MessageBox.Show(warningMessage, warningBoxCaption, theMessageBoxButtons);

                        if (theResult == MessageBoxResult.Cancel)
                        {
                            userStop = true;
                            break;
                        }
                    }
                    //If the family record exists only once in the source file everything is fine
                    // -> Go ahead
                    else if (familyCount == 1)
                    {
                        bufferXDocument.Element(CConstants.xmlElementCatalog).Add(familyXElement);
                    }
                    //More than one family record with the same UID in the source file
                    // -> do some magic to unite these data
                    else if (familyCount > 1)
                    {
                        foreach (XElement familyRecord in familiesWithUID)
                        {
                            //If there are more than one (most likely two...) persons in source
                            //file with the same UID than take the person whose ID is already in the
                            //buffer file
                            foreach (XElement personInFamilyRecord in familyRecord.Descendants())
                            {
                                IEnumerable<XElement> personsInBufferFile =
                                    from XElement bufferPersonXElement in bufferXDocument.Descendants(CConstants.xmlElementPerson)
                                    where
                                    bufferPersonXElement
                                    .Attribute(CConstants.xmlAttributeOrigID)
                                    .Value ==
                                    personInFamilyRecord
                                    .Attribute(CConstants.xmlAttributeValue)
                                    .Value
                                    select bufferPersonXElement;

                                //person exists in buffer file -> add corresponding data as well
                                if (personsInBufferFile.Count() == 1)
                                {
                                    IEnumerable<XElement> familyRecordsInBufferFile =
                                        from XElement bufferFamilyXElement in bufferXDocument.Descendants(CConstants.xmlElementFamily)
                                        where
                                        bufferFamilyXElement
                                        .Element(CConstants.xmlElement_UID)
                                        .Attribute(CConstants.xmlAttributeValue)
                                        .Value ==
                                        familyRecord
                                        .Element(CConstants.xmlElement_UID)
                                        .Attribute(CConstants.xmlAttributeValue)
                                        .Value
                                        select bufferFamilyXElement;

                                    //No family record in buffer file so far -> add a family record
                                    //for current person (...and nothing more)
                                    if (familyRecordsInBufferFile.Count() == 0)
                                    {
                                        XElement newXElement = new XElement(CConstants.xmlElementFamily,
                                                        new XAttribute(CConstants.xmlAttributeOrigID,
                                                            familyRecord
                                                            .Attribute(CConstants.xmlAttributeOrigID)
                                                            .Value));
                                        newXElement.Add(personInFamilyRecord);
                                        newXElement.Add(familyRecord.Element(CConstants.xmlElement_UID));
                                        bufferXDocument.Element(CConstants.xmlElementCatalog).Add(newXElement);
                                        break;
                                    }
                                    //Family record exists -> add person data of current person
                                    else if (familyRecordsInBufferFile.Count() == 1)
                                    {
                                        XElement currentFamilyXElement = familyRecordsInBufferFile.ElementAt(0);
                                        // Add person if does not exist
                                        if (!currentFamilyXElement
                                            .Descendants().Any(test => test
                                                .Attribute(CConstants.xmlAttributeValue)
                                                .Value ==
                                            personInFamilyRecord
                                            .Attribute(CConstants.xmlAttributeValue)
                                            .Value))
                                        {
                                            currentFamilyXElement.Add(personInFamilyRecord);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bufferXDocument;
        }

        private XElement createStandardGEDCOMHeader()
        {
            XElement standardHeaderElement = new XElement(CConstants.xmlElementHeader,
                new XElement(CConstants.xmlElementSource, new XAttribute(CConstants.xmlAttributeValue, ""),
                    new XElement(CConstants.xmlElementSourceVersion, new XAttribute(CConstants.xmlAttributeValue, "1.0")),
                    new XElement(CConstants.xmlElementSourceName, new XAttribute(CConstants.xmlAttributeValue, "GEDCOM View and Compare")),
                    new XElement(CConstants.xmlElementSourceCorporation, new XAttribute(CConstants.xmlAttributeValue, "Wolfgang Geithner 2011")),
                new XElement(CConstants.xmlElementGEDC, new XAttribute(CConstants.xmlAttributeValue, ""),
                    new XElement(CConstants.xmlElementGEDC_Version, new XAttribute(CConstants.xmlAttributeValue, "5.5")),
                    new XElement(CConstants.xmlElementGEDC_Version, new XAttribute(CConstants.xmlAttributeValue, "LINEAGE-LINKED"))),
                new XElement(CConstants.xmlElementCharacterEncoding, new XAttribute(CConstants.xmlAttributeValue, "UTF-8"))
                    //new XElement(CConstants.xmlElementLanguage, new XAttribute(CConstants.xmlAttributeValue, )),
                    ));
            return standardHeaderElement;
        }

        private void create_UidWhereNeeded(XDocument theXDocument)
        {
            foreach (XElement theXElement in theXDocument.Descendants(CConstants.xmlElementPerson))
            {
                if (theXElement.Element(CConstants.xmlElement_UID) == null)
                {
                    theXElement.Add(new XElement(CConstants.xmlElement_UID,
                        new XAttribute(CConstants.xmlAttributeValue, theXElement
                            .Attribute(CConstants.xmlAttributeUID).Value)));
                }
            }

            foreach (XElement theXElement in theXDocument.Descendants(CConstants.xmlElementFamily))
            {
                if (theXElement.Element(CConstants.xmlElement_UID) == null)
                {
                    theXElement.Add(new XElement(CConstants.xmlElement_UID,
                        new XAttribute(CConstants.xmlAttributeValue, theXElement
                            .Attribute(CConstants.xmlAttributeUID).Value)));
                }
            }
        }

        private void hardBreakpoint()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        #endregion
    }
}