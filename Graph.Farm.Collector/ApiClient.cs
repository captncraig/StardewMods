using System;
using System.Net;
using System.Threading.Tasks;
using Pathoschild.Http.Client;

namespace Graph.Farm.Collector
{

    public class NewAccountResponse
    {
        public string SecretToken { get; set; }
        public string ID { get; set; }
    }

    class ApiClient
    {
        private FluentClient client;
        public string ApiToken;

        public ApiClient(string host)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client = new FluentClient(host);
        }

        public Task<NewAccountResponse> CreateAccount()
        {
            return client.PostAsync("/api/newAccount").As<NewAccountResponse>();
        }


    }
}
