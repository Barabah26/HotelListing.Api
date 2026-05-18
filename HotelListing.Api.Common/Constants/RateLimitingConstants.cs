using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelListing.Api.Common.Constants;

public static class RateLimitingConstants
{
    public const string PerUserPolicy = "perUser";
    public const string FixedPolicy = "fixed";
}