using System;
using System.Collections.Generic;
using System.IO;

namespace Gibbed.IO
{
	// Token: 0x02000006 RID: 6
	public static class PathHelper
	{
		// Token: 0x0600003D RID: 61 RVA: 0x000024C0 File Offset: 0x000006C0
		public static string GetRelativePath(string fromPath, string toPath)
		{
			if (fromPath == null)
			{
				throw new ArgumentNullException("fromPath");
			}
			if (toPath == null)
			{
				throw new ArgumentNullException("toPath");
			}
			if (Path.IsPathRooted(fromPath) && Path.IsPathRooted(toPath) && string.Compare(Path.GetPathRoot(fromPath), Path.GetPathRoot(toPath), StringComparison.OrdinalIgnoreCase) != 0)
			{
				return null;
			}
			List<string> list = new List<string>();
			string[] array = fromPath.Split(new char[]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			});
			string[] array2 = toPath.Split(new char[]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			});
			int num = Math.Min(array.Length, array2.Length);
			int num2 = -1;
			int num3 = 0;
			while (num3 < num && string.Compare(array[num3], array2[num3], StringComparison.OrdinalIgnoreCase) == 0)
			{
				num2 = num3;
				num3++;
			}
			if (num2 == -1)
			{
				return toPath;
			}
			for (int i = num2 + 1; i < array.Length; i++)
			{
				if (array[i].Length > 0)
				{
					list.Add("..");
				}
			}
			for (int j = num2 + 1; j < array2.Length; j++)
			{
				list.Add(array2[j]);
			}
			return Path.Combine(list.ToArray());
		}
	}
}
