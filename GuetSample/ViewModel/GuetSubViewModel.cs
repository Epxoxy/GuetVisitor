using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MessengerLight;
using SitesModel.Request;
using SitesModel.Providers;
using SitesModel.ModelBase;
using SitesModel.Helpers;
using GuetSample.Extension;

namespace GuetSample.ViewModel
{
    public class GuetSubViewModel : MonitorViewModel, IRequestMonitor, IDisposable
    {
        protected override string Token { get; set; } = "SubTask";

        private int maxMatches = 100;//Max matches, prevent matches error
        private Requester requester;
        private ILogable logger;
        
        public GuetSubViewModel(HttpRequestConfig config, SitesProvider provider, string fileName) : this(config, provider, null, fileName)
        {
        }

        public GuetSubViewModel(HttpRequestConfig config, SitesProvider provider, ILogable logger, string fileName) : base(fileName)
        {
            requester = new Requester(config);
            //GuetModelProvider
            CurrentPage = provider.getSiteModel("Bkjw")?.getWebPageModel(DefaultKey.SelectCourse);
            this.logger = logger;
        }
        

        #region --------  Properties  --------

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
                if (_isTaskEmpty != value)
                {
                    _isTaskEmpty = value;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RaisePropertyChanged(nameof(IsTaskEmpty));
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
                        if (_isRequesting) debugMsg("SubTask is in requesting.");
                        else debugMsg("SubTask request handed.");
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
                _repeatEnable = value;
                RaisePropertyChanged(nameof(RepeatEnable));
            }
        }
        
        private WebPageModel _currentPage;
        public WebPageModel CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                RaisePropertyChanged(nameof(CurrentPage));
                //Create optionals
                var optionals = new BindingList<OptionalWithValue<string>>();
                foreach (var optional in value.PostOptionals)
                {
                    optionals.Add(OptionalWithValue<string>.FromOptional(optional));
                }
                OptionalsList = optionals;
            }
        }
        
        public DelegateCommand AbortCommand { get; private set; }
        public DelegateCommand ShowSelectCourseCommand { get; private set; }

        private BindingList<OptionalWithValue<string>> _optionalsList;
        public BindingList<OptionalWithValue<string>> OptionalsList
        {
            get
            {
                return _optionalsList;
            }
            set
            {
                _optionalsList = value;
                RaisePropertyChanged(nameof(OptionalsList));
            }
        }

        public int Process { get; set; }
        public bool IsTaskWaiting { get; set; }

        #endregion


        #region -------- Command --------

        protected override void OnInitCommands()
        {
            base.OnInitCommands();
            AbortCommand = new DelegateCommand();
            ShowSelectCourseCommand = new DelegateCommand();

            AbortCommand.CanExecuteCommand += o => !IsTaskEmpty;
            ShowSelectCourseCommand.CanExecuteCommand += o => IsTaskEmpty;
            
            AbortCommand.ExecuteCommand += o =>
            {
                requester.cancel();
            };
            ShowSelectCourseCommand.ExecuteCommand += o =>
            {
                var list = OptionalsList;
                string[] values = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    values[i] = list[i].Value;
                    System.Diagnostics.Debug.WriteLine(values[i]);
                }
                if (RepeatEnable)
                {
                    requester.formatRequest(this, CurrentPage, handTableData, null, RepeatInfo, values);
                }
                else
                {
                    requester.formatRequest(this, CurrentPage, handTableData, null, values);
                }
            };
        }

        #endregion
        

        protected Task<HandResult> handTableData(WebPageModel page, PassingData passing, ResponseData<string> res)
        {
            return Task.Run(() =>
            {
                EmptyTableWithoutMonitoring();
                if (string.IsNullOrEmpty(page.RegexPattern))
                {
                    return HandResult.ForceStopCurrent;
                }
                DataTable cacheTable = null;
                cacheTable = res.Data.RegexToTable("WebData",page.RegexPattern, page.DataHeaders, this.maxMatches);
                updateDataTable(cacheTable);
                if (cacheTable == null) return HandResult.HandedButWrong;
                return HandResult.HandComplete;
            });
        }
        

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
            ResponseData<string> res = null;
            HttpFireTask fireTask = null;
            foreach(var change in changedList)
            {
                fireTask = change.FireTask;
                if (fireTask == null) continue;
                if (string.IsNullOrWhiteSpace(fireTask.Url)) continue;
                string info = $"FireTask [{change.PrimaryKeyValue}] [{change.ColumnName}]";
                try
                {
                    debugMsg("Try to run " + info);
                    if (fireTask.TypeOfPost)
                    {
                        res = await requester.branchRequest(this, NetRequestProvider.HttpPost, fireTask.Url, fireTask.Data);
                    }
                    else
                    {
                        res = await requester.branchRequest(this, NetRequestProvider.HttpGet, fireTask.Url, fireTask.Data);
                    }
                    debugMsg(info + " complete.");
                }
                catch (AggregateException e)
                {
                    debugMsg(info + " error : " + e.Message);
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                    {
                        debugMsg("FireTask Responese NULL.");
                    }
                    else
                    {
                        HttpWebResponse response = null;
                        if ((response = (e.Response as HttpWebResponse)) != null)
                        {
                            debugMsg("FireTask WebException : " + response.StatusCode);
                            response.Dispose();
                        }
                    }
                }
            }
        }


        public void OnProcessUpdated()
        {
            debugMsg("SubTask ProcessUpdated.");
        }

        public void OnFaulted(Exception e)
        {
            debugMsg("SubTask Task Exception : " + e.Message);
            Messenger.Default.Send(new DialogContent()
            {
                Title = "SubTask Exception",
                Content = e.Message,
                PlayMusic = true
            }, Token);
        }

        public void OnTaskCanceled()
        {
            debugMsg("SubTask is canceled.");
        }

        public RequestReceipt OnRetryRequest(string msg)
        {
            debugMsg("SubTask request retry.\n" + msg);
            return RequestReceipt.OK;
        }

        private void updateDataTable(DataTable table)
        {
            this.DataTable = table;
        }
        
        private void debugMsg(string msg)
        {
            if (logger != null)
            {
                logger.HandDebugMessage(msg);
            }
        }

        public void Dispose()
        {
            debugMsg("SubTask dispose.");
            requester.cancel();
        }
    }
}
