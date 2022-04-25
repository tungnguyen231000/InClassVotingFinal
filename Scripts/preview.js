$(document).ready(function () {

    

    
    const alphabet = ["A.", "B.", "C.", "D.", "E.", "F.", "G.", "H.", "I.", "J.", "K.", "L.", "M.", "N.", "O.", "P.", "Q.", "R.", "S.", "T.", "U.", "V.", "W.", "X.", "Y.", "Z."];
    
    //=========Multiple Choice PreView=========
    $('#previewMultiple').click(function () {
        

        const getText = document.querySelectorAll('#optionMultiple-text');
        const getQuestion = document.querySelector('#questionMultiple').value;
        var indexAlphabet = 0;

        var item = '';

        for (var i = 0; i < getText.length; i++) {
            if (getText[i].value.trim() != '') {

                item +=     '<tr>' +
                                '<td class="w-2"><input type="checkbox" id="multiple-q1" name="cb-option1" value="1" disabled></td>' +
                                '<td class="w-2">' + alphabet[indexAlphabet] + '</td><td><label>' + getText[i].value + '</label></td>' +
                            '</tr>';

                ++indexAlphabet;
                $('#table-multiple-preview').html(item);
            }
        }

        $('#questionMultiple-preview').val(getQuestion);

        if ($(".q1-preview").val().trim() != "") {
            $(".q2-preview").show();
            if ($(".q1-preview")[0].scrollHeight > 232) {
                $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
            }
        } else {
            $(".q2-preview").hide();
        }
        
        
    });
    //=========End Multiple Choice PreView=========

    //=========Reading PreView=========
    $('#previewReading').click(function () {
        

        const getParagraph = document.querySelector('#paragraphReading').value;
        const getQuestion = document.querySelectorAll('#questionReading');
        const getListOption = document.querySelectorAll('#table-reading');
        var item1 = '';
        var indexQuestion = 0;
        //Add số Question tương ứng
        for (var i = 0; i < getQuestion.length; i++) {

            if (getQuestion[i].value.trim() != "") {

                var listText = getListOption[i].querySelectorAll('#optionReading-text');
                var indexAlphabet = 0;

                item1 += '<div id="reading-test-preview">' +
                    '<div class="w-full items-center bg-gray p-6 font-bold">' +
                    '<span>Question ' + (indexQuestion + 1) + ':</span>' +
                    '</div>' +
                    '<div class="w-full items-center bg-gray ">' +
                    '<div class="">' +
                    '<textarea id="questionReading-preview" class="w-full items-center bg-gray px-6 " readonly>' + getQuestion[i].value +
                    '</textarea>' +
                    '</div>' +
                    '</div>' +
                    '<div class="w-full items-center bg-gray ">' +
                    '<table id="table-reading-preview" class="w-full border-spacing-2">';

                ++indexQuestion;

                for(var z = 0; z < listText.length; z++) {
                    if (listText[z].value.trim() != '') {
                        item1 += '<tr>' +
                            '<td class="w-2"><input type="checkbox" id="multiple-q1" name="cb-option1" value="1" disabled></td>' +
                            '<td class="w-2">' + alphabet[indexAlphabet] + '</td>' +
                            '<td><label>' + listText[z].value + '</label></td>' +
                            '</tr>';

                        ++indexAlphabet;
                    }
                }

                item1 += '</table>' +
                    '</div>' +
                    '</div>';
            }
        }

        $('#table-reading-question-preview').html(item1);
        $('#paragraphReading-preview').val(getParagraph);
        if ($(".q1-preview").val().trim() != "") {
            $(".q2-preview").show();
            if ($(".q1-preview")[0].scrollHeight > 232) {
                $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
            }
        } else {
            $(".q2-preview").hide();
        }
    });

    //=========End Reading PreView=========

    //=========Short PreView=========
    if ($('#previewShort').length != 0) {
        $('#previewShort').click(function () {

            const getQuestion = document.querySelector('#questionShort').value;

            

          

            $('#questionShort-preview').val(getQuestion.trim());

            //Hidden
            //$('#answerShort-preview').val($('#answerShort').val());

            if ($(".q1-preview").val().trim() != "") {
                $(".q2-preview").show();
                if ($(".q1-preview")[0].scrollHeight > 232) {
                    $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
                }
            } else {
                $(".q2-preview").hide();
            }

        });
    }
    //=========End Short PreView=========

    //=========Matching PreView=========

    $('#previewMatching').click(function () {
        $("#matching-left-preview").val($("#matching-left"));
        $("#matching-right-preview").val($("#matching-right"));

        $("#matching-left-preview").height($("#matching-left").height());
        $("#matching-right-preview").height($("#matching-right").height());
    });
    //=========End Matching PreView=========
    
    //=========Indicate PreView=========
    if ($('#previewIndicate').length != 0) {
        $('#previewIndicate').click(function () {

            var getQuestion = $('#questionIndicate').val();
            var listOption = [];
            var listAnswer = [];

            var regexIndicateQuestion = /\([aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,.\-\+\=\*\/ ]+\)/g;
            var list = getQuestion.match(regexIndicateQuestion);

            var list2 = [];
            if (list != null) {
                for (let i = 0; i < list.length; i++) {
                    let xxx = list[i];

                    xxx = xxx.replace(")", "");
                    xxx = xxx.replace("(", "");

                    list2.push('<u>' + xxx + '</u>');
                }

                for (let i = 0; i < list.length; i++) {
                    let currentString = list[i];
                    getQuestion = getQuestion.replace(currentString, list2[i]);
                }
            }
            $('#questionIndicate-preview').html(getQuestion);

            var indexAlphabet = 0;

            var item = '';

            for (var i = 0; i < list.length; i++) {
                if (list[i].value != '') {
                    let xxx = list[i];

                    xxx = xxx.replace(")", "");
                    xxx = xxx.replace("(", "");

                    item += '<tr>' +
                        '<td class="w-2"><input type="checkbox" id="multiple-q1" name="cb-option1" value="1" disabled></td>' +
                        '<td class="w-2">' + alphabet[indexAlphabet] + '</td><td><span>' + xxx + '</span></td>' +
                        '</tr>';

                    ++indexAlphabet;
                }
            }
            $('#table-indicate-preview').html(item);

            if ($(".q1-preview").val().trim() != "") {
                $(".q2-preview").show();
                if ($(".q1-preview")[0].scrollHeight > 232) {
                    $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
                }
            } else {
                $(".q2-preview").hide();
            }
        });
    }
    //=========End Indicate PreView=========

    //=========Fill Blank PreView=========
    if ($('#previewFill').length != 0) {
        $('#previewFill').click(function () {

            const regexGroup = /\(\~[^\(\)]+\)/g;
            var fillText = $("#questionFill").val();
            var arrGroup = fillText.match(regexGroup);

            if ($("#givenFill").prop("checked") == true) {
                var arrGroup3 = fillText.match(regexGroup);

                if (arrGroup != null) {
                    for (var i = 0; i < arrGroup.length; i++) {
                        let selectString = '<select id="" style="color: blue" name="fillBankGivenAnswer" class="border border-black-400 w-2/12">' + '<option label=""></option>';

                        arrGroup[i] = arrGroup[i].replace("(", "");
                        arrGroup[i] = arrGroup[i].replace(")", "");
                        var listOption = arrGroup[i].match(/\~[^,.=]{0,1}[aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,. ]+/g);
                        var listAnswer = arrGroup[i].match(/\~=[^,.]{0,1}[aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,. ]+/g);
                        if (listOption != null) {
                            for (let j = 0; j < listOption.length; j++) {
                                selectString += '<option label="">' + listOption[j].replace("~", "").trim() + '</option>';
                            }
                        }
                        if (listAnswer != null) {
                            for (let z = 0; z < listAnswer.length; z++) {
                                selectString += '<option label="">' + listAnswer[z].replace("~=", "").trim() + '</option>';
                            }
                        }
                        
                        selectString += '</select>';

                        fillText = fillText.replace(arrGroup3[i], selectString);
                    }
                }

                $('#questionFill-preview').html(fillText);

            }
            if ($("#givenFill").prop("checked") == false) {

                const regexGroup2 = /\([^\(\)]+\)/g;
                var arrGroup2 = fillText.match(regexGroup2);
                if (arrGroup2 != null) {
                    for (var i = 0; i < arrGroup2.length; i++) {
                        let selectString = '<input type="text" id="" value="" style="width: 15%; color:blue" class="fill-input border border-black-400" name="fillBankNotGivenAnswer" placeholder="..." disabled>';
                        fillText = fillText.replace(arrGroup2[i], selectString);
                    }
                }

                

                $('#questionFill-preview').html(fillText);

            }
            if ($(".q1-preview").val().trim() != "") {
                $(".q2-preview").show();
                if ($(".q1-preview")[0].scrollHeight > 232) {
                    $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
                }
            } else {
                $(".q2-preview").hide();
            }
        });
    }
    //=========End Fill Blank PreView=========


    //=========Poll PreView=========
    $('#previewPoll').click(function () {

        const getText = document.querySelectorAll('#txtPoll');
        const getQuestion = document.querySelector('#questionPoll').value;

        var item = '';

        for (var i = 0; i < getText.length; i++) {
            if (getText[i].value.trim() != '') {

                item += '<tr>' +
                    '<td class="w-2"><input type="checkbox"  disabled></td>' +
                    '<td><label>' + getText[i].value + '</label>' +
                    '</tr>';

                $('#table-poll-preview').html(item);
            }
        }

        $('#questionPoll-preview').val(getQuestion);
        if ($(".q1-preview").val().trim() != "") {
            $(".q2-preview").show();
            if ($(".q1-preview")[0].scrollHeight > 232) {
                $(".q2-preview").height($(".q1-preview")[0].scrollHeight);
            }
        } else {
            $(".q2-preview").hide();
        }

    });
    //=========End Poll PreView=========
});