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
    public class CaraCoroa : SmartContract
    {
        // Definição de chaves estáticas para identificação no Storage
        static readonly string JuizKey = "Juiz";
        static readonly string Jogador1Key = "Jogador1";
        static readonly string Jogador2Key = "Jogador2";

        // Eventos para registrar e logar mensagens e valores durante a execução do contrato
        public static event Action<string, string> my_event_str;
        public static event Action<string> my_event_str_one;
        public static event Action<string, UInt160> my_event;
        public static event Action<BigInteger> my_event_integer;
        public static event Action<UInt160> my_event_uint160;

        // OpCode para converter dados para ByteString.
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);

        // Método que imprime o valor armazenado no Storage, convertido para UInt160, associado à chave dada como parâmetro.
        public static void PrintStorageUInt160(string s){

            ByteString storageResult = AsByteString(Storage.Get(Storage.CurrentContext, s));
            my_event_uint160((UInt160)storageResult);

        }

        // Método que imprime o valor armazenado no Storage, como uma string, associado à chave dada como parâmetro.
        public static void PrintStorageString(string s){

            ByteString storageResult = AsByteString(Storage.Get(Storage.CurrentContext, s));
            my_event_str_one(storageResult);

        }

        // Método para obter o jogador 1 do Storage e convertê-lo para UInt160.
        public static UInt160 GetPlayer1(){

            ByteString jogador1 = AsByteString(Storage.Get(Storage.CurrentContext, Jogador1Key));
            return (UInt160)jogador1;

        }

        // Método para obter o jogador 2 do Storage e convertê-lo para UInt160.
        public static UInt160 GetPlayer2(){

            ByteString jogador2 = AsByteString(Storage.Get(Storage.CurrentContext, Jogador2Key));
            return (UInt160)jogador2;

        }

        // Método para atribuir jogadores e juiz, inicializando o Storage com suas respectivas informações.
        public static bool AssignPlayers(UInt160 juiz, UInt160 jogador1, UInt160 jogador2){
            Inicializa();
            string strJuiz1 = (string)(ByteString)(byte[])juiz;
            string strJog2 = (string)(ByteString)(byte[])jogador1;
            string strJog3 = (string)(ByteString)(byte[])jogador2;
            Storage.Put(Storage.CurrentContext, JuizKey, strJuiz1);
            Storage.Put(Storage.CurrentContext, Jogador1Key, strJog2);
            Storage.Put(Storage.CurrentContext, Jogador2Key, strJog3);
            return true;
        }

        // Método para inicializar o jogo e evitar problemas com valores nulos, definindo uma chave "jogo" no Storage com o valor "X".
        public static void Inicializa(){
            Storage.Put(Storage.CurrentContext, "jogo", "X");
        }

        // Método para criar um novo jogo, armazenando a escolha (cara ou coroa) do jogador 1 no Storage.
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
            if (Storage.Get(Storage.CurrentContext, "jogo") != "X")
            {
                my_event_str_one("Erro: jogo ja comecou! Jogador 2 deve jogar!");
                return false;
            }
            Storage.Put(Storage.CurrentContext, "jogo", caraCoroa);
            my_event_str("Jogador 1 apostou em ", caraCoroa);
            return true;
        }

        // Método para realizar um sorteio e determinar o vencedor com base na escolha do jogador 1 e um número aleatório.
        public static bool Sorteio()
        {
            if (!Runtime.CheckWitness(GetPlayer2()))
            {
                my_event_str_one("Erro: esperava jogador 2");
                return false;
            }
            if (Storage.Get(Storage.CurrentContext, "jogo") == "X")
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
            Inicializa();
            return true;
        }
    }
}
