using System;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000007 RID: 7
	public static class Constants
	{
		// Token: 0x0400000F RID: 15
		public const int BYTES_PER_FACE = 6;

		// Token: 0x04000010 RID: 16
		public const int BYTES_PER_VERTEXPOSITION = 12;

		// Token: 0x04000011 RID: 17
		public const int SIZE_OF_BONE_MATRIX = 118;

		// Token: 0x04000012 RID: 18
		public const int BONE_MATRIX_WIDTH = 4;

		// Token: 0x04000013 RID: 19
		public const int BONE_MATRIX_HEIGHT = 4;

		// Token: 0x04000014 RID: 20
		public const int DESCRIPTION_SIZE = 17;

		// Token: 0x04000015 RID: 21
		public const uint P1_TYPE_UV = 3556617U;

		// Token: 0x04000016 RID: 22
		public const uint P1_TYPE_POSITION = 747804969U;

		// Token: 0x04000017 RID: 23
		public const uint P1_TYPE_NORMAL = 3255221479U;

		// Token: 0x04000018 RID: 24
		public const uint P1_TYPE_TANGENT = 2752995909U;

		// Token: 0x04000019 RID: 25
		public const uint P1_TYPE_WEIGHT = 1230441723U;

		// Token: 0x0400001A RID: 26
		public const uint P1_TYPE_GROUP = 1943391143U;

		// Token: 0x0400001B RID: 27
		public const uint P1_TYPE_UVPAD1 = 949550084U;

		// Token: 0x0400001C RID: 28
		public const string P2_TYPE_UV = "tex0";

		// Token: 0x0400001D RID: 29
		public const string P2_TYPE_POSITION = "position";

		// Token: 0x0400001E RID: 30
		public const string P2_TYPE_NORMAL = "normal";

		// Token: 0x0400001F RID: 31
		public const string P2_TYPE_WEIGHTS = "weights";

		// Token: 0x04000020 RID: 32
		public const string P2_TYPE_TANGENT = "tangent";

		// Token: 0x04000021 RID: 33
		public const string P2_TYPE_FACES = "indices";

		// Token: 0x04000022 RID: 34
		public const string P2_TYPE_INDICES = "indices";

		// Token: 0x04000023 RID: 35
		public const string P2_TYPE_COLOUR0 = "colour0";

		// Token: 0x04000024 RID: 36
		public const string P2_TYPE_PAD = "pad";

		// Token: 0x04000025 RID: 37
		public const string P2_TYPE_UV1 = "tex1";

		// Token: 0x04000026 RID: 38
		public const string P2_TYPE_COEFFS0 = "coeffs0";

		// Token: 0x04000027 RID: 39
		public const string P2_TYPE_COEFFS1 = "coeffs1";

		// Token: 0x04000028 RID: 40
		public const string P2_TYPE_COEFFS2 = "coeffs2";

		// Token: 0x04000029 RID: 41
		public const string P2_TYPE_COEFFS3 = "coeffs3";

		// Token: 0x0400002A RID: 42
		public const string P2_TYPE_DIFFCOEFFS0 = "diffcoeffs0";

		// Token: 0x0400002B RID: 43
		public const string P2_TYPE_DIFFCOEFFS1 = "diffcoeffs1";

		// Token: 0x0400002C RID: 44
		public const string P2_TYPE_DIFFCOEFFS2 = "diffcoeffs2";

		// Token: 0x0400002D RID: 45
		public const string P2_TYPE_DIFFCOEFFS3 = "diffcoeffs3";

		// Token: 0x0400002E RID: 46
		public const float COMPRESSION_FACTOR3 = 127f;

		// Token: 0x0400002F RID: 47
		public const float DECOMPRESSION_FACTOR3 = 0.007874016f;

		// Token: 0x04000030 RID: 48
		public const float COMPRESSION_FACTOR6 = 32767f;

		// Token: 0x04000031 RID: 49
		public const float DECOMPRESSION_FACTOR6 = 3.051851E-05f;

		// Token: 0x04000032 RID: 50
		public static readonly int[][] STATIC_INDEX = new int[][]
		{
			new int[]
			{
				1,
				2
			},
			new int[]
			{
				0,
				2
			},
			new int[]
			{
				0,
				1
			}
		};

		// Token: 0x04000033 RID: 51
		public const string ANIM_SCALE = "SCL\0";

		// Token: 0x04000034 RID: 52
		public const string ANIM_TRANSLATION = "TRAN";

		// Token: 0x04000035 RID: 53
		public const string ANIM_ROTATION = "ROT\0";

		// Token: 0x04000036 RID: 54
		public const int P1TodConstant = 916723515;

		// Token: 0x04000037 RID: 55
		public const int P2TodConstant = 217161721;
	}
}
