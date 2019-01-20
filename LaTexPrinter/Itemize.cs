using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTexPrinter
{
	public class Itemize : Enviroment
	{
		bool first = true;
		public Itemize(Printer p, bool indent_content = true)
			:base("itemize", p, indent_content: indent_content)
		{}

		public void Item(string square_args = null)
		{
			if (indent_content)
			{
				if (first)
				{
					underlay.IndUp();
					first = false;
				}
				else underlay.IndDw();
			}

			var s = underlay.AlingChar();
			underlay.Write(s);
			underlay.WriteLine($@"\item{(square_args != null? $"[{square_args}]" : "")}");
			if(indent_content)
				underlay.IndUp();
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public override void Dispose()
		{
			underlay.IndDw();
			underlay.IndDw();
			base.Dispose();
		}
	}
}
