using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityBase.Mvc.Architecture;
using UnityBase.Mvc.EditorTest;
using UnityBase.Visitor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UnityBase.Mvc.RuntimeTest
{
    public class CoinCollectionRuntimeTests 
    {
        [Test]
        public void VerifyApplicationPlaying()
        {
            Assert.That(Application.isPlaying, Is.True);
        }
        
        [Test]
        [LoadScene("Assets/UnityBase/Scripts/DesignPatterns/JustSample/Mvc-Mvp/MvcTest.unity")]
        public void VerifyScene()
        {
            var go = GameObject.Find("Hero");
            Debug.Log(go);
            Debug.Log(SceneManager.GetActiveScene().path);
            Assert.That(go, Is.Not.Null, "Hero not found in {0}", SceneManager.GetActiveScene().path);
        }

        [UnityTest]
        public IEnumerator Accept_ShouldExecuteVisit_WhenCalledWithVisitor()
        {
            var obj = new GameObject();
            var coinComponent = obj.AddComponent<CoinComponent>();
            coinComponent.controller = Substitute.For<ICoinController>();
            
            var pickUp = new GameObject();
            var visitor = pickUp.AddComponent<TestVisitor>();
            yield return null;

            coinComponent.Accept(visitor);
            yield return null;
            
            Assert.That(visitor.Visited, Is.True);
        }
        
        public class TestVisitor : MonoBehaviour, IVisitor
        {
            public bool Visited { get; private set; }
            public void Visit<T>(T visitable) where T : IVisitable
            {
                Visited = true;
            }
        }
    }
}