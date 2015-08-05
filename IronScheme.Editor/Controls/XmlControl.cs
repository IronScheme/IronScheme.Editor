using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using IronScheme.Editor.ComponentModel;
using System.IO;

namespace IronScheme.Editor.Controls
{
  [Name("Browser")]
  partial class XmlControl : UserControl, IDocument
  {
    public XmlControl()
    {
      InitializeComponent();
    }

    public string Url
    {
      get { return webBrowser1.Url.OriginalString; }
      set { webBrowser1.Url = new Uri(value); }
    }

    public string Html
    {
      get { return webBrowser1.DocumentText; }
      set { webBrowser1.DocumentText = value; }
    }

    #region IDocument Members

    public void Open(string filename)
    {
      // blah
    }

    public void Close()
    {
      
    }

    public string Info
    {
      get { return webBrowser1.StatusText; }
    }

    #endregion
  }

  class XmlDocument : Document
  {
    public XmlDocument()
    {
      AddView(new XmlControl());
    }

    string tempfilename;

    protected override void SwitchView(IDocument newview, IDocument oldview)
    {
      if (newview is AdvancedTextBox)
      {
        if (tempfilename != null && File.Exists(tempfilename))
        {
          File.Delete(tempfilename);
        }
      }
      else
      {
        
        AdvancedTextBox atb = oldview as AdvancedTextBox;

        tempfilename = Path.Combine(Path.GetDirectoryName(atb.Buffer.FileName),  
          "XmlControl.TMP" + Path.GetExtension(atb.Buffer.FileName));

        XmlControl g = newview as XmlControl;

        TextWriter w = File.CreateText(tempfilename);

        atb.Buffer.SaveInternal(w, false);

        w.Close();

        g.Url = Path.GetFullPath(tempfilename);
      }
    }
  }
}
