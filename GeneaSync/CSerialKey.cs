using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace GedcomViewCompare
{
    static class CSerialKey
    {
        public static string getSerialKey()
        {
            string theProcessorID = getCPUInfo();
            string theDiskID = getDiskID();
            string serialString = theDiskID + theProcessorID;

            Byte[] uidBytes = new Byte[1];

            if (serialString != "")
            {
                serialString = serialString.ToUpper();

                ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                uidBytes = asciiEncoding.GetBytes(serialString);

                MD5 md5 = MD5.Create();
                Byte[] hashArray = md5.ComputeHash(uidBytes);
                StringBuilder theStringBuilder = new StringBuilder();
                serialString = CConstants.uidTemplate;
                foreach (Byte hashByte in hashArray)
                {
                    theStringBuilder.Append(hashByte.ToString("x2"));
                }

                return theStringBuilder.ToString();
            }
            else
            {
                return serialString;
            }
        }

        private static string getCPUInfo()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (cpuInfo == "")
                {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        private static string getDiskID()
        {
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + "C" + @":""");
            dsk.Get();
            string volumeSerial = dsk["VolumeSerialNumber"].ToString();
            return volumeSerial;
        }
    }
}
