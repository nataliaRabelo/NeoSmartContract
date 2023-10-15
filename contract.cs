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
        static readonly string Reviewer1 = "A";
        static readonly string Reviewer2 = "B";
        static readonly string Reviewer3 = "C";
        static readonly string Y = "Y";
        static readonly string N = "N";
        static readonly string R = "R";
        public static event Action<string, UInt160> my_event;
        public static event Action<string, int> my_event_int;
        [OpCode(OpCode.CONVERT, "0x28")]
        public static extern ByteString AsByteString(ByteString buffer);

        /// <summary>
        /// Submete um artigo para revisão, verificando o estado das revisões anteriores, se houver.
        /// </summary>
        /// <param name="article">O hash do artigo a ser submetido.</param>
        /// <param name="author">O autor do artigo.</param>
        /// <returns>Retorna 'true' se o artigo for submetido com sucesso. 'false' se o autor não for verificado, o artigo já tiver sido reprovado, ou não for aprovado com ressalvas.</returns>
        public static bool SubmitArticle(UInt160 article, UInt160 author)
        {
            if (!Runtime.CheckWitness(author)) return false;
            string strArticle = (string)(ByteString)(byte[])article;
            string strAuthor = (string)(ByteString)(byte[])author;
            Storage.Put(Storage.CurrentContext, strAuthor, strArticle);
            /**
            ByteString reviewCheck = GetReviews(article1);
            if(reviewCheck == "Y"){ // verifica se é a primeira vez que o artigo é submetido
                Storage.Put(Storage.CurrentContext, author, article1);
                return true;
            } else if(reviewCheck == "Y"){ // verifica se o artigo foi aprovado com ressalvas e se pode ser enviado novamente
                Storage.Put(Storage.CurrentContext, author, article2);
                return true;
            }else{ // verifica se o artigo já recebeu avaliação de reprovado e se não pode ser submetido
                return false;
            }
            */
            return true;
        }

        /// <summary>
        /// Atribui revisores para avaliar artigos.
        /// </summary>
        /// <param name="editor">O editor responsável pela atribuição.</param>
        /// <param name="reviewer1">O primeiro revisor.</param>
        /// <param name="reviewer2">O segundo revisor.</param>
        /// <param name="reviewer3">O terceiro revisor.</param>
        /// <returns>Retorna 'true' se os revisores forem atribuídos com sucesso, 'false' caso contrário.</returns>
        public static bool AssignReviewers(UInt160 editor, UInt160 reviewer1, UInt160 reviewer2, UInt160 reviewer3)
        {
            if (!Runtime.CheckWitness(editor)) return false;
            string strReviewer1 = (string)(ByteString)(byte[])reviewer1;
            string strReviewer2 = (string)(ByteString)(byte[])reviewer2;
            string strReviewer3 = (string)(ByteString)(byte[])reviewer3;
            Storage.Put(Storage.CurrentContext, Reviewer1, strReviewer1);
            Storage.Put(Storage.CurrentContext, Reviewer2, strReviewer2);
            Storage.Put(Storage.CurrentContext, Reviewer3, strReviewer3);
            return true;
        }

        /// <summary>
        /// Submete uma revisão para um artigo.
        /// </summary>
        /// <param name="reviewer">O revisor que está submetendo.</param>
        /// <param name="review">A revisão, que pode ser 'Y', 'N', ou 'R'.</param>
        /// <param name="article">O hash do artigo submetido a ser associado à revisão.</param>
        /// <param name="feedback">O hash do parecer da revisão.</param>
        /// <returns>Retorna 'true' se a revisão for submetida com sucesso, 'false' caso contrário.</returns>
        public static bool SubmitReview(UInt160 reviewer, UInt160 review, UInt160 article, UInt160 feedback)
        {
            if (!Runtime.CheckWitness(reviewer)) return false;
            //Verificando se a identidade do reviewer é igual a de um dos reviewers assinalados e se as revisões estão nos formatos corretos, sendo "Y" para aprovado "N" para reprovado e "R" para aprovado com ressalvas.
            if (CheckReviewer(reviewer)) // && (review.Equals(Y) || review.Equals(N) || review.Equals(R))
            {
                string strReviewer = (string)(ByteString)(byte[])reviewer;
                string strArticle = (string)(ByteString)(byte[])article;
                string strFeedback = (string)(ByteString)(byte[])feedback;
                string strReview = (string)(ByteString)(byte[])review;
                Storage.Put(Storage.CurrentContext, strArticle + strReviewer, strReview); // submetendo a nota da revisão de um determinado artigo
                Storage.Put(Storage.CurrentContext, strReviewer + strArticle + strFeedback, feedback); // submetendo o hash do parecer da revisão de um determinado artigo
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retorna as revisões dos revisores.
        /// </summary>
        /// <param name="article">O hash do artigo submetido.</param>
        /// <returns>Retorna as revisões agregadas como soma de inteiros.</returns>
        public static string GetReviews(UInt160 article)
        {
            string strArticle = (string)(ByteString)(byte[])article;
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            ByteString savedReviewer1bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer1));
            ByteString savedReviewer2bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer2));
            ByteString savedReviewer3bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer3));
            UInt160 savedReviewer1 = (UInt160)savedReviewer1bs;
            UInt160 savedReviewer2 = (UInt160)savedReviewer1bs;
            UInt160 savedReviewer3 = (UInt160)savedReviewer1bs;
            string strReviewer1 = (string)(ByteString)(byte[])savedReviewer1;
            string strReviewer2 = (string)(ByteString)(byte[])savedReviewer2;
            string strReviewer3 = (string)(ByteString)(byte[])savedReviewer3;

            // Recuperando as revisões utilizando as chaves que são endereços das wallets dos revisores.
            ByteString review1 = AsByteString(Storage.Get(Storage.CurrentContext, strArticle + strReviewer1));
            ByteString review2 = AsByteString(Storage.Get(Storage.CurrentContext, strArticle + strReviewer2));
            ByteString review3 = AsByteString(Storage.Get(Storage.CurrentContext, strArticle + strReviewer3));
            UInt160 review160One = (UInt160)review1;
            UInt160 review160Two = (UInt160)review2;
            UInt160 review160Three = (UInt160)review3;
            string strReview1 = (string)(ByteString)(byte[])review160One;
            string strReview2 = (string)(ByteString)(byte[])review160Two;
            string strReview3 = (string)(ByteString)(byte[])review160Three;
            return strReview1 + strReview2 + strReview3;
            //my_event_str("sumOfStrings",sumOfStrings);

            //return strReview1;
        }

        /// <summary>
        /// Método de debug.
        /// </summary>
        /// <param name="article">O artigo submetido.</param>
        /// <returns>Retorna as revisões agregadas como soma de inteiros.</returns>
        public static string GetReviews0(UInt160 article)
        {
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            my_event("article", article);
            ByteString savedReviewer1bs = AsByteString(Storage.Get(Storage.CurrentContext, "A"));
            //byte[] savedReviewer1bs = (byte[])Storage.Get(Storage.CurrentContext, Reviewer1);
            my_event_int("savedReviewer1bs",savedReviewer1bs.Length);
            UInt160 savedReviewer1 = (UInt160)savedReviewer1bs;
            my_event("savedReviewer1", savedReviewer1);
            string str1 = (string)(ByteString)(byte[])savedReviewer1;
            string strArticle = (string)(ByteString)(byte[])article;
            return str1 + strArticle;
            //return article;
            
        }

        /// <summary>
        /// Verifica se a identidade do reviewer é igual a de um dos reviewers assinalados 
        /// </summary>
        /// <param name="reviewer">O revisor a ser verificado.</param>
        /// <returns>Retorna 'true' se o revisor estiver sido assinalado anteriormente.</returns>
        public static bool CheckReviewer(UInt160 reviewer){
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            ByteString savedReviewer1bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer1));
            ByteString savedReviewer2bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer2));
            ByteString savedReviewer3bs = AsByteString(Storage.Get(Storage.CurrentContext, Reviewer3));
            UInt160 savedReviewer1 = (UInt160)savedReviewer1bs;
            UInt160 savedReviewer2 = (UInt160)savedReviewer2bs;
            UInt160 savedReviewer3 = (UInt160)savedReviewer3bs;
            string strInputReviewer = (string)(ByteString)(byte[])reviewer;
            string strReviewer1 = (string)(ByteString)(byte[])savedReviewer1;
            string strReviewer2 = (string)(ByteString)(byte[])savedReviewer2;
            string strReviewer3 = (string)(ByteString)(byte[])savedReviewer3;
            //Verificando se a identidade do reviewer é igual a de um dos reviewers assinalados.
            if (strInputReviewer.Equals(strReviewer1) || strInputReviewer.Equals(strReviewer2) || strInputReviewer.Equals(strReviewer3)){
                return true;
            }
            return false;
        }

        public static UInt160 getY(){
            return Y;
        }
    }
}
