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
using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;

namespace RolfMichelsen.Dragon.DragonTools.DragonTape
{
    /// <summary>
    /// Program for dupling information about the contents of a virtual Dragon cassette.
    /// This program operates on the tape block structure.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }


        private void Run(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("ERROR: The program needs a single argument specifying the name of the tape file.");
                return;
            }

            try
            {
                DumpBlocks(args[0]);
            }
            catch (System.IO.IOException)
            {
                Console.Error.WriteLine("ERROR: Local filesystem I/O error.");
                return;
            }
        }




        private void DumpBlocks(string tapefile)
        {
            //TODO Print more information about each tape block
            using (var tape = new CasTape(new FileStream(tapefile, FileMode.Open, FileAccess.Read)))
            {
                try
                {
                    while (true)
                    {
                        var block = DragonTapeBlock.ReadBlock(tape, 1);
                        Console.WriteLine(block.ToString());
                    }
                }
                catch (EndOfTapeException)
                {
                    return;
                }
            }
        }
    }
}
