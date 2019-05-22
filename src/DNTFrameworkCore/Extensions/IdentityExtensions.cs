using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using DNTFrameworkCore.GuardToolkit;
using DNTFrameworkCore.Runtime;

namespace DNTFrameworkCore.Extensions
{
    public static class IdentityExtensions
    {
        public static T GetUserId<T>(this IIdentity identity) where T : IConvertible
        {
            var id = identity?.FindUserClaimValue(DNTClaimTypes.UserId);

            if (id != null) return (T) Convert.ChangeType(id, typeof(T), CultureInfo.InvariantCulture);

            return default;
        }

        public static long? GetUserId(this IIdentity identity)
        {
            var id = identity?.FindUserClaimValue(DNTClaimTypes.UserId);

            if (id.IsEmpty()) return null;

            if (!long.TryParse(id, out var userId)) return null;

            return userId;
        }

        public static long? GetBranchId(this IIdentity identity)
        {
            var branchClaim = identity?.FindUserClaimValue(DNTClaimTypes.BranchId);

            if (branchClaim.IsEmpty()) return default;

            return !long.TryParse(branchClaim, out var branchId) ? default : branchId;
        }
        public static long? GetTenantId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(DNTClaimTypes.TenantId);

            if (tenantClaim.IsEmpty()) return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static long? GetImpersonatorTenantId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(DNTClaimTypes.ImpersonatorTenantId);

            if (tenantClaim.IsEmpty()) return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static long? GetImpersonatorUserId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(DNTClaimTypes.ImpersonatorUserId);

            if (tenantClaim.IsEmpty()) return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static IReadOnlyList<string> GetPermissions(this IIdentity identity)
        {
            Guard.ArgumentNotNull(identity, nameof(identity));

            var claimsIdentity = identity as ClaimsIdentity;

            var permissionClaims = claimsIdentity?.FindAll(DNTClaimTypes.Permission);

            if (permissionClaims == null) return new List<string>();

            return permissionClaims.Select(a => a.Value).ToImmutableList();
        }

        public static IReadOnlyList<string> GetRoles(this IIdentity identity)
        {
            Guard.ArgumentNotNull(identity, nameof(identity));

            var claimsIdentity = identity as ClaimsIdentity;

            var roleClaims = claimsIdentity?.FindAll(DNTClaimTypes.Role);

            if (roleClaims == null) return new List<string>();

            return roleClaims.Select(a => a.Value).ToImmutableList();
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            return identity?.FindFirst(claimType)?.Value;
        }

        public static string FindUserClaimValue(this IIdentity identity, string claimType)
        {
            return (identity as ClaimsIdentity)?.FindFirstValue(claimType);
        }

        public static string GetUserFirstName(this IIdentity identity)
        {
            return identity?.FindUserClaimValue(DNTClaimTypes.GivenName);
        }

        public static string GetUserLastName(this IIdentity identity)
        {
            return identity?.FindUserClaimValue(DNTClaimTypes.Surname);
        }

        public static string GetUserFullName(this IIdentity identity)
        {
            return $"{GetUserFirstName(identity)} {GetUserLastName(identity)}";
        }

        public static string GetUserDisplayName(this IIdentity identity)
        {
            var fullName = GetUserFullName(identity);
            return string.IsNullOrWhiteSpace(fullName) ? GetUserName(identity) : fullName;
        }

        public static string GetUserName(this IIdentity identity)
        {
            return identity?.FindUserClaimValue(DNTClaimTypes.UserName);
        }
    }
}