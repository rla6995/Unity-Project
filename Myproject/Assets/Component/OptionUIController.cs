using UnityEngine;

public class OptionUIController : MonoBehaviour
{
    public GameObject optionMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionMenu.activeSelf)
            {
                AudioManager.Instance?.PlaySE(0);
                optionMenu.SetActive(false); // 옵션창 닫기
            }
        }
    }

    public void CloseOptionMenu()
    {
        AudioManager.Instance?.PlaySE(0);
        optionMenu.SetActive(false);
    }

    public void OpenOptionMenu()
    {
        AudioManager.Instance?.PlaySE(0);
        optionMenu.SetActive(true);
    }
}
