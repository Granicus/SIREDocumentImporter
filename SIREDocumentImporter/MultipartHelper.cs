using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace SIREDocumentImporter
{
    /// <summary>  
    /// Helper class to aid in uploading multipart  
    /// entities to HTTP web endpoints.  
    /// </summary>  
    public class MultipartHelper
    {
        private static Random random = new Random(Environment.TickCount);

        private List<NameValuePart> formData = new List<NameValuePart>();
        private FilesCollection files = null;
        private MemoryStream bufferStream = new MemoryStream();
        private string boundary;

        public String Boundary { get { return boundary; } }

        public static String GetBoundary()
        {
            return Environment.TickCount.ToString("X");
        }

        public MultipartHelper()
        {
            this.boundary = MultipartHelper.GetBoundary();
        }

        public void Add(NameValuePart part)
        {
            this.formData.Add(part);
            part.Boundary = boundary;
        }

        public void Add(FilePart part)
        {
            if (files == null)
            {
                files = new FilesCollection();
            }
            this.files.Add(part);
        }

        public void Upload(WebClient client, string address, string method)
        {
            // set header  
            client.Headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + this.boundary);
            Trace.WriteLine("Content-Type: multipart/form-data; boundary=" + this.boundary + "\r\n");

            // first, serialize the form data  
            foreach (NameValuePart part in this.formData)
            {
                part.CopyTo(bufferStream);
            }

            // serialize the files.  
            this.files.CopyTo(bufferStream);

            if (this.files.Count > 0)
            {
                // add the terminating boundary.  
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("--{0}", this.Boundary).Append("\r\n");
                byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                bufferStream.Write(buffer, 0, buffer.Length);
            }

            bufferStream.Seek(0, SeekOrigin.Begin);

            Trace.WriteLine(Encoding.ASCII.GetString(bufferStream.ToArray()));
            byte[] response = client.UploadData(address, method, bufferStream.ToArray());
            Trace.WriteLine("----- RESPONSE ------");
            Trace.WriteLine(Encoding.ASCII.GetString(response));
        }

        /// <summary>  
        /// Helper class that encapsulates all file uploads  
        /// in a mime part.  
        /// </summary>  
        class FilesCollection : MimePart
        {
            private List<FilePart> files;

            public FilesCollection()
            {
                this.files = new List<FilePart>();
                this.Boundary = MultipartHelper.GetBoundary();
            }

            public int Count
            {
                get { return this.files.Count; }
            }

            public override string ContentDisposition
            {
                get
                {
                    return String.Format("form-data; name=\"{0}\"", this.Name);
                }
            }

            public override string ContentType
            {
                get { return String.Format("multipart/mixed; boundary={0}", this.Boundary); }
            }

            public override void CopyTo(Stream stream)
            {
                foreach (FilePart part in files)
                {
                    part.Boundary = this.Boundary;
                    part.CopyTo(stream);
                }
            }

            public void Add(FilePart part)
            {
                this.files.Add(part);
            }
        }
    }
}
