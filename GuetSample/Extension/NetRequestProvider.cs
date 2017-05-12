using SitesModel.Request;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GuetSample.Extension
{
    public class NetRequestProvider
    {

        #region -------- public ----------

        public static Task<ResponseData<string>> HttpGet(HttpRequestConfig config, string url, string data, CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                string errorMsg = string.Empty;
                //check url
                if (!isRightUrl(url, ref errorMsg))
                    return ResponseData<string>.FromError(new ArgumentException("Wrong url", "url"));
                //Collect old if not clear
                //init request parameters
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                setAgentAndContentTypeFor(ref request, config);
                adaptHandlerSettingFor(ref request, config);
                //part02
                ResponseData<string> ResponseData = null;
                StringBuilder resultBuilder = new StringBuilder();
                HttpWebResponse response = null;
                Stream responseStream = null;
                StreamReader streamReader = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = config.HoleCookieContainer.GetCookies(response.ResponseUri);
                    //Hand responseStream
                    responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        resultBuilder.Append(GZipDecompress(responseStream, config.Encoding));
                    }
                    else
                    {
                        streamReader = new StreamReader(responseStream, config.Encoding);
                        resultBuilder.Append(streamReader.ReadToEnd());
                    }
                    ResponseData = ResponseData<string>.FromData(resultBuilder.ToString());
                }
                catch (Exception e)
                {
                    ResponseData = ResponseData<string>.FromError(e);
                    throw;
                }
                finally
                {
                    if (response != null)
                        response.Dispose();
                    if (responseStream != null)
                        responseStream.Dispose();
                    if (streamReader != null)
                        streamReader.Dispose();
                    if (request != null)
                        request.Abort();
                    request = null;
                }
                return ResponseData;
            }, token);
        }

        public static Task<ResponseData<byte[]>> HttpGet(HttpRequestConfig config, string url, CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                string errorMsg = string.Empty;
                if (!isRightUrl(url, ref errorMsg))
                {
                    return ResponseData<byte[]>.FromError(new ArgumentException("Wrong url", "url"));
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //Sp
                request.Method = "GET";
                setAgentAndContentTypeFor(ref request, config);
                adaptHandlerSettingFor(ref request, config);
                request.ContentType = "text/html, application/xhtml+xml, image/jxr, */*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";

                ResponseData<byte[]> ResponseData = null;
                HttpWebResponse httpResponse = null;
                Stream responseStream = null;
                MemoryStream ms = new MemoryStream();
                try
                {
                    httpResponse = (HttpWebResponse)request.GetResponse();
                    httpResponse.Cookies = config.HoleCookieContainer.GetCookies(httpResponse.ResponseUri);
                    //Hand responseStream
                    responseStream = httpResponse.GetResponseStream();
                    byte[] buff = new byte[512];
                    int c = 0;
                    while ((c = responseStream.Read(buff, 0, buff.Length)) > 0)
                    {
                        ms.Write(buff, 0, c);
                    }
                    byte[] bytes = null;
                    bytes = ms.ToArray();
                    ResponseData = ResponseData<byte[]>.FromData(bytes);
                }
                catch (Exception e)
                {
                    ResponseData = ResponseData<byte[]>.FromError(e);
                    throw;
                }
                finally
                {
                    if (httpResponse != null)
                        httpResponse.Dispose();
                    if (request != null)
                        request.Abort();
                    if (responseStream != null)
                        responseStream.Dispose();
                    if (ms != null)
                        ms.Dispose();
                }
                return ResponseData;
            }, token);
        }

        public static Task<ResponseData<string>> HttpPost(HttpRequestConfig config, string url, string data, CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                string errorMsg = string.Empty;
                if (!isRightUrl(url, ref errorMsg))
                    return ResponseData<string>.FromError(new ArgumentException("Wrong url", "url"));
                //initialize httpwebrequest
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = config.Encoding.GetByteCount(data);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
                adaptHandlerSettingFor(ref request, config);
                //initialize used var
                ResponseData<string> ResponseData = null;
                byte[] bytes = config.Encoding.GetBytes(data);
                StringBuilder resultBuilder = new StringBuilder();
                HttpWebResponse response = null;
                Stream responseStream = null;
                StreamReader streamReader = null;
                Stream requestStream = null;
                try
                {
                    requestStream = request.GetRequestStream();
                    //StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("GBK"));
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();

                    response = (HttpWebResponse)request.GetResponse();
                    //response.Cookies = cookie.GetCookies(response.ResponseUri);

                    responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        resultBuilder.Append(GZipDecompress(responseStream, config.Encoding));
                    }
                    else
                    {
                        streamReader = new StreamReader(responseStream, config.Encoding);
                        resultBuilder.Append(streamReader.ReadToEnd());
                    }
                    ResponseData = ResponseData<string>.FromData(resultBuilder.ToString());
                }
                catch (Exception e)
                {
                    ResponseData = ResponseData<string>.FromError(e);
                    throw;
                }
                //Dispose resource finally
                finally
                {
                    if (response != null)
                        response.Dispose();
                    if (responseStream != null)
                        responseStream.Dispose();
                    if (streamReader != null)
                        streamReader.Dispose();
                    if (requestStream != null)
                        requestStream.Dispose();
                    if (request != null)
                        request.Abort();
                    request = null;
                }
                //Todo return nullString if retstrb count is zero
                return ResponseData;
            }, token);
        }

        #endregion

        #region --------- private ---------------

        private static bool isRightUrl(string url, ref string errorMsg)
        {
            if (string.IsNullOrEmpty(url))
            {
                errorMsg = "Empty uri";
            }
            else if (!Regex.IsMatch(url, @"[a-zA-z]+:\/\/[^\s]*"))
            {
                errorMsg = "Wrong uri";
            }
            else return true;
            return false;
        }

        private static void setAgentAndContentTypeFor(ref HttpWebRequest request, HttpRequestConfig config)
        {
            var charset = config.Encoding.WebName;
            if (charset.ToLower() == "gb2312") charset = "gbk";
            request.ContentType = "text/html;charset=" + charset;
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
        }

        private static void adaptHandlerSettingFor(ref HttpWebRequest request, HttpRequestConfig config)
        {
            request.CookieContainer = config.HoleCookieContainer;
            request.Timeout = config.RequestTimeOut;
            request.ReadWriteTimeout = config.ReadWriteTimeOut;
            request.KeepAlive = config.KeepAlive;
        }

        /// <summary>
        /// Decompress gzip stream
        /// </summary>
        /// <param name="stream">Gzip stream</param>
        /// <param name="resultBuilder">result string builder</param>
        private static string GZipDecompress(Stream stream, Encoding encoding)
        {
            string result = string.Empty;
            GZipStream gZipStream = null;
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                gZipStream = new GZipStream(stream, CompressionMode.Decompress);

                memoryStream = new MemoryStream();
                byte[] bytes = new byte[1024];
                int len = 0;
                while ((len = gZipStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    memoryStream.Write(bytes, 0, len);
                    //resultstr.Append(encoding.GetString(bytes));
                }
                memoryStream.Seek(0, SeekOrigin.Begin);

                streamReader = new StreamReader(memoryStream, encoding);
                result = streamReader.ReadToEnd();
                memoryStream.Dispose();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (gZipStream != null) gZipStream.Dispose();
                if (streamReader != null) streamReader.Dispose();
                if (memoryStream != null) memoryStream.Dispose();
            }
            return result;
        }

        #endregion
    }

}
