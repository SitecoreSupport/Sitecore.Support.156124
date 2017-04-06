using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.Data;

namespace Sitecore.Support.ContentTesting.Data
{
    public class SitecoreContentTestScore : Sitecore.ContentTesting.Data.SitecoreContentTestStore
    {
        public override ITestConfiguration LoadTest(TestDefinitionItem testDefinitionItem, ID deviceId)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(testDefinitionItem, "testDefinitionItem");
            Sitecore.Data.ID iD = deviceId ?? testDefinitionItem.Device.TargetID;
            if (iD == (Sitecore.Data.ID)null)
            {
                return null;
            }
            Sitecore.Data.Items.Item contentItemTargetItem = testDefinitionItem.GetContentItemTargetItem();
            if (contentItemTargetItem == null)
            {
                return null;
            }
            return new TestConfiguration(contentItemTargetItem, iD, testDefinitionItem);
        }
    }
}