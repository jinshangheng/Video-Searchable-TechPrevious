using System;
using System.Collections.Generic;
using System.IO;

namespace VSTP.Subtitles
{
    public class RequestApi
    {
        private static string appid;
        private static string sceretKey;
        private static string uploadFilePath;

        private const string prepare = "/prepare";
        private const string upload = "/upload";
        private const string merge = "/merge";
        private const string getResult = "/getResult";
        private const string getProgress = "/getProgress";

        private const string apiHost = "http://raasr.xfyun.cn/api";

        private static int sliceSize = 10485760;
        public RequestApi(string id, string sk, string ufp)
        {
            appid = id;
            sceretKey = sk;
            uploadFilePath = ufp;
        }

        public RequestApi(string id, string sk, string ufp, int size)
        {
            appid = id;
            sceretKey = sk;
            uploadFilePath = ufp;
            sliceSize = size;
        }


        private static Dictionary<string, string> GetBaseAuthParam(string taskId)
        {
            var baseParam = new Dictionary<string, string>();
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string cur = Convert.ToInt64(ts.TotalSeconds).ToString();
            string md5 = EncryptHelper.MD5(appid + cur).Replace("-", "").ToLower();
            string signa = EncryptHelper.HMACSHA1Base64(md5, sceretKey);
            baseParam.Add("app_id", appid);
            baseParam.Add("ts", cur);
            baseParam.Add("signa", signa);
            if(!string.IsNullOrEmpty(taskId))
            {
                baseParam.Add("task_id", taskId);
            }

            return baseParam;
        }

        private static string UrlPara(Dictionary<string, string> prepareParam)
        {
            var result = "";

            foreach (var item in prepareParam)
            {
                result += $"{item.Key}={item.Value}&";
            }

            result = result.TrimEnd('&');

            return result;
        }

        public static XunFeiResult Prepare(FileInfo fileInfo)
        {
            var prepareParam = GetBaseAuthParam(null);
            try
            {
                if (fileInfo != null && fileInfo.Exists)
                {
                    prepareParam.Add("file_len", fileInfo.Length + "");
                    prepareParam.Add("file_name", fileInfo.Name);
                    prepareParam.Add("slice_num", (fileInfo.Length / sliceSize) + (fileInfo.Length % sliceSize == 0 ? 0 : 1) + "");
                }
                else
                {
                    Console.WriteLine("指定的文件路径不正确!"); //我们自己的报错台
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // 其他处理异常的代码
            }
            var url = apiHost + prepare + "?" + UrlPara(prepareParam);
            string response = HttpHelper.HttpPost(url, null);

            var result = new JSONHelper().Deserialize<XunFeiResult>(prepare);
            Console.WriteLine($"预处理结果：{response}");
            return result;
        }
        public static XunFeiResult UploadSlice(string taskId, string sliceId, byte[] slice, string fileName)
        {
            Dictionary<string, string> uploadParam = GetBaseAuthParam(taskId);
            uploadParam.Add("slice_id", sliceId);

            var url = apiHost + upload + "?" + UrlPara(uploadParam);
            string response = HttpHelper.HttpPostMulti(url, uploadParam, slice, fileName);
            if (response == null)
            {
                throw new Exception("分片上传接口请求失败！");
            }
            var result = new JSONHelper().Deserialize<XunFeiResult>(response);
            Console.WriteLine($"上传结果：{response}");

            return result;
        }
        public static bool Merge(string taskId)
        {
            Dictionary<string, string> mergeParam = GetBaseAuthParam(taskId);
            var url = apiHost + merge + "?" + UrlPara(mergeParam);
            string response = HttpHelper.HttpPost(url, null);
            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("文件合并接口请求失败！");
                return false;
            }

            var result = new JSONHelper().Deserialize<XunFeiResult>(response);

            if (result.Ok == 0)
            {
                Console.WriteLine("文件合并成功, taskId: " + taskId);
                return true;
            }
            Console.WriteLine("文件合并失败！" + response);
            return false;
        }
        public static XunFeiResult GetProgress(string taskId)
        {
            Dictionary<string, string> progressParam = GetBaseAuthParam(taskId);
            var url = apiHost + getProgress + "?" + UrlPara(progressParam);
            string response = HttpHelper.HttpPost(url, null);

            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("获取任务进度接口请求失败！");
            }
            var result = new JSONHelper().Deserialize<XunFeiResult>(response);

            return result;
        }
        public static XunFeiResult GetResult(string taskId)
        {
            Dictionary<string, string> resultParam = GetBaseAuthParam(taskId);
            var url = apiHost + getResult + "?" + UrlPara(resultParam);
            string response = HttpHelper.HttpPost(url, null);
            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("获取任务进度接口请求失败！");
            }
            var result = new JSONHelper().Deserialize<XunFeiResult>(response);

            return result;
        }
    }
}

//part copy from:https://blog.csdn.net/qq_23873839/article/details/84774396
