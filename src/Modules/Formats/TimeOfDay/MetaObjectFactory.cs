using System;
using System.Collections.Generic;
using System.Reflection;
using Nixson.Common;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000D RID: 13
	public static class MetaObjectFactory
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00002486 File Offset: 0x00000686
		public static Dictionary<string, Type> AttributeLookup
		{
			get
			{
				if (!MetaObjectFactory.IsLookupBuild)
				{
					MetaObjectFactory.BuildLookup();
				}
				return MetaObjectFactory._attributeLookup;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00002499 File Offset: 0x00000699
		private static bool IsLookupBuild
		{
			get
			{
				return MetaObjectFactory._p1Lookup != null && MetaObjectFactory._p2Lookup != null && MetaObjectFactory._attributeLookup != null;
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000024B4 File Offset: 0x000006B4
		private static void BuildLookup()
		{
			MetaObjectFactory._p1Lookup = new Dictionary<string, Type>();
			MetaObjectFactory._p2Lookup = new Dictionary<string, Type>();
			foreach (Type type in Assembly.GetAssembly(typeof(MetaObjectFactory)).GetTypes())
			{
				if (type.IsSubclassOf(typeof(MetaObject)))
				{
					TodAttribute todAttribute = (TodAttribute)type.GetCustomAttribute(typeof(TodAttribute), false);
					if (todAttribute != null)
					{
						switch (todAttribute.Game)
						{
						case PrototypeGame.P1:
							MetaObjectFactory._p1Lookup.Add(todAttribute.Name, type);
							break;
						case PrototypeGame.P2:
							MetaObjectFactory._p2Lookup.Add(todAttribute.Name, type);
							break;
						case PrototypeGame.Any:
							MetaObjectFactory._p1Lookup.Add(todAttribute.Name, type);
							MetaObjectFactory._p2Lookup.Add(todAttribute.Name, type);
							break;
						}
					}
				}
			}
			MetaObjectFactory._attributeLookup = new Dictionary<string, Type>
			{
				{
					"BoolAttribute",
					typeof(BoolAttribute)
				},
				{
					"FactionAttribute",
					typeof(FactionAttribute)
				},
				{
					"FloatAttribute",
					typeof(FloatAttribute)
				},
				{
					"IntAttribute",
					typeof(IntAttribute)
				},
				{
					"MatrixAttribute",
					typeof(MatrixAttribute)
				},
				{
					"NameAttribute",
					typeof(NameAttribute)
				},
				{
					"engine::PoseAttribute",
					typeof(PoseAttribute)
				},
				{
					"VectorAttribute",
					typeof(VectorAttribute)
				}
			};
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002648 File Offset: 0x00000848
		public static MetaObject CreateTod(string name, PrototypeGame game)
		{
			if (!MetaObjectFactory.IsLookupBuild)
			{
				MetaObjectFactory.BuildLookup();
			}
			Type type;
			if (game != PrototypeGame.P1)
			{
				if (game != PrototypeGame.P2)
				{
					if (!MetaObjectFactory._p1Lookup.TryGetValue(name, out type))
					{
						MetaObjectFactory._p2Lookup.TryGetValue(name, out type);
					}
				}
				else
				{
					MetaObjectFactory._p2Lookup.TryGetValue(name, out type);
				}
			}
			else
			{
				MetaObjectFactory._p1Lookup.TryGetValue(name, out type);
			}
			if (type == null)
			{
				return null;
			}
			MetaObject result;
			try
			{
				result = (MetaObject)Activator.CreateInstance(type);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			return result;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000026D8 File Offset: 0x000008D8
		public static MetaAttribute CreateAttribute(string name)
		{
			if (!MetaObjectFactory.IsLookupBuild)
			{
				MetaObjectFactory.BuildLookup();
			}
			MetaAttribute result;
			try
			{
				result = (MetaAttribute)Activator.CreateInstance(MetaObjectFactory._attributeLookup[name]);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			return result;
		}

		// Token: 0x04000010 RID: 16
		private static Dictionary<string, Type> _p1Lookup;

		// Token: 0x04000011 RID: 17
		private static Dictionary<string, Type> _p2Lookup;

		// Token: 0x04000012 RID: 18
		public static Dictionary<string, Type> _attributeLookup;
	}
}
