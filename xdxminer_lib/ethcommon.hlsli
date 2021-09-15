#define DATASET_SHARDS 4
#define SEARCH_NUM_THREADS_X SEARCH_NUM_THREADS_X_VALUE
#define SEARCH_NUM_THREADS_Y 1
#define SEARCH_NUM_THREADS_Z 1
#define WORKSIZE SEARCH_NUM_THREADS

#ifndef NUM_THREADS
#define NUM_THREADS 64
#endif

#ifndef KECCAK_ROUNDS
#define KECCAK_ROUNDS 24
#endif

#define DATASET_PARENTS 256
#define CACHE_ROUNDS 3
#define HASH_WORDS 16
#define ACCESSES 64
#define MAX_FOUND 6

#define PART_1_COUNT PART_1_VALUE
#define PART_2_COUNT PART_2_VALUE
#define PART_3_COUNT PART_3_VALUE

#define FNV_PRIME 0x01000193
#define fnv(v1, v2) (((v1) * FNV_PRIME) ^ (v2))

static const uint2 round_constants[24] =
{
	uint2(0x00000001, 0x00000000),
    uint2(0x00008082, 0x00000000),
    uint2(0x0000808a, 0x80000000),
    uint2(0x80008000, 0x80000000),
    uint2(0x0000808b, 0x00000000),
    uint2(0x80000001, 0x00000000),
    uint2(0x80008081, 0x80000000),
    uint2(0x00008009, 0x80000000),
    uint2(0x0000008a, 0x00000000),
    uint2(0x00000088, 0x00000000),
    uint2(0x80008009, 0x00000000),
    uint2(0x8000000a, 0x00000000),
    uint2(0x8000808b, 0x00000000),
    uint2(0x0000008b, 0x80000000),
    uint2(0x00008089, 0x80000000),
    uint2(0x00008003, 0x80000000),
    uint2(0x00008002, 0x80000000),
    uint2(0x00000080, 0x80000000),
    uint2(0x0000800a, 0x00000000),
    uint2(0x8000000a, 0x80000000),
    uint2(0x80008081, 0x80000000),
    uint2(0x00008080, 0x80000000),
    uint2(0x80000001, 0x00000000),
    uint2(0x80008008, 0x80000000),
};

static const uint keccak_rotc[24] =
{
	1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 2, 14,
    27, 41, 56, 8, 25, 43, 62, 18, 39, 61, 20, 44
};

static const uint keccak_piln[24] =
{
	10, 7, 11, 17, 18, 3, 5, 16, 8, 21, 24, 4,
    15, 23, 19, 13, 12, 2, 20, 14, 22, 9, 6, 1
};

//less than 32 rorate left
uint2 rol_1(uint2 a, uint b)
{
	uint2 t = uint2(0, 0);
	t.x = (a.x << b) | (a.y >> (32 - b));
	t.y = (a.y << b) | (a.x >> (32 - b));
	return t;
}


// more than 32 rotate
uint2 rol_2(uint2 a, uint b)
{
	uint2 t = uint2(0, 0);
	a = a.yx;
	b -= 32;
	t.x = (a.x << b) | (a.y >> (32 - b));
	t.y = (a.y << b) | (a.x >> (32 - b));
	return t;
}

uint2 rol(uint2 a, uint b)
{
	uint2 t = uint2(0, 0);
	if (b >= 32)
	{
		a = a.yx;
		b -= 32;
	}
	if (b == 0)
	{
		return a;
	}
	t.x = (a.x << b) | (a.y >> (32 - b));
	t.y = (a.y << b) | (a.x >> (32 - b));
	return t;
}


