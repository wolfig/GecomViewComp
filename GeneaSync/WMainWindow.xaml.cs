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
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class WMainWindow : Window
    {
        private bool diffCheckPerformed = false;
        private bool filesLoaded = false;

        private string _machineKey;
        public string machineKey
        {
            set { _machineKey = value; }
            get { return _machineKey; }
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        IDataManipulator theDataManipulator;
        CConstants.fileHandlerReturnStruct parsedFileData1 = new CConstants.fileHandlerReturnStruct();
        CConstants.fileHandlerReturnStruct parsedFileData2 = new CConstants.fileHandlerReturnStruct();

        public WMainWindow()
        {
            theDataManipulator = new CDataManipulator();
            InitializeComponent();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void openFiles_Click(object sender, RoutedEventArgs e)
        {
            diffCheckPerformed = false;
            filesLoaded = false;
            OpenFileDialog theFileDialog = new OpenFileDialog();
            theFileDialog.Title = "Open File 1 (left side)";

            theFileDialog.Filter = "GEDCOM File (*.ged)|*.ged";
            if (theFileDialog.ShowDialog() == true)
            {
                // Parse GEDCOM file to XML Document
                CGedcomFileHandler fileHandler1 = new CGedcomFileHandler();

                parsedFileData1 = fileHandler1.parseGedcomToXml(theFileDialog.FileName);
                theDataManipulator.theXmlDocument1 = parsedFileData1.xmlDoc;
                textBoxGDI1.Document = parsedFileData1.flowDoc;
                string[] fileNameArray;
                fileNameArray = theFileDialog.FileName.Split(new char[] { '\\' });
                int arraySize = fileNameArray.Length;
                headerGDI1.Text = fileNameArray[arraySize - 1].ToString() +
                                    ": " +
                                    parsedFileData1.personCounter.ToString() +
                                    " Persons";
            }

            theFileDialog.FileName = "";
            theFileDialog.Title = "Open File 2 (right side)";

            if (theFileDialog.ShowDialog() == true)
            {
                // Parse GEDCOM file to XML Document
                CGedcomFileHandler fileHandler2 = new CGedcomFileHandler();
                parsedFileData2 = fileHandler2.parseGedcomToXml(theFileDialog.FileName);
                theDataManipulator.theXmlDocument2 = parsedFileData2.xmlDoc;
                textBoxGDI2.Document = parsedFileData2.flowDoc;
                string[] fileNameArray;
                fileNameArray = theFileDialog.FileName.Split(new char[] { '\\' });
                int arraySize = fileNameArray.Length;
                headerGDI2.Text = fileNameArray[arraySize - 1].ToString() +
                                    ": " +
                                    parsedFileData2.personCounter.ToString() +
                                    " Persons";
            }

            if (theDataManipulator.theXDocument1 != null && theDataManipulator.theXDocument2 != null)
            {
                this.filesLoaded = true;
            }

            this.updateMetadata();
            this.Activate();

        }

        private void highlightDiff_Click(object sender, RoutedEventArgs e)
        {
            theDataManipulator.compareDocumentContent(theDataManipulator.theXDocument1, theDataManipulator.theXDocument2, textBoxGDI1);
            theDataManipulator.compareDocumentContent(theDataManipulator.theXDocument2, theDataManipulator.theXDocument1, textBoxGDI2);
            this.diffCheckPerformed = true;
            this.updateMetadata();
            this.displayResults();
        }

        private void buttonSort_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxGDI1.Document != null && theDataManipulator.theXDocument1 != null) theDataManipulator.sortDocumentBySurnames(textBoxGDI1, theDataManipulator.theXDocument1);
            if (textBoxGDI2.Document != null && theDataManipulator.theXDocument2 != null) theDataManipulator.sortDocumentBySurnames(textBoxGDI2, theDataManipulator.theXDocument2);
        }

        private void buttonHighlight_Click(object sender, RoutedEventArgs e)
        {
            parsedFileData1.xmlDoc = theDataManipulator.compareDocumentContent(theDataManipulator.theXDocument1, theDataManipulator.theXDocument2, textBoxGDI1);
            parsedFileData2.xmlDoc = theDataManipulator.compareDocumentContent(theDataManipulator.theXDocument2, theDataManipulator.theXDocument1, textBoxGDI2);
            this.diffCheckPerformed = true;
            this.updateMetadata();
            this.displayResults();
        }

        private void buttonIntersection_Click(object sender, RoutedEventArgs e)
        {
            CConstants.dataManipulatorStructure unionData = new CConstants.dataManipulatorStructure();

            // Find persons with attribute missing and merge the data in one theXDocument
            unionData = theDataManipulator.createIntersectionData();

            // Show result window
            Report theResultReport = new Report(unionData);
            theResultReport.Show();
        }

        private void buttonDiff_Click(object sender, RoutedEventArgs e)
        {
            CConstants.dataManipulatorStructure dataPersonsMissing = new CConstants.dataManipulatorStructure();

            // Find persons with attribute missing and merge the data in one theXDocument
            dataPersonsMissing = theDataManipulator.createDifferenceData();

            // Show result window
            Report theResultReport = new Report(dataPersonsMissing);
            theResultReport.Show();

        }

        private void buttonUnion_Click(object sender, RoutedEventArgs e)
        {
            CConstants.dataManipulatorStructure theUnionData = new CConstants.dataManipulatorStructure();

            // Find persons with attribute missing and merge the data in one theXDocument
            theUnionData = theDataManipulator.createUnionData();

            // Show result window
            Report theResultReport = new Report(theUnionData);
            theResultReport.Show();
        }
        
        /***************************** Keep for further development ***********************
        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            TextRange theTextRange;

            string searchText = textBoxSearchFor.Text;

            foreach (Block theBlock in textBoxGDI1.Document.Blocks)
            {
                theTextRange = new TextRange(theBlock.ContentStart, theBlock.ContentEnd);
                if (theTextRange.Text.Contains(searchText))
                {
                    textBoxGDI1.Focus();
                    var start = textBoxGDI1.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
                    textBoxGDI1.CaretPosition = theBlock.ContentStart;
                    var end = textBoxGDI1.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
                    textBoxGDI1.ScrollToVerticalOffset((start.Top + end.Bottom) / 2 + textBoxGDI1.VerticalOffset - textBoxGDI1.ViewportHeight / 10);
                    break;
                }
            }

            foreach (Block theBlock in textBoxGDI2.Document.Blocks)
            {
                theTextRange = new TextRange(theBlock.ContentStart, theBlock.ContentEnd);
                if (theTextRange.Text.Contains(searchText))
                {
                    textBoxGDI2.Focus();
                    var start = textBoxGDI2.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
                    textBoxGDI2.CaretPosition = theBlock.ContentStart;
                    var end = textBoxGDI2.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
                    textBoxGDI2.ScrollToVerticalOffset((start.Top + end.Bottom) / 2 + textBoxGDI2.VerticalOffset - textBoxGDI2.ViewportHeight / 10);
                    break;
                }
            }
        }
        */
        private void displayResults()
        {
            comparisonResults theCompResolutWindow = new comparisonResults();

            theCompResolutWindow.textBlock_Ambi1.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument1, CConstants.xmlAttributeCheckedValueAmbi);
            theCompResolutWindow.textBlock_Ambi2.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument2, CConstants.xmlAttributeCheckedValueAmbi);
            theCompResolutWindow.textBlock_Matc1.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument1, CConstants.xmlAttributeCheckedValueMatch);
            theCompResolutWindow.textBlock_Matc2.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument2, CConstants.xmlAttributeCheckedValueMatch);
            theCompResolutWindow.textBlock_Miss1.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument1, CConstants.xmlAttributeCheckedValueMiss);
            theCompResolutWindow.textBlock_Miss2.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument2, CConstants.xmlAttributeCheckedValueMiss);
            theCompResolutWindow.textBlock_NaMa1.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument1, CConstants.xmlAttributeCheckedValueNaMa);
            theCompResolutWindow.textBlock_NaMa2.Text = theDataManipulator.getPersonCountByCheckAttr(theDataManipulator.theXDocument2, CConstants.xmlAttributeCheckedValueNaMa);

            theCompResolutWindow.Show();
        }

        private void updateMetadata()
        {
            this.buttonHighlight.IsEnabled = this.filesLoaded;
            this.buttonDiff.IsEnabled = this.diffCheckPerformed;
            this.buttonIntersection.IsEnabled = this.diffCheckPerformed; 
            this.buttonUnion.IsEnabled = this.diffCheckPerformed;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.updateMetadata();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow theAboutWindow = new AboutWindow();
            theAboutWindow.Show();
        }

        private void saveFiles_Click(object sender, RoutedEventArgs e)
        {
            CGedcomFileHandler theFileHandler = new CGedcomFileHandler();
            if (parsedFileData1.flowDoc != null &&
                parsedFileData1.xmlDoc != null)
            {
                theFileHandler.saveDataToFile(parsedFileData1.flowDoc, parsedFileData1.xmlDoc);
            }
            if (parsedFileData2.flowDoc != null &&
                parsedFileData2.xmlDoc != null)
            {
                theFileHandler.saveDataToFile(parsedFileData2.flowDoc, parsedFileData2.xmlDoc);
            }
        }
    }
}
