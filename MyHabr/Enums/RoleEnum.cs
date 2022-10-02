using System.ComponentModel;

namespace MyHabr.Enums
{
    public enum RoleEnum
    {
        [Description("Administrator")]
        Admin = 1,
        [Description("Check new articles and can approve it or decline")]
        Moderator = 2,
        [Description("Can write new articles")]
        Writer = 3,
        [Description("Can read articles and comment it")]
        Reader = 4
    }
}
