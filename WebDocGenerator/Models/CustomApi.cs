using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebDocGenerator.Models
{
    /// <summary>
    /// API 接口
    /// </summary>
    public class CustomApi
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 接口地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        public string RequestType { get; set; }

        /// <summary>
        /// 输入参数
        /// </summary>
        public List<Parameter> InParameterList { get; set; }

        /// <summary>
        /// 输出参数
        /// </summary>
        public List<Parameter> OutParameterList { get; set; }

        /// <summary>
        /// Post参数
        /// </summary>
        public List<Parameter> PostParameterList { get; set; }

        /// <summary>
        /// Put参数
        /// </summary>
        public List<Parameter> PutParameterList { get; set; }
    }
}