// SPDX-License-Identifier: MIT
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace CaraCoroaContract
{
    [ManifestExtra("Author", "Seu Nome")]
    [ManifestExtra("Email", "seu_email@example.com")]
    [ManifestExtra("Description", "Um jogo simples de Cara ou Coroa")]
    public class CaraCoroa : SmartContract
    {
        static readonly string JuizKey = "Juiz";
        static readonly string Jogador1Key = "Jogador1";
        static readonly string Jogador2Key = "Jogador2";
        public static event Action<string, string> my_event_str;
        public static event Action<string> my_event_str_one;
        public static event Action<string, UInt160> my_event;
        public static event Action<BigInteger> my_event_integer;
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);

        public static bool Verify() => true;

        public static UInt160 GetPlayer1(){

            ByteString jogador1 = AsByteString(Storage.Get(Storage.CurrentContext, Jogador1Key));
            return (UInt160)jogador1;

        }

        public static UInt160 GetPlayer2(){

            ByteString jogador2 = AsByteString(Storage.Get(Storage.CurrentContext, Jogador2Key));
            return (UInt160)jogador2;

        }

        public static bool AssignPlayers(UInt160 juiz, UInt160 jogador1, UInt160 jogador2){
            if (!Runtime.CheckWitness(juiz)) return false;
            string strJuiz1 = (string)(ByteString)(byte[])juiz;
            string strJog2 = (string)(ByteString)(byte[])jogador1;
            string strJog3 = (string)(ByteString)(byte[])jogador2;
            Storage.Put(Storage.CurrentContext, JuizKey, strJuiz1);
            Storage.Put(Storage.CurrentContext, Jogador1Key, strJog2);
            Storage.Put(Storage.CurrentContext, Jogador2Key, strJog3);
            return true;

        }

        public static bool NovoJogo(string caraCoroa)
        {
            if (!(caraCoroa == "cara" || caraCoroa == "coroa"))
            {
                my_event_str_one("Erro: jogador 1 deve escolher cara ou coroa");
                return false;
            }
            if (!Runtime.CheckWitness(GetPlayer1()))
            {
                my_event_str_one("Erro: esperava jogador 1");
                return false;
            }
            if (Storage.Get(Storage.CurrentContext, "jogo").Length > 0)
            {
                my_event_str_one("Erro: jogo ja comecou! Jogador 2 deve jogar!");
                return false;
            }
            Storage.Put(Storage.CurrentContext, "jogo", caraCoroa);
            my_event_str("Jogador 1 apostou em ", caraCoroa);
            return true;
        }

        public static bool Sorteio()
        {
            if (!Runtime.CheckWitness(GetPlayer2()))
            {
                my_event_str_one("Erro: esperava jogador 2");
                return false;
            }
            if (Storage.Get(Storage.CurrentContext, "jogo").Length == 0)
            {
                my_event_str_one("Erro: jogo nao comecou! Jogador 1 deve jogar!");
                return false;
            }
            string caraCoroa = Storage.Get(Storage.CurrentContext, "jogo");
            BigInteger semente = Runtime.GetRandom();
            my_event_str_one("Numero sorteado:");
            my_event_integer(semente);
            bool bcara = (semente % 2) == 0;
            if ((bcara && caraCoroa == "cara") || (!bcara && caraCoroa == "coroa"))
                my_event_str_one("Jogador 1 venceu!");
            else
                my_event_str_one("Jogador 2 venceu!");
            Storage.Delete(Storage.CurrentContext, "jogo");
            return true;
        }
    }
}
