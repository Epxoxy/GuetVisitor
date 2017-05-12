using System;

namespace SitesModel.Request
{
    public class ResponseData<T>
    {
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }

        private ResponseData() { }

        public ResponseData(T data)
        {
            this.Data = data;
        }

        public ResponseData(T data, string errorMsg)
        {
            this.Data = data;
            this.ErrorMessage = errorMsg;
        }

        public static ResponseData<T> FromError(string msg)
        {
            return new ResponseData<T>() { ErrorMessage = msg };
        }

        public static ResponseData<T> FromData(T data)
        {
            return new ResponseData<T>(data);
        }

        public static ResponseData<T> FromError(Exception e)
        {
            return new ResponseData<T>() { Exception = e};
        }
    }
}
