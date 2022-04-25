function isCheckedMixQ() {
    if (document.getElementById("rdQuestion").checked) {
        document.getElementById("rdQuestionNum").style.display = "block";
    } else {
        document.getElementById("rdQuestionNum").style.display = "none";
    }
    saveChanges();
}
function checkMask() {
    if (document.getElementById("checkBoxMask").checked == false) {
        document.getElementById("checkBoxAnswer").checked = false;
    }
    saveChanges();
}
function checkAnswer() {
    if (document.getElementById("checkBoxAnswer").checked) {
        document.getElementById("checkBoxMask").checked = true;
    }
    saveChanges();
}

/*var quizLink = document.getElementById("quizLink").value;
if (quizLink.indexOf("DoQuizPaperTest") == -1) {
    document.getElementById("qQuiz").checked = "true";
} else {
    document.getElementById("papeQuiz").checked = "true";
}*/

function changeLinkqQuiz() {
    let link = document.getElementById("linkQ").innerHTML;
    /*document.getElementById("linkQ").innerHTML = link.replace("DoQuizPaperTest", "DoQuizQuestionByQuestion");
    document.getElementById("quizLink").value = link.replace("DoQuizPaperTest", "DoQuizQuestionByQuestion");*/
    let quizType = "ShowQuestionByQuestion";
    document.getElementById("qtype").value = quizType;
    document.getElementById("qtypeChange").value = quizType;

}

function changeLinkpQuiz() {
    let link = document.getElementById("linkQ").innerHTML;
   /* document.getElementById("linkQ").innerHTML = link.replace("DoQuizQuestionByQuestion", "DoQuizPaperTest");
    document.getElementById("quizLink").value = link.replace("DoQuizQuestionByQuestion", "DoQuizPaperTest");*/
    let quizType = "ShowAllQuestion";
    document.getElementById("qtype").value = quizType;
    document.getElementById("qtypeChange").value = quizType;

    
}

$(".rdQuestionNum").keyup(function () {
    saveChanges();
});

$(".rdQuestionNum").change(function () {
    saveChanges();
});

function copyLink() {
    var copyText = document.getElementById("quizLink");

    copyText.select();
    navigator.clipboard.writeText(copyText.value);

}


function getNewQuizInfo() {
    var newName = document.getElementById("newName").value;
    document.getElementById("quizName").value = newName;
    var checkedQuizMode = null;
    var listCheckBox = document.getElementsByClassName('rdQuizMode');
    for (var i = 0; listCheckBox[i]; ++i) {
        if (listCheckBox[i].checked) {
            checkedQuizMode = listCheckBox[i].value;
            break;
        }
    }

    document.getElementById("quizMode").value = checkedQuizMode;
}

function getTempQuizInfo() {
    var tempName = document.getElementById("newName").value;
    document.getElementById("tempName").value = tempName;
    var checkedTempMod = null;
    var listCheckBox = document.getElementsByClassName('rdQuizMode');
    for (var i = 0; listCheckBox[i]; ++i) {
        if (listCheckBox[i].checked) {
            checkedTempMod = listCheckBox[i].value;
            break;
        }
    }
    document.getElementById("tempMode").value = checkedTempMod;
}

function saveChanges() {
    if ($('#saveOption').length != 0) {
        document.getElementById("saveOption").hidden = false;
    }
    
}


function isCheckedMixNewQuiz() {
    if (document.getElementById("rdQuestion").checked) {
        document.getElementById("rdQuestionNum").style.display = "block";
    } else {
        document.getElementById("rdQuestionNum").style.display = "none";
    }
}
function checkMaskNewQuiz() {
    if (document.getElementById("checkBoxMask").checked == false) {
        document.getElementById("checkBoxAnswer").checked = false;
    }
}
function checkAnswerNewQuiz() {
    if (document.getElementById("checkBoxAnswer").checked) {
        document.getElementById("checkBoxMask").checked = true;
    }
}


$(document).ready(function () {
    if ($('.btn-start-quiz').length != 0) {
        $('.btn-start-quiz').prop('disabled', true);
        $('.btn-start-quiz').before('<div class="quiz-ms" style="color:brown">*Copy link to start quiz</div>');
        $('.btn-copy-quiz').click(function () {
            $('.btn-start-quiz').prop('disabled', false);
            $('.quiz-ms').remove();
        });
    }


    if ($('#createQuiz').length != 0) {
        var quizName = document.querySelector('#newName');
        var courseIDNew = document.querySelector('#cidNewQuiz');
        var createQuizbtn = document.querySelector('#confirmCreate');

        $('#createQuiz').click(function (e) {
            $('.image-error').remove();
            $('.image-error-2').remove();
            var newName = $(quizName).val();
            var cidCreate = $(courseIDNew).val();
            $.ajax({
                url: '/Teacher/Quiz/CheckDuplicateQuizName',
                dataType: "json",
                data: { text: newName, cid: cidCreate },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error').length == 0) {
                            $(createQuizbtn).before('<div class="image-error">*' + data.messBtn + '</div>');
                            $('.image-error').css("color", "red");
                            $('.image-error').css("font-weight", "bold");

                        }
                        if ($('.image-error-2').length == 0) {
                            $(quizName).after('<div class="image-error-2">*' + data.mess + '</div>');
                            $('.image-error-2').css("color", "red");
                            $('.image-error-2').css("font-weight", "bold");
                            $('.image-error-2').css("margin-top", "12px");
                            $('.image-error-2').css("margin-left", "8px");

                        }
                    }
                    if (data.check == "1") {
                        $('.image-error').remove();
                        $('.image-error-2').remove();
                    }

                    $('#form-create-quiz').submit();



                },
                error: function (xhr) {
                    alert('Error by Quiz');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });
        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-create-quiz').submit(function (e) {
            if ($('.image-error').length != 0 || $('.image-error-2').length != 0) {
                e.preventDefault();
                return false;
            }
        });

    }

    //check edit chapter
    if ($('#editQuizName').length != 0) {

        var newQuizName = document.querySelector('#newQuizName');
        var courseIDEditQuiz = document.querySelector('#cidEditQuizName');
        var quizIdToEdit = document.querySelector('#quizIdUpdate');

        $('#editQuizName').click(function (e) {
            $('.image-error-4').remove();
            var quizNameEdit = $(newQuizName).val();
            var courseIDToUpdate = $(courseIDEditQuiz).val();
            var quizIDToUpdate = $(quizIdToEdit).val();
            $.ajax({
                url: '/Teacher/Quiz/CheckEditQuizName',
                dataType: "json",
                data: { text: quizNameEdit, cid: courseIDToUpdate, qzid: quizIDToUpdate },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error-4').length == 0) {
                            $(newQuizName).after('<div class="image-error-4">*' + data.mess + '</div>');
                            $('.image-error-4').css("color", "red");
                            $('.image-error-4').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error-4').remove();
                    }

                    $('#form-edit-quizname').submit();


                },
                error: function (xhr) {
                    alert('Error by edit quiz name');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });

        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-edit-quizname').submit(function (e) {
            if ($('.image-error-4').length != 0) {
                e.preventDefault();
                return false;
            }
        });
    }



});