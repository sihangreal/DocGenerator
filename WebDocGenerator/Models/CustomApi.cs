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
    [DataContract]
    public class CustomApi
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 接口地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        /// 接口描述
        /// </summary>
        [DataMember]
        public string Describe { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        [DataMember]
        public string RequestType { get; set; }

        /// <summary>
        /// 输入参数
        /// </summary>
        [DataMember]
        public List<Parameter> InParameterList { get; set; }

        /// <summary>
        /// 输出参数
        /// </summary>
        [DataMember]
        public List<Parameter> OutParameterList { get; set; }

        /// <summary>
        /// Post参数
        /// </summary>
        [DataMember]
        public List<Parameter> PostParameterList { get; set; }

        /// <summary>
        /// Put参数
        /// </summary>
        [DataMember]
        public List<Parameter> PutParameterList { get; set; }
    }
}