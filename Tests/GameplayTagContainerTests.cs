using BandoWare.GameplayTags;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TestTools;

namespace BandoWare.GameplayAbilities.Tests
{
   public class GameplayTagContainerTests : GameplayTagContainerTestsBase<GameplayTagContainer>
   {
      public override GameplayTagContainer CreateContainer()
      {
         return new GameplayTagContainer();
      }
   }

   public class GameplayTagCountContainerTests : GameplayTagContainerTestsBase<GameplayTagCountContainer>
   {
      public override GameplayTagCountContainer CreateContainer()
      {
         return new GameplayTagCountContainer();
      }

      [Test]
      public void TagEvent()
      {
         GameplayTagCountContainer container = CreateContainer();

         StrongBox<int> onAnyCountChangeCalledTimes = new();
         void OnAnyCountChangeCallback(GameplayTag tag, int count)
         {
            onAnyCountChangeCalledTimes.Value++;
         }

         container.RegisterTagEventCallback("Test.A", GameplayTagEventType.AnyCountChange, OnAnyCountChangeCallback);

         StrongBox<int> newOrRemovedCalledTimes = new();
         void NewOrRemovedCallback(GameplayTag tag, int count)
         {
            newOrRemovedCalledTimes.Value++;
         }

         container.RegisterTagEventCallback("Test.A", GameplayTagEventType.NewOrRemoved, NewOrRemovedCallback);

         container.AddTag("Test.A");

         Assert.AreEqual(1, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(1, newOrRemovedCalledTimes.Value);

         container.AddTag("Test.A");

         Assert.AreEqual(2, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(1, newOrRemovedCalledTimes.Value);

         container.RemoveTag("Test.A");

         Assert.AreEqual(3, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(1, newOrRemovedCalledTimes.Value);

         container.RemoveTag("Test.A");

         Assert.AreEqual(4, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(2, newOrRemovedCalledTimes.Value);

         container.AddTag("Test.A.B");

         Assert.AreEqual(5, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(3, newOrRemovedCalledTimes.Value);

         container.RemoveTag("Test.A.B");

         Assert.AreEqual(6, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(4, newOrRemovedCalledTimes.Value);

         container.RemoveTagEventCallback("Test.A", GameplayTagEventType.NewOrRemoved, NewOrRemovedCallback);
         container.RemoveTagEventCallback("Test.A", GameplayTagEventType.AnyCountChange, OnAnyCountChangeCallback);

         container.AddTag("Test.A");
         container.RemoveTag("Test.A");

         Assert.AreEqual(6, onAnyCountChangeCalledTimes.Value);
         Assert.AreEqual(4, newOrRemovedCalledTimes.Value);
      }
   }

   public abstract class GameplayTagContainerTestsBase<T> where T : IGameplayTagContainer
   {
      public abstract T CreateContainer();

      public T CreateContainer(params GameplayTag[] tags)
      {
         T container = CreateContainer();
         foreach (GameplayTag tag in tags)
         {
            container.AddTag(tag);
         }

         return container;
      }

      [Test]
      public void HasTagExact()
      {
         T container = CreateContainer("Test.A.B");

         Assert.IsTrue(container.HasTagExact("Test.A.B"));
         Assert.IsFalse(container.HasTagExact("Test.A"));
         Assert.IsFalse(container.HasTagExact("Test"));
      }

      [Test]
      public void HasTag()
      {
         T container = CreateContainer("Test.A.B");

         Assert.IsTrue(container.HasTag("Test.A.B"));
         Assert.IsTrue(container.HasTag("Test"));
      }

      [Test]
      public void HasAll()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");

         T c0 = CreateContainer("Test.A", "Test.A.B", "Test.A.B.C1");
         T c1 = CreateContainer("Test.A", "Test.A.B.C0");
         T c2 = CreateContainer("Test.A.B", "Test.A.B.C0", "Test.A.B.C1");
         T c3 = CreateContainer("Test", "Test.A");

         Assert.IsFalse(c0.HasAll(container));
         Assert.IsTrue(container.HasAll(c1));
         Assert.IsFalse(container.HasAll(c2));
         Assert.IsTrue(container.HasAll(c3));
      }

      [Test]
      public void HasAllExact()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");

         T c0 = CreateContainer("Test.A", "Test.A.B", "Test.A.B.C1");
         T c1 = CreateContainer("Test.A", "Test.A.B.C0");
         T c2 = CreateContainer("Test.A.B", "Test.A.B.C0");
         T c3 = CreateContainer("Test", "Test.A");

         Assert.IsFalse(c0.HasAllExact(container));
         Assert.IsTrue(container.HasAllExact(c1));
         Assert.IsFalse(container.HasAllExact(c2));
         Assert.IsFalse(container.HasAllExact(c3));
      }

      [Test]
      public void AddTag()
      {
         T container = CreateContainer("Test.D");

         container.AddTag("Test.A.B.C0");

         CollectionAssert.AreEqual(new GameplayTag[]
         {
            "Test.A.B.C0", "Test.D"
         }, container.GetExplicitTags());

         CollectionAssert.AreEqual(new GameplayTag[]
         {
            "Test", "Test.A", "Test.A.B", "Test.A.B.C0", "Test.D"
         }, container.GetTags());
      }

      [Test]
      public void AddTags()
      {
         T container = CreateContainer("Test.D");
         T other = CreateContainer("Test.A.B.C0", "Test.A.B.C1");

         container.AddTags(other);

         CollectionAssert.AreEqual(new GameplayTag[]
         {
            "Test.A.B.C0", "Test.A.B.C1", "Test.D"
         }, container.GetExplicitTags());

         CollectionAssert.AreEqual(new GameplayTag[]
         {
            "Test", "Test.A", "Test.A.B", "Test.A.B.C0", "Test.A.B.C1", "Test.D"
         }, container.GetTags());
      }

      [Test]
      public void RemoveTag()
      {
         T container = CreateContainer("Test.A");

         container.RemoveTag("Test.A");

         Assert.IsFalse(container.HasTag("Test.A"));
         Assert.IsFalse(container.HasTagExact("Test.A"));

         GameplayTag tag = "Test.A.B";
         container.RemoveTag(tag);

         LogAssert.Expect(LogType.Warning,
            $"Attempted to remove tag {tag} from tag count container," +
            " but it is not explicitly added to the container.");
      }

      [Test]
      public void GetTags()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");
         GameplayTag[] expectedTags = new GameplayTag[] { "Test", "Test.A", "Test.A.B", "Test.A.B.C0" };
         CollectionAssert.AreEqual(expectedTags, container);
      }

      [Test]
      public void GetExplictTags()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");
         GameplayTag[] expectedTags = new GameplayTag[] { "Test.A", "Test.A.B.C0" };
         CollectionAssert.AreEqual(expectedTags, container.GetExplicitTags());
      }
   }
}
