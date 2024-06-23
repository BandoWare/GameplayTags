using BandoWare.GameplayTags;
using NUnit.Framework;
using System;

namespace BandoWare.GameplayAbilities.Tests
{
   public class GameplayTagTests
   {
      [Test]
      public void ValidateName()
      {
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName(null));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName(""));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName("   "));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName(".A"));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName(".."));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName("A."));
         Assert.Throws<ArgumentException>(() => GameplayTagUtility.ValidateName("$@"));

         Assert.DoesNotThrow(() => GameplayTagUtility.ValidateName("CrowdControl.Slow.Ice"));
         Assert.DoesNotThrow(() => GameplayTagUtility.ValidateName("Dead"));
         Assert.DoesNotThrow(() => GameplayTagUtility.ValidateName("AllowedCharacters._123456789"));
      }

      [Test]
      public void ComparisonTests()
      {
         GameplayTag a0 = "Test.A";
         GameplayTag a1 = "Test.A";
         GameplayTag b = "Test.A.B";

         Assert.IsTrue(a0 == a1);
         Assert.IsTrue(a0 != b);
         Assert.IsFalse(a0 == b);
         Assert.IsFalse(a0.Equals(b));
         Assert.IsTrue(a0.Equals("Test.A"));
      }

      [Test]
      public void IsParentTests()
      {
         GameplayTag test = "Test";
         GameplayTag a = "Test.A";
         GameplayTag b = "Test.A.B";

         Assert.IsTrue(test.IsParentOf(a));
         Assert.IsTrue(test.IsParentOf(b));
         Assert.IsTrue(a.IsParentOf(b));
         Assert.IsTrue(!b.IsParentOf(b));
         Assert.IsTrue(!b.IsParentOf(a));
         Assert.IsTrue(!b.IsParentOf(test));
      }

      [Test]
      public void IsChildOfTests()
      {
         GameplayTag test = "Test";
         GameplayTag a = "Test.A";
         GameplayTag b = "Test.A.B";

         Assert.IsTrue(a.IsChildOf(test));
         Assert.IsTrue(b.IsChildOf(test));
         Assert.IsTrue(b.IsChildOf(a));
         Assert.IsTrue(!b.IsChildOf(b));
         Assert.IsTrue(!a.IsChildOf(b));
         Assert.IsTrue(!test.IsChildOf(b));
      }

      [Test]
      public void ParentTagTests()
      {
         GameplayTag test = "Test";
         GameplayTag parent = "Test.Parent";
         GameplayTag firstChild = "Test.Parent.FirstChild";
         GameplayTag secondChild = "Test.Parent.SecondChild";
         GameplayTag grandson = "Test.Parent.SecondChild.Grandson";

         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent }, firstChild.ParentTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent }, secondChild.ParentTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent, secondChild }, grandson.ParentTags.ToArray());
      }

      [Test]
      public void ChildTagsTests()
      {
         GameplayTag parent = "Test.Parent";
         GameplayTag firstChild = "Test.Parent.FirstChild";
         GameplayTag secondChild = "Test.Parent.SecondChild";
         GameplayTag grandson = "Test.Parent.SecondChild.Grandson";

         CollectionAssert.AreEquivalent(new GameplayTag[] { firstChild, secondChild, grandson }, parent.ChildTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { grandson }, secondChild.ChildTags.ToArray());
         CollectionAssert.IsEmpty(grandson.ChildTags.ToArray());
      }

      [Test]
      public void HierarchyTagsTests()
      {
         GameplayTag test = "Test";
         GameplayTag parent = "Test.Parent";
         GameplayTag firstChild = "Test.Parent.FirstChild";
         GameplayTag secondChild = "Test.Parent.SecondChild";
         GameplayTag grandson = "Test.Parent.SecondChild.Grandson";

         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent, secondChild, grandson }, grandson.HierarchyTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent, secondChild }, secondChild.HierarchyTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent, firstChild }, firstChild.HierarchyTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test, parent }, parent.HierarchyTags.ToArray());
         CollectionAssert.AreEquivalent(new GameplayTag[] { test }, test.HierarchyTags.ToArray());
      }
   }
}
