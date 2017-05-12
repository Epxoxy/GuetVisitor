using SitesModel.ModelBase;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SitesModel.Request
{
    public interface IRequestMonitor
    {
        int Process { get; set; }
        bool IsTaskEmpty { get; set; }
        bool IsRequesting { get; set; }
        bool IsTaskWaiting { get; set; }
        void OnProcessUpdated();
        void OnTaskCanceled();
        RequestReceipt OnRetryRequest(string msg);
        void OnFaulted(Exception e);
    }

    public class HttpRequestConfig
    {
        public Encoding Encoding { get; set; }
        public int RequestTimeOut { get; set; }
        public int ReadWriteTimeOut { get; set; }
        public bool KeepAlive { get; set; }
        private CookieContainer holeCookieContainer;
        public CookieContainer HoleCookieContainer
        {
            get
            {
                if (holeCookieContainer == null)
                    holeCookieContainer = new CookieContainer();
                return holeCookieContainer;
            }
            set
            {
                holeCookieContainer = value;
            }
        }

        public HttpRequestConfig(CookieContainer cookieContainer, Encoding encoding, int requestTimeOut, int readWriteTimeOut, bool keepAlive)
        {
            this.HoleCookieContainer = cookieContainer;
            this.Encoding = encoding;
            this.RequestTimeOut = requestTimeOut;
            this.ReadWriteTimeOut = readWriteTimeOut;
            this.KeepAlive = keepAlive;
        }
        public HttpRequestConfig(Encoding encoding, int requestTimeOut, int readWriteTimeOut, bool keepAlive)
            : this(null, encoding, requestTimeOut, readWriteTimeOut, keepAlive)
        {
        }
        public HttpRequestConfig(Encoding encoding, int requestTimeOut, int readWriteTimeOut)
            : this(encoding, requestTimeOut, readWriteTimeOut, false)
        {
        }
        public HttpRequestConfig(Encoding encoding) : this(encoding, 10000, 4000)
        {
        }
        public HttpRequestConfig() : this(Encoding.UTF8)
        {
        }
    }

    public enum RequestReceipt
    {
        Dismiss,
        Cancel,
        OK
    }

    public class RequestMonitor : IRequestMonitor
    {
        public int Process { get; set; }
        public bool IsTaskEmpty { get; set; }
        public bool IsRequesting { get; set; }
        public bool IsTaskWaiting { get; set; }
        public void OnProcessUpdated() { }
        public void OnTaskCanceled() { }
        public void OnRequested(string msg) { }
        public void OnFaulted(Exception e) { }
        public RequestReceipt OnRetryRequest(string msg) { return RequestReceipt.OK; }
    }

    public delegate Task<ResponseData<string>> HttpRequestTask(HttpRequestConfig config, string url, string data, CancellationToken token);

    public delegate Task<HandResult> HandWebDataTask(WebPageModel page, PassingData passing, ResponseData<string> data);

    public abstract class RequestWrapper : IDisposable
    {
        #region ------ Properties --------

        private CancellationTokenSource cacheCTS;
        private HttpRequestConfig httpConfig;

        private int wrongRetryTimes = 2;
        public int WrongReceiveRetryTimes
        {
            get
            {
                return wrongRetryTimes;
            }
            set
            {
                if (wrongRetryTimes != value && wrongRetryTimes >= 0)
                {
                    wrongRetryTimes = value;
                }
            }
        }

        private int webErrorRetryTimes = 2;
        public int WebErrorRetryTimes
        {
            get
            {
                return webErrorRetryTimes;
            }
            set
            {
                if (webErrorRetryTimes != value && webErrorRetryTimes >= 0)
                {
                    webErrorRetryTimes = value;
                }
            }
        }

        #endregion


        #region ------ Constructor -----------

        public RequestWrapper()
        {
            this.httpConfig = new HttpRequestConfig();
        }

        public RequestWrapper(HttpRequestConfig httpConfig)
        {
            this.httpConfig = httpConfig;
        }

        #endregion


        #region ------ Request wrapping -----------

        public void formatRequest(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, object[] addition, params string[] datas)
        {
            formatRequest(monitor, page, handTask, addition, RepeatInfo.Once, datas);
        }

        public void formatRequest(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, object[] addition, RepeatInfo repeat, params string[] datas)
        {
            string data = string.Empty;
            data = string.Format(page.PostDataFormat, datas);
            request(monitor, page, handTask, repeat, new PassingData() { Data = data, Addition = addition });
        }

        public void request(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, string passingData = null)
        {
            request(monitor, page, handTask, new PassingData() { Data = passingData });
        }

        public void request(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, PassingData data)
        {
            request(monitor, page, handTask, RepeatInfo.Once, data);
        }

        public void request(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, RepeatInfo repeat, string passingData = null)
        {
            request(monitor, page, handTask, repeat, new PassingData() { Data = passingData });
        }

        public void request(IRequestMonitor monitor, WebPageModel page, HandWebDataTask handTask, RepeatInfo repeatInfo, PassingData data)
        {
            if (page == null) return;
            //Cancel Previous
            ensureCancelCTS();
            cacheCTS = new CancellationTokenSource();
            var token = cacheCTS.Token;
            //Create Monitor
            if (monitor == null) monitor = new RequestMonitor();
            //Request
            var httpRequest = provideRequestTask(page.IsPost);
            var requestInfo = new HttpRequestInfo()
            {
                RequestTask = httpRequest,
                HttpRequestConfig = httpConfig,
                RequestMonitor = monitor,
                HandDataTask = handTask,
                Page = page,
                Passing = data,
                RepeatInfo = repeatInfo,
                Token = token
            };
            monitor.IsTaskEmpty = false;
            monitor.IsTaskWaiting = false;
            Task.Run(() => loopingRequest(requestInfo).ContinueWith(task =>
            {
                ensureCancelCTS();
                monitor.IsTaskWaiting = false;
                monitor.IsTaskEmpty = true;
                monitor.IsRequesting = false;
                if (task.IsCanceled)
                {
                    monitor.OnTaskCanceled();
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception.GetBaseException() != null)
                    {
                        monitor.OnFaulted(task.Exception.GetBaseException());
                    }
                }
            }));
        }

        #endregion


        #region ------ Request Task -----------

        public async Task loopingRequest(HttpRequestInfo info)
        {
            var repeatInfo = info.RepeatInfo;
            var monitor = info.RequestMonitor;
            int repeatTimes = repeatInfo.RepeatTimes;
            int remainTimes = repeatTimes + 1;
            int delayTime = repeatInfo.DelayTime;
            do
            {
                monitor.IsTaskWaiting = false;
                await Task.Run(() => basicRequest(info));

                --remainTimes;
                repeatInfo.RemainTimes = remainTimes;

                monitor.IsTaskWaiting = true;
                await Task.Delay(delayTime, info.Token);

            } while (repeatTimes > 0);
        }

        public async Task basicRequest(HttpRequestInfo info)
        {
            //Jump out for request cancel
            info.Token.ThrowIfCancellationRequested();
            //Monitor
            //Basic data
            WebPageModel page = info.Page;
            ResponseData<string> received = null;
            var monitor = info.RequestMonitor;
            bool requestResend;
            int wrongDataRetryTimes = WrongReceiveRetryTimes;
            int webErrorRetryTimes = WebErrorRetryTimes;
            do
            {
                //Init
                requestResend = false;
                //Requesting
                monitor.IsRequesting = true;
                try
                {
                    received = await info.RequestTask(info.HttpRequestConfig, page.Url, info.Passing.Data, info.Token);
                }
                catch (AggregateException e)
                {
                    throw e;
                }
                catch (WebException e)
                {
                    if (webErrorRetryTimes > 0)
                    {
                        if (e.Response == null) throw;
                        HttpWebResponse response = null;
                        if ((response = (e.Response as HttpWebResponse)) != null)
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                case HttpStatusCode.InternalServerError:
                                case HttpStatusCode.Forbidden:
                                    throw;
                                default: break;
                            }
                            response.Dispose();
                        }
                        --wrongRetryTimes;
                        requestResend = true;
                        if (monitor.OnRetryRequest(e.Message) == RequestReceipt.Cancel) break;
                    }
                }
                finally
                {
                    monitor.IsRequesting = false;
                }

                //Jump out for request cancel
                info.Token.ThrowIfCancellationRequested();

                //Hand result
                if (!requestResend)
                {
                    if (string.IsNullOrEmpty(received.Data))
                    {
                        if (wrongDataRetryTimes > 0)
                        {
                            if (monitor.OnRetryRequest(received.ErrorMessage) == RequestReceipt.Cancel) break;
                            --wrongDataRetryTimes;
                            requestResend = true;
                        }
                    }
                    else
                    {
                        HandResult handResult = await info.HandDataTask(page, info.Passing, received);
                        if (handResult == HandResult.RequestResend)
                            requestResend = true;
                    }
                }
            } while (requestResend);
        }

        public async Task<ResponseData<string>> branchRequest(IRequestMonitor monitor, HttpRequestTask httpRequestTask, string url, string data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ResponseData<string> responsedata = null;
            //Await run try
            if (monitor != null) monitor.IsRequesting = false;
            try
            {
                responsedata = await httpRequestTask(this.httpConfig, url, data, cts.Token);
            }
            catch (AggregateException e)
            {
                throw e;
            }
            finally
            {
                cts.Dispose();
                cts = null;
                if (monitor != null)
                    monitor.IsRequesting = false;
            }
            return responsedata;
        }

        public async Task<ResponseData<string>> branchRequest(HttpRequestTask request, string url, string data, CancellationToken token)
        {
            return await request(this.httpConfig, url, data, token);
        }

        protected abstract HttpRequestTask provideRequestTask(bool isPost);

        #endregion


        private void ensureCancelCTS()
        {
            if (cacheCTS != null && !cacheCTS.IsCancellationRequested && cacheCTS.Token.CanBeCanceled)
            {
                cacheCTS.Cancel();
                cacheCTS.Dispose();
                cacheCTS = null;
            }
        }

        public void cancel()
        {
            ensureCancelCTS();
        }

        public void Dispose()
        {
            ensureCancelCTS();
        }
        
        public class HttpRequestInfo
        {
            public HttpRequestTask RequestTask { get; set; }
            public HttpRequestConfig HttpRequestConfig { get; set; }
            public HandWebDataTask HandDataTask { get; set; }
            public WebPageModel Page { get; set; }
            public PassingData Passing { get; set; }
            public RepeatInfo RepeatInfo { get; set; }
            public CancellationToken Token { get; set; }
            public IRequestMonitor RequestMonitor { get; set; }
        }
    }

    public class RepeatInfo
    {
        public int repeatTimes;
        public int RepeatTimes
        {
            get { return repeatTimes; }
            set
            {
                if (repeatTimes != value)
                {
                    repeatTimes = value;
                }
            }
        }

        public int remainTimes;
        public int RemainTimes
        {
            get { return remainTimes; }
            set
            {
                if (remainTimes != value)
                {
                    remainTimes = value;
                }
            }
        }

        public int delayTime;
        public int DelayTime
        {
            get { return delayTime; }
            set
            {
                if (delayTime != value)
                {
                    delayTime = value;
                }
            }
        }

        public int minDelayTime;
        public int MinDelayTime
        {
            get { return minDelayTime; }
            set
            {
                if (minDelayTime != value)
                {
                    minDelayTime = value;
                }
            }
        }

        private RepeatInfo() { }
        public RepeatInfo(int repeatTimes, int delayTime, int minDelayTime)
        {
            this.RepeatTimes = repeatTimes;
            this.DelayTime = delayTime;
            this.MinDelayTime = minDelayTime;
            this.RemainTimes = repeatTimes;
        }

        public static RepeatInfo NewOnce()
        {
            return new RepeatInfo()
            {
                MinDelayTime = 2000
            };
        }

        public readonly static RepeatInfo Once = new RepeatInfo()
        {
            MinDelayTime = 2000
        };
    }
}
