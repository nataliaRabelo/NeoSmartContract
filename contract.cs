using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.System;
using System;
using System.ComponentModel;
using System.Numerics;

/**
@author Natália Bruno Rabelo
**/
namespace PeerReviewContract
{
    /**
    Classe que representa um contrato de revisão de pares.
    **/
    public class PeerReview : SmartContract
    {
        
        /**
        Acessa a storage do contrato e buscar o autor associado ao texto. 
        Esse autor foi armazenado como chave na storage do contrato, e o valor associado a essa chave é o texto submetido pelo autor.
        **/
        private static byte[] GetAuthor(byte[] text)
        {
            return Storage.Get(Storage.CurrentContext, text);
        }

        /**
        Recebe o parâmetro author, que representa o autor do texto original, e usa esse valor para obter o texto correspondente a partir do armazenamento. 
        Em seguida, ele chama a função Anonymize() para obter o texto anônimo correspondente, que é então retornado.
        **/
        private static byte[] GetAnonText(byte[] author) {
        byte[] text = Storage.Get(Storage.CurrentContext, author);
        if (text == null) return null;
        return Anonymize(text);
        }

        /**
        Retorna a revisão final de um texto.
        **/
        private static byte[] GetFinalReview(byte[] reviews)
        {
            if (reviews == null) return null;
            BigInteger numReviews = reviews.Length / 33;
            BigInteger numApprovals = 0;

        // Verifica se há maioria de aprovação entre os revisores.
        for (int i = 0; i < numReviews; i++)
        {
            byte[] reviewer = new byte[33];
            Array.Copy(reviews, i * 33, reviewer, 0, 33);

            byte[] review = new byte[1];
            Array.Copy(reviews, 33 * numReviews + i, review, 0, 1);

            if (review[0] == 1) numApprovals++;
        }

        if (numApprovals >= numReviews / 2 + 1)
        {
            // Retorna a revisão final.
            return new byte[] { 1 };
        }
        else
        {
            // Retorna nulo caso a revisão final não tenha sido aprovada.
            return null;
        }
        }

        /**
        Retorna o editor armazenado anteriormente na chave "editor" no Storage.
        **/
        private static byte[] GetEditor()
        {
            return Storage.Get(Storage.CurrentContext, "editor");
        }
        
        /**
        TODO: Refatorar para pegar essa galera criptografada do sistema
        Retorna revisores de acordo com ordem aleatória
        **/
        private static byte[] GetReviewers()
        {
        // Coloque aqui os endereços dos possíveis revisores
        byte[][] possibleReviewers = new byte[][]
        {
            new byte[] {0x01, 0x23, 0x45, 0x67},
            new byte[] {0x89, 0xab, 0xcd, 0xef},
            new byte[] {0xfe, 0xdc, 0xba, 0x98}
        };

        // Escolha um revisor aleatório
        Random rand = new Random();
        int index = rand.Next(possibleReviewers.Length);
        return possibleReviewers[index];
        }

        /**
        Recebe uma lista de avaliações, adiciona uma nova avaliação à lista e retorna a lista atualizada.
        **/
        private static byte[] Anonymize(byte[] text)
        {
            byte[] hash = Neo.Cryptography.SHA256(text);
            byte[] result = new byte[hash.Length];

            // Substitui cada byte do texto original com um byte correspondente ao mesmo índice no hash
            for (int i = 0; i < hash.Length; i++)
            {
                result[i] = hash[i];
            }

            return result;
        }

        /**
        Recebe uma lista de avaliações, adiciona uma nova avaliação à lista e retorna a lista atualizada.
        **/
        private static byte[] AppendReview(byte[] reviews, byte[] reviewer, byte[] review)
        {
            if (reviews == null || reviews.Length == 0) return new byte[][] {reviewer, review}.Serialize();
            object[] deserializedReviews = (object[])reviews.Deserialize();
            byte[][] newReview = new byte[][] {reviewer, review};
            object[] updatedReviews = new object[deserializedReviews.Length + 1];
            for (int i = 0; i < deserializedReviews.Length; i++)
            {
                updatedReviews[i] = deserializedReviews[i];
            }
            updatedReviews[updatedReviews.Length - 1] = newReview;
            return updatedReviews.Serialize();
        }

        public static object Main(string operation, object[] args)
        {
            if (operation == "submitText")
            {
                if (args.Length != 2) return false;
                byte[] author = (byte[])args[0];
                byte[] text = (byte[])args[1];
                Storage.Put(Storage.CurrentContext, author, text);
                return true;
            }
            else if (operation == "sendToEditor")
            {
                if (args.Length != 2) return false;
                byte[] author = (byte[])args[0];
                byte[] editor = (byte[])args[1];
                byte[] text = Storage.Get(Storage.CurrentContext, author);
                if (text == null) return false;
                byte[] anonText = Anonymize(text);
                Storage.Put(Storage.CurrentContext, anonText, editor);
                return true;
            }
            else if (operation == "sendToReviewers")
            {
                if (args.Length != 1) return false;
                byte[] anonText = (byte[])args[0];
                byte[] reviewers = GetReviewers();
                if (reviewers == null) return false;
                Storage.Put(Storage.CurrentContext, anonText, reviewers);
                return true;
            }
            else if (operation == "submitReview")
            {
                if (args.Length != 2) return false;
                byte[] reviewer = (byte[])args[0];
                byte[] review = (byte[])args[1];
                byte[] anonText = GetAnonText(reviewer);
                if (anonText == null) return false;
                byte[] reviews = Storage.Get(Storage.CurrentContext, anonText);
                if (reviews == null) return false;
                byte[] updatedReviews = AppendReview(reviews, reviewer, review);
                Storage.Put(Storage.CurrentContext, anonText, updatedReviews);
                return true;
            }
            else if (operation == "sendToEditorForApproval")
            {
                if (args.Length != 1) return false;
                byte[] anonText = (byte[])args[0];
                byte[] editor = GetEditor();
                if (editor == null) return false;
                byte[] reviews = Storage.Get(Storage.CurrentContext, anonText);
                if (reviews == null) return false;
                byte[] finalReview = GetFinalReview(reviews);
                if (finalReview == null) return false;
                Storage.Put(Storage.CurrentContext, anonText, editor);
                Storage.Put(Storage.CurrentContext, finalReview, editor);
                return true;
            }
            else if (operation == "sendToAuthor")
            {
                if (args.Length != 1) return false;
                byte[] anonText = (byte[])args[0];
                byte[] editor = GetEditor();
                if (editor == null) return false;
                byte[] finalReview = Storage.Get(Storage.CurrentContext, anonText);
                if (finalReview == null) return false;
                byte[] author = GetAuthor(anonText);
                if (author == null) return false;
                Storage.Put(Storage.CurrentContext, finalReview, author);
                return true;
            }
            return false;
        }

    }
    
}