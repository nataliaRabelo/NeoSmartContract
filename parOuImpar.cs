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
        // Definição de strings chave para identificar os jogadores no armazenamento.
        private static readonly string Jogador1Key = "Jogador1";
        private static readonly string Jogador2Key = "Jogador2";
        // Mapa de armazenamento para armazenar informações sobre o jogo.
        private static StorageMap GameInfoMap => new StorageMap(Storage.CurrentContext, "gameInfo");
        // Declaração de eventos para notificação e log.
        public static event Action<string> my_event_str_one;
        public static event Action<string, string> my_event_str;
        public static event Action<UInt160> my_event_uint160;
        // Declaração de um OpCode externo para conversão de dados para ByteString.
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

        // Método para obter o número escolhido pelo primeiro jogador.
        public static BigInteger GetJogo(){
            ByteString mao1 = AsByteString(GameInfoMap.Get("jogo_mao"));
            return (BigInteger)mao1;
        }

        // Método para atribuir jogadores ao jogo e inicializá-lo.
        public static bool AssignPlayers(UInt160 jogador1, UInt160 jogador2){
            // Inicializa variáveis do jogo.
            Inicializa();
            // Converte o UInt160 dos jogadores em strings e os armazena.
            string strJog2 = (string)(ByteString)(byte[])jogador1;
            string strJog3 = (string)(ByteString)(byte[])jogador2;
            Storage.Put(Storage.CurrentContext, Jogador1Key, strJog2);
            Storage.Put(Storage.CurrentContext, Jogador2Key, strJog3);
            return true;
        }

        // Método para inicializar/zerar as variáveis de jogo.
        public static void Inicializa(){
            Storage.Put(Storage.CurrentContext, "jogo", -1);
        }
        
        // Método para iniciar um novo jogo com o primeiro jogador.
        public static bool NovoJogo(UInt160 jogador1, string parImpar, BigInteger mao)
        {
            // Verifica se a escolha é válida ("par" ou "impar").
            if (!(parImpar == "par" || parImpar == "impar")) 
                my_event_str_one("Erro: jogador 1 deve escolher par ou impar");
             // Verifica se a transação é realizada pelo jogador 1.
            if (!Runtime.CheckWitness(jogador1)) 
                my_event_str_one("Erro: esperava jogador 1");
            // Verifica se um jogo já foi iniciado.
            if (GameInfoMap.Get("jogo") != null)
                my_event_str_one("Erro: jogo ja comecou! Jogador 2 deve jogar!");
            // Armazena as escolhas do jogador 1 no armazenamento.
            GameInfoMap.Put("jogo", parImpar);
            GameInfoMap.Put("jogo_mao", mao);
            // Gera um evento para log.
            my_event_str("Jogador 1 apostou em :", parImpar);
            return true;
        }

        // Método para o jogador 2 continuar o jogo.
        public static bool ContinuaJogo(UInt160 jogador2, BigInteger mao2)
        {
            // Verifica se a transação é realizada pelo jogador 2.
            if (!Runtime.CheckWitness(jogador2)) 
                my_event_str_one("Erro: esperava jogador 2");

            // Verifica se o jogo foi iniciado pelo jogador 1.
            string parImpar = GameInfoMap.Get("jogo");
            if (parImpar is null)
                my_event_str_one("Erro: jogo nao comecou! Jogador 1 deve jogar!");

            // Recupera a jogada do jogador 1.
            ByteString mao1 = AsByteString(GameInfoMap.Get("jogo_mao"));
            BigInteger mao1BigInteger = (BigInteger)mao1;

            // Calcula o resultado.
            BigInteger resultado = (mao1BigInteger + mao2) % 2;
            bool isPar = resultado % 2 == 0;

            // Verifica quem venceu e gera um evento para log.
            if ((isPar && parImpar == "par") || (!isPar && parImpar == "impar"))
                my_event_str_one("Jogador 1 venceu!");
            else
                my_event_str_one("Jogador 2 venceu!");

            // Limpa os dados do jogo para um novo round.
            GameInfoMap.Delete("jogo");
            GameInfoMap.Delete("jogo_mao");

            return true;
        }
    }
}
