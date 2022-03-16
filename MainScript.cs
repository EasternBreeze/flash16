using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    [SerializeField] private GameObject prefabFlashOverlay;
    [SerializeField] private GameObject effectPanelCrash;
    [SerializeField] private GameObject overlay;

    private readonly int[,] positions = new int[16, 2];
    private readonly int[] borderX = new int[5];
    private readonly int[] borderY = new int[5];
    private readonly int[] ranks = { 30000, 25000, 20000, 15000, 10000, 0 };

    [SerializeField] private AudioSource audBGM;
    [SerializeField] private AudioSource audSE;
    [SerializeField] private AudioSource audPitch;

    private bool firstPlay = true;
    private bool update = false;

    private int flashCode;

    private int score = 0;
    private int countSuccess = 0;
    private int countFailure = 0;
    private int time = 1800; // 1800
    private int reaction = 0;

    [SerializeField] private GameObject title;

    [SerializeField] private Text tCenter;
    [SerializeField] private Text tScore;
    [SerializeField] private Text tReaction;
    [SerializeField] private Text tTimeSec;
    [SerializeField] private Text tTimeMs;

    [SerializeField] private GameObject groupResult;
    [SerializeField] private GameObject shutter;
    [SerializeField] private Text tRsuccess;
    [SerializeField] private Text tRfailure;
    [SerializeField] private Text tRcps;
    [SerializeField] private Text tRscore;
    [SerializeField] private Image iRrank;

    [SerializeField] private AudioClip bgmTitle;
    [SerializeField] private AudioClip bgmMain;
    [SerializeField] private AudioClip seCountdown;
    [SerializeField] private AudioClip seStart;
    [SerializeField] private AudioClip seSuccess;
    [SerializeField] private AudioClip seFailure;
    [SerializeField] private AudioClip seFinish;
    [SerializeField] private AudioClip seResult;
    [SerializeField] private AudioClip seRank;

    [SerializeField] private Sprite[] rankTable = new Sprite[6];
    void Awake()
    {
        Application.targetFrameRate = 60;

        for (int i = 0; i < this.borderX.Length; i++)
        {
            this.borderX[i] = i * 150 - 300;
        }
        for (int i = 0; i < this.borderY.Length; i++)
        {
            this.borderY[i] = i * 150 - 300;
        }

        for (int i = 0; i < this.borderX.Length - 1; i++)
        {
            this.positions[i, 0] = borderX[i] + 75;
            this.positions[i + 4, 0] = borderX[i] + 75;
            this.positions[i + 8, 0] = borderX[i] + 75;
            this.positions[i + 12, 0] = borderX[i] + 75;
        }
        for (int i = 0; i < this.borderY.Length - 1; i++)
        {
            this.positions[i * 4, 1] = borderY[i] + 75;
            this.positions[i * 4 + 1, 1] = borderY[i] + 75;
            this.positions[i * 4 + 2, 1] = borderY[i] + 75;
            this.positions[i * 4 + 3, 1] = borderY[i] + 75;
        }
    }
    void Start()
    {
        AudioPlay(this.bgmTitle);
        StartCoroutine(PressStart());
    }

    private IEnumerator PressStart()
    {

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(AudioFadeout());
                break;
            }
            yield return null;
        }
        StartCoroutine(GameStart());
    }

    private IEnumerator GameStart()
    {
        SePlay(this.seFinish);
        Refresh();
        this.tScore.text = "0";
        this.tReaction.text = "---<size=30> / --f</size>";

        if (this.firstPlay)
        {
            this.firstPlay = false;
            for (int i = 1; i <= 6; i++)
            {
                this.title.transform.position = new Vector2(0, i * 120);
                yield return null;
            }
        }
        else
        {
            for (int i = 1; i <= 6; i++)
            {
                this.title.transform.position = new Vector2(0, 720 - (i * 120));
                yield return null;
            }
            this.shutter.transform.position = new Vector2(0, 720);
            this.groupResult.SetActive(false);
            this.overlay.SetActive(false);
            score = 0;
            countSuccess = 0;
            countFailure = 0;
            time = 1800;
            reaction = 0;
            Refresh();
            yield return new WaitForSeconds(1);
            for (int i = 1; i <= 6; i++)
            {
                this.title.transform.position = new Vector2(0, i * 120);
                yield return null;
            }
        }


        for (int i = 3; i > 0; i--)
        {
            SePlay(this.seCountdown);
            this.tCenter.text = $"{i:0}";
            yield return new WaitForSeconds(1);
            if (i == 3)
            {
                AudioPlay(this.bgmMain);
            }
        }

        SePlay(this.seStart);
        this.tCenter.text = "";
        RandomFlash(-1);
        this.overlay.SetActive(true);
        this.update = true;
    }

    void Update()
    {
        if (!this.update)
        {
            return;
        }

        this.time--;
        this.reaction++;

        if (this.time <= 300 && this.time > 0 && this.time % 60 == 0)
        {
            SePlay(this.seCountdown);
        }
        else if (this.time <= 0)
        {
            StartCoroutine(GameOver());
        }
        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }

        Refresh();
    }

    private void MouseClick()
    {
        int code = ClickPosition();

        if (code == this.flashCode)
        {
            ClickSuccess(code);
            return;
        }
        ClickFailure();
    }

    private IEnumerator GameOver()
    {
        SePlay(this.seFinish);
        this.update = false;
        StartCoroutine(AudioFadeout());

        for (int i = 1; i <= 6; i++)
        {
            this.shutter.transform.position = new Vector2(0, 720 - (i * 120));
            yield return null;
        }

        this.tRsuccess.text = "";
        this.tRfailure.text = "";
        this.tRcps.text = "";
        this.tRscore.text = "";
        this.iRrank.enabled = false;
        this.groupResult.SetActive(true);
        yield return new WaitForSeconds(2);

        SePlay(this.seResult);
        this.tRsuccess.text = "<i>" + this.countSuccess + "</i>";
        yield return new WaitForSeconds(0.5f);
        SePlay(this.seResult);
        this.tRfailure.text = "<i>" + this.countFailure + "</i>";
        yield return new WaitForSeconds(0.5f);
        SePlay(this.seResult);
        this.tRcps.text = $"<i>{1.0 * this.countSuccess / 30.0:F3}</i>";
        yield return new WaitForSeconds(0.5f);
        SePlay(this.seResult);
        this.tRscore.text = $"<i>{this.score:#,0}</i>";
        yield return new WaitForSeconds(0.5f);
        SePlay(this.seRank);
        for (int i = 0; i < this.ranks.Length; i++)
        {
            if (this.score >= this.ranks[i])
            {
                this.iRrank.sprite = this.rankTable[i];
                this.iRrank.enabled = true;
                break;
            }
        }

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }
        StartCoroutine(GameStart());
    }


    private int ClickPosition()
    {
        int x = (int)Input.mousePosition.x - 640;
        int y = (int)Input.mousePosition.y - 360;

        if (this.borderX[0] >= x || this.borderX[4] <= x || this.borderY[0] >= y || this.borderY[4] <= y)
        {
            return -1;
        }

        int code = -1;
        for (int i = 1; i < this.borderX.Length; i++)
        {
            if (this.borderX[i] > x)
            {
                code = i - 1;
                break;
            }
        }
        for (int i = 1; i < this.borderY.Length; i++)
        {
            if (this.borderY[i] > y)
            {
                code += (i - 1) * 4;
                break;
            }
        }

        return code;
    }

    private void ClickSuccess(int code)
    {
        this.countSuccess++;
        int add = 300;
        int act = this.reaction;
        this.reaction = 0;
        if (act <= 20)
        {
            SePlay(this.seSuccess, 1.4f);
            add = (add - act) * 13 / 10;
        }
        else if (act <= 40)
        {
            SePlay(this.seSuccess, 1.2f);
            add = (add - act) * 12 / 10;
        }
        else if (act <= 60)
        {
            SePlay(this.seSuccess, 1.1f);
            add = (add - act) * 11 / 10;
        }
        else
        {
            SePlay(this.seSuccess);
            add = add - act;
        }
        add = add >= 10 ? add : 10;
        AddScore(add, act);

        GameObject o = Instantiate(this.effectPanelCrash);
        o.transform.position = new Vector2(positions[code, 0], positions[code, 1]);
        Destroy(o, 0.333f);
        RandomFlash(this.flashCode);
    }
    private void ClickFailure()
    {
        SePlay(this.seFailure);
        this.countFailure++;
        AddScore(-300, -1);
    }

    private void RandomFlash(int disable)
    {
        int code = Random.Range(0, 16);
        while (code == disable)
        {
            code = Random.Range(0, 16);
        }

        this.flashCode = code;
        ReplacePanel();
    }

    private void ReplacePanel()
    {
        this.overlay.transform.position = new Vector2(this.positions[this.flashCode, 0], this.positions[this.flashCode, 1]);
    }
    private void Refresh()
    {
        this.tTimeSec.text = $"<i>{this.time / 60:00}</i>";
        this.tTimeMs.text = $"<i>.{this.time % 60 * 100 / 60:00}</i>";
    }
    private void AddScore(int add, int act)
    {
        this.score += add;
        this.score = this.score >= 0 ? this.score : 0;
        this.tScore.text = $"{this.score:#,0}";

        if (add > 0)
        {
            this.tReaction.text = $"+{add:0}<size=30> / {act:00}f</size>";
        }
        else
        {
            this.tReaction.text = $"{add:0}<size=30> / Miss</size>";
        }
        // TODO: スコア加算エフェクトとか
        Debug.Log(this.score + " +" + add + " (" + act + "f)");
    }

    private void AudioPlay(AudioClip clip)
    {
        this.audBGM.clip = clip;
        this.audBGM.volume = 0.5f;
        this.audBGM.Play();
    }
    private void SePlay(AudioClip clip)
    {
        this.audSE.PlayOneShot(clip);
    }
    private void SePlay(AudioClip clip, float pitch)
    {
        this.audPitch.pitch = pitch;
        this.audPitch.PlayOneShot(clip);
    }
    private IEnumerator AudioFadeout()
    {
        for (int i = 0; i < 60; i++)
        {
            this.audBGM.volume = (1.0f - i / 60.0f) / 2.0f;
            yield return null;
        }
        this.audBGM.volume = 0.0f;
        this.audBGM.clip = null;
    }

}
