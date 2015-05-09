using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace GedcomViewCompare
{
    /// <summary>
    /// Interaktionslogik für Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        CGedcomFileHandler theFileHandler;
        CDataManipulator theDataManipulator;
        CConstants.dataManipulatorStructure theReportData;

        public Report(CConstants.dataManipulatorStructure reportDataInput)
        {
            theReportData = new CConstants.dataManipulatorStructure();
            theFileHandler = new CGedcomFileHandler();
            theDataManipulator = new CDataManipulator();

            theReportData = reportDataInput;
            InitializeComponent();
            this.theReportTextBox.Document = theReportData.flowDoc;
            int numberOfPersons = theReportData.flowDoc.Blocks.Where(test => test.Name.Contains("INDI_")).Count();
            this.Title = "Combined Data: " + numberOfPersons.ToString() + " Persons";
        }

        private void buttonSaveReport_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument theXmlDocument = CHelper.convertXDocToXmlDoc(theReportData.XDoc);
            theFileHandler.saveDataToFile(theReportData.flowDoc, theXmlDocument);
        }
    }
}
