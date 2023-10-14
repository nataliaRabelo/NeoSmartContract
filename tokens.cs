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
    [ManifestExtra("Author", "Your Name")]
    [ManifestExtra("Email", "Your Email")]
    [ManifestExtra("Description", "Staking Token Contract with mint and burn functionality")]
    [SupportedStandards("NEP-17")]
    [ContractPermission("*", "onNEP17Payment")]
    public partial class StakingToken : Nep17Token
    {
        private static readonly UInt160 Owner = 
           InitialValueAttribute.InitContractAccount("NhGobEnuWX5rVdpnuZZAZExPoRs5J6D2Sb");
        private static StorageMap StakeMap => new StorageMap(Storage.CurrentContext, "stake");
        
        public override byte Decimals() => 8;
        public override string Symbol() => "STK";

        public static void _deploy(object data, bool update)
        {
            if (update) return;
            Nep17Token.Mint(Owner, 100_000_000 * (BigInteger.Pow(10, 8)));
        }

        private static bool IsOwner() => Runtime.CheckWitness(Owner);

        public static void MintTokens(UInt160 to, BigInteger amount)
        {
            if (!IsOwner()) throw new InvalidOperationException("No Authorization!");
            Nep17Token.Mint(to, amount);
        }

        public static void BurnTokens(UInt160 from, BigInteger amount)
        {
            if (!IsOwner() && !Runtime.CheckWitness(from)) throw new InvalidOperationException("No Authorization!");
            Nep17Token.Burn(from, amount);
        }

        public static void StakeTokens(UInt160 account, BigInteger amount)
        {
            if (!Runtime.CheckWitness(account)) throw new InvalidOperationException("No Authorization!");
            if (BalanceOf(account) < amount) throw new InvalidOperationException("Insufficient Balance!");

            Nep17Token.Transfer(account, Runtime.ExecutingScriptHash, amount, null);
            BigInteger currentStake = StakeMap.Get(account).ToBigInteger();
            StakeMap.Put(account, currentStake + amount);
        }

        public static bool SendTokens(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            if (!Runtime.CheckWitness(from)) throw new InvalidOperationException("No Authorization!");
            if (BalanceOf(from) < amount) throw new InvalidOperationException("Insufficient Balance!");
            
            return Nep17Token.Transfer(from, to, amount, data);
        }


        public static void UnstakeTokens(UInt160 account, BigInteger amount)
        {
            if (!Runtime.CheckWitness(account)) throw new InvalidOperationException("No Authorization!");
            BigInteger currentStake = StakeMap.Get(account).ToBigInteger();
            if (currentStake < amount) throw new InvalidOperationException("Insufficient Staked Balance!");

            Nep17Token.Transfer(Runtime.ExecutingScriptHash, account, amount, null);
            StakeMap.Put(account, currentStake - amount);
        }

        public static BigInteger GetStakedAmount(UInt160 account)
        {
            return StakeMap.Get(account).ToBigInteger();
        }
    }
}

