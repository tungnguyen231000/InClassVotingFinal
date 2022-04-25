using InClassVoting.Filter;
using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.teacher.Controllers
{
    /*[AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]*/
    public class ChapterController : Controller
    {
        private DBModel db = new DBModel();

        [HandleError]
        //Create Chapter
        [HttpPost]
        public ActionResult CreateChapter(string newChapCID, string chapterName /*, string chid*/)
        {
            try
            {
                Chapter chapter = new Chapter();
                int courseID = int.Parse(newChapCID);
                chapter.CourseID = courseID;
                chapter.Name = chapterName.Trim();
                db.Chapters.Add(chapter);
                db.SaveChanges();

                int chaptID = db.Chapters.Where(ch => ch.CourseID == courseID && ch.Name.Equals(chapterName)).OrderByDescending(ch => ch.ChID).Select(ch => chapter.ChID).FirstOrDefault();

               /* string previousPage = "~/Teacher/Question/QuestionBank" ;
                if (Request.UrlReferrer.ToString() != null  )
                {
                    previousPage = Request.UrlReferrer;
                }*/

                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid="+chaptID);


            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }


        //Check New chapter Name
        [HandleError]
        [HttpPost]
        public JsonResult CheckDuplicateNewChapter(string text, string cid)
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            string dataInput = text;
            int courseId = int.Parse(cid);
            string check = "";
            string message = "";


            if (dataInput.Trim() == "")
            {
                check = "0";
                message = "Please input chapter name !";
            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var chapter = db.Chapters.Where(ch => ch.Course.TeacherID == teacherId &&
                ch.CourseID == courseId &&
                ch.Name.ToLower().Trim().Equals(dataInput.ToLower().Trim())).FirstOrDefault();
                /*var currentCourse = db.Courses.Find(courseId);*/

                if (chapter != null)
                {
                    check = "0";
                    message = "This chapter name already existed!";
                }
                else
                {
                    check = "1";
                    message = "";
                }
            }

            return Json(new { mess = message, check = check });

        }

        [HandleError]
        //Edit Chapter Name
        [HttpPost]
        public ActionResult EditChapter(string newChapterName, string chid)
        {
            try
            {
                int chapterID = int.Parse(chid);
                //check for dupilcate name
                var updateChapter = db.Chapters.Find(chapterID);
                updateChapter.Name = newChapterName.Trim();
                db.Entry(updateChapter).State = EntityState.Modified;
                db.SaveChanges();

                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + chapterID;
                if (Request.UrlReferrer != null)
                {
                    previousPage = Request.UrlReferrer.ToString();
                }

                return Redirect(previousPage);


            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //Check New chapter Name
        [HttpPost]
        public JsonResult CheckDuplicateEditChapter(string text, string cid, string chid)
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            string dataInput = text;
            int courseId = int.Parse(cid);
            int chapterId = int.Parse(chid);
            string check = "";
            string message = "";



            if (dataInput.Trim() == "")
            {
                check = "0";
                message = "Please input chapter name !";
            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var chapter = db.Chapters.Where(ch => ch.Course.TeacherID == teacherId &&
                ch.CourseID == courseId && ch.ChID != chapterId &&
                ch.Name.ToLower().Trim().Equals(dataInput.ToLower().Trim())).FirstOrDefault();
                var currentChapter = db.Chapters.Find(chapterId);

                if (chapter != null)
                {
                    check = "0";
                    message = "This chapter name already existed!";
                }
                else
                {
                    if (dataInput.Trim().ToLower().Equals(currentChapter.Name.Trim().ToLower()))
                    {
                        check = "0";
                        message = "Your chapter name is unchange!";
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
        //Delete Chapter
        [HttpPost]
        public ActionResult DeleteChapter(string chid)
        {

            int chapterId = int.Parse(chid);
            var chapter = db.Chapters.Find(chapterId);

            var questionInsideChapter = db.Questions.Where(q => q.ChapterID == chapterId).ToList();
            //delete question inside chap
            foreach (var question in questionInsideChapter)
            {
                var answers = db.QuestionAnswers.Where(q => q.QuestionID == question.QID).ToList();
                //delete answer of a question
                foreach (var answer in answers)
                {
                    db.QuestionAnswers.Remove(answer);
                }

                var lo = db.QuestionLOes.Where(l => l.QuestionID == question.QID && l.Qtype == question.Qtype).ToList();
                //delete lo of a question
                foreach (var o in lo)
                {
                    db.QuestionLOes.Remove(o);
                }

                db.Questions.Remove(question);
            }

            var questionDoneContain = db.QuestionDones.Where(q => q.ChapterID == chapterId).ToList();
            //remove chapter of question done
            foreach (var questionDone in questionDoneContain)
            {
                questionDone.ChapterID = null;
                db.Entry(questionDone).State = EntityState.Modified;
            }
            db.Chapters.Remove(chapter);

            var passageContain = db.Passages.Where(p => p.ChapterID == chapterId).ToList();

            //remove passage inside chapter
            foreach (var passage in passageContain)
            {
                db.Passages.Remove(passage);
            }

            var passageDoneContain = db.Passage_Done.Where(p => p.ChapterID == chapterId).ToList();
            //remove chapter of question done
            foreach (var passageD in passageDoneContain)
            {
                passageD.ChapterID = null;
                db.Entry(passageD).State = EntityState.Modified;
            }

            var matchQuestionInsideChapter = db.MatchQuestions.Where(m => m.ChapterId == chapterId).ToList();
            //delete match question inside chap
            foreach (var question in matchQuestionInsideChapter)
            {
                var lo = db.QuestionLOes.Where(l => l.QuestionID == question.MID && l.Qtype == 5).ToList();
                //delete lo of a question
                foreach (var o in lo)
                {
                    db.QuestionLOes.Remove(o);
                }
                db.MatchQuestions.Remove(question);
            }

            var matchQuestionDoneContain = db.QuestionDones.Where(q => q.ChapterID == chapterId).ToList();
            //remove chapter of question done
            foreach (var questionDone in matchQuestionDoneContain)
            {
                questionDone.ChapterID = null;
                db.Entry(questionDone).State = EntityState.Modified;
            }

            db.Chapters.Remove(chapter);

            db.SaveChanges();
            int courseID = int.Parse(chid);
            string previousPage = "~/Teacher/Question/QuestionBank";
            if (Request.UrlReferrer != null)
            {
                previousPage = Request.UrlReferrer.ToString();
            }

            return Redirect(previousPage);

        }

    }

}
