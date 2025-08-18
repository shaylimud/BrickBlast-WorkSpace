using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIElementMediator : MonoBehaviour
{
    [System.Serializable]
    public class CanvasElements
    {
        public GameObject Loading;
        public GameObject Menu;
        public GameObject Insufficient;
        public GameObject Tutorial;
        public GameObject Shop;
        public GameObject Level;
        public GameObject Revive;
        public GameObject End;
        public GameObject SettingsAndroid;
        public GameObject SettingiOS;
        public GameObject UpdateApplication;
        public GameObject DataMismatch;

        public GameObject NoEnemies;
        public GameObject ExtraSpace;
    }
    [System.Serializable]
    public class MenuElements
    {
        public TextMeshProUGUI MenuCurrency;
        public TextMeshProUGUI Level;
        public TextMeshProUGUI ReachCost;
        public TextMeshProUGUI SpaceLevel;
        public TextMeshProUGUI SpaceCost;

        public GameObject BtnShop;

        public Image ImageSoundsIos;
        public Image ImageSoundsAndroid;
        public Sprite SpriteSoundOn;
        public Sprite SpriteSoundOff;

        public GameObject WebIndexOn;
        public GameObject WebIndexOff;

        public TextMeshProUGUI FreeGiftCooldown;
        public GameObject BtnWatchFreeGift;

        public GameObject IconCheater;
    }


    [System.Serializable]
    public class ShopElements
    {
        public TextMeshProUGUI ShopCurrency;

        public GameObject panelProducts;
        public GameObject PrefabSubscriptionNoAds;
        public GameObject PrefabConsumable;

        public GameObject CtnrSubscriptionNoAds;

        //iOS Spesific
    }

    [System.Serializable]
    public class ReviveElements
    {
        public Image MeterRevive;
        public TextMeshProUGUI ReviveSpace;
    }

    [System.Serializable]
    public class LevelElements
    {
        public TextMeshProUGUI LevelCurrency;
        public TextMeshProUGUI LevelReach;
        public TextMeshPro LevelSpace;
    }

    [System.Serializable]
    public class EndElements
    {
        public GameObject CtnrFullSpace;
        public GameObject CtnrPartialSpace;

        public TextMeshProUGUI EndCurrency;

        public GameObject InfoOfferTriple;
        public GameObject BtnTriple;
        public Image MeterTriple;
    }

    [System.Serializable]
    public class FeatureElements
    {
        public Image NoEnemiesMeter;

        public TextMeshProUGUI ExtraSpaceReward;
        public Image ExtraSpaceMeter;
    }

    [Header("Debugger")]
    [SerializeField] private RayDebugService _rayDebug;

    [Header("Canvases")]
    public CanvasElements Canvas = new CanvasElements();

    [Header("Menu")]
    public MenuElements Menu = new MenuElements();

    [Header("Shop")]
    public ShopElements Shop = new ShopElements();

    [Header("Revive")]
    public ReviveElements Revive = new ReviveElements();

    [Header("Level")]
    public LevelElements Level = new LevelElements();

    [Header("End")]
    public EndElements End = new EndElements();

    [Header("Features")]
    public FeatureElements Feature = new FeatureElements();
}
