using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

//Notes for Me:
/*
 * The search field must be able to guess if the user is typing a tool crib part number or a description.
 * Use a messagebox to notify the criteria used by the software.
 * Use an event to delete the initial string in the search field 
 * Focus the cursos on search field at program load.
 * 
 */


namespace ToolCrib_SearchApp
{
    public partial class Form1 : Form
    {
        //CBM Connection String 10.110.3.23
        string sqlCBMConnectionString = "Dsn=CBM;uid=CBM";
        string sqlSAPConnectionString = "Password=immutaBle.2022;Persist Security Info=True;User ID=usrCBM;Initial Catalog=CBM;Data Source=nama-jua-s0020.nama-juarez.nama.org";

        //declare global arrays to store de inforation fetched from SAP Database
        List<string> listResultToolCribPN = new List<string>();
        List<string> listResultLocation = new List<string>();
        List<string> listResultCurrency = new List<string>();
        List<decimal> listResultPrice = new List<decimal>();
        List<decimal> listResultStockOnHand = new List<decimal>();


        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            toolCribQuery();
        }

        private void textToSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolCribQuery();
            }
        }

        //This is used to change the values of the tool crib part number once a different description is selected
        private void comboBoxDescription_SelectedIndexChanged(object sender, EventArgs e)
        {
            textPartNumber.Text = listResultToolCribPN[comboBoxDescription.SelectedIndex].ToString();
            textPrice.Text = listResultPrice[comboBoxDescription.SelectedIndex].ToString();
            textStock.Text = listResultStockOnHand[comboBoxDescription.SelectedIndex].ToString();
            textLocation.Text = listResultLocation[comboBoxDescription.SelectedIndex].ToString();
            textCurrency.Text = listResultCurrency[comboBoxDescription.SelectedIndex].ToString();         
            
        }

        //Focus the cursor on the text to search field at program load
        private void Form1_Load(object sender, EventArgs e)
        {
            textToSearch.Select();
            pictureBoxLogo.Show();
        }

        //if the user hovers the mouse on the text to search, it displays a hint.
        private void textToSearch_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textToSearch, "Hey!, Let me know if you are searching a tool crib PN by adding TC before the number, or simply type the description...");
        }

       /* private void textToSearch_TextChanged(object sender, EventArgs e)
        {

        }
       */
       

        //subroutine to make it able to be called from other places 
        public void toolCribQuery()
        {
            //Clear Tool Crib Part Number during the search
            textPartNumber.Text = "";
            comboBoxDescription.Text = "";
            textLocation.Text = "";
            textPrice.Text = "";
            textCurrency.Text = "";
            textStock.Text = "";

            listResultToolCribPN.Clear();
            listResultLocation.Clear();
            listResultCurrency.Clear();
            listResultPrice.Clear();
            listResultStockOnHand.Clear();


            //init list to gather information from databse to ease the items population
            List<string> listToolCribPN = new List<string>();
            List<string> listDescription = new List<string>();
            List<string> listCurrency = new List<string>();
            List<string> listLocation = new List<string>();
            List<decimal> listPrice = new List<decimal>();
            List<decimal> listStockOnHand = new List<decimal>();

            //TCPN: Tool crib part number
            bool searchByTCPN = false;
            string searchItem = textToSearch.Text;
            //Clear the items at new search to avoid overlap
            comboBoxDescription.Items.Clear();

            //connect to the database  *** Very important!! use the connection string from the UDL, but remove the "Provider" parameter and it works
            SqlConnection conn = new SqlConnection(sqlSAPConnectionString);

            //Init the datareader with the name reader
            SqlDataReader reader;
            //init a command named "cmd" which is a sqlcommand and in the argument pass the name of the stored procedure and the connection
            SqlCommand cmd = new SqlCommand("SP_ToolCrib_Costs", conn);
            //Tell to the program that the command will call a stored procedure.
            cmd.CommandType = CommandType.StoredProcedure;
            //open the connection
            conn.Open();
            //execute the command and store all the dataset in reader
            reader = cmd.ExecuteReader();
            // in this case the result of the stored procedure generated by Nidec IT is 
            //Columns
            //0=ToolCrin Part Number
            //2=Description
            //4=Price
            //8=Stock on Hand


            ///while the reader is reading, populate all the items of the arrays
            while (reader.Read())
            {
                //items from colum cero go to tool crib part number
                listToolCribPN.Add(reader.GetString(0).Remove(0, 12));
                //items from column one go to combobox items
                listDescription.Add(reader.GetString(2));
                //items from column four go to price array
                listPrice.Add(reader.GetDecimal(4));
                //items from columns five go to currency array
                listCurrency.Add(reader.GetString(5));
                //items from column eight go to stock on hand array
                listStockOnHand.Add(reader.GetDecimal(9));
                //items from columns eleven go to location array
                listLocation.Add(reader.GetString(11));

            }

            //Close the reader
            reader.Close();
            //Close the connection
            conn.Close();


            /*Here the program will check if the user typed a tool crib part number by checking if the first two digits are TC
             * and by measuring the lenght of the string being equals to 8.
             * if not, the program will assum it is a description and just will pass the string as typed.
             * 
             */

            //This is used to decide if the filter must be made by tool crib part number or by description
            if ((textToSearch.Text.Length == 8) && ((textToSearch.Text.Substring(0, 2) == "TC") || (textToSearch.Text.Substring(0, 2) == "tc")))
            {
                searchByTCPN = true;
            }
            else
            {
                searchByTCPN = false;
            }



            if (searchByTCPN)
            {


                //In this case the user is trying to search by the tool crib part number
                int ToolCribPNIndex = -1;
                //The value of search item is the 6 digits of the tool crib part number.
                searchItem = textToSearch.Text.Substring(2, 6);
                //This is used to find the index of the tool crib part number given by the user and use it to return only that value for description, cost and stock.
                ToolCribPNIndex = listToolCribPN.FindIndex(a => a.Contains(searchItem));
                //if the index if different than -1 in means that it exists
                if (ToolCribPNIndex != -1)
                {
                    //asign the value of the index found to each element of the search result
                    textPartNumber.Text = listToolCribPN.ElementAt(ToolCribPNIndex);
                    comboBoxDescription.Text = listDescription.ElementAt(ToolCribPNIndex).ToString();
                    textPrice.Text = ((listPrice.ElementAt(ToolCribPNIndex)) / 1000).ToString();
                    textStock.Text = listStockOnHand[ToolCribPNIndex].ToString();
                    textLocation.Text = listLocation[ToolCribPNIndex].ToString();
                    textCurrency.Text = listCurrency[ToolCribPNIndex].ToString();
                    MessageBox.Show("I found the item!");
                }
                // if the index is equals to -1, it just doesn't exists.
                else
                {
                    MessageBox.Show("No item was found :(");
                }
            }
            else
            {
                //In this case the user is trying to search by the description of the part.
                int iDIndex = -1;
                int indexFor = 0;



                //sweeps through the descriptions to find which contain the description typed by the user and add those matching to the comoboBoxDescription items 
                foreach (string part in listDescription)
                {
                    //Return the value of the current index and store in a single element variable to add it to the resultant list
                    string thisToolPN = listToolCribPN.ElementAt(indexFor).ToString();
                    decimal thisPrice = (listPrice.ElementAt(indexFor) / 1000);
                    decimal thisStock = listStockOnHand.ElementAt(indexFor);
                    string thisLocation = listLocation[indexFor].ToString();
                    string thisCurrency = listCurrency[indexFor].ToString();

                    //Check if the searchItem matches with the current description and if it does, add to the elements of the combobox together with the list of each value.
                    if (System.Text.RegularExpressions.Regex.IsMatch(part, searchItem, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        comboBoxDescription.Items.Add(part);
                        listResultPrice.Add(thisPrice);
                        listResultStockOnHand.Add(thisStock);
                        listResultToolCribPN.Add(thisToolPN);
                        listResultCurrency.Add(thisCurrency);
                        listResultLocation.Add(thisLocation);

                        //Set the first element found after the search to show the user it found something
                        if (iDIndex == -1)
                        {
                            textPartNumber.Text = "mamadas";
                            textLocation.Text = thisLocation;
                            textPrice.Text = thisPrice.ToString();
                            textCurrency.Text = thisCurrency;
                            textStock.Text = thisStock.ToString();
                            comboBoxDescription.Text = part;

                            //EACR I need to create some new array to match the size of the found items in the description
                        }
                        iDIndex++;
                    }
                    indexFor++;
                }


                //
                MessageBox.Show("Some elements were found! CUalquier mamada");
            }
        }
    }
}