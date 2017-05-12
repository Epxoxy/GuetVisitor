using MessengerLight;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SitesModel.Request;
using SitesModel.Helpers;
using GuetSample.Extension;
using SitesModel.ModelBase;

namespace GuetSample.ViewModel
{
    public class CustomRequestViewModel : NotificationObjectEx, IRequestMonitor
    {
        private const string Token = "Custom";
        private const int NotifyNoneValue = 0;
        private const int NotifyOnMatchValue = 1;
        private const int NotifyNotMatchValue = 2;

        private RepeatInfo _csRepeatInfo = RepeatInfo.NewOnce();
        public RepeatInfo CustomRepeatInfo
        {
            get
            {
                return _csRepeatInfo;
            }
            private set
            {
                _csRepeatInfo = value;
                RaisePropertyChanged(nameof(CustomRepeatInfo));
            }
        }

        private bool _customRepeatEnable;
        public bool CustomRepeatEnable
        {
            get
            {
                return _customRepeatEnable;
            }
            set
            {
                _customRepeatEnable = value;
                RaisePropertyChanged(nameof(CustomRepeatEnable));
            }
        }

        private string _customPattern;
        public string CustomPattern
        {
            get
            {
                return _customPattern;
            }
            set
            {
                _customPattern = value;
                RaisePropertyChanged(nameof(CustomPattern));
            }
        }

        private string _customUrl = "http://bkjw2.guet.edu.cn/student/select.asp";
        public string CustomUrl
        {
            get
            {
                return _customUrl;
            }
            set
            {
                _customUrl = value;
                RaisePropertyChanged(nameof(CustomUrl));
            }
        }

        private string _customData = "spno=000005&selecttype=%D6%D8%D0%DE&testtime=&course={0}&textbook{0}=0&lwBtnselect=%CC%E1%BD%BB";
        public string CustomData
        {
            get
            {
                return _customData;
            }
            set
            {
                _customData = value;
                RaisePropertyChanged(nameof(CustomData));
            }
        }

        private string _customEncoding = "gb2312";
        public string CustomEncoding
        {
            get
            {
                return _customEncoding;
            }
            set
            {
                if(_customEncoding != value)
                {
                    _customEncoding = value;
                    httpRequestConfig.Encoding = Encoding.GetEncoding(value);
                    RaisePropertyChanged(nameof(CustomEncoding));
                }
            }
        }

        private bool _customTypeOfPost;
        public bool CustomTypeOfPost
        {
            get
            {
                return _customTypeOfPost;
            }
            set
            {
                _customTypeOfPost = value;
                RaisePropertyChanged(nameof(CustomTypeOfPost));
            }
        }
        
        private int _notifySettingValue;
        public int NotifySettingValue
        {
            get
            {
                return _notifySettingValue;
            }
            set
            {
                _notifySettingValue = value;
                RaisePropertyChanged(nameof(NotifySettingValue));
            }
        }

        private OptionalItem<int>[] _notifySelection;
        public OptionalItem<int>[] NotifySelection
        {
            get
            {
                if(_notifySelection == null)
                {
                    _notifySelection = new OptionalItem<int>[]
                    {
                        new OptionalItem<int>("None", NotifyNoneValue),
                        new OptionalItem<int>("OnMatch", NotifyOnMatchValue),
                        new OptionalItem<int>("NotMatch", NotifyNotMatchValue)
                    };
                }
                return _notifySelection;
            }
            set
            {
                _notifySelection = value;
                RaisePropertyChanged(nameof(NotifySelection));
            }
        }

