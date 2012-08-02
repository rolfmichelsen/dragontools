/*
Copyright (c) 2011-2012, Rolf Michelsen
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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// Encapsulates a filename in the Dragon tape filesystem.  
    /// In this filesystem a filename is simply a sequence of up to 8 characters.  There is no concept
    /// of filename extensions, paths, etc.
    /// </summary>
    public sealed class DragonFileName : IFileName, ICloneable, IEquatable<DragonFileName>
    {
        /// <summary>
        /// The name of a file, omitting any directory information for hierarchical filesystems.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// The complete path of the file.  This is equal to Name for flat filesystems.
        /// </summary>
        public string Path { get { return Name; }}


        /// <summary>
        /// The name of a file, omitting any file name extensions.
        /// The Dragon tape filesystem does not support extensions so this is similar to <see cref="Name">Name</see>.
        /// </summary>
        public string Base { get { return Name; }}


        /// <summary>
        /// The extension of the file.
        /// The Dragon tape filesystem does not support extensions so this will always return <value>null</value>.
        /// </summary>
        public string Extension { get { return null; }}


        /// <summary>
        /// Returns the path of the directory containing this file.  
        /// The Dragon tape filesystem does not support directories so this will always return <value>null</value>.
        /// </summary>
        public string Ascend()
        {
            return null;
        }


        /// <summary>
        /// Returns the path of the child file contained in the directory represented by the current file name.
        /// The Dragon tape filesystem does not support directories so this will always throw a InvalidOperationException.
        /// </summary>
        /// <param name="child">Name of file to descend to.</param>
        /// <exception cref="InvalidOperationException">Thrown for a flat filesystem.</exception>
        public string Descend(string child)
        {
            throw new InvalidOperationException(GetType().FullName + " is not a hierarchical filesystem.");
        }


        /// <summary>
        /// True for a valid filename.
        /// </summary>
        public bool IsValid()
        {
            return true;
        }



        /// <summary>
        /// Create a filename object for the given file name.
        /// </summary>
        /// <param name="filename">Filename to encapsulate.</param>
        public DragonFileName(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");
            Name = filename;
        }


        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is DragonFileName)) return false;
            return Equals((DragonFileName) obj);
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DragonFileName obj)
        {
            if (obj == null) return false;
            return Name.Equals(obj.Name);
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new DragonFileName(Name);
        }
    }
}
