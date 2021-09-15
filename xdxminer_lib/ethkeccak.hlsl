
void keccakf1600(uint2 state[25])
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

	for (n = 0; n < 24; n += 2)
	{
        // Round (n + 0): Axx -> Exx

		Ba = Aba ^ Aga ^ Aka ^ Ama ^ Asa;
		Be = Abe ^ Age ^ Ake ^ Ame ^ Ase;
		Bi = Abi ^ Agi ^ Aki ^ Ami ^ Asi;
		Bo = Abo ^ Ago ^ Ako ^ Amo ^ Aso;
		Bu = Abu ^ Agu ^ Aku ^ Amu ^ Asu;

		Da = Bu ^ rol(Be, 1);
		De = Ba ^ rol(Bi, 1);
		Di = Be ^ rol(Bo, 1);
		Do = Bi ^ rol(Bu, 1);
		Du = Bo ^ rol(Ba, 1);

		Ba = Aba ^ Da;
		Be = rol(Age ^ De, 44);
		Bi = rol(Aki ^ Di, 43);
		Bo = rol(Amo ^ Do, 21);
		Bu = rol(Asu ^ Du, 14);
		Eba = Ba ^ (~Be & Bi) ^ round_constants[n];
		Ebe = Be ^ (~Bi & Bo);
		Ebi = Bi ^ (~Bo & Bu);
		Ebo = Bo ^ (~Bu & Ba);
		Ebu = Bu ^ (~Ba & Be);

		Ba = rol(Abo ^ Do, 28);
		Be = rol(Agu ^ Du, 20);
		Bi = rol(Aka ^ Da, 3);
		Bo = rol(Ame ^ De, 45);
		Bu = rol(Asi ^ Di, 61);
		Ega = Ba ^ (~Be & Bi);
		Ege = Be ^ (~Bi & Bo);
		Egi = Bi ^ (~Bo & Bu);
		Ego = Bo ^ (~Bu & Ba);
		Egu = Bu ^ (~Ba & Be);

		Ba = rol(Abe ^ De, 1);
		Be = rol(Agi ^ Di, 6);
		Bi = rol(Ako ^ Do, 25);
		Bo = rol(Amu ^ Du, 8);
		Bu = rol(Asa ^ Da, 18);
		Eka = Ba ^ (~Be & Bi);
		Eke = Be ^ (~Bi & Bo);
		Eki = Bi ^ (~Bo & Bu);
		Eko = Bo ^ (~Bu & Ba);
		Eku = Bu ^ (~Ba & Be);

		Ba = rol(Abu ^ Du, 27);
		Be = rol(Aga ^ Da, 36);
		Bi = rol(Ake ^ De, 10);
		Bo = rol(Ami ^ Di, 15);
		Bu = rol(Aso ^ Do, 56);
		Ema = Ba ^ (~Be & Bi);
		Eme = Be ^ (~Bi & Bo);
		Emi = Bi ^ (~Bo & Bu);
		Emo = Bo ^ (~Bu & Ba);
		Emu = Bu ^ (~Ba & Be);

		Ba = rol(Abi ^ Di, 62);
		Be = rol(Ago ^ Do, 55);
		Bi = rol(Aku ^ Du, 39);
		Bo = rol(Ama ^ Da, 41);
		Bu = rol(Ase ^ De, 2);
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

		Da = Bu ^ rol(Be, 1);
		De = Ba ^ rol(Bi, 1);
		Di = Be ^ rol(Bo, 1);
		Do = Bi ^ rol(Bu, 1);
		Du = Bo ^ rol(Ba, 1);

		Ba = Eba ^ Da;
		Be = rol(Ege ^ De, 44);
		Bi = rol(Eki ^ Di, 43);
		Bo = rol(Emo ^ Do, 21);
		Bu = rol(Esu ^ Du, 14);
		Aba = Ba ^ (~Be & Bi) ^ round_constants[n + 1];
		Abe = Be ^ (~Bi & Bo);
		Abi = Bi ^ (~Bo & Bu);
		Abo = Bo ^ (~Bu & Ba);
		Abu = Bu ^ (~Ba & Be);

		Ba = rol(Ebo ^ Do, 28);
		Be = rol(Egu ^ Du, 20);
		Bi = rol(Eka ^ Da, 3);
		Bo = rol(Eme ^ De, 45);
		Bu = rol(Esi ^ Di, 61);
		Aga = Ba ^ (~Be & Bi);
		Age = Be ^ (~Bi & Bo);
		Agi = Bi ^ (~Bo & Bu);
		Ago = Bo ^ (~Bu & Ba);
		Agu = Bu ^ (~Ba & Be);

		Ba = rol(Ebe ^ De, 1);
		Be = rol(Egi ^ Di, 6);
		Bi = rol(Eko ^ Do, 25);
		Bo = rol(Emu ^ Du, 8);
		Bu = rol(Esa ^ Da, 18);
		Aka = Ba ^ (~Be & Bi);
		Ake = Be ^ (~Bi & Bo);
		Aki = Bi ^ (~Bo & Bu);
		Ako = Bo ^ (~Bu & Ba);
		Aku = Bu ^ (~Ba & Be);

		Ba = rol(Ebu ^ Du, 27);
		Be = rol(Ega ^ Da, 36);
		Bi = rol(Eke ^ De, 10);
		Bo = rol(Emi ^ Di, 15);
		Bu = rol(Eso ^ Do, 56);
		Ama = Ba ^ (~Be & Bi);
		Ame = Be ^ (~Bi & Bo);
		Ami = Bi ^ (~Bo & Bu);
		Amo = Bo ^ (~Bu & Ba);
		Amu = Bu ^ (~Ba & Be);

		Ba = rol(Ebi ^ Di, 62);
		Be = rol(Ego ^ Do, 55);
		Bi = rol(Eku ^ Du, 39);
		Bo = rol(Ema ^ Da, 41);
		Bu = rol(Ese ^ De, 2);
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

