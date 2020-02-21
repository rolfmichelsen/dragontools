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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Encapsulates data to uniquely identify a sector.
    /// </summary>
    public struct SectorId : IEquatable<SectorId>
    {
        private readonly int head;
        private readonly int track;
        private readonly int sector;

        public int Head { get { return head; }}
        public int Track { get { return track; }}
        public int Sector { get { return sector; }}

        public SectorId(int head, int track, int sector)
        {
            this.head = head;
            this.track = track;
            this.sector = sector;
        }


        public bool Equals(SectorId other)
        {
            return head == other.head && track == other.track && sector == other.sector;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            return this.Equals((SectorId) obj);
        }


        public static bool operator ==(SectorId left, SectorId right)
        {
            return left.sector == right.sector && left.track == right.track && left.head == right.head;
        }


        public static bool operator !=(SectorId left, SectorId right)
        {
            return !(left == right);
        }


        public override int GetHashCode()
        {
            return head*3557 + track*3559 + sector*3571;
        }

    }
}
