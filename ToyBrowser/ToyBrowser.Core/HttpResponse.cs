using System;
using System.Collections.Generic;
using System.Text;

namespace ToyBrowser.Core
{
    public class HttpResponse
    {
        public string Status { get; set; }
        public List<HttpHeader> Headers { get; set; }
        public string Body { get; set; }

        public HttpResponse()
        {
            Headers = new List<HttpHeader>();
        }
    }
}
