namespace MarkRestaurant.Models
{
    public class ErrorViewModel
    {
        public ErrorViewModel(string title, string description)
        {
            Title = title;
            Description = description;
            AspController = "Home";
            AspAction = "Index";
        }
        public ErrorViewModel(string title, string description, string aspController, string aspAction)
        {
            Title = title;
            Description = description;
            AspController = aspController;
            AspAction = aspAction;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string AspController { get; set; }
        public string AspAction { get; set; }

    }
}