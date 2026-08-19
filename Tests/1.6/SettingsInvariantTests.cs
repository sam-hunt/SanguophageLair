using Xunit;

namespace SanguophageLair.Tests;

// Pure-constant checks (consts are compile-time inlined, so these run without
// loading any Verse type). Real suites arrive with the first pure-logic helpers.
public class SettingsInvariantTests
{
    [Fact]
    public void LairQuestWeightDefault_IsWithinSliderBounds()
    {
        Assert.InRange(
            SanguophageLairSettings.LairQuestWeightDefault,
            SanguophageLairSettings.LairQuestWeightMin,
            SanguophageLairSettings.LairQuestWeightMax);
    }
}
