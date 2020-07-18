using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
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
        

        [SetUp]
        public void Setup()
        {
            GameObject gameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/Save Camera"));
            uploader = gameObj.GetComponent<UploadDownloadDrawing>();
            testImage = Resources.Load<Texture2D>("Test Prefab/pastel gradient v2");
            GameObject anotherGameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/FirebaseInit"));
            init = anotherGameObj.GetComponent<FirebaseInit>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(uploader.gameObject);
            Object.Destroy(init.gameObject);
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

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSecondsRealtime(2f);


            string path = uploader.GetDownloadURL();
            Assert.IsTrue(path == null);
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DownloadingImageSuccessful()
        {
            uploader.StartUpload(testImage);
            yield return new WaitForSecondsRealtime (2f);

            uploader.DownloadDrawing(uploader.GetDownloadURL());
            yield return new WaitForSecondsRealtime(2f);

            LogAssert.Expect(LogType.Log, "texture displayed");
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DownloadingImageFailedBecauseImageDoesNotExistAtThisURL()
        {
           
            uploader.DownloadDrawing("gs://picartsso.appspot.com/drawings/0f8b2d18-98a6-484e-ac41-ff56db2da351");

            yield return new WaitForSeconds(2f);
            
            LogAssert.Expect(LogType.Error, "Failed to download");
            LogAssert.Expect(LogType.Exception, "StorageException: Not Found.  Could not get object  Http Code: 404");

        }

        [UnityTest]
        public IEnumerator FirebaseTest_DeletingImageSuccess()
        {
            uploader.StartUpload(testImage);
            yield return new WaitForSecondsRealtime(2f);

            uploader.DeleteDrawing(uploader.GetDownloadURL());
            yield return new WaitForSecondsRealtime(2f);

            LogAssert.Expect(LogType.Log, "Successfully deleted");
        }

        [UnityTest]
        public IEnumerator FirebaseTest_DeletingImageFailedBecauseImageDoesNotExistAtThisURL()
        {
            uploader.StartUpload(testImage);
            yield return new WaitForSecondsRealtime(2f);

            uploader.DeleteDrawing(uploader.GetDownloadURL() + "mistake");
            yield return new WaitForSecondsRealtime(2f);

            LogAssert.Expect(LogType.Error, "Failed to delete");
        }

        [UnityTest]
        public IEnumerator FirebaseTest_SavingImageOnDeviceSuccessful()
        {
            string path = "gs://picartsso.appspot.com/drawings/844740b3-1ec5-43aa-ac47-de8b3970448c.png";
            uploader.SaveDrawingOnDevice(path);
            yield return new WaitForSeconds(1f);

            LogAssert.Expect(LogType.Log, "SaveToGallery called successfully in the Editor");
        }
    }
}
