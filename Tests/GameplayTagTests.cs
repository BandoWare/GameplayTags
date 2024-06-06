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
   }
}
