using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace DocGenerator
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
            CustomApi api= generator.GetCustomApi(name);
            return api;
        }
    }
}
