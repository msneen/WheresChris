using StayTogether.Classes;

namespace WheresChris.ViewModels
{
    public class ChatMessageSimpleVm
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public GroupMemberVm Member { get; set; }
    }
}
