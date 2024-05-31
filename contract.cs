using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

namespace PeerReview
{
    [ManifestExtra("Author", "Natália Rabelo")]
    [ManifestExtra("Email", "nataliabruno@id.uff.br")]
    [ManifestExtra("Description", "Sistema de revisão de pares científicos")]
    public class Contract1 : SmartContract
    {
        //Constantes 
        static readonly string Reviewer1Key = "Reviewer1";
        static readonly string Reviewer2Key = "Reviewer2";
        static readonly string Reviewer3Key = "Reviewer3";
        static readonly string Y = "Y";
        static readonly string N = "N";
        static readonly string R = "R";
        public static event Action<string> print;
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);

        public static bool SubmitArticle(string article, UInt160 author)
        {
            if (!Runtime.CheckWitness(author)) return false;
            string strAuthor = (string)(ByteString)(byte[])author;
            Storage.Put(Storage.CurrentContext, strAuthor, article);
            return true;
        }

        public static bool ResubmitArticle(string article, UInt160 author)
        {
            if (!Runtime.CheckWitness(author)) return false;
            if (DetermineArticleStatus(article) != R) return false;
            string strAuthor = (string)(ByteString)(byte[])author;
            Storage.Put(Storage.CurrentContext, strAuthor, article);
            return true;
        }

        public static bool AssignReviewers(UInt160 editor, UInt160 reviewer1, UInt160 reviewer2, UInt160 reviewer3)
        {
            if (!Runtime.CheckWitness(editor)) return false;
            string strReviewer1 = (string)(ByteString)(byte[])reviewer1;
            string strReviewer2 = (string)(ByteString)(byte[])reviewer2;
            string strReviewer3 = (string)(ByteString)(byte[])reviewer3;
            Storage.Put(Storage.CurrentContext, Reviewer1Key, strReviewer1);
            Storage.Put(Storage.CurrentContext, Reviewer2Key, strReviewer2);
            Storage.Put(Storage.CurrentContext, Reviewer3Key, strReviewer3);
            return true;
        }

        public static bool SubmitReview(UInt160 reviewer, string review, string article)
        {
            if (!Runtime.CheckWitness(reviewer)) return false;
            if (CheckReviewer(reviewer) && (review == Y || review == N || review == R))
            {
                string strReviewer = (string)(ByteString)(byte[])reviewer;
                Storage.Put(Storage.CurrentContext, strReviewer + article, review);
                return true;
            }
            return false;
        }

        public static string GetReviews(string article)
        {
            string strReviewer1 = (string)(ByteString)(byte[])GetReviewer(Reviewer1Key);
            string strReviewer2 = (string)(ByteString)(byte[])GetReviewer(Reviewer2Key);
            string strReviewer3 = (string)(ByteString)(byte[])GetReviewer(Reviewer3Key);
            string review1 = Storage.Get(Storage.CurrentContext, strReviewer1 + article);
            string review2 = Storage.Get(Storage.CurrentContext, strReviewer2 + article);
            string review3 = Storage.Get(Storage.CurrentContext, strReviewer3 + article);
            return review1 + review2 + review3;
        }

        public static bool CheckReviewer(UInt160 reviewer)
        {
            string strInputReviewer = (string)(ByteString)(byte[])reviewer;
            string strReviewer1 = Storage.Get(Storage.CurrentContext, Reviewer1Key);
            string strReviewer2 = Storage.Get(Storage.CurrentContext, Reviewer2Key);
            string strReviewer3 = Storage.Get(Storage.CurrentContext, Reviewer3Key);
            return strInputReviewer == strReviewer1 || strInputReviewer == strReviewer2 || strInputReviewer == strReviewer3;
        }

        public static string DetermineArticleStatus(string article)
        {
            string reviews = GetReviews(article);

            int countY = 0;
            int countN = 0;
            int countR = 0;

            foreach (char review in reviews)
            {
                if (review == 'Y') countY++;
                else if (review == 'N') countN++;
                else if (review == 'R') countR++;
            }

            if (countY > countN && countY > countR){
                return Y;
                print("Aprovado");
            }
            else if (countN > countY && countN > countR){
                return N;
                print("Reprovado");
            }
            else if (countR > countY && countR > countN){
                return R;
                print("Resubmeter");
            } 
            else if (countY == 1 && countN == 1 && countR == 1){
                return R;
                print("Resubmeter");
            }
            else{
                return "I";
                print("Indefinido, falta alguma das três revisões necessárias.");
            } 
        }

        public static UInt160 GetReviewer(string s){
            ByteString storageResult = AsByteString(Storage.Get(Storage.CurrentContext, s));
            return (UInt160)storageResult;
        }
    }
}
