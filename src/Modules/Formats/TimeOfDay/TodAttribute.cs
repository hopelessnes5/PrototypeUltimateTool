using System;
using Nixson.Common;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000E RID: 14
	[AttributeUsage(AttributeTargets.Class)]
	public class TodAttribute : Attribute
	{
		// Token: 0x06000047 RID: 71 RVA: 0x00002724 File Offset: 0x00000924
		public TodAttribute(string name)
		{
			this.Name = name;
			this.Game = PrototypeGame.Any;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x0000273A File Offset: 0x0000093A
		public TodAttribute(string name, PrototypeGame game)
		{
			this.Name = name;
			this.Game = game;
		}

		// Token: 0x04000013 RID: 19
		public string Name;

		// Token: 0x04000014 RID: 20
		public PrototypeGame Game;
	}
}
