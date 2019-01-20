using System;
using System.Collections.Generic;
using System.Linq;
using LaTexPrinter;

namespace HMM
{
	class HMModel
	{
		int ord = 1;
		public readonly string[] states_tags, obs_tags;
		public readonly double[] Pinit;
		public readonly double[,] Ptransitions;
		public readonly double[,] Pemision;

		public HMModel(int st, int obs, int ord = 1)
		{
			states_tags = new string[st];
			obs_tags = new string[obs];
			Pinit = new double[st];
			Ptransitions = new double[st, st];
			Pemision = new double[st, obs];
		}

		public void UniformFill()
		{
			double stp = 1.0 / states_tags.Length;
			for (int i = 0; i < states_tags.Length; i++)
			{
				states_tags[i] = $"state{i}";

				Pinit[i] = stp;
				for (int j = 0; j < states_tags.Length; j++)
					Ptransitions[i, j] = stp;
				for (int j = 0; j < obs_tags.Length; j++)
					Pemision[i, j] = stp;
			}

			for (int i = 0; i < obs_tags.Length; i++)
				obs_tags[i] = $"observation{i}";
		}

		public void ReportParametrization(Printer p)
		{
			using (var i = p.Itemize())
			{
				i.Item();
				p.Ind();
				p.WriteLine($@"estados = {{{string.Join(", ", states_tags)}}}");

				i.Item();
				p.Ind();
				p.WriteLine($@"observaciones = {{{string.Join(", ", obs_tags)}}}");

				i.Item();
				p.Ind();
				p.WriteLine($@"Probabilidades a priori: {string.Join(", ", from j in Enumerable.Range(0, states_tags.Length) select $@"P({states_tags[j]}) = {Pinit[j]}")}");

				i.Item();
				p.Ind();
				p.WriteLine("Probabilidades de transición:");
				p.Tabular(3, 3, (a, b) =>
				{
					if (a == 0 && b == 0) return "";
					if (a == 0 || b == 0) return states_tags[a + b - 1];
					return Ptransitions[a - 1, b - 1].ToString();
				});

				i.Item();
				p.Ind();
				p.WriteLine("Probabilidades de emisión:");
				p.Tabular(3, 4, (a, b) =>
				{
					if (a == 0 && b == 0) return "";
					if (a == 0 ) return obs_tags[b - 1];
					if (b == 0) return states_tags[a - 1];
					return Pemision[a - 1, b - 1].ToString();
				});
			}
			p.WriteLine();
		}

		public double ObsLikelihoodDist(int[] observations, Printer tracer)
		{
			int[] st = new int[states_tags.Length];
			for (int i = 0; i < st.Length; i++) st[i] = i;
			double likelihood = 0;

			Func<IEnumerable<int[]>> perm = () => Utils.Permutations(st, observations.Length, true);

			tracer.Ind();
			tracer.WriteLine("Sequencias de estados:");

			List<double> stp = new List<double>();

			using (tracer.Align())
			{
				foreach (var a in perm())
				{
					tracer.Ind();
					tracer.WriteLine($@"P(Q_{stp.Count}|\mu) &= P(<{string.Join(", ", from k in a select states_tags[k])}>|\mu)" +
					$@" = \pi_{a[0] + 1} \cdot {string.Join(@" \cdot ", from j in Enumerable.Range(0, a.Length - 1) select $"a_{{{a[j] + 1},{a[j + 1] + 1}}}")}\\");

					double qv = Pinit[a[0]];
					for (int i = 0; i < a.Length - 1; i++)
						qv *= Ptransitions[a[i], a[i + 1]];

					tracer.Ind();
					tracer.WriteLine($@"P(Q_{stp.Count}|\mu) &= {Pinit[a[0]]} \cdot {string.Join(@" \cdot ", from j in Enumerable.Range(0, a.Length - 1) select Ptransitions[a[j], a[j + 1]])} = {qv}\\");
					stp.Add(qv);

					tracer.Ind();
					tracer.WriteLine(@"\\");
				}
			}

			tracer.Ind();
			tracer.WriteLine("Probabilidad de la observación:");

			double[] op = new double[stp.Count];

			using (tracer.Align())
			{
				int l = 0;
				foreach (var a in perm())
				{
					tracer.Ind();
					tracer.WriteLine($@"P(O|Q_{l}, \mu) &= {string.Join(@" \cdot ", from i in Enumerable.Range(0, a.Length) select $"b_{a[i] + 1}({observations[i] + 1})")}\\");

					double ov = 1;
					for (int i = 0; i < a.Length; i++)
						ov *= Pemision[a[i], observations[i]];

					tracer.Ind();
					tracer.WriteLine($@"P(O|Q_{l}, \mu) &= {string.Join(@" \cdot ", from i in Enumerable.Range(0, a.Length) select $"{Pemision[a[i], observations[i]]}")} = {ov}\\");

					tracer.Ind();
					tracer.WriteLine(@"\\");
					op[l] = ov;
					l++;
				}
			}

			for (int i = 0; i < op.Length; i++)
				likelihood += op[i] * stp[i];

			using (tracer.Align())
			{
				tracer.Ind();
				tracer.WriteLine($@"P(O|\mu) &= \forall_{{i = 1}}^{op.Length} p(O|q_i, \mu)p(q_i|\mu)\\");
				tracer.Ind();
				tracer.WriteLine($@" &= {string.Join(@" + ", from i in Enumerable.Range(0, op.Length) select $@"({stp[i]} \cdot {op[i]})")} = {likelihood}\\");
			}

			return likelihood;
		}


