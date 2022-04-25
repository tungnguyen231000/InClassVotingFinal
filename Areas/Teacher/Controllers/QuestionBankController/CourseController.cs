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
    public class CourseController : Controller
    {
        private DBModel db = new DBModel();

        [HandleError]
        public PartialViewResult ShowCourseListForQuestionBank()
        {
            int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
            List<Course> courseList = db.Courses.Where(c => c.TeacherID == teacherId).ToList();
            ViewBag.CourseList = courseList;
            ViewBag.ChapterList = db.Chapters.Where(ch => ch.Course.TeacherID == teacherId).ToList();
            ViewBag.CourseCount = courseList.Count;
            ViewBag.SelectedCourse = Convert.ToInt32(HttpContext.Session["SelectedCourse"]);
            ViewBag.SelectedChapter = Convert.ToInt32(HttpContext.Session["SelectedChapter"]);
            return PartialView("_ShowCourseListForQuestionBank");
        }

        [HandleError]
        public PartialViewResult ShowCourseListForQuizLibrary()
        {
            int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
            List<Course> courseList = db.Courses.Where(c => c.TeacherID == teacherId).ToList();
            ViewBag.CourseList = courseList;
            ViewBag.ChapterList = db.Chapters.Where(ch => ch.Course.TeacherID == teacherId).ToList();
            ViewBag.CourseCount = courseList.Count;
            return PartialView("_ShowCourseListForQuizLibrary");

        }

        [HandleError]
        public PartialViewResult ShowCourseListForReport()
        {
            int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
            List<Course> courseList = db.Courses.Where(c => c.TeacherID == teacherId).ToList();
            ViewBag.CourseList = courseList;
            ViewBag.ChapterList = db.Chapters.Where(ch => ch.Course.TeacherID == teacherId).ToList();
            ViewBag.CourseCount = courseList.Count;
            return PartialView("_ShowCourseListForReport");

        }

        [HandleError]
        //Create New Course
        [HttpPost]
        public ActionResult CreateCourse(string newcourseName)
        {
            try
            {
                Course course = new Course();
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                course.TeacherID = teacherId;
                course.Name = newcourseName.ToUpper().Trim();
                db.Courses.Add(course);
                db.SaveChanges();

                string previousPage = "~/Teacher/Question/QuestionBank";
                if (Request.UrlReferrer != null)
                {
                    previousPage = Request.UrlReferrer.ToString();
                }

                return Redirect(previousPage);

            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //Check New Course Name
        [HttpPost]
        public JsonResult CheckDuplicateCourse(string text)
        {
            /* ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
             ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);*/

            string dataInput = text;
            string check = "";
            string message = "";

            if (dataInput.Trim() == "")
            {
                check = "0";
                message = "Please input course name !";
            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var course = db.Courses.Where(c => c.TeacherID == teacherId && c.Name.ToLower().Trim().Equals(dataInput.ToLower().Trim())).FirstOrDefault();

                if (course != null)
                {
                    check = "0";
                    message = "This course name already existed!";
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
        //Edit CourseName
        [HttpPost]
        public ActionResult EditCourse(string newCourseName, string courseIdUpdate/*, string chid*/)
        {
            try
            {
                int courseIdToUpdate = int.Parse(courseIdUpdate);
                var updateCourse = db.Courses.Find(courseIdToUpdate);
                updateCourse.Name = newCourseName.ToUpper().Trim();
                db.Entry(updateCourse).State = EntityState.Modified;
                db.SaveChanges();
                string previousPage = "~/Teacher/Question/QuestionBank";
                if (Request.UrlReferrer != null)
                {
                    previousPage = Request.UrlReferrer.ToString();
                }

                return Redirect(previousPage);

            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }


        //Check New Course Name when edit name
        [HttpPost]
        public JsonResult CheckDuplicateEditCourse(string text, string cid)
        {
            /* ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
             ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);*/
            string dataInput = text.Trim();
            int courseId = int.Parse(cid);
            string check = "";
            string message = "";


            if (dataInput.Trim() == "")
            {
                check = "0";
                message = "Please input course name!";
            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var course = db.Courses.Where(c => c.TeacherID == teacherId &&
                c.CID != courseId &&
                c.Name.ToLower().Trim().Equals(dataInput.ToLower().Trim())).FirstOrDefault();
                var currentCourse = db.Courses.Find(courseId);

                if (course != null)
                {
                    check = "0";
                    message = "This course name already existed!";
                }
                else
                {
                    if (dataInput.Trim().ToLower().Equals(currentCourse.Name.Trim().ToLower()))
                    {
                        check = "0";
                        message = "Your course name is unchange!";
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
        //delete course
        [HttpPost]
        public ActionResult DeleteCourse(string courseIdDelete)
        {
            int courseIdToDelete = int.Parse(courseIdDelete);
            var deleteCourse = db.Courses.Find(courseIdToDelete);
            var chapterInsideCourse = db.Chapters.Where(ch => ch.CourseID == courseIdToDelete).ToList();
            var loInsideCourse = db.LearningOutcomes.Where(lo => lo.CourseID == courseIdToDelete).ToList();
            var quizInsideCourse = db.Quizs.Where(qz => qz.CourseID == courseIdToDelete).ToList();
            var quizDoneInsideCourse = db.QuizDones.Where(qz => qz.CourseID == courseIdToDelete).ToList();

            //delete lo inside course
            foreach (var lo in loInsideCourse)
            {
                var questionContainLO = db.QuestionLOes.Where(ql => ql.LearningOutcomeID == lo.LOID).ToList();
                //delete LO from question
                foreach (var questionLO in questionContainLO)
                {
                    db.QuestionLOes.Remove(questionLO);
                }

                var questionDoneContainLO = db.QuestionDoneLOes.Where(ql => ql.LearningOutcomeID == lo.LOID).ToList();
                //delete LO from question that is done
                foreach (var questionLO in questionDoneContainLO)
                {
                    db.QuestionDoneLOes.Remove(questionLO);
                }
                db.LearningOutcomes.Remove(lo);
            }

            //delete question inside course
            foreach (var chapter in chapterInsideCourse)
            {
                var questionInsideChapter = db.Questions.Where(q => q.ChapterID == chapter.ChID).ToList();
                //delete question inside chap
                foreach (var question in questionInsideChapter)
                {
                    var answers = db.QuestionAnswers.Where(q => q.QuestionID == question.QID).ToList();
                    //delete answer of a question
                    foreach (var answer in answers)
                    {
                        db.QuestionAnswers.Remove(answer);
                    }
                    db.Questions.Remove(question);
                }

                var matchingquestionInsideChapter = db.MatchQuestions.Where(m => m.ChapterId == chapter.ChID).ToList();
                //delete match question inside chap
                foreach (var question in matchingquestionInsideChapter)
                {
                    db.MatchQuestions.Remove(question);
                }

                var passageContain = db.Passages.Where(p => p.ChapterID == chapter.ChID).ToList();

                //remove passage inside chapter
                foreach (var passage in passageContain)
                {
                    db.Passages.Remove(passage);
                }
                /*
                                var passageDoneContain = db.Passage_Done.Where(p => p.ChapterID == chapter.ChID).ToList();
                                //remove chapter of question done
                                foreach (var passageD in passageDoneContain)
                                {
                                    db.Passage_Done.Remove(passageD);
                                }*/
            }

            //delete quiz inside course
            foreach (var quiz in quizInsideCourse)
            {
                db.Quizs.Remove(quiz);
            }

            //delete report inside course
            foreach (var quizDone in quizDoneInsideCourse)
            {
                string[] questSet = quizDone.Questions.Split(new char[] { ';' });

                //delete question inside report
                foreach (string qIdAndType in questSet)
                {
                    string[] questAndType = qIdAndType.Split(new char[] { '-' });
                    int qtypeID = int.Parse(questAndType[1]);
                    if (qtypeID == 5)
                    {
                        int mID = int.Parse(questAndType[0]);
                        var matchQuest = db.MatchQuestionDones.Find(mID); var questionDoneLO = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == mID && ql.Qtype == 5).ToList();
                        //deleteLO
                        foreach (var qlo in questionDoneLO)
                        {
                            db.QuestionDoneLOes.Remove(qlo);
                        }
                        db.MatchQuestionDones.Remove(matchQuest);

                    }
                    else if (qtypeID == 2)
                    {
                        int pID = int.Parse(questAndType[0]);
                        var passage = db.Passage_Done.Find(pID);
                        var qList = passage.QuestionDones;
                        List<QuestionDone> deleteQuestionList = new List<QuestionDone>();
                        foreach (var q in qList)
                        {
                            var qaList = db.QuestionAnswerDones.Where(qa => qa.QuestionID == q.Q_DoneID).ToList();
                            //remove answer of question
                            foreach (var qa in qaList)
                            {
                                db.QuestionAnswerDones.Remove(qa);
                            }

                            var questionDoneLO = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == q.Q_DoneID && ql.Qtype == q.Qtype).ToList();

                            //deleteLO
                            foreach (var qlo in questionDoneLO)
                            {
                                db.QuestionDoneLOes.Remove(qlo);
                            }
                            deleteQuestionList.Add(q);


                        }
                        foreach (QuestionDone q in deleteQuestionList)
                        {
                            db.QuestionDones.Remove(q);
                        }
                        db.Passage_Done.Remove(passage);
                    }
                    else
                    {
                        int qID = int.Parse(questAndType[0]);
                        var questionDone = db.QuestionDones.Find(qID);
                        var qaList = db.QuestionAnswerDones.Where(qa => qa.QuestionID == qID).ToList();
                        //remove answer of question
                        foreach (var qa in qaList)
                        {
                            db.QuestionAnswerDones.Remove(qa);
                        }
                        var questionDoneLO = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == qID && ql.Qtype == questionDone.Qtype).ToList();

                        //deleteLO
                        foreach (var qlo in questionDoneLO)
                        {
                            db.QuestionDoneLOes.Remove(qlo);
                        }
                        db.QuestionDones.Remove(questionDone);
                    }
                }

                var studentAnswers = db.Student_Answer.Where(sa => sa.QuizDoneID == quizDone.QuizDoneID).ToList();
                //delete student answer
                foreach (var studentAnswer in studentAnswers)
                {
                    db.Student_Answer.Remove(studentAnswer);
                }

                var studenWorks = db.Student_QuizDone.Where(sq => sq.QuizDoneID == quizDone.QuizDoneID).ToList();
                //delete student report
                foreach (var studentWork in studenWorks)
                {
                    db.Student_QuizDone.Remove(studentWork);
                }


                db.QuizDones.Remove(quizDone);
            }

            //delete chapter inside course
            foreach (var chapter in chapterInsideCourse)
            {
                db.Chapters.Remove(chapter);
            }

            db.Courses.Remove(deleteCourse);
            db.SaveChanges();

            //delete quiz inside course

            /* int chapID = int.Parse(chid);*/
            string previousPage = "~/Teacher/Question/QuestionBank";
            if (Request.UrlReferrer != null)
            {
                previousPage = Request.UrlReferrer.ToString();
            }

            return Redirect(previousPage);
        }

    }
}
