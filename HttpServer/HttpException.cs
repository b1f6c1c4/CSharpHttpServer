using System;

namespace CSharpUtil
{
    public class HttpException : Exception
    {
        public int ResponseCode { get; }

        public HttpException(int code) { ResponseCode = code; }
    }
}
