using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using InClassVoting.Filter;

namespace InClassVoting.Areas.teacher.Controllers.QuestionBankController
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]
    public class QuestionController : Controller
    {
        private DBModel db = new DBModel();
        // GET: teacher/Question
        public ActionResult QuestionBank()
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            Session["SelectedCourse"] = null;
            Session["SelectedChapter"] = null;
            return View();
        }

        private bool checkChapterIdAvailbile(string chid)
        {
            bool check = true;
            int chapID;
            bool isInt = int.TryParse(chid, out chapID);
            //check if chapter id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                var chapter = db.Chapters.Find(chapID);
                //check if chapter exist in db
                if (chapter == null)
                {
                    check = false;
                }
                else
                {
                    //check if chapter belong to teacher
                    if (chapter.Course.TeacherID != teacherId)
                    {
                        check = false;
                    }
                }
            }
            return (check);
        }

        private bool checkQuestionIdAvailble(string qid, int qType)
        {
            bool check = true;
            int questId;
            bool isInt = int.TryParse(qid, out questId);
            //check if question id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                int teacherId = Convert.ToInt32(HttpContext.Session["TeacherId"]);
                //if question is matching
                if (qType == 5)
                {
                    var question = db.MatchQuestions.Find(questId);
                    //check if question exist in db
                    if (question == null)
                    {
                        check = false;
                    }
                    else
                    {
                        //if question belong to teacher
                        if (question.Chapter.Course.TeacherID != teacherId)
                        {
                            check = false;
                        }
                    }

                }
                else
                {
                    var question = db.Questions.Find(questId);
                    if (question == null)
                    {
                        check = false;
                    }
                    else
                    {
                        //if question belong to teacher
                        if (question.Chapter.Course.TeacherID != teacherId)
                        {
                            check = false;
                        }
                        else
                        {
                            //if question type is wrong
                            if (question.Qtype != qType)
                            {
                                check = false;
                            }
                        }
                    }
                }


            }
            return (check);
        }

        private bool checkQuestionTypeIdAvailbile(string qtype)
        {
            bool check = true;
            int qTypeId;
            bool isInt = int.TryParse(qtype, out qTypeId);
            //check if question type id is int
            if (isInt == false)
            {
                check = false;
            }
            else if (qTypeId == -1)
            {
                check = true;
            }
            else
            {
                var questionType = db.QuestionTypes.Find(qTypeId);
                //check if question type exist in db
                if (questionType == null)
                {
                    check = false;
                }
            }
            return (check);
        }



        //Get question list by chapter (Question Bank)
        [HandleError]
        public ActionResult ViewQuestionByChapter(string chid, string qtype, string searchText, int? i)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {
                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);

                int chapID = int.Parse(chid);
                var chapter = db.Chapters.Find(chapID);
                var questions = db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).ToList();
                var matchings = db.MatchQuestions.Where(m => m.ChapterId == chapID).ToList();
                var passages = db.Passages.Where(p => p.ChapterID == chapID).ToList();
                List<Question> qList = new List<Question>();
                List<MatchQuestion> mList = new List<MatchQuestion>();
                List<Passage> pList = new List<Passage>();


                if (checkQuestionTypeIdAvailbile(qtype) == false)
                {
                    qtype = "-1";
                }
                //get question type by dropdown list
                int qTypeID = int.Parse(qtype);
                if (qTypeID == -1)
                {
                    mList = matchings;
                    qList = questions.ToList();
                    pList = passages;
                }
                else
                {
                    if (qTypeID == 5)
                    {
                        mList = matchings;
                    }
                    else if (qTypeID == 2)
                    {
                        pList = passages;
                    }
                    else
                    {
                        qList = questions.Where(q => q.Qtype == qTypeID).ToList();
                    }

                }

                //add matching question as question
                foreach (var m in mList)
                {
                    Question q = new Question();
                    q.Text = m.ColumnA + "//" + m.ColumnB;
                    q.Qtype = 5;
                    q.Mark = m.Mark;
                    q.QID = m.MID;
                    q.CreatedDate = m.CreatedDate;
                    qList.Add(q);
                }

                //add passage question as question
                foreach (var p in pList)
                {
                    Question q = new Question();
                    q.Text = p.Text;
                    q.Qtype = 2;
                    double totalReadingMark = 0;
                    foreach (var questionInside in p.Questions)
                    {
                        totalReadingMark = totalReadingMark + questionInside.Mark;

                    }
                    q.PassageID = p.PID;
                    q.Mark = totalReadingMark;
                    q.QID = p.Questions.OrderBy(qt => qt.CreatedDate).Select(qt => qt.QID).FirstOrDefault();
                    q.ImageData = p.PassageImage;
                    q.CreatedDate = p.Questions.OrderBy(qt => qt.CreatedDate).Select(qt => qt.CreatedDate).FirstOrDefault();
                    qList.Add(q);
                }


                // check if user search by question text
                if (searchText != null && !searchText.Equals(""))
                {
                    if (qList != null)
                    {
                        qList = qList.Where(q => q.Text.ToLower().Contains(searchText.ToLower().Trim())).ToList();
                    }
                    ViewBag.Search = searchText;

                }
                else
                {
                    ViewBag.Search = "";
                }




                //get question type

                if (qtype == null)
                {
                    ViewBag.QType = "";
                }
                else
                {
                    ViewBag.QType = qtype;
                }

                ViewBag.CountQuest = db.Questions.Count(q => q.ChapterID == chapID) + db.MatchQuestions.Count(m => m.ChapterId == chapID); ;
                ViewBag.CourseName = chapter.Course.Name;
                ViewBag.LoList = db.QuestionLOes.Where(ql => ql.LearningOutcome.CourseID == chapter.CourseID).ToList();
                ViewBag.Chapter = chapter;
                ViewBag.QuestionType = db.QuestionTypes.ToList();


                //return page after delete and add
                if (i == null || i < 1)
                {
                    i = 1;
                }
                else
                {
                    if (qList.Count % 10 == 0 && i > qList.Count / 10)
                    {
                        i = 1;
                    }
                    else if (qList.Count % 10 != 0 && i > ((qList.Count / 10) + 1))
                    {
                        i = 1;
                    }

                }

                ViewBag.QuestCount = (i - 1) * 10;
                ViewBag.Page = i;
                qList = qList.OrderBy(q => q.CreatedDate).ToList();
                if (qList.Count == 0)
                {
                    i = null;
                }
                Session["SelectedCourse"] = chapter.Course.CID;
                Session["SelectedChapter"] = chapter.ChID;
                return View(qList.ToPagedList(i ?? 1, 10));

            }

        }

        //delete question
        [HandleError]
        [HttpPost]
        public ActionResult DeleteQuestion(string chapterId, string qtype, string searchText, FormCollection collection, int? page)
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int chapID = int.Parse(chapterId);
            var questions = collection["questionIdAndType"];
            string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + chapterId;
            if (Request.UrlReferrer != null  )
            {
                /*previousPage = Request.UrlReferrer.ToString();*/
            }

            //check if question id and matching question id list is null
            if (questions == null)
            {
                return Redirect(previousPage);
            }
            else
            {
                //get question set List
                if (questions != null)
                {
                    string[] qSet = questions.Split(new char[] { ',' });

                    foreach (string set in qSet)
                    {
                        string qtypeCheck = set.Substring(set.Length - 1, 1);
                        //if it is matching question
                        if (qtypeCheck.Equals("5"))
                        {
                            int mid = int.Parse(set.Substring(0, set.Length - 2));
                            var matchQuest = db.MatchQuestions.Find(mid);
                            db.MatchQuestions.Remove(matchQuest);
                            var questionLO = db.QuestionLOes.Where(ql => ql.QuestionID == mid && ql.Qtype == 5).ToList();
                            //deleteLO
                            foreach (var qlo in questionLO)
                            {
                                db.QuestionLOes.Remove(qlo);
                            }

                        }
                        else if (qtypeCheck.Equals("2"))
                        {
                            int pid = int.Parse(set.Substring(0, set.Length - 2));
                            Passage passage = db.Passages.Find(pid);
                            List<Question> deleteQuestionList = new List<Question>();
                            foreach (var q in passage.Questions)
                            {
                                var qaList = db.QuestionAnswers.Where(qa => qa.QuestionID == q.QID).ToList();
                                //remove answer of question
                                foreach (QuestionAnswer qa in qaList)
                                {
                                    db.QuestionAnswers.Remove(qa);
                                }

                                var questionLO = db.QuestionLOes.Where(ql => ql.QuestionID == q.QID && ql.Qtype == q.Qtype).ToList();
                                //deleteLO
                                foreach (QuestionLO qlo in questionLO)
                                {
                                    db.QuestionLOes.Remove(qlo);
                                }
                                deleteQuestionList.Add(q);

                            }

                            foreach (Question q in deleteQuestionList)
                            {
                                db.Questions.Remove(q);
                            }
                            db.Passages.Remove(passage);
                        }
                        else
                        {
                            int qid = int.Parse(set.Substring(0, set.Length - 2));
                            var question = db.Questions.Find(qid);
                            var qaList = db.QuestionAnswers.Where(qa => qa.QuestionID == qid).ToList();
                            //remove answer of question
                            foreach (QuestionAnswer qa in qaList)
                            {
                                db.QuestionAnswers.Remove(qa);
                            }

                            var questionLO = db.QuestionLOes.Where(ql => ql.QuestionID == qid && ql.Qtype == question.Qtype).ToList();
                            //deleteLO
                            foreach (var qlo in questionLO)
                            {
                                db.QuestionLOes.Remove(qlo);
                            }

                            //delete image
                            db.Questions.Remove(question);
                        }
                        db.SaveChanges();

                        var quizContainQuests = db.Quizs.Where(qz => qz.Questions.Contains(set));
                        //delete question inside quest
                        foreach (var quiz in quizContainQuests)
                        {
                            string newSet = "";
                            string[] questSet = quiz.Questions.Split(new char[] { ';' });
                            foreach (string qIdAndType in questSet)
                            {
                                if (!qIdAndType.Equals(set))
                                {
                                    newSet = newSet + qIdAndType + ";";
                                }
                            }
                            if (newSet != "")
                            {
                                quiz.Questions = newSet.Substring(0, newSet.Length - 1);
                            }
                            else
                            {
                                quiz.Questions = null;
                            }
                            db.Entry(quiz).State = EntityState.Modified;
                            updateQuizTimeAndMark(quiz);
                        }
                    }


                    db.SaveChanges();
                }
            }

            int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID) + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
            int lastPage = 0;
            if (getLastPage % 10 == 0)
            {
                lastPage = getLastPage / 10;
            }
            else
            {
                lastPage = (getLastPage / 10) + 1;
            }

            if (page > lastPage)
            {
                page = lastPage;
            }

            return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&searchText=" + searchText + "&qtype=" + qtype + "&i=" + page);
        }


        //edit question
        [HandleError]
        public ActionResult EditQuestion(string qid, string qtype)
        {

            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            int questID = int.Parse(qid);
            int qTypeID = int.Parse(qtype);
            /*var question = db.Questions.Find(questID);*/
            if (qTypeID == 1)
            {
                return Redirect("~/Teacher/Question/EditMultipleChoiceQuestion?qid=" + questID);
            }
            else if (qTypeID == 2)
            {
                return Redirect("~/Teacher/Question/EditReadingQuestion?qid=" + questID);
            }
            else if (qTypeID == 3)
            {
                return Redirect("~/Teacher/Question/EditFillBlankQuestion?qid=" + questID);
            }
            else if (qTypeID == 4)
            {
                return Redirect("~/Teacher/Question/EditShortAnswerQuestion?qid=" + questID);
            }
            else if (qTypeID == 5)
            {
                return Redirect("~/Teacher/Question/EditMatchingQuestion?qid=" + questID);
            }
            else
            {
                return Redirect("~/Teacher/Question/EditIndicateMistakeQuestion?qid=" + questID);
            }


        }

        //show page to create multiple choice question
        [HandleError]
        public ActionResult CreateMultipleChoiceQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //add new multiple choice question
        [HandleError]
        [HttpPost]
        public ActionResult CreateMultipleChoiceQuestion(string chid, string questionText, FormCollection collection, string mark, string time,
            HttpPostedFileBase imgfile)
        {
            try
            {
                int chapID = int.Parse(chid);
                Question question = new Question();
                Chapter chapter = db.Chapters.Find(chapID);
                question.ChapterID = chapter.ChID;
                question.Text = questionText.Trim();
                question.CreatedDate = DateTime.Now;
                question.Qtype = 1;
                //check if mark is null
                if (!mark.Trim().Equals(""))
                {
                    question.Mark = float.Parse(mark);
                }
                //check if time is null
                if (!time.Trim().Equals(""))
                {
                    question.Time = int.Parse(time);
                }

                //add image
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imgfile.FileName);
                    var newPath = Server.MapPath("~/Images");
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }
                    else
                    {
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }
                    question.ImageData = new byte[imgfile.ContentLength];
                    imgfile.InputStream.Read(question.ImageData, 0, imgfile.ContentLength);
                }


                string mixChoice = collection["mixChoice"];
                //get mix choice check box value if 1 is true, 0 is false
                if (mixChoice == null)
                {
                    question.MixChoice = false;

                }
                else
                {
                    question.MixChoice = true;

                }
                db.Questions.Add(question);
                db.SaveChanges();

                int qid = int.Parse(db.Questions.OrderByDescending(q => q.QID).Select(q => q.QID).First().ToString());

                /*string[] options = collection["option"].Split(new char[] { ',' });*/
                string[] options = Request.Form.GetValues("option");
                string cbOption = collection["cbOption"];


                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i] != null && !options[i].Trim().Equals(""))
                    {
                        QuestionAnswer qa = new QuestionAnswer();
                        qa.QuestionID = qid;
                        qa.Text = options[i].Trim();
                        int answerIndex = i + 1;
                        if (cbOption.Contains(answerIndex.ToString()))
                        {
                            qa.IsCorrect = true;
                        }
                        else
                        {
                            qa.IsCorrect = false;
                        }
                        db.QuestionAnswers.Add(qa);
                    }
                }


                //get lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = qid;
                            qLO.Qtype = 1;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }
                db.SaveChanges();

                int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                    + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
                int lastPage = 0;
                if (getLastPage % 10 == 0)
                {
                    lastPage = getLastPage / 10;
                }
                else
                {
                    lastPage = (getLastPage / 10) + 1;
                }


                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);
            }
            catch
            {
                return Redirect("~/Error");
            }
        }

        //show page to edit multiple choice question
        [HandleError]
        public ActionResult EditMultipleChoiceQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 1) == false)
            {

                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int questionID = int.Parse(qid);
                Question question = db.Questions.Find(questionID);
                ViewBag.ChapterID = question.ChapterID;
                ViewBag.Question = question;
                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + question.ChapterID;
                if (Request.UrlReferrer!= null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }

                ViewBag.Previous = previousPage;

                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == question.QID && ql.Qtype == 1).ToList();
                Chapter chapter = db.Chapters.Find(question.ChapterID);
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //save multiple choice question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditMultipleChoiceQuestion(string previousUrl, string qid, string chid, string questionText, FormCollection collection, string mark, string time,
            HttpPostedFileBase imgfile)
        {
            try
            {
                int questionID = int.Parse(qid);
                int chapID = int.Parse(chid);
                Question question = db.Questions.Find(questionID);
                question.Text = questionText.Trim();
                question.Mark = float.Parse(mark);
                question.Time = int.Parse(time);
                string mixChoice = collection["mixChoice"];
                //get mix choice check box value if 1 is true, 0 is false
                if (mixChoice == null)
                {
                    question.MixChoice = false;
                }
                else
                {
                    question.MixChoice = true;

                }

                string imageExisted = null;
                if (collection["imageExisted"] != null)
                {
                    imageExisted = collection["imageExisted"];
                }
                if (imageExisted != null)
                {
                    if (imageExisted.Equals("1"))
                    {
                        //change image
                        if (imgfile != null && imgfile.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(imgfile.FileName);
                            var newPath = Server.MapPath("~/Images");
                            if (!Directory.Exists(newPath))
                            {
                                Directory.CreateDirectory(newPath);
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            else
                            {
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            question.ImageData = new byte[imgfile.ContentLength];
                            imgfile.InputStream.Read(question.ImageData, 0, imgfile.ContentLength);
                        }
                        else
                        {
                            //delete image
                            /* var base64 = Convert.ToBase64String(question.ImageData);
                             string fullPath = Request.MapPath(base64);
                             if (System.IO.File.Exists(fullPath))
                             {
                                 System.IO.File.Delete(fullPath);
                             }*/

                            /* question.ImageData = null;*/
                        }
                    }
                    else
                    {
                        question.ImageData = null;
                    }
                }
                else
                {
                    question.ImageData = null;
                }
                db.Entry(question).State = EntityState.Modified;

                var answerList = db.QuestionAnswers.Where(qa => qa.QuestionID == questionID);
                //delete the old answer
                foreach (var a in answerList)
                {
                    db.QuestionAnswers.Remove(a);
                }

                string questAndType = question.QID + "-" + question.Qtype;
                var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();

                foreach (var quiz in quizContainQuestions)
                {
                    updateQuizTimeAndMark(quiz);
                }

                var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == questionID && lo.Qtype == 1);
                //delete the old lo
                foreach (var lo in oldLoList)
                {
                    db.QuestionLOes.Remove(lo);
                }

                //get new lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = questionID;
                            qLO.Qtype = 1;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }

                db.SaveChanges();

                /*string[] options = collection["option"].Split(new char[] { ',' });*/
                string[] options = Request.Form.GetValues("option");
                string[] cbOption = collection["cbOption"].Split(new char[] { ',' });

                /*List<string> oppttt = collection["option"].ToList();*/

                string[] Match = Request.Form.GetValues("option");

                //new answer
                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i] != null && !options[i].Trim().Equals(""))
                    {
                        QuestionAnswer qa = new QuestionAnswer();
                        qa.QuestionID = questionID;
                        qa.Text = options[i].Trim();
                        int answerIndex = i + 1;
                        if (cbOption.Contains(answerIndex.ToString()))
                        {
                            qa.IsCorrect = true;
                        }
                        else
                        {
                            qa.IsCorrect = false;
                        }
                        db.QuestionAnswers.Add(qa);
                    }

                }
                db.SaveChanges();
                return Redirect(previousUrl);
            }
            catch
            {
                return Redirect("~/Error/NotFound");
            }
        }

        //show page to create reading question
        [HandleError]
        public ActionResult CreateReadingQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }



        //add new reading question
        [HandleError]
        [HttpPost]
        public ActionResult CreateReadingQuestion(string chid, FormCollection collection, string paragraph,
            HttpPostedFileBase imgfile)
        {
            try
            {
                int chapID = int.Parse(chid);
                Passage passage = new Passage();
                passage.Text = paragraph.Trim();
                passage.ChapterID = chapID;
                db.Passages.Add(passage);
                //add image
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imgfile.FileName);
                    var newPath = Server.MapPath("~/Images");
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }
                    else
                    {
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }


                    passage.PassageImage = new byte[imgfile.ContentLength];
                    imgfile.InputStream.Read(passage.PassageImage, 0, imgfile.ContentLength);
                }

                db.SaveChanges();
                int pid = int.Parse(db.Passages.OrderByDescending(p => p.PID).Where(p => p.ChapterID == chapID).Select(p => p.PID).First().ToString());


                /*string[] questionList = collection["question"].Split(new char[] { ',' });*/
                /*List<string> options = collection["option"].Split(new char[] { ',' }).ToList();*/
                string[] questionList = Request.Form.GetValues("question");
                List<string> options = Request.Form.GetValues("option").ToList();

                List<string> cbOption = collection["cboption"].Split(new char[] { ',' }).ToList();
                string[] markList = collection["mark"].Split(new char[] { ',' });
                string[] timeList = collection["time"].Split(new char[] { ',' });
                List<string> mixChoiceList = collection["mixChoice"].Split(new char[] { ',' }).ToList(); ;


                int countAnswer = 0;

                //get each question and answer
                for (int i = 0; i < questionList.Length; i++)
                {
                    Question question = new Question();
                    Chapter chapter = db.Chapters.Find(chapID);
                    question.ChapterID = chapter.ChID;
                    question.PassageID = pid;
                    question.Text = questionList[i].Trim();
                    question.Qtype = 2;
                    question.CreatedDate = DateTime.Now;
                    question.Mark = float.Parse(markList[i]);
                    question.Time = int.Parse(timeList[i]);
                    //get mixchoice checkbox
                    if (mixChoiceList[0].Contains("1"))
                    {
                        question.MixChoice = true;
                        mixChoiceList.RemoveAt(0);
                        mixChoiceList.RemoveAt(0);
                    }
                    else
                    {
                        question.MixChoice = false;
                        mixChoiceList.RemoveAt(0);
                    }

                    db.Questions.Add(question);
                    db.SaveChanges();

                    int qid = int.Parse(db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).First().ToString());

                    int countIndex = 0;

                    for (int j = countAnswer; j < (i + 1) * 6; j++)
                    {
                        countIndex++;
                        if (options[j] != null && !options[j].Trim().Equals(""))
                        {
                            QuestionAnswer qa = new QuestionAnswer();
                            qa.QuestionID = qid;
                            qa.Text = options[j].Trim();
                            int answerIndex = j + 1;
                            if (cbOption[0].Contains("1"))
                            {
                                qa.IsCorrect = true;
                                cbOption.Remove(cbOption[0]);
                                cbOption.Remove(cbOption[0]);
                            }
                            else
                            {
                                qa.IsCorrect = false;
                                cbOption.Remove(cbOption[0]);
                            }

                            db.QuestionAnswers.Add(qa);
                        }
                        else
                        {
                            cbOption.Remove(cbOption[0]);
                        }

                    }
                    countAnswer = countAnswer + 6;
                    //get lo
                    if (collection["lo"] != null)
                    {
                        string[] loList = collection["lo"].Split(new char[] { ',' });
                        foreach (string loId in loList)
                        {
                            var lo = db.LearningOutcomes.Find(int.Parse(loId));
                            if (lo != null)
                            {
                                QuestionLO qLO = new QuestionLO();
                                qLO.QuestionID = qid;
                                qLO.Qtype = 2;
                                qLO.LearningOutcomeID = lo.LOID;
                                db.QuestionLOes.Add(qLO);
                            }
                        }
                    }
                }
                db.SaveChanges();


                int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                    + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
                int lastPage = 0;
                if (getLastPage % 10 == 0)
                {
                    lastPage = getLastPage / 10;
                }
                else
                {
                    lastPage = (getLastPage / 10) + 1;
                }

                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);
            }
            catch
            { return Redirect("~/Error/NotFound"); }

        }


        //show page to edit reading question
        [HandleError]
        public ActionResult EditReadingQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 2) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);

                int questionID = int.Parse(qid);
                Question question = db.Questions.Find(questionID);
                ViewBag.ChapterID = question.ChapterID;
                ViewBag.QuestionList = db.Questions.Where(q => q.PassageID == question.PassageID).ToList();
                ViewBag.Passage = question.Passage;
                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + question.ChapterID;
                if (Request.UrlReferrer != null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }
                ViewBag.Previous = previousPage;
                Chapter chapter = db.Chapters.Find(question.ChapterID);
                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == questionID && ql.Qtype == 2).ToList();
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                var idList = db.Questions.Where(q => q.PassageID == question.PassageID).Select(q => q.QID).ToList();
                string qidList = "";
                for (int i = 0; i < idList.Count; i++)
                {
                    qidList = qidList + idList[i];
                    if (i < idList.Count - 1)
                    {
                        qidList = qidList + ";";
                    }
                }
                ViewBag.QuestionIDList = qidList;
                return View();
            }
        }

        //savereading question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditReadingQuestion(string previousUrl, string chid, string pid, FormCollection collection, string qidList,
            string paragraph, HttpPostedFileBase imgfile)
        {
            /*try
            {*/
                int chapID = int.Parse(chid);
                int passageID = int.Parse(pid);

                //edit passage
                var passage = db.Passages.Find(passageID);
                passage.Text = paragraph.Trim();

                string imageExisted = null;
                if (collection["imageExisted"] != null)
                {
                    imageExisted = collection["imageExisted"];
                }

                if (imageExisted != null)
                {
                    if (imageExisted.Equals("1"))
                    {
                        //change image
                        if (imgfile != null && imgfile.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(imgfile.FileName);
                            var newPath = Server.MapPath("~/Images");
                            if (!Directory.Exists(newPath))
                            {
                                Directory.CreateDirectory(newPath);
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            else
                            {
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            passage.PassageImage = new byte[imgfile.ContentLength];
                            imgfile.InputStream.Read(passage.PassageImage, 0, imgfile.ContentLength);
                        }
                    }
                    else
                    {
                        passage.PassageImage = null;
                    }
                }
                else
                {
                    passage.PassageImage = null;
                }


                db.Entry(passage).State = EntityState.Modified;

                List<string> oldQid = qidList.Split(new char[] { ';' }).ToList();

                string[] qIDList = null;
                if (collection["qid"] != null && !collection["qid"].Equals(""))
                {
                    qIDList = Request.Form.GetValues("qid");
                }
                /*string[] qIDList = collection["qid"].Split(new char[] { ',' });*/
                /*string[] questionList = collection["question"].Split(new char[] { ',' });*/
                /*List<string> options = collection["option"].Split(new char[] { ',' }).ToList();*/
                string[] questionList = Request.Form.GetValues("question");
                List<string> options = Request.Form.GetValues("option").ToList();

                List<string> cbOption = collection["cboption"].Split(new char[] { ',' }).ToList();
                string[] markList = collection["mark"].Split(new char[] { ',' });
                string[] timeList = collection["time"].Split(new char[] { ',' });
                List<string> mixChoiceList = collection["mixChoice"].Split(new char[] { ',' }).ToList(); ;


                int countAnswer = 0;
                int countIndex = 0;
                if (qIDList != null)
                {
                    //get each question and answer
                    for (int i = 0; i < qIDList.Length; i++)
                    {
                        int questionID = int.Parse(qIDList[i]);
                        Question question = db.Questions.Find(questionID);
                        question.Text = questionList[i].Trim();
                        question.Qtype = 2;
                        question.Mark = float.Parse(markList[i]);
                        question.Time = int.Parse(timeList[i]);
                        //get mixchoice checkbox
                        if (mixChoiceList[0].Contains("1"))
                        {
                            question.MixChoice = true;
                            mixChoiceList.RemoveAt(0);
                            mixChoiceList.RemoveAt(0);
                        }
                        else
                        {
                            question.MixChoice = false;
                            mixChoiceList.RemoveAt(0);
                        }
                        db.Entry(question).State = EntityState.Modified;

                        //update quiz
                        string questAndType = question.QID + "-" + question.Qtype;
                        var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();

                        foreach (var quiz in quizContainQuestions)
                        {
                            updateQuizTimeAndMark(quiz);
                        }

                        var answerList = db.QuestionAnswers.Where(qa => qa.QuestionID == questionID);
                        //delete the old answer
                        foreach (var a in answerList)
                        {
                            db.QuestionAnswers.Remove(a);
                        }

                        //add new answer for question
                        for (int j = countAnswer; j < (i + 1) * 6; j++)
                        {
                            countIndex++;
                            if (options[j] != null && !options[j].Trim().Equals(""))
                            {
                                QuestionAnswer qa = new QuestionAnswer();
                                qa.QuestionID = questionID;
                                qa.Text = options[j].Trim();
                                int answerIndex = j + 1;
                                if (cbOption[0].Contains("1"))
                                {
                                    qa.IsCorrect = true;
                                    cbOption.Remove(cbOption[0]);
                                    cbOption.Remove(cbOption[0]);
                                }
                                else
                                {
                                    qa.IsCorrect = false;
                                    cbOption.Remove(cbOption[0]);
                                }

                                db.QuestionAnswers.Add(qa);
                            }
                            else
                            {
                                cbOption.Remove(cbOption[0]);
                            }

                        }
                        countAnswer = countAnswer + 6;

                        var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == questionID && lo.Qtype == 2);

                        //delete the old lo
                        foreach (var lo in oldLoList)
                        {
                            db.QuestionLOes.Remove(lo);
                        }

                        //get new lo
                        if (collection["lo"] != null)
                        {
                            string[] loList = collection["lo"].Split(new char[] { ',' });
                            foreach (string loId in loList)
                            {
                                var lo = db.LearningOutcomes.Find(int.Parse(loId));
                                if (lo != null)
                                {
                                    QuestionLO qLO = new QuestionLO();
                                    qLO.QuestionID = questionID;
                                    qLO.Qtype = 2;
                                    qLO.LearningOutcomeID = lo.LOID;
                                    db.QuestionLOes.Add(qLO);
                                }
                            }
                        }

                    }

                    //delete question which is remove
                    foreach (string id in oldQid)
                    {
                        bool isExisted = false;
                        foreach (string ids in qIDList)
                        {
                            if (id.Equals(ids))
                            {
                                isExisted = true;
                            }
                        }
                        if (isExisted == false)
                        {
                            var q = db.Questions.Find(int.Parse(id));
                            foreach (var a in q.QuestionAnswers.ToList())
                            {
                                db.QuestionAnswers.Remove(a);
                            }
                            db.Questions.Remove(q);
                           

                        }

                    }

                /*    db.SaveChanges();*/
                }
                else
                {
                    //delete question which is remove
                    foreach (string id in oldQid)
                    {
                        var q = db.Questions.Find(int.Parse(id));
                        foreach (var a in q.QuestionAnswers.ToList())
                        {
                            db.QuestionAnswers.Remove(a);
                        }
                        db.Questions.Remove(q);
                    }

                  /*  db.SaveChanges();*/
                }
                db.SaveChanges();

            //add new question if user add question
            if (qIDList == null)
            {
                for (int i = 0; i < questionList.Count(); i++)
                {
                    Question question = new Question();
                    question.Text = questionList[i].Trim();
                    question.Qtype = 2;
                    question.PassageID = passageID;
                    question.ChapterID = chapID;
                    question.Mark = float.Parse(markList[i]);
                    question.CreatedDate = DateTime.Now;
                    question.Time = int.Parse(timeList[i]);
                    //get mixchoice checkbox
                    if (mixChoiceList[0].Contains("1"))
                    {
                        question.MixChoice = true;
                        mixChoiceList.RemoveAt(0);
                        mixChoiceList.RemoveAt(0);
                    }
                    else
                    {
                        question.MixChoice = false;
                        mixChoiceList.RemoveAt(0);
                    }
                    db.Questions.Add(question);
                    db.SaveChanges();
                    int questionID = db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).FirstOrDefault();
                    //add new answer for question
                    for (int j = countAnswer; j < (i + 1) * 6; j++)
                    {
                        countIndex++;
                        if (options[j] != null && !options[j].Trim().Equals(""))
                        {
                            QuestionAnswer qa = new QuestionAnswer();
                            qa.QuestionID = questionID;
                            qa.Text = options[j];
                            int answerIndex = j + 1;
                            if (cbOption[0].Contains("1"))
                            {
                                qa.IsCorrect = true;
                                cbOption.Remove(cbOption[0]);
                                cbOption.Remove(cbOption[0]);
                            }
                            else
                            {
                                qa.IsCorrect = false;
                                cbOption.Remove(cbOption[0]);
                            }

                            db.QuestionAnswers.Add(qa);
                        }
                        else
                        {
                            cbOption.Remove(cbOption[0]);
                        }

                    }
                    countAnswer = countAnswer + 6;

                    //get new lo
                    if (collection["lo"] != null)
                    {
                        string[] loList = collection["lo"].Split(new char[] { ',' });
                        foreach (string loId in loList)
                        {
                            var lo = db.LearningOutcomes.Find(int.Parse(loId));
                            if (lo != null)
                            {
                                QuestionLO qLO = new QuestionLO();
                                qLO.QuestionID = questionID;
                                qLO.Qtype = 2;
                                qLO.LearningOutcomeID = lo.LOID;
                                db.QuestionLOes.Add(qLO);
                            }
                        }
                    }
                }
            }
            else
            {
                if (questionList.Count() > qIDList.Count())
                {
                    for (int i = qIDList.Count(); i < questionList.Count(); i++)
                    {
                        Question question = new Question();
                        question.Text = questionList[i].Trim();
                        question.Qtype = 2;
                        question.PassageID = passageID;
                        question.ChapterID = chapID;
                        question.Mark = float.Parse(markList[i]);
                        question.CreatedDate = DateTime.Now;
                        question.Time = int.Parse(timeList[i]);
                        //get mixchoice checkbox
                        if (mixChoiceList[0].Contains("1"))
                        {
                            question.MixChoice = true;
                            mixChoiceList.RemoveAt(0);
                            mixChoiceList.RemoveAt(0);
                        }
                        else
                        {
                            question.MixChoice = false;
                            mixChoiceList.RemoveAt(0);
                        }
                        db.Questions.Add(question);
                        db.SaveChanges();
                        int questionID = db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).FirstOrDefault();
                        //add new answer for question
                        for (int j = countAnswer; j < (i + 1) * 6; j++)
                        {
                            countIndex++;
                            if (options[j] != null && !options[j].Trim().Equals(""))
                            {
                                QuestionAnswer qa = new QuestionAnswer();
                                qa.QuestionID = questionID;
                                qa.Text = options[j];
                                int answerIndex = j + 1;
                                if (cbOption[0].Contains("1"))
                                {
                                    qa.IsCorrect = true;
                                    cbOption.Remove(cbOption[0]);
                                    cbOption.Remove(cbOption[0]);
                                }
                                else
                                {
                                    qa.IsCorrect = false;
                                    cbOption.Remove(cbOption[0]);
                                }

                                db.QuestionAnswers.Add(qa);
                            }
                            else
                            {
                                cbOption.Remove(cbOption[0]);
                            }

                        }
                        countAnswer = countAnswer + 6;

                        //get new lo
                        if (collection["lo"] != null)
                        {
                            string[] loList = collection["lo"].Split(new char[] { ',' });
                            foreach (string loId in loList)
                            {
                                var lo = db.LearningOutcomes.Find(int.Parse(loId));
                                if (lo != null)
                                {
                                    QuestionLO qLO = new QuestionLO();
                                    qLO.QuestionID = questionID;
                                    qLO.Qtype = 2;
                                    qLO.LearningOutcomeID = lo.LOID;
                                    db.QuestionLOes.Add(qLO);
                                }
                            }
                        }
                    }

                }
            }
            string qSet =passageID + "-2";
            var quizContainQuests = db.Quizs.Where(qz => qz.Questions.Contains(qSet));
            //delete question inside quest
            foreach (var quiz in quizContainQuests)
            {
                /*    string newSet = "";
                    string[] questSet = quiz.Questions.Split(new char[] { ';' });
                    foreach (string qIdAndType in questSet)
                    {
                        if (!qIdAndType.Equals(qSet))
                        {
                            newSet = newSet + qIdAndType + ";";
                        }
                    }

                    if (newSet != "")
                    {
                        quiz.Questions = newSet.Substring(0, newSet.Length - 1);
                    }
                    else
                    {
                        quiz.Questions = null;
                    }
                    db.Entry(quiz).State = EntityState.Modified;*/
                /*db.SaveChanges();*/
                updateQuizTimeAndMark(quiz);
            }

            db.SaveChanges();

                return Redirect(previousUrl);
            /*}
            catch
            { return Redirect("~/Error/NotFound"); }*/
        }

        //show page to create short answer question
        public ActionResult CreateShortAnswerQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }
        [HandleError]

        //add new short answer question
        [HandleError]
        [HttpPost]
        public ActionResult CreateShortAnswerQuestion(string chid, string questionText, FormCollection collection, string mark, string time,
            HttpPostedFileBase imgfile)
        {
            try
            {
                int chapID = int.Parse(chid);
                Question question = new Question();
                Chapter chapter = db.Chapters.Find(chapID);
                question.ChapterID = chapter.ChID;
                question.Text = questionText.Trim();
                question.CreatedDate = DateTime.Now;
                question.Qtype = 4;
                //check if mark is null
                if (!mark.Trim().Equals(""))
                {
                    question.Mark = float.Parse(mark);
                }
                //check if time is null
                if (!time.Trim().Equals(""))
                {
                    question.Time = int.Parse(time);
                }

                //add image
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imgfile.FileName);
                    var newPath = Server.MapPath("~/Images");
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }
                    else
                    {
                        var path = Path.Combine(newPath, fileName);
                        imgfile.SaveAs(path);
                    }

                    question.ImageData = new byte[imgfile.ContentLength];
                    imgfile.InputStream.Read(question.ImageData, 0, imgfile.ContentLength);
                }


                db.Questions.Add(question);
                db.SaveChanges();

                int qid = int.Parse(db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).First().ToString());

                string answer = collection["answer"];

                if (answer != null && !answer.Trim().Equals(""))
                {
                    QuestionAnswer qa = new QuestionAnswer();
                    qa.QuestionID = qid;
                    qa.Text = answer.Trim();
                    qa.IsCorrect = true;
                    db.QuestionAnswers.Add(qa);
                }

                //get lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = qid;
                            qLO.Qtype = 4;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }

                db.SaveChanges();
                int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                    + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
                int lastPage = 0;
                if (getLastPage % 10 == 0)
                {
                    lastPage = getLastPage / 10;
                }
                else
                {
                    lastPage = (getLastPage / 10) + 1;
                }

                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //show page to edit short answer question
        [HandleError]
        public ActionResult EditShortAnswerQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 4) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int questionID = int.Parse(qid);
                Question question = db.Questions.Find(questionID);
                ViewBag.ChapterID = question.ChapterID;
                ViewBag.Question = question;
                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + question.ChapterID;
                if (Request.UrlReferrer != null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }
                ViewBag.Previous = previousPage;

                Chapter chapter = db.Chapters.Find(question.ChapterID);
                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == questionID && ql.Qtype == 4).ToList();
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //saveedit short answer question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditShortAnswerQuestion(string previousUrl, string qid, string chid, string questionText, FormCollection collection, string mark, string time,
            HttpPostedFileBase imgfile)
        {
            try
            {
                int questionID = int.Parse(qid);
                int chapID = int.Parse(chid);
                Question question = db.Questions.Find(questionID);

                question.Text = questionText.Trim();
                question.Mark = float.Parse(mark);
                question.Time = int.Parse(time);

                //change image
                string imageExisted = null;
                if (collection["imageExisted"] != null)
                {
                    imageExisted = collection["imageExisted"];
                }
                if (imageExisted != null)
                {
                    if (imageExisted.Equals("1"))
                    {
                        //change image
                        if (imgfile != null && imgfile.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(imgfile.FileName);
                            var newPath = Server.MapPath("~/Images");
                            if (!Directory.Exists(newPath))
                            {
                                Directory.CreateDirectory(newPath);
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            else
                            {
                                var path = Path.Combine(newPath, fileName);
                                imgfile.SaveAs(path);
                            }
                            question.ImageData = new byte[imgfile.ContentLength];
                            imgfile.InputStream.Read(question.ImageData, 0, imgfile.ContentLength);
                        }
                        else
                        {
                            //delete image
                            /*var base64 = Convert.ToBase64String(question.ImageData);
                            string fullPath = Request.MapPath(base64);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }*/
                            /* question.ImageData = null;*/
                        }
                    }
                    else
                    {
                        question.ImageData = null;
                    }
                }
                else
                {
                    question.ImageData = null;
                }
                db.Entry(question).State = EntityState.Modified;

                var answerList = db.QuestionAnswers.Where(a => a.QuestionID == questionID);
                //delete the old answer
                foreach (var a in answerList)
                {
                    db.QuestionAnswers.Remove(a);
                }

                string questAndType = question.QID + "-" + question.Qtype;
                var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();
                //update mark for quiz
                foreach (var quiz in quizContainQuestions)
                {
                    updateQuizTimeAndMark(quiz);
                }

                db.SaveChanges();

                //add new answer
                string answer = collection["answer"];

                if (answer != null && !answer.Trim().Equals(""))
                {
                    QuestionAnswer qa = new QuestionAnswer();
                    qa.QuestionID = questionID;
                    qa.Text = answer.Trim();
                    qa.IsCorrect = true;
                    db.QuestionAnswers.Add(qa);
                }

                var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == question.QID && lo.Qtype == 4);
                //delete the old lo
                foreach (var lo in oldLoList)
                {
                    db.QuestionLOes.Remove(lo);
                }

                //get new lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = question.QID;
                            qLO.Qtype = 4;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }

                db.SaveChanges();

                return Redirect(previousUrl);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //show page to create matching question
        [HandleError]
        public ActionResult CreateMatchingQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //add new matching question
        [HandleError]
        [HttpPost]
        public ActionResult CreateMatchingQuestion(string chid, FormCollection collection, string mark, string time)
        {
            try
            {
                int chapID = int.Parse(chid);
                MatchQuestion matching = new MatchQuestion();
                Chapter chapter = db.Chapters.Find(chapID);
                matching.ChapterId = chapter.ChID;

                //check if mark is null
                if (!mark.Trim().Equals(""))
                {
                    matching.Mark = float.Parse(mark);
                }
                //check if time is null
                if (!time.Trim().Equals(""))
                {
                    matching.Time = int.Parse(time);
                }

                string columnA = collection["columnA"];
                string columnB = collection["columnB"];
                string solution = collection["solution"];
                solution = solution.Replace(',', ';');


                matching.ColumnA = columnA.Trim();
                matching.ColumnB = columnB.Trim();
                matching.Solution = solution.ToUpper();
                matching.CreatedDate = DateTime.Now;
                db.MatchQuestions.Add(matching);
                db.SaveChanges();

                int mid = int.Parse(db.MatchQuestions.OrderByDescending(m => m.MID).Where(m => m.ChapterId == chapID).Select(m => m.MID).First().ToString());
                //get lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = mid;
                            qLO.Qtype = 5;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }
                db.SaveChanges();
                int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                    + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
                int lastPage = 0;
                if (getLastPage % 10 == 0)
                {
                    lastPage = getLastPage / 10;
                }
                else
                {
                    lastPage = (getLastPage / 10) + 1;
                }
                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }

        //show page to edit matching question
        [HandleError]
        public ActionResult EditMatchingQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 5) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int matchingID = int.Parse(qid);
                MatchQuestion matching = db.MatchQuestions.Find(matchingID);
                ViewBag.Matching = matching;
                ViewBag.Solution = matching.Solution.Split(new char[] { ';' }).ToList();
                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + matching.ChapterId;
                if (Request.UrlReferrer != null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }
                ViewBag.Previous = previousPage;
                Chapter chapter = db.Chapters.Find(matching.ChapterId);
                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == matchingID && ql.Qtype == 5).ToList();
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //save matching question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditMatchingQuestion(string previousUrl, string chid, string qid, FormCollection collection, string mark, string time)
        {
            int matchingID = int.Parse(qid);
            int chapID = int.Parse(chid);
            MatchQuestion matching = db.MatchQuestions.Find(matchingID);
            double? oldMark = matching.Mark;
            matching.Mark = float.Parse(mark);
            matching.Time = int.Parse(time);

            string columnA = collection["columnA"];
            string columnB = collection["columnB"];
            string solution = collection["solution"];
            solution = solution.Replace(',', ';');
            matching.Solution = solution.ToUpper();
            matching.ColumnA = columnA.Trim();
            matching.ColumnB = columnB.Trim();
            db.Entry(matching).State = EntityState.Modified;

            string questAndType = matching.MID + "-5";
            var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();
            //update mark for quiz
            foreach (var quiz in quizContainQuestions)
            {
                updateQuizTimeAndMark(quiz);
            }

            var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == matching.MID && lo.Qtype == 5);
            //delete the old lo
            foreach (var lo in oldLoList)
            {
                db.QuestionLOes.Remove(lo);
            }

            //get new lo
            if (collection["lo"] != null)
            {
                string[] loList = collection["lo"].Split(new char[] { ',' });
                foreach (string loId in loList)
                {
                    var lo = db.LearningOutcomes.Find(int.Parse(loId));
                    if (lo != null)
                    {
                        QuestionLO qLO = new QuestionLO();
                        qLO.QuestionID = matching.MID;
                        qLO.Qtype = 5;
                        qLO.LearningOutcomeID = lo.LOID;
                        db.QuestionLOes.Add(qLO);
                    }
                }
            }


            db.SaveChanges();
            return Redirect(previousUrl);
        }

        //show page to create indicate mistake question
        [HandleError]
        public ActionResult CreateIndicateMistakeQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //add new indicate mistake question
        [HandleError]
        [HttpPost]
        public ActionResult CreateIndicateMistakeQuestion(string chid, string questionText, FormCollection collection, string mark, string time)
        {
            int chapID = int.Parse(chid);
            Question question = new Question();
            Chapter chapter = db.Chapters.Find(chapID);
            question.ChapterID = chapter.ChID;
            question.Text = questionText.Trim();
            question.CreatedDate = DateTime.Now;
            question.Qtype = 6;

            //check if mark is null
            if (!mark.Trim().Equals(""))
            {
                question.Mark = float.Parse(mark);
            }
            //check if time is null
            if (!time.Trim().Equals(""))
            {
                question.Time = int.Parse(time);
            }

            db.Questions.Add(question);

            string questAndType = question.QID + "-" + question.Qtype;
            var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();
            //update mark for quiz
            foreach (var quiz in quizContainQuestions)
            {
                updateQuizTimeAndMark(quiz);
            }

            db.SaveChanges();

            int qid = int.Parse(db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).First().ToString());

            //get correct answer
            string correctAnswer = collection["answer"].Trim();

            //get choice inside round bracket
            List<string> answerList = new List<string>();
            Regex regex = new Regex(@"\(([^()]+)\)*");
            /*Regex regex =new Regex(@"/\([aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,.\-\+\=\*\/ ] +\)/ g");*/
            foreach (Match match in regex.Matches(questionText))
            {
                string ans = match.Value;
                answerList.Add(ans);
            }

            //add answer to db
            if (answerList != null)
            {
                /*foreach (string ans in answerList)
                {
                    string trimBracketAns = ans.Trim().Substring(1, ans.Length - 2).Trim();
                    //if answer is correct
                    if (trimBracketAns.ToString().ToLower().Equals(correctAnswer.Trim().ToLower()))
                    {
                        QuestionAnswer qa = new QuestionAnswer();
                        qa.QuestionID = qid;
                        qa.Text = trimBracketAns;
                        qa.IsCorrect = true;
                        db.QuestionAnswers.Add(qa);
                    }

                }*/
                foreach (string ans in answerList)
                {
                    string trimBracketAns = ans.Trim().Substring(1, ans.Length - 2).Trim();

                    QuestionAnswer qa = new QuestionAnswer();
                    qa.QuestionID = qid;
                    qa.Text = trimBracketAns;
                    //if answer is wrong
                    if (!trimBracketAns.ToString().ToLower().Equals(correctAnswer.Trim().ToLower()))
                    {
                        qa.IsCorrect = false;
                    }
                    else
                    {
                        qa.IsCorrect = true;
                    }
                    db.QuestionAnswers.Add(qa);
                }
            }

            //get lo
            if (collection["lo"] != null)
            {
                string[] loList = collection["lo"].Split(new char[] { ',' });
                foreach (string loId in loList)
                {
                    var lo = db.LearningOutcomes.Find(int.Parse(loId));
                    if (lo != null)
                    {
                        QuestionLO qLO = new QuestionLO();
                        qLO.QuestionID = qid;
                        qLO.Qtype = 6;
                        qLO.LearningOutcomeID = lo.LOID;
                        db.QuestionLOes.Add(qLO);
                    }
                }
            }

            db.SaveChanges();

            int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
            int lastPage = 0;
            if (getLastPage % 10 == 0)
            {
                lastPage = getLastPage / 10;
            }
            else
            {
                lastPage = (getLastPage / 10) + 1;
            }

            return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);

        }

        //show page to edit indicate mistake question
        [HandleError]
        public ActionResult EditIndicateMistakeQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 6) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int questionID = int.Parse(qid);
                Question question = db.Questions.Find(questionID);
                ViewBag.ChapterID = question.ChapterID;
                ViewBag.Question = question;
                string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + question.ChapterID;
                if (Request.UrlReferrer != null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }
                ViewBag.Previous = previousPage;
                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == question.QID && ql.Qtype == 6).ToList();
                Chapter chapter = db.Chapters.Find(question.ChapterID);
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //save indicate mistake question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditIndicateMistakeQuestion(string previousUrl, string qid, string chid, string questionText, FormCollection collection, string mark, string time)
        {
            try
            {
                int questionID = int.Parse(qid);
                int chapID = int.Parse(chid);
                Question question = db.Questions.Find(questionID);

                question.Text = questionText.Trim();
                question.Mark = float.Parse(mark);
                question.Time = int.Parse(time);
                db.Entry(question).State = EntityState.Modified;

                var oldAnswerList = db.QuestionAnswers.Where(a => a.QuestionID == questionID);
                //delete the old answer
                foreach (var a in oldAnswerList)
                {
                    db.QuestionAnswers.Remove(a);
                }

                string questAndType = question.QID + "-" + question.Qtype;
                var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();
                //update mark for quiz
                foreach (var quiz in quizContainQuestions)
                {
                    updateQuizTimeAndMark(quiz);
                }

                db.SaveChanges();


                //get correct answer
                string correctAnswer = collection["answer"];

                //get choice inside round bracket
                List<string> answerList = new List<string>();
                Regex regex = new Regex(@"\(([^()]+)\)*");
                foreach (Match match in regex.Matches(questionText))
                {
                    string ans = match.Value;
                    answerList.Add(ans);
                }

                //add answer to db
                if (answerList != null)
                {
                    foreach (string ans in answerList)
                    {
                        string trimBracketAns = ans.Trim().Substring(1, ans.Length - 2).Trim();

                        QuestionAnswer qa = new QuestionAnswer();
                        qa.QuestionID = questionID;
                        qa.Text = trimBracketAns;
                        //if answer is wrong
                        if (!trimBracketAns.ToString().ToLower().Equals(correctAnswer.Trim().ToLower()))
                        {
                            qa.IsCorrect = false;
                        }
                        else
                        {
                            qa.IsCorrect = true;
                        }
                        db.QuestionAnswers.Add(qa);
                    }
                }


                var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == questionID && lo.Qtype == 6);
                //delete the old lo
                foreach (var lo in oldLoList)
                {
                    db.QuestionLOes.Remove(lo);
                }

                //get new lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = questionID;
                            qLO.Qtype = 6;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }
                db.SaveChanges();



                return Redirect(previousUrl);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }


        //show page to create fill blank question
        [HandleError]
        public ActionResult CreateFillBlankQuestion(string chid)
        {
            //check if chapter is availble
            if (checkChapterIdAvailbile(chid) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int chapID = int.Parse(chid);
                Chapter chapter = db.Chapters.Find(chapID);
                ViewBag.ChapterID = chapID;
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //add new fill blank question
        [HandleError]
        [HttpPost]
        public ActionResult CreateFillBlankQuestion(string chid, string questionText, FormCollection collection, string mark, string time)
        {
            try
            {
                int chapID = int.Parse(chid);
                Question question = new Question();
                Chapter chapter = db.Chapters.Find(chapID);
                question.ChapterID = chapter.ChID;
                question.Text = questionText.Trim();
                question.CreatedDate = DateTime.Now;
                question.Qtype = 3;
                //check if mark is null
                if (!mark.Trim().Equals(""))
                {
                    question.Mark = float.Parse(mark);
                }
                //check if time is null
                if (!time.Trim().Equals(""))
                {
                    question.Time = int.Parse(time);
                }

                //check if question have given word or not
                string givenWord = collection["givenWord"];
                if (givenWord != null && !givenWord.Trim().Equals(""))
                {
                    question.GivenWord = true;

                }
                else
                {
                    question.GivenWord = false;
                }

                db.Questions.Add(question);
                db.SaveChanges();

                int qid = int.Parse(db.Questions.OrderByDescending(q => q.QID).Where(q => q.ChapterID == chapID).Select(q => q.QID).First().ToString());

                //check if the question is given word or not
                if (question.GivenWord == true)
                {
                    List<string> answerList = new List<string>();
                    Regex regex = new Regex(@"\(([^()]+)\)*");
                    //get text inside round bracket
                    foreach (Match match in regex.Matches(questionText))
                    {
                        string ansList = match.Value;
                        answerList.Add(ansList);
                    }

                    //add correct answer to db
                    if (answerList != null)
                    {
                        foreach (string ans in answerList)
                        {
                            string trimBracketAns = ans.Trim().Substring(2, ans.Length - 3);
                            string[] choices = trimBracketAns.Split(new char[] { '~' });
                            foreach (string choice in choices)
                            {
                                if (choice.Trim().ToLower().Contains("="))
                                {
                                    string qChoice = choice.Trim(); 
                                    if (qChoice != null && !qChoice.Trim().Equals(""))
                                    {
                                        QuestionAnswer qa = new QuestionAnswer();
                                        qa.QuestionID = qid;
                                        qa.Text = qChoice.Substring(1, qChoice.Length - 1).Trim();
                                        qa.IsCorrect = true;
                                        db.QuestionAnswers.Add(qa);
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    List<string> answerList = new List<string>();
                    Regex regex = new Regex(@"\(([^()]+)\)*");
                    //get text inside round bracket
                    foreach (Match match in regex.Matches(questionText))
                    {
                        string ansList = match.Value;
                        answerList.Add(ansList);
                    }
                    //add correct answer to db
                    if (answerList != null)
                    {
                        foreach (string ans in answerList)
                        {
                            if (ans.Contains("~=") && ans.Contains("~"))
                            {
                                string trimBracketAns = ans.Trim().Substring(2, ans.Length - 3);
                                string[] choices = trimBracketAns.Split(new char[] { '~' });
                                foreach (string choice in choices)
                                {
                                    if (choice.Trim().ToLower().Contains("="))
                                    {
                                        string qChoice = choice.Trim();
                                        if (qChoice != null && !qChoice.Trim().Equals(""))
                                        {
                                            QuestionAnswer qa = new QuestionAnswer();
                                            qa.QuestionID = qid;
                                            qa.Text = qChoice.Substring(1, qChoice.Length - 1).Trim();
                                            qa.IsCorrect = true;
                                            db.QuestionAnswers.Add(qa);
                                        }
                                           
                                    }
                                }
                            }
                            else
                            {
                                string correctAns = ans.Trim().Substring(1, ans.Length - 2);
                                if(correctAns!=null && !correctAns.Trim().Equals(""))
                                {

                                    QuestionAnswer qa = new QuestionAnswer();
                                    qa.QuestionID = qid;
                                    qa.Text = correctAns.Trim();
                                    qa.IsCorrect = true;
                                    db.QuestionAnswers.Add(qa);
                                }
                            }
                        }
                    }
                }

                //get lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = qid;
                            qLO.Qtype = 3;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }

                db.SaveChanges();
                int getLastPage = (db.Questions.Where(q => q.ChapterID == chapID && q.Qtype != 2).Count(q => q.ChapterID == chapID)
                    + db.MatchQuestions.Count(m => m.ChapterId == chapID) + db.Passages.Count(p => p.ChapterID == chapID));
                int lastPage = 0;
                if (getLastPage % 10 == 0)
                {
                    lastPage = getLastPage / 10;
                }
                else
                {
                    lastPage = (getLastPage / 10) + 1;
                }

                return Redirect("~/Teacher/Question/ViewQuestionByChapter?chid=" + chapID + "&i=" + lastPage);
            }
            catch
            {
                return Redirect("~/Error/NotFound");
            }
        }

        //show page to edit fill blank question
        [HandleError]
        public ActionResult EditFillBlankQuestion(string qid)
        {
            //check if chapter is availble
            if (checkQuestionIdAvailble(qid, 3) == false)
            {
                return RedirectToAction("QuestionBank");
            }
            else
            {

                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                int questionID = int.Parse(qid);
                Question question = db.Questions.Find(questionID);
                ViewBag.ChapterID = question.ChapterID;
                ViewBag.Question = question; string previousPage = "~/Teacher/Question/ViewQuestionByChapter?chid=" + question.ChapterID;
                if (Request.UrlReferrer != null  )
                {
                    previousPage = Request.UrlReferrer.ToString();
                }
                ViewBag.Previous = previousPage;
                Chapter chapter = db.Chapters.Find(question.ChapterID);
                ViewBag.QuestionLO = db.QuestionLOes.Where(ql => ql.QuestionID == question.QID && ql.Qtype == 3).ToList();
                ViewBag.LoList = db.LearningOutcomes.Where(lo => lo.CourseID == chapter.CourseID).ToList();
                return View();
            }
        }

        //save fill blank question after edit
        [HandleError]
        [HttpPost]
        public ActionResult EditFillBlankQuestion(string previousUrl, string qid, string chid, string questionText, FormCollection collection, string mark, string time)
        {
            try
            {
                int questionID = int.Parse(qid);
                int chapID = int.Parse(chid);
                Question question = db.Questions.Find(questionID);

                question.Text = questionText.Trim();
                question.Mark = float.Parse(mark);
                question.Time = int.Parse(time);

                //check if question have given word or not
                string givenWord = collection["givenWord"];
                if (givenWord != null && !givenWord.Trim().Equals(""))
                {
                    question.GivenWord = true;

                }
                else
                {
                    question.GivenWord = false;
                }

                db.Entry(question).State = EntityState.Modified;

                var oldAnswerList = db.QuestionAnswers.Where(a => a.QuestionID == questionID);
                //delete the old answer
                foreach (var a in oldAnswerList)
                {
                    db.QuestionAnswers.Remove(a);
                }

                //check if the question is given word or not
                if (question.GivenWord == true)
                {
                    List<string> answerList = new List<string>();
                    Regex regex = new Regex(@"\(([^()]+)\)*");
                    //get text inside round bracket
                    foreach (Match match in regex.Matches(questionText))
                    {
                        string ansList = match.Value;
                        answerList.Add(ansList);
                    }

                    //add correct answer to db
                    if (answerList != null)
                    {
                        foreach (string ans in answerList)
                        {
                            string trimBracketAns = ans.Trim().Substring(2, ans.Length - 3);
                            string[] choices = trimBracketAns.Split(new char[] { '~' });
                            foreach (string choice in choices)
                            {
                                if (choice.Trim().ToLower().Contains("="))
                                {
                                    string qChoice = choice.Trim();
                                    if (qChoice != null && !qChoice.Trim().Equals(""))
                                    {
                                        QuestionAnswer qa = new QuestionAnswer();
                                        qa.QuestionID = questionID;
                                        qa.Text = qChoice.Substring(1, qChoice.Length - 1).Trim();
                                        qa.IsCorrect = true;
                                        db.QuestionAnswers.Add(qa);
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    List<string> answerList = new List<string>();
                    Regex regex = new Regex(@"\(([^()]+)\)*");
                    //get text inside round bracket
                    foreach (Match match in regex.Matches(questionText))
                    {
                        string ansList = match.Value;
                        answerList.Add(ansList);
                    }
                    //add correct answer to db
                    if (answerList != null)
                    {

                        foreach (string ans in answerList)
                        {
                            if (ans.Contains("~=") && ans.Contains("~"))
                            {
                                string trimBracketAns = ans.Trim().Substring(2, ans.Length - 3);
                                string[] choices = trimBracketAns.Split(new char[] { '~' });
                                foreach (string choice in choices)
                                {
                                    if (choice.Trim().ToLower().Contains("="))
                                    {
                                        string qChoice = choice.Trim();
                                        if (qChoice != null && !qChoice.Trim().Equals(""))
                                        {

                                            QuestionAnswer qa = new QuestionAnswer();
                                            qa.QuestionID = questionID;
                                            qa.Text = qChoice.Substring(1, qChoice.Length - 1).Trim();
                                            qa.IsCorrect = true;
                                            db.QuestionAnswers.Add(qa);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string correctAns = ans.Trim().Substring(1, ans.Length - 2);
                                if (correctAns != null && !correctAns.Trim().Equals(""))
                                {

                                    QuestionAnswer qa = new QuestionAnswer();
                                    qa.QuestionID = questionID;
                                    qa.Text = correctAns.Trim();
                                    qa.IsCorrect = true;
                                    db.QuestionAnswers.Add(qa);
                                }
                            }
                        }

                    }
                }
                string questAndType = question.QID + "-" + question.Qtype;
                var quizContainQuestions = db.Quizs.Where(qz => qz.Questions.Contains(questAndType)).ToList();
                //update mark for quiz
                foreach (var quiz in quizContainQuestions)
                {
                    updateQuizTimeAndMark(quiz);
                }

                var oldLoList = db.QuestionLOes.Where(lo => lo.QuestionID == questionID && lo.Qtype == 3);
                //delete the old lo
                foreach (var lo in oldLoList)
                {
                    db.QuestionLOes.Remove(lo);
                }

                //get new lo
                if (collection["lo"] != null)
                {
                    string[] loList = collection["lo"].Split(new char[] { ',' });
                    foreach (string loId in loList)
                    {
                        var lo = db.LearningOutcomes.Find(int.Parse(loId));
                        if (lo != null)
                        {
                            QuestionLO qLO = new QuestionLO();
                            qLO.QuestionID = questionID;
                            qLO.Qtype = 3;
                            qLO.LearningOutcomeID = lo.LOID;
                            db.QuestionLOes.Add(qLO);
                        }
                    }
                }

                db.SaveChanges();


                return Redirect(previousUrl);
            }
            catch
            { return Redirect("~/Error/NotFound"); }
        }


        [HandleError]
        private void updateQuizTimeAndMark(Quiz quiz)
        {
            /*var quiz = db.Quizs.Find(qzid);*/
            double newMark = 0;
            int newTime = 0;
            int newNumQuest = 0;
            //check if question List is null
            if (quiz.Questions != null && !quiz.Questions.Equals(""))
            {
                string[] quizQuestions = quiz.Questions.Split(new char[] { ';' });
                Dictionary<int, string> questionSet = new Dictionary<int, string>();
                Dictionary<int, string> matchingSet = new Dictionary<int, string>();
                Dictionary<int, string> passageSet = new Dictionary<int, string>();
                //get question List from test
                foreach (string questions in quizQuestions)
                {
                    string[] questAndType = questions.Split(new char[] { '-' });
                    int qtypeID = int.Parse(questAndType[1]);
                    if (qtypeID == 5)
                    {
                        int mID = int.Parse(questAndType[0]);
                        matchingSet.Add(mID, "5");
                    }
                    else if (qtypeID == 2)
                    {
                        int pID = int.Parse(questAndType[0]);
                        passageSet.Add(pID, "2");
                    }
                    else
                    {
                        int qID = int.Parse(questAndType[0]);
                        questionSet.Add(qID, questAndType[1]);
                    }

                }

                //get question from DB
                foreach (KeyValuePair<int, string> keyValuePair in questionSet)
                {
                    var quest = db.Questions.Find(keyValuePair.Key);

                    newMark = newMark + quest.Mark;
                    newTime = newTime + quest.Time;
                    newNumQuest++;
                }

                //get matching question from DB
                foreach (KeyValuePair<int, string> keyValuePair in matchingSet)
                {
                    var mQuest = db.MatchQuestions.Find(keyValuePair.Key);
                    newMark = newMark + mQuest.Mark;
                    newTime = newTime + mQuest.Time;
                    newNumQuest++;
                }

                //get reading question from DB
                foreach (KeyValuePair<int, string> keyValuePair in passageSet)
                {
                    var passage = db.Passages.Find(keyValuePair.Key);
                    foreach (var q in passage.Questions)
                    {

                        newMark = newMark + q.Mark;
                        newTime = newTime + q.Time;
                    }
                    newNumQuest++;
                }
            }
            quiz.Mark = newMark;
            quiz.Time = newTime;
            quiz.NumOfQuestion = newNumQuest;
            db.Entry(quiz).State = EntityState.Modified;
            /*db.SaveChanges();*/
        }
    }
}