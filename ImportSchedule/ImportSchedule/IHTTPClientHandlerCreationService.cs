using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ImportSchedule
{
    public interface IHTTPClientHandlerCreationService
    {
        HttpClientHandler GetInsecureHandler();
    }
}
