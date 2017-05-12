using MessengerLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SitesModel.Request;
using GuetSample.Extension;
using SitesModel.Providers;
using SitesModel.ModelBase;
using SitesModel.Helpers;

namespace GuetSample.ViewModel
{
    public class GuetViewModel : MonitorViewModel, IRequestMonitor, ILogable
    {
        protected override string Token { get; set; } = "HomeView";

        public delegate Task<ResponseData<string>> HttpRequestTask(string url, string data);
        public delegate Task<HandResult> HandWebDataTask(WebPageModel page, PassingData passing, string result);
        private int maxMatches = 100;//Max matches, prevent matches error
        private Requester requester;
        private HttpRequestConfig httpRequestConfig;
        private SitesProvider guetModelProvider;
        private SiteModel guetBkjw;
        private StringBuilder consoleBuilder;
        private object lockObject = new object();
        private const string _actionAbortText = "Abort";
        private const string _actionRequestText = "Request";
        private const string _actionLoginText = "Login";
        private const string _actionLogouttText = "Logout";

        public GuetViewModel()
        {
            //GuetModelProvider
            guetModelProvider = new SitesProvider(new SitesDataProvider("GuetSample.Resources.Data"));
            guetBkjw = guetModelProvider.getSiteModel("Bkjw");
            if (guetBkjw != null)
            {
                SelectPage = guetBkjw.getWebPageModel(DefaultKey.SelectCourse);
                //HttpRequestConfig
                Encoding encoding = Encoding.GetEncoding(guetBkjw.EncodingName);
                httpRequestConfig = new HttpRequestConfig(encoding);
                httpRequestConfig.HoleCookieContainer = HttpControlCenter.CommonCookieContainer;
                //AsyncHttpRequestProvider
                requester = new Requester(httpRequestConfig);
            }
            consoleBuilder = new StringBuilder();
        }


        private string _logActionText = _actionLoginText;
        public string LogActionText
        {
            get
            {
                return _logActionText;
            }
            set
            {
                _logActionText = value;
                RaisePropertyChanged(nameof(LogActionText));
            }
        }


        private string _requestActionText = _actionRequestText;
        public string RequestActionText
        {
            get
            {
                return _requestActionText;
            }
            set
            {
                _requestActionText = value;
                RaisePropertyChanged(nameof(RequestActionText));
            }
        }


        #region --------  Properties  --------


