// SPDX-License-Identifier: MIT
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace Neo.SmartContract
{
    public class ParImparContract : Framework.SmartContract
    {
        private static readonly string Jogador1Key = "Jogador1";
        private static readonly string Jogador2Key = "Jogador2";
        private static StorageMap GameInfoMap => new StorageMap(Storage.CurrentContext, "gameInfo");

        public static event Action<string> my_event_str_one;
        public static event Action<string, string> my_event_str;
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);

        public static BigInteger GetJogo(){
            ByteString mao1 = AsByteString(GameInfoMap.Get("jogo_mao"));
            return (BigInteger)mao1;
        }

        public static bool AssignPlayers(UInt160 jogador1, UInt160 jogador2){
            Inicializa();
            string strJog2 = (string)(ByteString)(byte[])jogador1;
            string strJog3 = (string)(ByteString)(byte[])jogador2;
            Storage.Put(Storage.CurrentContext, Jogador1Key, strJog2);
            Storage.Put(Storage.CurrentContext, Jogador2Key, strJog3);
            return true;
        }

        public static void Inicializa(){
            Storage.Put(Storage.CurrentContext, "jogo", -1);
        }
        
        public static bool NovoJogo(UInt160 jogador1, string parImpar, BigInteger mao)
        {
            if (!(parImpar == "par" || parImpar == "impar")) 
                my_event_str_one("Erro: jogador 1 deve escolher par ou impar");

            if (!Runtime.CheckWitness(jogador1)) 
                my_event_str_one("Erro: esperava jogador 1");

            if (GameInfoMap.Get("jogo") != null)
                my_event_str_one("Erro: jogo ja comecou! Jogador 2 deve jogar!");
            
            GameInfoMap.Put("jogo", parImpar);
            GameInfoMap.Put("jogo_mao", mao);

            my_event_str("Jogador 1 apostou em ", parImpar);
            return true;
        }

        public static bool ContinuaJogo(UInt160 jogador2, BigInteger mao2)
        {
            if (!Runtime.CheckWitness(jogador2)) 
                my_event_str_one("Erro: esperava jogador 2");
            
            string parImpar = GameInfoMap.Get("jogo");
            if (parImpar is null)
                my_event_str_one("Erro: jogo nao comecou! Jogador 1 deve jogar!");

            ByteString mao1 = AsByteString(GameInfoMap.Get("jogo_mao"));
            BigInteger mao1BigInteger = (BigInteger)mao1;

            BigInteger resultado = (mao1BigInteger + mao2) % 2;
            bool isPar = resultado % 2 == 0;

            if ((isPar && parImpar == "par") || (!isPar && parImpar == "impar"))
                my_event_str_one("Jogador 1 venceu!");
            else
                my_event_str_one("Jogador 2 venceu!");

            GameInfoMap.Delete("jogo");
            GameInfoMap.Delete("jogo_mao");

            return true;
        }
    }
}
