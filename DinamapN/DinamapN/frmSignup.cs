﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.Odbc;

namespace DinamapN
{
    public partial class frmSignup : Form
    {
        OdbcConnection MyConnection;

        // Constructor
        public frmSignup()
        {
            InitializeComponent();
        }

        // Upon loading form...
        private void frmSignup_Load(object sender, EventArgs e)
        {
            MyConnection = new OdbcConnection("DSN=dinamapMySQL2");

            //Gender and Ethnicity have drop-down menus
            txtGender.DropDownStyle = ComboBoxStyle.DropDownList;
            txtEth.DropDownStyle = ComboBoxStyle.DropDownList;
            // Begin input at first name       
            txtFName.Select();
        }

        //Save Patient registration to Database
        private void saveDB(Hashtable h)
        {
            OdbcCommand DbCommand = new OdbcCommand();
            try
            {
                MyConnection.Open();
                DbCommand = buildInsertStatement(h);
                DbCommand.ExecuteNonQuery();
                MyConnection.Close();
                continueSignup(h);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Error. Check data validity.");
            }
        }

        //move on to frmInit
        private void continueSignup(Hashtable h)
        {
            OdbcCommand DbCommand = new OdbcCommand();
            try//patient should already exist in DB
            {
                MyConnection.Open();
                DbCommand = buildQueryStatement(h);
                // Grab patientID from previously created entry
                string patientID = DbCommand.ExecuteScalar().ToString();
                MyConnection.Close();

                //continue on to next form
                frmInit fInit = new frmInit(patientID);
                fInit.Show();
                this.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in continueSignup:\n" + ex.ToString());
            }
        }

        //create SQL INSERT statement with Patient info for new patients
        private OdbcCommand buildInsertStatement(Hashtable h)
        {
            OdbcCommand dbcommand = MyConnection.CreateCommand();
            StringBuilder sb = new StringBuilder();
            
            try
            {
                sb.Append("INSERT INTO Patient (Last_Name, First_Name, Gender, Ethnicity, Comments, VUH, DOB, SSN, Diagnosis, Other_Diagnosis, Diagnosis_Questionable) VALUES");
                sb.Append("(");
                sb.Append("?");
                sb.Append(",");
                sb.Append("?");
                sb.Append(",'");
                sb.Append(h["Gender"]);
                sb.Append("','");
                sb.Append(h["Ethnicity"]);
                sb.Append("',");
                sb.Append("?");
                sb.Append(",");
                sb.Append("?");
                sb.Append(",STR_TO_DATE('");
                sb.Append(h["DOB"]);
                sb.Append("','%m/%d/%Y'),'");
                sb.Append(h["SSN"]);
                sb.Append("',");
                sb.Append("?");
                sb.Append(",");
                sb.Append("?");
                sb.Append(",'");
                sb.Append(h["Diagnosis_Questionable"]);
                sb.Append("'");
                sb.Append(");");

                dbcommand.CommandText = sb.ToString();
                dbcommand.Parameters.Add("@Last_Name", OdbcType.Text).Value = h["Last_Name"];
                dbcommand.Parameters.Add("@First_Name", OdbcType.Text).Value = h["First_Name"];
                dbcommand.Parameters.Add("@Comments", OdbcType.Text).Value = h["Comments"];
                dbcommand.Parameters.Add("@VUH", OdbcType.Text).Value = h["VUH"];
                dbcommand.Parameters.Add("@Diagnosis", OdbcType.Text).Value = h["Diagnosis"];
                dbcommand.Parameters.Add("@Other_Diagnosis", OdbcType.Text).Value = h["Other_Diagnosis"];

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in buildInsertStatement: \n" + ex.ToString());
            }

            return dbcommand;
        }

        //Build SQL query to see if patient already exists in DB (or
        //to retrieve index of registered patient)
        private OdbcCommand buildQueryStatement(Hashtable h)
        {
            OdbcCommand dbcommand = MyConnection.CreateCommand();
            StringBuilder sb = new StringBuilder();

            try
            {
                //Construct query from inputs (first and last name to begin)
                sb.Append("Select * from patient where First_Name = ");
                sb.Append("?");
                sb.Append(" AND Last_Name = ");
                sb.Append("?");
                sb.Append(" AND DOB = ");
                sb.Append("STR_TO_DATE('");
                sb.Append(h["DOB"]);
                sb.Append("','%m/%d/%Y')");

                dbcommand.CommandText = sb.ToString();

                dbcommand.Parameters.Add("@First_Name", OdbcType.Text).Value = h["First_Name"];
                dbcommand.Parameters.Add("@Last_Name", OdbcType.Text).Value = h["Last_Name"];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in buildQueryStatement: \n" + ex.ToString());
            }
            return dbcommand;
        }
        
