using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyVet.Web.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboPetTypes();
    }
}