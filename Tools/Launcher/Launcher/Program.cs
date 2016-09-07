using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Launcher.Properties;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Text;

namespace Launcher
{

    //   NOTE: HTTPListener rquires either being run as admin, or a nesh urlacl exception added for the prefix "http://+:80806/"


    static class Program
    {

        private static HttpListener http;
        private static Process process;

        static void Server()
        {
            
            try
            {
                http = new HttpListener();
                http.Prefixes.Add("http://+:8086/");
                http.Start();

                while (http.IsListening && (Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) == 0)
                {
                    ThreadPool.QueueUserWorkItem((c) =>
                 {
                     var ctx = c as HttpListenerContext;
                     try
                     {
                            string rstr = ProcessRequest(ctx.Request);
                            byte[] buf = Encoding.UTF8.GetBytes(rstr);
                            ctx.Response.ContentLength64 = buf.Length;
                            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                     }
                     catch
                     { } // suppress any exceptions
                        finally
                     {
                            ctx.Response.OutputStream.Close();
                     }
                 }, http.GetContext());

                }
            } catch (ThreadAbortException)
            { }
            finally
            {
                if (http != null)
                    http.Stop();
                http.Close();
                http = null;
            }

        }

        static string ProcessRequest(HttpListenerRequest request)
        {
            switch (request.Url.AbsolutePath.Trim('/').ToLower())
            {
                case "start":
                    if(process != null)
                    {
                        process.CloseMainWindow();
                    }
                    process = new Process();
                    process.StartInfo.FileName = Settings.Default.EXEPath;
                    process.Start();

                    break;
                case "stop":
                    if (process != null)
                    {
                        process.CloseMainWindow();
                        process = null;
                    }
  
                    break;

            }
            return "OK";
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NotifyIcon notifyIcon1 = new NotifyIcon();
            ContextMenu contextMenu1 = new ContextMenu();
            MenuItem menuItem1 = new MenuItem();
            contextMenu1.MenuItems.AddRange(new MenuItem[] { menuItem1 });
            menuItem1.Index = 0;
            menuItem1.Text = "Exit";
            menuItem1.Click += new EventHandler(menuItem1_Click);
            notifyIcon1.Icon = Resources.Icon;
            notifyIcon1.Text = "Project Freeride IC launcher";
            notifyIcon1.ContextMenu = contextMenu1;
            notifyIcon1.Visible = true;


            //start http server
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Server();
            });


            Application.Run();
            notifyIcon1.Visible = false;
        }

        private static void menuItem1_Click(object Sender, EventArgs e)
        {
            Application.Exit();
        }
        
    }

}
