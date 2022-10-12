using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;

namespace S5LkForm.Utils
{
    public static class Extensions
    {
        public static Dictionary<string, ICollection> CopyPropertyCollectionToDictionary(this PropertyCollection collection)
        {
            Dictionary<string, ICollection> result = new();
            foreach (string name in collection.PropertyNames)
            {
                ICollection valueList = collection[name] as ICollection;
                if (valueList != null) valueList = new ArrayList(valueList); 
                result.Add(name, valueList);
            }
            return result;
        }
    }
}
