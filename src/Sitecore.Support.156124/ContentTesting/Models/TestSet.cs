using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentTesting.Models;
using Sitecore.StringExtensions;

namespace Sitecore.Support.ContentTesting.Models
{
    [Serializable]
    public class TestSet : Sitecore.ContentTesting.Models.TestSet
    {
        public TestSet(Guid id, string name) : base(id, name)
        {
        }

        public new IEnumerable<Sitecore.ContentTesting.Model.TestExperience> GetExperiences()
        {
            if (base.Variables.Any<TestVariable>())
            {
                byte[] values = new byte[base.Variables.Count];
                int limit = base.Variables.Select<TestVariable, int>((Func<TestVariable, int>)(variable => variable.Values.Count)).Aggregate<int>((Func<int, int, int>)((x, y) => x * y));
                for (int count = 0; count < limit + 1; ++count)
                {
                    List<Guid> ids = new List<Guid>();
                    for (int index = 0; index < values.Length; ++index)
                    {
                        if ((int)values[index] >= base.Variables[index].Values.Count)
                        {
                            if (index + 1 > values.Length - 1)
                            {
                                yield break;
                            }
                            else
                            {
                                values[index] = (byte)0;
                                ++values[index + 1];
                            }
                        }
                        ids.Add(base.Variables[index].Values[(int)values[index]].Id);
                    }
                    yield return new Sitecore.ContentTesting.Model.TestExperience("Experience {0}".FormatWith((object)(count + 1)), (byte[])values.Clone(), (IEnumerable<Guid>)ids);
                    ++values[0];
                }
            }
        }
    }
}