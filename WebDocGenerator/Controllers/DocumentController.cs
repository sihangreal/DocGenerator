using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebDocGenerator.Models;

namespace WebDocGenerator.Controllers
{
    public class DocumentController : ApiController
    {
        [Route("Api/Document/Names")]
        public List<string> GetApiNameList()
        {
            Generator generator = Generator.GetGenerator();
            return generator.GetApiNameList();
        }

        [Route("Api/Document/{name}")]
        public CustomApi GetCustomApi(string name)
        {
            Generator generator = Generator.GetGenerator();
            CustomApi api = generator.GetCustomApi(name);
            return api;
        }
    }
}