void keccak(inout uint2 state[25])
{
	uint n;
	uint2 Aba, Abe, Abi, Abo, Abu;
	uint2 Aga, Age, Agi, Ago, Agu;
	uint2 Aka, Ake, Aki, Ako, Aku;
	uint2 Ama, Ame, Ami, Amo, Amu;
	uint2 Asa, Ase, Asi, Aso, Asu;

	uint2 Eba, Ebe, Ebi, Ebo, Ebu;
	uint2 Ega, Ege, Egi, Ego, Egu;
	uint2 Eka, Eke, Eki, Eko, Eku;
	uint2 Ema, Eme, Emi, Emo, Emu;
	uint2 Esa, Ese, Esi, Eso, Esu;

	uint2 Ba, Be, Bi, Bo, Bu;

	uint2 Da, De, Di, Do, Du;

	Aba = state[0];
	Abe = state[1];
	Abi = state[2];
	Abo = state[3];
	Abu = state[4];
	Aga = state[5];
	Age = state[6];
	Agi = state[7];
	Ago = state[8];
	Agu = state[9];
	Aka = state[10];
	Ake = state[11];
	Aki = state[12];
	Ako = state[13];
	Aku = state[14];
	Ama = state[15];
	Ame = state[16];
	Ami = state[17];
	Amo = state[18];
	Amu = state[19];
	Asa = state[20];
	Ase = state[21];
	Asi = state[22];
	Aso = state[23];
	Asu = state[24];

	[unroll(12)]
	for (n = 0; n < 24; n += 2)
	{
        // Round (n + 0): Axx -> Exx

		Ba = Aba ^ Aga ^ Aka ^ Ama ^ Asa;
		Be = Abe ^ Age ^ Ake ^ Ame ^ Ase;
		Bi = Abi ^ Agi ^ Aki ^ Ami ^ Asi;
		Bo = Abo ^ Ago ^ Ako ^ Amo ^ Aso;
		Bu = Abu ^ Agu ^ Aku ^ Amu ^ Asu;

		Da = Bu ^ rol_1(Be, 1);
		De = Ba ^ rol_1(Bi, 1);
		Di = Be ^ rol_1(Bo, 1);
		Do = Bi ^ rol_1(Bu, 1);
		Du = Bo ^ rol_1(Ba, 1);

		Ba = Aba ^ Da;
		Be = rol_2(Age ^ De, 44); //1
		Bi = rol_2(Aki ^ Di, 43); //2
		Bo = rol_1(Amo ^ Do, 21); //3
		Bu = rol_1(Asu ^ Du, 14); //4
		Eba = Ba ^ (~Be & Bi) ^ round_constants[n];
		Ebe = Be ^ (~Bi & Bo);
		Ebi = Bi ^ (~Bo & Bu);
		Ebo = Bo ^ (~Bu & Ba);
		Ebu = Bu ^ (~Ba & Be);

		Ba = rol_1(Abo ^ Do, 28); //5
		Be = rol_1(Agu ^ Du, 20);//6
		Bi = rol_1(Aka ^ Da, 3);//7
		Bo = rol_2(Ame ^ De, 45);//8
		Bu = rol_2(Asi ^ Di, 61);//9
		Ega = Ba ^ (~Be & Bi);
		Ege = Be ^ (~Bi & Bo);
		Egi = Bi ^ (~Bo & Bu);
		Ego = Bo ^ (~Bu & Ba);
		Egu = Bu ^ (~Ba & Be);

		Ba = rol_1(Abe ^ De, 1); //10
		Be = rol_1(Agi ^ Di, 6); //11
		Bi = rol_1(Ako ^ Do, 25);//12
		Bo = rol_1(Amu ^ Du, 8); //13
		Bu = rol_1(Asa ^ Da, 18);//14
		Eka = Ba ^ (~Be & Bi);
		Eke = Be ^ (~Bi & Bo);
		Eki = Bi ^ (~Bo & Bu);
		Eko = Bo ^ (~Bu & Ba);
		Eku = Bu ^ (~Ba & Be);

		Ba = rol_1(Abu ^ Du, 27);//15
		Be = rol_2(Aga ^ Da, 36);//16
		Bi = rol_1(Ake ^ De, 10);//17
		Bo = rol_1(Ami ^ Di, 15);//18
		Bu = rol_2(Aso ^ Do, 56);//19
		Ema = Ba ^ (~Be & Bi);
		Eme = Be ^ (~Bi & Bo);
		Emi = Bi ^ (~Bo & Bu);
		Emo = Bo ^ (~Bu & Ba);
		Emu = Bu ^ (~Ba & Be);

		Ba = rol_2(Abi ^ Di, 62);//20
		Be = rol_2(Ago ^ Do, 55);//21
		Bi = rol_2(Aku ^ Du, 39);//21
		Bo = rol_2(Ama ^ Da, 41);//22
		Bu = rol_1(Ase ^ De, 2);//23
		Esa = Ba ^ (~Be & Bi);
		Ese = Be ^ (~Bi & Bo);
		Esi = Bi ^ (~Bo & Bu);
		Eso = Bo ^ (~Bu & Ba);
		Esu = Bu ^ (~Ba & Be);


        // Round (n + 1): Exx -> Axx

		Ba = Eba ^ Ega ^ Eka ^ Ema ^ Esa;
		Be = Ebe ^ Ege ^ Eke ^ Eme ^ Ese;
		Bi = Ebi ^ Egi ^ Eki ^ Emi ^ Esi;
		Bo = Ebo ^ Ego ^ Eko ^ Emo ^ Eso;
		Bu = Ebu ^ Egu ^ Eku ^ Emu ^ Esu;

		Da = Bu ^ rol_1(Be, 1); 
		De = Ba ^ rol_1(Bi, 1);
		Di = Be ^ rol_1(Bo, 1);
		Do = Bi ^ rol_1(Bu, 1);
		Du = Bo ^ rol_1(Ba, 1);

		Ba = Eba ^ Da;
		Be = rol_2(Ege ^ De, 44);
		Bi = rol_2(Eki ^ Di, 43);
		Bo = rol_1(Emo ^ Do, 21);
		Bu = rol_1(Esu ^ Du, 14);
		Aba = Ba ^ (~Be & Bi) ^ round_constants[n + 1];
		Abe = Be ^ (~Bi & Bo);
		Abi = Bi ^ (~Bo & Bu);
		Abo = Bo ^ (~Bu & Ba);
		Abu = Bu ^ (~Ba & Be);

		Ba = rol_1(Ebo ^ Do, 28);
		Be = rol_1(Egu ^ Du, 20);
		Bi = rol_1(Eka ^ Da, 3);
		Bo = rol_2(Eme ^ De, 45);
		Bu = rol_2(Esi ^ Di, 61);
		Aga = Ba ^ (~Be & Bi);
		Age = Be ^ (~Bi & Bo);
		Agi = Bi ^ (~Bo & Bu);
		Ago = Bo ^ (~Bu & Ba);
		Agu = Bu ^ (~Ba & Be);

		Ba = rol_1(Ebe ^ De, 1);
		Be = rol_1(Egi ^ Di, 6);
		Bi = rol_1(Eko ^ Do, 25);
		Bo = rol_1(Emu ^ Du, 8);
		Bu = rol_1(Esa ^ Da, 18);
		Aka = Ba ^ (~Be & Bi);
		Ake = Be ^ (~Bi & Bo);
		Aki = Bi ^ (~Bo & Bu);
		Ako = Bo ^ (~Bu & Ba);
		Aku = Bu ^ (~Ba & Be);

		Ba = rol_1(Ebu ^ Du, 27);
		Be = rol_2(Ega ^ Da, 36);
		Bi = rol_1(Eke ^ De, 10);
		Bo = rol_1(Emi ^ Di, 15);
		Bu = rol_2(Eso ^ Do, 56);
		Ama = Ba ^ (~Be & Bi);
		Ame = Be ^ (~Bi & Bo);
		Ami = Bi ^ (~Bo & Bu);
		Amo = Bo ^ (~Bu & Ba);
		Amu = Bu ^ (~Ba & Be);

		Ba = rol_2(Ebi ^ Di, 62);
		Be = rol_2(Ego ^ Do, 55);
		Bi = rol_2(Eku ^ Du, 39);
		Bo = rol_2(Ema ^ Da, 41);
		Bu = rol_1(Ese ^ De, 2);
		Asa = Ba ^ (~Be & Bi);
		Ase = Be ^ (~Bi & Bo);
		Asi = Bi ^ (~Bo & Bu);
		Aso = Bo ^ (~Bu & Ba);
		Asu = Bu ^ (~Ba & Be);
	}

	state[0] = Aba;
	state[1] = Abe;
	state[2] = Abi;
	state[3] = Abo;
	state[4] = Abu;
	state[5] = Aga;
	state[6] = Age;
	state[7] = Agi;
	state[8] = Ago;
	state[9] = Agu;
	state[10] = Aka;
	state[11] = Ake;
	state[12] = Aki;
	state[13] = Ako;
	state[14] = Aku;
	state[15] = Ama;
	state[16] = Ame;
	state[17] = Ami;
	state[18] = Amo;
	state[19] = Amu;
	state[20] = Asa;
	state[21] = Ase;
	state[22] = Asi;
	state[23] = Aso;
	state[24] = Asu;
}

