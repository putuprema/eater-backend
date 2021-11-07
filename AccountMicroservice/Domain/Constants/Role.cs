using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        None,
        CUSTOMER,
        EMPLOYEE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EmployeeRole
    {
        Admin,
        Cashier,
        Kitchen,
        Waiter
    }
}
