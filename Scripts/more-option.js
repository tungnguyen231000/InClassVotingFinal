$(document).ready(function () {

    //===============Các hàm đánh số thứ tự====================


    function note2() {
        var count = document.querySelectorAll('#poll-option td span');
        for (var i = 0; i < count.length; i++) {
            count[i].innerHTML = i + 1;
        };
    }

    function note3() {
        var count = document.querySelectorAll('#reading-note');
        for (var i = 0; i < count.length; i++) {
            var x = (i + 1);
            count[i].innerHTML = 'Question ' + x + ':';
        };
    }

    //===============Phần Matching====================
    if ($('#matching-form').length != 0) {
        $("#matching-left").height(300);
        $("#matching-right").height(300);

        $('#previewMatching').click(function () {

            $("#matching-left-preview").val($("#matching-left").val());
            $("#matching-left-preview").height(300);

            $("#matching-right-preview").val($("#matching-right").val());
            $("#matching-right-preview").height(300);

            $("#table-solution-preview").html($("#table-solution").html());
        });
    }

    var matchingleft = document.querySelectorAll("#matching-left");
    var matchingright = document.querySelectorAll("#matching-right");
    matchingleft.forEach((item) => { $(item).height(300); });
    matchingright.forEach((item) => { $(item).height(300); });

    if ($("#matching-test").length == 0) {
        const regexLetter = /^[a-zA-Z]{1}$/;
        const regexNumber = /^[0-9]+$/;
        var addMatching = document.querySelectorAll('#add-matching');
        var numberMatching = document.querySelectorAll('#number');
        var letterMatching = document.querySelectorAll('#letter');
        var tableSolution = document.querySelectorAll('#table-solution');
        var removeMatching = document.querySelectorAll('#remove-matching');


        var checkListNumber = [];
        var checkListLetter = [];

        if (addMatching.length <= 1) {
            if ($('#matching-solution').length != 0) {
                var solutionTotal = $(tableSolution[0]).children();
                for (let i = 0; i < solutionTotal.length; i++) {
                    let x = $(solutionTotal[i]).val();
                    let index1 = x.split("-")[0];
                    let index2 = x.split("-")[1];

                    checkListNumber.push(index1.toLowerCase());
                    checkListLetter.push(index2.toLowerCase());

                }
            }
        }
        
        //Add ban đầu
        addMatching.forEach((item) => {
            $(item).click(function (e) {
                var z = e.target;

                var index = Array.prototype.indexOf.call(addMatching, z);

                if (addMatching.length > 1) {
                    if ($('#matching-solution').length != 0) {
                        checkListNumber = [];
                        checkListLetter = [];
                        var solutionTotal = $(tableSolution[index]).children();

                        for (let i = 0; i < solutionTotal.length; i++) {
                            let x = $(solutionTotal[i]).val();
                            let index1 = x.split("-")[0];
                            let index2 = x.split("-")[1];

                            checkListNumber.push(index1.toLowerCase());
                            checkListLetter.push(index2.toLowerCase());
                        }
                    }
                }
            
                $(".letterq-error").remove();
                $(".numberp-error").remove();
                $(".letterr-error").remove();
                $(".numbero-error").remove();
                
                if (regexLetter.test(letterMatching[index].value) == false) {
                    if ($(".letterq-error").length == 0) {
                        $(letterMatching[index]).after('<div class="letterq-error">*Only 1 character</div>');
                        $('.letterq-error').css("color", "red");
                        $('.letterq-error').css("font-weight", "bold");
                    }
                    return false;
                }
                $(".letterq-error").remove();
                


                if (regexNumber.test(numberMatching[index].value) == false) {
                    if ($(".numberp-error").length == 0) {
                        $(numberMatching[index]).after('<div class="numberp-error">*Positive number only</div>');
                        $('.numberp-error').css("color", "red");
                        $('.numberp-error').css("font-weight", "bold");
                    }
                    return false;
                }
                $(".numberp-error").remove();
                
                

                if (numberMatching[index].value != '' && letterMatching[index].value != '') {

                    if (checkListLetter.includes(letterMatching[index].value.toLowerCase())) {
                        if ($(".letterr-error").length == 0) {
                            $(letterMatching[index]).after('<div class="letterr-error">*Letter duplicate</div>');
                            $('.letterr-error').css("color", "red");
                            $('.letterr-error').css("font-weight", "bold");
                        }
                        return false;
                    }
                    $(".letterr-error").remove();
                    
                    if (checkListNumber.includes(numberMatching[index].value)) {
                        if ($(".numbero-error").length == 0) {
                            $(numberMatching[index]).after('<div class="numbero-error">*Number duplicate</div>');
                            $('.numbero-error').css("color", "red");
                            $('.numbero-error').css("font-weight", "bold");
                        }
                        return false;
                    }
                    $(".numbero-error").remove();
                    

                    checkListNumber.push(numberMatching[index].value.toLowerCase());
                    checkListLetter.push(letterMatching[index].value.toLowerCase());

                    var txtLine = '<input type="text" id="matching-solution" class="text-center" name="solution" value="' + numberMatching[index].value + '-' + letterMatching[index].value + '" readonly/>';

                    $(tableSolution[index]).append(txtLine);

                    if ($("#table-solution-preview").length != 0) {
                        $("#table-solution-preview").append(txtLine);
                    }
                }


                $(numberMatching[index]).val("");
                $(letterMatching[index]).val("");

                var listSolution = $(tableSolution[index]).children();
                
                for (var z = 0; z < listSolution.length; z++) {
                    listSolution[z].addEventListener('click', function (event) {

                       
                        if ($('.solution-active').length == 0) {
                            $(event.target).addClass("solution-active");
                        } else {
                            $('.solution-active').removeClass("solution-active");
                            $(event.target).addClass("solution-active");
                        }

                        $(removeMatching[index]).click(function (e) {
                            //event.target.remove();
                            
                            if ($("#table-solution-preview").length != 0) {
                                var listSolution = $("#table-solution-preview").children();
                                $("#table-solution-preview").html($("#table-solution").html());
                                $(listSolution[z]).remove();
                            }

                            if (document.querySelector('.solution-active') != null) {
                                let x = document.querySelector('.solution-active').value;

                                let index1 = Array.prototype.indexOf.call(checkListNumber, x.split("-")[0]);
                                let index2 = Array.prototype.indexOf.call(checkListLetter, x.split("-")[1]);
                                checkListNumber.splice(index1, 1);
                                checkListLetter.splice(index2, 1);
                                $('.solution-active').remove();
                            }
                           
                        });

                    });
                };

            });
        });
        //Xóa ban đầu 
        var listSolution = document.querySelectorAll('#matching-solution');
        if ($('#matching-solution').length != 0) {
            for (let z = 0; z < listSolution.length; z++) {
                listSolution[z].addEventListener('click', function (event) {
                    const parent = event.currentTarget.parentNode;

                    var index = Array.prototype.indexOf.call(tableSolution, parent);


                    if ($('.solution-active').length == 0) {
                        $(listSolution[z]).addClass("solution-active");
                    } else {
                        $('.solution-active').removeClass("solution-active");
                        $(listSolution[z]).addClass("solution-active");
                    }

                    $(removeMatching[index]).click(function (e) {
                       
                        
                        if ($("#table-solution-preview").length != 0) {
                            var listSolution = $("#table-solution-preview").children();
                            $("#table-solution-preview").html($("#table-solution").html());
                            $(listSolution[z]).remove();
                        }

                        if (document.querySelector('.solution-active') != null) {
                            let x = document.querySelector('.solution-active').value;

                            let index1 = Array.prototype.indexOf.call(checkListNumber, x.split("-")[0]);
                            let index2 = Array.prototype.indexOf.call(checkListLetter, x.split("-")[1]);
                            checkListNumber.splice(index1, 1);
                            checkListLetter.splice(index2, 1);

                            $('.solution-active').remove();
                        }
                    });

                });
            };
        }
        
    }

    //===============Phần Poll====================
    $('.option-btn').click(function () {

        var txtLine2 = '<tr id="poll-option">' +
            '<td><span></span></td>' +
            '<td class="w-full"><input type="text" id="txtPoll" class="w-full p-3 mt-2 option" name="option" placeholder="..."></td>' +
            '<td class="w-2"><i id="remove-option-poll" class="far fa-times-circle my-3 p-2"></i></td>' +
            '</tr>';
        $('#table-poll').append(txtLine2);

        note2();

        var remove = document.querySelectorAll('#remove-option-poll');

        //Không cho xóa dưới 1
        if (remove.length == 1) {
            $('#remove-option-poll').hide();
        } else {
            $('#remove-option-poll').show();
        }


        remove[remove.length - 1].addEventListener("click", function (e) {
            var z = e.target;
            var list = document.querySelectorAll('#poll-option');
            remove = document.querySelectorAll('#remove-option-poll');

            var index = Array.prototype.indexOf.call(remove, z);


            list[index].remove();
            note2();


            remove = document.querySelectorAll('#remove-option-poll');

            //Không cho xóa dưới 1
            var removecheck = document.querySelectorAll('#remove-option-poll');
            if (removecheck.length <= 1) {
                $('#remove-option-poll').hide();
            } else {
                $('#remove-option-poll').show();
            }
        });

    });
    //Xóa Poll ban đầu
    var remove = document.querySelectorAll('#remove-option-poll');

    remove.forEach((item) => {
        item.addEventListener("click", function (e) {
            var z = e.target;
            var list = document.querySelectorAll('#poll-option');

            var index = Array.prototype.indexOf.call(remove, z);



            list[index].remove();

            note2();

            remove = document.querySelectorAll('#remove-option-poll');

            //Không cho xóa dưới 1
            var removecheck = document.querySelectorAll('#remove-option-poll');
            if (removecheck.length <= 1) {
                $('#remove-option-poll').hide();
            } else {
                $('#remove-option-poll').show();
            }

        });
    });
    //===============End Phần Poll====================

    //===============Phần Reading====================
    $('#more-question-reading').click(function () {
        //Thêm Câu hỏi vào Reading
        var txtLine4 = '<div id="reading-question" class="w-full items-center bg-gray p-6 border border-black-400">' +
            '<div class="ms-error"></div>' +
            '<div class="m-2">' +
            '<i id="remove-question-reading" class="fas fa-times delete h4 float-right"></i>' +
            '</div>' +
            '<div><span id="reading-note" class="question-num-2" style="margin-right:20px">Question 1:</span></div>' +
            '<div class="mx-4 mt-2">' +
            '<textarea class="p-2 w-full border border-black-400" id="questionReading" name="question" rows="4" placeholder=" ..."></textarea>' +
            '</div>' +
            '<div class="mx-4 mt-10">'+
            '<span class="font-bold">Answer:</span>'+
            '</div>'+
            '<table id="table-reading" class="w-full border-spacing-2">' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            ' <td> A.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            ' <td> B.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            ' <td> C.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            ' <td> D.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            ' <td> E.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '<tr id="reading-option">' +
            '<td class="w-2"><input id="optionReading-checkbox" type="checkbox" name="cbOption" value="1"></td>' +
            '<input type="hidden" name="cboption" value="0">' +
            '<td> F.&nbsp;&nbsp;<input style="width:95%" id="optionReading-text" type="text" class="w-full pl-3 border border-black-400" name="option" placeholder="...."></td>' +
            '</tr>' +
            '</table>' +
            '<div class="mx-4 div-margin-20">' +
            '<input type="checkbox" class="form-check-input" id="cb-option2" name="mixChoice" value="1" style="margin-top:6px">' +
            '<input type="hidden" name="mixChoice" value="0">' +
            '<span class="label-mode-4"> Shuffle Answer</span>' +
            '</div>' +
            '<div class="flex">' +
            '<div class="m-6">' +
            '<span class="label-mode-4">Time (second): </span>' +
            '<input id="timeReading" class="border border-black-400 pl-1 w-32" min="15" max="3600" step="1" type="number" name="time" placeholder="...">' +
            '</div>' +
            '<div class="m-6 ">' +
            '<span class="label-mode-4">Mark: </span>' +
            '<input id="markReading" class="border border-black-400 pl-1 w-32" min="0.5" max="10" step="0.5" type="number" name="mark" placeholder="...">' +
            '</div>'+
            '</div>'+
            '</div>';

        //Thêm Câu hỏi vào Reading
        $('#table-question-reading').append(txtLine4);

        $('#remove-question-reading').show();

        //Dánh index câu
        note3();

        //Remove Question sau khi add thêm
        var removeQuestion = document.querySelectorAll('#remove-question-reading');
        removeQuestion[removeQuestion.length-1].addEventListener('click', function (e) {
                var z = e.currentTarget;
                var list = document.querySelectorAll('#reading-question');

                removeQuestion = document.querySelectorAll('#remove-question-reading');
                


                var index = Array.prototype.indexOf.call(removeQuestion, z);

                list[index].remove();

                removeQuestion = document.querySelectorAll('#remove-question-reading');

                //Không cho xóa dưới 1
                if (removeQuestion.length == 1) {
                    $('#remove-question-reading').hide();
                } else {
                    $('#remove-question-reading').show();
                }

                note3();

        });
        

    });

    if ($('#reading-form').length != 0) {

        //Xoá Câu Hỏi Ban dau
        var removeQuestion = document.querySelectorAll('#remove-question-reading');

        if (removeQuestion.length == 1) {
            $('#remove-question-reading').hide();
        } else {
            $('#remove-question-reading').show();
        }

        removeQuestion.forEach((item) => {
            item.addEventListener('click', function (e) {
                var z = e.currentTarget;
                var list = document.querySelectorAll('#reading-question');
                
                removeQuestion = document.querySelectorAll('#remove-question-reading');
                
 

                var index = Array.prototype.indexOf.call(removeQuestion, z);

                list[index].remove();

                removeQuestion = document.querySelectorAll('#remove-question-reading');

                //Không cho xóa dưới 1
                if (removeQuestion.length == 1) {
                    $('#remove-question-reading').hide();
                } else {
                    $('#remove-question-reading').show();
                }

                note3();

            });
        });

        //Xoá option ban đầu- fix cung roi thi khong can
        var removeOption = document.querySelectorAll('#remove-option-reading');
        removeOption.forEach((item) => {
            item.addEventListener('click', function () {
                var list = document.querySelectorAll('#reading-option');

                for (var index = 0; index < list.length; index++) {
                    list[index].querySelector("#remove-option-reading").addEventListener("click",
                        function () {
                            this.closest("#reading-option").remove();
                        });
                }

            });
        });

    }


});