void keccak_512_512(out uint4x4 dst, in uint4x4 src)
{
	uint i;
	uint2 st[25];

	for (i = 0; i < 8; i++)
		st[i] = uint2(src[(i * 2) % 4][(i * 2)  / 4], src[(i * 2 + 1) % 4][(i * 2) / 4]);

	for (i = 8; i < 25; i++)
		st[i] = 0;

    // 64, 71
	st[8] = uint2(0x00000001, 0x80000000);

	keccak(st);

	for (i = 0; i < 8; i++)
	{
		dst[(i * 2) % 4][(i * 2) / 4] = st[i].x;
		dst[(i * 2 + 1) % 4][(i * 2 + 1) / 4] = st[i].y;
	}
}

void keccak_512_320(out uint4x4 dst, in uint src[10])
{
	uint i;
	uint2 st[25];

	[unroll(5)]
	for (i = 0; i < 5; i++)
	{
		st[i] = uint2(src[i * 2], src[i * 2 + 1]);
	}
	
	[unroll(20)]
	for (i = 5; i < 25; i++)
	{
		st[i] = 0;
	}
	
    // 40, 71
	st[5] = uint2(0x00000001, 0x00000000);
	st[8] = uint2(0x00000000, 0x80000000);

	keccak(st);

	
	[unroll(8)]
	for (i = 0; i < 8; i++)
	{
		dst[(i * 2) % 4][(i * 2) / 4] = st[i].x;
		dst[(i * 2 + 1) % 4][(i * 2 + 1) / 4] = st[i].y;
	}
}

