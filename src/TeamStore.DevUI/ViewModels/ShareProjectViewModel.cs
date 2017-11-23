using TeamStore.Keeper.Enums;

namespace TeamStore.ViewModels
{
    public class AccessChangeProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Details { get; set; }
        public bool? Result { get; set; }
        public Role Role { get; set; }
    }
}
