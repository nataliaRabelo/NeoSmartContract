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
        static readonly ByteString Reviewer1 = "Reviewer1";
        static readonly ByteString Reviewer2 = "Reviewer2";
        static readonly ByteString Reviewer3 = "Reviewer3";
        static readonly ByteString Y = "Y";
        static readonly ByteString N = "N";
        static readonly ByteString R = "R";

        /// <summary>
        /// Submete um artigo para revisão, verificando o estado das revisões anteriores, se houver.
        /// </summary>
        /// <param name="article">O artigo a ser submetido.</param>
        /// <param name="author">O autor do artigo.</param>
        /// <param name="editor">O editor responsável pelo intermédio da revisão.</param>
        /// <returns>Retorna 'true' se o artigo for submetido com sucesso. 'false' se o autor não for verificado, o artigo já tiver sido reprovado, ou não for aprovado com ressalvas.</returns>
        public static bool SubmitArticle(ByteString article, UInt160 author, UInt160 editor)
        {
            if (!Runtime.CheckWitness(author)) return false;
            ByteString reviewCheck = CheckReviews(editor);
            if(reviewCheck == null){ // verifica se é a primeira vez que o artigo é submetido
                Storage.Put(Storage.CurrentContext, author, article);
                return true;
            } else if(reviewCheck.Equals(R)){ // verifica se o artigo foi aprovado com ressalvas e se pode ser enviado novamente
                Storage.Put(Storage.CurrentContext, author, article);
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
        /// <returns>Retorna 'true' se a revisão for submetida com sucesso, 'false' caso contrário.</returns>
        public static bool SubmitReview(UInt160 reviewer, ByteString review, ByteString article)
        {
            if (!Runtime.CheckWitness(reviewer)) return false;
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos.
            UInt160 savedReviewer1 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer1);
            UInt160 savedReviewer2 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer2);
            UInt160 savedReviewer3 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer3);
            //Verificando se a identidade do reviewer é igual a de um dos reviewers assinalados e se as revisões estão nos formatos corretos, sendo "Y" para aprovado "N" para reprovado e "R" para aprovado com ressalvas.
            if ((reviewer.Equals(savedReviewer1) || reviewer.Equals(savedReviewer2) || reviewer.Equals(savedReviewer3)) && (review.Equals(Y) || review.Equals(N) || review.Equals(R)))
            {
                Storage.Put(Storage.CurrentContext, reviewer, review);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Agrega as revisões dos revisores.
        /// </summary>
        /// <param name="editor">O editor responsável por agregar as revisões.</param>
        /// <returns>Retorna as revisões agregadas como uma sequência de bytes.</returns>
        public static ByteString AggregateReviews(UInt160 editor)
        {
            if (!Runtime.CheckWitness(editor)) return null;
            // Recuperando o endereço dos revisores através das chaves contidas nos atributos. 
            UInt160 savedReviewer1 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer1);
            UInt160 savedReviewer2 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer2);
            UInt160 savedReviewer3 = (UInt160)Storage.Get(Storage.CurrentContext, Reviewer3);
            // Recuperando as revisões utilizando as chaves que são endereços das wallets dos revisores.
            ByteString review1 = Storage.Get(Storage.CurrentContext, savedReviewer1);
            ByteString review2 = Storage.Get(Storage.CurrentContext, savedReviewer2);
            ByteString review3 = Storage.Get(Storage.CurrentContext, savedReviewer3);
            return review1 + review2 + review3;
        }

        /// <summary>
        /// Verifica as revisões para determinar o resultado com base na maioria de 'Y', 'N', ou 'R'.
        /// </summary>
        /// <param name="editor">O identificador do editor.</param>
        /// <returns>Retorna 'Y' se a maioria das revisões for 'Y', 'N' se a maioria for 'N', e 'R' para qualquer outra maioria ou se não houver maioria.</returns>
        public static ByteString CheckReviews(UInt160 editor){
            ByteString allReviews = AggregateReviews(editor);
            if (allReviews == null) return N; 

            int countY = 0;
            int countN = 0;
            int countR = 0;

            // Contando as letras Y, N e R
            foreach (byte b in allReviews)
            {
                if (b == (byte)'Y') countY++;
                else if (b == (byte)'N') countN++;
                else if (b == (byte)'R') countR++;
            }
            // Verificando qual letra é a maioria
            if (countY > countN && countY > countR)
            {
                // A maioria das revisões é "Y", o artigo foi aprovado
                return Y;
            }
            else if (countN > countY && countN > countR)
            {
                // A maioria das revisões é "N", o artigo foi reprovado
                return N;
            }
            else if (countR > countY && countR > countN)
            {
                // A maioria das revisões é "R", o artigo foi aprovado com ressalvas
                return R;
            }
            else
            {
                // Nenhuma letra é a maioria, então será aprovado com ressalvas
                return R;
            }
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
