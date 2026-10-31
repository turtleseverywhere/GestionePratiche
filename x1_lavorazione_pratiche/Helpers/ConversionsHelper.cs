using System;
using System.Collections.Generic;

namespace x1_lavorazione_pratiche.Helpers
{
    public static class ConversionsHelper
    {
        public static List<ResultType> ConvertList<SourceType, ResultType>(List<SourceType> queryResult, Func<SourceType, ResultType> conversionFunction)
        {
            var toSender = new List<ResultType>();
            foreach (var item in queryResult)
                toSender.Add(conversionFunction(item));
            return toSender;
        }
    }
}
