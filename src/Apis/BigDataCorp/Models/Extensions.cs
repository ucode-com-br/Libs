using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models
{
    public static class Extensions
    {

        public static bool TryDatasetInfo<T>(this T instance, out DatasetInfo? datasetInfo)
        {
            if (instance.TryConvert(out DatasetMap? map) && map != null)
            {
                datasetInfo = (DatasetInfo)map;
                return true;
            }
            else
            {
                var getDatasetInfoAttribute = typeof(T).GetCustomAttribute<DatasetInfoAttribute>(true);
                if (getDatasetInfoAttribute != null)
                {
                    datasetInfo = (DatasetInfo)getDatasetInfoAttribute;
                    return true;
                }
                else
                {
                    datasetInfo = null;
                    return false;
                }
            }
        }

        public static bool TryConvert<TSource, TResult>(this TSource? obj, out TResult? @value)
        {
            var sourceType = typeof(TSource);
            var resultType = typeof(TResult);

            try
            {
                if (obj == null)
                {
                    @value = (TResult?)(object?)obj;
                    return true;
                }
                else if (sourceType.IsSubclassOf(resultType) || resultType.IsAssignableFrom(sourceType) || obj is TResult)
                {
                    @value = ((TResult)(object)obj)!;
                    return true;
                }
                else if (sourceType == resultType)
                {
                    @value = ((TResult)(object)obj)!;
                    return true;
                }
                else
                {
                    @value = default;
                    return false;
                }
            }
            catch
            {
                @value = default;
                return false;
            }
        }
    }
}
