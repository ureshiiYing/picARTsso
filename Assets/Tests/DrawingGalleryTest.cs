using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class DrawingGalleryTestScript
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
        public IEnumerator RandomiseIntArrIsRandomisedTest()
        {
            bool res = true;
            int[] v1 = gallery.RandomiseIntArray(6);
            yield return null;

            for (int i = 0; i < 6; i++)
            {
                if (v1[i] != i)
                {
                    res = false;
                }
            }

            Assert.AreEqual(false, res);
        }
    }
}
