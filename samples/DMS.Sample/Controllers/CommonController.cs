﻿using DMS.Common;
using DMS.Common.BaseResult;
using DMS.Common.Extensions;
using DMS.Common.Helper;
using DMS.Log4net;
using DMS.NLogs;
using DMS.Redis;
using DMS.Sample.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace DMS.Sample.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public enum EnumMemUserType
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("普通用户")]
        Nornm = 0,
        /// <summary>
        /// 
        /// </summary>
        [Description("QQ用户")]
        QQType = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("dfdfdsfd ")]
        Test = 2
    }
    /// <summary>
    /// /
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pwd { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        /// <summary>
        /// 日志处理
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLog4net")]
        public ActionResult GetLog4net()
        {
            DMS.Log4net.Logger.Info("这是log4net的日志");
            DMS.Log4net.Logger.Error("这是log4net的异常日志");

            DMS.NLogs.Logger.Debug("这是nlog的Debug日志");
            DMS.NLogs.Logger.Info("这是nlog的日志");
            DMS.NLogs.Logger.Error("这是nlog的异常日志");
            return Ok("");
        }

        /// <summary>
        /// 使用前请先注册： services.UseAppConfig(Configuration);
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAppConfig")]
        public ActionResult GetAppConfig()
        {
            string memberApi = DMS.Common.AppConfig.GetVaule<string>("MemberUrl");
            memberApi = DMS.Common.AppConfig.GetVaule("MemberUrl");
            var ip = $"获取IP：{IPHelper.GetWebClientIp()}";
            var dev = DMS.Common.AppConfig.GetVaule("dev");
            var redisOption = DMS.Redis.AppConfig.RedisOption;


            var data = new
            {
                memberApi,
                ip,
                dev,
                redisOption
            };
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRedisTest")]
        public ActionResult GetRedisTest([FromQuery]string msg)
        {
            //DMS.Redis.RedisManager redisManager = new DMS.Redis.RedisManager(3);
            //redisManager.ListRightPush("key123", "right11111");

            #region 发布
            RedisManager redisManager5 = new RedisManager(5);
           // var keys = redisManager5.HashKeys<object>("room1");
            var a = redisManager5.Publish("queue_roomvisirecord", msg);
            #endregion

            return Ok("");
        }


        #region AttributeExtensions实体自定义属性转换为另一实体
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("AttributeExtension")]
        public ActionResult AttributeExtension()
        {
            //if (_rabbitMqAttribute.IsNullOrEmpty())
            //{
            //    var typeOfT = typeof(MessageModel);
            //    _rabbitMqAttribute = typeOfT.GetCustomAttribute<RabbitMqAttribute>();
            //}


            MessageModel model = new MessageModel()
            {
                Msg = "aaaaaaaaaa",
                CreateDateTime = DateTime.Now,
            };
            Publish(model);
            return Content("");
        }

        private void Publish<T>(T command) where T : class
        {
            var queueInfo = GetRabbitMqAttribute<T>();
        }

        private static RabbitMqAttribute _rabbitMqAttribute;
        private static RabbitMqAttribute GetRabbitMqAttribute<T>()
        {
            if (_rabbitMqAttribute.IsNullOrEmpty())
            {
                var typeOfT = typeof(T);
                _rabbitMqAttribute = typeOfT.GetCustomAttribute<RabbitMqAttribute>();
            }
            return _rabbitMqAttribute;
        }
        #endregion

        #region EnumExtensions枚举扩展
        /// <summary>
        /// 枚举扩展
        /// </summary>
        /// <returns></returns>
        [HttpGet("EnumExtensions")]
        public ActionResult EnumExtensions()
        {
            var json = typeof(EnumMemUserType).ToJson();
            var des = EnumMemUserType.QQType.GetDescription();
            des = typeof(EnumMemUserType).GetDescription(2);
            return Ok(des);
        }
        #endregion

        #region SerializerExtensions序列化/反序列化
        /// <summary>
        /// 序列化/反序列化
        /// ConfigureServices=>AddJsonOptions{ options.SerializerSettings.Converters.Add(new CustomStringConverter());}
        /// [ColumnMapping(Name = "ShopMemberID"), JsonConverter(typeof(CustomStringConverter))]
        /// </summary>
        /// <returns></returns>
        [HttpGet("SerializerExtensions")]
        public ActionResult SerializerExtensions()
        {
            User user = new User()
            {
                UserID = 1125964271981826048,
                UserName = "aaaa",
                Pwd = "pwd"
            };
            string serObject = user.SerializeObject();
            User u = serObject.DeserializeObject<User>();

            string s = "aaaaaaaaaaa";
            var ss = s.SerializeObject();
            var bb = ss.DeserializeObject<string>();

            return Content("");
        }
        #endregion

        #region ExportBuilderHelper,ExportHelper导入导出excel
        #region ExportBuilderHelper,ExportHelper数据导出excel
        /// <summary>
        /// excel导出
        /// 请求方式：http://localhost:5000/api/Common/ExportFS
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExportFS")]
        public FileResult ExportFS()
        {
            List<User> userList = new List<User>() {
                 new User(){ UserID=1, Pwd="aaaa", UserName="aaaaa" },
                 new User(){ UserID=2, Pwd="bbbb", UserName="bbbbb"},
            };
            var fs = ExportHelper.ExportToFsExcel<User>(userList, new string[] { "UserID", "UserName" },
               c => c.UserID,
               c => c.UserName.ToString()
               );

            return File(fs, "application/vnd.ms-excel", "ExportFS.xls");
        }

        /// <summary>
        /// excel导出
        /// 请求方式：http://localhost:5000/api/Common/ExportBuilderFS
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExportBuilderFS")]
        public FileResult ExportBuilderFS()
        {
            List<User> userList = new List<User>() {
                  new User(){ UserID=1, Pwd="aaaa", UserName="aaaaa" },
                 new User(){ UserID=2, Pwd="bbbb", UserName="bbbbb"},
            };

            var fs = new ExportBuilderHelper<User>()
                   .Column(c => c.UserID)
                   .Column(c => c.UserName)
                   .Export(userList);

            return File(fs, "application/vnd.ms-excel", "ExportBuilderFS.xls");
        }

        /// <summary>
        /// API导出数据
        /// 请求方式：API接口，地址浏览
        /// </summary>
        /// <returns></returns>
        [HttpGet("Export")]
        public string Export()
        {
            List<User> list = new List<User>();
            list.Add(new User() { UserID = 1, UserName = "lang" });
            list.Add(new User() { UserID = 1, UserName = "aaa" });
            DataTable dt = list.ToDataTable("dd");
            return ExportHelper.ExportToExcel(dt, @"d:\Export.xls");
        }

        /// <summary>
        /// API导出数据
        /// 请求方式：API接口，地址浏览
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExportToExcel")]
        public string ExportToExcel()
        {
            List<User> userList = new List<User>() {
                 new User(){ UserID=1,UserName="aaa" },
                 new User(){ UserID=2,UserName="bbb" },
            };
            var s = ExportHelper.ExportToExcel<User>(userList, @"d:\ExportToExcel.xls",
               new string[] { "用户ID", "用户名称" },
               c => c.UserID,
               c => c.UserName.ToString()
               );

            return s;
        }
        #endregion
        #region 导入excel
        /// <summary>
        /// 根据文件导入
        /// </summary>
        /// <returns></returns>
        [HttpPost("ImportToExcel")]
        public object ImportToExcel()
        {
            string filePath = @"D:\Export.xls";
            DataTable dataTable = ExportHelper.ImportToExcel(filePath);
            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn c in dataTable.Columns)
                {
                    string columnName = c.ColumnName;
                    object value = dr[c];
                }
            }

            return dataTable;
        }

        /// <summary>
        /// 上传文件流导入
        /// </summary>
        /// <returns></returns>
        [HttpPost("ImportToExcelV2")]
        public object ImportToExcelV2(IFormFile file)
        {
            DataTable dataTable = ExportHelper.ImportToExcel(file.OpenReadStream());
            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn c in dataTable.Columns)
                {
                    string columnName = c.ColumnName;
                    object value = dr[c];
                }
            }
            return dataTable;
        }
        #endregion
        #endregion

        #region HttpClientHelper
        /// <summary>
        /// HttpClientHelper=>get请求
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetHttpClientHelper")]
        public ActionResult GetHttpClientHelper()
        {
            string url = "https://productapi.trydou.com/api/product/GetProductEntity";
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { "ProductID", "1123955947824353280" }
            };
            ResponseResult<ProductEntityResult> responseResult = HttpClientHelper.GetResponse<ResponseResult<ProductEntityResult>>(url, dic);
            return Content("");
        }

        /// <summary>
        /// HttpClientHelper=>Post请求
        /// </summary>
        /// <returns></returns>
        [HttpPost("PostHttpClientHelper")]
        public ActionResult PostHttpClientHelper()
        {
            SearchProductParam dict = new SearchProductParam()
            {
                SearchKey = "国元信托",
                AttrParam = new SearchProductAttrParam() { CodeName = "xintuo1" },
            };
            //第一种
            for (int i = 0; i <= 100; i++)
            {
                var result = HttpClientHelper.PostResponse<SearchProductParam>("http://productapi.jinglih.com/api/Product/GetProductList", dict);
                Console.WriteLine(i + "====" + result);
            }


            //第二种
            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            for (int i = 0; i <= 100; i++)
            {
                var result = HttpClientHelper.PostResponse("http://productapi.jinglih.com/api/Product/GetProductList", jsonStr);
                Console.WriteLine(i + "====" + result);
            }
            //第三种
            for (int i = 0; i <= 100; i++)
            {
                var result = HttpClientHelper.PostResponse<ResponseResult>("http://productapi.jinglih.com/api/Product/GetProductList", jsonStr);
                Console.WriteLine(i + "====" + result);
            }
            return Content("");
        }
        #endregion

        #region HttpWebHelper
        /// <summary>
        /// HttpWebHelper=>get请求
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetHttpWebHelper")]
        public ActionResult GetHttpWebHelper()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                { "SearchKey","国元信托"},
                { "AttrParam.CodeName","xintuo"},
                { "AttrParam.ProductStatusType","1"},
            };

            for (int i = 0; i <= 100; i++)
            {
                var result = HttpWebHelper.GetRequest<ResponseResult>("http://productapi.jinglih.com/api/Product/GetProductList", dict);
                Console.WriteLine(i + "====" + result);
            }
            return Content("");
        }

        /// <summary>
        /// HttpWebHelper=>Post请求
        /// </summary>
        /// <returns></returns>
        [HttpPost("PostHttpWebHelper")]
        public ActionResult PostHttpWebHelper()
        {
            SearchProductParam dict = new SearchProductParam()
            {
                SearchKey = "国元信托1",
                AttrParam = new SearchProductAttrParam() { CodeName = "xintuo", ProductStatusType = 1 },
            };
            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(dict);

            for (int i = 0; i <= 100; i++)
            {
                var result = HttpWebHelper.PostRequest<ResponseResult>("http://productapi.jinglih.com/api/Product/GetProductList", jsonStr);
                Console.WriteLine(i + "====" + result);
            }
            return Content("");
        }
        #endregion
    }
}