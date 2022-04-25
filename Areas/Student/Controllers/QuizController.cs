using InClassVoting.Filter;
using InClassVoting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.Student.Controllers.QuizController
{
    [AccessAuthenForQuiz]
    [UserAuthorizeFilter("Student")]
    public class QuizController : Controller
    {
        private DBModel db = new DBModel();

        // GET: Student/Quiz

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private bool checkQuizIdEncodeAvailbile(string qzid)
        {
            bool check = true;
            string checkQuizId = Base64Decode(qzid);
            int quizId;
            bool isInt = int.TryParse(checkQuizId, out quizId);
            //check if quiz id is int
            if (isInt == false)
            {
                check = false;
            }
            else
            {
                var quiz = db.Quizs.Find(quizId);
                //check if quiz exist in db
                if (quiz == null)
                {
                    check = false;
                }
            }
            return (check);
        }


        public ActionResult DoQuiz(string qzid)
        {
            //check if quiz is availble
            if (checkQuizIdEncodeAvailbile(qzid) == false)
            { 
                return Redirect("~/Student/Home/Home");
            }
            else
            {
                int quizId = int.Parse(Base64Decode(qzid));
                var getQuiz = db.Quizs.Find(quizId);
                if (getQuiz.QuizType.Equals("ShowAllQuestion"))
                { 
                    return RedirectToAction("DoQuizShowAllQuestion", new { qzid = qzid });
                }
                else
                { 
                    return RedirectToAction("DoQuizQuestionByQuestion", new { qzid = qzid });
                }
            }
        }

        [NoCache]
        public ActionResult DoQuizShowAllQuestion(string qzid)
        {
            //check if quiz is availble
            if (checkQuizIdEncodeAvailbile(qzid) == false)
            {
                return Redirect("~/Student/Home/Home");
            }
            else
            {
                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                qzid = Base64Decode(qzid);
                int quizId = int.Parse(qzid);
                var getQuiz = db.Quizs.Find(quizId);
                if (getQuiz.Status.Equals("Not Done") || getQuiz.Status.Equals("Done"))
                {
                    return View("QuizNotStartYet");
                }
                else
                {
                    //if the quiz type is different
                    if (getQuiz.QuizType.Equals("ShowQuestionByQuestion"))
                    {
                        return Redirect("~/Student/Home/Home");
                    }
                    else
                    {
                        List<QuestionDone> multipleQuestionsList = new List<QuestionDone>();
                        List<QuestionDone> readingQuestionsList = new List<QuestionDone>();
                        List<QuestionDone> fillBlankQuestionsList = new List<QuestionDone>();
                        List<QuestionDone> shortAnswerQuestionsList = new List<QuestionDone>();
                        List<QuestionDone> indicateMistakeQuestionsList = new List<QuestionDone>();
                        List<MatchQuestionDone> matchQuestionsList = new List<MatchQuestionDone>();
                        List<Passage_Done> passageList = new List<Passage_Done>();

                        //get quiz saved in database
                        var quiz = db.QuizDones.Where(qz => qz.QuizID == quizId).OrderByDescending(qz => qz.QuizDoneID).FirstOrDefault();

                        int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
                        Student_QuizDone getStudentQuizFromDB = db.Student_QuizDone.Where(sq => sq.StudentID == studentId && sq.QuizDoneID == quiz.QuizDoneID).FirstOrDefault();



                        //if student not access to the quiz before
                        if (getStudentQuizFromDB == null)
                        {
                            double quizMark = 0;
                            //check if questions list is null
                            if (quiz.Questions != null && !quiz.Questions.Equals(""))
                            {
                                //////////////////////////////////////
                                string[] quizQuestions = quiz.Questions.Split(new char[] { ';' });
                                List<string> questionList = new List<string>();
                                //if quiz mix question
                                if (quiz.MixQuestion == true)
                                {
                                    Random rd = new Random();
                                    int numOfQuest = quizQuestions.Count();
                                    while (numOfQuest > 1)
                                    {
                                        numOfQuest--;
                                        int k = rd.Next(numOfQuest + 1);
                                        var qaTemp = quizQuestions[k];
                                        quizQuestions[k] = quizQuestions[numOfQuest];
                                        quizQuestions[numOfQuest] = qaTemp;
                                    }
                                }
                                //get random question in quiz
                                if (quiz.MixQuestionNumber != null && quiz.MixQuestionNumber != 0)
                                {
                                    List<int> addedQuestion = new List<int>();
                                    Random rd = new Random();
                                    for (int i = 0; i < quiz.MixQuestionNumber; i++)
                                    {
                                        int q = rd.Next(quizQuestions.Length); 
                                        while (addedQuestion.Contains(q))
                                        {
                                            q = rd.Next(quizQuestions.Length); 
                                        } 
                                        questionList.Add(quizQuestions[q]);
                                        addedQuestion.Add(q);

                                    }  

                                }
                                else
                                {
                                    questionList = quizQuestions.ToList();
                                } 
                                //////////////////////////////////////
/*
                                Dictionary<int, string> questionSet = new Dictionary<int, string>();
                                Dictionary<int, string> matchingSet = new Dictionary<int, string>();*/
                                foreach (string questions in questionList)
                                {
                                    string[] questAndType = questions.Split(new char[] { '-' });
                                    int qType = int.Parse(questAndType[1]);
                                    if (qType == 5)
                                    {
                                        int mID = int.Parse(questAndType[0]);
                                        /*matchingSet.Add(mID, questAndType[1]);*/
                                        var matchQuest = db.MatchQuestionDones.Find(mID);
                                        matchQuestionsList.Add(matchQuest);
                                        quizMark = quizMark + matchQuest.Mark;
                                    }
                                    else if (qType == 2)
                                    {
                                        int pID = int.Parse(questAndType[0]);
                                        var passage = db.Passage_Done.Find(pID);
                                        passageList.Add(passage);

                                        foreach(var quest in passage.QuestionDones)
                                        {
                                            List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                            //if question mix choice
                                            if (quest.MixChoice == true)
                                            {
                                                Random rd = new Random();
                                                int numOfAnswer = qAnswer.Count;
                                                while (numOfAnswer > 1)
                                                {
                                                    numOfAnswer--;
                                                    int k = rd.Next(numOfAnswer + 1);
                                                    var qaTemp = qAnswer[k];
                                                    qAnswer[k] = qAnswer[numOfAnswer];
                                                    qAnswer[numOfAnswer] = qaTemp;
                                                }
                                                quest.QuestionAnswerDones = qAnswer;
                                            }
                                            readingQuestionsList.Add(quest);
                                            quizMark = quizMark + quest.Mark;
                                        }
                                    }
                                    else
                                    {
                                        int qID = int.Parse(questAndType[0]);
                                        var quest = db.QuestionDones.Find(qID);
                                        List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                        if (quest.Qtype == 1)
                                        {
                                            //if question mix choice
                                            if (quest.MixChoice == true)
                                            {
                                                Random rd = new Random();
                                                int numOfAnswer = qAnswer.Count;
                                                while (numOfAnswer > 1)
                                                {
                                                    numOfAnswer--;
                                                    int k = rd.Next(numOfAnswer + 1);
                                                    var qaTemp = qAnswer[k];
                                                    qAnswer[k] = qAnswer[numOfAnswer];
                                                    qAnswer[numOfAnswer] = qaTemp;
                                                }
                                                quest.QuestionAnswerDones = qAnswer;
                                            }
                                            multipleQuestionsList.Add(quest);
                                            quizMark = quizMark + quest.Mark;
                                        }
                                       /* else if (quest.Qtype == 2)
                                        {
                                            //if question mix choice
                                            if (quest.MixChoice == true)
                                            {
                                                Random rd = new Random();
                                                int numOfAnswer = qAnswer.Count;
                                                while (numOfAnswer > 1)
                                                {
                                                    numOfAnswer--;
                                                    int k = rd.Next(numOfAnswer + 1);
                                                    var qaTemp = qAnswer[k];
                                                    qAnswer[k] = qAnswer[numOfAnswer];
                                                    qAnswer[numOfAnswer] = qaTemp;
                                                }
                                                quest.QuestionAnswerDones = qAnswer;
                                            }
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
                                            quizMark = quizMark + quest.Mark;
                                        }*/
                                        else if (quest.Qtype == 3)
                                        {
                                            fillBlankQuestionsList.Add(quest);
                                            quizMark = quizMark + quest.Mark;
                                        }
                                        else if (quest.Qtype == 4)
                                        {
                                            shortAnswerQuestionsList.Add(quest);
                                            quizMark = quizMark + quest.Mark;
                                        }
                                        else if (quest.Qtype == 6)
                                        {
                                            indicateMistakeQuestionsList.Add(quest);
                                            quizMark = quizMark + quest.Mark;
                                        }
                                    }

                                }


                               /* foreach (KeyValuePair<int, string> keyValuePair in questionSet)
                                {
                                    var quest = db.QuestionDones.Find(keyValuePair.Key);
                                    List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                     
                                    if (quest.Qtype == 1)
                                    {
                                        //if question mix choice
                                        if (quest.MixChoice == true)
                                        {
                                            Random rd = new Random();
                                            int numOfAnswer = qAnswer.Count;
                                            while (numOfAnswer > 1)
                                            {
                                                numOfAnswer--;
                                                int k = rd.Next(numOfAnswer + 1);
                                                var qaTemp = qAnswer[k];
                                                qAnswer[k] = qAnswer[numOfAnswer];
                                                qAnswer[numOfAnswer] = qaTemp;
                                            }
                                            quest.QuestionAnswerDones = qAnswer;
                                        }
                                        multipleQuestionsList.Add(quest);
                                        quizMark = quizMark + quest.Mark; 
                                    }
                                    else if (quest.Qtype == 2)
                                    {
                                        //if question mix choice
                                        if (quest.MixChoice == true)
                                        {
                                            Random rd = new Random();
                                            int numOfAnswer = qAnswer.Count;
                                            while (numOfAnswer > 1)
                                            {
                                                numOfAnswer--;
                                                int k = rd.Next(numOfAnswer + 1);
                                                var qaTemp = qAnswer[k];
                                                qAnswer[k] = qAnswer[numOfAnswer];
                                                qAnswer[numOfAnswer] = qaTemp;
                                            }
                                            quest.QuestionAnswerDones = qAnswer;
                                        }
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
                                        quizMark = quizMark + quest.Mark; 
                                    }
                                    else if (quest.Qtype == 3)
                                    {
                                        fillBlankQuestionsList.Add(quest);
                                        quizMark = quizMark + quest.Mark;
                                    }
                                    else if (quest.Qtype == 4)
                                    {
                                        shortAnswerQuestionsList.Add(quest);
                                        quizMark = quizMark + quest.Mark;
                                    }
                                    else if (quest.Qtype == 6)
                                    {
                                        indicateMistakeQuestionsList.Add(quest);
                                        quizMark = quizMark + quest.Mark;
                                    }
                                }

                                foreach (KeyValuePair<int, string> keyValuePair in matchingSet)
                                {
                                    var matchQuest = db.MatchQuestionDones.Find(keyValuePair.Key);
                                    matchQuestionsList.Add(matchQuest);
                                    quizMark = quizMark + matchQuest.Mark;

                                }*/

                                string receivedQuestions = "";
                                foreach (string quest in questionList)
                                {
                                    receivedQuestions = receivedQuestions + quest + ";";
                                }
                                Student_QuizDone studentQuizDone = new Student_QuizDone();
                                studentQuizDone.StudentID = studentId;
                                studentQuizDone.QuizDoneID = quiz.QuizDoneID;
                                studentQuizDone.ReceivedQuestions = receivedQuestions.Substring(0, receivedQuestions.Length - 1);
                                studentQuizDone.Status = "Doing";
                                studentQuizDone.StudentMark = 0;
                                studentQuizDone.TotalMark = quizMark;
                                db.Student_QuizDone.Add(studentQuizDone);
                                db.SaveChanges();
                            } 
                        }
                        else
                        {
                            if (getStudentQuizFromDB.Status.Equals("Doing"))
                            {
                                double quizMark = 0;
                                if (getStudentQuizFromDB.ReceivedQuestions != null && !getStudentQuizFromDB.ReceivedQuestions.Equals(""))
                                {

                                    //////////////////////////////////////
                                    string[] quizQuestions = quiz.Questions.Split(new char[] { ';' });
                                    List<string> questionList = new List<string>();
                                    //if quiz mix question
                                    if (quiz.MixQuestion == true)
                                    {
                                        Random rd = new Random();
                                        int numOfQuest = quizQuestions.Count();
                                        while (numOfQuest > 1)
                                        {
                                            numOfQuest--;
                                            int k = rd.Next(numOfQuest + 1);
                                            var qaTemp = quizQuestions[k];
                                            quizQuestions[k] = quizQuestions[numOfQuest];
                                            quizQuestions[numOfQuest] = qaTemp;
                                        }
                                    }
                                    //get random question in quiz
                                    if (quiz.MixQuestionNumber != null && quiz.MixQuestionNumber != 0)
                                    {
                                        List<int> addedQuestion = new List<int>();
                                        Random rd = new Random();
                                        for (int i = 0; i < quiz.MixQuestionNumber; i++)
                                        {
                                            int q = rd.Next(quizQuestions.Length);
                                            while (addedQuestion.Contains(q))
                                            {
                                                q = rd.Next(quizQuestions.Length);
                                            }
                                            questionList.Add(quizQuestions[q]);
                                            addedQuestion.Add(q);

                                        }

                                    }
                                    else
                                    {
                                        questionList = quizQuestions.ToList();
                                    }
                                    //////////////////////////////////////
/*
                                    Dictionary<int, string> questionSet = new Dictionary<int, string>();
                                    Dictionary<int, string> matchingSet = new Dictionary<int, string>();*/
                                    foreach (string questions in questionList)
                                    {
                                        string[] questAndType = questions.Split(new char[] { '-' });
                                        int qType = int.Parse(questAndType[1]);
                                        if (qType == 5)
                                        {
                                            int mID = int.Parse(questAndType[0]);
                                            /*matchingSet.Add(mID, questAndType[1]);*/
                                            var matchQuest = db.MatchQuestionDones.Find(mID);
                                            matchQuestionsList.Add(matchQuest);
                                            quizMark = quizMark + matchQuest.Mark;
                                        }
                                        else if (qType == 2)
                                        {
                                            int pID = int.Parse(questAndType[0]);
                                            var passage = db.Passage_Done.Find(pID);
                                            passageList.Add(passage);

                                            foreach (var quest in passage.QuestionDones)
                                            {
                                                List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                                //if question mix choice
                                                if (quest.MixChoice == true)
                                                {
                                                    Random rd = new Random();
                                                    int numOfAnswer = qAnswer.Count;
                                                    while (numOfAnswer > 1)
                                                    {
                                                        numOfAnswer--;
                                                        int k = rd.Next(numOfAnswer + 1);
                                                        var qaTemp = qAnswer[k];
                                                        qAnswer[k] = qAnswer[numOfAnswer];
                                                        qAnswer[numOfAnswer] = qaTemp;
                                                    }
                                                    quest.QuestionAnswerDones = qAnswer;
                                                }
                                                readingQuestionsList.Add(quest);
                                                quizMark = quizMark + quest.Mark;
                                            }
                                        }
                                        else
                                        {
                                            int qID = int.Parse(questAndType[0]);
                                            var quest = db.QuestionDones.Find(qID);
                                            List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                            if (quest.Qtype == 1)
                                            {
                                                //if question mix choice
                                                if (quest.MixChoice == true)
                                                {
                                                    Random rd = new Random();
                                                    int numOfAnswer = qAnswer.Count;
                                                    while (numOfAnswer > 1)
                                                    {
                                                        numOfAnswer--;
                                                        int k = rd.Next(numOfAnswer + 1);
                                                        var qaTemp = qAnswer[k];
                                                        qAnswer[k] = qAnswer[numOfAnswer];
                                                        qAnswer[numOfAnswer] = qaTemp;
                                                    }
                                                    quest.QuestionAnswerDones = qAnswer;
                                                }
                                                multipleQuestionsList.Add(quest);
                                                quizMark = quizMark + quest.Mark;
                                            }
                                            /* else if (quest.Qtype == 2)
                                             {
                                                 //if question mix choice
                                                 if (quest.MixChoice == true)
                                                 {
                                                     Random rd = new Random();
                                                     int numOfAnswer = qAnswer.Count;
                                                     while (numOfAnswer > 1)
                                                     {
                                                         numOfAnswer--;
                                                         int k = rd.Next(numOfAnswer + 1);
                                                         var qaTemp = qAnswer[k];
                                                         qAnswer[k] = qAnswer[numOfAnswer];
                                                         qAnswer[numOfAnswer] = qaTemp;
                                                     }
                                                     quest.QuestionAnswerDones = qAnswer;
                                                 }
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
                                                 quizMark = quizMark + quest.Mark;
                                             }*/
                                            else if (quest.Qtype == 3)
                                            {
                                                fillBlankQuestionsList.Add(quest);
                                                quizMark = quizMark + quest.Mark;
                                            }
                                            else if (quest.Qtype == 4)
                                            {
                                                shortAnswerQuestionsList.Add(quest);
                                                quizMark = quizMark + quest.Mark;
                                            }
                                            else if (quest.Qtype == 6)
                                            {
                                                indicateMistakeQuestionsList.Add(quest);
                                                quizMark = quizMark + quest.Mark;
                                            }
                                        }

                                    }
                                    //////////////////////////////////////
                                    /*string[] quizQuestions = getStudentQuizFromDB.ReceivedQuestions.Split(new char[] { ';' });
                                    *//*List<string> questionList = new List<string>();*//*

                                    Dictionary<int, string> questionSet = new Dictionary<int, string>();
                                    Dictionary<int, string> matchingSet = new Dictionary<int, string>();
                                    foreach (string questions in quizQuestions)
                                    {
                                        string[] questAndType = questions.Split(new char[] { '-' });
                                        int qType = int.Parse(questAndType[1]);
                                        if (qType == 5)
                                        {
                                            int mID = int.Parse(questAndType[0]);
                                            matchingSet.Add(mID, questAndType[1]);
                                        }
                                        else
                                        {
                                            int qID = int.Parse(questAndType[0]);
                                            questionSet.Add(qID, questAndType[1]);
                                        }

                                    }


                                    foreach (KeyValuePair<int, string> keyValuePair in questionSet)
                                    {
                                        var quest = db.QuestionDones.Find(keyValuePair.Key);
                                        List<QuestionAnswerDone> qAnswer = quest.QuestionAnswerDones.ToList();
                                         
                                        if (quest.Qtype == 1)
                                        {
                                            //if question mix choice
                                            if (quest.MixChoice == true)
                                            {
                                                Random rd = new Random();
                                                int numOfAnswer = qAnswer.Count;
                                                while (numOfAnswer > 1)
                                                {
                                                    numOfAnswer--;
                                                    int k = rd.Next(numOfAnswer + 1);
                                                    var qaTemp = qAnswer[k];
                                                    qAnswer[k] = qAnswer[numOfAnswer];
                                                    qAnswer[numOfAnswer] = qaTemp;
                                                }
                                                quest.QuestionAnswerDones = qAnswer;
                                            }
                                            multipleQuestionsList.Add(quest);
                                        }
                                        else if (quest.Qtype == 2)
                                        {
                                            //if question mix choice
                                            if (quest.MixChoice == true)
                                            {
                                                Random rd = new Random();
                                                int numOfAnswer = qAnswer.Count;
                                                while (numOfAnswer > 1)
                                                {
                                                    numOfAnswer--;
                                                    int k = rd.Next(numOfAnswer + 1);
                                                    var qaTemp = qAnswer[k];
                                                    qAnswer[k] = qAnswer[numOfAnswer];
                                                    qAnswer[numOfAnswer] = qaTemp;
                                                }
                                                quest.QuestionAnswerDones = qAnswer;
                                            }
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
                                }
                            }
                            else if (getStudentQuizFromDB.Status.Equals("Done"))
                            {
                                return Redirect("~/Student/Report/QuizReport?qzid=" + getStudentQuizFromDB.QuizDoneID);
                            }
                        }

                        DateTime now = DateTime.Now;
                        TimeSpan? totalTime = quiz.EndTime - now;
                        int? countDown = (int?)totalTime?.TotalSeconds;
                        ViewBag.CountDown = countDown;

                        ViewBag.Quiz = quiz;
                        ViewBag.MultipleQuestion = multipleQuestionsList;
                        ViewBag.FillBlankQuestion = fillBlankQuestionsList;
                        ViewBag.ShortAnswerQuestion = shortAnswerQuestionsList;
                        ViewBag.IndicateMistakeQuestion = indicateMistakeQuestionsList;
                        ViewBag.PassageList = passageList;
                        ViewBag.ReadingQuestion = readingQuestionsList;
                        ViewBag.MatchingQuestion = matchQuestionsList;
                        return View();
                    }
                }
            }

        }

        [NoCache]
        public ActionResult DoQuizQuestionByQuestion(string qzid)
        {
            //check if quiz is availble
            if (checkQuizIdEncodeAvailbile(qzid) == false)
            {
                return Redirect("~/Student/Home/Home");
            }
            else
            {
                ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
                ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
                qzid = Base64Decode(qzid);
                int quizId = int.Parse(qzid);
                var getQuiz = db.Quizs.Find(quizId);
                int countMatching = 0;
                if (getQuiz.Status.Equals("Not Done") || getQuiz.Status.Equals("Done"))
                {
                    return View("QuizNotStartYet");
                }
                else
                {
                    //if the quiz type is different
                    if (getQuiz.QuizType.Equals("ShowAllQuestion"))
                    {
                        return Redirect("~/Student/Home/Home");
                    }
                    else
                    {
                        //get quiz saved in database
                        var quiz = db.QuizDones.Where(qz => qz.QuizID == quizId).OrderByDescending(qz => qz.QuizDoneID).FirstOrDefault();

                        DateTime now = DateTime.Now;
                        if (now >= quiz.EndTime)
                        {
                            return View("QuizTimeOverDue");
                        }
                        else
                        {
                            int studentId = Convert.ToInt32(HttpContext.Session["StudentId"]);
                            Student_QuizDone getStudentQuizFromDB = db.Student_QuizDone.Where(sq => sq.StudentID == studentId && sq.QuizDoneID == quiz.QuizDoneID).FirstOrDefault();

                            //if student havent access to quiz
                            if (getStudentQuizFromDB == null)
                            {
                                double quizMark = 0;
                                List<QuestionDone> questionListForQuiz = new List<QuestionDone>();
                                List<QuestionDone> readingQuestionList = new List<QuestionDone>();
                                List<MatchQuestionDone> matchQuestionsList = new List<MatchQuestionDone>();
                                List<Passage_Done> passageList = new List<Passage_Done>();

                                //check if questions list is null
                                if (quiz.Questions != null && !quiz.Questions.Equals(""))
                                {
                                    //////////////////////////////////////
                                    string[] quizQuestions = quiz.Questions.Split(new char[] { ';' });
                                    List<string> questionList = new List<string>();
                                    //if quiz mix question
                                    if (quiz.MixQuestion == true)
                                    {
                                        Random rd = new Random();
                                        int numOfQuest = quizQuestions.Count();
                                        while (numOfQuest > 1)
                                        {
                                            numOfQuest--;
                                            int k = rd.Next(numOfQuest + 1);
                                            var qaTemp = quizQuestions[k];
                                            quizQuestions[k] = quizQuestions[numOfQuest];
                                            quizQuestions[numOfQuest] = qaTemp;
                                        }
                                    }
                                    //get random question in quiz
                                    if (quiz.MixQuestionNumber != null && quiz.MixQuestionNumber != 0)
                                    {
                                        List<int> addedQuestion = new List<int>();
                                        Random rd = new Random();
                                        for (int i = 0; i < quiz.MixQuestionNumber; i++)
                                        {
                                            int q = rd.Next(quizQuestions.Length); 
                                            while (addedQuestion.Contains(q))
                                            {
                                                q = rd.Next(quizQuestions.Length); 
                                            } 
                                            questionList.Add(quizQuestions[q]);
                                            addedQuestion.Add(q);

                                        } 


                                    }
                                    else
                                    {
                                        questionList = quizQuestions.ToList();
                                    }

                                    //////////////////////////////////////

                                    //get question list
                                    foreach (string questions in questionList)
                                    {
                                        string[] questAndType = questions.Split(new char[] { '-' });
                                        int qType = int.Parse(questAndType[1]);
                                        if (qType == 5)
                                        {
                                            countMatching++;
                                            int mID = int.Parse(questAndType[0]);
                                            /*matchingSet.Add(mID, questAndType[1]);*/
                                            var matchQuest = db.MatchQuestionDones.Find(mID);
                                            QuestionDone matching = new QuestionDone();
                                            matching.Q_DoneID = mID;
                                            matching.Text = matchQuest.ColumnA + "//" + matchQuest.ColumnB;
                                            matching.Time = matchQuest.Time;
                                            matching.Qtype = 5;
                                            quizMark = quizMark + matchQuest.Mark;
                                            questionListForQuiz.Add(matching);

                                            matchQuestionsList.Add(matchQuest);
                                        }
                                        else if (qType == 2)
                                        {
                                            int pID = int.Parse(questAndType[0]);
                                            var passage = db.Passage_Done.Find(pID);
                                            passageList.Add(passage);
                                            QuestionDone passageQuest = new QuestionDone();
                                            int time = 0;
                                            foreach (var q in passage.QuestionDones)
                                            {
                                                List<QuestionAnswerDone> qAnswer = q.QuestionAnswerDones.ToList();
                                                //if question mix choice
                                                if (q.MixChoice == true)
                                                {
                                                    Random rd = new Random();
                                                    int numOfAnswer = qAnswer.Count;
                                                    while (numOfAnswer > 1)
                                                    {
                                                        numOfAnswer--;
                                                        int k = rd.Next(numOfAnswer + 1);
                                                        var qaTemp = qAnswer[k];
                                                        qAnswer[k] = qAnswer[numOfAnswer];
                                                        qAnswer[numOfAnswer] = qaTemp;
                                                    }
                                                    q.QuestionAnswerDones = qAnswer;
                                                }
                                                readingQuestionList.Add(q);
                                                time = time + q.Time;
                                                quizMark = quizMark + q.Mark;
                                            }
                                            passageQuest.Q_DoneID = passage.P_DoneID;
                                            passageQuest.Time = time;
                                            passageQuest.Text = passage.Text;
                                            passageQuest.Qtype = 2;
                                            questionListForQuiz.Add(passageQuest);
                                        }
                                        else
                                        {
                                            int qID = int.Parse(questAndType[0]);
                                            QuestionDone question = db.QuestionDones.Find(qID);
                                            quizMark = quizMark + question.Mark;
                                            if (qType == 1 )
                                            {
                                                List<QuestionAnswerDone> qAnswer = question.QuestionAnswerDones.ToList();
                                                //if question mix choice
                                                if (question.MixChoice == true)
                                                {
                                                    Random rd = new Random();
                                                    int numOfAnswer = qAnswer.Count;
                                                    while (numOfAnswer > 1)
                                                    {
                                                        numOfAnswer--;
                                                        int k = rd.Next(numOfAnswer + 1);
                                                        var qaTemp = qAnswer[k];
                                                        qAnswer[k] = qAnswer[numOfAnswer];
                                                        qAnswer[numOfAnswer] = qaTemp;
                                                    }
                                                    question.QuestionAnswerDones = qAnswer;
                                                }

                                            }
                                            questionListForQuiz.Add(question);
                                        }

                                    }
                                    string receivedQuestions = "";
                                    foreach (string quest in questionList)
                                    {
                                        receivedQuestions = receivedQuestions + quest + ";";
                                    }
                                    Student_QuizDone studentQuizDone = new Student_QuizDone();
                                    studentQuizDone.StudentID = studentId;
                                    studentQuizDone.QuizDoneID = quiz.QuizDoneID;
                                    studentQuizDone.ReceivedQuestions = receivedQuestions.Substring(0, receivedQuestions.Length - 1);
                                    studentQuizDone.Status = "Doing";
                                    studentQuizDone.StudentMark = 0;
                                    studentQuizDone.TotalMark = quizMark;
                                    db.Student_QuizDone.Add(studentQuizDone);
                                    db.SaveChanges();
                                } 
                                ViewBag.Quiz = quiz;
                                ViewBag.QuestionList = questionListForQuiz;
                                ViewBag.CountMatching = countMatching;
                                ViewBag.ReadingQuestion = readingQuestionList;
                                ViewBag.PassageList = passageList;
                                ViewBag.MatchingQuestion = matchQuestionsList;

                                return View();
                            }
                            else
                            {
                                getStudentQuizFromDB.Status = "Done";
                                db.Entry(getStudentQuizFromDB).State = EntityState.Modified;
                                db.SaveChanges();
                                return Redirect("~/Student/Report/QuizReport?qzid=" + getStudentQuizFromDB.QuizDoneID);
                            }

                        }
                    }
                }
            }

        }

        //student press submit quiz
        [HttpPost]
        public ActionResult SubmitQuiz(FormCollection form, string qDoneID)
        {
            int sID = Convert.ToInt32(HttpContext.Session["StudentId"]);
            int qzDoneID = int.Parse(qDoneID);
            Student_QuizDone getStudentQuizFromDB = db.Student_QuizDone.Where(sq => sq.StudentID == sID && sq.QuizDoneID == qzDoneID).FirstOrDefault();

            if (!getStudentQuizFromDB.Status.Equals("Done"))
            {
                double studentMark = 0;
                string qListStr = "";
                ////////////////////////////////////////////////

                ////////////////////////////////////////////////

                string[] questionList = null;
                string[] matchingList = null;
                //get question id
                if (form["qid"] != null && !form["qid"].Trim().Equals(""))
                {
                    questionList = form["qid"].Split(new char[] { ',' });
                }

                if (form["mid"] != null && !form["mid"].Trim().Equals(""))
                {
                    matchingList = form["mid"].Split(new char[] { ',' });
                }


                List<QuestionDone> multipleQuestionsList = new List<QuestionDone>();
                List<QuestionDone> readingQuestionsList = new List<QuestionDone>();
                List<QuestionDone> fillBlankQuestionsList = new List<QuestionDone>();
                List<QuestionDone> shortAnswerQuestionsList = new List<QuestionDone>();
                List<QuestionDone> indicateMistakeQuestionsList = new List<QuestionDone>();
                List<MatchQuestionDone> matchQuestionsList = new List<MatchQuestionDone>();

                if (questionList != null)
                {
                    //get student question 
                    foreach (var q in questionList)
                    {
                        var question = db.QuestionDones.Find(int.Parse(q));
                        /*totalMark = totalMark + question.Mark;*/
                        //if question is multiple choice type
                        if (question.Qtype == 1)
                        {
                            multipleQuestionsList.Add(question);

                        }
                        else if (question.Qtype == 2)
                        {
                            readingQuestionsList.Add(question);

                        }
                        else if (question.Qtype == 3)
                        {
                            fillBlankQuestionsList.Add(question);

                        }
                        else if (question.Qtype == 4)
                        {
                            shortAnswerQuestionsList.Add(question);
                        }
                        else if (question.Qtype == 6)
                        {
                            indicateMistakeQuestionsList.Add(question);
                        }
                    }
                }

                if (matchingList != null)
                {
                    //get student matching question 
                    foreach (var m in matchingList)
                    {
                        var match = db.MatchQuestionDones.Find(int.Parse(m));
                        /*totalMark = totalMark + match.Mark;*/
                        matchQuestionsList.Add(match);
                    }
                }

                //add student answer of multiple question to db
                if (multipleQuestionsList != null)
                {
                    string[] cbAnswer = null;
                    //get mutiple choice answer
                    if (form["cbMultipleOption"] != null && !form["cbMultipleOption"].Trim().Equals(""))
                    {
                        cbAnswer = Request.Form.GetValues("cbMultipleOption");
                    }
                    foreach (var question in multipleQuestionsList)
                    {
                        question.StudentReceive = question.StudentReceive + 1;
                        

                        List<QuestionAnswerDone> correctAnswerList = db.QuestionAnswerDones.Where(qd => qd.QuestionID == question.Q_DoneID && qd.IsCorrect == true).ToList();
                        List<QuestionAnswerDone> choosenAnsList = new List<QuestionAnswerDone>();

                        //if student answer not null
                        if (cbAnswer != null)
                        {
                            //get student answer
                            foreach (var a in cbAnswer)
                            {
                                int ansID = int.Parse(a);
                                /*Debug.WriteLine(question.Q_DoneID + "cbbvlue2:" + ansID);*/
                                var answer = db.QuestionAnswerDones.Where(qa => qa.QuestionID == question.Q_DoneID && qa.QA_DoneID == ansID).FirstOrDefault();
                                if (answer != null)
                                {
                                    choosenAnsList.Add(answer);
                                    /*Debug.WriteLine(answer.Text + "--0-" + question.Q_DoneID);*/
                                }

                            }

                            int countCorrectAns = 0;
                            //if number of chosen option is different from correct answers
                            foreach (var choosenAns in choosenAnsList)
                            {
                                /*Debug.WriteLine(choosenAns.Text);*/
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 1;
                                studentChoice.Answer = choosenAns.Text;

                                //if answer is correct
                                if (choosenAns.IsCorrect == true)
                                {
                                    studentChoice.IsCorrect = true;
                                    countCorrectAns++;
                                    /*Debug.WriteLine("chooo:" + choosenAns.QA_DoneID + "//" + countCorrectAns);*/
                                }
                                else
                                {
                                    studentChoice.IsCorrect = false;
                                }
                                db.Student_Answer.Add(studentChoice);
                            }
                            //check if number of correct answer is equal as correct choosen answer
                            if (countCorrectAns == correctAnswerList.Count && correctAnswerList.Count == choosenAnsList.Count)
                            {
                                studentMark = studentMark + question.Mark;
                                question.CorrectNumber = question.CorrectNumber + 1;
                            }
                        }
                        db.Entry(question).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + question.Q_DoneID + "-1;";
                    }
                }

                //add student answer of reading question to db
                if (readingQuestionsList != null)
                {
                    foreach (var question in readingQuestionsList)
                    {
                        question.StudentReceive = question.StudentReceive + 1;
                        string[] cbAnswer = null;
                        //get mutiple choice answer
                        if (form["cbReadingOption"] != null && !form["cbReadingOption"].Trim().Equals(""))
                        {
                            cbAnswer = form["cbReadingOption"].Split(new char[] { ',' });
                        }

                        List<QuestionAnswerDone> correctAnswerList = db.QuestionAnswerDones.Where(qd => qd.QuestionID == question.Q_DoneID && qd.IsCorrect == true).ToList();
                        List<QuestionAnswerDone> choosenAnsList = new List<QuestionAnswerDone>();

                        //if student answer not null
                        if (cbAnswer != null)
                        {
                            //get student answer
                            foreach (var a in cbAnswer)
                            {
                                int ansID = int.Parse(a);
                                /*Debug.WriteLine(question.Q_DoneID + "cbbvlue2:" + ansID);*/
                                var answer = db.QuestionAnswerDones.Where(qa => qa.QuestionID == question.Q_DoneID && qa.QA_DoneID == ansID).FirstOrDefault();
                                if (answer != null)
                                {
                                    choosenAnsList.Add(answer);
                                    /*Debug.WriteLine(answer.Text + "--0-" + question.Q_DoneID);*/
                                }

                            }

                            int countCorrectAns = 0;
                            //if number of chosen option is different from correct answers
                            foreach (var choosenAns in choosenAnsList)
                            {
                                /*Debug.WriteLine(choosenAns.Text);*/
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 2;
                                studentChoice.Answer = choosenAns.Text;

                                //if answer is correct
                                if (choosenAns.IsCorrect == true)
                                {
                                    studentChoice.IsCorrect = true;
                                    countCorrectAns++;
                                    /*Debug.WriteLine("chooo:" + choosenAns.QA_DoneID + "//" + countCorrectAns);*/
                                }
                                else
                                {
                                    studentChoice.IsCorrect = false;
                                }
                                db.Student_Answer.Add(studentChoice);
                            }
                            //check if number of correct answer is equal as correct choosen answer
                            if (countCorrectAns == correctAnswerList.Count && correctAnswerList.Count == choosenAnsList.Count)
                            {
                                studentMark = studentMark + question.Mark;
                                question.CorrectNumber = question.CorrectNumber + 1;
                            }
                        }
                        db.Entry(question).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + question.Q_DoneID + "-2;";
                    }

                }

                //add student answer of fill blank question to db
                if (fillBlankQuestionsList != null)
                {
                    List<string> studentAnswerFillBlankNotGiven = null;
                    //get fillblank not given word answer
                    if (form["fillBankNotGivenAnswer"] != null && !form["fillBankNotGivenAnswer"].Trim().Equals(""))
                    {
                        /*studentAnswerFillBlankNotGiven = form["fillBankNotGivenAnswer"].Split(new char[] { ',' }).ToList();*/
                        studentAnswerFillBlankNotGiven = Request.Form.GetValues("fillBankNotGivenAnswer").ToList();


                    }

                    List<string> studentAnswerFillBlankGiven = null;
                    //get fillblank given word answer
                    if (form["fillBankGivenAnswer"] != null && !form["fillBankGivenAnswer"].Trim().Equals(""))
                    {
                        /*studentAnswerFillBlankGiven = form["fillBankGivenAnswer"].Split(new char[] { ',' }).ToList();*/
                        studentAnswerFillBlankGiven = Request.Form.GetValues("fillBankGivenAnswer").ToList();

                    }
                    foreach (var question in fillBlankQuestionsList)
                    {
                        question.StudentReceive = question.StudentReceive + 1;
                        //if this quesion doesn't have given word
                        if (question.GivenWord == false)
                        {
                            //check if student have answer
                            if (studentAnswerFillBlankNotGiven != null)
                            {
                                List<QuestionAnswerDone> correctAnswers = db.QuestionAnswerDones.Where(qa => qa.QuestionID == question.Q_DoneID).ToList();
                                int numOfCorrectAns = correctAnswers.Count;
                                int countCorrectAns = 0;
                                for (int i = 0; i < correctAnswers.Count; i++)
                                {
                                    Student_Answer studentChoice = new Student_Answer();
                                    studentChoice.QuizDoneID = qzDoneID;
                                    studentChoice.StudentID = sID;
                                    studentChoice.QuestionDoneID = question.Q_DoneID;
                                    studentChoice.Qtype = 3;
                                    studentChoice.Answer = studentAnswerFillBlankNotGiven[i];
                                    //check if answer is correct
                                    if (studentAnswerFillBlankNotGiven[i].Trim().ToLower().Equals(correctAnswers[i].Text.ToLower().Trim()))
                                    {
                                        studentChoice.IsCorrect = true;
                                        countCorrectAns++;
                                    }
                                    else
                                    {
                                        studentChoice.IsCorrect = false;
                                    }

                                    db.Student_Answer.Add(studentChoice);
                                }
                                for (int i = 0; i < correctAnswers.Count; i++)
                                {
                                    studentAnswerFillBlankNotGiven.RemoveAt(0);
                                }
                                //if the number of correct answer that student chose equal to number of correct answer of question
                                if (countCorrectAns == numOfCorrectAns)
                                {
                                    studentMark = studentMark + question.Mark;
                                    question.CorrectNumber = question.CorrectNumber + 1;
                                }
                            }
                            else
                            {
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 3;
                                studentChoice.Answer = "";
                                studentChoice.IsCorrect = false;
                                db.Student_Answer.Add(studentChoice);
                            }

                        }
                        else
                        {
                            //if student have answer
                            if (studentAnswerFillBlankGiven != null)
                            {
                                List<QuestionAnswerDone> correctAnswers = db.QuestionAnswerDones.Where(qa => qa.QuestionID == question.Q_DoneID).ToList();
                                int numOfCorrectAns = correctAnswers.Count;
                                int countCorrectAns = 0;
                                for (int i = 0; i < correctAnswers.Count; i++)
                                {
                                    Student_Answer studentChoice = new Student_Answer();
                                    studentChoice.QuizDoneID = qzDoneID;
                                    studentChoice.StudentID = sID;
                                    studentChoice.QuestionDoneID = question.Q_DoneID;
                                    studentChoice.Qtype = 3;
                                    studentChoice.Answer = studentAnswerFillBlankGiven[i];
                                    //check if answer is correct
                                    if (studentAnswerFillBlankGiven[i].Trim().ToLower().Equals(correctAnswers[i].Text.ToLower().Trim()))
                                    {
                                        studentChoice.IsCorrect = true;
                                        countCorrectAns++;
                                    }
                                    else
                                    {
                                        studentChoice.IsCorrect = false;
                                    }

                                    db.Student_Answer.Add(studentChoice);
                                }
                                for (int i = 0; i < correctAnswers.Count; i++)
                                {
                                    studentAnswerFillBlankGiven.RemoveAt(0);
                                }
                                //if the number of correct answer that student chose equal to number of correct answer of question
                                if (countCorrectAns == numOfCorrectAns)
                                {
                                    studentMark = studentMark + question.Mark;
                                    question.CorrectNumber = question.CorrectNumber + 1;
                                }
                            }
                            else
                            {
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 3;
                                studentChoice.Answer = "";
                                studentChoice.IsCorrect = false;
                                db.Student_Answer.Add(studentChoice);
                            }

                        }
                        db.Entry(question).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + question.Q_DoneID + "-3;";
                    }
                }

                //add student answer of short answer question to db
                if (shortAnswerQuestionsList != null)
                {
                    int countShortAnswerQuest = 0;
                    foreach (var question in shortAnswerQuestionsList)
                    {
                        question.StudentReceive = question.StudentReceive + 1;
                        countShortAnswerQuest++;
                        string[] answerList = null;
                        //get short answer question answer
                        if (form["txtshortAnswer"] != null && !form["txtshortAnswer"].Trim().Equals(""))
                        {

                            /*answerList = form["txtshortAnswer"].Split(new char[] { ',' });*/
                            answerList = Request.Form.GetValues("txtshortAnswer");
                        }
                        var correctAnswer = db.QuestionAnswerDones.Where(qd => qd.QuestionID == question.Q_DoneID && qd.IsCorrect == true).FirstOrDefault();

                        string[] correctAnswerList = correctAnswer.Text.Split(new char[] { '\\' });

                        if (answerList != null)
                        {
                            
                            var answer = answerList[countShortAnswerQuest - 1];
                            //if student answer not null
                            if (answer != null)
                            {
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 4;
                                studentChoice.Answer = answer;
                                studentChoice.IsCorrect = false;
                                foreach (var ans in correctAnswerList)
                                {
                                    //check if the answer is correct
                                    if (ans.Trim().ToLower().Equals(answer.Trim().ToLower()))
                                    {
                                        studentChoice.IsCorrect = true;
                                        studentMark = studentMark + question.Mark;
                                        question.CorrectNumber = question.CorrectNumber + 1;
                                    }
                                }
                                db.Student_Answer.Add(studentChoice);
                            }
                        }

                        db.Entry(question).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + question.Q_DoneID + "-4;";
                    }
                }

                //add student answer of indicate mistake question to db
                if (indicateMistakeQuestionsList != null)
                {
                    foreach (var question in indicateMistakeQuestionsList)
                    {
                        question.StudentReceive = question.StudentReceive + 1;
                        string[] cbAnswer = null;
                        //get mutiple choice answer
                        if (form["cbIndicateOption"] != null && !form["cbIndicateOption"].Trim().Equals(""))
                        {
                            cbAnswer = form["cbIndicateOption"].Split(new char[] { ',' });
                        }

                        List<QuestionAnswerDone> correctAnswerList = db.QuestionAnswerDones.Where(qd => qd.QuestionID == question.Q_DoneID && qd.IsCorrect == true).ToList();
                        List<QuestionAnswerDone> choosenAnsList = new List<QuestionAnswerDone>();

                        //if student answer not null
                        if (cbAnswer != null)
                        {
                            //get student answer
                            foreach (var a in cbAnswer)
                            {
                                int ansID = int.Parse(a); 
                                var answer = db.QuestionAnswerDones.Where(qa => qa.QuestionID == question.Q_DoneID && qa.QA_DoneID == ansID).FirstOrDefault();
                                if (answer != null)
                                {
                                    choosenAnsList.Add(answer); 

                                }

                            }

                            /*int countCorrectAns = 0;*/
                            //if number of chosen option is different from correct answers
                            foreach (var choosenAns in choosenAnsList)
                            { 
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = question.Q_DoneID;
                                studentChoice.Qtype = 6;
                                studentChoice.Answer = choosenAns.Text;

                                //if answer is correct
                                if (choosenAns.IsCorrect == true)
                                {
                                    studentChoice.IsCorrect = true;
                                    studentMark = studentMark + question.Mark;
                                    question.CorrectNumber = question.CorrectNumber + 1; 
                                }
                                else
                                {
                                    studentChoice.IsCorrect = false;
                                }
                                db.Student_Answer.Add(studentChoice);
                            }
                        }
                        db.Entry(question).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + question.Q_DoneID + "-6;";
                    }
                }

                //add student answer of matchib question to db
                if (matchQuestionsList != null)
                {
                    for (int i = 0; i < matchQuestionsList.Count; i++)
                    { 
                        matchQuestionsList[i].StudentReceive = matchQuestionsList[i].StudentReceive + 1;
                        List<string> solutionList = new List<string>(); ;
                        //get matching answer
                        if (form["solution"] != null && !form["solution"].Trim().Equals(""))
                        {
                            solutionList = form["solution"].Split(new char[] { '|' }).ToList();

                            string studentAnswerAddToDB = "";
                            bool isCorrect = false;
                            if (solutionList != null)
                            { 
                                int countCorrectAnswer = 0;
                                List<string> questionAnswer = matchQuestionsList[i].Solution.Split(new char[] { ';' }).ToList();
                                List<string> studentSolution = solutionList[i].Split(new char[] { ',' }).ToList();
                                int numberOfStudentSolution = studentSolution.Count;
                                
                                for (int j = 0; j < numberOfStudentSolution; j++)
                                {
                                    string checkAns = studentSolution[0]; 
                                    if (checkAns != null && !checkAns.Trim().Equals(""))
                                    {
                                        foreach (string correctAns in questionAnswer)
                                        {
                                            Debug.WriteLine(correctAns + "====: " + checkAns);
                                            if (checkAns.ToLower().Trim().Equals(correctAns.ToLower()))
                                            {
                                                countCorrectAnswer++;
                                            }

                                        }

                                        studentAnswerAddToDB = studentAnswerAddToDB + checkAns + ";";

                                    } 
                                    studentSolution.RemoveAt(0); 

                                }
                                 
                                if (countCorrectAnswer == questionAnswer.Count)
                                {
                                    isCorrect = true;
                                    studentMark = studentMark + matchQuestionsList[i].Mark;
                                    matchQuestionsList[i].CorrectNumber = matchQuestionsList[i].CorrectNumber + 1;

                                }


                            }
                            if (!studentAnswerAddToDB.Equals(""))
                            {
                                Student_Answer studentChoice = new Student_Answer();
                                studentChoice.QuizDoneID = qzDoneID;
                                studentChoice.StudentID = sID;
                                studentChoice.QuestionDoneID = matchQuestionsList[i].M_DoneID;
                                studentChoice.Qtype = 5;
                                studentChoice.Answer = studentAnswerAddToDB.Substring(0, studentAnswerAddToDB.Length - 1);
                                studentChoice.IsCorrect = isCorrect;
                                db.Student_Answer.Add(studentChoice);
                            }


                        }
                        db.Entry(matchQuestionsList[i]).State = EntityState.Modified;
                        db.SaveChanges();
                        qListStr = qListStr + matchQuestionsList[i].M_DoneID + "-5;";

                    }
                }

                /*Student_QuizDone report = new Student_QuizDone();*/
                /*getStudentQuizFromDB.StudentID = sID;
                getStudentQuizFromDB.QuizDoneID = qzDoneID;*/
                /*getStudentQuizFromDB.TotalMark = totalMark;*/
                getStudentQuizFromDB.StudentMark = studentMark;
                getStudentQuizFromDB.Status = "Done";
                /*getStudentQuizFromDB.ReceivedQuestions = qListStr.Substring(0, qListStr.Length - 1);*/
                /*db.Student_QuizDone.Add(report);*/
                db.Entry(getStudentQuizFromDB).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Redirect("~/Student/Report/QuizReport?qzid=" + qzDoneID);
        }
    }
}