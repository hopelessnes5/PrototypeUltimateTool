using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004A RID: 74
	[TypeConverter(typeof(BaseNodeConverter))]
	public abstract class BaseNode : ISerializable, ICloneable
	{
		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x000083E8 File Offset: 0x000065E8
		[DisplayName("Type ID")]
		[Category("Pure3D")]
		[TypeConverter(typeof(TypeIdConverter))]
		public virtual uint TypeId
		{
			get
			{
				object[] customAttributes = base.GetType().GetCustomAttributes(typeof(KnownTypeAttribute), false);
				if (customAttributes.Length == 1)
				{
					return ((KnownTypeAttribute)customAttributes[0]).Id;
				}
				return this.ID;
			}
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x00008426 File Offset: 0x00006626
		public override string ToString()
		{
			return base.GetType().Name;
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060002BA RID: 698 RVA: 0x00008433 File Offset: 0x00006633
		// (set) Token: 0x060002BB RID: 699 RVA: 0x0000843B File Offset: 0x0000663B
		[Category("Pure3D")]
		[Display(Order = 0)]
		[ReadOnly(true)]
		public uint StartPosition { get; set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060002BC RID: 700 RVA: 0x00008444 File Offset: 0x00006644
		// (set) Token: 0x060002BD RID: 701 RVA: 0x0000844C File Offset: 0x0000664C
		[Category("Pure3D")]
		[Display(Order = 0)]
		[ReadOnly(true)]
		public uint HeaderSize { get; set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060002BE RID: 702 RVA: 0x00008455 File Offset: 0x00006655
		// (set) Token: 0x060002BF RID: 703 RVA: 0x0000845D File Offset: 0x0000665D
		[Category("Pure3D")]
		[Display(Order = 0)]
		[ReadOnly(true)]
		public uint TotalSize { get; set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060002C0 RID: 704 RVA: 0x00008466 File Offset: 0x00006666
		// (set) Token: 0x060002C1 RID: 705 RVA: 0x0000846E File Offset: 0x0000666E
		[Browsable(false)]
		public PrototypeGame Game { get; set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x00008477 File Offset: 0x00006677
		[Browsable(false)]
		public int ChildCount
		{
			get
			{
				if (this.Children == null)
				{
					return 0;
				}
				return this.Children.Count;
			}
		}

		// Token: 0x060002C3 RID: 707
		public abstract void Serialize(Stream output, Endian endian);

		// Token: 0x060002C4 RID: 708
		public abstract void Deserialize(Stream input, Endian endian);

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060002C5 RID: 709 RVA: 0x0000848E File Offset: 0x0000668E
		[Browsable(false)]
		public virtual bool Exportable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060002C6 RID: 710 RVA: 0x00008491 File Offset: 0x00006691
		[Browsable(false)]
		public virtual string ExportExtension
		{
			get
			{
				return "";
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00008498 File Offset: 0x00006698
		public virtual void Export(Stream output)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060002C8 RID: 712 RVA: 0x0000848E File Offset: 0x0000668E
		[Browsable(false)]
		public virtual bool Importable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00008498 File Offset: 0x00006698
		public virtual void Import(Stream input)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x060002CA RID: 714 RVA: 0x0000848E File Offset: 0x0000668E
		[Browsable(false)]
		public virtual bool Runnable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x060002CB RID: 715 RVA: 0x0000848E File Offset: 0x0000668E
		[Browsable(false)]
		public virtual BaseNode.RunToolType RunTool
		{
			get
			{
				return BaseNode.RunToolType.None;
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000849F File Offset: 0x0000669F
		public virtual BaseNode RunNode()
		{
			return this;
		}

		// Token: 0x060002CD RID: 717 RVA: 0x000084A2 File Offset: 0x000066A2
		public virtual object Preview()
		{
			return null;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x000084A8 File Offset: 0x000066A8
		public T GetParentNode<T>() where T : BaseNode
		{
			BaseNode parentNode = this.ParentNode;
			if (parentNode is T)
			{
				return parentNode as T;
			}
			if (parentNode != null)
			{
				return parentNode.GetParentNode<T>();
			}
			return default(T);
		}

		// Token: 0x060002CF RID: 719 RVA: 0x000084E3 File Offset: 0x000066E3
		public T GetChildNode<T>() where T : BaseNode
		{
			return (T)((object)this.Children.SingleOrDefault((BaseNode candidate) => candidate is T));
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x00008514 File Offset: 0x00006714
		public List<T> GetChildNodes<T>() where T : BaseNode
		{
			return new List<T>(this.Children.OfType<T>());
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00008528 File Offset: 0x00006728
		public List<T> GetChildNodesRecursive<T>() where T : BaseNode
		{
			List<T> childNodes = this.GetChildNodes<T>();
			foreach (BaseNode baseNode in this.Children)
			{
				List<T> childNodesRecursive = baseNode.GetChildNodesRecursive<T>();
				childNodes.AddRange(childNodesRecursive);
			}
			return childNodes;
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x00008588 File Offset: 0x00006788
		public object Clone()
		{
			Pure3DFile pure3DFile = new Pure3DFile();
			pure3DFile.Endian = Endian.Little;
			pure3DFile.GamePicked = this.Game;
			pure3DFile.Nodes.Add(this);
			Stream stream = new MemoryStream();
			pure3DFile.Serialize(stream, Endian.Little);
			stream.Position = 0L;
			pure3DFile.Nodes.Clear();
			pure3DFile.Deserialize(stream, Endian.Little);
			return pure3DFile.Nodes[0];
		}

		// Token: 0x0400011E RID: 286
		public uint ID = uint.MaxValue;

		// Token: 0x0400011F RID: 287
		public BaseNode ParentNode;

		// Token: 0x04000120 RID: 288
		public List<BaseNode> Children = new List<BaseNode>();

		// Token: 0x0200008F RID: 143
		public enum RunToolType
		{
			// Token: 0x04000214 RID: 532
			None,
			// Token: 0x04000215 RID: 533
			TodEditor
		}
	}
}
