/*
   Copyright 2011-2020 Rolf Michelsen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/

using System;


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// An OS-9 module file.
    /// </summary>
    public sealed class OS9ModuleFile : OS9File
    {
        /// <summary>
        /// The size of a standard (minimal) module header.  
        /// This includes the module header from the sync bytes up to and including the module header checksum.
        /// </summary>
        public const int StandardModuleHeaderSize = 9;


        /// <summary>
        /// Complete file data, including headers and payload.
        /// </summary>
        private byte[] data;


        /// <summary>
        /// Return the module type.
        /// </summary>
        public OS9ModuleType ModuleType { get { return (OS9ModuleType) ((data[6] >> 4) & 0x0f); }}


        /// <summary>
        /// Return the module size as given in the module header.  The module size includes the module header, payload data and 
        /// module CRC.  The module size may be different from GetData.Length.
        /// </summary>
        public int ModuleSize { get { return (data[2] << 8 | data[3]); } }


        /// <summary>
        /// Return the module language identifier.
        /// </summary>
        public int ModuleLanguage { get { return (data[6] & 0x0f); } }


        /// <summary>
        /// Return the module name.
        /// </summary>
        public string ModuleName { get; private set; }


        /// <summary>
        /// Return the module attributes.
        /// </summary>
        public int ModuleAttributes { get { return ((data[7] >> 4) & 0x0f); } }


        /// <summary>
        ///  Return the module revision number.
        /// </summary>
        public int ModuleRevision { get { return (data[7] & 0x0f); } }
        

        /// <summary>
        /// Header parity value as read from the file.
        /// </summary>
        public int HeaderParity { get { return data[8]; } }


        /// <summary>
        /// Module CRC as read from the file.
        /// </summary>
        public int ModuleCRC { get { return (data[ModuleSize - 3] << 16) | (data[ModuleSize - 2] << 8) | data[ModuleSize - 1]; } }


        /// <summary>
        /// Create a OS9 module file object.
        /// </summary>
        /// <param name="fileinfo">Directory information for this file, or <value>null</value> if no directory information is available.</param>
        /// <param name="filedata">Raw file data, including headers and payload.</param>
        /// <param name="validate">If cleared, an invalid module file can be created, i.e. header and file checksum values are not validated.</param>
        internal OS9ModuleFile(OS9FileInfo fileinfo, byte[] filedata, bool validate=true) : base(fileinfo)
        {
            if (filedata == null) throw new ArgumentNullException("filedata");
            var filename = fileinfo == null ? null : fileinfo.Name;
            if (filedata.Length < StandardModuleHeaderSize) throw new InvalidFileException(filename, String.Format("A module file must contain a {0} byte header.  This file is only {1} bytes long.", StandardModuleHeaderSize, filedata.Length));
            if (filedata[0] != 0x87 || filedata[1] != 0xcd) throw new InvalidFileException(filename, "Module file header does not contain valid sync sequence.");

            data = (byte[]) filedata.Clone();

            if (ModuleSize > data.Length) throw new InvalidFileException(filename, String.Format("The module header specifies a module size of {0} bytes but the file is only {1} bytes.", ModuleSize, data.Length));

            int nameoffset = (data[4] << 8) | data[5];
            if (nameoffset > data.Length) throw new InvalidFileException(filename, "Module filename offset is outside the file data.");
            ModuleName = OS9Utils.ParseString(data, nameoffset);

            if (validate)
            {
                if (HeaderParity != CalculateHeaderParity(data, StandardModuleHeaderSize-1)) throw new InvalidFileException(filename, "Header parity error");
                if (ModuleCRC != CalculateModuleCRC(data, ModuleSize-3)) throw new InvalidFileException(filename, "Invalid module CRC");
            }
        }



        /// <summary>
        /// Calculate the module header parity value.
        /// The header parity is the 1's complement of the xor of the data bytes.
        /// </summary>
        /// <param name="data">Array containing the data to calculate the parity over.</param>
        /// <param name="length">The number of bytes to calculate the parity over.</param>
        public static int CalculateHeaderParity(byte[] data, int length)
        {
            int parity = 0;
            for (int i = 0; i < length; i++)
                parity ^= data[i];
            return ((~parity) & 0xff);
        }


        /// <summary>
        /// Calculate the module CRC.
        /// The result is a 24-bit CRC value generated using the polynomial 0x800063.
        /// </summary>
        public static int CalculateModuleCRC(byte[] data, int length)
        {
            /* This algorithm is copied directly from the documentation of the F$CRC service request in the
             * OS-9 System Programmer's Manual. */
            var crc = 0xffffff;
            for (int i = 0; i < length; i++ )
            {
                crc ^= (data[i] << 16);
                for (int j=0; j<8; j++)
                {
                    crc <<= 1;
                    if ((crc & 0x1000000) != 0) crc ^= 0x800063;
                }
            }
            return ((crc ^ 0xffffff) & 0xffffff);
        }


        /// <summary>
        /// Return the file payload data.
        /// </summary>
        public override byte[] GetData()
        {
            return (byte[]) data.Clone();
        }




        /// <summary>
        /// Returns <value>true</value> if the data contains a module header.
        /// </summary>
        internal static bool IsModuleFile(byte[] data)
        {
            return (data.Length >= StandardModuleHeaderSize && data[0] == 0x87 && data[1] == 0xcd);
        }



        public override string ToString()
        {
            return String.Format("OS-9 Module File: type={0}", (int) ModuleType);
        }
    }



    /// <summary>
    /// OS-9 modile file types.
    /// </summary>
    public enum OS9ModuleType
    {
        Program = 1,
        Subroutine = 2,
        Multimodule = 3,
        Data = 4,
        Userdefined1 = 5,
        Userdefined2 = 6,
        Userdefined3 = 7,
        Userdefined4 = 8,
        Userdefined5 = 9,
        Userdefined6 = 10,
        Userdefined7 = 11,
        System = 12,
        FileManager = 13,
        DeviceDriver = 14,
        DeviceDescriptor = 15
    }

}
