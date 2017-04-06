using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Analytics.Reporting;
using Sitecore.ContentTesting.ComponentTesting;
using Sitecore.ContentTesting.Configuration;
using Sitecore.ContentTesting.Data;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Support.ContentTesting.Data;

namespace Sitecore.Support.ContentTesting.Pipelines.GetChromeData
{
    public class GetRenderingTestVariations: Sitecore.ContentTesting.Pipelines.GetChromeData.GetRenderingTestVariations
    {
        protected readonly SitecoreContentTestScore testScore;

        public GetRenderingTestVariations()
     : this((IContentTestingFactory)null, (SitecoreContentTestStore)null)
        {
        }

        public GetRenderingTestVariations(IContentTestingFactory factory, SitecoreContentTestStore testStore) : base(factory, testStore)
        {

            this.testScore = new SitecoreContentTestScore();
        }

        public new void Process(Sitecore.Pipelines.GetChromeData.GetChromeDataArgs args)
        {
            if (!Settings.IsAutomaticContentTestingEnabled)
            {
                return;
            }
            Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
            Sitecore.Diagnostics.Assert.IsNotNull(args.ChromeData, "Chrome Data");
            if (!"rendering".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Sitecore.Layouts.RenderingReference renderingReference = args.CustomData["renderingReference"] as Sitecore.Layouts.RenderingReference;
            if (renderingReference == (Sitecore.Layouts.RenderingReference)null)
            {
                return;
            }
            if (string.IsNullOrEmpty(renderingReference.Settings.MultiVariateTest))
            {
                return;
            }
            JArray jArray = this.ProcessTestingRendering(renderingReference);
            if (jArray != null && jArray.Count > 0)
            {
                args.ChromeData.Custom.Add("testVariations", jArray);
            }



        }

        protected override JArray ProcessTestingRendering(RenderingReference rendering)
        {
            TestVariationSelector testVariationSelector = new TestVariationSelector(rendering, this.testScore, this.factory);
            MultivariateTestValueItem value = testVariationSelector.GetTestValueItem(Sitecore.Context.Language);
            if (value == null)
            {
                return null;
            }
            TestDefinitionItem testDefinition = ((TestValueItem)value).TestDefinition;
            IEnumerable<JObject> enumerable = from x in value.Variable.Values
                                              select JObject.FromObject(new
                                              {
                                                  guid = x.ID.ToGuid(),
                                                  id = x.ID.ToShortID().ToString(),
                                                  name = x.Name,
                                                  isActive = (x.ID == value.ID)
                                              });
            if (testDefinition.IsRunning)
            {
                ITestConfiguration testConfiguration = this.testScore.LoadTest(testDefinition, Sitecore.Context.Device.ID);
                if (testConfiguration != null)
                {
                    TestValueEngagementQuery testValueEngagementQuery = new TestValueEngagementQuery(testConfiguration,
                        null);
                    testValueEngagementQuery.Execute();
                    foreach (JObject current in enumerable)
                    {
                        double valueEngagementScore =
                            testValueEngagementQuery.GetValueEngagementScore(current.Value<Guid>("guid"));
                        current.Add("value", valueEngagementScore);
                    }
                }
            }
            return new JArray(enumerable);
        }

    }
}