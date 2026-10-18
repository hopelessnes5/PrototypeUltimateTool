using System;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000015 RID: 21
	public enum Unlockable
	{
		// Token: 0x04000058 RID: 88
		None,
		// Token: 0x04000059 RID: 89
		Powers_Begin,
		// Token: 0x0400005A RID: 90
		Claws,
		// Token: 0x0400005B RID: 91
		MuscleMass,
		// Token: 0x0400005C RID: 92
		HammerFist,
		// Token: 0x0400005D RID: 93
		Blades,
		// Token: 0x0400005E RID: 94
		Shield,
		// Token: 0x0400005F RID: 95
		Armour,
		// Token: 0x04000060 RID: 96
		ChameleonicSkin,
		// Token: 0x04000061 RID: 97
		ThermalVision,
		// Token: 0x04000062 RID: 98
		InfectedVision,
		// Token: 0x04000063 RID: 99
		Disguise,
		// Token: 0x04000064 RID: 100
		Powers_End,
		// Token: 0x04000065 RID: 101
		Upgrades_Begin,
		// Token: 0x04000066 RID: 102
		Health1,
		// Token: 0x04000067 RID: 103
		Health2,
		// Token: 0x04000068 RID: 104
		Health3,
		// Token: 0x04000069 RID: 105
		CriticalMass1,
		// Token: 0x0400006A RID: 106
		CriticalMass2,
		// Token: 0x0400006B RID: 107
		CriticalMass3,
		// Token: 0x0400006C RID: 108
		AdrenalineSurge,
		// Token: 0x0400006D RID: 109
		Jump1,
		// Token: 0x0400006E RID: 110
		Jump2,
		// Token: 0x0400006F RID: 111
		Jump3,
		// Token: 0x04000070 RID: 112
		Jump4,
		// Token: 0x04000071 RID: 113
		Sprint1,
		// Token: 0x04000072 RID: 114
		Sprint2,
		// Token: 0x04000073 RID: 115
		Sprint3,
		// Token: 0x04000074 RID: 116
		StealthConsume,
		// Token: 0x04000075 RID: 117
		StealthConsumeFast,
		// Token: 0x04000076 RID: 118
		ConsumeExtraHealth1,
		// Token: 0x04000077 RID: 119
		ConsumeExtraHealth2,
		// Token: 0x04000078 RID: 120
		RegenerationRate1,
		// Token: 0x04000079 RID: 121
		RegenerationRate2,
		// Token: 0x0400007A RID: 122
		RegenerationDelay1,
		// Token: 0x0400007B RID: 123
		RegenerationDelay2,
		// Token: 0x0400007C RID: 124
		WallLatch,
		// Token: 0x0400007D RID: 125
		DiveRoll,
		// Token: 0x0400007E RID: 126
		Upgrades_End,
		// Token: 0x0400007F RID: 127
		Abilities_Begin,
		// Token: 0x04000080 RID: 128
		JackArmouredVehicle,
		// Token: 0x04000081 RID: 129
		JackHelicopter,
		// Token: 0x04000082 RID: 130
		ArmouredVehicle1,
		// Token: 0x04000083 RID: 131
		ArmouredVehicle2,
		// Token: 0x04000084 RID: 132
		ArmouredVehicle3,
		// Token: 0x04000085 RID: 133
		Helicopter1,
		// Token: 0x04000086 RID: 134
		Helicopter2,
		// Token: 0x04000087 RID: 135
		Helicopter3,
		// Token: 0x04000088 RID: 136
		ArtilleryStrike1,
		// Token: 0x04000089 RID: 137
		ArtilleryStrike2,
		// Token: 0x0400008A RID: 138
		ArtilleryStrike3,
		// Token: 0x0400008B RID: 139
		ArtilleryStrike4,
		// Token: 0x0400008C RID: 140
		ArtilleryStrike5,
		// Token: 0x0400008D RID: 141
		ArtilleryStrike6,
		// Token: 0x0400008E RID: 142
		Javelin1,
		// Token: 0x0400008F RID: 143
		Javelin2,
		// Token: 0x04000090 RID: 144
		Javelin3,
		// Token: 0x04000091 RID: 145
		GrenadeLauncher1,
		// Token: 0x04000092 RID: 146
		GrenadeLauncher2,
		// Token: 0x04000093 RID: 147
		GrenadeLauncher3,
		// Token: 0x04000094 RID: 148
		SAW1,
		// Token: 0x04000095 RID: 149
		SAW2,
		// Token: 0x04000096 RID: 150
		SAW3,
		// Token: 0x04000097 RID: 151
		Rifle1,
		// Token: 0x04000098 RID: 152
		Rifle2,
		// Token: 0x04000099 RID: 153
		Rifle3,
		// Token: 0x0400009A RID: 154
		Abilities_End,
		// Token: 0x0400009B RID: 155
		Moves_Begin,
		// Token: 0x0400009C RID: 156
		GroundPound,
		// Token: 0x0400009D RID: 157
		AirGroundPound,
		// Token: 0x0400009E RID: 158
		GroundSpikeDevastator,
		// Token: 0x0400009F RID: 159
		AirGroundSpikeDevastator,
		// Token: 0x040000A0 RID: 160
		SonicClap,
		// Token: 0x040000A1 RID: 161
		AirSonicClap,
		// Token: 0x040000A2 RID: 162
		TendrilDevastator,
		// Token: 0x040000A3 RID: 163
		AirTendrilDevastator,
		// Token: 0x040000A4 RID: 164
		PalmStrike,
		// Token: 0x040000A5 RID: 165
		Cannonball,
		// Token: 0x040000A6 RID: 166
		CriticalPainDevastator,
		// Token: 0x040000A7 RID: 167
		AirCriticalPainDevastator,
		// Token: 0x040000A8 RID: 168
		ElbowDrop,
		// Token: 0x040000A9 RID: 169
		GroundStomp,
		// Token: 0x040000AA RID: 170
		FlyingKickLong,
		// Token: 0x040000AB RID: 171
		FlyingKickCombo,
		// Token: 0x040000AC RID: 172
		FlyingKickComboSpecial,
		// Token: 0x040000AD RID: 173
		Uppercut,
		// Token: 0x040000AE RID: 174
		UppercutKick,
		// Token: 0x040000AF RID: 175
		AirCombo,
		// Token: 0x040000B0 RID: 176
		AirComboSpike,
		// Token: 0x040000B1 RID: 177
		AirComboGrappleBounce,
		// Token: 0x040000B2 RID: 178
		AirDash,
		// Token: 0x040000B3 RID: 179
		AirDashLong,
		// Token: 0x040000B4 RID: 180
		AirDash2,
		// Token: 0x040000B5 RID: 181
		AirDash2Long,
		// Token: 0x040000B6 RID: 182
		AirRecover,
		// Token: 0x040000B7 RID: 183
		SkydiveHover,
		// Token: 0x040000B8 RID: 184
		BulletDive,
		// Token: 0x040000B9 RID: 185
		RunningGrab,
		// Token: 0x040000BA RID: 186
		RunningThrow,
		// Token: 0x040000BB RID: 187
		ThrowCharged,
		// Token: 0x040000BC RID: 188
		ThrowLongRange,
		// Token: 0x040000BD RID: 189
		GrappleSlam,
		// Token: 0x040000BE RID: 190
		GrappleGrind,
		// Token: 0x040000BF RID: 191
		PropDropKick,
		// Token: 0x040000C0 RID: 192
		ProneStomp,
		// Token: 0x040000C1 RID: 193
		BrawlerRiding,
		// Token: 0x040000C2 RID: 194
		ClawsUppercut,
		// Token: 0x040000C3 RID: 195
		ClawsDashAttack,
		// Token: 0x040000C4 RID: 196
		GroundSpike,
		// Token: 0x040000C5 RID: 197
		GroundSpikeMedium,
		// Token: 0x040000C6 RID: 198
		GroundSpikeLarge,
		// Token: 0x040000C7 RID: 199
		GroundSpikeRepeater,
		// Token: 0x040000C8 RID: 200
		BladesSpecialCombo,
		// Token: 0x040000C9 RID: 201
		BladesDashAttack,
		// Token: 0x040000CA RID: 202
		BladesLunge,
		// Token: 0x040000CB RID: 203
		BladesAirStab,
		// Token: 0x040000CC RID: 204
		MuscleMassDamageIncrease,
		// Token: 0x040000CD RID: 205
		MuscleMassThrowIncrease,
		// Token: 0x040000CE RID: 206
		BrawlerThrow,
		// Token: 0x040000CF RID: 207
		HammerFistSpecialCombo,
		// Token: 0x040000D0 RID: 208
		HammerToss,
		// Token: 0x040000D1 RID: 209
		HammerFistElbowDrop,
		// Token: 0x040000D2 RID: 210
		LongShot,
		// Token: 0x040000D3 RID: 211
		LongShotStrike,
		// Token: 0x040000D4 RID: 212
		Patsy,
		// Token: 0x040000D5 RID: 213
		PatsyLongRange,
		// Token: 0x040000D6 RID: 214
		SupermanPunch,
		// Token: 0x040000D7 RID: 215
		Moves_End,
		// Token: 0x040000D8 RID: 216
		Tutorial_Begin,
		// Token: 0x040000D9 RID: 217
		Move1,
		// Token: 0x040000DA RID: 218
		JumpCharged,
		// Token: 0x040000DB RID: 219
		WallClimb,
		// Token: 0x040000DC RID: 220
		WallRun,
		// Token: 0x040000DD RID: 221
		Consume,
		// Token: 0x040000DE RID: 222
		Tutorial_End,
		// Token: 0x040000DF RID: 223
		Mission_Begin,
		// Token: 0x040000E0 RID: 224
		Mission000,
		// Token: 0x040000E1 RID: 225
		Mission001,
		// Token: 0x040000E2 RID: 226
		Mission002,
		// Token: 0x040000E3 RID: 227
		Mission003,
		// Token: 0x040000E4 RID: 228
		Mission004,
		// Token: 0x040000E5 RID: 229
		Mission005,
		// Token: 0x040000E6 RID: 230
		Mission006,
		// Token: 0x040000E7 RID: 231
		Mission007,
		// Token: 0x040000E8 RID: 232
		Mission008,
		// Token: 0x040000E9 RID: 233
		Mission009,
		// Token: 0x040000EA RID: 234
		Mission010,
		// Token: 0x040000EB RID: 235
		Mission011,
		// Token: 0x040000EC RID: 236
		Mission012,
		// Token: 0x040000ED RID: 237
		Mission013,
		// Token: 0x040000EE RID: 238
		Mission014,
		// Token: 0x040000EF RID: 239
		Mission015,
		// Token: 0x040000F0 RID: 240
		Mission016,
		// Token: 0x040000F1 RID: 241
		Mission017,
		// Token: 0x040000F2 RID: 242
		Mission018,
		// Token: 0x040000F3 RID: 243
		Mission019,
		// Token: 0x040000F4 RID: 244
		Mission020,
		// Token: 0x040000F5 RID: 245
		Mission021,
		// Token: 0x040000F6 RID: 246
		Mission022,
		// Token: 0x040000F7 RID: 247
		Mission023,
		// Token: 0x040000F8 RID: 248
		Mission024,
		// Token: 0x040000F9 RID: 249
		Mission025,
		// Token: 0x040000FA RID: 250
		Mission026,
		// Token: 0x040000FB RID: 251
		Mission027,
		// Token: 0x040000FC RID: 252
		Mission028,
		// Token: 0x040000FD RID: 253
		Mission029,
		// Token: 0x040000FE RID: 254
		Mission030,
		// Token: 0x040000FF RID: 255
		Mission031,
		// Token: 0x04000100 RID: 256
		Mission032,
		// Token: 0x04000101 RID: 257
		Mission033,
		// Token: 0x04000102 RID: 258
		Mission034,
		// Token: 0x04000103 RID: 259
		Mission035,
		// Token: 0x04000104 RID: 260
		Mission036,
		// Token: 0x04000105 RID: 261
		Mission037,
		// Token: 0x04000106 RID: 262
		Mission038,
		// Token: 0x04000107 RID: 263
		Mission039,
		// Token: 0x04000108 RID: 264
		Objective000,
		// Token: 0x04000109 RID: 265
		Objective001,
		// Token: 0x0400010A RID: 266
		Objective002,
		// Token: 0x0400010B RID: 267
		Objective003,
		// Token: 0x0400010C RID: 268
		Objective004,
		// Token: 0x0400010D RID: 269
		Objective005,
		// Token: 0x0400010E RID: 270
		Objective006,
		// Token: 0x0400010F RID: 271
		Objective007,
		// Token: 0x04000110 RID: 272
		Objective008,
		// Token: 0x04000111 RID: 273
		Objective009,
		// Token: 0x04000112 RID: 274
		Objective010,
		// Token: 0x04000113 RID: 275
		Objective011,
		// Token: 0x04000114 RID: 276
		Objective012,
		// Token: 0x04000115 RID: 277
		Objective013,
		// Token: 0x04000116 RID: 278
		Objective014,
		// Token: 0x04000117 RID: 279
		Objective015,
		// Token: 0x04000118 RID: 280
		Objective016,
		// Token: 0x04000119 RID: 281
		Objective017,
		// Token: 0x0400011A RID: 282
		Objective018,
		// Token: 0x0400011B RID: 283
		Objective019,
		// Token: 0x0400011C RID: 284
		Objective020,
		// Token: 0x0400011D RID: 285
		Objective021,
		// Token: 0x0400011E RID: 286
		Objective022,
		// Token: 0x0400011F RID: 287
		Objective023,
		// Token: 0x04000120 RID: 288
		Objective024,
		// Token: 0x04000121 RID: 289
		Objective025,
		// Token: 0x04000122 RID: 290
		Objective026,
		// Token: 0x04000123 RID: 291
		Objective027,
		// Token: 0x04000124 RID: 292
		Objective028,
		// Token: 0x04000125 RID: 293
		Objective029,
		// Token: 0x04000126 RID: 294
		Objective030,
		// Token: 0x04000127 RID: 295
		Objective031,
		// Token: 0x04000128 RID: 296
		Objective032,
		// Token: 0x04000129 RID: 297
		Objective033,
		// Token: 0x0400012A RID: 298
		Objective034,
		// Token: 0x0400012B RID: 299
		Objective035,
		// Token: 0x0400012C RID: 300
		Objective036,
		// Token: 0x0400012D RID: 301
		Objective037,
		// Token: 0x0400012E RID: 302
		Objective038,
		// Token: 0x0400012F RID: 303
		Objective039,
		// Token: 0x04000130 RID: 304
		Objective040,
		// Token: 0x04000131 RID: 305
		Objective041,
		// Token: 0x04000132 RID: 306
		Objective042,
		// Token: 0x04000133 RID: 307
		Objective043,
		// Token: 0x04000134 RID: 308
		Objective044,
		// Token: 0x04000135 RID: 309
		Objective045,
		// Token: 0x04000136 RID: 310
		Objective046,
		// Token: 0x04000137 RID: 311
		Objective047,
		// Token: 0x04000138 RID: 312
		Objective048,
		// Token: 0x04000139 RID: 313
		Objective049,
		// Token: 0x0400013A RID: 314
		Objective050,
		// Token: 0x0400013B RID: 315
		Objective051,
		// Token: 0x0400013C RID: 316
		Objective052,
		// Token: 0x0400013D RID: 317
		Objective053,
		// Token: 0x0400013E RID: 318
		Objective054,
		// Token: 0x0400013F RID: 319
		Objective055,
		// Token: 0x04000140 RID: 320
		Objective056,
		// Token: 0x04000141 RID: 321
		Objective057,
		// Token: 0x04000142 RID: 322
		Objective058,
		// Token: 0x04000143 RID: 323
		Objective059,
		// Token: 0x04000144 RID: 324
		Objective060,
		// Token: 0x04000145 RID: 325
		Objective061,
		// Token: 0x04000146 RID: 326
		Objective062,
		// Token: 0x04000147 RID: 327
		Objective063,
		// Token: 0x04000148 RID: 328
		Objective064,
		// Token: 0x04000149 RID: 329
		Objective065,
		// Token: 0x0400014A RID: 330
		Objective066,
		// Token: 0x0400014B RID: 331
		Objective067,
		// Token: 0x0400014C RID: 332
		Objective068,
		// Token: 0x0400014D RID: 333
		Objective069,
		// Token: 0x0400014E RID: 334
		Objective070,
		// Token: 0x0400014F RID: 335
		Objective071,
		// Token: 0x04000150 RID: 336
		Objective072,
		// Token: 0x04000151 RID: 337
		Objective073,
		// Token: 0x04000152 RID: 338
		Objective074,
		// Token: 0x04000153 RID: 339
		Objective075,
		// Token: 0x04000154 RID: 340
		Objective076,
		// Token: 0x04000155 RID: 341
		Objective077,
		// Token: 0x04000156 RID: 342
		Objective078,
		// Token: 0x04000157 RID: 343
		Objective079,
		// Token: 0x04000158 RID: 344
		Objective080,
		// Token: 0x04000159 RID: 345
		Objective081,
		// Token: 0x0400015A RID: 346
		Objective082,
		// Token: 0x0400015B RID: 347
		Objective083,
		// Token: 0x0400015C RID: 348
		Objective084,
		// Token: 0x0400015D RID: 349
		Objective085,
		// Token: 0x0400015E RID: 350
		Objective086,
		// Token: 0x0400015F RID: 351
		Objective087,
		// Token: 0x04000160 RID: 352
		Objective088,
		// Token: 0x04000161 RID: 353
		Objective089,
		// Token: 0x04000162 RID: 354
		Objective090,
		// Token: 0x04000163 RID: 355
		Objective091,
		// Token: 0x04000164 RID: 356
		Objective092,
		// Token: 0x04000165 RID: 357
		Objective093,
		// Token: 0x04000166 RID: 358
		Objective094,
		// Token: 0x04000167 RID: 359
		Objective095,
		// Token: 0x04000168 RID: 360
		Objective096,
		// Token: 0x04000169 RID: 361
		Objective097,
		// Token: 0x0400016A RID: 362
		Objective098,
		// Token: 0x0400016B RID: 363
		Objective099,
		// Token: 0x0400016C RID: 364
		Objective100,
		// Token: 0x0400016D RID: 365
		Objective101,
		// Token: 0x0400016E RID: 366
		Objective102,
		// Token: 0x0400016F RID: 367
		Objective103,
		// Token: 0x04000170 RID: 368
		Objective104,
		// Token: 0x04000171 RID: 369
		Objective105,
		// Token: 0x04000172 RID: 370
		Objective106,
		// Token: 0x04000173 RID: 371
		Objective107,
		// Token: 0x04000174 RID: 372
		Objective108,
		// Token: 0x04000175 RID: 373
		Objective109,
		// Token: 0x04000176 RID: 374
		Objective110,
		// Token: 0x04000177 RID: 375
		Objective111,
		// Token: 0x04000178 RID: 376
		Objective112,
		// Token: 0x04000179 RID: 377
		Objective113,
		// Token: 0x0400017A RID: 378
		Objective114,
		// Token: 0x0400017B RID: 379
		Objective115,
		// Token: 0x0400017C RID: 380
		Objective116,
		// Token: 0x0400017D RID: 381
		Objective117,
		// Token: 0x0400017E RID: 382
		Objective118,
		// Token: 0x0400017F RID: 383
		Objective119,
		// Token: 0x04000180 RID: 384
		Objective120,
		// Token: 0x04000181 RID: 385
		Objective121,
		// Token: 0x04000182 RID: 386
		Objective122,
		// Token: 0x04000183 RID: 387
		Objective123,
		// Token: 0x04000184 RID: 388
		Objective124,
		// Token: 0x04000185 RID: 389
		Objective125,
		// Token: 0x04000186 RID: 390
		Objective126,
		// Token: 0x04000187 RID: 391
		Objective127,
		// Token: 0x04000188 RID: 392
		Objective128,
		// Token: 0x04000189 RID: 393
		Objective129,
		// Token: 0x0400018A RID: 394
		Objective130,
		// Token: 0x0400018B RID: 395
		Objective131,
		// Token: 0x0400018C RID: 396
		Objective132,
		// Token: 0x0400018D RID: 397
		Objective133,
		// Token: 0x0400018E RID: 398
		Objective134,
		// Token: 0x0400018F RID: 399
		Objective135,
		// Token: 0x04000190 RID: 400
		Objective136,
		// Token: 0x04000191 RID: 401
		Objective137,
		// Token: 0x04000192 RID: 402
		Objective138,
		// Token: 0x04000193 RID: 403
		Objective139,
		// Token: 0x04000194 RID: 404
		Objective140,
		// Token: 0x04000195 RID: 405
		Objective141,
		// Token: 0x04000196 RID: 406
		Objective142,
		// Token: 0x04000197 RID: 407
		Objective143,
		// Token: 0x04000198 RID: 408
		Objective144,
		// Token: 0x04000199 RID: 409
		Objective145,
		// Token: 0x0400019A RID: 410
		Objective146,
		// Token: 0x0400019B RID: 411
		Objective147,
		// Token: 0x0400019C RID: 412
		Objective148,
		// Token: 0x0400019D RID: 413
		Objective149,
		// Token: 0x0400019E RID: 414
		Objective150,
		// Token: 0x0400019F RID: 415
		Objective151,
		// Token: 0x040001A0 RID: 416
		Objective152,
		// Token: 0x040001A1 RID: 417
		Objective153,
		// Token: 0x040001A2 RID: 418
		Objective154,
		// Token: 0x040001A3 RID: 419
		Objective155,
		// Token: 0x040001A4 RID: 420
		Objective156,
		// Token: 0x040001A5 RID: 421
		Objective157,
		// Token: 0x040001A6 RID: 422
		Objective158,
		// Token: 0x040001A7 RID: 423
		Objective159,
		// Token: 0x040001A8 RID: 424
		Objective160,
		// Token: 0x040001A9 RID: 425
		Objective161,
		// Token: 0x040001AA RID: 426
		Objective162,
		// Token: 0x040001AB RID: 427
		Objective163,
		// Token: 0x040001AC RID: 428
		Objective164,
		// Token: 0x040001AD RID: 429
		Objective165,
		// Token: 0x040001AE RID: 430
		Objective166,
		// Token: 0x040001AF RID: 431
		Objective167,
		// Token: 0x040001B0 RID: 432
		Objective168,
		// Token: 0x040001B1 RID: 433
		Objective169,
		// Token: 0x040001B2 RID: 434
		Objective170,
		// Token: 0x040001B3 RID: 435
		Objective171,
		// Token: 0x040001B4 RID: 436
		Objective172,
		// Token: 0x040001B5 RID: 437
		Objective173,
		// Token: 0x040001B6 RID: 438
		Objective174,
		// Token: 0x040001B7 RID: 439
		Objective175,
		// Token: 0x040001B8 RID: 440
		Objective176,
		// Token: 0x040001B9 RID: 441
		Objective177,
		// Token: 0x040001BA RID: 442
		Objective178,
		// Token: 0x040001BB RID: 443
		Objective179,
		// Token: 0x040001BC RID: 444
		Objective180,
		// Token: 0x040001BD RID: 445
		Objective181,
		// Token: 0x040001BE RID: 446
		Objective182,
		// Token: 0x040001BF RID: 447
		Objective183,
		// Token: 0x040001C0 RID: 448
		Objective184,
		// Token: 0x040001C1 RID: 449
		Objective185,
		// Token: 0x040001C2 RID: 450
		Objective186,
		// Token: 0x040001C3 RID: 451
		Objective187,
		// Token: 0x040001C4 RID: 452
		Objective188,
		// Token: 0x040001C5 RID: 453
		Objective189,
		// Token: 0x040001C6 RID: 454
		Objective190,
		// Token: 0x040001C7 RID: 455
		Objective191,
		// Token: 0x040001C8 RID: 456
		Objective192,
		// Token: 0x040001C9 RID: 457
		Objective193,
		// Token: 0x040001CA RID: 458
		Objective194,
		// Token: 0x040001CB RID: 459
		Objective195,
		// Token: 0x040001CC RID: 460
		Objective196,
		// Token: 0x040001CD RID: 461
		Objective197,
		// Token: 0x040001CE RID: 462
		Objective198,
		// Token: 0x040001CF RID: 463
		Objective199,
		// Token: 0x040001D0 RID: 464
		Objective200,
		// Token: 0x040001D1 RID: 465
		Objective201,
		// Token: 0x040001D2 RID: 466
		Objective202,
		// Token: 0x040001D3 RID: 467
		Objective203,
		// Token: 0x040001D4 RID: 468
		Objective204,
		// Token: 0x040001D5 RID: 469
		Objective205,
		// Token: 0x040001D6 RID: 470
		Objective206,
		// Token: 0x040001D7 RID: 471
		Objective207,
		// Token: 0x040001D8 RID: 472
		Objective208,
		// Token: 0x040001D9 RID: 473
		Objective209,
		// Token: 0x040001DA RID: 474
		Objective210,
		// Token: 0x040001DB RID: 475
		Objective211,
		// Token: 0x040001DC RID: 476
		Objective212,
		// Token: 0x040001DD RID: 477
		Objective213,
		// Token: 0x040001DE RID: 478
		Objective214,
		// Token: 0x040001DF RID: 479
		Objective215,
		// Token: 0x040001E0 RID: 480
		Objective216,
		// Token: 0x040001E1 RID: 481
		Objective217,
		// Token: 0x040001E2 RID: 482
		Objective218,
		// Token: 0x040001E3 RID: 483
		Objective219,
		// Token: 0x040001E4 RID: 484
		Objective220,
		// Token: 0x040001E5 RID: 485
		Objective221,
		// Token: 0x040001E6 RID: 486
		Objective222,
		// Token: 0x040001E7 RID: 487
		Objective223,
		// Token: 0x040001E8 RID: 488
		Objective224,
		// Token: 0x040001E9 RID: 489
		Objective225,
		// Token: 0x040001EA RID: 490
		Objective226,
		// Token: 0x040001EB RID: 491
		Objective227,
		// Token: 0x040001EC RID: 492
		Objective228,
		// Token: 0x040001ED RID: 493
		Objective229,
		// Token: 0x040001EE RID: 494
		Objective230,
		// Token: 0x040001EF RID: 495
		Objective231,
		// Token: 0x040001F0 RID: 496
		Objective232,
		// Token: 0x040001F1 RID: 497
		Objective233,
		// Token: 0x040001F2 RID: 498
		Objective234,
		// Token: 0x040001F3 RID: 499
		Objective235,
		// Token: 0x040001F4 RID: 500
		Objective236,
		// Token: 0x040001F5 RID: 501
		Objective237,
		// Token: 0x040001F6 RID: 502
		Objective238,
		// Token: 0x040001F7 RID: 503
		Objective239,
		// Token: 0x040001F8 RID: 504
		Objective240,
		// Token: 0x040001F9 RID: 505
		Objective241,
		// Token: 0x040001FA RID: 506
		Objective242,
		// Token: 0x040001FB RID: 507
		Objective243,
		// Token: 0x040001FC RID: 508
		Objective244,
		// Token: 0x040001FD RID: 509
		Objective245,
		// Token: 0x040001FE RID: 510
		Objective246,
		// Token: 0x040001FF RID: 511
		Objective247,
		// Token: 0x04000200 RID: 512
		Objective248,
		// Token: 0x04000201 RID: 513
		Objective249,
		// Token: 0x04000202 RID: 514
		Objective250,
		// Token: 0x04000203 RID: 515
		Objective251,
		// Token: 0x04000204 RID: 516
		Objective252,
		// Token: 0x04000205 RID: 517
		Objective253,
		// Token: 0x04000206 RID: 518
		Objective254,
		// Token: 0x04000207 RID: 519
		Objective255,
		// Token: 0x04000208 RID: 520
		Objective256,
		// Token: 0x04000209 RID: 521
		Objective257,
		// Token: 0x0400020A RID: 522
		Objective258,
		// Token: 0x0400020B RID: 523
		Objective259,
		// Token: 0x0400020C RID: 524
		Objective260,
		// Token: 0x0400020D RID: 525
		Objective261,
		// Token: 0x0400020E RID: 526
		Objective262,
		// Token: 0x0400020F RID: 527
		Objective263,
		// Token: 0x04000210 RID: 528
		Objective264,
		// Token: 0x04000211 RID: 529
		Objective265,
		// Token: 0x04000212 RID: 530
		Objective266,
		// Token: 0x04000213 RID: 531
		Objective267,
		// Token: 0x04000214 RID: 532
		Objective268,
		// Token: 0x04000215 RID: 533
		Objective269,
		// Token: 0x04000216 RID: 534
		Objective270,
		// Token: 0x04000217 RID: 535
		Objective271,
		// Token: 0x04000218 RID: 536
		Objective272,
		// Token: 0x04000219 RID: 537
		Objective273,
		// Token: 0x0400021A RID: 538
		Objective274,
		// Token: 0x0400021B RID: 539
		Objective275,
		// Token: 0x0400021C RID: 540
		Objective276,
		// Token: 0x0400021D RID: 541
		Objective277,
		// Token: 0x0400021E RID: 542
		Objective278,
		// Token: 0x0400021F RID: 543
		Objective279,
		// Token: 0x04000220 RID: 544
		Objective280,
		// Token: 0x04000221 RID: 545
		Objective281,
		// Token: 0x04000222 RID: 546
		Objective282,
		// Token: 0x04000223 RID: 547
		Objective283,
		// Token: 0x04000224 RID: 548
		Objective284,
		// Token: 0x04000225 RID: 549
		Objective285,
		// Token: 0x04000226 RID: 550
		Objective286,
		// Token: 0x04000227 RID: 551
		Objective287,
		// Token: 0x04000228 RID: 552
		Objective288,
		// Token: 0x04000229 RID: 553
		Objective289,
		// Token: 0x0400022A RID: 554
		Objective290,
		// Token: 0x0400022B RID: 555
		Objective291,
		// Token: 0x0400022C RID: 556
		Objective292,
		// Token: 0x0400022D RID: 557
		Objective293,
		// Token: 0x0400022E RID: 558
		Objective294,
		// Token: 0x0400022F RID: 559
		Objective295,
		// Token: 0x04000230 RID: 560
		Objective296,
		// Token: 0x04000231 RID: 561
		Objective297,
		// Token: 0x04000232 RID: 562
		Objective298,
		// Token: 0x04000233 RID: 563
		Objective299,
		// Token: 0x04000234 RID: 564
		PlatinumEvents,
		// Token: 0x04000235 RID: 565
		UpgradesStore,
		// Token: 0x04000236 RID: 566
		Credits,
		// Token: 0x04000237 RID: 567
		Mission_End
	}
}
