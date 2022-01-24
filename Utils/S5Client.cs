using Microsoft.Extensions.Configuration;
using S5Lk;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

namespace S5LkForm.Utils
{
    public class S5Client
    {
        public IConfiguration Configuration { get; }

        public ChannelFactory<ServiceSoap> Factory { get; set; }

        public ServiceSoap Get
        {
            get
            {
                return Factory.CreateChannel();
            }
        }

        public string User { get; }
        public string Password { get; }

        public S5Client(IConfiguration configuration)
        {
            Configuration = configuration;

            User = Configuration["S5:User"];
            Password = Configuration["S5:Password"];

            BasicHttpBinding binding = new BasicHttpBinding
            {
                SendTimeout = TimeSpan.FromSeconds(100),
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = true,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max
            };
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            EndpointAddress address = new EndpointAddress(Configuration["S5:EndPoint"]);
            Factory = new ChannelFactory<ServiceSoap>(binding, address);

            if (!Factory.Endpoint.EndpointBehaviors.TryGetValue(typeof(ClientCredentials), out IEndpointBehavior behaviour))
            {
                throw new Exception("Endpoint, could not obtain ClientCredentials");
            }
            ClientCredentials credentials = (ClientCredentials)behaviour;
            credentials.UserName.UserName = Configuration["S5:User"];
            credentials.UserName.Password = Configuration["S5:Password"];
        }

        public DataTable CallTable(
            object request
        )
        {
            Type reqType = request.GetType();
            string reqFullName = reqType.FullName;

            //GetViewObjectRequest
            string reqName = reqFullName.Split('.').ToList().LastOrDefault();
            //GetViewObject
            string mName = reqName.Substring(0, reqName.LastIndexOf("Request"));
            //Object
            string tName = mName.Substring("GetView".Length);

            ServiceSoap client = Get;
            object response = client.GetType().GetMethod(mName).Invoke(client, new[] { request });

            //GetViewObjectResult
            ReturnValueView view = (ReturnValueView)response.GetType().GetField(mName + "Result").GetValue(response);
            if (view.table.Any1.IsEmpty)
            {
                return 
                    new DataTable()
                    { 
                        TableName = "xxxEmpty",
                        Columns = { { "Skilaboð", typeof(string) } },
                        Rows = { { "Engin gögn fundust." } }
                    };
            }

            //Populate DataSet and retrieve the "Object" table
            DataSet dataSet = new();
            using (StringReader resStringReader = new(view.table.Any1.InnerXml))
            {
                dataSet.ReadXml(resStringReader);
            }
            DataTable table = dataSet.Tables[tName];

            return table;
        }
    }
}
