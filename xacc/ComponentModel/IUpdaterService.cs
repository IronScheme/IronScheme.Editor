using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Xacc.ComponentModel
{
  public interface IUpdaterService : IService
  {
    void Check(bool download, bool install);
  }

  class UpdaterService : ServiceBase, IUpdaterService
  {
    enum StateFlags
    {
      Notify = 0,
      Download = 1,
      DownloadAndInstall = 2,
    }


    public void Check(bool download, bool install)
    {
      Trace.WriteLine("Checking for updates");
      Latest l = new Latest();
      l.GetLatestVersionCompleted += new GetLatestVersionCompletedEventHandler(l_GetLatestVersionCompleted);
      l.GetLatestVersionAsync(download ? install ? 2 : 1 : 0);
    }

    string latest;

    void l_GetLatestVersionCompleted(object sender, GetLatestVersionCompletedEventArgs e)
    {
      if (!e.Cancelled && e.Error == null)
      {
        int state = (int)e.UserState;
        latest = e.Result;
        Version latestver = new Version(latest);
        Version currver = typeof(UpdaterService).Assembly.GetName().Version;

        Trace.WriteLine("Latest version: {0} Current version: {1}", latest, currver);

        int d = 1;// latestver.CompareTo(currver);

        if (d > 0)
        {
          switch (state)
          {
            case 0:
              if (DialogResult.Yes == MessageBox.Show(ServiceHost.Window.MainForm, string.Format(
@"Your version: {0}   New version: {1}

Do you want to download the latest version?", currver, latestver),
                "New version avaliable!", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
              {
                state = 1;
                goto case 1;
              }
              break;
            case 1:
            case 2:
              Status.Write("Downloading latest version...");
              Trace.WriteLine("Downloading latest version");
              ServiceHost.StatusBar.Progress = 0;
              WebClient wc = new WebClient();
              wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
              wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
              wc.DownloadFileAsync(new Uri(string.Format("http://downloads.sourceforge.net/xacc/xacc.ide-{0}-setup.exe", latestver)),
                string.Format("{1}/xacc.ide-{0}-setup.exe", latestver, Path.GetDirectoryName(Application.ExecutablePath)),
                string.Format("{1}/xacc.ide-{0}-setup.exe", latestver, Path.GetDirectoryName(Application.ExecutablePath)));
              break;
          }
        }
      }
    }

    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      ServiceHost.StatusBar.Progress = e.ProgressPercentage / 100f;
      ServiceHost.StatusBar.StatusText = string.Format("Downloading... {0}% {1:f0}KB remaining", e.ProgressPercentage,(e.TotalBytesToReceive - e.BytesReceived)/1024f);
    }

    void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      ServiceHost.StatusBar.Progress = 1;
      if (!e.Cancelled && e.Error == null)
      {
        Status.Write("Download completed");
        Trace.WriteLine("Download completed");

        string fn = e.UserState as string;
        if (DialogResult.Yes == MessageBox.Show(ServiceHost.Window.MainForm,
          @"Would you like to install it now?", "Download completed", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
        {
          Process installer = Process.Start(fn);
          Application.Exit();
        }
      }
      else
      {
        Trace.WriteLine("Download failed: {0}", e.Error.Message);

        if (DialogResult.Yes == MessageBox.Show(ServiceHost.Window.MainForm,
          @"Download failed. Would you like to download it manually?", "Download error: " + e.Error.Message, MessageBoxButtons.YesNo, MessageBoxIcon.Error))
        {
          Process.Start(string.Format("http://downloads.sourceforge.net/xacc/xacc.ide-{0}-setup.exe", latest));
        }
      }
    }

  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.1366")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Web.Services.WebServiceBindingAttribute(Name = "LatestSoap", Namespace = "http://xacc.qsh.eu/")]
  public partial class Latest : System.Web.Services.Protocols.SoapHttpClientProtocol
  {

    private System.Threading.SendOrPostCallback GetLatestVersionOperationCompleted;

    private bool useDefaultCredentialsSetExplicitly;

    /// <remarks/>
    public Latest()
    {
      this.Url = "http://xacc-ide.qsh.eu/latest.asmx";
      if ((this.IsLocalFileSystemWebService(this.Url) == true))
      {
        this.UseDefaultCredentials = true;
        this.useDefaultCredentialsSetExplicitly = false;
      }
      else
      {
        this.useDefaultCredentialsSetExplicitly = true;
      }
    }

    public new string Url
    {
      get
      {
        return base.Url;
      }
      set
      {
        if ((((this.IsLocalFileSystemWebService(base.Url) == true)
                    && (this.useDefaultCredentialsSetExplicitly == false))
                    && (this.IsLocalFileSystemWebService(value) == false)))
        {
          base.UseDefaultCredentials = false;
        }
        base.Url = value;
      }
    }

    public new bool UseDefaultCredentials
    {
      get
      {
        return base.UseDefaultCredentials;
      }
      set
      {
        base.UseDefaultCredentials = value;
        this.useDefaultCredentialsSetExplicitly = true;
      }
    }

    /// <remarks/>
    public event GetLatestVersionCompletedEventHandler GetLatestVersionCompleted;

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://xacc.qsh.eu/GetLatestVersion", RequestNamespace = "http://xacc.qsh.eu/", ResponseNamespace = "http://xacc.qsh.eu/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetLatestVersion()
    {
      object[] results = this.Invoke("GetLatestVersion", new object[0]);
      return ((string)(results[0]));
    }

    /// <remarks/>
    public void GetLatestVersionAsync()
    {
      this.GetLatestVersionAsync(null);
    }

    /// <remarks/>
    public void GetLatestVersionAsync(object userState)
    {
      if ((this.GetLatestVersionOperationCompleted == null))
      {
        this.GetLatestVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLatestVersionOperationCompleted);
      }
      this.InvokeAsync("GetLatestVersion", new object[0], this.GetLatestVersionOperationCompleted, userState);
    }

    private void OnGetLatestVersionOperationCompleted(object arg)
    {
      if ((this.GetLatestVersionCompleted != null))
      {
        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        this.GetLatestVersionCompleted(this, new GetLatestVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
      }
    }

    /// <remarks/>
    public new void CancelAsync(object userState)
    {
      base.CancelAsync(userState);
    }

    private bool IsLocalFileSystemWebService(string url)
    {
      if (((url == null)
                  || (url == string.Empty)))
      {
        return false;
      }
      System.Uri wsUri = new System.Uri(url);
      if (((wsUri.Port >= 1024)
                  && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0)))
      {
        return true;
      }
      return false;
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.1366")]
  public delegate void GetLatestVersionCompletedEventHandler(object sender, GetLatestVersionCompletedEventArgs e);

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.1366")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class GetLatestVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
  {

    private object[] results;

    internal GetLatestVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
      :
            base(exception, cancelled, userState)
    {
      this.results = results;
    }

    /// <remarks/>
    public string Result
    {
      get
      {
        this.RaiseExceptionIfNecessary();
        return ((string)(this.results[0]));
      }
    }
  }

}
