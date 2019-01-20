using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTexPrinter
{
	public enum MatixBorder
	{
		Pare
	}

	public class Matrix : Align
	{
		bool row_first = true;
		public Matrix(Printer p)
			:base(false, p)
		{
			p.IndUp();
			p.Ind();
			p.WriteLine(@"\begin{pmatrix}");
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public override void Dispose()
		{
			underlay.Ind();
			underlay.WriteLine(@"\end{pmatrix}");
			underlay.IndDw();
			base.Dispose();
		}

		public void NewRow()
		{
			underlay.WriteLine(@"\\");
			row_first = true;
		}

		public void NewElm()
		{
			if (row_first)
			{
				underlay.Ind();
				row_first = false;
			}
			else underlay.Write(" & ");
		}
	}
}
