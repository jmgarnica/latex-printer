namespace LaTexPrinter
{
	public class Align : Enviroment
	{
		public Align(bool eq_number, Printer p)
			:base(eq_number? "align" : "align*", p)
		{}
	}
}