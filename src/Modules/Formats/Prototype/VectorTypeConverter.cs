using System;
using System.ComponentModel;
using System.Globalization;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000017 RID: 23
	public class VectorTypeConverter : TypeConverter
	{
		// Token: 0x060000A7 RID: 167 RVA: 0x000030F2 File Offset: 0x000012F2
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00003CD0 File Offset: 0x00001ED0
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				Vector2 vector = value as Vector2;
				if (vector != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Vector2 (X={0:0.000000}, Y={1:0.000000})", vector.X, vector.Y);
				}
				Vector3 vector2 = value as Vector3;
				if (vector2 != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Vector3 (X={0:0.000000}, Y={1:0.000000}, Z={2:0.000000})", vector2.X, vector2.Y, vector2.Z);
				}
				Vector3Int vector3Int = value as Vector3Int;
				if (vector3Int != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Vector3Int (X={0:0}, Y={1:0}, Z={2:0})", vector3Int.X, vector3Int.Y, vector3Int.Z);
				}
				Vector4 vector3 = value as Vector4;
				if (vector3 != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Vector4 (X={0:0.000000}, Y={1:0.000000}, Z={2:0.000000}, W={3:0.000000})", new object[]
					{
						vector3.X,
						vector3.Y,
						vector3.Z,
						vector3.W
					});
				}
				Vector4Int vector4Int = value as Vector4Int;
				if (vector4Int != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Vector4Int (X={0:0.000000}, Y={1:0.000000}, Z={2:0.000000}, W={3:0.000000})", new object[]
					{
						vector4Int.X,
						vector4Int.Y,
						vector4Int.Z,
						vector4Int.W
					});
				}
				Weight weight = value as Weight;
				if (weight != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Weight (X={0:0.000000}, Y={1:0.000000}, Z={2:0.000000})", weight.X, weight.Y, weight.Z);
				}
				UVCoordinate uvcoordinate = value as UVCoordinate;
				if (uvcoordinate != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "UV Coordinates (U={0:0.000000}, V={1:0.000000})", uvcoordinate.U, uvcoordinate.V);
				}
				if (value is Face)
				{
					return string.Format(CultureInfo.InvariantCulture, "Face", Array.Empty<object>());
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00003EEC File Offset: 0x000020EC
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptor[] properties = null;
			if (value is Vector2)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Vector2), typeof(float), "X"),
					new VectorTypeConverter.Descriptor(typeof(Vector2), typeof(float), "Y")
				};
				properties = array;
			}
			else if (value is Vector3)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Vector3), typeof(float), "X"),
					new VectorTypeConverter.Descriptor(typeof(Vector3), typeof(float), "Y"),
					new VectorTypeConverter.Descriptor(typeof(Vector3), typeof(float), "Z")
				};
				properties = array;
			}
			else if (value is Vector3Int)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Vector3Int), typeof(uint), "X"),
					new VectorTypeConverter.Descriptor(typeof(Vector3Int), typeof(uint), "Y"),
					new VectorTypeConverter.Descriptor(typeof(Vector3Int), typeof(uint), "Z")
				};
				properties = array;
			}
			else if (value is Vector4)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Vector4), typeof(float), "X"),
					new VectorTypeConverter.Descriptor(typeof(Vector4), typeof(float), "Y"),
					new VectorTypeConverter.Descriptor(typeof(Vector4), typeof(float), "Z"),
					new VectorTypeConverter.Descriptor(typeof(Vector4), typeof(float), "W")
				};
				properties = array;
			}
			else if (value is Vector4Int)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Vector4Int), typeof(uint), "X"),
					new VectorTypeConverter.Descriptor(typeof(Vector4Int), typeof(uint), "Y"),
					new VectorTypeConverter.Descriptor(typeof(Vector4Int), typeof(uint), "Z"),
					new VectorTypeConverter.Descriptor(typeof(Vector4Int), typeof(uint), "W")
				};
				properties = array;
			}
			else if (value is Weight)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Weight), typeof(float), "X"),
					new VectorTypeConverter.Descriptor(typeof(Weight), typeof(float), "Y"),
					new VectorTypeConverter.Descriptor(typeof(Weight), typeof(float), "Z")
				};
				properties = array;
			}
			else if (value is UVCoordinate)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(UVCoordinate), typeof(float), "U"),
					new VectorTypeConverter.Descriptor(typeof(UVCoordinate), typeof(float), "V")
				};
				properties = array;
			}
			else if (value is Face)
			{
				PropertyDescriptor[] array = new VectorTypeConverter.Descriptor[]
				{
					new VectorTypeConverter.Descriptor(typeof(Face), typeof(uint), "Point 1"),
					new VectorTypeConverter.Descriptor(typeof(Face), typeof(string), "Point 2"),
					new VectorTypeConverter.Descriptor(typeof(Face), typeof(byte[]), "Point 3")
				};
				properties = array;
			}
			return new PropertyDescriptorCollection(properties);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00003219 File Offset: 0x00001419
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		// Token: 0x0200008D RID: 141
		private class Descriptor : TypeConverter.SimplePropertyDescriptor
		{
			// Token: 0x060005A5 RID: 1445 RVA: 0x0000C7B7 File Offset: 0x0000A9B7
			public Descriptor(Type componentType, Type elementType, string name) : base(componentType, name, elementType, null)
			{
				this._Name = name;
			}

			// Token: 0x060005A6 RID: 1446 RVA: 0x0000C7CC File Offset: 0x0000A9CC
			public override object GetValue(object instance)
			{
				Vector2 vector = instance as Vector2;
				if (vector != null)
				{
					string name = this._Name;
					if (name == "X")
					{
						return vector.X.ToString("0.000000", CultureInfo.InvariantCulture);
					}
					if (!(name == "Y"))
					{
						return null;
					}
					return vector.Y.ToString("0.000000", CultureInfo.InvariantCulture);
				}
				else
				{
					Vector3 vector2 = instance as Vector3;
					if (vector2 != null)
					{
						string name = this._Name;
						if (name == "X")
						{
							return vector2.X.ToString("0.000000", CultureInfo.InvariantCulture);
						}
						if (name == "Y")
						{
							return vector2.Y.ToString("0.000000", CultureInfo.InvariantCulture);
						}
						if (!(name == "Z"))
						{
							return null;
						}
						return vector2.Z.ToString("0.000000", CultureInfo.InvariantCulture);
					}
					else
					{
						Vector3Int vector3Int = instance as Vector3Int;
						if (vector3Int != null)
						{
							string name = this._Name;
							if (name == "X")
							{
								return vector3Int.X.ToString("0", CultureInfo.InvariantCulture);
							}
							if (name == "Y")
							{
								return vector3Int.Y.ToString("0", CultureInfo.InvariantCulture);
							}
							if (!(name == "Z"))
							{
								return null;
							}
							return vector3Int.Z.ToString("0", CultureInfo.InvariantCulture);
						}
						else
						{
							Vector4 vector3 = instance as Vector4;
							if (vector3 != null)
							{
								string name = this._Name;
								if (name == "X")
								{
									return vector3.X.ToString("0.000000", CultureInfo.InvariantCulture);
								}
								if (name == "Y")
								{
									return vector3.Y.ToString("0.000000", CultureInfo.InvariantCulture);
								}
								if (name == "Z")
								{
									return vector3.Z.ToString("0.000000", CultureInfo.InvariantCulture);
								}
								if (!(name == "W"))
								{
									return null;
								}
								return vector3.W.ToString("0.000000", CultureInfo.InvariantCulture);
							}
							else
							{
								Vector4Int vector4Int = instance as Vector4Int;
								if (vector4Int != null)
								{
									string name = this._Name;
									if (name == "X")
									{
										return vector4Int.X.ToString("0", CultureInfo.InvariantCulture);
									}
									if (name == "Y")
									{
										return vector4Int.Y.ToString("0", CultureInfo.InvariantCulture);
									}
									if (name == "Z")
									{
										return vector4Int.Z.ToString("0", CultureInfo.InvariantCulture);
									}
									if (!(name == "W"))
									{
										return null;
									}
									return vector4Int.W.ToString("0", CultureInfo.InvariantCulture);
								}
								else
								{
									Weight weight = instance as Weight;
									if (weight != null)
									{
										string name = this._Name;
										if (name == "X")
										{
											return weight.X.ToString("0.000000", CultureInfo.InvariantCulture);
										}
										if (name == "Y")
										{
											return weight.Y.ToString("0.000000", CultureInfo.InvariantCulture);
										}
										if (!(name == "Z"))
										{
											return null;
										}
										return weight.Z.ToString("0.000000", CultureInfo.InvariantCulture);
									}
									else
									{
										UVCoordinate uvcoordinate = instance as UVCoordinate;
										if (uvcoordinate != null)
										{
											string name = this._Name;
											if (name == "U")
											{
												return uvcoordinate.U.ToString("0.000000", CultureInfo.InvariantCulture);
											}
											if (!(name == "V"))
											{
												return null;
											}
											return uvcoordinate.V.ToString("0.000000", CultureInfo.InvariantCulture);
										}
										else
										{
											Face face = instance as Face;
											if (face == null)
											{
												return null;
											}
											string name = this._Name;
											if (name == "Point 1")
											{
												return face.Point1.ToString("0", CultureInfo.InvariantCulture);
											}
											if (name == "Point 2")
											{
												return face.Point2.ToString("0", CultureInfo.InvariantCulture);
											}
											if (!(name == "Point 3"))
											{
												return null;
											}
											return face.Point3.ToString("0", CultureInfo.InvariantCulture);
										}
									}
								}
							}
						}
					}
				}
			}

			// Token: 0x170001D1 RID: 465
			// (get) Token: 0x060005A7 RID: 1447 RVA: 0x00003219 File Offset: 0x00001419
			public override bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			// Token: 0x060005A8 RID: 1448 RVA: 0x0000A320 File Offset: 0x00008520
			public override void SetValue(object instance, object value)
			{
			}

			// Token: 0x0400020F RID: 527
			private readonly string _Name;
		}
	}
}
