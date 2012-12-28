/*
Copyright (c) 2012, Rolf Michelsen
All rights reserved.

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright 
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright 
      notice, this list of conditions and the following disclaimer in the 
      documentation and/or other materials provided with the distribution.
    * Neither the name of Rolf Michelsen nor the 
      names of other contributors may be used to endorse or promote products 
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY 
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
            catch (System.IO.IOException e)
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
                var blockno = 0;
                try
                {
                    while (true)
                    {
                        var block = DragonTapeBlock.ReadBlock(tape, 1);
                        Console.WriteLine(block.ToString());
                    }
                }
                catch (EndOfTapeException e)
                {
                    return;
                }
            }
        }
    }
}
