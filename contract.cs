using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace PeerReview
{
    public class Contract1 : SmartContract
    {
        //Constantes 
        static readonly ByteString Reviewer1 = "A";
        static readonly ByteString Reviewer2 = "B";
        static readonly ByteString Reviewer3 = "C";
        static readonly BigInteger Y = 2;
        static readonly BigInteger N = -2;
        static readonly BigInteger R = 1;

        /// <summary>
        /// Submete um artigo para revisão, verificando o estado das revisões anteriores, se houver.
        /// </summary>
        /// <param name="article1">O hash do artigo a ser submetido.</param>
        /// <param name="article2">O hash do artigo a ser submetido em caso de aprovação com ressalvas.</param>
        /// <param name="author">O autor do artigo.</param>
        /// <returns>Retorna 'true' se o artigo for submetido com sucesso. 'false' se o autor não for verificado, o artigo já tiver sido reprovado, ou não for aprovado com ressalvas.</returns>
        public static bool SubmitArticle(UInt160 article1, UInt160 article2, UInt160 author)
        {
            if (!Runtime.CheckWitness(author)) return false;
            BigInteger reviewCheck = GetReviews(article1);
            if(reviewCheck == -7){ // verifica se é a primeira vez que o artigo é submetido
                Storage.Put(Storage.CurrentContext, author, article1);
                return true;
            } else if(reviewCheck >= 1){ // verifica se o artigo foi aprovado com ressalvas e se pode ser enviado novamente
                Storage.Put(Storage.CurrentContext, author, article2);
                return true;
            }else{ // verifica se o artigo já recebeu avaliação de reprovado e se não pode ser submetido
                return false;
            }
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
            Storage.Put(Storage.CurrentContext, Reviewer1, reviewer1);
            Storage.Put(Storage.CurrentContext, Reviewer2, reviewer2);
            Storage.Put(Storage.CurrentContext, Reviewer3, reviewer3);
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
        public static bool SubmitReview(UInt160 reviewer, BigInteger review, UInt160 article, UInt160 feedback)
        {
            if (!Runtime.CheckWitness(reviewer)) return false;
            //Verificando se a identidade do reviewer é igual a de um dos reviewers assinalados e se as revisões estão nos formatos corretos, sendo "Y" para aprovado "N" para reprovado e "R" para aprovado com ressalvas.
            if (CheckReviewer(reviewer) && (review.Equals(Y) || review.Equals(N) || review.Equals(R)))
            {
                Storage.Put(Storage.CurrentContext, article + reviewer, review); // submetendo a nota da revisão de um determinado artigo
                Storage.Put(Storage.CurrentContext, reviewer + article + (UInt160)review, feedback); // submetendo o hash do parecer da revisão de um determinado artigo
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retorna as revisões dos revisores.
        /// </summary>
        /// <param name="article">O artigo submetido.</param>
        /// <returns>Retorna as revisões agregadas como soma de inteiros.</returns>
        public static BigInteger GetReviews(UInt160 article)
        {
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            UInt160 savedReviewer1 = (UInt160)(Storage.Get(Storage.CurrentContext, Reviewer1));
            UInt160 savedReviewer2 = (UInt160)(Storage.Get(Storage.CurrentContext, Reviewer2));
            UInt160 savedReviewer3 = (UInt160)(Storage.Get(Storage.CurrentContext, Reviewer3));

            // Recuperando as revisões utilizando as chaves que são endereços das wallets dos revisores.
            BigInteger review1 = (BigInteger)(Storage.Get(Storage.CurrentContext, article + savedReviewer1));
            BigInteger review2 = (BigInteger)(Storage.Get(Storage.CurrentContext, article + savedReviewer2));
            BigInteger review3 = (BigInteger)(Storage.Get(Storage.CurrentContext, article + savedReviewer3));

            if(review1 == 0 && review2 == 0 && review3 == 0){
                return -7;
            }
            return review1 + review2 + review3;
        }

        /// <summary>
        /// Verifica se a identidade do reviewer é igual a de um dos reviewers assinalados 
        /// </summary>
        /// <param name="reviewer">O revisor a ser verificado.</param>
        /// <returns>Retorna 'true' se o revisor estiver sido assinalado anteriormente.</returns>
        public static bool CheckReviewer(UInt160 reviewer){
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            UInt160 savedReviewer1 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer1);
            UInt160 savedReviewer2 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer2);
            UInt160 savedReviewer3 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer3);
            //Verificando se a identidade do reviewer é igual a de um dos reviewers assinalados.
            if (reviewer.Equals(savedReviewer1) || reviewer.Equals(savedReviewer2) || reviewer.Equals(savedReviewer3)){
                return true;
            }
            return false;
        }

        /// <summary>
        /// Destroi o contrato inteligente atual.
        /// </summary>
        /// <param name="editor">O endereço do editor, que deve ser uma entidade autorizada para destruir o contrato.</param>
        /// <exception cref="Exception">Lança uma exceção se o chamador não for autorizado a destruir o contrato.</exception>
        public static void Destroy(UInt160 editor)
        {
            if (!Runtime.CheckWitness(editor)) throw new Exception("No authorization.");
            ContractManagement.Destroy();
        }
    }
}
