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
    public partial class AddUser : Form
    {

        public readonly IMongoDatabase _db;

        public AddUser()
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
        }

        public IMongoCollection<User> userDetails =>
            _db.GetCollection<User>("Users");

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //form level validations needs to be added add
            if (String.IsNullOrEmpty(txtEmail.Text) || String.IsNullOrEmpty(txtFullName.Text)
                || String.IsNullOrEmpty(txtMobile.Text)|| String.IsNullOrEmpty(txtPassword.Text)
                || String.IsNullOrEmpty(txtUserName.Text)) 
            
            {
                MessageBox.Show("All fields are mandatory here..");
                return;
            }
            var a= userDetails.Find(a => true).ToList();
            var username = from i in a
                           where i.Username == txtUserName.Text
                           select i;
            if (username.Any())
            {
                txtUserName.Text = null;
                MessageBox.Show("Username already taken...");
                return;
            }
            var mobilenumber = from i in a
                               where i.PhoneNumber == txtMobile.Text
                            select i;
            if (mobilenumber.Any())
            {
                txtUserName.Text = null;
                MessageBox.Show("with this mobile number there is already a user named: "+ mobilenumber.First().Username);
                return;
            }
            User newUser = new User();
            newUser.Username = txtUserName.Text;
            newUser.Password = txtPassword.Text;
            newUser.PhoneNumber = txtMobile.Text;
            newUser.EmailId = txtEmail.Text;
            newUser.FullName = txtFullName.Text;
            userDetails.InsertOne(newUser);
            MessageBox.Show("Account created successfully!!");
            this.Hide();
            Login login = new Login();
            login.Show();

        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.Show();
        }
    }
}
