using System;
using System.Collections.Generic;

namespace Util
{
	public static class SortHelper
	{
		public static Comparison<T> CompareBy<T>(Func<T, int> getter)
		{
			return (lhs, rhs) => {
				return getter(lhs).CompareTo(getter(rhs));
			};
		}

		public static Comparison<T> Chain<T>(List<Comparison<T>> comparisons, bool reverse = false)
		{
			return (lhs, rhs) => {
				foreach (Comparison<T> comparison in comparisons)
				{
					int result = comparison(lhs, rhs);
					if (result != 0)
					{
						if (reverse)
						{
							return -result;
						}
						else
						{
							return result;
						}
					}
				}

				return 0;
			};
		}
	}
}
