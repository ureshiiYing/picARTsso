using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class FirebaseTest
    {
        private Texture2D testImage;
        private UploadDownloadDrawing uploader;
        private FirebaseInit init;
        private string testURL;

        [SetUp]
        public void Setup()
        {
            testURL = "gs://picartsso.appspot.com/drawings/0f8b2d18-98a6-484e-ac41-ff56db2da351.png";
            GameObject gameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Save Camera"));
            uploader = gameObj.GetComponent<UploadDownloadDrawing>();
            testImage = Resources.Load<Texture2D>("Test Prefab/pastel gradient v2");
            GameObject anotherGameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/FirebaseInit"));
            init = anotherGameObj.GetComponent<FirebaseInit>();
        }



        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator FirebaseTest_UploadingImageSuccess()
        {
            uploader.StartUpload(testImage);
            

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSecondsRealtime(2f);
            
            string path = uploader.GetDownloadURL();
            
            Assert.IsTrue(path != null);
        }

        [UnityTest]
        public IEnumerator FirebaseTest_UploadingImageFailedBecauseNoTextureGiven()
        {
            uploader.StartUpload(null);
            string path = uploader.GetDownloadURL();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSecondsRealtime(2f);


            Assert.IsTrue(path == null);
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DownloadingImageSuccessful()
        {
            uploader.DownloadDrawing(testURL);
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSecondsRealtime (2f);

            Assert.IsTrue(uploader.CheckSetDisplay());
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DownloadingImageFailed()
        {
            bool passed = true;
            try
            {
                uploader.DownloadDrawing("gs://picartsso.appspot.com/drawings/0f8b2d18-98a6-484e-ac41-ff56db2da351");
            }
            catch
            {
                passed = false;
            }
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSecondsRealtime(2f);
            Assert.IsFalse(passed);
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DeletingImage()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