        private string _htmlValue;
        public string HtmlValue
        {
            get
            {
                return _htmlValue;
            }
            set
            {
                _htmlValue = value;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RaisePropertyChanged(nameof(HtmlValue));
                });
            }
        }
        
        private bool _isRequesting;
        public bool IsRequesting
        {
            get
            {
                return _isRequesting;
            }
            set
            {
                _isRequesting = value;
                RaisePropertyChanged(nameof(IsRequesting));
            }
        }

        private bool _isTaskEmpty = true;
        public bool IsTaskEmpty
        {
            get
            {
                return _isTaskEmpty;
            }
            set
            {
                if(_isTaskEmpty != value)
                {
                    _isTaskEmpty = value;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RaisePropertyChanged(nameof(IsTaskEmpty));
                        AbortCommand.RaiseCanExecuteChanged();
                        SendCustomDataCommand.RaiseCanExecuteChanged();
                    }));
                }
            }
        }

        public DelegateCommand AbortCommand { get; set; }
        public DelegateCommand SendCustomDataCommand { get; set; }

        public int Process { get; set; }
        public bool IsTaskWaiting { get; set; }

        private HttpRequestConfig httpRequestConfig;
        private Requester requester;
        public CustomRequestViewModel()
        {
            //HttpRequestConfig
            httpRequestConfig = new HttpRequestConfig(Encoding.GetEncoding(CustomEncoding));
            httpRequestConfig.HoleCookieContainer = HttpControlCenter.CommonCookieContainer;
            //AsyncHttpRequestProvider
            requester = new Requester(httpRequestConfig);
        }
        
        protected override void OnInitCommands()
        {
            SendCustomDataCommand = new DelegateCommand();
            AbortCommand = new DelegateCommand();
            SendCustomDataCommand.CanExecuteCommand += o => IsTaskEmpty;
            AbortCommand.CanExecuteCommand += o => !IsTaskEmpty;
            SendCustomDataCommand.ExecuteCommand += o =>
            {
                WebPageModel pageTemp = null;
                try
                {
                    pageTemp = new WebPageModel("subTask", CustomUrl, CustomTypeOfPost, httpRequestConfig.Encoding.EncodingName, null, null, null);
                }
                catch (Exception ex)
                {
                    return;
                }
                if (CustomRepeatEnable)
                {
                    requester.request(this, pageTemp, customMatchHtmlTask, CustomRepeatInfo, CustomData);
                }
                else
                {
                    requester.request(this, pageTemp, customMatchHtmlTask, CustomData);
                }
            };
            AbortCommand.ExecuteCommand += o =>
            {
                requester.cancel();
            };
        }

        private Task<HandResult> customMatchHtmlTask(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                string webData = res.Data;
                if (string.IsNullOrEmpty(webData)) return HandResult.RequestResend;
                if (!string.IsNullOrEmpty(CustomPattern))
                {
                    if (NotifySettingValue == NotifyNoneValue) return HandResult.HandComplete;

                    MatchCollection matches = webData.clearHTMLHeadBody().Matches(CustomPattern);
                    if(matches.Count > 0 && NotifySettingValue == NotifyOnMatchValue)
                    {
                        Messenger.Default.Send(new DialogContent()
                        {
                            Title = "Notify",
                            Content = "New web data match pattern"
                        }, Token);
                    }
                    else if (matches.Count <= 0 && NotifySettingValue == NotifyNotMatchValue)
                    {
                        Messenger.Default.Send(new DialogContent()
                        {
                            Title = "Notify",
                            Content = "New web data not match pattern"
                        }, Token);
                    }
                }
                this.HtmlValue = webData;
                return HandResult.HandComplete;
            });
        }

        public void OnProcessUpdated()
        {
            XLog.LogLine("ProcessUpdated.");
        }

        public void OnFaulted(Exception e)
        {
            XLog.LogLine("Task Exception : " + e.Message);
            Messenger.Default.Send(new DialogContent
            {
                Title = "Task Exception",
                Content = e.Message
            }, Token);
        }

        public void OnTaskCanceled()
        {
            XLog.LogLine("Task is canceled.");
        }

        public void OnRequested(string msg)
        {
            XLog.LogLine("Task is in requesting.");
        }

        public RequestReceipt OnRetryRequest(string msg)
        {
            XLog.LogLine("Task request retry.\n" + msg);
            return RequestReceipt.OK;
        }
        
    }
}