        private string _userName = "";
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                RaisePropertyChanged(nameof(UserName));
            }
        }

        private bool _isLogined;
        public bool IsLogined
        {
            get
            {
                return _isLogined;
            }
            private set
            {
                if(_isLogined != value)
                {
                    _isLogined = value;
                    RaisePropertyChanged(nameof(IsLogined));
                    LoginCommand.RaiseCanExecuteChanged();
                    LogoutCommand.RaiseCanExecuteChanged();
                    ShowSelectCourseCommand.RaiseCanExecuteChanged();
                }
            }
        }
        
        private string _consoleMsg;
        public string ConsoleMsg
        {
            get
            {
                return _consoleMsg;
            }
            set
            {
                _consoleMsg = value;
                RaisePropertyChanged(nameof(ConsoleMsg));
            }
        }

        private RepeatInfo _repeatInfo = RepeatInfo.NewOnce();
        public RepeatInfo RepeatInfo
        {
            get
            {
                return _repeatInfo;
            }
            private set
            {
                _repeatInfo = value;
                RaisePropertyChanged(nameof(RepeatInfo));
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
                        LoginCommand.RaiseCanExecuteChanged();
                        AbortCommand.RaiseCanExecuteChanged();
                        ShowSelectCourseCommand.RaiseCanExecuteChanged();
                    }));
                }
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
                if (_isRequesting != value)
                {
                    _isRequesting = value;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if(_isRequesting) HandDebugMessage("MainTask is in requesting.");
                        else HandDebugMessage("MainTask request handed.");
                        RaisePropertyChanged(nameof(IsRequesting));
                    }));
                }
            }
        }

        private bool _repeatEnable;
        public bool RepeatEnable
        {
            get
            {
                return _repeatEnable;
            }
            set
            {
                if (_repeatEnable != value)
                {
                    _repeatEnable = value;
                    RaisePropertyChanged(nameof(RepeatEnable));
                }
            }
        }

        private WebPageModel _selectPage;
        public WebPageModel SelectPage
        {
            get
            {
                return _selectPage;
            }
            set
            {
                if(_selectPage != value)
                {
                    _selectPage = value;
                    RaisePropertyChanged(nameof(SelectPage));
                    //Create optionals
                    var optionals = new BindingList<OptionalWithValue<string>>();
                    foreach (var optional in value.PostOptionals)
                    {
                        optionals.Add(OptionalWithValue<string>.FromOptional(optional));
                    }
                    OptionalsList = optionals;
                }
            }
        }
        
        private BindingList<OptionalWithValue<string>> _optionalsList;
        public BindingList<OptionalWithValue<string>> OptionalsList
        {
            get
            {
                return _optionalsList;
            }
            set
            {
                if (_optionalsList != value)
                {
                    _optionalsList = value;
                    RaisePropertyChanged(nameof(OptionalsList));
                }
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
        
        public DelegateCommand ShowSelectCourseCommand { get; private set; }
        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand LogoutCommand { get; private set; }
        public DelegateCommand AbortCommand { get; private set; }
        public DelegateCommand ClearConsoleMsgCommand { get; private set; }
        public DelegateCommand ForceClearAllCommand { get; private set; }
        public DelegateCommand NewSubVMCommand { get; private set; }
        public DelegateCommand NavHtmlToBrowserCommand { get; private set; }

        public int Process { get; set; }
        public bool IsTaskWaiting { get; set; }

        #endregion
        

        #region -------- Command --------

        protected override void OnInitCommands()
        {
            base.OnInitCommands();
            ShowSelectCourseCommand = new DelegateCommand();
            LoginCommand = new DelegateCommand();
            LogoutCommand = new DelegateCommand();
            AbortCommand = new DelegateCommand();
            ClearConsoleMsgCommand = new DelegateCommand();
            ForceClearAllCommand = new DelegateCommand();
            NewSubVMCommand = new DelegateCommand();
            NavHtmlToBrowserCommand = new DelegateCommand();

            LoginCommand.CanExecuteCommand += o => IsTaskEmpty && !IsLogined;
            LogoutCommand.CanExecuteCommand += o => IsLogined;
            ShowSelectCourseCommand.CanExecuteCommand += o => IsTaskEmpty && IsLogined;
            AbortCommand.CanExecuteCommand += o => !IsTaskEmpty;
            
            ShowSelectCourseCommand.ExecuteCommand += o =>
            {
                var list = OptionalsList;
                string[] values = new string[list.Count];
                for(int i = 0; i < list.Count; i++)
                {
                    values[i] = list[i].Value;
                }
                var page = guetBkjw.WebPageModels[DefaultKey.SelectCourse];
                if (RepeatEnable)
                {
                    requester.formatRequest(this, page, handTableData, null, RepeatInfo, values);
                }
                else
                {
                    requester.formatRequest(this, page, handTableData, null, values);
                }
            };
            LoginCommand.ExecuteCommand += o =>
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    var pswBox = o as System.Windows.Controls.PasswordBox;
                    if (pswBox != null && !string.IsNullOrEmpty(pswBox.Password))
                    {
                        var page = guetBkjw.LoginModel;
                        requester.formatRequest(this, page, handLoginData, new object[] { UserName }, UserName, pswBox.Password);
                    }
                }
            };
            LogoutCommand.ExecuteCommand += o =>
            {
                //Abort previous
                requester.cancel();
                var page = guetBkjw.LogoutModel;
                requester.request(this, page, handLogoutData, string.Empty);
                updateLogin(false);
            };
            AbortCommand.ExecuteCommand += o =>
            {
                requester.cancel();
            };
            ClearConsoleMsgCommand.ExecuteCommand += o =>
            {
                this.emptyHandDebugMessage();
            };
            ForceClearAllCommand.ExecuteCommand += o =>
            {
                requester.cancel();
                httpRequestConfig.HoleCookieContainer = new CookieContainer();
                IsTaskEmpty = true;
                IsLogined = false;
            };
            NewSubVMCommand.ExecuteCommand += o =>
            {
                var subVM = new GuetSubViewModel(httpRequestConfig, guetModelProvider, this, "submonitors.dat");
                Messenger.Default.Send(new DataContextMsg()
                {
                    Message = "NewMonitorViewModel",
                    DataContext = subVM
                }, Token);
            };
            NavHtmlToBrowserCommand.ExecuteCommand += o =>
            {
                if (this.HtmlValue == null) return;
                Messenger.Default.Send(new DataContextMsg()
                {
                    Message = "ShowHtmlBrowser",
                    DataContext = this.HtmlValue
                }, Token);
            };
        }

        #endregion

        
        #region --------  WebData Handler  --------

        private Task<HandResult> handLoginData(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                HtmlValue = res.Data;
                if (passing.Addition == null || passing.Addition.Length <= 0) return HandResult.ForceStopCurrent;
                string userName = passing.Addition[0].ToString();
                string rightPattern = page.RegexPattern;
                string wrongPattern = "<p.+?><font.+?><big><b>(.+?)</b></big></font></p>;";
                string webData = res.Data;
                //
                updateLogin(false);
                if (string.IsNullOrWhiteSpace(webData) || webData.Length < 10)
                {   //Check if received data is error
                    HandDebugMessage("Received data error/nCheckBkjwLogin().");
                    return HandResult.RequestResend;
                }
                else
                {   //Check data match
                    MatchCollection matches = webData.clearHTMLHead().Matches(rightPattern);
                    if (matches.Count == 1)
                    {
                        GroupCollection group = matches[0].Groups;
                        if (group[2].Value == userName)
                        {
                            updateLogin(true);
                            HandDebugMessage("Login successful!");
                            return HandResult.HandComplete;
                        }
                        else
                        {
                            HandDebugMessage("User name no match, Request name : " + group[1].Value + "Input name : " + userName + "/nCheckBkjwLogin()");
                            return HandResult.ForceStopCurrent;
                        }
                    }
                    else
                    {
                        HandDebugMessage(Regex.IsMatch(res.Data, wrongPattern) ? "Wrong user name or password/nCheckBkjwLogin()." : "Data no match wrong pattern/nCheckBkjwLogin().");
                        return HandResult.ForceStopCurrent;
                    }
                }
            });
        }

        private Task<HandResult> handTableData(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                HtmlValue = res.Data;
                EmptyTableWithoutMonitoring();
                if (string.IsNullOrEmpty(page.RegexPattern))
                {
                    return HandResult.ForceStopCurrent;
                }
                DataTable cacheTable = res.Data.RegexToTable("data", page.RegexPattern, page.DataHeaders, maxMatches);
                updateDataTable(cacheTable);
                if (cacheTable == null) return HandResult.HandedButWrong;
                return HandResult.HandComplete;
            });
        }
        
        private Task<HandResult> handLogoutData(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                HtmlValue = res.Data;
                httpRequestConfig.HoleCookieContainer = new CookieContainer();
                updateLogin(false);
                HandDebugMessage("Logout.");
                return HandResult.HandComplete;
            });
        }

        private Task<HandResult> handHtmlTask(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(res.Data)) return HandResult.RequestResend;
                this.HtmlValue = res.Data;
                return HandResult.HandComplete;
            });
        }

        #endregion


        #region --------  Async run http request  --------
        
        private void sendRequestOnly(WebPageModel pageModel, string data = null)
        {
            requester.request(this, pageModel, handHtmlTask, data);
        }

        #endregion


        #region --------  Monitoring changed --------

        protected override void OnItemChanged(IEnumerable<CellMonitor> changedList, IEnumerable<CellMonitor> removedList)
        {
            base.OnItemChanged(changedList, removedList);
            StringBuilder builder = new StringBuilder();
            int changeCount = 0;
            foreach (var change in changedList)
            {
                builder.AppendLine(change.ToString());
                ++changeCount;
            }
            if (changeCount > 0) runHttpFireTask(changedList);
            foreach (var removed in removedList)
            {
                builder.AppendLine(removed.ToString());
            }
            string value = builder.ToString();
            Messenger.Default.Send(new DialogContent()
            {
                Title = "Change List (" + DateTime.Now.ToString() + ")",
                Content = value,
                PlayMusic = true
            }, Token);
        }

        private async void runHttpFireTask(IEnumerable<CellMonitor> changedList)
        {
            ResponseData<string> received = null;
            HttpFireTask fireTask = null;
            foreach (var change in changedList)
            {
                fireTask = change.FireTask;
                if (fireTask == null) continue;
                if (string.IsNullOrWhiteSpace(fireTask.Url)) continue;
                string info = $"FireTask [{change.PrimaryKeyValue}] [{change.ColumnName}]";
                try
                {
                    HandDebugMessage("Try to run " + info);
                    if (fireTask.TypeOfPost)
                    {
                        received = await requester.branchRequest(this, NetRequestProvider.HttpPost, fireTask.Url, fireTask.Data);
                    }
                    else
                    {
                        received = await requester.branchRequest(this, NetRequestProvider.HttpGet, fireTask.Url, fireTask.Data);
                    }
                    HandDebugMessage(info + " complete.");
                }
                catch (AggregateException e)
                {
                    HandDebugMessage(info + " error : " + e.Message);
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                    {
                        HandDebugMessage("FireTask Responese null.");
                    }
                    else
                    {
                        HttpWebResponse response = null;
                        if ((response = (e.Response as HttpWebResponse)) != null)
                        {
                            HandDebugMessage("FireTask WebException : " + response.StatusCode);
                            response.Dispose();
                        }
                    }
                }
            }
        }

        #endregion


        #region --------  IRequestMonitor  --------

        public void OnProcessUpdated()
        {
            HandDebugMessage("MainTask ProcessUpdated.");
        }

        public void OnFaulted(Exception e)
        {
            HandDebugMessage("MainTask Exception : " + e.Message);
            Messenger.Default.Send(new DialogContent()
            {
                Title = "MainTask Exception",
                Content = e.Message,
                PlayMusic = true
            }, Token);
        }

        public void OnTaskCanceled()
        {
            HandDebugMessage("MainTask is canceled.");
        }

        public RequestReceipt OnRetryRequest(string msg)
        {
            HandDebugMessage("MainTask request retry.\n" + msg);
            return RequestReceipt.OK;
        }

        #endregion


        private void updateDataTable(DataTable table)
        {
            this.DataTable = table;
        }

        private void updateLogin(bool isLogined)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => IsLogined = isLogined);
        }
        

        #region --------  Debug  --------

        private void emptyHandDebugMessage()
        {
            this.consoleBuilder.Clear();
            ConsoleMsg = string.Empty;
        }
        
        public void HandDebugMessage(string msg)
        {
            string xMsg = DateTime.Now.ToString("HH:mm:ss ffff") + " => " + msg;
            lock (lockObject)
            {
                consoleBuilder.AppendLine(xMsg);
                XLog.LogLine(xMsg);
                ConsoleMsg = consoleBuilder.ToString();
            }
        }

        #endregion

    }
}