void keccak_256_768(out uint dst[8], in uint src[24])
{
	uint i;
	uint2 st[25];

	[unroll(12)]
	for (i = 0; i < 12; i++) {
		st[i] = uint2(src[i * 2], src[i * 2 + 1]);
	}
	
	
	[unroll(13)]
	for (i = 12; i < 25; i++)
	{
		st[i] = 0;
	}
	
    // 96 135
	st[12] = uint2(0x00000001, 0x00000000);
	st[16] = uint2(0x00000000, 0x80000000);

	keccak(st);

	[unroll(4)]
	for (i = 0; i < 4; i++)
	{
		dst[i * 2] = st[i].x;
		dst[i * 2 + 1] = st[i].y;
	}
}

typedef
struct
{
	uint4x4 data;
} DagItem;

#if DATASET_SHARDS == 4
#ifndef ETHSEARCH
RWStructuredBuffer<DagItem> dataset0 : register(u0);
RWStructuredBuffer<DagItem> dataset1 : register(u1);
RWStructuredBuffer<DagItem> dataset2 : register(u2);
RWStructuredBuffer<DagItem> dataset3 : register(u3);

void datasetStore(uint index, uint4x4 data)
{

	if (index < PART_1_COUNT)
	{
		dataset0[index].data = data;
	}
	else if (index < PART_2_COUNT)
	{
		dataset1[index - PART_1_COUNT].data = data;
		if (index == PART_1_COUNT)
		{
			dataset0[index].data = data;
		}
	}
	else if (index < PART_3_COUNT)
	{
		dataset2[index - PART_2_COUNT].data = data;

		if (index == PART_2_COUNT)
		{
			dataset1[index - PART_1_COUNT].data = data;
		}
	}
	else
	{
		dataset3[index - PART_3_COUNT].data = data;

		if (index == PART_3_COUNT)
		{
			dataset2[index - PART_2_COUNT].data = data;
		}
	}
}
#else
StructuredBuffer<DagItem> dataset0 : register(t0);
StructuredBuffer<DagItem> dataset1 : register(t1);
StructuredBuffer<DagItem> dataset2 : register(t2);
StructuredBuffer<DagItem> dataset3 : register(t3);
#endif




void datasetLoad2_uint4(out uint4x4 data0, out uint4x4 data1, uint index)
{
	if (index < PART_1_COUNT)
	{
		data0 = dataset0[index].data;
		data1 = dataset0[index + 1].data;
	}
	else if (index < PART_2_COUNT)
	{
		data0 = dataset1[index - PART_1_COUNT].data;
		data1 = dataset1[index + 1 - PART_1_COUNT].data;
	}
	else if (index < PART_3_COUNT)
	{
		data0 = dataset2[index - PART_2_COUNT].data;
		data1 = dataset2[index + 1 - PART_2_COUNT].data;
	}
	else
	{
		data0 = dataset3[index - PART_3_COUNT].data;
		data1 = dataset3[index + 1 - PART_3_COUNT].data;
	}
}

void datasetLoad2(out uint4x4 data0, out uint4x4 data1, uint index)
{
	if (index < PART_1_COUNT)
	{
		data0 = dataset0[index].data;
		data1 = dataset0[index + 1].data;
	}
	else if (index < PART_2_COUNT)
	{
		data0 = dataset1[index - PART_1_COUNT].data;
		data1 = dataset1[index + 1 - PART_1_COUNT].data;
	}
	else if (index < PART_3_COUNT)
	{
		data0 = dataset2[index - PART_2_COUNT].data;
		data1 = dataset2[index + 1 - PART_2_COUNT].data;
	}
	else
	{
		data0 = dataset3[index - PART_3_COUNT].data;
		data1 = dataset3[index + 1 - PART_3_COUNT].data;
	}
}


#elif !defined(DATASET_SHARDS) || DATASET_SHARDS == 1

#ifndef ETHSEARCH
RWStructuredBuffer<DagItem> dataset : register(u0);
void datasetStore(uint index, uint data[16])
{
	dataset[index] = (uint4[4]) data;
}

#else 
StructuredBuffer<DagItem> dataset : register(t0);
#endif


void datasetLoad2(out uint data0[16], out uint data1[16], uint index) {
    data0 = (uint[16])dataset[index].data;
    data1 = (uint[16])dataset[index + 1 ].data;
}


void datasetLoad(out uint data[16], uint index) {
    data = (uint[16])dataset[index].data;
}

#else
#error Unsupported number of shards
#endif