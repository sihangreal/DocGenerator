﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocGenerator
{
    public class Parameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParamDescribe { get; set; }
    }
}