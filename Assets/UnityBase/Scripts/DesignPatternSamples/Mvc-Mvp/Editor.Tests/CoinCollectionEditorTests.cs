using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityBase.Mvc.Architecture;
using UnityBase.Observable;

namespace UnityBase.Mvc.EditorTest
{
    public class CoinCollectionEditorTests
    {
        [Test]
        public void CoinCollectionEditorTestsSimplePasses()
        {
            // 1st level Is/Has/Does/Contains
            // 2nd level All/Not/Some/Exactly
            // Or/And/Not
            // Is.Unique / Is.Ordered
            // Assert.IsTrue

            var userName = "User123";
            Assert.That(userName, Does.StartWith("U"));
            Assert.That(userName, Does.EndWith("3"));

            var list = new List<int> { 1, 2, 3, 4, 5 };
            Assert.That(list, Contains.Item(3));
            Assert.That(list, Is.All.Positive);
            Assert.That(list, Has.Exactly(2).LessThan(3));
            Assert.That(list, Is.Ordered);
            Assert.That(list, Is.Unique);
            Assert.That(list, Has.Exactly(3).Matches<int>(NumberPredicates.IsOdd));
        }

        private ICoinController _controller;
        private ICoinView _view;
        private ICoinModel _model;
        private ICoinService _service;

        [SetUp]
        public void SetUp()
        {
            _view = Substitute.For<ICoinView>();
            _service = Substitute.For<ICoinService>();
            _model = Substitute.For<ICoinModel>();
            
            Assert.That(_view, Is.Not.Null);
            Assert.That(_service, Is.Not.Null);
            Assert.That(_model, Is.Not.Null);

            _model.Coins.Returns(new Observable<int>(0));
            Assert.That(_model.Coins, Is.Not.Null);
            Assert.That(_model, Has.Property("Coins").Not.Null);

            _service.Load().Returns(_model);
            _controller = new CoinController.Builder().WithService(_service).WithView(_view).Build();

            Assert.That(_controller, Is.Not.Null);
        }

        [TearDown]
        public void TearDown() { }

        [Test]
        public void CoinControllerBuilder_Build_ShouldThrowArgumentNullException_WhenViewIsNull()
        {
            Assert.That(()=> new CoinController.Builder().Build(), Throws.ArgumentNullException);
        }
        
        [Test]
        public void CoinControllerBuilder_Build_ShouldThrowArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(()=> new CoinController.Builder().WithService(null).WithView(_view).Build());
        }

        [Test]
        public void UpdateView_ShouldUpdateCoinsDisplay_WhenCoinsAreCollected()
        {
            _controller.Collect(1);
            _view.Received(1).UpdateCoinsDisplay(1);
        }

        [TestCase(5, 5, 10)]
        [TestCase(0, 5, 5)]
        [TestCase(0, 0, 0)]
        public void Collect_ShouldAddCoins_WhenCalledWithAPositiveNumber(int initialCoins, int coinsToAdd, int expectedCoins)
        {
            _model.Coins.Returns(new Observable<int>(initialCoins));
            _controller.Collect(coinsToAdd);
            Assert.That(_model.Coins.Value, Is.EqualTo(expectedCoins));
        }

    }

    public static class NumberPredicates
    {
        public static bool IsEven(int number) => number % 2 == 0;
        public static bool IsOdd(int number) => number % 2 != 0;
    }
}
