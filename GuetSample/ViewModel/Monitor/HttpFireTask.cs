using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    [Serializable]
    public class HttpFireTask
    {
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }

        private string data;
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        private bool typeOfPost = false;
        public bool TypeOfPost
        {
            get
            {
                return typeOfPost;
            }
            set
            {
                typeOfPost = value;
            }
        }

        public static HttpFireTask CreateDefault()
        {
            return new HttpFireTask()
            {
                Url = "http://bkjw2.guet.edu.cn/student/select.asp",
                Data = "spno=000005&selecttype=%D6%D8%D0%DE&testtime=&course={0}&textbook{0}=0&lwBtnselect=%CC%E1%BD%BB",
                TypeOfPost = true
            };
        }
    }
}
