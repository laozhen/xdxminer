#define ETHSEARCH true
#include "ethcommon.hlsli"

cbuffer param : register(b1)
{
	uint4 target[2]; // 32 bytes
	uint4 header[2]; // 32 bytes 
	uint2 startNonce; // 8 bytes
	uint numDatasetElementsDivideByTwo; //4 bytes
	uint init; // 4 bytes
}; // 20 words, 80 bytes total

struct result
{
	uint count; // 4 bytes
	uint pad1[3]; //12 byte
	struct
	{
		uint nonce[2]; // 8 bytes
	} nonces[MAX_FOUND];
	uint hashResult[8]; // 32 byte 
};


RWStructuredBuffer<struct result> mineResult : register(u4);


// headerNonce is [header .. nonce]
void hashimoto(out uint result[8], uint headerNonce[10])
{
	uint i, j, parentIndex,k;
	uint4x4 seed;

	uint4x4 mix[2];

	uint4x4 temp0;
	uint4x4 temp1;
	

	
	// 32 bytes digest
	uint4 digest;
	uint concat[24];
	
    // calculate seed
	keccak_512_320(seed, headerNonce);
		
	//AllMemoryBarrierWithGroupSync();
    // initialize mix
	mix[0] = seed;
	mix[1] = seed;
	uint init0 = seed[0][0];
	
	// [unroll(ACCESSES)]
    //[unroll(ACCESSES)]
	for (i = 0; i < ACCESSES; i++)
	{
		j = i % 32;
		k = j % 16;
		parentIndex = fnv(init0 ^ i, mix[j/16][k % 4][k / 4]) % (numDatasetElementsDivideByTwo);
		datasetLoad2(temp0, temp1, parentIndex * 2);
		mix[0] =fnv(mix[0], temp0);
		mix[1] =fnv(mix[1], temp1);
	}

    // compress mix into 256 bits
	digest = fnv(fnv(fnv(mix[0][0], mix[0][1]), mix[0][2]), mix[0][3]);
	
	for (i = 16; i < 20; i++)
	{
		concat[i] = digest[(i - 16) % 4];
	}
	
	digest = fnv(fnv(fnv(mix[1][0], mix[1][1]), mix[1][2]), mix[1][3]);
	
	for (i = 20; i < 24; i++)
	{
		concat[i] = digest[(i - 16) % 4];
	}

    // concatinate seed and string
	[unroll(16)]
	for (i = 0; i < 16; i++)
	{
		concat[i] = seed[i % 4][i / 4];
	}
	
	// AllMemoryBarrierWithGroupSync();
	
	keccak_256_768(result, concat);

}

[numthreads(SEARCH_NUM_THREADS_X, 1, 1)]
void main(uint3 tid : SV_DispatchThreadID)
{

	int j;
	uint i, index, foundIndex;

    //32 bytes 
	uint hashResult[8];
	uint hash_num;
	uint target_num;

    //40 bytes 
	uint headerNonce[10]; // [header .. nonce]
	bool found;

	index = tid[0]  ;

	/*
	if (index == 0 && init != 0)
	{
		mineResult[0].count = 0;
	}
	*/
	
	for (i = 0; i < 8; i++)
	{
		headerNonce[i] = header[i / 4][i % 4];
	}

	for (i = 0; i < 2; i++)
	{
		headerNonce[i + 8] = startNonce[i];
	}

    // search for NUM_THREADS nounce in the same thread group
	headerNonce[8] += index;

	//TODO check if this is needed or not
	/*
	if (headerNonce[8] < startNonce[0])
	{
		headerNonce[9]++;
	}*/
	
	for (i = 0; i < 8; i++)
	{
		hashResult[i] = 0;
	}

	hashimoto(hashResult, headerNonce);

	found = false;
 
	
	for (i = 0; i < 8; i++)
	{
		hash_num = hashResult[i];
		hash_num = ((hash_num >> 24) & 0xff) | // move byte 3 to byte 0
            ((hash_num << 8) & 0xff0000) | // move byte 1 to byte 2
            ((hash_num >> 8) & 0xff00) | // move byte 2 to byte 1
            ((hash_num << 24) & 0xff000000);
		target_num = target[i / 4][i % 4];


		if (target_num > hash_num)
		{
			found = true;
			break;
		}
		else if (target_num < hash_num)
		{
			break;
		}
	}
    
	if (!found || (mineResult[0].count >= MAX_FOUND))
	{
		return;
	}

	for (i = 0; i < 8; i++)
	{
		mineResult[0].hashResult[i] = hashResult[i];
	}


	InterlockedAdd(mineResult[0].count, 1, foundIndex);
	for (i = 0; i < 2; i++)
	{
		mineResult[0].nonces[foundIndex].nonce[i] = headerNonce[i + 8];
	}


}
