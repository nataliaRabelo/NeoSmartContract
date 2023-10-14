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
    [ManifestExtra("Author", "Seu Nome")]
    [ManifestExtra("Email", "SeuEmail@example.com")]
    [ManifestExtra("Description", "Jogo Par ou Impar")]
    public class ParImparContract : Framework.SmartContract
    {
        private static readonly UInt160 Jogador1 = "NcbDk1XmMvGD5jjBgaRJhgnMmXCYLjqZov".ToScriptHash();
        private static readonly UInt160 Jogador2 = "NgiRuwZGH9wEwK9TVCQ4tY9mx2vwUJU7kt".ToScriptHash();
        private static StorageMap GameInfoMap => new StorageMap(Storage.CurrentContext, "gameInfo");
        
        public static bool NovoJogo(string parImpar, BigInteger mao)
        {
            if (!(parImpar == "par" || parImpar == "impar")) 
                throw new Exception("Erro: jogador 1 deve escolher par ou impar");

            if (!Runtime.CheckWitness(Jogador1)) 
                throw new Exception("Erro: esperava jogador 1");

            if (GameInfoMap.Get("jogo") != null)
                throw new Exception("Erro: jogo ja comecou! Jogador 2 deve jogar!");
            
            GameInfoMap.Put("jogo", parImpar);
            GameInfoMap.Put("jogo_mao", mao);

            Runtime.Log("Jogador 1 apostou em " + parImpar);
            return true;
        }

        public static bool ContinuaJogo(BigInteger mao2)
        {
            if (!Runtime.CheckWitness(Jogador2)) 
                throw new Exception("Erro: esperava jogador 2");
            
            string parImpar = GameInfoMap.Get("jogo")?.AsString();
            if (parImpar is null)
                throw new Exception("Erro: jogo nao comecou! Jogador 1 deve jogar!");

            BigInteger mao1 = GameInfoMap.Get("jogo_mao").AsBigInteger();

            BigInteger resultado = (mao1 + mao2) % 2;
            bool isPar = resultado % 2 == 0;

            if ((isPar && parImpar == "par") || (!isPar && parImpar == "impar"))
                Runtime.Log("Jogador 1 venceu!");
            else
                Runtime.Log("Jogador 2 venceu!");

            GameInfoMap.Delete("jogo");
            GameInfoMap.Delete("jogo_mao");

            return true;
        }
    }
}
