using InClassVoting.Filter;
using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.Teacher.Controllers.PollController
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]
    public class PollController : Controller
    {
        private DBModel db = new DBModel();

        private bool checkPollIdAvailbile(string poid)
        {
            bool check = true;
            int pollID;
            bool isInt = int.TryParse(poid, out pollID);
            //check if chapter id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var poll = db.Polls.Find(pollID);
                //check if chapter exist in db
                if (poll == null)
                {
                    check = false;
                }
                else
                {
                    //check if chapter belong to teacher
                    if (poll.TeacherID != teacherId)
                    {
                        check = false;
                    }
                }
            }
            return (check);
        }

        [HandleError]
        public ActionResult CreatePoll()
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            return View();
        }

        [HandleError]
        [HttpPost]
        public ActionResult CreatePoll(FormCollection form)
        {
            try
            {
                string questionText = form["questionText"];
                string pollName = form["pollName"];
                string pollType = form["rdPollType"];
                List<string> optionList = new List<string>();
                int? polltime = null;
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);

                if (form["option"] != null && !form["option"].Equals(""))
                {
                    optionList = Request.Form.GetValues("option").ToList().ToList();
                }

                if (form["time"] != "" && !form["time"].Equals(""))
                {
                    polltime = int.Parse(form["time"]);
                }
                int ptype = int.Parse(pollType);

                Poll newPoll = new Poll();

                newPoll.PollName = pollName.Trim();
                newPoll.Question = questionText.Trim();
                newPoll.Time = polltime;
                newPoll.TotalParticipian = 0;
                newPoll.IsDoing = false;
                newPoll.TeacherID = teacherId;
                newPoll.CreatedDate = DateTime.Today;
                if (ptype == 1)
                {
                    newPoll.Polltype = "MultipleAnswer";
                }
                else
                {
                    newPoll.Polltype = "OneAnswer";
                }

                db.Polls.Add(newPoll);

                db.SaveChanges();

                int pollID = db.Polls.Where(p => p.TeacherID == teacherId).OrderByDescending(p => p.PollID).Select(p => p.PollID).FirstOrDefault();

                foreach (string opt in optionList)
                {
                    if (!opt.Trim().Equals("") && opt != null)
                    {
                        Poll_Answer pollAns = new Poll_Answer();
                        pollAns.Answer_Text = opt.Trim();
                        pollAns.PollID = pollID;
                        pollAns.ChosenQuantity = 0;
                        db.Poll_Answer.Add(pollAns);
                    }
                }

                db.SaveChanges();

                return Redirect("~/Teacher/Poll/ShowPollLink?poid=" + pollID);

            }
            catch
            { return Redirect("~/Error/NotFound"); }

        }
        
        
        [HandleError]
        [HttpPost]
        public ActionResult ReopenPoll(FormCollection form)
        {
            try
            {
                string oldPollID = form["oldPollID"];
                string pollName = form["newPollName"];

                var oldPoll = db.Polls.Find(int.Parse(oldPollID));

                Poll newPoll = new Poll();

                newPoll.PollName = pollName.Trim();
                newPoll.Question = oldPoll.Question.Trim();
                newPoll.Time = oldPoll.Time;
                newPoll.TotalParticipian = 0;
                newPoll.IsDoing = false;
                newPoll.TeacherID = oldPoll.TeacherID;
                newPoll.CreatedDate = DateTime.Today;
                newPoll.Polltype = oldPoll.Polltype;

                db.Polls.Add(newPoll);

                db.SaveChanges();

                int pollID = db.Polls.Where(p => p.TeacherID == oldPoll.TeacherID).OrderByDescending(p => p.PollID).Select(p => p.PollID).FirstOrDefault();


                List<string> oldPollAnswer = oldPoll.Poll_Answer.Select(pa => pa.Answer_Text).ToList();

                foreach (string opt in oldPollAnswer)
                {
                    if (!opt.Equals("") && opt != null)
                    {
                        Poll_Answer pollAns = new Poll_Answer();
                        pollAns.Answer_Text = opt.Trim();
                        pollAns.PollID = pollID;
                        pollAns.ChosenQuantity = 0;
                        db.Poll_Answer.Add(pollAns);
                    }
                }

                db.SaveChanges();

                return Redirect("~/Teacher/Poll/ShowPollLink?poid=" + pollID);

            }
            catch
            { return Redirect("~/Error/NotFound"); }

        }

        [HandleError]
        public ActionResult ShowPollLink(string poid)
        {
            //check if poll is availble
            if (checkPollIdAvailbile(poid) == false)
            {
                return RedirectToAction("CreatePoll");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int pollID = int.Parse(poid);
                var poll = db.Polls.Find(pollID);
                string pollLinkEnCode = Base64Encode(poid);
                ViewBag.PollLink = "https://inclassvoting.azurewebsites.net/Student/Poll/DoPoll?poid=" + pollLinkEnCode;
                /*ViewBag.PollLink = "https://localhost:44350/Student/Poll/DoPoll?poid=" + pollLinkEnCode;*/


                ViewBag.Poll = poll;
                return View();
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        [HandleError]
        public ActionResult StartPoll(string poid)
        {
            //check if poll is availble
            if (checkPollIdAvailbile(poid) == false)
            {
                return RedirectToAction("CreatePoll");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int pollID = int.Parse(poid);
                var poll = db.Polls.Find(pollID);

                //if poll have time
                if (poll.Time != null)
                {
                    DateTime now = DateTime.Now;
                    if (poll.EndTime == null)
                    {
                        DateTime endTime = now.AddSeconds(double.Parse(poll.Time.ToString()));
                        poll.EndTime = endTime;
                        TimeSpan totalTime = (endTime - now);
                        int countDown = (int)totalTime.TotalSeconds;
                        ViewBag.CountDown = countDown + 10;
                    }
                    else
                    {
                        DateTime? endTime = poll.EndTime;
                        //if poll is expired
                        if (endTime < now)
                        {
                            poll.IsDoing = false;
                            db.Entry(poll).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ShowPollResult", new { poid = poid });
                        }
                        else
                        {
                            TimeSpan? totalTime = (endTime - now);
                            int countDown = 0;
                            if (totalTime.HasValue)
                            {
                                countDown = (int)totalTime.Value.TotalSeconds;
                            }

                            ViewBag.CountDown = countDown + 10;
                        }
                    }


                }
                poll.IsDoing = true;
                db.Entry(poll).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.Poll = poll;
                string pollLinkEnCode = Base64Encode(poid);
                ViewBag.PollLink = "https://inclassvoting.azurewebsites.net/Student/Poll/DoPoll?poid=" + pollLinkEnCode;
                /*ViewBag.PollLink = "https://localhost:44350/Student/Poll/DoPoll?poid=" + pollLinkEnCode;*/

                return View();
            }
        }

        [HandleError]
        [HttpPost]
        public ActionResult EndPoll(string poid)
        {
            int pollID = int.Parse(poid);
            var poll = db.Polls.Find(pollID);
            poll.IsDoing = false;
            db.Entry(poll).State = EntityState.Modified;
            db.SaveChanges();

            return Redirect("~/Teacher/Poll/ShowPollResult?poid=" + pollID);
        }

        [HandleError]
        public ActionResult ShowPollResult(string poid)
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int pollID = int.Parse(poid);
            var poll = db.Polls.Find(pollID);
            if (poll.Polltype.Equals("MultipleAnswer"))
            {
                return Redirect("~/Teacher/Poll/PollResultProgressBar?poid=" + pollID);
            }
            else
            {
                return Redirect("~/Teacher/Poll/PollResultPieChart?poid=" + pollID);
            }

        }

        [HandleError]
        public ActionResult PollResultProgressBar(string poid)
        {
            //check if poll is availble
            if (checkPollIdAvailbile(poid) == false)
            {
                return Redirect("~/Teacher/Home/Home");
            }
            else
            {
                int pollID = int.Parse(poid);
                var poll = db.Polls.Find(pollID);
                if (poll.IsDoing == true)
                {
                    return RedirectToAction("StartPoll", new { poid = poid });
                }
                else
                {
                    ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                    ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                    ViewBag.Poll = poll;
                    return View();
                }
            }

        }

        [HandleError]
        public ActionResult PollResultPieChart(string poid)
        {
            //check if poll is availble
            if (checkPollIdAvailbile(poid) == false)
            {
                return Redirect("~/Teacher/Home/Home");
            }
            else
            {
                int pollID = int.Parse(poid);
                var poll = db.Polls.Find(pollID);
                if (poll.IsDoing == true)
                {
                    return RedirectToAction("StartPoll", new { poid = poid });
                }
                else
                {
                    ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                    ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                    ViewBag.Poll = poll;
                    return View();
                }
            }

        }


        [HandleError]
        [HttpPost]
        public ActionResult DeletePoll(string poid)
        {
            int pollID = int.Parse(poid);
            var poll = db.Polls.Find(pollID);

            List<Poll_Answer> pollAns = poll.Poll_Answer.ToList();

            foreach(var ans in pollAns)
            {
                List<Student_PollAnswer> studentPoll = ans.Student_PollAnswer.ToList();
                foreach(var stuPollAn in studentPoll)
                {
                    db.Student_PollAnswer.Remove(stuPollAn);

                }
                db.Poll_Answer.Remove(ans);
            }

            db.Polls.Remove(poll);
            db.SaveChanges();

           

            return Redirect("~/Teacher/Report/ReportPollList");
        }

        //Check New Poll Name
        [HandleError]
        [HttpPost]
        public JsonResult CheckLengthPollName(string text)
        {/*
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);*/
            string dataInput = text;
            string check = "";
            string message = "";

            if (dataInput.Trim() == "")
            {
                check = "0";
                message = "Please input poll name !";
            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
            }
            else
            {
                    check = "1";
                    message = "";
            }

            Debug.WriteLine(check + "======" + message);
            return Json(new { mess = message, check = check });

        }
    }
}