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
    public partial class ConfrimExpense : Form
    {
        public readonly IMongoDatabase _db;
        public Expense Expense { get; set; }
        public ConfrimExpense()
        {
            InitializeComponent();
        }
        public ConfrimExpense(Expense expense)
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
            this.Expense = expense;
            txtExpenseDescription.Text = expense.Name;
            txtPaidby.Text = expense.Payer;
            txtAmount.Text = Decimal.Round(expense.Amount,2).ToString();
        }

        public IMongoCollection<ExpenseMapping> expenseMapping =>
            _db.GetCollection<ExpenseMapping>("ExpenseMapping");
        public IMongoCollection<Expense> expenses =>
            _db.GetCollection<Expense>("Expenses");

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfrimExpense_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cbConfirm.Text)){
                MessageBox.Show("Select confirm or reject!");
                return;
            }
            var allExpenseMappings = expenseMapping.Find(a => true).ToList();
            var expenseMappingUpdateList = from item in allExpenseMappings
                                           where item.Payee == Program.loggedInUser.Username
                                           && item.ExpenseId == Expense._id
                                           select item;
            var expenseMappingUpdate = expenseMappingUpdateList.FirstOrDefault();
            if (cbConfirm.Text == "Confirm")
            {
                for (int i = 0; i < Expense.Confirmed.Length; i++)
                {
                    if (String.IsNullOrEmpty(Expense.Confirmed[i]))
                    {
                        Expense.Confirmed[i] = Program.loggedInUser.Username;
                        break;
                    }
                }
                var filter = Builders<Expense>.Filter.Eq(c => c._id, Expense._id);
                var update = Builders<Expense>.Update
                             .Set("GroupId", Expense.GroupId)
                             .Set("Name", Expense.Name)
                             .Set("DateCreated", Expense.DateCreated)
                             .Set("CreatedBy", Expense.CreatedBy)
                             .Set("Payer", Expense.Payer)
                             .Set("Amount", Expense.Amount)
                             .Set("Involved", Expense.Involved)
                             .Set("Confirmed", Expense.Confirmed);
                expenses.UpdateOne(filter, update);

                

                var filter1 = Builders<ExpenseMapping>.Filter.Eq(c => c._id, expenseMappingUpdate._id);
                var update1 = Builders<ExpenseMapping>.Update
                             .Set("ExpenseId", expenseMappingUpdate.ExpenseId)
                             .Set("GroupId", expenseMappingUpdate.GroupId)
                             .Set("Payer", expenseMappingUpdate.Payer)
                             .Set("Payee", expenseMappingUpdate.Payee)
                             .Set("Amount", expenseMappingUpdate.Amount)
                             .Set("IsConfirmed", "Confirmed");
                expenseMapping.UpdateOne(filter1, update1);
                MessageBox.Show("Confirmed..");
                this.Hide();
                Home home = new Home();
                home.Show();
                return;
            }
            else {

                for (int i = 0; i < Expense.Involved.Length; i++)
                {
                    if (Expense.Involved[i]== Program.loggedInUser.Username)
                    {
                        Expense.Involved[i] = null;
                        break;
                    }
                }
                int availableMembers = 0;
                
                for (int i = 0; i < Expense.Involved.Length; i++)
                {
                    if (String.IsNullOrEmpty(Expense.Involved[i]))
                    {
                        continue;
                    }
                    availableMembers++;
                }
                if (availableMembers == 1)
                {
                    //delete expense and all mappings
                    deleteExpenseMapping(expenseMappingUpdate._id);
                    deleteExpense(Expense._id);
                }
                else {
                    //update expense and mappings
                    //delete one expense mapping
                    deleteExpenseMapping(expenseMappingUpdate._id);

                    var filter = Builders<Expense>.Filter.Eq(c => c._id, Expense._id);
                    var update = Builders<Expense>.Update
                                 .Set("GroupId", Expense.GroupId)
                                 .Set("Name", Expense.Name)
                                 .Set("DateCreated", Expense.DateCreated)
                                 .Set("CreatedBy", Expense.CreatedBy)
                                 .Set("Payer", Expense.Payer)
                                 .Set("Amount", Expense.Amount)
                                 .Set("Involved", Expense.Involved)
                                 .Set("Confirmed", Expense.Confirmed);
                    expenses.UpdateOne(filter, update);

                    var remainingExpenseMappings = from item in allExpenseMappings
                                                   where item.ExpenseId == Expense._id
                                                   select item;
                    foreach (var remainingExpenseMapping in remainingExpenseMappings)
                    {
                        var filter1 = Builders<ExpenseMapping>.Filter.Eq(c => c._id, remainingExpenseMapping._id);
                        var update1 = Builders<ExpenseMapping>.Update
                                     .Set("ExpenseId", remainingExpenseMapping.ExpenseId)
                                     .Set("GroupId", remainingExpenseMapping.GroupId)
                                     .Set("Payer", remainingExpenseMapping.Payer)
                                     .Set("Payee", remainingExpenseMapping.Payee)
                                     .Set("Amount", Expense.Amount/availableMembers)
                                     .Set("IsConfirmed", remainingExpenseMapping.IsConfirmed);
                        expenseMapping.UpdateOne(filter1, update1);
                    }
                }
                MessageBox.Show("Rejected!!");
                this.Hide();
                Home home = new Home();
                home.Show();


            }

        }

        public void deleteExpenseMapping(String id)
        {
            var filter = Builders<ExpenseMapping>.Filter.Eq(c => c._id, id);
            expenseMapping.DeleteOne(filter);
        }
        public void deleteExpense(String id)
        {
            var filter = Builders<Expense>.Filter.Eq(c => c._id, id);
            expenses.DeleteOne(filter);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Home home = new Home();
            home.Show();
        }
    }
}
