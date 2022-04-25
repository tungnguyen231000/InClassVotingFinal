using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Controllers
{
    public class HomeTotalController : Controller
    {
        private DBModel db = new DBModel();

        [HandleError]
        public ActionResult Home()
        {
            return View();
        }

        [HandleError]
        public ActionResult Login()
        {
            if ("".Equals(Session["User"]))
            {
                return View();
            }
            else
            {
                if ("Teacher".Equals(Session["User"]))
                {
                    return Redirect("~/Teacher/Home/Home");
                }
                else if ("Student".Equals(Session["User"]))
                {
                    return Redirect("~/Student/Home/Home");
                }
                else if ("Admin".Equals(Session["User"]))
                {
                    return Redirect("~/Admin/Home/Home");
                }
            }
            return View();
        }


        [HandleError]
        public ActionResult Logout()
        {
            Session.Clear();
            return Redirect("~/");
        }


        [HandleError]
        [HttpPost]
        public ActionResult LoginAdmin(string inputUserName, string inputPassword)
        {
            var redirectUrl = "";
            try
            {
                var adminAcc = db.Admins.Where(a => a.Username.Trim().Equals(inputUserName) && a.Password.Equals(inputPassword)).FirstOrDefault();
                if (adminAcc != null /*inputUserName=="1"*/)
                {
                    Session["User"] = "Admin";

                    Session["Name"] = adminAcc.Username /*"admin"*/;
                    TempData["UserExist"] = "True";
                    return Redirect("~/Admin/Home/Home");
                }
                else
                {
                    /* Debug.WriteLine("2222222");*/
                    redirectUrl = new UrlHelper(Request.RequestContext).Action("Login", "HomeTotal");
                    TempData["UserExist"] = "False";
                    return Redirect(redirectUrl);
                }
            }
            catch
            {
                /* Debug.WriteLine("333333");*/
                redirectUrl = new UrlHelper(Request.RequestContext).Action("Login", "HomeTotal");
                return Redirect(redirectUrl);
            }
        }

        [HandleError]
        [HttpPost]
        public ActionResult getInfoUser(string email, string name, string image_URL)
        {
            var redirectUrl = "";
            try
            {
                if ("".Equals(email) || "".Equals(name) || "".Equals(image_URL))
                {
                    redirectUrl = new UrlHelper(Request.RequestContext).Action("Login", "HomeTotal");
                    return Json(new { Url = redirectUrl });
                }
                else
                {
                    string[] words = email.Split('@');
                    /*if (words[words.Length - 1].Equals("fe.edu.vn"))*/
                    if (words[words.Length - 1].Equals("gmail.com"))
                    {
                        Session["User"] = "Teacher";
                        Session["Name"] = name;
                        Session["Email"] = email;
                        Session["ImageURL"] = image_URL;

                        var teacherInDb = db.Teachers.Where(t => t.Email.ToLower().Trim().Equals(email.ToLower().Trim())).FirstOrDefault();

                        if (teacherInDb != null)
                        {
                            Session["TeacherId"] = teacherInDb.TID;
                        }
                        else
                        {
                            Teacher newTeacher = new Teacher();
                            newTeacher.Email = email;
                            newTeacher.Name = name;
                            db.Teachers.Add(newTeacher);
                            db.SaveChanges();
                            var getTeacher = db.Teachers.Where(t => t.Email.ToLower().Trim().Equals(newTeacher.Email.ToLower().Trim())).FirstOrDefault();
                            Session["TeacherId"] = getTeacher.TID;
                        }

                        redirectUrl = new UrlHelper(Request.RequestContext).Action("Home", "Home", new { area = "teacher" });

                        return Json(new { Url = redirectUrl });
                    }
                    else if (words[words.Length - 1].Equals("fpt.edu.vn"))
                    /*else if (words[words.Length - 1].Equals("gmail.com"))*/
                    {

                        Session["User"] = "Student";
                        Session["Name"] = name;
                        Session["Email"] = email;
                        Session["ImageURL"] = image_URL;

                        var studentInDb = db.Students.Where(s => s.Email.ToLower().Trim().Equals(email.ToLower().Trim())).FirstOrDefault();

                        if (studentInDb != null)
                        {
                            Session["StudentId"] = studentInDb.SID;
                        }
                        else
                        {
                            Student newStudent = new Student();
                            newStudent.Email = email;
                            newStudent.Name = name;
                            db.Students.Add(newStudent);
                            db.SaveChanges();
                            var getStudent = db.Students.Where(s => s.Email.ToLower().Trim().Equals(newStudent.Email.ToLower().Trim())).FirstOrDefault();
                            Session["StudentId"] = getStudent.SID;

                        }
                        if ("".Equals(Common.UrlQuiz))
                        {
                            redirectUrl = new UrlHelper(Request.RequestContext).Action("Home", "Home", new { area = "Student" });

                            return Json(new { Url = redirectUrl });
                        }
                        else
                        {
                            String url = Common.UrlQuiz;
                            Common.UrlQuiz = "";
                            return Json(new { Url = url });

                        }
                        /*
                         * redirectUrl = new UrlHelper(Request.RequestContext).Action("Home", "Home", new { area = "Student" });
                            return Json(new { Url = redirectUrl });
                        */
                        /*string resultString = Regex.Match(email, @"\d+").Value;

                        if (resultString.Length >= 5)
                        {
                            Session["User"] = "Student";
                            Session["Name"] = name;
                            Session["Email"] = email;
                            Session["ImageURL"] = image_URL;
                            var studentInDb = db.Students.Where(s => s.Email.ToLower().Trim().Equals(email.ToLower().Trim())).FirstOrDefault();

                            if (studentInDb != null)
                            {
                                Session["StudentId"] = studentInDb.SID;
                            }
                            else
                            {
                                Student newStudent = new Student();
                                newStudent.Email = email;
                                newStudent.Name = name;
                                db.Students.Add(newStudent);
                                db.SaveChanges();
                                var getStudent = db.Students.Where(s => s.Email.ToLower().Trim().Equals(newStudent.Email.ToLower().Trim())).FirstOrDefault();
                                Session["StudentId"] = getStudent.SID;
                            }
                            redirectUrl = new UrlHelper(Request.RequestContext).Action("Home", "Home", new { area = "Student" });
                            return Json(new { Url = redirectUrl });
                        }
                        else
                        {
                            Session["User"] = "Teacher";
                            Session["Name"] = name;
                            Session["Email"] = email;
                            Session["ImageURL"] = image_URL;
                            var teacherInDb = db.Teachers.Where(t => t.Email.ToLower().Trim().Equals(email.ToLower().Trim())).FirstOrDefault();

                            if (teacherInDb != null)
                            {
                                Session["TeacherId"] = teacherInDb.TID;
                            }
                            else
                            {
                                Teacher newTeacher = new Teacher();
                                newTeacher.Email = email;
                                newTeacher.Name = name;
                                db.Teachers.Add(newTeacher);
                                db.SaveChanges();
                                var getTeacher = db.Teachers.Where(t => t.Email.ToLower().Trim().Equals(newTeacher.Email.ToLower().Trim())).FirstOrDefault();
                                Session["TeacherId"] = getTeacher.TID;
                            }

                            redirectUrl = new UrlHelper(Request.RequestContext).Action("Home", "Home", new { area = "teacher" });
                            return Json(new { Url = redirectUrl });
                        }*/

                    }
                    else
                    {
                        redirectUrl = new UrlHelper(Request.RequestContext).Action("Login", "HomeTotal");
                        return Json(new { Url = redirectUrl });
                    }
                }

            }
            catch
            {
                redirectUrl = new UrlHelper(Request.RequestContext).Action("Login", "HomeTotal");
                return Json(new { Url = redirectUrl });
            }
        }

        //Check Admin Account
        [HttpPost]
        public JsonResult CheckAdminAccount(string user, string pass)
        {

            string username = user;
            string password = pass;
            string check = "";
            string message = "";

            var adminAcc = db.Admins.Where(a => a.Username.Trim().Equals(username) && a.Password.Equals(password)).FirstOrDefault();

            if (username == null || username.Trim().Equals(""))
            {
                check = "0";
                message = "Please enter username!";
            }
            else if (password == null || password.Trim().Equals(""))
            {
                check = "0";
                message = "Please enter password!";
            }
            else
            {
                if (adminAcc == null)
                {
                    check = "0";
                    message = "Username or password is incorrect!";
                }
                else
                {
                    check = "1";
                    message = "";
                }
            }


            return Json(new { mess = message, check = check });

        }
    }
}