using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharpUtil
{
    // ReSharper disable once UnusedMember.Global
    public class HttpServer
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Thread m_ListenerThread;
        private readonly TcpListener m_Listener;

        public delegate HttpResponse OnHttpRequestEventHandler(HttpRequest request);

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event OnHttpRequestEventHandler OnHttpRequest;

        public HttpServer(IPAddress ip, int port)
        {
            m_Listener = new TcpListener(ip, port);
            m_ListenerThread = new Thread(MainProcess)
                                   {
                                       IsBackground = true,
                                       Name = "HttpServer"
                                   };
        }

        // ReSharper disable once UnusedMember.Global
        public void Start() => m_ListenerThread.Start();

        private void MainProcess()
        {
            m_Listener.Start();
            while (true)
            {
                var tcp = m_Listener.AcceptTcpClient();
                var thr = new Thread(o => Process((TcpClient)o));
                thr.Start(tcp);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Process(TcpClient tcp)
        {
            try
            {
                using (var stream = tcp.GetStream())
                {
                    HttpResponse response;
                    try
                    {
                        var request = RequestParser.Parse(stream);
                        if (OnHttpRequest == null)
                            throw new HttpException(501);
                        response = OnHttpRequest(request);
                    }
                    catch (HttpException e)
                    {
                        response = new HttpResponse { ResponseCode = e.ResponseCode };
                    }
                    catch (Exception e)
                    {
                        response = HttpUtil.GenerateHttpResponse(e.ToString(), "text/plain");
                        response.ResponseCode = 500;
                    }

                    using (response)
                        ResponseWriter.Write(stream, response);

                    stream.Close();
                }
                tcp.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
