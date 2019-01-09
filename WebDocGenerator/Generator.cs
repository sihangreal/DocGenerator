using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using WebDocGenerator.Models;

namespace WebDocGenerator
{
    public class Generator
    {
        static Generator _generator;
        Assembly _assembly;//程序集
        List<CustomApi> _apiList = new List<CustomApi>();
        XmlDocument _xmlDoc;//注释xml

        private Generator()
        {
            string dllpath = ConfigurationManager.AppSettings["DLLPath"];
            string xmlPath = ConfigurationManager.AppSettings["XmlPath"];

            _assembly =Assembly.LoadFrom(dllpath);
            _xmlDoc = LoadXmlDocument(xmlPath);
        }

        public static Generator GetGenerator()
        {
            if (_generator == null)
            {
                _generator = new Generator();
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
                CustomApi api = new CustomApi();
                api.Name = GetMethodAnnoatation(method);
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
                        //Post参数
                        api.PostParameterList = GetParameterList(paraInfos[0]);
                        //输入参数
                        api.InParameterList = GetParameterList(1, method);
                    }
                    if (api.RequestType.Equals("PUT"))
                    {
                        ParameterInfo[] paraInfos = method.GetParameters();
                        //Put参数
                        api.PutParameterList = GetParameterList(paraInfos[0]);
                        //输入参数
                        api.InParameterList = GetParameterList(1, method);
                    }
                }
                //Get
                WebGetAttribute webGetAttr = method.GetCustomAttribute<WebGetAttribute>();
                if (webGetAttr != null)
                {
                    api.RequestType = "GET";
                    api.Address = webGetAttr.UriTemplate;
                    ParameterInfo[] paraInfos = method.GetParameters();
                    //输入参数
                    api.InParameterList = GetParameterList(0, method);
                }
                //输出参数
                api.OutParameterList = GetCmfChinaOutParameter(method);
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
            return _apiList.FirstOrDefault(v => v.Name.Equals(name));
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
            return methodList;
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
            paraList = GetParameter(type);
            return paraList;
        }
        /// <summary>
        /// 构造参数 输入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="paraInfos"></param>
        /// <returns></returns>
        private List<Parameter> GetParameterList(int index, MethodInfo method)
        {
            ParameterInfo[] paraInfos = method.GetParameters();
            if (paraInfos.Length == index)
                return null;
            List<Parameter> paraList = new List<Parameter>();
            for (int i = index; i < paraInfos.Length; ++i)
            {
                ParameterInfo paraInfo = paraInfos[i];
                Parameter para = new Parameter();
                //构造参数
                para.ParamName = paraInfo.Name;
                para.ParamType = paraInfo.ParameterType.Name;
                para.ParamDescribe = GetMethodParameterAnnoatation(method, paraInfo.Name);
                paraList.Add(para);
            }
            return paraList;
        }
        /// <summary>
        /// 获取输出参数 WCF 通用
        /// </summary>
        /// <param name="paraInfo"></param>
        /// <returns></returns>
        private List<Parameter> GetOutParameterList(MethodInfo method, ParameterInfo paraInfo)
        {
            Type type = paraInfo.ParameterType;
            List<Parameter> paraList = GetParameter(type);
            return paraList;
        }
        /// <summary>
        /// 获取输出参数 招商基金专用
        /// </summary>
        /// <param name="paraInfo"></param>
        /// <returns></returns>
        private List<Parameter> GetCmfChinaOutParameter(MethodInfo method)
        {
            ParameterInfo paraInfo = method.ReturnParameter;
            Type type = paraInfo.ParameterType;
            List<Parameter> paraList = new List<Parameter>();
            if (type.IsClass && !type.Name.Equals("Stream"))
            {
                PropertyInfo propertyInfo = type.GetProperty("Data");
                Type proType = propertyInfo.PropertyType;
                paraList=GetParameter(proType);
            }
            else
            {
                Parameter para = new Parameter();
                //构造参数
                para.ParamName = "下载的文件";
                para.ParamType = type.Name;
                para.ParamDescribe = "下载的文件";
                paraList.Add(para);
            }
            return paraList;
        }
        /// <summary>
        /// 获取单个参数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<Parameter> GetParameter(Type type)
        {
            List<Parameter> parameterList = new List<Parameter>();
            if (type.IsGenericType)
            {
                Parameter parameter = new Parameter();
                parameter.ParamType = type.Name;
                Type[] gptyes = type.GetGenericArguments();
                Type gtype = gptyes[0];
                if (gtype.IsClass)
                {
                    parameter.childParam = GetParameter(gtype);
                }
                else
                {
                    Parameter gparam = new Parameter();
                    if (type.Name.Equals("Nullable`1"))
                        gparam.ParamType = gtype.GenericTypeArguments[0].Name;
                    else
                        gparam.ParamType = gtype.Name;
                    parameter.childParam = new List<Parameter>() { gparam };
                }
                parameterList.Add(parameter);
            }
            else
            {
                foreach (var pinfo in type.GetProperties())
                {
                    Parameter parameter = new Parameter();
                    parameter.ParamName = pinfo.Name;
                    Type ptype = pinfo.PropertyType;

                    DisplayNameAttribute disPaly = pinfo.GetCustomAttribute<DisplayNameAttribute>();
                    string desc = "";
                    if (disPaly != null)
                    {
                        desc = disPaly.DisplayName;
                    }
                    else
                    {
                        string paraName = "P:" + pinfo.DeclaringType.FullName + "." + pinfo.Name;
                        desc = GetParameterAnnoation(paraName);
                    }
                    parameter.ParamDescribe = desc;

                    if (ptype.Name.Equals("Nullable`1"))
                        parameter.ParamType = ptype.GenericTypeArguments[0].Name;
                    else if (ptype.IsGenericType)
                    {
                        parameter.ParamType = ptype.Name;
                        Type[] gptyes = ptype.GetGenericArguments();
                        Type gtype = gptyes[0];
                        if (gtype.IsClass)
                        {
                            parameter.childParam = GetParameter(gtype);
                        }
                        else
                        {
                            Parameter gparam = new Parameter();
                            if (ptype.Name.Equals("Nullable`1"))
                                gparam.ParamType = gtype.GenericTypeArguments[0].Name;
                            else
                                gparam.ParamType = gtype.Name;
                            parameter.childParam = new List<Parameter>() { gparam };
                        }
                    }
                    else if (ptype.IsValueType)
                    {
                        parameter.ParamType = ptype.Name;
                    }
                    else if (ptype.IsClass)
                    {
                        if (ptype.Name.Equals("String"))
                            parameter.ParamType = ptype.Name;
                        else
                        {
                            parameter.ParamType = ptype.Name;
                            parameter.childParam = GetParameter(ptype);
                        }
                    }
                    parameterList.Add(parameter);
                }
            }
            return parameterList;
        }

