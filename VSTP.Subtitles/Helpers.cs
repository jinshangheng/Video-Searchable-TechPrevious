using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VSTP.Subtitles
{
    public static class EncryptHelper
    {
        public static string MD5Encrypt(string e)
        {
            string ret;
            try
            {
                e = "#*f-`" + e + "6^)!mL";
                var md5 = new MD5CryptoServiceProvider();
                ret = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(e))).Replace("-", "");
                md5.Clear();
            }
            catch(Exception)
            {
                throw;
            }
            return ret;
        }

        public static string MD5(string e)
        {
            string ret;
            try
            {
                var md5 = new MD5CryptoServiceProvider();
                ret = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(e))).Replace("-", "");
                md5.Clear();
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }

        public static string HMACSHA1Base64(string value, string keyStr)
        {
            var encode = Encoding.UTF8;
            var byteData = encode.GetBytes(value);
            var byteKey = encode.GetBytes(keyStr);
            var hmac = new HMACSHA1(byteKey);
            var cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);
            cs.Write(byteData, 0, byteData.Length);
            cs.Close();
            return Convert.ToBase64String(hmac.Hash);
        }
    }

    public static class HttpHelper
    {
        public static string HttpPost(string url, string postData, Dictionary<string, string> headers = null, string contentType = null, int timeout = 60, Encoding encoding = null)
        {
            HttpClient client = null;
            try
            {
                if (url.StartsWith("https"))
                {
                    var handler = new HttpClientHandler();
                    X509Certificate2 cerCaiShang = new X509Certificate2(ApiConfig.CertFile, ApiConfig.CertPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                    //证书和证书的密码，目前是缺失的，待解决
                    handler.ClientCertificates.Add(cerCaiShang);
                    handler.PreAuthenticate = true;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };
                    client = new HttpClient(handler);
                }
                else
                {
                    client = new HttpClient();
                }

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                if (timeout > 0)
                {
                    client.Timeout = new TimeSpan(0, 0, timeout);
                }
                using (HttpContent content = new StringContent(postData ?? "", encoding ?? Encoding.UTF8))
                {
                    if (contentType != null)
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    }
                    using (HttpResponseMessage responseMessage = client.PostAsync(url, content).Result)
                    {
                        var result = responseMessage.Content.ReadAsStringAsync().Result;

                        //Log4netUtil.Info("返回结果：" + result);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                client.Dispose();
            }
        }

        public static string HttpPostMulti(string url, Dictionary<string, string> postData, byte[] body, string fileName, Dictionary<string, string> headers = null, string contentType = null, int timeout = 60, Encoding encoding = null)
        {
            string result = string.Empty;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream requestStream = null;
            Stream responseStream = null;

            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = -1;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                //对发送的数据不使用缓存【重要、关键】
                request.AllowWriteStreamBuffering = false;
                request.SendChunked = true;//支持分块上传
                string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
                byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                //请求头部信息 
                StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"content\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

                //request.AddRange(body.Length);
                requestStream = request.GetRequestStream();

                requestStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                requestStream.Write(body, 0, body.Length);

                //发送其他参数

                //string Enter = "\r\n";
                //foreach (var item in postData)
                //{
                //    StringBuilder sbBody = new StringBuilder($"Content-Disposition: form-data; name=\"{item.Key}\"" + Enter + Enter
                //    + item.Value + Enter);
                //    byte[] postBodyBytes = Encoding.UTF8.GetBytes(sbBody.ToString());
                //    requestStream.Write(postBodyBytes, 0, postBodyBytes.Length);
                //}

                requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                result = streamReader.ReadToEnd();//返回信息
                streamReader.Close();
                Dispose(null, request, response, requestStream, responseStream);
            }
            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                Dispose(null, request, response, requestStream, responseStream);
            }

            return result;
        }

        private static void Dispose(object p, HttpWebRequest request, HttpWebResponse response, Stream requestStream, Stream responseStream)
        {
            throw new NotImplementedException();
        }
    }

    public class JSONHelper
    {
        public JSONHelper()
        {

        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }

        public string SerializeByConverter(object obj, params JsonConverter[] converters)
        {
            return JsonConvert.SerializeObject(obj, converters);
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }

        public T DeserializeByConverter<T>(string input, params JsonConverter[] converter)
        {
            return JsonConvert.DeserializeObject<T>(input, converter);
        }

        public T DeserializeBySetting<T>(string input, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(input, settings);
        }

        private object NullToEmpty(object obj)
        {
            return null;
        }
    }
}
