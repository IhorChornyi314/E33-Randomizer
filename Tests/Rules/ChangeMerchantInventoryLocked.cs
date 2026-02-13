namespace Tests.Rules;

public class ChangeMerchantInventoryLocked: OutputRuleBase
{
    private static float _threshold = 0.1f;
    public override bool IsSatisfied(Output output, Config config)
    {
        var lockedFrequency = 0.0f;
        foreach (var outputCheck in output.Checks)
        {
            if (!outputCheck.Name.Contains("DT_Merchant_")) continue;
            var originalCheck = TestLogic.OriginalData.GetCheck(outputCheck.Name);
            for (int i = 0; i < Math.Min(outputCheck.Size, originalCheck.Size); i++)
            {
                if (!config.Settings.ChangeMerchantInventoryLocked && originalCheck.Items[i].MerchantInventoryLocked !=
                        outputCheck.Items[i].MerchantInventoryLocked)
                {
                    FailureMessage += $"{outputCheck.Name}'s locked status invalid";
                    return false;
                }
            }
            
            var lockedCount = outputCheck.Items.Count(i => i.MerchantInventoryLocked);
            lockedFrequency += outputCheck.Size != 0 ? (float)lockedCount / outputCheck.Size : 0;
        }

        if (!config.Settings.ChangeMerchantInventoryLocked) return true;
        
        lockedFrequency /= output.Checks.Count;
        var lockedPercent = (int)(lockedFrequency * 100);
        if (Math.Abs(lockedFrequency - config.Settings.MerchantInventoryLockedChancePercent / 100.0) > _threshold)
        {
            FailureMessage += $"Locked percent was {lockedPercent}, should be {config.Settings.MerchantInventoryLockedChancePercent}";
        }
        return true;
    }
}