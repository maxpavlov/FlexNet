using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Diagnostics
{
    public class AuditEvent
    {
        public string AuditCategory { get; private set; }
        public int EventId { get; private set; }

        public static readonly AuditEvent LoginTry = new AuditEvent("LoginTry", 11000);
        public static readonly AuditEvent LoginSuccessful = new AuditEvent("LoginSuccessful", 11001);
        public static readonly AuditEvent LoginUnsuccessful = new AuditEvent("LoginUnsuccessful", 11002);
        public static readonly AuditEvent Logout = new AuditEvent("Logout", 11003);
        public static readonly AuditEvent ContentCreated = new AuditEvent("ContentCreated", 11004);
        public static readonly AuditEvent ContentUpdated = new AuditEvent("ContentUpdated", 11005);
        public static readonly AuditEvent ContentDeleted = new AuditEvent("ContentDeleted", 11006);
        public static readonly AuditEvent VersionChanged = new AuditEvent("VersionChanged", 11007);
        public static readonly AuditEvent PermissionChanged = new AuditEvent("PermissionChanged", 11008);

        public AuditEvent(string auditCategory, int eventId)
        {
            AuditCategory = auditCategory;
            EventId = eventId;
        }

        public override string ToString()
        {
            return AuditCategory;
        }
    }

}
