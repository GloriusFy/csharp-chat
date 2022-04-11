using System.Collections.ObjectModel;

namespace DataContract.Model
{
    public class DataModel
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<UserModel> Users { get; set; }
        public DataModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Users = new ObservableCollection<UserModel>();
        }
    }
}
