using InClassVoting.Filter;
using InClassVoting.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.Teacher.Controllers.QuestionBankController
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]
    public class LearningOutcomeController : Controller
    {
        private DBModel db = new DBModel();

        private bool checkCourserIdAvailbile(string cid)
        {
            bool check = true;
            int courseId;
            bool isInt = int.TryParse(cid, out courseId);
            //check if chapter id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var course = db.Courses.Find(courseId);
                //check if chapter exist in db
                if (course == null)
                {
                    check = false;
                }
                else
                {
                    //check if chapter belong to teacher
                    if (course.TeacherID != teacherId)
                    {
                        check = false;
                    }
                }
            }
            return (check);
        }

        [HandleError]
        public ActionResult ViewLearningOutcome(string cid, string searchText, int? i)
        {
            //check if course is availble
            if (checkCourserIdAvailbile(cid) == false)
            {
                return Redirect("~/Teacher/Question/QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int courseID = int.Parse(cid);
                Course course = db.Courses.Find(courseID);

                var loList = db.LearningOutcomes.Where(lo => lo.CourseID == courseID).ToList();

                if (searchText != null && !searchText.Trim().Equals(""))
                {
                    loList = loList.Where(lo => lo.LO_Description.ToLower().Trim().Contains(searchText.ToLower().Trim()) ||
                    lo.LO_Name.ToLower().Trim().Contains(searchText.ToLower().Trim())).ToList();
                }
                int totalLo = db.LearningOutcomes.Count(lo => lo.CourseID == courseID);
                ViewBag.Course = course;
                ViewBag.CountLO = totalLo;
                ViewBag.Search = searchText;

                if (i == null || i < 1)
                {
                    i = 1;
                }
                else
                {
                    if (totalLo % 10 == 0 && i > totalLo / 10 && totalLo!=0)
                    {
                        i = (totalLo / 10);

                    }
                    else if (totalLo % 10 != 0 && i > ((totalLo / 10) + 1))
                    {
                        i = (totalLo / 10)+1;
                    }

                }

                ViewBag.LONo = (i - 1) * 10;
                Session["SelectedChapter"] = null;
                Session["SelectedCourse"] = courseID;
                ViewBag.Page = i;

                return View(loList.ToPagedList(i ?? 1, 10));
            }
        }

        // Create New LO
        [HandleError]
        [HttpPost]
        public ActionResult CreateLearningOutcome(string cid, string loName, string loDes)
        {
            try
            {
                LearningOutcome lo = new LearningOutcome();
                int courseID = int.Parse(cid);
                lo.CourseID = courseID;
                lo.LO_Name = loName.ToUpper().Trim();
                lo.LO_Description = loDes.Trim();
                db.LearningOutcomes.Add(lo);
                db.SaveChanges();
                int lastPage = 0;
                int countLo = db.LearningOutcomes.Count(l => l.CourseID == courseID);
                if ((countLo % 10) != 0)
                {
                    lastPage = (countLo / 10) + 1;

                }
                else
                {
                    lastPage = countLo / 10;
                }
                return Redirect("~/Teacher/LearningOutcome/ViewLearningOutcome?cid=" + cid + "&i=" + lastPage);

            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //Check New LO Name
        [HttpPost]
        public JsonResult CheckDuplicateNewLO(string text, string cid, string descrip)
        {
            string dataInput = text.Trim();
            string descriptionLO = descrip;
            int courseId = int.Parse(cid);
            string check = "";
            string message = "";


            if (dataInput.Trim().Equals("") || dataInput == null)
            {
                check = "0";
                message = "Please enter learning outcome name !";
            }
            else if (dataInput.Trim().Length > 50)
            {
                check = "0";
                message = "You can only enter 50 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var lo = db.LearningOutcomes.Where(l => l.Course.TeacherID == teacherId &&
                l.CourseID == courseId &&
                l.LO_Name.ToLower().Trim().Equals(dataInput.ToLower())).FirstOrDefault();
                /*var currentCourse = db.Courses.Find(courseId);*/
                if (lo != null)
                {
                    check = "0";
                    message = "This learning outcome name already existed!";
                }
                else
                {

                    if (descriptionLO.Trim().Equals("") || descriptionLO == null)
                    {
                        check = "2";
                        message = "Please enter learning outcome description!";
                    }
                    else
                    {
                        check = "1";
                        message = "";
                    }
                }

            }


            return Json(new { mess = message, check = check });

        }


        //Update LO
        [HandleError]
        [HttpPost]
        public ActionResult EditLearningOutcome(string cid, string newLoName, string newLoDes, string loid)
        {
            try
            {
                var lo = db.LearningOutcomes.Find(int.Parse(loid));
                lo.LO_Name = newLoName.ToUpper().Trim();
                lo.LO_Description = newLoDes.Trim();
                int courseID = lo.CourseID;
                db.Entry(lo).State = EntityState.Modified;
                db.SaveChanges();
                string previousPage = "~/Teacher/LearningOutcome/ViewLearningOutcome?cid="+courseID;
                if (Request.UrlReferrer != null)
                {
                    previousPage = Request.UrlReferrer.ToString();
                }

                return Redirect(previousPage);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }


        //Check edit LO Name
        [HttpPost]
        public JsonResult CheckDuplicateEditLO(string text, string cid, string loid, string descrip)
        {
/*            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);*/
            string descriptionLO = descrip.Trim();
            string dataInput = text.Trim();
            int courseId = int.Parse(cid);
            int loId = int.Parse(loid);
            string check = "";
            string message = "";


            if (dataInput.Trim() == "" || dataInput==null)
            {
                check = "0";
                message = "Please enter learning outcome name !";
            }
            else if (dataInput.Trim().Length > 50)
            {
                check = "0";
                message = "You can only enter 50 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var lo = db.LearningOutcomes.Where(l => l.Course.TeacherID == teacherId &&
                l.CourseID == courseId && l.LOID != loId &&
                l.LO_Name.ToLower().Trim().Equals(dataInput.ToLower().Trim())).FirstOrDefault();
                var curentLO = db.LearningOutcomes.Find(loId);

                if (lo != null)
                {
                    check = "0";
                    message = "This learning outcome name already existed!";
                }
                else
                {
                    if (descriptionLO.Trim().Equals("") || descriptionLO == null)
                    {
                        check = "2";
                        message = "Please enter learning outcome description!";
                    }
                    else
                    {
                        check = "1";
                        message = "";
                    }
                }

                
            }

            return Json(new { mess = message, check = check });

        }

        [HandleError]
        public ActionResult DeleteLearningOutcome(string cid, FormCollection collection, string page, string searchText)
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int courseID = int.Parse(cid);
            var loListSelected = collection["loid"];
            int? i=null;
            if (collection["page"] != null)
            {
                i = int.Parse(page);
                Debug.WriteLine(collection["page"]+"--=--=-");
            }
            if (loListSelected == null)
            {

                string previousPage = "~/Teacher/LearningOutcome/ViewLearningOutcome?cid=" + courseID;
                

                return Redirect(previousPage);

            }
            else
            {
                string[] loIdList = collection["loid"].Split(new char[] { ',' });
                foreach (string lo in loIdList)
                {
                    int loID = int.Parse(lo);

                    var questionContainLO = db.QuestionLOes.Where(ql => ql.LearningOutcomeID == loID).ToList();
                    //delete LO from question
                    foreach (var questionLO in questionContainLO)
                    {
                        db.QuestionLOes.Remove(questionLO);
                    }

                    var questionDoneContainLO = db.QuestionDoneLOes.Where(ql => ql.LearningOutcomeID == loID).ToList();
                    //delete LO from question that is done
                    foreach (var questionLO in questionDoneContainLO)
                    {
                        db.QuestionDoneLOes.Remove(questionLO);
                    }

                    var learnOC = db.LearningOutcomes.Find(loID);
                    //delete LO
                    db.LearningOutcomes.Remove(learnOC);
                }
                db.SaveChanges();
                Debug.WriteLine(i + "====");
                return Redirect("~/Teacher/LearningOutcome/ViewLearningOutCome?cid="+cid+"&searhText="+searchText+"&i="+i);
            }



        }
    }
}