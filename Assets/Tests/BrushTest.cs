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
        private GameObject cam;

        [SetUp]
        public void Setup()
        {
            cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Main Camera"));
            GameObject gameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Draw Manager"));
            brush = gameObj.GetComponent<DrawManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(brush.gameObject);
            Object.Destroy(cam.gameObject);
        }


        [UnityTest]
        public IEnumerator BrushTest_ChangeBrushSizeSuccess()
        {
            float size = 0.2f;
            brush.SetBrushSize(size);

            yield return null;

            Assert.AreEqual(size, brush.drawPrefab.GetComponent<TrailRenderer>().startWidth);
        }

        [UnityTest]
        public IEnumerator BrushTest_ChangeBrushSizeFailed()
        {
            float originalSize = brush.drawPrefab.GetComponent<TrailRenderer>().startWidth;
            float size = -0.2f;
            brush.SetBrushSize(size);

            yield return null;

            Assert.AreEqual(originalSize, brush.drawPrefab.GetComponent<TrailRenderer>().startWidth);
            Assert.AreNotEqual(size, brush.drawPrefab.GetComponent<TrailRenderer>().startWidth);
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
