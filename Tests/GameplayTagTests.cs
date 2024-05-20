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
   }
}
