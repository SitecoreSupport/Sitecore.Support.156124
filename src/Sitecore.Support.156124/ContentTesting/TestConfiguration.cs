using System.Collections.Generic;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Model;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.ContentTesting.Models;
using Sitecore.Diagnostics;
using System.Linq;

namespace Sitecore.Support.ContentTesting
{
    public class TestConfiguration : Sitecore.ContentTesting.TestConfiguration
    {
        private readonly TestDefinitionItem testDefinition;
        private ITestSet testSet;
        private IEnumerable<TestExperience> testExperiences;

        public Sitecore.Data.Items.Item ContentItem
        {
            get;
            protected set;
        }

        public TestConfiguration(Sitecore.Data.Items.Item contentItem, Sitecore.Data.ID deviceId, TestDefinitionItem test) : base(contentItem, deviceId, test)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(contentItem, "contentItem");
            Sitecore.Diagnostics.Assert.ArgumentNotNull(test, "test");
            this.ContentItem = contentItem;
            this.DeviceId = deviceId;
            this.testDefinition = test;
        }
        public new IEnumerable<TestExperience> Experiences
        {
            get
            {
                if (this.testExperiences == null)
                {
                    this.testExperiences = this.LoadExperienceData();
                }
                return this.testExperiences ?? Enumerable.Empty<TestExperience>();
            }
        }


        public Sitecore.ContentTesting.Models.ITestSet TestSet
        {
            get
            {
                if (base.TestSet != null)
                {
                    return new Sitecore.Support.ContentTesting.Models.TestSet(base.TestSet.Id, base.TestSet.Name);
                }

                var baseTestSet = TestManager.GetTestSet((IEnumerable<TestDefinitionItem>)new TestDefinitionItem[1]
                {
          this.TestDefinitionItem
                }, this.ContentItem, this.DeviceId);
                return new Sitecore.Support.ContentTesting.Models.TestSet(baseTestSet.Id, baseTestSet.Name);
            }
        }

        public override IEnumerable<TestExperience> LoadExperienceData()
        {
            if (this.TestDefinitionItem == null || this.TestSet == null)
            {
                return null;
            }
            IEnumerable<TestExperience> experiences = this.TestSet.GetExperiences();
            List<TestExperience> list = new List<TestExperience>();
            if (this.TestSet.Variables.Count == 0)
            {
                return list;
            }
            foreach (TestExperience current in experiences)
            {
                current.IsOriginal = ContentTestingFactory.Instance.ExperienceInspector.IsOriginalExperience((Sitecore.ContentTesting.Models.TestSet)this.TestSet, current.Combination);
                if (current.IsOriginal)
                {

                    this.OriginalExperience = current;
                }
                list.Add(current);
            }
            string text = this.TestType;
            if (string.IsNullOrEmpty(text))
            {
                text = "[unknown]";
            }
            this.RenameExperiences(list, this.ContentItem, text);
            return list;
        }
    }
}