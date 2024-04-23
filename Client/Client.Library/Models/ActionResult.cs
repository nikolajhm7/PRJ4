using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Library.Models
{
    public record ActionResult(bool Success, string? Msg);
    public record ActionResult<T>(bool Success, string? Msg, T? Value);
}
