using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LaTexPrinter
{
    public class Printer : StreamWriter
    {
		public string BaseTemplate;
	    public int Indentationlevel = 0;
	    readonly bool usetabs;
	    int template_position = 0;

	    public Printer(string path, bool usetabs = true)
		    : base(new FileStream(path, FileMode.Create, FileAccess.Write))
	    {
		    this.usetabs = usetabs;
	    }

	    public void LoadTemplate(string path)
	    {
		    using (var f = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
		    {
			    BaseTemplate = f.ReadToEnd();
		    }
	    }

	    public void Ind()
	    {
		    Write(new string(usetabs? '\t' : ' ', Indentationlevel));
	    }

	    public void IndUp()
	    {
		    Indentationlevel++;
	    }

	    public void IndDw()
	    {
		    Indentationlevel -= Indentationlevel > 0? 1 : 0;
	    }

	    public string AlingChar()
	    {
		    char t = usetabs? '\t' : ' ';
			return new string(t, Indentationlevel);
	    }

	    public bool AdvanceToTag(string tag, bool autoset_indent = true)
	    {
		    int tagdex = BaseTemplate.IndexOf($"<{tag}>", template_position, StringComparison.Ordinal);
		    if (tagdex < 0)
		    {
			    Write(BaseTemplate.Substring(template_position));
			    return false;
			}

		    int indent = 0;
		    int i = tagdex - 1;
		    while (i >= 0 && BaseTemplate[i] == (usetabs? '\t' : ' '))
		    {
			    indent++;
			    i--;
		    }
		    if (i >= 0 && (BaseTemplate[i] == '\n' || BaseTemplate[i] == '\r'))
		    {
			    Indentationlevel = indent;
			    tagdex -= indent;
		    }
		    else indent = 0;

		    Write(BaseTemplate.Substring(template_position, tagdex - template_position));

		    template_position = tagdex + indent + tag.Length + 2;
		    return true;
	    }

	    public void EndTemplate()
	    {
			Write(BaseTemplate.Substring(template_position));
		    template_position = BaseTemplate.Length;
	    }

	    public void Tabular(int rows, int colunms, Func<int, int, string> content,
	                        Func<int, char> col_align = null, string separator = "|", bool hr = true, string ambient = "tabular")
	    {
		    col_align = col_align ?? (i => 'c');
		    var s = AlingChar();
			Write(s);
		    WriteLine($@"\begin{{{ambient}}}{{{separator + string.Join(separator, from i in Enumerable.Range(0, colunms) select col_align(i)) + separator}}}");
			if (hr)
			{
				Write(s);
				WriteLine(@"\hline");
			}
			for (int i = 0; i < rows; i++)
		    {
				Write(s);
				if(colunms > 0) Write(content(i,0));
			    for (int j = 1; j < colunms; j++)
			    {
					Write(" & ");
				    Write(content(i, j));
			    }
				WriteLine(@"\\");
			    if (hr)
			    {
					Write(s);
					WriteLine(@"\hline");
			    }
		    }
			Write(s);
			WriteLine($@"\end{{{ambient}}}");
	    }

	    public void Section(string title, int sub = 0, bool autoset_indent = false)
	    {
		    if (autoset_indent)
			    Indentationlevel = sub + 1;

		    var s = AlingChar();
			Write(s);
			WriteLine($@"\{string.Join("", Enumerable.Repeat("sub", sub))}section{{{title}}}");
	    }

	    public Itemize Itemize(bool auto_indent = true)
	    {
		    return new Itemize(this, auto_indent);
	    }

	    public Align Align(bool eqnumber = false)
	    {
		    return new Align(eqnumber, this);
	    }

	    public Matrix Matrix()
	    {
		    return new Matrix(this);
	    }
    }
}