using DocmentHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace DocGenerator
{ 
    public class Generator
    {
        static Generator _generator;
        Assembly _assembly;
        List<CustomApi> _apiList = new List<CustomApi>();

        private Generator()
        {
            string dllpath = ConfigurationManager.AppSettings["DLLPath"];
            _assembly = Assembly.LoadFrom(dllpath);
        }

        public static Generator GetGenerator()
        {
            if(_generator==null)
            {
                _generator=new Generator();
            }
            return _generator;
        }

        /// <summary>
        /// 构造Api接口
        /// </summary>
        public void GeneratorApi()
        {
            List<MethodInfo> methodList = GetApiMethods();
            //构造Api接口
            foreach (var method in methodList)
            {
                DescribleAttribute desc = method.GetCustomAttribute<DescribleAttribute>();
                CustomApi api = new CustomApi();
                api.OutParameterList = GetCmfChinaOutParameter(method.ReturnParameter);
                api.Name = desc.Name;
                api.Describe = desc.Describle;
                //Post Put
                WebInvokeAttribute webAttr = method.GetCustomAttribute<WebInvokeAttribute>();
                if (webAttr != null)
                {
                    api.RequestType = webAttr.Method;
                    api.Address = webAttr.UriTemplate;
                    //Post Put 输入参数都是第一个
                    if (api.RequestType.Equals("POST"))
                    {
                        ParameterInfo[] paraInfos = method.GetParameters();
                        api.PostParameterList = GetParameterList(paraInfos[0]);
                        //输入参数
                        api.InParameterList = GetParameterList(1, paraInfos);
                    }
                    if (api.RequestType.Equals("PUT"))
                    {
                        ParameterInfo[] paraInfos = method.GetParameters();
                        GetParameterList(paraInfos[0]);
                    }
                }
                //Get
                WebGetAttribute webGetAttr = method.GetCustomAttribute<WebGetAttribute>();
                if (webGetAttr != null)
                {
                    api.RequestType = "GET";
                    api.Address = webGetAttr.UriTemplate;
                    ParameterInfo[] paraInfos = method.GetParameters();
                    api.InParameterList = GetParameterList(0, paraInfos);
                }
                _apiList.Add(api);
            }
        }
        /// <summary>
        /// 返回接口文档名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetApiNameList()
        {
            return _apiList.Select(v => v.Name).ToList();
        }
        /// <summary>
        /// 根据接口名找到接口
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CustomApi GetCustomApi(string name)
        {
            return _apiList.FirstOrDefault(v=>v.Name.Equals(name));
        }

        #region private method
        /// <summary>
        /// 获取接口方法
        /// </summary>
        /// <returns></returns>
        private List<MethodInfo> GetApiMethods()
        {
            //获取所有接口名
            List<Type> interfaceList = new List<Type>();
            foreach (var type in _assembly.GetTypes())
            {
                if (type.IsInterface)
                {
                    interfaceList.Add(type);
                }
            }
            //找出接口带wcf标志的方法 
            List<MethodInfo> methodList = new List<MethodInfo>();
            foreach (var iface in interfaceList)
            {
                foreach (var method in iface.GetMethods())
                {
                    var v = method.GetCustomAttribute<OperationContractAttribute>();
                    if (v != null)
                    {
                        methodList.Add(method);
                    }
                }
            }
            List<MethodInfo> apiMethodList = new List<MethodInfo>();
            //找出带有生成文档标记的方法
            foreach (var method in methodList)
            {
                DescribleAttribute desc = method.GetCustomAttribute<DescribleAttribute>();
                if (desc != null)
                {
                    apiMethodList.Add(method);
                }
            }
            return apiMethodList;
        }
        /// <summary>
        /// 构造参数 Post/Put
        /// </summary>
        /// <param name="paraInfo"></param>
        /// <returns></returns>
        private List<Parameter> GetParameterList(ParameterInfo paraInfo)
        {
            Type type = paraInfo.ParameterType;
            List<Parameter> paraList = new List<Parameter>();
            foreach (var property in type.GetProperties())
            {
                paraList.Add(GetParameter(property));
            }
            return paraList;
        }
        /// <summary>
        /// 构造参数 输入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="paraInfos"></param>
        /// <returns></returns>
        private List<Parameter> GetParameterList(int index,ParameterInfo[] paraInfos)
        {
            if (paraInfos.Length == index)
                return null;
            List<Parameter> paraList = new List<Parameter>();
            for (int i=index;i<paraInfos.Length;++i)
            {
                ParameterInfo paraInfo = paraInfos[i];
                Parameter para = new Parameter();
                DescribleAttribute desc = paraInfo.GetCustomAttribute<DescribleAttribute>();
                //构造参数
                para.ParamName = paraInfo.Name;
                para.ParamType = paraInfo.ParameterType.Name;
                para.ParamDescribe = desc.Name;
                paraList.Add(para);
            }
            return paraList;
        }
        /// <summary>
        /// 获取输出参数 WCF 通用
        /// </summary>
        /// <param name="paraInfo"></param>
        /// <returns></returns>
        private List<Parameter> GetOutParameterList(ParameterInfo paraInfo)
        {
            Type type = paraInfo.ParameterType;
            List<Parameter> paraList = new List<Parameter>();
            if (type.IsClass)
            {
                foreach (var property in type.GetProperties())
                {
                    paraList.Add(GetParameter(property));
                }
            }
            else
            {
                Parameter para = new Parameter();
                DescribleAttribute desc = paraInfo.GetCustomAttribute<DescribleAttribute>();
                //构造参数
                para.ParamName = paraInfo.Name;
                para.ParamType = paraInfo.ParameterType.Name;
                para.ParamDescribe = desc.Describle;
                paraList.Add(para);
            }
            return paraList;
        }
        /// <summary>
        /// 获取输出参数 招商基金专用
        /// </summary>
        /// <param name="paraInfo"></param>
        /// <returns></returns>
        private List<Parameter> GetCmfChinaOutParameter(ParameterInfo paraInfo)
        {
            Type type = paraInfo.ParameterType;
            List<Parameter> paraList = new List<Parameter>();
            PropertyInfo propertyInfo= type.GetProperty("Data");
            Type proType = propertyInfo.PropertyType;
            foreach(PropertyInfo property in proType.GetProperties())
            {
                paraList.Add(GetParameter(property));
            }
            return paraList;
        }
        /// <summary>
        /// 获取单个参数
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private Parameter GetParameter(PropertyInfo property)
        {
            DescribleAttribute desc = property.GetCustomAttribute<DescribleAttribute>();
            if(desc!=null)
            {
                Parameter para = new Parameter();
                //构造参数
                para.ParamName = property.Name;
                para.ParamType = property.PropertyType.Name;
                para.ParamDescribe = desc.Name;
                return para;
            }
            return null;
        }
        #endregion
    }
}