        //User clicks Register Patient button
        private void btnRegister_Click(object sender, EventArgs e)
        {
            Hashtable h = validateForm(); // collect form data into hash
           
            if(h["Errors"].ToString() == "")//if no errors...
            {
                RegisterPatient(h);
            }
            else
            {
                MessageBox.Show("Please fix the following fields:\n" + h["Errors"]);
            }
        }

        // Upload form information to database after checking to see
        // if information corresponds to an existing patient.
        private void RegisterPatient(Hashtable h)
        {
            OdbcCommand DbCommand = new OdbcCommand();
            //connect to DB and send query
            try
            {
                //check if patient entry already exists in database
                MyConnection.Open();
                DbCommand = buildQueryStatement(h);
                if (DbCommand.ExecuteScalar() == null || DbCommand.ExecuteScalar().ToString() == "")
                {//patient is new, commit data
                    MyConnection.Close();
                    saveDB(h);
                }
                else
                {//patient already exists in DB, kick out error
                    MyConnection.Close();
                    MessageBox.Show("Patient already registered.\nGo back and try Existing Patient Login.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in RegisterPatient: \n" + ex.ToString());
            }
        }

        //Check for mandatory fields and formatting errors, return a hash
        //table with all field entries plus any errors.  Highlight fields with errors
        private Hashtable validateForm()
        {
            clearHighlights();
            Hashtable h = new Hashtable();
            h["Errors"] = "";

            // Check that mandatory fields were filled
            if (txtFName.Text != "")
                h["First_Name"] = txtFName.Text;
            else
            {
                h["Errors"] += "First Name\n";
                txtFName.BackColor = Color.Yellow;
            }

            if (txtLName.Text != "")
                h["Last_Name"] = txtLName.Text;
            else
            {
                h["Errors"] += "Last Name\n";
                txtLName.BackColor = Color.Yellow;
            }

            if (txtGender.SelectedItem != null)
                h["Gender"] = txtGender.SelectedItem.ToString();
            else
            {
                h["Errors"] += "Gender\n";
                txtGender.BackColor = Color.Yellow;
            }

            // Do a special check for DOB
            if (validateDOB())
                h["DOB"] = txtDOB.Text;
            else
            {
                h["Errors"] += "DOB\n";
                txtDOB.BackColor = Color.Yellow;
            }

            if (txtEth.SelectedItem != null)
                h["Ethnicity"] = txtEth.SelectedItem.ToString();
            else
            {
                h["Errors"] += "Ethnicity\n";
                txtEth.BackColor = Color.Yellow;
            }

            // Do special check for SSN (all numbers filled)
            if (txtSSN.Text != "   -  -")
                if (txtSSN.Text.Remove(6, 1).Remove(3, 1).Length == 9)
                    h["SSN"] = txtSSN.Text.Remove(6, 1).Remove(3, 1);
                else
                {
                    h["Errors"] += "SSN Length\n";
                    txtSSN.BackColor = Color.Yellow;
                }
            else
                h["SSN"] = txtSSN.Text.Remove(6, 1).Remove(3, 1);

            // Simply copy non-required fields
            h["VUH"] = txtVUH.Text;
            h["Diagnosis"] = txtDiag.Text;
            h["Other_Diagnosis"] = txtODiag.Text;
            h["Diagnosis_Questionable"] = txtQuestionable.Checked;
            h["Comments"] = txtComments.Text;

            // Convert string boolean to integer for checkbox
            if (h["Diagnosis_Questionable"].ToString() == "False")
                h["Diagnosis_Questionable"] = "0";
            else
                h["Diagnosis_Questionable"] = "1";
            
            return h;
        }

        // Close application when form's closed
        private void frmSignup_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // Enter key at any point attempts registration
        private void frmSignup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.Equals(Keys.Enter))
                btnRegister_Click(null, null);
        }

        // User clicks back button
        private void btnBack_Click(object sender, EventArgs e)
        {
            // Hide this window and show the initial window
            frmStart fStart = new frmStart();
            fStart.Show();
            this.Visible = false;
        }

        // Clear all fields of highlights indicating errors
        private void clearHighlights()
        {
            txtFName.BackColor = Color.White;
            txtLName.BackColor = Color.White;
            txtGender.BackColor = Color.White;
            txtDOB.BackColor = Color.White;
            txtEth.BackColor = Color.White;
            txtSSN.BackColor = Color.White;
        }

        // Check that DOB field was filled out without issues.
        private Boolean validateDOB()
        {
            if (txtDOB.Text.ToString() == "  /  /")
                return false;
            // Grab date-of-birth from textbox
            DateTime dateOfBirth = new DateTime();
            txtDOB.ValidatingType = typeof(System.DateTime);
            object DOB = txtDOB.ValidateText();

            // Validate date-of-birth, return an error to user if not valid
            if (DOB == null)
                return false;
            else
            {
                dateOfBirth = (DateTime)DOB;
                // Even if valide date, DOB cannot be future date
                if (dateOfBirth > DateTime.Now)
                    return false;
                else
                    return true;
            }
        }

    }
}
