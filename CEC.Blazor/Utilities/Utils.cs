using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Data;
using CEC.Blazor.Extensions;

namespace CEC.Blazor.Utilities
{
    public class Utils
    {

        /// <summary>
        /// Method to get a dummy task
        /// </summary>
        /// <returns></returns>
        public static Task<DbTaskResult> GetDummyTask()
        {
            return Task.FromResult(new DbTaskResult());
        }

        /// <summary>
        /// Method to get a sorted deictionary list of an enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static SortedDictionary<int, string> GetEnumList<TEnum>()
        {
            var list = new SortedDictionary<int, string>();
            if (typeof(TEnum).IsEnum)
            {
                var values = Enum.GetValues(typeof(TEnum));
                foreach (int v in values) list.Add(v, (Enum.GetName(typeof(TEnum), v)).AsSeparatedString());
            }
            return list;
        }
    }
}
