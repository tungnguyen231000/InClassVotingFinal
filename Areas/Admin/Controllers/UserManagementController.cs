using InClassVoting.Filter;
using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

namespace InClassVoting.Areas.Admin.Controllers
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Admin")]
    public class UserManagementController : Controller
    {
        private DBModel db = new DBModel();

        [HandleError]
        public ActionResult TeacherReport(int? i, string searchText)
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            var teacherList = db.Teachers.ToList();

            Dictionary<int, int> teacherQuestionCountValuePair = new Dictionary<int, int>();
            Dictionary<int, int> teacherQuizCountValuePair = new Dictionary<int, int>();
            Dictionary<int, int> teacherPollCountValuePair = new Dictionary<int, int>();

            /*var quizList = db.Quizs.ToList();
            var chapterList = db.Chapters.ToList();*/
            var pollList = db.Polls.ToList();

            var courseList = db.Courses.ToList();

            foreach (var teacher in teacherList)
            {

                int countQuestion = 0;
                int countQuiz = 0;
                foreach (var course in courseList)
                {
                    if (course.TeacherID == teacher.TID)
                    {
                        foreach (var chapter in course.Chapters)
                        {
                            countQuestion = countQuestion + chapter.Questions.Count + chapter.MatchQuestions.Count;
                        }
                        countQuiz = countQuiz + course.Quizs.Count;
                    }
                    

                }

                teacherQuestionCountValuePair.Add(teacher.TID, countQuestion);
                teacherQuizCountValuePair.Add(teacher.TID, countQuiz);
                int pollCount = pollList.Where(p => p.TeacherID == teacher.TID).Count();
                teacherPollCountValuePair.Add(teacher.TID, pollCount);
            }

            if (searchText != null && !searchText.Trim().Equals(""))
            {
                teacherList = teacherList.Where(t => t.Name.ToLower().Contains(searchText.Trim().ToLower())
                || t.Email.ToLower().Contains(searchText.Trim().ToLower())).ToList();
            }

            /*ViewBag.Quizzes = quizList;
            ViewBag.Chapters = chapterList;
            ViewBag.Polls = db.Polls.ToList();*/

            ViewBag.Quizzes = teacherQuizCountValuePair;
            ViewBag.Questions = teacherQuestionCountValuePair;
            ViewBag.Polls = teacherPollCountValuePair;

            ViewBag.Search = searchText;

            //return page after delete and add
            if (i == null || i < 1)
            {
                i = 1;
            }
            else
            {
                if (teacherList.Count % 10 == 0 && i > teacherList.Count / 10)
                {
                    i = 1;
                }
                else if (teacherList.Count % 10 != 0 && i > ((teacherList.Count / 10) + 1))
                {
                    i = 1;
                }

            }

            ViewBag.TeacherCount = (i - 1) * 10;

            ViewBag.CountTotalTeacher = db.Teachers.Count();
            ViewBag.CountTotalStudent = db.Students.Count();

            return View(teacherList.ToPagedList(i ?? 1, 10));
        }


        [HandleError]
        public ActionResult StudentReport(int? i, string searchText)
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);

            var studentList = db.Students.ToList();

            if (searchText != null && !searchText.Trim().Equals(""))
            {
                studentList = studentList.Where(s => s.Name.ToLower().Contains(searchText.Trim().ToLower())
                || s.Email.ToLower().Contains(searchText.Trim().ToLower())).ToList();
            }

            ViewBag.Search = searchText;

            Dictionary<int, int> studentQuizDoneCountValuePair = new Dictionary<int, int>();
            Dictionary<int, int> studentPollAnswerCountValuePair = new Dictionary<int, int>();

            foreach (var student in studentList)
            {
                int countPollDone = 0;
                int countQuizDone = 0;

                countQuizDone = countQuizDone + student.Student_QuizDone.Count;
                countPollDone = countPollDone + student.Student_PollAnswer.GroupBy(sa=>sa.Poll_Answer.Poll).Count();

                studentQuizDoneCountValuePair.Add(student.SID, countQuizDone);
                studentPollAnswerCountValuePair.Add(student.SID, countPollDone);
                
            }

            //return page after delete and add
            if (i == null || i < 1)
            {
                i = 1;
            }
            else
            {
                if (studentList.Count % 10 == 0 && i > studentList.Count / 10)
                {
                    i = 1;
                }
                else if (studentList.Count % 10 != 0 && i > ((studentList.Count / 10) + 1))
                {
                    i = 1;
                }

            }

            ViewBag.StudentCount = (i - 1) * 10;

            ViewBag.QuizDones = studentQuizDoneCountValuePair;
            ViewBag.PollDones = studentPollAnswerCountValuePair;
            ViewBag.CountTotalTeacher = db.Teachers.Count();
            ViewBag.CountTotalStudent = db.Students.Count();

            return View(studentList.ToPagedList(i ?? 1, 10));

        }
    }
}