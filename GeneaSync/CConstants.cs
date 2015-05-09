using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace GedcomViewCompare
{
    static public class CConstants
    {
        public struct fileHandlerReturnStruct
        {
            public XmlDocument xmlDoc;
            public UInt16 personCounter;
            public FlowDocument flowDoc;
        }

        public struct personInFlowDocStruct
        {
            public bool isPersonInFlowDoc;
            public string personID;
        }

        public struct dataManipulatorStructure
        {
            public FlowDocument flowDoc;
            public XDocument XDoc;
        }

        public const string xmlAttributeChecked = "Checked";
        public const string xmlAttributeFile = "File"; 
        public const string xmlAttributeMatchPartner = "matchPartner";
        public const string xmlAttributeName = "Name";
        public const string xmlAttributeOrigID = "OrigID";
        public const string xmlAttributeUID = "UID";
        public const string xmlAttributeValue = "Value";
        public const string xmlAttributeCheckedValueAmbi = "Ambigious";
        public const string xmlAttributeCheckedValueMatch = "Match";
        public const string xmlAttributeCheckedValueMiss = "Missing";
        public const string xmlAttributeCheckedValueNaMa = "Name-Match";
        public const string xmlElementBirth = "BIRT_L1";
        public const string xmlElementCatalog = "CATALOG";
        public const string xmlElementCharacterEncoding = "CHAR_L1";
        public const string xmlElementChild = "CHIL_L1";
        public const string xmlElementDate = "DATE_L1";
        public const string xmlElementDateL2 = "DATE_L2";
        public const string xmlElementDeath = "DEAT_L1";
        public const string xmlElementFamily = "FAM_L0";
        public const string xmlElementFamilyChild = "FAMC_L1";
        public const string xmlElementFamilySpouse = "FAMS_L1";
        public const string xmlElementGEDC = "GEDC_L1";
        public const string xmlElementGEDC_FORM = "FORM_L2";
        public const string xmlElementGEDC_Version = "VERS_L2";
        public const string xmlElementHeader = "HEAD_L0";
        public const string xmlElementHusband = "HUSB_L1";
        public const string xmlElementLanguage = "LANG_L1";
        public const string xmlElementName = "NAME_L1";
        public const string xmlElementPerson = "INDI_L0";
        public const string xmlElementRepository = "REPO_L0";
        public const string xmlElementSex = "SEX_L1";
        public const string xmlElementSourceL0 = "SOUR_L0";
        public const string xmlElementSource = "SOUR_L1";
        public const string xmlElementSourceCorporation = "CORP_L2";
        public const string xmlElementSourceData = "DATA_L2";
        public const string xmlElementSourceName = "NAME_L2";
        public const string xmlElementSourceVersion = "VERS_L2";
        public const string xmlElement_UID = "_UID_L1";
        public const string xmlElementWife = "WIFE_L1";

        public const string markerSpouse = "Spouse";
        public const string markerSelf = "Self";
        public const string markerMother = "Mother";
        public const string markerFather = "Father";

        public const string particleFam = "FAM";
        public const string particleHead = "HEAD";
        public const string particleIndi = "INDI";

        public const string uidTemplate = "_N:_S:_B:_D:_U:";
        public const string familyUidTemplate = "_H:_W:_D:";

        public const string debugIndicator = "DEBU_L0";

        public const string serialKey = "7f0b-bf80-3145-ccf6-7baf-04ae-a6b2-c813";
    }
}
