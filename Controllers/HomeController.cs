using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using S5Lk;
using S5LkForm.Models;
using S5LkForm.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace S5LkForm.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ILogger<HomeController> Logger { get; }
        private S5Client S5Client { get; }

        public HomeController(ILogger<HomeController> logger, S5Client s5Client)
        {
            Logger = logger;
            S5Client = s5Client;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListStofnanir()
        {
            return View(StofnanirTable());
        }

        public IActionResult ListBeidni(
            string IBU
        )
        {
            return View(BeidniTable(IBU));
        }

        public IActionResult ListFellilistar(
            string Listid
        )
        {
            return View(FellilistarTable(Listid));
        }

        private DataTable StofnanirTable()
        {
            return S5Client.CallTable(
                new GetViewStofnanirRequest()
                {
                    S5Username = S5Client.User,
                    S5Password = S5Client.Password
                }
            );
        }

        private DataTable BeidniTable(
            string IBU = "*"
        )
        {
            return S5Client.CallTable(
                new GetViewBeidnirStofnanirRequest()
                {
                    S5Username = S5Client.User,
                    S5Password = S5Client.Password,
                    Stofnun = IBU
                }
            );
        }

        private DataTable FellilistarTable(
            string Listid = "*"
        )
        {
            return S5Client.CallTable(
                new GetViewFellilistarRequest()
                {
                    S5Username = S5Client.User,
                    S5Password = S5Client.Password,
                    Listid = Listid
                }
            );
        }

        private DataTable SkjolTable(
            string S5RequestID
        )
        {
            return S5Client.CallTable(
                new GetViewAPP_FileRequest()
                {
                    S5Username = S5Client.User,
                    S5Password = S5Client.Password,
                    Request = S5RequestID
                }
            );
        }

        public class ListViewModel
        {
            public DataTable Table { get; set; }
            public DataRow[] Rows { get; set; }
        }

        [HttpGet]
        public IActionResult CreateAndUpdateBeidni(
            string IBU,
            string S5RequestID
        )
        {
            ValuesBEI values = new()
            {
                 IBU = IBU,
                 Forgangur = "Meðal"
            };

            if (!string.IsNullOrEmpty(S5RequestID))
            {
                ServiceSoap client = S5Client.Get;
                try
                {
                    GetBeidniRequest request = new()
                    {
                        S5Username = S5Client.User,
                        S5Password = S5Client.Password,
                        S5RequestID = S5RequestID
                    };
                    GetBeidniResponse response = client.GetBeidni(request);

                    if (response.GetBeidniResult.success)
                    {
                        values = response.GetBeidniResult.values;
                    }
                    else
                    {
                        throw new Exception($"Gat ekki lesið Beidni fyrir S5RequstID={S5RequestID}");
                    }
                }
                finally
                {
                    ((IClientChannel)client).Close();
                }
            }

            return View(
                new BeidniModel 
                { 
                    Values = values, 
                    StofnanirTable = StofnanirTable(),
                    SkjolTable = SkjolTable(S5RequestID ?? "xxxEmpty"),
                    ForgangurTable = FellilistarTable("BEI_Forgangur")
                }
            );
        }

        public class BeidniModel
        {
            public ValuesBEI Values { get; set; }
            public DataTable StofnanirTable { get; set; }
            public DataTable SkjolTable { get; set; }
            public DataTable ForgangurTable { get; set; }
        }

        [HttpPost]
        public IActionResult CreateAndUpdateBeidni(
            string IBU,
            string Heiti,
            string Lysing,
            string Tengilidur,
            string Simi,
            string Forgangur,
            List<IFormFile> files
        )
        {
            ValuesBEI values = DoBeidni(IBU, Heiti, Lysing, files, null, User.Identity.Name, null, Tengilidur, Simi, null, null, Forgangur, null, null, null, null, null);
            ViewBag.ResultOfLastRequest = values;
            return View(
                new BeidniModel { 
                    Values = values, 
                    StofnanirTable = StofnanirTable(),
                    SkjolTable = SkjolTable(values.S5RequestID),
                    ForgangurTable = FellilistarTable("BEI_Forgangur")
                }
            );
        }

        private ValuesBEI DoBeidni(
            string IBU,
            string Heiti,
            string Lysing,
            List<IFormFile> files,
            string Stada,
            string Umbedid_af, 
            string Tegund_beidni, 
            string Tengilidur,
            string Simi, 
            string Tegund_vinnu, 
            System.Nullable<System.DateTime> Umbedinn_dagur,
            string Forgangur,
            string Umbedin_klst, 
            string Urlausnaradili, 
            System.Nullable<System.DateTime> Dags_urlausnar, 
            string Urlausn,
            string Svar_til_leigjanda,
            ServiceSoap client = null
        )
        {
            client ??= S5Client.Get;

            CreateAndUpdateBeidni_PRequest request = new()
            {
                S5Username = S5Client.User,
                S5Password = S5Client.Password,
                IBU = IBU,
                Nafn_tengilids = Tengilidur,
                Simi_tengilids = Simi,
                Forgangur = Forgangur,
                Heiti = Heiti,
                Lysing = Lysing,
                Umbedid_af = Umbedid_af
            };

            Task<CreateAndUpdateBeidni_PResponse> task = client.CreateAndUpdateBeidni_PAsync(request);
            ReturnValueBEI result = task.Result.CreateAndUpdateBeidni_PResult;
            if (result != null && !result.success)
            {
                throw new Exception($"CreateAndUpdateBeidni failes with {result.message}");
            }
            ValuesBEI values = result.values;

            if (files != null) DoAddFiles(values.S5RequestID, files, client);

            return values;
        }

        private void DoAddFiles(string s5EequestID, List<IFormFile> files, ServiceSoap client = null)
        {
            client ??= S5Client.Get;

            foreach (IFormFile file in files)
            {
                MemoryStream memoryStream = new();
                using (Stream fileStream = file.OpenReadStream())
                {
                    fileStream.CopyTo(memoryStream);
                }
                byte[] bytes = memoryStream.ToArray();

                AddFileRequest request = new()
                {
                    S5Username = S5Client.User,
                    S5Password = S5Client.Password,
                    S5RequestID = s5EequestID,
                    File = bytes,
                    Filename = file.FileName
                };

                Task<AddFileResponse> task = client.AddFileAsync(request);
                AddFileResponse response = task.Result;
                if (!response.AddFileResult.success)
                {
                    throw new Exception($"Add file {file.FileName} failes with {response.AddFileResult.message}");
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
