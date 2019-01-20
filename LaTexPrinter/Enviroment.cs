using System;

namespace LaTexPrinter
{
	public class Enviroment : IDisposable
	{
		protected string label;
		protected bool open;
		protected Printer underlay;
		protected bool indent_content;
		public Enviroment(string label,  Printer underlay,
		                  string curly_args = null, string square_args = null, bool indent_content = false, bool open = false)
		{
			this.label = label;
			this.open = open;
			this.underlay = underlay;
			this.indent_content = indent_content;

			var s = underlay.AlingChar();
			underlay.Write(s);
			underlay.WriteLine($@"\begin{{{label}}}{(curly_args != null? $"{{{curly_args}}}": "")}{(square_args != null? $"[{square_args}]" : "")}");
		}

		public virtual void Dispose()
		{
			if (open) return;
			var s = underlay.AlingChar();
			underlay.Write(s);
			underlay.WriteLine($@"\end{{{label}}}");
		}
	}
}