		public double ObsLikelihoodForward(int[] observations, Printer tracer)
		{
			double[,] pm = new double[states_tags.Length + 2, observations.Length];

			tracer.Ind();
			tracer.WriteLine("Inicialización:");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if(j == 0)
							tracer.Write($@"\pi_{i+1} \cdot b_{i+1}({observations[j]})");
						else
							tracer.Write("0");
					}
					m.NewRow();
				}
			}


			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if (j == 0)
						{
							pm[i, j] = Pinit[i] * Pemision[i, j];
							tracer.Write($@"{Pinit[i]} \cdot {Pemision[i, j]}");
						}
						else
							tracer.Write("0");

					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine(@"Inducción:\\");
			tracer.IndUp();

			for (int t = 1; t < observations.Length; t++)
			{
				tracer.Ind();
				tracer.WriteLine($@"t = {t + 1}");

				using (var m = tracer.Matrix())
				{
					for (int i = 0; i < states_tags.Length; i++)
						pm[i, t] = (from s in Enumerable.Range(0, states_tags.Length) select pm[s, t - 1] * Ptransitions[s, i] * Pemision[i, observations[t]]).Sum();

					for (int i = 0; i < states_tags.Length; i++)
					{
						for (int j = 0; j < observations.Length; j++)
						{
							int d = i;
							int f = t;
							m.NewElm();
							if (j < t)
								tracer.Write($@"{pm[i, j]}");
							else if (j == t)
								tracer.Write($@"[{string.Join(" + ", from s in Enumerable.Range(0, states_tags.Length)
								                                     select
								                                     $@"forward[{s + 1},{f}] \cdot a_{{{s + 1}, {d}}} \cdot b_{{{d}}}({observations
									                                     [f]})")}]");
							else tracer.Write("0");

						}
						m.NewRow();
					}
				}
			}

			double vero = 0;
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						tracer.Write($@"{pm[i, j]}");
						if (j == observations.Length - 1)
							vero += pm[i, j];
					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine($@"P(O|\mu) = {vero}");
			return vero;
		}

		public double ObsLikelihoodBackward(int[] observations, Printer tracer)
		{
			double[,] pm = new double[states_tags.Length + 2, observations.Length];

			tracer.Ind();
			tracer.WriteLine(@"Inicialización:\\");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if (j == observations.Length - 1)
							tracer.Write($@"a_{{{i + 1}, f}}");
						else
							tracer.Write("0");
					}
					m.NewRow();
				}
			}


			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if (j == observations.Length - 1)
						{
							pm[i, j] = 1;
							tracer.Write($@"{pm[i, j]}");
						}
						else
							tracer.Write("0");

					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine(@"Induccioón:\\");
			tracer.IndUp();

			for (int t = observations.Length - 2; t >= 0; t--)
			{
				tracer.Ind();
				tracer.WriteLine($@"t = {t + 1}");

				using (var m = tracer.Matrix())
				{
					for (int i = 0; i < states_tags.Length; i++)
						pm[i, t] = (from s in Enumerable.Range(0, states_tags.Length) select pm[s, t + 1] * Ptransitions[i, s] * Pemision[s, observations[t + 1]]).Sum();

					for (int i = 0; i < states_tags.Length; i++)
					{
						for (int j = 0; j < observations.Length; j++)
						{
							int d = i;
							int f = t + 1;
							m.NewElm();
							if (j > t)
								tracer.Write($@"{pm[i, j]}");
							else if (j == t)
								tracer.Write($@"[{string.Join(" + ", from s in Enumerable.Range(0, states_tags.Length)
																	 select
																	 $@"backward[{s + 1},{f}] \cdot a_{{{s + 1}, {d}}} \cdot b_{{{d}}}({observations
																		 [f]})")}]");
							else tracer.Write("0");

						}
						m.NewRow();
					}
				}
			}

			double vero = 0;
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					vero += pm[i, 0] * Pinit[i] * Pemision[i, observations[0]];
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						tracer.Write($@"{pm[i, j]}");
					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine($@"P(O|\mu) = {vero}");
			return vero;
		}

		public int[] PredictViterbi(int[] observations, Printer tracer)
		{
			int[,] path = new int[states_tags.Length, observations.Length];
			double[,] pm = new double[states_tags.Length, observations.Length];

			tracer.Ind();
			tracer.WriteLine(@"Inicialización:\\");
			tracer.Ind();
			tracer.WriteLine("Probabilidades iniciales:");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if (j == 0)
						{
							tracer.Write($@"\pi_{{{i + 1}}} \cdot b_{{{i + 1}}}({observations[0]})");
							pm[i, j] = Pinit[i] * Pemision[i, observations[0]];
						}
						else tracer.Write("0");
					}
					m.NewRow();
				}
			}
			tracer.Ind();
			tracer.WriteLine("Estados iniciales:");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						if (j == 0)
						{
							tracer.Write($@"{i+1}");
							path[i, j] = i;
						}
						else tracer.Write("0");
					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine(@"Inducción:\\");
			for (int t = 1; t < observations.Length; t++)
            {
				tracer.Ind();
				tracer.WriteLine($"t = {t+1}");
				using (var m = tracer.Matrix())
	            {
		            for (int i = 0; i < states_tags.Length; i++)
		            {
			            for (int j = 0; j < observations.Length; j++)
			            {
				            m.NewElm();
				            if (j < t)
					            tracer.Write($"{pm[i, j]}");
				            if (j == t)
				            {
					            tracer.Write(
					                         $@"max_{{1\le i \le {states_tags.Length}}} viterbi[{i + 1}, {t}] \cdot a_{{i, {i + 1}}} \cdot b_{{{i}}}({observations
						                         [t]})");
					            var a = (from s in Enumerable.Range(0, states_tags.Length)
					                     select new {v = pm[s, t - 1] * Ptransitions[s, i] * Pemision[i, observations[t]], b = s})
						            .OrderByDescending(c => c.v).First();

					            pm[i, j] = a.v;
					            path[i, j] = a.b;
				            }
				            else tracer.Write("0");
			            }
						m.NewRow();
		            }
	            }
			}

			tracer.Ind();
			tracer.WriteLine("Probabilidades finales:");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						tracer.Write($@"{pm[i,j]}");
					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine("Secuencias:");
			using (var m = tracer.Matrix())
			{
				for (int i = 0; i < states_tags.Length; i++)
				{
					for (int j = 0; j < observations.Length; j++)
					{
						m.NewElm();
						tracer.Write($@"{path[i,j] + 1}");
					}
					m.NewRow();
				}
			}

			tracer.Ind();
			tracer.WriteLine("Terminación:");
			var aa =
				(from s in Enumerable.Range(0, states_tags.Length) select new {v = pm[s, observations.Length - 1], b = s}).OrderByDescending(
				                                                                                                                   c
					                                                                                                                   =>
						                                                                                                                   c
							                                                                                                                   .v)
				                                                                                                          .First();
            tracer.Ind();
			tracer.WriteLine($@"$$ p(O|Q,\mu) = max_{{1\le i \le {states_tags.Length}}} viterbi[i, {observations.Length}] = {aa.v}$$");

			int[] p = new int[observations.Length];
			p[observations.Length - 1] = aa.b;
			for (int t = observations.Length-2 ; t >= 0; t--)
				p[t] = path[p[t + 1], t];

			tracer.Ind();
			tracer.Write($@"Secuencia más probable: $<${string.Join(", ", from s in p select states_tags[s])}$>$");
			return p;
		}


	}
}