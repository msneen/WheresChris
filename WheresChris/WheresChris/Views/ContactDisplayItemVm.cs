namespace WheresChris.Views
{
    public class ContactDisplayItemVm
    {
        public string Text { get; set; }
        public string Detail { get; set; }
        public bool Selected { get; set; }

        public override string ToString() => Text;
    }
}