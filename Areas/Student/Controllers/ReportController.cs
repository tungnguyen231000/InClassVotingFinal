using InClassVoting.Filter;
using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

namespace InClassVoting.Areas.Student.Controllers
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Student")]
    public class ReportController : Controller
    {
        private DBModel db = new DBModel();
        // GET: Student/Report

        private bool checkCourseIdAvailbile(string cid)
        {
            bool check = true;
            if (cid == null || cid.Equals(""))
            {
                check = false;
            }
            else
            {
                int courseID;
                bool isInt = int.TryParse(cid, out courseID);
                if (courseID != -1)
                {
                    //check if chapter id is int
                    if (isInt == false)
                    {
                        check = false;
                    }
                    else
                    {
                        int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
                        var course = db.Courses.Find(courseID);
                        //check if course exist in db
                        if (course == null)
                        {
                            check = false;
                        }
                        else
                        {
                            var getQuizDone = db.Student_QuizDone.Where(sq => sq.QuizDone.CourseID == courseID && sq.StudentID == studentId).ToList();
                            //check if course belong to student
                            if (getQuizDone.Count == 0)
                            {
                                check = false;
                            }
                        }
                    }
                }
            }
            return (check);
        }

        private bool checkDateAvailbile(string date)
        {
            bool check = true;
            if (date == null || date.Equals(""))
            {
                check = false;
            }
            else
            {
                if (!date.Equals("-1"))
                {
                    DateTime dateSearch;
                    bool isDate = DateTime.TryParse(date, out dateSearch);

                    //check if chapter id is int
                    if (isDate == false)
                    {
                        check = false;
                    }
                    else
                    {
                        int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
                        var getQuizDone = db.Student_QuizDone.Where(sq => sq.QuizDone.CreatedDate == dateSearch && sq.StudentID == studentId).ToList();
                       
                        //check if course belong to student
                        if (getQuizDone.Count == 0)
                        {
                            check = false;
                        }

                    }
                }
            }


            return (check);
        }

        private bool checkQuizDoneAvailble(string qzid)
        {
            bool check = true;
            if (qzid == null || qzid.Equals(""))
            {
                check = false;
            }
            else
            {
                if (!qzid.Equals("-1"))
                {
                    int quizDoneId;
                    bool isInt = int.TryParse(qzid, out quizDoneId);

                    //check if chapter id is int
                    if (isInt == false)
                    {
                        check = false;
                    }
                    else
                    {
                        int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
                        var getQuizDone = db.Student_QuizDone.Where(sq => sq.QuizDoneID == quizDoneId && sq.StudentID == studentId).ToList();

                        //check if course belong to student
                        if (getQuizDone.Count == 0)
                        {
                            check = false;
                        }

                    }
                }
            }


            return (check);
        }

        [HandleError]
        public ActionResult ReportHome(string cid, string date, string searchText, int? i)
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
            var listStudentQuizDone = db.Student_QuizDone.Where(sq => sq.StudentID == studentId && !sq.QuizDone.Quiz.Status.Equals("Doing")).ToList();
            int totalReport = listStudentQuizDone.Count;

            var courseListOfStudent = listStudentQuizDone.Select(sq => sq.QuizDone.Course).ToList();
            var dateListOfQuizDone = listStudentQuizDone.Select(sq => sq.QuizDone.CreatedDate).ToList();

            List<Course> courseList = new List<Course>();
            List<DateTime> dateList = new List<DateTime>();

            //get course list of quiz student have done
            foreach (var course in courseListOfStudent)
            {
                if (!courseList.Contains(course))
                {
                    courseList.Add(course);
                }
            }

            //get date list of quiz student have done
            foreach (var d in dateListOfQuizDone)
            {
                if (!dateList.Contains(d))
                {
                    dateList.Add(d);
                }
            }


            //get course from dropdown list
            if (checkCourseIdAvailbile(cid) == false)
            {
                cid = "-1";
            }

            //get date from dropdown list
            if (checkDateAvailbile(date) == false)
            {
                date = "-1";
            }


            //get list report by search text
            if (searchText != null && !searchText.Equals(""))
            {
                listStudentQuizDone = listStudentQuizDone.Where(sq => sq.QuizDone.Quiz_Name.ToLower().Trim().Contains(searchText.ToLower().Trim())).ToList();
            }

            //get list report by course dropdown 
            if (!cid.Equals("-1"))
            {
                int courseIdSearch = int.Parse(cid);
                listStudentQuizDone = listStudentQuizDone.Where(sq => sq.QuizDone.CourseID == courseIdSearch).ToList();
            }

            //get list report by date dropdown 
            if (!date.Equals("-1"))
            {
                DateTime dateSearch = Convert.ToDateTime(date);
                listStudentQuizDone = listStudentQuizDone.Where(sq => sq.QuizDone.CreatedDate == dateSearch).ToList();
            }




            ViewBag.DateSearch = date.ToString();
            ViewBag.TextSearch = searchText;
            ViewBag.CourseSearch = int.Parse(cid);
            ViewBag.CourseList = courseList;
            ViewBag.DateList = dateList;
            ViewBag.CountReport = totalReport;
            if (i == null || i == 0)
            {
                i = 1;
            }
            else
            {
                if (i % 10 == 0 &&i> listStudentQuizDone.Count/10)
                {
                    i = 1;
                }
                else if (i % 10 != 0 && i > ((listStudentQuizDone.Count / 10)+1))
                {
                    i = 1;
                }

            }
            ViewBag.ReportCount = (i - 1) * 10;

           /* listStudentQuizDone = listStudentQuizDone.OrderByDescending(sq => sq.SQID).ToList();*/

            return View(listStudentQuizDone.ToPagedList(i ?? 1, 10));



        }

        [HandleError]
        public ActionResult QuizReport(string qzid)
        {
            if (checkQuizDoneAvailble(qzid) == false)
            {
                return RedirectToAction("ReportHome");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int quizId = int.Parse(qzid);
                int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);

                var student_quiz = db.Student_QuizDone.Where(sq => sq.StudentID == studentId && sq.QuizDoneID == quizId).OrderByDescending(sq => sq.SQID).FirstOrDefault();
                if (student_quiz.QuizDone.PublicResult == true)
                {
                    double? markPercentage = (student_quiz.StudentMark / student_quiz.TotalMark) * 100;
                    int percentage = Convert.ToInt32(markPercentage);
                    ViewBag.Percentage = percentage;
                }


                List<QuestionDone> multipleQuestionsList = new List<QuestionDone>();
                List<QuestionDone> readingQuestionsList = new List<QuestionDone>();
                List<QuestionDone> fillBlankQuestionsList = new List<QuestionDone>();
                List<QuestionDone> shortAnswerQuestionsList = new List<QuestionDone>();
                List<QuestionDone> indicateMistakeQuestionsList = new List<QuestionDone>();
                List<MatchQuestionDone> matchQuestionsList = new List<MatchQuestionDone>();
                List<Passage_Done> passageList = new List<Passage_Done>();

                List<Student_Answer> student_Answers = new List<Student_Answer>();
                if (student_quiz.QuizDone.PublicAnswer == true)
                {
                    //get student answer
                    student_Answers = db.Student_Answer.Where(sa => sa.QuizDoneID == student_quiz.QuizDoneID&&sa.StudentID==studentId).ToList();
                    string[] questionReceived = student_quiz.ReceivedQuestions.Split(new char[] { ';' });

                   /* Dictionary<int, string> questionSet = new Dictionary<int, string>();
                    Dictionary<int, string> matchingSet = new Dictionary<int, string>();*/


                    //get question that student received
                    foreach (string questions in questionReceived)
                    {
                        string[] questAndType = questions.Split(new char[] { '-' });
                        int qType = int.Parse(questAndType[1]);
                        /*if (qType == 5)
                        {
                            int mID = int.Parse(questAndType[0]);
                            *//*matchingSet.Add(mID, questAndType[1]);*//*
                        }
                        else if (qType == 2)
                        {

                        }
                        else
                        {
                            int qID = int.Parse(questAndType[0]);
                            *//*questionSet.Add(qID, questAndType[1]);*//*
                        }*/
                        if (qType == 5)
                        {
                            int mID = int.Parse(questAndType[0]);
                            /*matchingSet.Add(mID, questAndType[1]);*/
                            var matchQuest = db.MatchQuestionDones.Find(mID);
                            matchQuestionsList.Add(matchQuest);
                        }
                        else if (qType == 2)
                        {
                            int pID = int.Parse(questAndType[0]);
                            var passage = db.Passage_Done.Find(pID);
                            passageList.Add(passage);
                            foreach (var quest in passage.QuestionDones)
                            {
                                readingQuestionsList.Add(quest);
                            }
                        }
                        else
                        {
                            int qID = int.Parse(questAndType[0]);
                            var quest = db.QuestionDones.Find(qID);
                            List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                            if (quest.Qtype == 1)
                            {
                                multipleQuestionsList.Add(quest);
                            }
                            else if (quest.Qtype == 3)
                            {
                                fillBlankQuestionsList.Add(quest);
                            }
                            else if (quest.Qtype == 4)
                            {
                                shortAnswerQuestionsList.Add(quest);
                            }
                            else if (quest.Qtype == 6)
                            {
                                indicateMistakeQuestionsList.Add(quest);
                            }
                        }
                    }
/*
                    foreach (KeyValuePair<int, string> keyValuePair in questionSet)
                    {
                        var quest = db.QuestionDones.Find(keyValuePair.Key);

                        if (quest.Qtype == 1)
                        {
                            multipleQuestionsList.Add(quest);

                        }
                        else if (quest.Qtype == 2)
                        {
                            readingQuestionsList.Add(quest);
                            //add passage to a list
                            var passage = quest.Passage_Done;
                            bool existed = false;
                            foreach (var p in passageList)
                            {
                                if (passage.P_DoneID == p.P_DoneID)
                                {
                                    existed = true;

                                }

                            }
                            if (!existed)
                            {
                                passageList.Add(passage);
                            }
                        }
                        else if (quest.Qtype == 3)
                        {
                            fillBlankQuestionsList.Add(quest);

                        }
                        else if (quest.Qtype == 4)
                        {
                            shortAnswerQuestionsList.Add(quest);

                        }
                        else if (quest.Qtype == 6)
                        {
                            indicateMistakeQuestionsList.Add(quest);
                        }
                    }

                    foreach (KeyValuePair<int, string> keyValuePair in matchingSet)
                    {
                        var matchQuest = db.MatchQuestionDones.Find(keyValuePair.Key);
                        matchQuestionsList.Add(matchQuest);
                    }
*/


                }
                ViewBag.Quiz = db.QuizDones.Find(quizId);
                ViewBag.Student = db.Students.Find(studentId);
                ViewBag.MultipleQuestion = multipleQuestionsList;
                ViewBag.FillBlankQuestion = fillBlankQuestionsList;
                ViewBag.ShortAnswerQuestion = shortAnswerQuestionsList;
                ViewBag.IndicateMistakeQuestion = indicateMistakeQuestionsList;
                ViewBag.ReadingQuestion = readingQuestionsList;
                ViewBag.MatchingQuestion = matchQuestionsList;
                ViewBag.StudentAnswer = student_Answers;
                ViewBag.PassageList = passageList;
                return View();
            }
            
        }
    }
}