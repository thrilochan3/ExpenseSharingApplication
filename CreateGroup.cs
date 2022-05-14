using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExpenseSharingApplication
{
    public partial class CreateGroup : Form
    {
        public readonly IMongoDatabase _db;
        public CreateGroup()
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
            var allUsers = userDetails.Find(a => true).ToList();
            var username = from i in allUsers
                           where i.Username != Program.loggedInUser.Username
                           select i.Username;
            checkListBox.Items.Clear();
            foreach (var item in username) {
                checkListBox.Items.Add(item.ToString());
            }
            
        }

        public IMongoCollection<User> userDetails =>
            _db.GetCollection<User>("Users");
        public IMongoCollection<Group> groups =>
            _db.GetCollection<Group>("UserGroups");
        private void Members_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void textGroupName_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textGroupName.Text))
            {
                MessageBox.Show("Group name is mandatory field.");
                return;
            }
            if (checkListBox.CheckedItems.Count < 1)
            {
                MessageBox.Show("Select atleast one group member");
                return;
            }
            String[] selectedUsers = new String[checkListBox.CheckedItems.Count+1];
            selectedUsers[0] = Program.loggedInUser.Username;
            int count = 1;
            foreach (object item in checkListBox.CheckedItems) {
                selectedUsers[count++] = item.ToString();
            }
           
            Group group = new Group();
            group.Name = textGroupName.Text;
            group.DateCreated = DateTime.Now;
            group.Members = selectedUsers;
            group.CreatedBy = Program.loggedInUser.Username;

            groups.InsertOne(group);

            MessageBox.Show("Group created!");

            this.Hide();
            Home home = new Home();
            home.Show();

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Home home = new Home(); 
            home.Show();
        }
    }
}
