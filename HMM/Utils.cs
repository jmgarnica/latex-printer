using System.Collections.Generic;

namespace HMM
{
	static class Utils
	{

		public static IEnumerable<T[]> Permutations<T>(T[] source, int count, bool repetitions = false)
        {
			T[] comb = new T[count];
			bool[] used = new bool[source.Length];
			return Permutations(source, comb, used, 0, repetitions);
		}

		static IEnumerable<T[]> Permutations<T>(T[] source, T[] comb, bool[] used, int lvl, bool repetitions)
		{
			if (lvl >= comb.Length)
				yield return (T[])comb.Clone();
			else
				for (int i = 0; i < source.Length; i++)
				{
					if (!repetitions && used[i]) continue;

					comb[lvl] = source[i];
					if(!repetitions)
						used[i] = true;
					foreach (var a in Permutations(source, comb, used, lvl + 1, repetitions))
						yield return a;
					if(!repetitions)
						used[i] = false;
				}
		}

		public static IEnumerable<T[]> Combina<T>(T[] source, int cant)
		{
			T[] comb = new T[cant];
			return Combina(source, comb, 0, 0);
		}

		static IEnumerable<T[]> Combina<T>(T[] source, T[] comb, int index, int lvl)
		{
			if (lvl == comb.Length)
				yield return (T[])comb.Clone();
			else
				for (int i = index; i < source.Length; i++)
				{
					comb[lvl] = source[i];
					foreach (var a in Combina(source, comb, i + 1, lvl + 1))
						yield return a;
				}
		}
	}
}