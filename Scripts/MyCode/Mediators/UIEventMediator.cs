using Ray.Services;
using UnityEngine;

public class UIEventMediator : MonoBehaviour
{
    // Application Open
    public void _OnUpdateApplicationBtn() => EventService.UI.OnUpdateApplicationBtn.Invoke(this);

    // Menu
    public void _OnStartBtn() => EventService.UI.OnStartBtn.Invoke(this);
    public void _OnReachUpgradeBtn() => EventService.UI.OnReachUpgradeBtn.Invoke(this);
    public void _OnSpaceUpgradeBtn() => EventService.UI.OnSpaceUpgradeBtn.Invoke(this);

    public void _OnToggleSound() => EventService.UI.OnToggleSound.Invoke(this);
    public void _OnToggleTutorial() => EventService.UI.OnToggleTutorial.Invoke(this);
    public void _OnToggleInsufficientBtn() => EventService.UI.OnToggleInsufficient.Invoke(this);
    public void _OnToggleDataMismatchBtn() => EventService.UI.OnToggleDataMismatch.Invoke(this);
    public void _OnToggleShop() => EventService.UI.OnToggleShop.Invoke(this);

    // Rewarded
    public void _OnPenaltyBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.Penalty);
    public void _OnNoEnemiesBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.NoEnemies);
    public void _OnReviveBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.Revive);
    public void _OnTripleBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.Triple);
    public void _OnFreeGiftBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.FreeGift);
    public void _OnExtraSpaceBtn() => EventService.UI.OnRewardedBtn.Invoke(this, RewardedType.ExtraSpace);


    //iOS
    public void _OnRestoreBtn() => EventService.IAP.OnRestoreBtn.Invoke(this);
    public void OnTermsOfUseBtn() => EventService.UI.OnTermsOfUse.Invoke(this);
    public void OnPrivacyPolicyBtn() => EventService.UI.OnPrivacyPolicy.Invoke(this);

    //Settings
    public void _OnToggleSettings() => EventService.UI.OnSettingBtn.Invoke(this);
    public void _OnShowConsent() => EventService.UI.OnShowConsent.Invoke(this);

    public void _OnLearnMore() => EventService.UI.OnLearnMore.Invoke(this);
}