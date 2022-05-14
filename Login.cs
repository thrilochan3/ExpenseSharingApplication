using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;

namespace ExpenseSharingApplication
{
    public partial class Login : Form
    {
        public readonly IMongoDatabase _db;
        public Login()
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
        }

        public IMongoCollection<User> userDetails =>
            _db.GetCollection<User>("Users");


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void txtPwd_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddUser addUser = new AddUser();
            this.Hide();
            addUser.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var a = userDetails.Find(a => true).ToList();
            var validation =  from i in a
                              where i.Username == txtUserName.Text
                              && i.Password == txtPwd.Text
                               select i;
            if (validation.Any())
            {
               this.Hide();
                Program.loggedInUser = validation.First();
                Home home = new Home();
                home.Show();
                
            }
            else {
                MessageBox.Show("Invalid username or password!!");
                return;
            }
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
