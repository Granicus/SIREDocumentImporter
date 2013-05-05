using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SIREDocumentImporter
{
    /// <summary>  
    /// MimePart  
    /// Abstract class for all MimeParts  
    /// </summary>  
    public abstract class MimePart
    {
        public string Name { get; set; }

        public abstract string ContentDisposition { get; }

        public abstract string ContentType { get; }

        public abstract void CopyTo(Stream stream);

        public String Boundary
        {
            get;
            set;
        }
    }  
}
