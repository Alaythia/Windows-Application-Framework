﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Waf.Foundation;
using System.Waf.UnitTesting;

namespace Test.Waf.Foundation
{
    [TestClass]
    public class ObservableListTest
    {
        [TestMethod]
        public void PropertyChangedEventTest()
        {
            var list = new ObservableList<TestModel>(new[] { new TestModel { Name = "first" } });

            bool countChangedHandlerCalled = false;
            list.PropertyChanged += PropertyChangedHandler;
            list.Add(new TestModel { Name = "second" });
            Assert.IsTrue(countChangedHandlerCalled);

            countChangedHandlerCalled = false;
            list.PropertyChanged -= PropertyChangedHandler;
            list.Add(new TestModel { Name = "third" });
            Assert.IsFalse(countChangedHandlerCalled);

            void PropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
            {
                Assert.AreEqual(list, sender);
                if (e.PropertyName == nameof(list.Count)) countChangedHandlerCalled = true;
            }
        }

        [TestMethod]
        public void CollectionChangingEventTest()
        {
            var list = new ObservableList<TestModel>();

            var collectionChangingArgs = new List<NotifyCollectionChangedEventArgs>();
            var collectionChangedArgs = new List<NotifyCollectionChangedEventArgs>();

            list.CollectionChanging += CollectionChangingHandler;
            list.CollectionChanged += CollectionChangedHandler;
            list.Add(new TestModel { Name = "first" });
            AssertLastListEvent();

            list.Insert(1, new TestModel { Name = "second" });
            AssertLastListEvent();

            list.Insert(1, new TestModel { Name = "third" });
            AssertLastListEvent();

            list.Move(2, 0);
            AssertLastListEvent();

            list[1] = new TestModel { Name = "fourth" };
            AssertLastListEvent();

            list.Remove(list[0]);
            AssertLastListEvent();

            list.Clear();
            AssertLastListEvent();

            Assert.AreEqual(7, collectionChangingArgs.Count);

            void CollectionChangingHandler(object? sender, NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(collectionChangedArgs.Count, collectionChangingArgs.Count);
                Assert.AreEqual(list, sender);
                collectionChangingArgs.Add(e);
            }

            void CollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(list, sender);
                collectionChangedArgs.Add(e);
                Assert.AreEqual(collectionChangedArgs.Count, collectionChangingArgs.Count);
            }

            void AssertLastListEvent() => AssertEqualEventArgs(collectionChangedArgs.Last(), collectionChangingArgs.Last());

            static void AssertEqualEventArgs(NotifyCollectionChangedEventArgs expected, NotifyCollectionChangedEventArgs actual)
            {
                Assert.AreEqual(expected.Action, actual.Action);
                Assert.AreEqual(expected.NewStartingIndex, actual.NewStartingIndex);
                Assert.AreEqual(expected.OldStartingIndex, actual.OldStartingIndex);
                AssertHelper.SequenceEqual(ToGeneric(expected.NewItems), ToGeneric(actual.NewItems));
                AssertHelper.SequenceEqual(ToGeneric(expected.OldItems), ToGeneric(actual.OldItems));

                static IEnumerable<object> ToGeneric(IList? list) => list?.OfType<object>() ?? Array.Empty<object>();
            }
        }


        private class TestModel : Model
        {
            private string? name;

            public string? Name { get => name; set => SetProperty(ref name, value); }
        }
    }
}
