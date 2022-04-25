using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.Diagnostics;
using System.Data.Entity;
using InClassVoting.Filter;
using OfficeOpenXml;

namespace InClassVoting.Areas.Teacher.Controllers.ReportController
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]
    public class ReportController : Controller
    {
        private DBModel db = new DBModel();

        public void StudentMarkReport(string qzid)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "Name";
            Sheet.Cells["B1"].Value = "Account";
            Sheet.Cells["C1"].Value = "Student Mark/Total Mark";
            Sheet.Cells["D1"].Value = "Percentage";

            int row = 2;
            int quizDoneID = int.Parse(qzid);
            var report = db.QuizDones.Find(quizDoneID);
            var studentDoneTest = db.Student_QuizDone.Where(stq => stq.QuizDoneID == quizDoneID).ToList();
            string reportName = "ICVS_" + report.Quiz_Name + "_Student_Result.xlsx";
            foreach (Student_QuizDone item in studentDoneTest)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.Student.Name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Student.Email;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.StudentMark.ToString() + "/" + item.TotalMark.ToString();
                int percentage = Convert.ToInt32((item.StudentMark / item.TotalMark) * 100);

                Sheet.Cells[string.Format("D{0}", row)].Value = percentage.ToString() + "%";

                row++;
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", string.Format("attachment: filename={0}", reportName));
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

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

        private bool checkQuizDoneIdAvailbile(string qzid)
        {
            bool check = true;
            int quizDoneId;
            bool isInt = int.TryParse(qzid, out quizDoneId);
            //check if chapter id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var quiz = db.QuizDones.Find(quizDoneId);
                //check if chapter exist in db
                if (quiz == null)
                {
                    check = false;
                }
                else
                {
                    //check if chapter belong to teacher
                    if (quiz.Course.TeacherID != teacherId)
                    {
                        check = false;
                    }
                }
            }
            return (check);
        }

        private bool checkStudentIdAvailbile(string qzid, string stid)
        {
            bool check = true;
            int quizDoneId;
            int studentId;
            bool quizIsInt = int.TryParse(qzid, out quizDoneId);
            bool studentIsInt = int.TryParse(stid, out studentId);
            //check if quiz and student id is int
            if (quizIsInt == false || studentIsInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var student = db.Students.Find(studentId);
                var quiz = db.QuizDones.Find(quizDoneId);
                var student_quiz = db.Student_QuizDone.Where(sq => sq.QuizDoneID == quizDoneId && sq.StudentID == studentId).FirstOrDefault();
                //check if student quizdone exist in db
                if (student_quiz == null || student == null || quiz == null)
                {
                    check = false;
                }
                else
                {
                    //check if chapter belong to teacher
                    if (quiz.Course.TeacherID != teacherId)
                    {
                        check = false;
                    }
                }
            }
            return (check);
        }

        [HandleError]
        public ActionResult ReportHome()
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            return View();

        }

        [HandleError]
        public ActionResult ViewReportListByCourse(string cid, string searchText, int? i)
        {
            //check if course is availble
            if (checkCourserIdAvailbile(cid) == false)
            {
                return RedirectToAction("ReportHome");
            }
            else
            {
                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int courseId = int.Parse(cid);
                var course = db.Courses.Find(courseId);
                DateTime now = DateTime.Now;
                var listQuizDone = db.QuizDones.Where(q => q.CourseID == courseId && q.EndTime < now)/*.OrderByDescending(q => q.QuizDoneID)*/.ToList();

                List<QuizDone> quizzes = new List<QuizDone>();
                if (searchText != null && !searchText.Trim().Equals(""))
                {
                    quizzes = listQuizDone.Where(qz => qz.Quiz_Name.Trim().ToLower().Contains(searchText.Trim().ToLower())).ToList();
                }
                else
                {
                    quizzes = listQuizDone;
                }

                if (i == null || i == 0)
                {
                    i = 1;
                }
                else
                {
                    if (quizzes.Count % 10 == 0 && i > quizzes.Count / 10)
                    {
                        i = 1;
                    }
                    else if (quizzes.Count % 10 != 0 && i > ((quizzes.Count / 10) + 1))
                    {
                        i = 1;
                    }

                }

                /*quizzes = quizzes.Where(qz => qz.Student_QuizDone.Count != 0).ToList();*/
                ViewBag.QuizCount = (i - 1) * 10;
                ViewBag.Course = course;
                ViewBag.Search = searchText;
                ViewBag.CountReport = db.QuizDones.Where(q => q.CourseID == courseId).Count();
                return View(quizzes.ToPagedList(i ?? 1, 10));
            }
        }

        [HandleError]
        public ActionResult ReportByQuestion(string qzid, string searchText, int? i)
        {
            //check if quiz done is availble
            if (checkQuizDoneIdAvailbile(qzid) == false)
            {
                return RedirectToAction("ReportHome");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int quizDoneID = int.Parse(qzid);
                var qzDone = db.QuizDones.Find(quizDoneID);
                var quiz = qzDone.Quiz;

                if (quiz != null)
                {

                    if (quiz.Status.Equals("Doing"))
                    {
                        quiz.Status = "Done";
                        db.Entry(quiz).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                List<QuestionDone> questionsList = new List<QuestionDone>();
                List<Chapter> chapterList = new List<Chapter>();
                List<LearningOutcome> loList = new List<LearningOutcome>();

                //get question
                if (qzDone.Questions != null && !qzDone.Questions.Equals(""))
                {
                    List<string> questSet = qzDone.Questions.Split(new char[] { ';' }).ToList();

                    //get question in report
                    foreach (string questions in questSet)
                    {
                        string[] questAndType = questions.Split(new char[] { '-' });
                        int qType = int.Parse(questAndType[1]);
                        //if question is matching
                        if (qType == 5)
                        {
                            int mID = int.Parse(questAndType[0]);
                            var matchQuest = db.MatchQuestionDones.Find(mID);
                            QuestionDone matching = new QuestionDone();
                            matching.Q_DoneID = mID;
                            matching.Chapter = matchQuest.Chapter;
                            matching.ChapterID = matchQuest.ChapterID;
                            matching.Text = matchQuest.ColumnA + "//" + matchQuest.ColumnB;
                            matching.Mark = matchQuest.Mark;
                            matching.StudentReceive = matchQuest.StudentReceive;
                            matching.CorrectNumber = matchQuest.CorrectNumber;
                            matching.Qtype = 5;
                            questionsList.Add(matching);
                            //get question chapter
                            if (matchQuest.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (matchQuest.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(matchQuest.Chapter);
                                }
                            }
                            //get question lo
                            var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == matchQuest.M_DoneID && ql.Qtype == 5).ToList();
                            foreach (var qdLo in qdLOList)
                            {
                                var lo = qdLo.LearningOutcome;
                                if (!loList.Contains(lo))
                                {
                                    loList.Add(lo);
                                }
                            }
                        }
                        else if (qType == 2)
                        {
                            int pID = int.Parse(questAndType[0]);
                            var passage = db.Passage_Done.Find(pID);
                            //get question chapter
                            if (passage.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (passage.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(passage.Chapter);
                                }

                            }
                            QuestionDone readingQ = new QuestionDone();
                            readingQ.Chapter = passage.Chapter;
                            readingQ.ChapterID = passage.ChapterID;
                            readingQ.Text = passage.Text;
                            double totalMark = 0;
                            //get lo and add mark
                            foreach (var q in passage.QuestionDones)
                            {
                                var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == q.Q_DoneID && ql.Qtype == 2).ToList();
                                foreach (var qdLo in qdLOList)
                                {
                                    var lo = qdLo.LearningOutcome;
                                    if (!loList.Contains(lo))
                                    {
                                        loList.Add(lo);
                                    }
                                }
                                totalMark = totalMark + q.Mark;
                            }
                            readingQ.Mark = totalMark;
                            var firstQuestion = db.QuestionDones.OrderBy(qt => qt.Q_DoneID).Where(qt => qt.PassageID == pID).FirstOrDefault();
                            readingQ.Q_DoneID = firstQuestion.Q_DoneID;
                            readingQ.StudentReceive = firstQuestion.StudentReceive;
                            readingQ.CorrectNumber = firstQuestion.CorrectNumber;
                            readingQ.Qtype = 2;
                            questionsList.Add(readingQ);

                        }
                        else
                        {
                            int qID = int.Parse(questAndType[0]);
                            QuestionDone question = db.QuestionDones.Find(qID);
                            //get question chapter
                            if (question.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (question.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(question.Chapter);
                                }

                            }

                            //get question Lo
                            var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == question.Q_DoneID && ql.Qtype == question.Qtype).ToList();
                            foreach (var qdLo in qdLOList)
                            {
                                var lo = qdLo.LearningOutcome;
                                if (!loList.Contains(lo))
                                {
                                    loList.Add(lo);
                                }
                            }

                            questionsList.Add(question);
                        }
                    }

                    ViewBag.QuestCount = questionsList.Count;

                }
                if (searchText != null && !searchText.Trim().Equals(""))
                {
                    questionsList = questionsList.Where(ql => ql.Text.Trim().ToLower().Contains(searchText.Trim().ToLower())).ToList();
                }

                var studentDoneTest = db.Student_QuizDone.Where(stq => stq.QuizDoneID == quizDoneID).ToList();

                ViewBag.Quiz = qzDone;
                ViewBag.QuestionList = questionsList;
                ViewBag.StudentCount = studentDoneTest.Count;
                ViewBag.Search = searchText;
                ViewBag.ChapterList = chapterList.OrderBy(c => c.ChID);
                ViewBag.CountChapter = chapterList.Count;
                ViewBag.LOList = loList.OrderBy(lo => lo.LOID);
                ViewBag.CountLO = loList.Count;
                ViewBag.QuestionLoList = db.QuestionDoneLOes.Where(ql => ql.LearningOutcome.CourseID == qzDone.CourseID).ToList();
                if (i == null || i == 0)
                {
                    i = 1;
                }

                ViewBag.QuestNo = (i - 1) * 10;
                return View(questionsList.ToPagedList(i ?? 1, 10));
            }
        }

        [HandleError]
        public ActionResult ReportByStudent(string qzid, string searchText, int? i)
        {
            //check if quiz done is availble
            if (checkQuizDoneIdAvailbile(qzid) == false)
            {
                return RedirectToAction("ReportHome");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int quizDoneID = int.Parse(qzid);
                var qzDone = db.QuizDones.Find(quizDoneID);
                var quiz = qzDone.Quiz;
                if (quiz != null)
                {

                    if (quiz.Status.Equals("Doing"))
                    {
                        quiz.Status = "Done";
                        db.Entry(quiz).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                var studentDoneTest = db.Student_QuizDone.Where(stq => stq.QuizDoneID == quizDoneID).ToList();

                ViewBag.StudentCount = studentDoneTest.Count;

                if (searchText != null && !searchText.Trim().Equals(""))
                {
                    studentDoneTest = studentDoneTest.Where(st => st.Student.Name.Trim().ToLower().Contains(searchText.Trim().ToLower())).ToList();
                }

                List<Chapter> chapterList = new List<Chapter>();
                List<LearningOutcome> loList = new List<LearningOutcome>();

                //get question
                if (qzDone.Questions != null && !qzDone.Questions.Equals(""))
                {
                    List<string> questSet = qzDone.Questions.Split(new char[] { ';' }).ToList();

                    //get question in report
                    foreach (string questions in questSet)
                    {
                        string[] questAndType = questions.Split(new char[] { '-' });
                        int qType = int.Parse(questAndType[1]);
                        //if question is matching
                        if (qType == 5)
                        {
                            int mID = int.Parse(questAndType[0]);
                            var matchQuest = db.MatchQuestionDones.Find(mID);
                            if (matchQuest.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (matchQuest.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(matchQuest.Chapter);
                                }
                            }
                            var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == matchQuest.M_DoneID && ql.Qtype == 5).ToList();
                            foreach (var qdLo in qdLOList)
                            {
                                var lo = qdLo.LearningOutcome;
                                if (!loList.Contains(lo))
                                {
                                    loList.Add(lo);
                                }
                            }
                        }
                        else if (qType == 2)
                        {
                            int pID = int.Parse(questAndType[0]);
                            var passage = db.Passage_Done.Find(pID);
                            //get question chapter
                            if (passage.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (passage.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(passage.Chapter);
                                }

                            }
                            //get question learning outcome
                            foreach (var q in passage.QuestionDones)
                            {
                                var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == q.Q_DoneID && ql.Qtype == 2).ToList();
                                foreach (var qdLo in qdLOList)
                                {
                                    var lo = qdLo.LearningOutcome;
                                    if (!loList.Contains(lo))
                                    {
                                        loList.Add(lo);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int qID = int.Parse(questAndType[0]);
                            QuestionDone question = db.QuestionDones.Find(qID);
                            if (question.Chapter != null)
                            {
                                bool isExisted = false;
                                foreach (Chapter c in chapterList)
                                {
                                    if (question.ChapterID == c.ChID)
                                    {
                                        isExisted = true;
                                    }
                                }
                                if (isExisted == false)
                                {
                                    chapterList.Add(question.Chapter);
                                }

                            }
                            var qdLOList = db.QuestionDoneLOes.Where(ql => ql.QuestionDoneID == question.Q_DoneID && ql.Qtype == question.Qtype).ToList();
                            foreach (var qdLo in qdLOList)
                            {
                                var lo = qdLo.LearningOutcome;
                                if (!loList.Contains(lo))
                                {
                                    loList.Add(lo);
                                }
                            }

                        }
                    }
                }

                ViewBag.Quiz = qzDone;
                ViewBag.QuestCount = qzDone.NumOfQuestion;
                /*ViewBag.StudenDone = studentDoneTest;*/
                ViewBag.Search = searchText;
                ViewBag.ChapterList = chapterList.OrderBy(c => c.ChID); ViewBag.CountChapter = chapterList.Count;
                ViewBag.LOList = loList.OrderBy(lo => lo.LOID);
                ViewBag.CountLO = loList.Count;
                if (i == null || i == 0)
                {
                    i = 1;
                }

                ViewBag.StudentNo = (i - 1) * 10;
                return View(studentDoneTest.ToPagedList(i ?? 1, 10));
            }
        }


        [HandleError]
        public ActionResult ReportStudentQuiz(string qzid, string stid)
        {
            //check if quiz done is availble
            if (checkQuizDoneIdAvailbile(qzid) == false || checkStudentIdAvailbile(qzid, stid) == false)
            {
                return RedirectToAction("ReportHome");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                if (stid == null)
                {
                    stid = "1";
                }

                int quizId = int.Parse(qzid);
                int studentId = int.Parse(stid);
                var student_quiz = db.Student_QuizDone.Where(sq => sq.StudentID == studentId && sq.QuizDoneID == quizId).OrderByDescending(sq => sq.SQID).FirstOrDefault();


                string[] questionReceived = student_quiz.ReceivedQuestions.Split(new char[] { ';' });

                //get student answer
                var student_Answers = db.Student_Answer.Where(sa => sa.QuizDoneID == student_quiz.QuizDoneID && sa.StudentID == studentId).ToList();
                /*

                                Dictionary<int, string> questionSet = new Dictionary<int, string>();
                                Dictionary<int, string> matchingSet = new Dictionary<int, string>();*/
                List<QuestionDone> multipleQuestionsList = new List<QuestionDone>();
                List<QuestionDone> readingQuestionsList = new List<QuestionDone>();
                List<QuestionDone> fillBlankQuestionsList = new List<QuestionDone>();
                List<QuestionDone> shortAnswerQuestionsList = new List<QuestionDone>();
                List<QuestionDone> indicateMistakeQuestionsList = new List<QuestionDone>();
                List<MatchQuestionDone> matchQuestionsList = new List<MatchQuestionDone>();
                List<Passage_Done> passageList = new List<Passage_Done>();

                //get question that student received
                foreach (string questions in questionReceived)
                {
                    string[] questAndType = questions.Split(new char[] { '-' });
                    int qType = int.Parse(questAndType[1]);
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

                /*foreach (KeyValuePair<int, string> keyValuePair in questionSet)
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
                }*/

                double? markPercentage = (student_quiz.StudentMark / student_quiz.TotalMark) * 100;

                int percentage = Convert.ToInt32(markPercentage);

                ViewBag.MultipleQuestion = multipleQuestionsList;
                ViewBag.FillBlankQuestion = fillBlankQuestionsList;
                ViewBag.ShortAnswerQuestion = shortAnswerQuestionsList;
                ViewBag.IndicateMistakeQuestion = indicateMistakeQuestionsList;
                ViewBag.ReadingQuestion = readingQuestionsList;
                ViewBag.PassageList = passageList;
                ViewBag.MatchingQuestion = matchQuestionsList;
                ViewBag.Percentage = percentage;
                ViewBag.StudentAnswer = student_Answers;
                ViewBag.Quiz = db.QuizDones.Find(quizId);
                ViewBag.Student = db.Students.Find(studentId);

                return View();
            }
        }

        [HandleError]
        [HttpPost]
        public ActionResult SaveReportOption(string qzID, string currentPage, string searchText, string cbPublishMark, string cbPublishAnswer)
        {
            int quizDoneID = int.Parse(qzID);
            var qzDone = db.QuizDones.Find(quizDoneID);

            if (cbPublishMark != null)
            {
                qzDone.PublicResult = true;
            }
            else
            {
                qzDone.PublicResult = false;
            }

            if (cbPublishAnswer != null)
            {
                qzDone.PublicAnswer = true;
            }
            else
            {
                qzDone.PublicAnswer = false;
            }

            db.Entry(qzDone).State = EntityState.Modified;
            db.SaveChanges();

            int page = int.Parse(currentPage);
            if (page == 1)
            {
                return Redirect("~/Teacher/Report/ReportByQuestion?qzid=" + quizDoneID + "&searchText=" + searchText);
            }
            else
            {
                return Redirect("~/Teacher/Report/ReportByStudent?qzid=" + quizDoneID + "&searchText=" + searchText);
            }
        }

        [HandleError]
        public ActionResult ReportPollList(string searchText, int? i)
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);

            var listPoll = db.Polls.Where(p => p.TeacherID == teacherId &&
            p.TotalParticipian != 0 || p.TeacherID == teacherId && p.IsDoing == true).OrderByDescending(p => p.PollID).ToList();

            List<Poll> polls = new List<Poll>();
            if (searchText != null && !searchText.Trim().Equals(""))
            {
                polls = listPoll.Where(p => p.PollName.Trim().ToLower().Contains(searchText.Trim().ToLower())).ToList();
            }
            else
            {
                polls = listPoll;
            }

            if (i == null || i == 0)
            {
                i = 1;
            }
            else
            {
                if (polls.Count % 10 == 0 && i > polls.Count / 10)
                {
                    i = 1;
                }
                else if (polls.Count % 10 != 0 && i > ((polls.Count / 10) + 1))
                {
                    i = 1;
                }

            }

            ViewBag.PollCount = (i - 1) * 10;
            ViewBag.Search = searchText;
            ViewBag.CountPoll = listPoll.Count;
            return View(polls.ToPagedList(i ?? 1, 10));
        }

        [HandleError]
        [HttpPost]
        public ActionResult DeleteReport(string qzid)
        {
            int quizDoneID = int.Parse(qzid);
            var quizDone = db.QuizDones.Find(quizDoneID);
            int courseId = quizDone.CourseID;

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



            var studentAnswers = db.Student_Answer.Where(sa => sa.QuizDoneID == quizDoneID).ToList();
            //delete student answer
            foreach (var studentAnswer in studentAnswers)
            {
                db.Student_Answer.Remove(studentAnswer);
            }

            var studenWorks = db.Student_QuizDone.Where(sq => sq.QuizDoneID == quizDoneID).ToList();
            //delete student report
            foreach (var studentWork in studenWorks)
            {
                db.Student_QuizDone.Remove(studentWork);
            }

            db.QuizDones.Remove(quizDone);
            db.SaveChanges();

            return Redirect("~/Teacher/Report/ViewReportListByCourse?cid=" + courseId);
        }

        [HandleError]
        [HttpPost]
        public ActionResult EditReportName(string qzid, string newReportName)
        {
            int quizDoneID = int.Parse(qzid);
            var quizDone = db.QuizDones.Find(quizDoneID);

            quizDone.Quiz_Name = newReportName.Trim();
            db.Entry(quizDone).State = EntityState.Modified;
            db.SaveChanges();

            string previousPage = "~/Teacher/Report/ReportByStudent?qzid=" + quizDoneID;
            if (Request.UrlReferrer != null)
            {
                previousPage = Request.UrlReferrer.ToString();
            }

            return Redirect(previousPage);

        }

        //Check New Quiz Name
        [HttpPost]
        public JsonResult CheckReportName(string text)
        {
            string dataInput = text;
            string check = "";
            string message = "";


            if (dataInput.Trim().Equals("") || dataInput == null)
            {
                check = "0";
                message = "Please enter report name!";
                Debug.WriteLine("huhu111");

            }
            else if (dataInput.Trim().Length > 100)
            {
                check = "0";
                message = "You can only enter 100 characters max!";
                Debug.WriteLine("huhu");
            }
            else
            {
                check = "1";
                message = "";
                Debug.WriteLine("huhu2222");
            }
            Debug.WriteLine(check + "==-=-" + message);



            return Json(new { mess = message, check = check });

        }
    }
}