using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using Xacc.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

namespace Xacc.LanguageDesigner
{
  [Serializable]
  class MacroList : ArrayList
  {
    TokenClass type;

    public MacroList(TokenClass type)
    {
      this.type = type;
    }

    public TokenClass Type
    {
      get {return type;}
    }

    public override string ToString()
    {
      return type.ToString();
    }

  }
	/// <summary>
	[Serializable]
  [XmlRoot("language")]
  public class Template
  {
    string name = "Untitled", longname, linecomment, commentstart, commentend;
    string[] exts, keywords, pragmas, types, operators;
    ArrayList strings = new MacroList(TokenClass.String), chars = new MacroList(TokenClass.Character),
      numbers = new MacroList(TokenClass.Number), identifiers = new MacroList(TokenClass.Identifier);

    EncodingEnum encoding;
    LexMode mode;
    bool ignorecase;

    internal string generated = string.Empty;

    
    public string sampletext = string.Empty;

    [Category("General")]
    [XmlAttribute("name")]
    public string Name
    {
      get {return name;}
      set {name = value;}
    }

    [Category("General")]
    [XmlAttribute("longname")]
    public string LongName
    {
      get {return longname;}
      set {longname = value;}
    }

    [Category("General")]
    [XmlElement("extension")]
    public string[] Extensions
    {
      get {return exts;}
      set 
      {
        if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == string.Empty))
        {
          exts = null;
        }
        else
        {
          exts = value;
        }
      }
    }

    public enum EncodingEnum { SevenBit, EightBit, Unicode }

    [Category("Settings")]
    [XmlAttribute("encoding")]
    public EncodingEnum Encoding
    {
      get {return encoding;}
      set {encoding = value;}
    }

    [Category("Settings")]
    [XmlAttribute("ignorecase")]
    public bool IgnoreCase
    {
      get {return ignorecase;}
      set {ignorecase = value;}
    }

    public enum LexMode { Lax , Strict }

    [Category("Settings")]
    [XmlAttribute("mode")]
    public LexMode Mode
    {
      get {return mode;}
      set {mode = value;}
    }

    [Category("Keywords")]
    [XmlElement("keyword")]
    public string[] Keywords
    {
      get {return keywords;}
      set 
      {
        if (value == null || value.Length == 0  || (value.Length == 1 && value[0] == string.Empty))
        {
          keywords = null;
        }
        else
        {
          keywords = value;
        }
      }
    }

    [Category("Keywords")]
    [XmlElement("pragma")]
    public string[] Pragmas
    {
      get {return pragmas;}
      set 
      {
        if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == string.Empty))
        {
          pragmas = null;
        }
        else
        {
          pragmas = value;
        }
      }
    }

    [Category("Keywords")]
    [XmlElement("type")]
    public string[] Types
    {
      get {return types;}
      set 
      {
        if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == string.Empty))
        {
          types = null;
        }
        else
        {
          types = value;
        }
      }
    }

    [Category("Keywords")]
    [XmlElement("operator")]
    public string[] Operators
    {
      get {return operators;}
      set 
      {
        if (value == null || value.Length == 0 || (value.Length == 1 && value[0] == string.Empty))
        {
          operators = null;
        }
        else
        {
          operators = value;
        }
      }
    }

    [Category("Comments")]
    public string LineComment
    {
      get {return linecomment;}
      set {linecomment = value;}
    }

    [Category("Comments")]
    public string CommentStart
    {
      get {return commentstart;}
      set {commentstart = value;}
    }

    [Category("Comments")]
    public string CommentEnd
    {
      get {return commentend;}
      set {commentend = value;}
    }



    class MacroCollectionConverter :TypeConverter
    {
      public MacroCollectionConverter()
      {
        ;
      }

      public override bool GetPropertiesSupported(ITypeDescriptorContext context)
      {
        return base.GetPropertiesSupported(context);
      }

      public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
      {
        return base.GetProperties (context, value, attributes);
      }

      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
        //return true;
        return base.CanConvertTo (context, destinationType);
      }

      public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
      {
        //return "foo";
        return base.ConvertTo (context, culture, value, destinationType);
      }

    }

    class MacroCollectionEditor :CollectionEditor
    {
      public MacroCollectionEditor() : base(typeof(Macro[]))
      {
        ;
      }

      protected override CollectionForm CreateCollectionForm()
      {
        CollectionForm f = base.CreateCollectionForm ();
        foreach (Control c in f.Controls)
        {
          Button b = c as Button;
          if (b != null)
          {
            b.FlatStyle = FlatStyle.System;
            continue;
          }
          GroupBox gb = c as GroupBox;
          if (gb != null)
          {
            gb.FlatStyle = FlatStyle.System;
            continue;
          }
          Label l = c as Label;
          if (l != null)
          {
            l.FlatStyle = FlatStyle.System;
            continue;
          }


        }
        f.Width += 100;
        return f;
      }


      public override bool GetPaintValueSupported(ITypeDescriptorContext context)
      {
        return true;
      }

      static Hashtable brushes = new Hashtable();

      static Brush GetBrush(TokenClass c)
      {
        if (!brushes.ContainsKey(c))
        {
          Color clr = Languages.Language.GetColorInfo(c).ForeColor;
          brushes[c] = new SolidBrush(clr);
        }
        return brushes[c] as Brush;
      }

      public override void PaintValue(PaintValueEventArgs e)
      {
        MacroList l = e.Value as MacroList;
        if (l != null)
        {
          e.Graphics.FillRectangle(GetBrush(l.Type), e.Bounds);
        }
        base.PaintValue (e);
      }

      protected override object CreateInstance(Type itemType)
      {
        return Macro.PREDEFINED["Nothing"];
      }

      protected override Type CreateCollectionItemType()
      {
        return typeof(Macro);
      }

      protected override Type[] CreateNewItemTypes()
      {
        return new Type[] { typeof(Macro)};
      }
    }

    public enum MacroType
    {
      String,
      Regex,
      Predefined
    }

    class MacroEditor : UITypeEditor
    {
      public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
      {
        return UITypeEditorEditStyle.None;
      }

      public override bool GetPaintValueSupported(ITypeDescriptorContext context)
      {
        return true;
        //return base.GetPaintValueSupported (context);
      }

      static StringFormat SF = new StringFormat();

      static MacroEditor()
      {
         SF.Alignment = SF.LineAlignment = StringAlignment.Center;
      }

      public override void PaintValue(PaintValueEventArgs e)
      {
        string val = "";
        if (e.Value is PredefinedMacro)
        {
          val = "P";
        }
        else
        {
          Macro m = e.Value as Macro;
          if (m.type == MacroType.Regex)
          {
            val = "RE";
          }
          else if (m.type == MacroType.String)
          {
            val = "S";
          }
        }
        e.Graphics.DrawString(val, SystemInformation.MenuFont, System.Drawing.Brushes.Black, e.Bounds, SF);
        base.PaintValue (e);
      }
    }

    class MacroConverter : TypeConverter
    {
      public MacroConverter()
      {
        ;
      }

      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
        return true;
      }

      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
        ArrayList vals = new ArrayList(Macro.PREDEFINED.Values);
        vals.Sort();
        return new StandardValuesCollection(vals);
      }

      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
        return true;
      }

      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
        return true;
      }

      public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
        Macro m;
        if ((m = Macro.IsPredefined(value as String)) != null)
        {
          return m;
        }
        m = new Macro(value as string);
        return m; 
      }

      public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
      {
        if (value is PredefinedMacro)
        {
          return ((PredefinedMacro)value).name;
        }

        return ((Macro) value).value;
      }

      public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
      {
        return true;
      }

      public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
      {
        return base.CreateInstance (context, propertyValues);
      }


    }

 
    [Serializable]
    [Editor(typeof(MacroEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(MacroConverter))]
    public class Macro
    {
      public MacroType type;
      internal string value;
      const string NOTHING = "\\b";

      public Macro()
      {
        value = NOTHING;
      }

      public Macro(string value)
      {
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
          type = MacroType.String;
        }
        else
        {
          type = MacroType.Regex;
        }
        this.value = value;
      }

      public static readonly Hashtable PREDEFINED = new Hashtable();

      public static Macro IsPredefined(string name)
      {
        return PREDEFINED[name] as Macro;
      }

      public static void Add(string name, string value)
      {
        PREDEFINED.Add(name, new PredefinedMacro(name, value));
      }

      static Macro()
      {
        Add("Nothing", NOTHING);
        Add("C identifier", "[_a-zA-Z][_0-9a-zA-Z]*");
        Add("C string", "\\\"([^\\\"])*\\\"");
        Add("C character", "'([^'])'");
        Add("C escape character", @"'(\\[nrtvb0\\])'");
        Add("Integer", "[0-9]+");
        Add("Float", @"[0-9]*\.[0-9]+");
      }

      public override string ToString()
      {
        return value;
      }

    }

    [Serializable]
    public class PredefinedMacro : Macro, IComparable
    {
      internal string name;
      public PredefinedMacro(string name, string value) : base(value)
      {
        type = MacroType.Predefined;
        this.name = name;
      }
      

      public int CompareTo(object obj)
      {
        PredefinedMacro m = obj as PredefinedMacro;
        if (m == null)
        {
          return -1;
        }
        return name.CompareTo(m.name);
      }

    }

    [XmlElement("string")]
    [Editor(typeof(MacroCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(MacroCollectionConverter))]
    public ArrayList String
    {
      get {return strings;}
    }

    [XmlElement("character")]
    [Editor(typeof(MacroCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(MacroCollectionConverter))]
    public ArrayList Character
    {
      get {return chars;}
    }

    [XmlElement("identifier")]
    [Editor(typeof(MacroCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(MacroCollectionConverter))]
    public ArrayList Identifier
    {
      get {return identifiers;}
    }

    [XmlElement("number")]
    [Editor(typeof(MacroCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(MacroCollectionConverter))]
    public ArrayList Number
    {
      get {return numbers;}
    }




    public void Generate()
    {
      using (TextReader r = new StreamReader(typeof(Template).Assembly.GetManifestResourceStream("Xacc.LanguageDesigner.Template.lex")))
      {
        generated = string.Empty;
        string lines = r.ReadToEnd();

        lines = lines.Replace("#NAME#", Name);
        if (longname == null || longname == string.Empty)
        {
          longname = Name;
        }
        lines = lines.Replace("#LONGNAME#", longname);

        if (exts == null)
        {
          lines = lines.Replace("#EXTS#", string.Empty);
        }
        else
        {
          string[] ex = Stringify(exts);
          lines = lines.Replace("#EXTS#", string.Join(", ", ex));
        }

        const string NOTHING = "\\b";

        lines = lines.Replace("#UNICODE#", encoding == EncodingEnum.Unicode ? "%unicode" : string.Empty);
        lines = lines.Replace("#FULL#", encoding == EncodingEnum.EightBit ? "%full" : string.Empty);
        lines = lines.Replace("#IGNORECASE#", ignorecase ? "%ignorecase" : string.Empty);

        lines = lines.Replace("#KEYWORDS#", Join(Stringify(keywords)));
        lines = lines.Replace("#PREPROCS#",Join(Stringify(pragmas)));
        lines = lines.Replace("#TYPES#", Join(Stringify(types)));

        lines = lines.Replace("#STRINGS#", Join(strings));
        lines = lines.Replace("#CHARACTERS#", Join(chars));
        lines = lines.Replace("#IDENTIFIER#", Join(identifiers));
        lines = lines.Replace("#NUMBERS#", Join(numbers));
        
        lines = lines.Replace("#OPERATOR#", Join(Stringify(operators)));

        lines = lines.Replace("#LINE_COMMENT#", linecomment == null ? NOTHING : Stringify(linecomment));
        lines = lines.Replace("#COMMENT_START#", commentstart == null ? NOTHING : Stringify(commentstart));
        lines = lines.Replace("#COMMENT_END#", commentend == null ? NOTHING : Stringify(commentend));

        lines = lines.Replace("#COMMENT_END_FIRST#", commentend == null || commentend.Length == 0 ? "b" : commentend[0].ToString());

        lines = lines.Replace("#MODE#", mode == LexMode.Lax ? "PLAIN" : "ERROR");

        

        //System.Diagnostics.Trace.WriteLine(lines);

        using (TextWriter w = File.CreateText(name + ".lex"))
        {
          w.WriteLine(lines);
        }

        generated = lines;
      }
    }

    static string Join(IList vals)
    {
      if (vals == null || vals.Count == 0)
      {
        return "\\b";
      }
      string val = vals[0].ToString();
      for (int i = 1; i < vals.Count; i++)
      {
        val += "|" + vals[i];
      }
      return val;
    }

    static string Stringify(object val)
    {
      return "\"" + val + "\"";
    }

    static string[] Stringify(IList vals)
    {
      if (vals == null || vals.Count == 0)
      {
        return null;
      }
      string[] output = new string[vals.Count];

      for (int i = 0; i < output.Length; i++)
      {
        output[i] = Stringify(vals[i]);
      }

      return output;
    }

    public Languages.Language Compile()
    {
      Generate();
      
      Process p = new Process();
      ProcessStartInfo psi = new ProcessStartInfo("Tools/cs_lex.exe", name + ".lex");
      psi.UseShellExecute = false;
      psi.RedirectStandardError = psi.RedirectStandardOutput = true;
      psi.CreateNoWindow = true;
      
      p.StartInfo = psi;
      
      p.Start();
      p.WaitForExit();

      Trace.WriteLine(p.StandardError.ReadToEnd());
      Trace.WriteLine(p.StandardOutput.ReadToEnd());

      string outfn = name + ".lex.cs";

      bool dbg = Debugger.IsAttached;

      string cmd = ServiceHost.Discovery.NetRuntimeRoot + "\\csc.exe";
      string args = string.Format("-nologo -out:{0} {1} -t:library -r:xacc.dll -d:STANDALONE -nowarn:0162 {2}", 
        name + ".dll", dbg ? "-debug" : "-o", outfn);

      psi = new ProcessStartInfo(cmd, args);
      psi.CreateNoWindow = true;
      psi.UseShellExecute = false;
      psi.RedirectStandardError = true;
      psi.RedirectStandardOutput = true;

      p = Process.Start(psi);

      p.WaitForExit();

      Trace.WriteLine(p.StandardError.ReadToEnd());
      Trace.WriteLine(p.StandardOutput.ReadToEnd());

      byte[] data = null;
      byte[] dbgdata = null;

      using (Stream s = File.OpenRead(name + ".dll"))
      {
        data = new byte[s.Length];
        s.Read(data, 0, data.Length);
      }

      if (File.Exists(name + ".pdb"))
      {
        using (Stream s = File.OpenRead(name + ".pdb"))
        {
          dbgdata = new byte[s.Length];
          s.Read(dbgdata, 0, dbgdata.Length);
        }

      }
      
      Assembly ass = Assembly.Load(data, dbgdata);

      ServiceHost.Plugin.LoadAssembly(ass);

      Languages.Language l = ServiceHost.Language.GetLanguage(name);

      return l;
    }

    public Languages.Language Install()
    {
      Languages.Language l = Compile();

      if (!Directory.Exists("Plugins"))
      {
        Directory.CreateDirectory("Plugins");
      }

      File.Copy(Name + ".dll", "Plugins/Plugin.Language." + Name + ".dll", true);
      return l;
    }

    //static XmlSerializer SER = new XmlSerializer(typeof(Template));
    static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter SER = 
      new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

    public void Save(string sampletext)
    {
      using (Stream w = File.Create(name + ".lang"))
      {
        this.sampletext = sampletext;
        SER.Serialize(w, this);
      }
    }

    public static Template Open(string filename)
    {
      using (Stream r = File.OpenRead(filename))
      {
        return SER.Deserialize(r) as Template;
      }
    }
  }
}
