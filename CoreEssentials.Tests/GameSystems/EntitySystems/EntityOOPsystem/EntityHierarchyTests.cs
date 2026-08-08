using System;
using System.Collections.Generic;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntityHierarchyTests
    {
        private EntitySystem CreateSystem() => new EntitySystem();

        // ===== Parent-Child Storage (T1) =====

        [Fact]
        public void AddChild_SetsParentAndAddsToChildrenCollection()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.AddChild(child);

            Assert.Equal(parent, child.Parent);
            Assert.Contains(child, parent.Children);
        }

        [Fact]
        public void AddChild_RemovesFromPreviousParent()
        {
            var system = CreateSystem();
            var oldParent = system.CreateEntity<TestEntity>();
            var newParent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            oldParent.AddChild(child);
            newParent.AddChild(child);

            Assert.Equal(newParent, child.Parent);
            Assert.DoesNotContain(child, oldParent.Children);
            Assert.Contains(child, newParent.Children);
        }

        [Fact]
        public void AddChild_ThrowsWhenChildIsNull()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();

            var ex = Assert.Throws<ArgumentNullException>(() => parent.AddChild(null!));
            Assert.Equal("child", ex.ParamName);
        }

        [Fact]
        public void AddChild_ThrowsWhenAddingSelf()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Throws<ArgumentException>(() => entity.AddChild(entity));
        }

        [Fact]
        public void AddChild_ThrowsOnCircularReference()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.AddChild(child);

            Assert.Throws<ArgumentException>(() => child.AddChild(parent));
        }

        [Fact]
        public void RemoveChild_RemovesParentAndFromChildrenCollection()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.AddChild(child);
            var result = parent.RemoveChild(child);

            Assert.True(result);
            Assert.Null(child.Parent);
            Assert.DoesNotContain(child, parent.Children);
        }

        [Fact]
        public void RemoveChild_ReturnsFalseWhenChildNotFound()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var other = system.CreateEntity<TestEntity>();

            var result = parent.RemoveChild(other);

            Assert.False(result);
        }

        [Fact]
        public void LocalPosition_DefaultsToZero()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Equal(Vector2.Zero, entity.LocalPosition);
        }

        [Fact]
        public void LocalRotation_DefaultsToZero()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Equal(0f, entity.LocalRotation);
        }

        // ===== Transform Inheritance (T2) =====

        [Fact]
        public void ChildPosition_ReturnsParentPositionPlusLocalOffset()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.Position = new Vector2(100, 200);
            parent.AddChild(child);
            child.LocalPosition = new Vector2(10, 20);

            Assert.Equal(new Vector2(110, 220), child.Position);
        }

        [Fact]
        public void ChildRotation_ReturnsParentRotationPlusLocalOffset()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.Rotation = MathHelper.PiOver4;
            parent.AddChild(child);
            child.LocalRotation = MathHelper.PiOver4;

            Assert.Equal(MathHelper.PiOver2, child.Rotation);
        }

        [Fact]
        public void NestedHierarchy_PositionAccumulatesCorrectly()
        {
            var system = CreateSystem();
            var root = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();
            var grandchild = system.CreateEntity<TestEntity>();

            root.Position = new Vector2(100, 100);
            root.AddChild(child);
            child.LocalPosition = new Vector2(50, 50);
            child.AddChild(grandchild);
            grandchild.LocalPosition = new Vector2(25, 25);

            Assert.Equal(new Vector2(150, 150), child.Position);
            Assert.Equal(new Vector2(175, 175), grandchild.Position);
        }

        [Fact]
        public void NestedHierarchy_RotationAccumulatesCorrectly()
        {
            var system = CreateSystem();
            var root = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();
            var grandchild = system.CreateEntity<TestEntity>();

            root.Rotation = MathHelper.PiOver4;
            root.AddChild(child);
            child.LocalRotation = MathHelper.PiOver4;
            child.AddChild(grandchild);
            grandchild.LocalRotation = MathHelper.PiOver4;

            Assert.Equal(MathHelper.PiOver2, child.Rotation);
            Assert.Equal(3 * MathHelper.PiOver4, grandchild.Rotation);
        }

        [Fact]
        public void WithoutParent_PositionReturnsOwnPosition()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            entity.Position = new Vector2(42, 42);

            Assert.Equal(new Vector2(42, 42), entity.Position);
        }

        [Fact]
        public void WithoutParent_RotationReturnsOwnRotation()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            entity.Rotation = MathHelper.PiOver2;

            Assert.Equal(MathHelper.PiOver2, entity.Rotation);
        }

        // ===== Hierarchy Lifecycle (T3) =====

        [Fact]
        public void Destroy_ParentDestroysAllChildren()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child1 = system.CreateEntity<TestEntity>();
            var child2 = system.CreateEntity<TestEntity>();

            parent.AddChild(child1);
            parent.AddChild(child2);
            parent.Destroy();

            Assert.True(parent.Destroyed);
            Assert.True(child1.Destroyed);
            Assert.True(child2.Destroyed);
        }

        [Fact]
        public void Destroy_NestedHierarchyDestroysAllDescendants()
        {
            var system = CreateSystem();
            var root = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();
            var grandchild = system.CreateEntity<TestEntity>();

            root.AddChild(child);
            child.AddChild(grandchild);
            root.Destroy();

            Assert.True(root.Destroyed);
            Assert.True(child.Destroyed);
            Assert.True(grandchild.Destroyed);
        }

        [Fact]
        public void SetActive_FalseDeactivatesAllChildren()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child1 = system.CreateEntity<TestEntity>();
            var child2 = system.CreateEntity<TestEntity>();

            parent.AddChild(child1);
            parent.AddChild(child2);
            parent.SetActive(false);

            Assert.False(parent.GetActive());
            Assert.False(child1.GetActive());
            Assert.False(child2.GetActive());
        }

        [Fact]
        public void SetActive_TrueActivatesAllChildren()
        {
            var system = CreateSystem();
            var parent = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();

            parent.AddChild(child);
            parent.SetActive(false);

            parent.SetActive(true);

            Assert.True(parent.GetActive());
            Assert.True(child.GetActive());
        }

        [Fact]
        public void SetActive_NestedHierarchyPropagatesCorrectly()
        {
            var system = CreateSystem();
            var root = system.CreateEntity<TestEntity>();
            var child = system.CreateEntity<TestEntity>();
            var grandchild = system.CreateEntity<TestEntity>();

            root.AddChild(child);
            child.AddChild(grandchild);
            root.SetActive(false);

            Assert.False(root.GetActive());
            Assert.False(child.GetActive());
            Assert.False(grandchild.GetActive());
        }

        // ===== Test Entity =====

        private class TestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}
