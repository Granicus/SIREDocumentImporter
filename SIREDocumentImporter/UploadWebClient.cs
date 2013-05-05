using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SIREDocumentImporter
{
    public class CustomWebClient : WebClient
    {
        private int _timeout;
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public CustomWebClient()
        {
            // We default the timeout to an insanely high 10 minutes
            this._timeout = 900000;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this._timeout;
            return result;
        }
    }
}
