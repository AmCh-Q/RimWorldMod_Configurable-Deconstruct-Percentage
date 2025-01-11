#if !v1_0 && !v1_1
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
	public static class ReturnsCount
	{
		public static readonly Func<int, int>
			d_DeconstructReturnCount = DeconstructReturnCount;
		public static int DestroyReturnCount(int count)
			=> ReturnCount(count, Settings.DestroyReturnPercent);
		public static int DeconstructReturnCount(int count)
			=> ReturnCount(count, Settings.DeconstructPercent);
		public static int CancelBuildingCount(int count)
			=> ReturnCount(count, Settings.CancelBuildingPercent);
		public static int CancelUnfinishedCount(int count)
			=> ReturnCount(count, Settings.CancelUnfinishedPercent);
		public static int FailReturnCount(int count)
			=> ReturnCount(count, Settings.FailReturnPercent);
		public static int ReturnCount(int count, int returnPct)
		{
			int min = Settings.MinimumReturn;
			if (returnPct >= 100 || min >= count)
				return count;
			int res = GenMath.RoundRandom(count * returnPct * 0.01f);
			if (res < min)
				return min;
			return res;
		}
		public static int DeconstructCount_Round(int count)
		{
			int min = Settings.MinimumReturn;
			if (Settings.DeconstructPercent >= 100 || min >= count)
				return count;
			int res = Mathf.RoundToInt(count * Settings.DeconstructPercent * 0.01f);
			if (res < min)
				return min;
			return res;
		}
	}
}
#endif
