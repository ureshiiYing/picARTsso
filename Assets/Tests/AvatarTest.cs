using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class AvatarTest
    {
        private AvatarSystem avatarSystem;

        [SetUp]
        public void Setup()
        {
            GameObject gameObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Test Prefab/AvatarSystem"));
            avatarSystem = gameObj.GetComponent<AvatarSystem>();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ToggleAvatarRight14TimesTest()
        {
            Texture2D drawing = Resources.Load<Texture2D>("Avatar/Untitled35_20200716225919");
            for(int i = 0; i < 14; i++)
            {
                avatarSystem.Toggle(1);
            }
            yield return null;
            Texture2D currDisplay = (Texture2D)avatarSystem.display.GetComponent<RawImage>().texture;

            Assert.AreEqual(drawing, currDisplay);
        }

        [UnityTest]
        public IEnumerator ToggleAvatarLeft1TimeTest()
        {
            Texture2D drawing = Resources.Load<Texture2D>("Avatar/Untitled35_20200716230334");
            
            avatarSystem.Toggle(0);
            
            yield return null;
            Texture2D currDisplay = (Texture2D)avatarSystem.display.GetComponent<RawImage>().texture;

            Assert.AreEqual(drawing, currDisplay);
        }
    }
}
