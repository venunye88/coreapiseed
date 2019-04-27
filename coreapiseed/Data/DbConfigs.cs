using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeApiSeed.Data
{
    public class Privileges
    {
        //Permissions on the Role
        public const string RoleCreate = "Role.Create";
        public const string RoleUpdate = "Role.Update";
        public const string RoleRead = "Role.Read";
        public const string RoleDelete = "Role.Delete";

        //Permissions on the User Model
        public const string UserCreate = "User.Create";
        public const string UserUpdate = "User.Update";
        public const string UserRead = "User.Read";
        public const string UserDelete = "User.Delete";

        //Permissions on systems configuration
        public const string MessagePortal = "MessagePortal";
        public const string Dashboard = "Dashboard";
        public const string Report = "Report";
        public const string Setting = "Setting";
        public const string Administration = "Administration";
    }

    public class AmountFormats
    {
        public static string CurrencyFormat = "GHC##,###.00";
        public static string AmountFormat = "##,###.00";
    }

    public class ConfigKeys{
    public static string OrganizationName = "OrganizationName";
    public static string OrganizationAddress = "OrganizationAddress";
    public static string OrganizationEmail = "OrganizationEmail";
    public static string OrganizationTelephone = "OrganizationTelephone";
        public static string ApiKey = "ApiKey";

    }

}
