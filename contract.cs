// SPDX-License-Identifier: MIT
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace HelloWorldDemo
{
    public class Contract1 : SmartContract
    {
        static readonly UInt160 Editor = default;
        static readonly UInt160 Reviewer1 = default;
        static readonly UInt160 Reviewer2 = default;
        static readonly UInt160 Reviewer3 = default;

        public static bool SubmitArticle(ByteString article, UInt160 author)
        {
            if (!Runtime.CheckWitness(author)) return false;
            Storage.Put(Storage.CurrentContext, author, article);
            return true;
        }

        public static bool AssignReviewers(UInt160 editor)
        {
            if (!Runtime.CheckWitness(editor)) return false;
            Storage.Put(Storage.CurrentContext, "Reviewer1", Reviewer1);
            Storage.Put(Storage.CurrentContext, "Reviewer2", Reviewer2);
            Storage.Put(Storage.CurrentContext, "Reviewer3", Reviewer3);
            return true;
        }

        public static bool SubmitReview(UInt160 reviewer, ByteString review)
        {
            if (!Runtime.CheckWitness(reviewer)) return false;
            UInt160 savedReviewer1 = (UInt160)Storage.Get(Storage.CurrentContext, "Reviewer1");
            UInt160 savedReviewer2 = (UInt160)Storage.Get(Storage.CurrentContext, "Reviewer2");
            UInt160 savedReviewer3 = (UInt160)Storage.Get(Storage.CurrentContext, "Reviewer3");

            if (reviewer.Equals(savedReviewer1) || reviewer.Equals(savedReviewer2) || reviewer.Equals(savedReviewer3))
            {
                Storage.Put(Storage.CurrentContext, reviewer, review);
                return true;
            }

            return false;
        }

        public static ByteString AggregateReviews(UInt160 editor)
        {
            if (!Runtime.CheckWitness(editor)) return null;
            ByteString review1 = Storage.Get(Storage.CurrentContext, "Reviewer1");
            ByteString review2 = Storage.Get(Storage.CurrentContext, "Reviewer2");
            ByteString review3 = Storage.Get(Storage.CurrentContext, "Reviewer3");
            return review1 + review2 + review3;
        }

    }
}
