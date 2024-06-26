using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Game
{
	/*public enum Str
	{
		Coalpowder,
		CokeCoal,
		Cylinder,
		FireBrick,
		ScrapIron,
		LPG,
		LNG,
		He,
		Ar,
		CrudeSalt,
		RefinedSalt,
		Yeast,
		Alum,
		QuartzPowder,
		Lichenin,
		MonoSi,
		PolySi,
		Resistor,
		BasicMachineCase,
		SecondryMachineCase,
		FireBrickWall,
		Temperature,
		Drill,
		Tubularis,
		SteelBlock,
		Fridge,
		Magnetizer,
		Separator,
		Battery,
		Wire,
		DirectCurrentDynamo,
		ResistanceFurnaceName,
		ResistanceFurnace,
		AirBlower,
		Fan,
		SteamBoat,
		Train,
		Carriage,
		Airship,
		RifleBarrel,
		Unloader,
		Mine,
		Torpedo,
		Incendiary,
		FL,
		Sensitive,
		Tiny,
		Small,
		Medium,
		Large,
	}*/
	public static partial class Utils
	{
		public static readonly string[] Strings =
		{
			"钢",
			"金",
			"银",
			"铅",
			"铂",
			"锌",
			"锡",
			"铬",
			"钛",
			"镍",
			"铝",
			"铀",
			"磷",
			"铁",
			"铜",
			"汞",
			"锗",
			"Fe-Al-Cr合金",
			"塑料",
		};
		public static Dictionary<string, string> TR = new();
		/// <summary>
		/// 读取键值对文件
		/// </summary>
		/// <param name="dict"></param>
		/// <param name="stream">要读取的流</param>
		/// <param name="separator">分隔符</param>
		/// <param name="commentchar">注释符</param>
		public static void ReadKeyValueFile(Dictionary<string, string> dict, Stream stream, char separator = '=', char commentchar = '#')
		{
			var reader = new StreamReader(stream);
			while (true)
			{
				var line = reader.ReadLine();
				if (line == null) return;
				if (line[0] != commentchar)
				{
					int i = line.IndexOf(separator);
					if (i >= 0)
						dict[line.Substring(0, i)] = line.Substring(i + 1);
				}
			}
		}
		[MethodImpl((MethodImplOptions)0x100)]
		public static string Get(string s) => s != null && TR.TryGetValue(s, out string result) ? result : s;
		public static Stream GetTargetFile(string name, bool throwIfNotFound = true)
		{
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (c.EndsWith(Storage.GetExtension(name))) {
                    if(string.Equals(c, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read);
                    }
                }
            }
            if (throwIfNotFound)
                throw new InvalidOperationException(name + " not found.");
            return null;
#if false
      			for (var enumerator = ModsManager.GetEntries(Storage.GetExtension(name)).GetEnumerator(); enumerator.MoveNext();)
				if (string.Equals(Path.GetFileName(enumerator.Current.Filename), name, StringComparison.OrdinalIgnoreCase))
					return enumerator.Current.Stream;
			if (throwIfNotFound)
				throw new InvalidOperationException(name + " not found.");
			return null;
#endif

		}
		[MethodImpl((MethodImplOptions)0x100)]
		public static string ToStr(this Materials m)
		{
			return Strings[(int)m];
		}
	}
}