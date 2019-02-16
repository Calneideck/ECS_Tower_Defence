using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    public Button[] continueButtons;
    public Vector3 camSpawnPos;
    public Vector3 camEndPos;
    public float camMoveTime = 0.5f;

    private Vector3 camStartPos;
    private int lastIndex;

    private void Start()
    {
        camStartPos = Camera.main.transform.position;

        for (int i = 0; i < continueButtons.Length; i++)
        {
            int index = i;
            continueButtons[index].onClick.AddListener(() => { Next(index); });
        }
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            if (Input.GetKeyDown(KeyCode.Space))
                Next(lastIndex);
    }

    void Next(int index)
    {
        transform.GetChild(index).gameObject.SetActive(false);

        if (index == 0)
            Camera.main.transform.DOMove(camSpawnPos, camMoveTime).SetEase(Ease.OutCubic);
        else if (index == 1)
            Camera.main.transform.DOMove(camEndPos, camMoveTime).SetEase(Ease.OutCubic);
        else if (index == 2)
            Camera.main.transform.DOMove(camStartPos, camMoveTime).SetEase(Ease.OutCubic);

        if (index == transform.childCount - 1)
        {
            PlayerPrefs.SetInt("tutorial", 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
            MainController.Instance.StartGame();
        }
        else
            transform.GetChild(index + 1).gameObject.SetActive(true);

        lastIndex++;
    }
}
