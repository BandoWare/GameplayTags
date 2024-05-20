using BandoWare.GameplayTags;
using NUnit.Framework;
using System;
using System.Linq;
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

      public void DisposeIfNeeded(params T[] containers)
      {
         foreach (T container in containers)
         {
            if (container is IDisposable disposable)
            {
               disposable.Dispose();
            }
         }
      }

      [Test]
      public void HasTagExact()
      {
         T container = CreateContainer("Test.A.B");

         Assert.IsTrue(container.HasTagExact("Test.A.B"));
         Assert.IsFalse(container.HasTagExact("Test.A"));
         Assert.IsFalse(container.HasTagExact("Test"));

         DisposeIfNeeded(container);
      }

      [Test]
      public void HasTag()
      {
         T container = CreateContainer("Test.A.B");

         Assert.IsTrue(container.HasTag("Test.A.B"));
         Assert.IsTrue(container.HasTag("Test"));

         DisposeIfNeeded(container);
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

         DisposeIfNeeded(container, c0, c1, c2, c3);
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

         DisposeIfNeeded(container, c0, c1, c2, c3);
      }

      [Test]
      public void AddTag()
      {
         T container = CreateContainer();

         container.AddTag("Test.A.B.C0");

         Assert.IsTrue(container.HasTag("Test.A.B.C0"));
         Assert.IsFalse(container.HasTag("Test.A.B.C1"));

         DisposeIfNeeded(container);
      }

      [Test]
      public void AddTags()
      {
         T container = CreateContainer();
         T other = CreateContainer("Test.A.B.C0", "Test.A.B.C1");

         container.AddTags(other);

         Assert.IsTrue(container.HasTag("Test.A.B.C0"));
         Assert.IsTrue(container.HasTag("Test.A.B.C1"));

         DisposeIfNeeded(container, other);
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

         DisposeIfNeeded(container);

      }

      [Test]
      public void GetTags()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");

         GameplayTagEnumerator enumerator = container.GetTags();
         GameplayTag[] tags = enumerator.ToArray();

         CollectionAssert.Contains(tags, "Test");
         CollectionAssert.Contains(tags, "Test.A");
         CollectionAssert.Contains(tags, "Test.A.B");
         CollectionAssert.Contains(tags, "Test.A.B.C0");
         CollectionAssert.DoesNotContain(tags, "Test.A.B.C1");

         DisposeIfNeeded(container);
      }

      [Test]
      public void GetExplictTags()
      {
         T container = CreateContainer("Test.A", "Test.A.B.C0");

         GameplayTagEnumerator enumerator = container.GetExplicitTags();
         GameplayTag[] tags = enumerator.ToArray();

         CollectionAssert.DoesNotContain(tags, "Test");
         CollectionAssert.Contains(tags, "Test.A");
         CollectionAssert.DoesNotContain(tags, "Test.A.B");
         CollectionAssert.Contains(tags, "Test.A.B.C0");
         CollectionAssert.DoesNotContain(tags, "Test.A.B.C1");

         DisposeIfNeeded(container);
      }
   }
}
