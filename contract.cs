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
    Classe que representa um contrato de revisão de pares
    **/
    public class PeerReview : SmartContract
    {
        /**
        TODO: Refatorar para pegar criptografado do sistema
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
        /**
        TODO: Refatorar para deixar anônimo só as coisas que importam, como o nome dos autores de forma criptografada.
        **/
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
                byte[] anonText = GetAnonText();
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