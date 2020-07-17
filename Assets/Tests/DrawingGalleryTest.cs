using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NewTestScript
    {
        private DrawingGallery gallery;

        [SetUp]
        public void Setup()
        {
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Judging Canvas"));
            gallery = obj.GetComponent<DrawingGallery>();

        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator RandomiseIntArrTest()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
