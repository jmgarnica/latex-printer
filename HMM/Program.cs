using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaTexPrinter;

namespace HMM
{
	class Program
	{
		static void Main(string[] args)
		{
			var m = Init();

			using (var p = new Printer("test.txt"))
			{
				p.LoadTemplate("doc_template.tex");

				p.AdvanceToTag("title");
				p.Write("Tarea de Modelos Gráficos Probabilistas");

				p.AdvanceToTag("author");
				p.Write(@"Jesús Manuel Garnica Bonome\\C511");

				p.AdvanceToTag("date");

				p.AdvanceToTag("content");

				p.Ind();
				p.WriteLine("Parametrizacion del Modelo");
				m.ReportParametrization(p);

				p.Section("Ejercicio 8.1");
				m.ObsLikelihoodDist(new[] {0, 0, 2}, p);

				p.Section("Ejercicio 8.2");
				p.Section("Forward", 1, true);
				m.ObsLikelihoodForward(new[] {0, 0, 2}, p);

				p.Section("Ejercicio 8.3", 1, true);
				m.ObsLikelihoodBackward(new[] { 0, 0, 2 }, p);

				p.Section("Viterbi", 0, true);
				m.PredictViterbi(new[] { 0, 0, 2 }, p);

				p.EndTemplate();
			}
		}

		public static HMModel Init()
		{
			HMModel m = new HMModel(2, 3);
			m.states_tags[0] = "lluvioso";
			m.states_tags[1] = "soleado";
			m.obs_tags[0] = "caminar";
			m.obs_tags[1] = "comprar";
			m.obs_tags[2] = "limpiar";
			m.Pinit[0] = .6;
			m.Pinit[1] = .4;
			m.Ptransitions[0, 0] = .7;
			m.Ptransitions[0, 1] = .3;
			m.Ptransitions[1, 0] = .4;
			m.Ptransitions[1, 1] = .6;
			m.Pemision[0, 0] = .1;
			m.Pemision[0, 1] = .4;
			m.Pemision[0, 2] = .5;
			m.Pemision[1, 0] = .6;
			m.Pemision[1, 1] = .3;
			m.Pemision[1, 2] = .1;

			return m;
		}
	}
}
