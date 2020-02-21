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

namespace RolfMichelsen.Dragon.DragonTools.IO
{
    /// <summary>
    /// Encapsulates a filesystem timestamp.
    /// </summary>
    public sealed class Timestamp
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;
        public readonly int Hour;
        public readonly int Minute;
        public readonly int Second;

        public Timestamp(int year, int month, int day, int hour, int minute, int second)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }


        public override string ToString()
        {
            return String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", Year, Month, Day, Hour, Minute, Second);
        }
    }
}
