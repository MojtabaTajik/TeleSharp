using System.Linq;
using RestSharp;

namespace TeleSharp
{
    public class SnakeCaseSerializationStrategy : PocoJsonSerializerStrategy
    {
        protected override string MapClrMemberNameToJsonFieldName(string memberName)
        {
            return
                string.Concat(
                    memberName.Select(
                        (x, i) => i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x).ToString() : x.ToString().ToLower()));
        }
    }
}