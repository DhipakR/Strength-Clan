using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour, PageController
{
    public TextMeshProUGUI levelText, coinText, messageText;
    public CharacterSide currentSide = CharacterSide.Front;
    public Button shopButton, emotesButton, achievementButton;
    public Button leftButton;
    public Button rightButton;
    public ImageSpriteLoop characterImage;
    //public List<string> spriteFolderNames;
    public SkeletonGraphic characterSkeletonGraphic;
    public SkeletonDataAsset[] characterSkeletonDataVer1;
    public SkeletonDataAsset[] characterSkeletonDataVer2;
    private string animName = "";
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        levelText.GetComponent<Button>().onClick.AddListener(LevelDetailPopup);
    }
    private void Start()
    {
        levelText.text = "Level " + userSessionManager.Instance.characterLevel.ToString();
        shopButton.onClick.AddListener(ShopeButtonClick);
        emotesButton.onClick.AddListener(EmotesButtonClick);
        achievementButton.onClick.AddListener(AchievementButtonClick);
        shopButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        emotesButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        achievementButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        leftButton.onClick.AddListener(OnLeftButtonPressed);
        rightButton.onClick.AddListener(OnRightButtonPressed);
        leftButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        rightButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }
    private void OnEnable()
    {
        coinText.text = userSessionManager.Instance.currentCoins.ToString();
        currentSide = CharacterSide.Front;
        UpdateCharacterView();
    }

    public void ShopeButtonClick()
    {
        StateManager.Instance.OpenStaticScreen("character", gameObject, "shopScreen", null, true);
        StateManager.Instance.CloseFooter();
    }
    public void EmotesButtonClick()
    {
        GlobalAnimator.Instance.ShowTextMessage(messageText, "Coming Soon...", 2);
        //StateManager.Instance.OpenStaticScreen("character", gameObject, "emotesScreen", null, true);
        //StateManager.Instance.CloseFooter();
    }
    public void AchievementButtonClick()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            { "onFooter", true },{"backAction",true}
        };
        StateManager.Instance.OpenStaticScreen("character", gameObject, "achievementScreen", mData, true);
        StateManager.Instance.CloseFooter();
    }
    public void LevelDetailPopup()
    {
        PopupController.Instance.OpenPopup("character", "levelDetailPopup", null, null);
    }

    public void OnLeftButtonPressed()
    {
        if (currentSide == CharacterSide.Front)
        {
            currentSide = CharacterSide.Side;
        }
        else if (currentSide == CharacterSide.Side)
        {
            currentSide = CharacterSide.Back;
        }
        else if (currentSide == CharacterSide.Back)
        {
            currentSide = CharacterSide.BackLeft;
        }
        else if (currentSide == CharacterSide.BackLeft)
        {
            currentSide = CharacterSide.SideLeft;
        }
        else if (currentSide == CharacterSide.SideLeft)
        {
            currentSide = CharacterSide.FrontLeft;
        }
        else if (currentSide == CharacterSide.FrontLeft)
        {
            currentSide = CharacterSide.Front;
        }
        UpdateCharacterView();
    }

    public void OnRightButtonPressed()
    {
        if (currentSide == CharacterSide.Front)
        {
            currentSide = CharacterSide.FrontLeft;
        }
        else if (currentSide == CharacterSide.FrontLeft)
        {
            currentSide = CharacterSide.SideLeft;
        }
        else if (currentSide == CharacterSide.SideLeft)
        {
            currentSide = CharacterSide.BackLeft;
        }
        else if (currentSide == CharacterSide.BackLeft)
        {
            currentSide = CharacterSide.Back;
        }
        else if (currentSide == CharacterSide.Back)
        {
            currentSide = CharacterSide.Side;
        }
        else if (currentSide == CharacterSide.Side)
        {
            currentSide = CharacterSide.Front;
        }
        UpdateCharacterView();
    }
    private void UpdateCharacterView()
    {
        //userSessionManager.Instance.characterLevel = 3;
        switch (currentSide)
        {
            case CharacterSide.Front:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[2], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[2], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " front");
                break;
            case CharacterSide.Side:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[1], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[1], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " side");
                break;
            case CharacterSide.Back:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[0], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = Vector3.one * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, -3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[0], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " back");
                break;
            case CharacterSide.FrontLeft:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[2], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[2], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " back");
                break;
            case CharacterSide.SideLeft:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[1], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[1], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " back");
                break;
            case CharacterSide.BackLeft:
                if (userSessionManager.Instance.characterLevel <= 3)
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer1[0], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                else
                {
                    characterSkeletonGraphic.transform.localScale = new Vector3(-1, 1, 1) * 0.25f;
                    characterSkeletonGraphic.transform.localRotation = Quaternion.Euler(0, 0, 3f);
                    SetSkeletonDataAsset(characterSkeletonGraphic, characterSkeletonDataVer2[0], new string[] { GetNameSkin(userSessionManager.Instance.clotheName) });
                }
                //LoadSpriteFromFolder(userSessionManager.Instance.clotheName + " back");
                break;
        }
        SetAnimation(characterSkeletonGraphic, animName, true);
    }
    private void LoadSpriteFromFolder(string folderName)
    {
        string path = ($"{userSessionManager.Instance.gifsPath}{userSessionManager.Instance.GetGifFolder(userSessionManager.Instance.characterLevel)}{folderName}");
        print(path);
        var sprites = Resources.LoadAll<Sprite>(path);
        if (sprites.Length > 0)
        {
            var sortedSprites = sprites
           .OrderBy(sprite =>
            {
                // Extract the numeric part of the name using Regex
                var match = Regex.Match(sprite.name, @"\d+$");
                return match.Success ? int.Parse(match.Value) : 0; // Parse number, default to 0 if no match
            }).ToArray();

            var match = Regex.Match(userSessionManager.Instance.GetGifFolder(userSessionManager.Instance.characterLevel), @"\d+(?=/)");
            if (match.Success)
            {
                int extractedValue = int.Parse(match.Value);
                if (extractedValue == 0)
                    characterImage.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                else if (extractedValue == 1)
                    characterImage.transform.localScale = new Vector3(0.62f, 0.62f, 0.62f);

            }
            characterImage.ResetGif(sortedSprites);
        }
        else
        {
            Debug.LogError($"No sprites found in folder: {folderName}");
        }
    }
    #region Spine
    public void SetSkeletonDataAsset(SkeletonGraphic ske, SkeletonDataAsset asset, string[] nameSkins)
    {
        if (!ske.gameObject.activeInHierarchy) return;
        StartCoroutine(WaitSkeleton(ske, () =>
        {
            string name = nameSkins[0];

            ske.Clear();
            ske.initialSkinName = name;
            ske.skeletonDataAsset = asset;

            Skeleton skeleton = ske.Skeleton;
            SkeletonData skeletonData = skeleton.Data;

            if (skeletonData.FindSkin(name) == null)
            {
                Debug.LogError(string.Format("Name skin {0} is null", name));
            }

            ske.Initialize(true);
            ske.SetMaterialDirty();
            SetSkinSkeleton(ske, nameSkins);
        }));
    }
    public void SetSkinSkeleton(SkeletonGraphic ske, string[] nameSkins)
    {
        StartCoroutine(WaitSkeleton(ske, () => { ChangeSkinSkeleton(ske, nameSkins); }));
    }
    public void ChangeSkinSkeleton(SkeletonGraphic ske, string[] nameSkins)
    {
        Skeleton skeleton = ske.Skeleton;
        SkeletonData skeletonData = skeleton.Data;

        Skin characterSkin = new Skin(nameSkins[0]);

        for (int i = 0; i < nameSkins.Length; i++)
        {
            string name = nameSkins[i];

            if (skeletonData.FindSkin(name) != null)
            {
                characterSkin.AddSkin(skeletonData.FindSkin(name));
            }
            else
            {
                Debug.LogError(string.Format("Name skin {0} is null", name));
            }
        }

        skeleton.SetSkin(characterSkin);
        skeleton.SetSlotsToSetupPose();
        ske.Update(0);
    }
    IEnumerator WaitSkeleton(SkeletonGraphic ske, Action callback = null)
    {
        yield return new WaitUntil(() => ske != null && ske.AnimationState != null);
        if (ske)
        {
            callback?.Invoke();
        }
    }
    private string GetNameSkin(string nameSkinCheck)
    {
        if (userSessionManager.Instance.characterLevel >= 4)
            animName = "idle_suite";
        else
            animName = "idle";
        string nameSkinSet = "default";
        switch (nameSkinCheck)
        {
            case "armour":
                nameSkinSet = "armor";

                break;
            case "axe warrior":
                nameSkinSet = "warrior";
                break;
            case "boxing":
                nameSkinSet = "boxing";
                break;
            case "compression shirt":
                nameSkinSet = "gym";
                break;
            case "karate gi":
                nameSkinSet = "karate";
                break;
            case "ninja":
                nameSkinSet = "ninja";
                break;
            case "no clothes":
                nameSkinSet = "default";
                break;
            case "suit":
                nameSkinSet = "suite";
                break;
            case "superhero":
                nameSkinSet = "superhero";
                break;
            case "trenches":
                nameSkinSet = "original";
                break;
            case "zeus":
                nameSkinSet = "zeus";
                break;
        }
        return nameSkinSet;
    }
    public void SetAnimation(SkeletonGraphic ske, string name, bool loop, float timeScale = 1f, Action callBack = null, Action callBackStart = null)
    {
        if (!ske.gameObject.activeInHierarchy) return;
        StartCoroutine(WaitSkeleton(ske, () =>
        {
            Spine.Animation runAnimation = ske.SkeletonData.FindAnimation(name);
            if (runAnimation != null)
            {
                TrackEntry animationEntry = ske.AnimationState.SetAnimation(0, name, loop);
                animationEntry.TimeScale = timeScale;

                if (callBackStart != null)
                {
                    animationEntry.Start += trackEntry => { callBackStart(); };
                }

                if (callBack != null)
                {
                    animationEntry.Complete += trackEntry => { callBack(); };
                }
            }
            else
            {
                Debug.LogError(string.Format("{0} is null", name));
            }
        }));
    }
    #endregion
}
