using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class shopController : MonoBehaviour, PageController
{
    public TMP_InputField searchBar;
    public TextMeshProUGUI messageText;
    public Button backButton;
    public GameObject[] items;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {

    }
    private void Start()
    {
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        searchBar.onValueChanged.AddListener(SearchItems);
        ShopModel shopModel = ApiDataHandler.Instance.getShopData();
        foreach(GameObject item in items)
        {
            TextMeshProUGUI priceText = item.transform.Find("price text").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = item.transform.Find("item name").GetComponent<TextMeshProUGUI>();
            ShopItem itemData = GetShopItemByName(shopModel, nameText.text);
            if (itemData.buyed)
            {
                priceText.text = "Bought";
            }
            else
            {
                priceText.text=itemData.price.ToString();
            }
            item.GetComponent<Button>().onClick.AddListener(() => BuyItemButton(itemData, priceText,nameText));
            if (itemData.price == 0)
            {
                priceText.text = itemData.price.ToString();
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }
    public void BuyItemButton(ShopItem shopItem,TextMeshProUGUI priceText,TextMeshProUGUI nameText)
    {
        if (shopItem.price == 0)
        {
            StartCoroutine(SetClotheName("no clothes"));
            //ApiDataHandler.Instance.SetCloths(shopItem.itemName.ToLower());
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Clear Clothe", 2);
            return;
        }
        if (shopItem.buyed)
        {
            StartCoroutine(SetClotheName(shopItem.itemName.ToLower()));
            //ApiDataHandler.Instance.SetCloths(shopItem.itemName.ToLower());
            GlobalAnimator.Instance.ShowTextMessage(messageText, nameText.text+" Selected", 2);
        }
        else
        {
            if (userSessionManager.Instance.currentCoins >= shopItem.price)
            {
                //GlobalAnimator.Instance.FadeInLoader();
                List<object> initialData = new List<object> { shopItem,priceText,nameText};
                Action<List<object>> onFinish = Bought;
                PopupController.Instance.OpenPopup("character", "BoughtPopup", onFinish,initialData);

            }
            else
            {
                // insufficient coins
                GlobalAnimator.Instance.ShowTextMessage(messageText, "Insufficient coins", 2);
            }
        }
    }
    public void Bought(List<object> list)
    {
        ShopItem shopItem = (ShopItem)list[0];
        TextMeshProUGUI priceText = (TextMeshProUGUI)list[1];
        TextMeshProUGUI nameText = (TextMeshProUGUI)list[2];
        int newCoins = userSessionManager.Instance.currentCoins - shopItem.price;
        userSessionManager.Instance.currentCoins= newCoins;
        print("new coin: " + newCoins);
        ApiDataHandler.Instance.SetCoinsToFirebase(newCoins);
        GlobalAnimator.Instance.ShowTextMessage(messageText, nameText.text + " Selected", 2);
        SuccessfullyBuy(shopItem, priceText, nameText);
    }
    public void SuccessfullyBuy(ShopItem shopItem, TextMeshProUGUI priceText, TextMeshProUGUI nameText)
    {
        //GlobalAnimator.Instance.FadeOutLoader();
        shopItem.buyed = true;
        StartCoroutine(SetClotheName(shopItem.itemName.ToLower()));
        //ApiDataHandler.Instance.SetCloths(shopItem.itemName.ToLower());

        //string jsonString = JsonUtility.ToJson(ApiDataHandler.Instance.getShopData());
        //ApiDataHandler.Instance.SetShopDataToFirebase(jsonString);

        ApiDataHandler.Instance.SaveUserPurchaseData(shopItem.id);

        userSessionManager.Instance.CheckAchievementStatus();
        priceText.text = "Bought";
    }
    void SearchItems(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            foreach (var item in items)
            {
                item.SetActive(true);
            }
            return;
        }

        searchTerm = searchTerm.ToLower();

        foreach (var item in items)
        {
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            bool matchFound = false;

            foreach (var text in texts)
            {
                string itemText = text.text.ToLower();
                if (itemText.Contains(searchTerm))
                {
                    matchFound = true;
                    break;
                }
            }
            item.SetActive(matchFound);
        }
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        GameObject.FindGameObjectWithTag("characterScreen").GetComponentInChildren<CharacterController>().UpdateCharacterView();
        StateManager.Instance.OpenFooter(null, null, false);
    }
    public ShopItem GetShopItemByName(ShopModel shopModel, string itemName)
    {
        // Use LINQ to find the item
        ShopItem item = shopModel.items.FirstOrDefault(i => i.itemName.ToLower() == itemName.ToLower());

        if (item == null)
        {
            Console.WriteLine($"Item with name '{itemName}' not found.");
        }

        return item;
    }
    public IEnumerator SetClotheName(string name)
    {
        GlobalAnimator.Instance.FadeInLoader();
        // Build the reference path for the 'friends' node
        string path = $"users/{FirebaseManager.Instance.user.UserId}/clothes/";

        // Start deleting the friend from Firebase
        //var deleteTask = FirebaseDatabase.DefaultInstance.RootReference.Child(path).RemoveValueAsync();
        var dataTask = FirebaseDatabase.DefaultInstance.RootReference.Child(path).SetValueAsync(name);

        // Wait until the task completes
        yield return new WaitUntil(() => dataTask.IsCompleted);

        // Check for errors
        if (dataTask.Exception != null)
        {
            Debug.LogError("Error while saving clothe: " + dataTask.Exception);
        }
        else
        {
            userSessionManager.Instance.clotheName = name;
        }
        GlobalAnimator.Instance.FadeOutLoader();
        Back();
    }
}