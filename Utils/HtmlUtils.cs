using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace S5LkForm.Utils
{
    public static class HtmlUtils
    {
        public static List<SelectListItem> GetSelectItemList(DataTable table, string textKey, string valueKey, string selectedValue = null)
        {
            List<SelectListItem> list = (
                from o in table.AsEnumerable()
                orderby o.Field<string>(textKey) ascending
                select new SelectListItem
                {
                    Text = o.Field<string>(textKey),
                    Value = o.Field<string>(valueKey),
                    Selected = (o.Field<string>(valueKey) == selectedValue)
                }
            ).ToList();
            return list;
        }
    }
}
