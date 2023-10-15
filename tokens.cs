// SPDX-License-Identifier: MIT
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace StakingTokenContract
{
    // Atributos que especificam padrões e permissões do contrato.
    [SupportedStandards("NEP-17")]
    [ContractPermission("*", "onNEP17Payment")]
    public partial class StakingToken : Nep17Token
    {
        // Mapeamento de Storage usado para armazenar stakes dos usuários.
        private static StorageMap StakeMap => new StorageMap(Storage.CurrentContext, "stake");
        // Declaração de um OpCode externo, neste caso, convertendo dados para ByteString.
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);
        // Declaração de um evento para ser disparado durante execuções específicas do contrato, neste caso sendo usado para printar mensagens e valores armazenados no storage em string.
        public static event Action<string> my_event_str_one;
        // Declaração de um evento para ser disparado durante execuções específicas do contrato, neste caso sendo usado para printar valores armazenados no storage em UInt160.
        public static event Action<UInt160> my_event_uint160;
        // Sobreposição do método Decimals para definir a quantidade de casas decimais do token.
        public override byte Decimals() => 8;
        // Sobreposição do método Symbol para definir o símbolo do token.
        public override string Symbol() => "STK";
        // Método de implantação do contrato que também minta tokens iniciais para o owner.

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

        // Método de implantação do contrato que também minta tokens iniciais para o owner.
        public static void _deploy(UInt160 owner, object data, bool update)
        {
            if (update) return;
            Nep17Token.Mint(owner, 100_000_000 * (BigInteger.Pow(10, 8)));
        }

        // Método para criar novos tokens, verificando permissões e autenticações.
        public static void MintTokens(UInt160 owner, UInt160 to, BigInteger amount)
        {
            // Verifica se a chamada foi feita pelo owner.
            if (!Runtime.CheckWitness(owner)){
                my_event_str_one("No Authorization!");
            }
            Nep17Token.Mint(to, amount);
        }

        // Método para queimar/destuir tokens, verificando permissões e autenticações.
        public static void BurnTokens(UInt160 owner, UInt160 from, BigInteger amount)
        {
            // Verifica se a chamada foi feita pelo owner e pelo titular dos tokens.
            if (!Runtime.CheckWitness(owner) || !Runtime.CheckWitness(from)){
                my_event_str_one("No Authorization!");
            } 
            Nep17Token.Burn(from, amount);
        }

        // Método para fazer stake de tokens.
        public static void StakeTokens(UInt160 account, BigInteger amount)
        {
            // Verifica se a chamada foi feita pelo titular da conta.
            if (!Runtime.CheckWitness(account)){
                my_event_str_one("No Authorization!");
            }
            // Verifica se a conta tem saldo suficiente.
            if (BalanceOf(account) < amount){
                my_event_str_one("Insufficient Balance!");
            }
            // Transfere os tokens para o contrato e atualiza o mapeamento de stake.
            Nep17Token.Transfer(account, Runtime.ExecutingScriptHash, amount, null);
            ByteString currentStake = AsByteString(StakeMap.Get(account));
            BigInteger currentStakeBigInteger = (BigInteger)currentStake;
            StakeMap.Put(account, currentStakeBigInteger + amount);
        }

        // Método para enviar tokens entre contas, verificando permissões e saldos.
        public static bool SendTokens(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            // Verifica se a chamada foi feita pelo remetente.
            if (!Runtime.CheckWitness(from)){
                my_event_str_one("No Authorization!");
            }
            // Verifica se o remetente tem saldo suficiente. 
            if (BalanceOf(from) < amount){
                my_event_str_one("Insufficient Balance!");
            } 
            return Nep17Token.Transfer(from, to, amount, data);
        }

        // Método para remover tokens do stake, verificando permissões e saldos.
        public static void UnstakeTokens(UInt160 account, BigInteger amount)
        {
            // Verifica se a chamada foi feita pelo titular da conta.
            if (!Runtime.CheckWitness(account)){
                my_event_str_one("No Authorization!");
            } 
            ByteString currentStake = AsByteString(StakeMap.Get(account));
            BigInteger currentStakeBigInteger = (BigInteger)currentStake;
            // Verifica se o titular da conta tem saldo de stake suficiente.
            if (currentStakeBigInteger < amount){
                my_event_str_one("Insufficient Staked Balance!");
            } 
            // Transfere os tokens do contrato para o titular e atualiza o mapeamento de stake.
            Nep17Token.Transfer(Runtime.ExecutingScriptHash, account, amount, null);
            StakeMap.Put(account, currentStakeBigInteger - amount);
        }

        // Método para obter a quantidade de tokens em stake de uma conta.
        public static BigInteger GetStakedAmount(UInt160 account)
        {
            ByteString stakeAmount = AsByteString(StakeMap.Get(account));
            return (BigInteger) stakeAmount;
        }
    }
}

