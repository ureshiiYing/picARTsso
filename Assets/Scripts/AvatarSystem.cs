using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class AvatarSystem : MonoBehaviour
{
    public RawImage display;

    private Texture2D myAvatar; // stores the selected avatar
    private Texture2D[] avatars;

    private int pointer;

    // Start is called before the first frame update
    private void Start()
    {
        avatars = Resources.LoadAll<Texture2D>("Avatar");
        pointer = 0;

        SetDisplay(avatars[pointer]);
    }

    private void SetDisplay(Texture2D image)
    {
        display.GetComponent<RawImage>().texture = image;
    }

    public void Toggle(int direction)
    {
        // 0 = left, 1 = right
        Assert.IsTrue(direction == 0 || direction == 1);
        if (direction == 0)
        {
            pointer -= 1;
        } 
        else if (direction == 1)
        {
            pointer += 1;
        }

        if (pointer < 0)
        {
            Debug.Log("preceeding");
            pointer = avatars.Length - 1;

        }
        else if (pointer >= avatars.Length)
        {
            Debug.Log("exceeding");
            pointer = 0;
        }

        SetDisplay(avatars[pointer]);
    }

    // tag to connect to lobby button
    public void SetAvatar()
    {
        myAvatar = avatars[pointer];

        // set in player info? not sure if a player exists on the network at menu screen.
        //StartCoroutine(CoUpdatePlayerAvatar());
    }

    private IEnumerator CoUpdatePlayerAvatar()
    {
        // code to set player URL
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("Avatar", myAvatar);
        Debug.Log("added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);

    }
}