        /// <summary>
        /// 加载注释XML
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private XmlDocument LoadXmlDocument(string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filename);
            }
            catch (Exception e)
            {
                //显示错误信息  
                Console.WriteLine(e.Message);
                return null;
            }
            return xmlDoc;
        }
        /// <summary>
        /// 找到该方法的节点
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private XmlNode GetMethodNode(MethodInfo method)
        {
            string key = GetMehthodFullName(method);
            XmlNodeList nodeList = _xmlDoc.SelectNodes("/doc/members/member");
            foreach (XmlNode node in nodeList)
            {
                string temp = node.Attributes["name"].Value;
                if (temp.Equals(key))
                {
                    return node;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取方法的完全名称
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetMehthodFullName(MethodInfo method)
        {
            StringBuilder strb = new StringBuilder();
            strb.Append("M:" + method.ReflectedType.FullName + "." + method.Name + "(");//方法前面加M:
            ParameterInfo[] parameterInfos = method.GetParameters();
            foreach (ParameterInfo pInfo in parameterInfos)
            {
                strb.Append(pInfo.ParameterType + ",");
            }
            strb.Remove(strb.Length - 1, 1);
            strb.Append(")");
            //处理List
            return strb.ToString().Replace("List`1", "List").Replace("[", "{").Replace("]", "}");
        }
        /// <summary>
        /// 获取方法的注释
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetMethodAnnoatation(MethodInfo method)
        {
            XmlNode node = GetMethodNode(method);
            if (node == null)
                return "";
            return node.FirstChild.InnerText.Trim();
        }
        /// <summary>
        /// 获取方法参数的注释(值类型)
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        private string GetMethodParameterAnnoatation(MethodInfo method, string paraName)
        {
            #region 特殊处理 懒得写太多注释
            if (paraName.Equals("token"))
                return "令牌";
            if (paraName.ToLower().Equals("fundid"))
                return "基金ID";
            if (paraName.ToLower().Equals("combinationid"))
                return "组合ID";
            if (paraName.ToLower().Equals("instrumentid"))
                return "证券ID";
            if (paraName.ToLower().Equals("exchangeid"))
                return "交易所ID";
            if (paraName.ToLower().Equals("userid"))
                return "用户ID";
            if (paraName.ToLower().Equals("strategyid"))
                return "策略ID";
            if (paraName.ToLower().Equals("basketid"))
                return "篮子ID";
            if (paraName.ToLower().Equals("batchid"))
                return "指令ID";
            #endregion
            XmlNode node = GetMethodNode(method);
            if (node == null)
                return "";
            XmlNodeList nodeList = node.SelectNodes("param");
            foreach (XmlNode chidNode in nodeList)
            {
                string temp = chidNode.Attributes["name"].Value;
                if (temp.Equals(paraName))
                {
                    return chidNode.InnerText.Trim();
                }
            }
            return "";
        }
        /// <summary>
        /// 找出参数的注释
        /// </summary>
        /// <param name="paraName"></param>
        /// <returns></returns>
        private string GetParameterAnnoation(string paraName)
        {
            XmlNodeList nodeList = _xmlDoc.SelectNodes("/doc/members/member");
            foreach (XmlNode node in nodeList)
            {
                string temp = node.Attributes["name"].Value;
                if (temp.Equals(paraName))
                {
                    return node.FirstChild.InnerText.Trim();
                }
            }
            return "";
        }
        /// <summary>
        /// 获取返回值的注释
        /// </summary>
        /// <param name="method"></param>
        /// <param name="returnName"></param>
        /// <returns></returns>
        private string GetReturnAnnoation(MethodInfo method, string returnName)
        {
            XmlNode node = GetMethodNode(method);
            if (node == null)
                return "";
            XmlNode returnNode = node.SelectSingleNode("returns");
            return returnNode.InnerText.Trim();
        }
        #endregion
    }
}