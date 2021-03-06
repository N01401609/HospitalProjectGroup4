﻿using System;
using System.Collections.Generic;
using System.Data;
//required for SqlParameter class
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HospitalProjectTeam4.Data;
using HospitalProjectTeam4.Models;
using HospitalProjectTeam4.Models.ViewModels;
using System.Diagnostics;
using System.IO;
//needed for other sign in feature classes
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace HospitalProjectTeam4.Controllers
{
    public class RecordController : Controller

    {
        private HospitalProjectContext db = new HospitalProjectContext();
        // GET: Record
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult List()
        {
            
            string query = "Select * from Records join Bookings on Records.BookingID = Bookings.BookingID order by BookingDate Desc";

            //Checks to see if the query is being sent
            Debug.WriteLine(query);

            //Grabs all the records plus the associated information regarding that specific record
            List<Record> allrecords = db.Records.SqlQuery(query).ToList();

            ListRecords viewmodel = new ListRecords();
            viewmodel.records = allrecords;
          

            return View(viewmodel);
        }

        public ActionResult Show(int? id)
        {
            Debug.WriteLine(id);

            //Get the information regarding one record id 
            var first_query = "select * from Records where RecordID= @id";
            var first_parameter = new SqlParameter("@id", id);
            Record recordinfo = db.Records.SqlQuery(first_query,first_parameter ).FirstOrDefault();
            if (recordinfo == null)
            {
                return HttpNotFound();
            }
            var second_parameter = new SqlParameter("@id", id);
            //Find information about the booking related to that  record
            var second_query = "select * from Bookings join Records on Bookings.BookingID = Records.BookingID where RecordID= @id";
            Booking bookinginfo = db.Bookings.SqlQuery(second_query, second_parameter).FirstOrDefault();
            if (bookinginfo == null)
            {
                return HttpNotFound();
            }


            ListRecords viewmodel = new ListRecords();
            viewmodel.recordinfo = recordinfo;
            viewmodel.bookinginfo = bookinginfo;

            return View(viewmodel);
        }
        
        //Display the Add page
        public ActionResult Add()
        {
            return View();
        }

        //ADD A NEW RECORD TO THE DATABASE
        //Method is only called when it comes from a form submission
        //Parameters are all the values from the form
        [HttpPost]
        public ActionResult New(string recordName, string recordType, string recordContent, int bookingID, HttpPostedFileBase recordFile)
        {
            

            //CHECK IF THE VALUES ARE BEING PASSED INTO THE METHOD
            Debug.WriteLine("The values passed into the method are: " + recordName + ", " + recordType + ", " + recordContent + ", " + bookingID);

            //CREATE THE INSERT INTO QUERY
            string query = "insert into Records (RecordName, RecordType, RecordContent, BookingID) values (@recordName, @recordType, @recordContent, @bookingID)";

            //Binding the variables to the parameters
            SqlParameter[] sqlparams = new SqlParameter[4]; //0,1,2,3 pieces of information to add
            //each piece of information is a key and value pair
            sqlparams[0] = new SqlParameter("@recordName", recordName);
            sqlparams[1] = new SqlParameter("@recordType", recordType);
            sqlparams[2] = new SqlParameter("@recordContent", recordContent);
            sqlparams[3] = new SqlParameter("@bookingID", bookingID);

            //RUN THE QUERY WITH THE PARAMETERS 
            db.Database.ExecuteSqlCommand(query, sqlparams);

            //GRABING THE ID OF THE LAST RECORD WE JUST INSERTED
            //var idquery = "select RecordID from Records desc limit 1";

            return RedirectToAction("List");
        }

        //UPDATE 
        //Update contorller that pulls information for the page
        public ActionResult Update(int id)
        {
            string query = "select * from Records where RecordID = @id";
            var parameter = new SqlParameter("@id", id);
            Record selectedrecord = db.Records.SqlQuery(query, parameter).FirstOrDefault();

            return View(selectedrecord);
        }

        //UPDATE that actually changes the query
        [HttpPost]
        public ActionResult Update(int id, string recordName, string recordType, string recordContent, int bookingID, HttpPostedFileBase recordFile, string fileExtension, string fileDelete)
        {
            //We assume at the beggining that there is no document
            int hasfile = 0;
            string fileextension = "";

            //checking to see if some information is there
            //if they did input the pdf
            if (recordFile != null)
            {
                Debug.WriteLine("Something identified...");

                //checking to see if the file size is greater than 0 (bytes)
                //If it is it means that the extension is one of a picture
                if (recordFile.ContentLength > 0)
                {
                    Debug.WriteLine("Successfully Identified PDF");
                    //file extensioncheck taken from https://www.c-sharpcorner.com/article/file-upload-extension-validation-in-asp-net-mvc-and-javascript/
                    var valtypes = new[] { "pdf" };

                    //Identifies the exension at the end of the picture
                    var extension = Path.GetExtension(recordFile.FileName).Substring(1);

                    //if the extension is one of the valid types
                    if (valtypes.Contains(extension))
                    {
                        try
                        {
                            //file name is the id of the image
                            string fn = id + "." + extension;

                            //get a direct file path to ~/Content/Records/{id}.{extension}
                            string path = Path.Combine(Server.MapPath("~/Content/Records/"), fn);

                            //save the file
                            recordFile.SaveAs(path);
                            //if these are all successful then we can set these fields
                            hasfile  = 1;
                            fileextension = extension;

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Record Attachement was not saved successfully.");
                            Debug.WriteLine("Exception:" + ex);
                        }



                    }
                }
                //if a record was not sent make sure there isnt one in store already
            }
            else
            {
                //Check if there is a record
                if (fileExtension != null)
                {
                    //If there is a file do they wish to delete it?
                    if (fileDelete == "no")
                    {
                        //If they didn't choose to delete it, we assign these values so it pulls the exisitng record
                        hasfile = 1;
                        fileextension = fileExtension;
                    } // If they choose to delete it that means we keep the has record assigned at the beginning at 0 and assume  there is no record on the update

                }

            }


            Debug.WriteLine("I am trying to edit the follwoing values: " + recordName + ", " + recordType + ", " + recordContent + " "+ recordFile);

            string query = "update Records set RecordName=@recordName, RecordType=@recordType, RecordContent=@recordContent, BookingID=@bookingID, HasFile=@hasfile, FileExtension=@fileExtension where RecordID=@id";
            SqlParameter[] sqlparams = new SqlParameter[7];
            sqlparams[0] = new SqlParameter("@recordName", recordName);
            sqlparams[1] = new SqlParameter("@recordType", recordType);
            sqlparams[2] = new SqlParameter("@recordContent", recordContent);
            sqlparams[3] = new SqlParameter("@bookingID", bookingID);
            sqlparams[4] = new SqlParameter("@hasfile", hasfile);
            sqlparams[5] = new SqlParameter("@fileextension", fileextension);
            sqlparams[6] = new SqlParameter("@id", id);

            db.Database.ExecuteSqlCommand(query, sqlparams);


            return RedirectToAction("Show/"+id);
        }
        //DELETE CONFIRM PAGE
        //Sends the view of the delete confirmation with the info of the Record
        public ActionResult DeleteConfirm(int id)
        {
            string query = "select * from Records where RecordID=@id";
            SqlParameter param = new SqlParameter("@id", id);
            Record selectedrecord = db.Records.SqlQuery(query, param).FirstOrDefault();
            return View(selectedrecord);
        }

        //DELETING THE RECORDS FROM THE DATABASE
        [HttpPost]
        public ActionResult Delete(int id)
        {
            string query = "delete from Records where RecordID=@id";
            SqlParameter param = new SqlParameter("@id", id);
            db.Database.ExecuteSqlCommand(query, param);


            return RedirectToAction("List");
        }
    }
}