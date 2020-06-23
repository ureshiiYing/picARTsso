using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BrushTest
    {
        private DrawManager brush;

        [SetUp]
        public void Setup()
        {
            MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Main Camera"));
            GameObject gameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Draw Manager"));
            brush = gameObj.GetComponent<DrawManager>();
        }

        //[TearDown]
        //public void Teardown()
        //{
        //    Object.Destroy(brush.gameObject);
        //    Object.Destroy(cam.gameObjec);
        //}


        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator BrushTest_ChangeBrushSize()
        {
            float size = 0.2f;
            brush.SetBrushSize(size);

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            Assert.AreEqual(brush.drawPrefab.GetComponent<TrailRenderer>().startWidth, size);
        }

        [UnityTest]
        public IEnumerator BrushTest_ChangePickerColourToRed()
        {
            brush.SetBrushColour(Color.red);

            yield return null;
            Assert.AreEqual(Color.red, brush.colourPickerPanel.GetComponent<FlexibleColorPicker>().color);
        }

        [UnityTest]
        public IEnumerator BrushTest_ChangeBrushColourToRed()
        {
            brush.ChangeBrushColour(Color.red);

            yield return null;
            Assert.AreEqual(Color.red, brush.drawPrefab.GetComponent<TrailRenderer>().startColor);
        }

        [UnityTest]
        public IEnumerator BrushTest_ClearAnyStrokes()
        {
            GameObject drawPrefab = Resources.Load<GameObject>("Test Prefab/Test Brush");
            // create some strokes
            MonoBehaviour.Instantiate(drawPrefab, new Vector3(Random.Range(0, 10), Random.Range(0, 10)), Quaternion.identity);
            MonoBehaviour.Instantiate(drawPrefab, new Vector3(Random.Range(0, 20), Random.Range(0, 10)), Quaternion.identity);

            yield return null;

            brush.Clear();

            yield return null;
            var strokesLeft = GameObject.FindGameObjectsWithTag("Brush Line").Length;

            Assert.AreEqual(strokesLeft, 0);

        }
    }
}
