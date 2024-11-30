using System.ComponentModel;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    public enum DistrictLevel
    {
        [Description("省")]
        Province = 0,
        [Description("城市")]
        City = 1,
        [Description("区县")]
        County = 2,
        [Description("乡镇")]
        Town = 3
    }

    public enum HouseType
    {
        LowRise = 0,
        MidRise = 1,
        MidHigh = 2,
        HighRise = 3,
        All = 99
    }

    public enum OwnerSex
    {
        Male = 1,
        Female = 2
    }

    public enum AgeType
    {
        Young = 1,
        Adult = 2,
        Older = 3
    }

    public enum CarType
    {
        Large = 1,
        Mid = 2,
        Small = 3
    }

    public enum CarLogType
    {
        DriveIn = 1,
        DriveOut = 2
    }

    public enum GuardMode
    {
        Card,
        Password,
        Face,
        Remote
    }
}
