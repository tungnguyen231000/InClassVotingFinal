$(document).ready(function () {
    //Check Student Checked Test
    var x = document.querySelectorAll('input[type="checkbox"]');
    x.forEach((item) => {
        item.addEventListener("change", () => {
            var z = item.getAttribute('id');
            var y = document.querySelectorAll('#' + z);
            var checked = 0;
            y.forEach((item2) => {
                if (item2.checked == true) {
                    ++checked;
                }
            });

            if (checked > 0) {
                var a = document.querySelector('button[data-option="' + z + '"]');
                a.classList.remove("bg-gray-200");
            } else {
                var a = document.querySelector('button[data-option="' + z + '"]');
                a.classList.add("bg-gray-200");
            }

        });
    });

    //Check Student Input Test
    var textbox = document.querySelectorAll('.txt');
    textbox.forEach((item) => {
        item.addEventListener("keyup", () => {
            var z = item.getAttribute('id');
            var y = document.querySelector('#' + z);
            var vvv = $('#' + z).val().length;
            var checked = 0;
            if (vvv > 0) {
                ++checked;
            };

            if (checked > 0) {
                var a = document.querySelector('button[data-option="' + z + '"]');
                a.classList.remove("bg-gray-200");
            } else {
                var a = document.querySelector('button[data-option="' + z + '"]');
                a.classList.add("bg-gray-200");
            }

        });
    });

    //Check Student Change Matching Test
    if ($('#matching-form').length != 0) {
        $("textarea").each(function () {
            this.setAttribute("style", "height:" + (this.scrollHeight) + "px;overflow-y:hidden;");
        }).on("input", function () {
            this.style.height = "auto";
            this.style.height = (this.scrollHeight) + "px";
        });
    }

    var matchingleft = document.querySelectorAll("#matching-left");
    var matchingright = document.querySelectorAll("#matching-right");
    matchingleft.forEach((item) => { $(item).height(300); });
    matchingright.forEach((item) => { $(item).height(300); });

    var addMatching = document.querySelectorAll('#add-matching');
    var numberMatching = document.querySelectorAll('#number');
    var letterMatching = document.querySelectorAll('#letter');
    var tableSolution = document.querySelectorAll('#table-solution');
    var removeMatching = document.querySelectorAll('#remove-matching');

    const regexLetter = /^[a-zA-Z]{1}$/;
    const regexNumber = /^[0-9]+$/;

    

    var removePosition = 0;

    addMatching.forEach((item) => {
        $(item).click(function (e) {

            var checkListNumber = [];
            var checkListLetter = [];

            var z = e.target;
            var parentDiv = z.parentNode;
            var a = document.querySelector('button[data-option="' + parentDiv.id + '"]');
            

            var index = Array.prototype.indexOf.call(addMatching, z);

            var listSolution = $(tableSolution[index]).children();

            if (listSolution.length != 0) {
                for (let i = 0; i < listSolution.length; i++) {
                    let x = $(listSolution[i]).val();
                    let index1 = x.split("-")[0];
                    let index2 = x.split("-")[1];

                    checkListNumber.push(index1.toLowerCase());
                    checkListLetter.push(index2.toLowerCase());
                }
            }

            if (numberMatching[index].value != '' && letterMatching[index].value != '') {
                $(".letter2-error").remove();
                $(".letter1-error").remove();
                $(".number2-error").remove();
                $(".number1-error").remove();

                if (regexLetter.test(letterMatching[index].value) == false) {
                    if ($(".letter1-error").length == 0) {
                        $(letterMatching[index]).after('<div class="letter1-error">*Only 1 character</div>');
                        $('.letter1-error').css("color", "red");
                        $('.letter1-error').css("font-weight", "bold");
                    }
                    return false;
                } 
                $(".letter1-error").remove();
                


                if (regexNumber.test(numberMatching[index].value) == false) {
                    if ($(".number2-error").length == 0) {
                        $(numberMatching[index]).after('<div class="number2-error">*Positive number only</div>');
                        $('.number2-error').css("color", "red");
                        $('.number2-error').css("font-weight", "bold");
                    }
                    return false;
                }
                $(".number2-error").remove();
                

                if (checkListLetter.includes(letterMatching[index].value.toLowerCase())) {
                    if ($(".letter2-error").length == 0) {
                        $(letterMatching[index]).after('<div class="letter2-error">*Letter duplicate</div>');
                        $('.letter2-error').css("color", "red");
                        $('.letter2-error').css("font-weight", "bold");
                    }
                    return false;
                } 
                $(".letter2-error").remove();
                
                if (checkListNumber.includes(numberMatching[index].value.toLowerCase())) {
                    if ($(".number1-error").length == 0) {
                        $(numberMatching[index]).after('<div class="number1-error">*Number duplicate</div>');
                        $('.number1-error').css("color", "red");
                        $('.number1-error').css("font-weight", "bold");
                    }
                    return false;
                }
                $(".number1-error").remove();
                

                checkListNumber.push(numberMatching[index].value.toLowerCase());
                checkListLetter.push(letterMatching[index].value.toLowerCase());

                var txtLine = '<input type="text" id="matching-solution" class="text-center" name="solution" value="' + numberMatching[index].value + '-' + letterMatching[index].value + '" readonly/>';
                a.classList.remove("bg-gray-200");
                $(tableSolution[index]).append(txtLine);
            }

            $(numberMatching[index]).val("");
            $(letterMatching[index]).val("");

            listSolution = $(tableSolution[index]).children();


            for (var z = 0; z < listSolution.length; z++) {
                listSolution[z].addEventListener('click', function (event) {
                    let check1 = Array.prototype.indexOf.call(tableSolution, event.currentTarget.parentNode);
                    removePosition = check1;

                    if ($('.solution-active').length == 0) {
                        $(event.target).addClass("solution-active");
                    } else {
                        $('.solution-active').removeClass("solution-active");
                        $(event.target).addClass("solution-active");
                    }

                    $(removeMatching[index]).click(function (e) {

                        let check2 = Array.prototype.indexOf.call(removeMatching, e.currentTarget);
                        //event.target.remove();
                        if (removePosition == check2) {

                            if (document.querySelector('.solution-active') != null) {
                                let x = document.querySelector('.solution-active').value;

                                let index1 = Array.prototype.indexOf.call(checkListNumber, x.split("-")[0]);
                                let index2 = Array.prototype.indexOf.call(checkListLetter, x.split("-")[1]);
                                checkListNumber.splice(index1, 1);
                                checkListLetter.splice(index2, 1);
                                $('.solution-active').remove();
                            }

                            $('.solution-active').remove();
                        }



                        if (tableSolution[check2].children.length < 1) {
                            a.classList.add("bg-gray-200");
                        } else {
                            a.classList.remove("bg-gray-200");
                        }
                        
                    });

                });
            };

        });
    });

  


    //Check Student Input Fillblank Test --- DONE
    var fillContent = document.querySelectorAll('#fill-content');
    fillContent.forEach((item) => {
        var fill1 = item.querySelectorAll('input');
        var checked = 0;
        fill1.forEach((item) => {
            item.addEventListener("keyup", (e) => {
                var z = item.getAttribute('id');
                var lengtext = item.value.length;

                if (lengtext != 0 && !$(item).hasClass("acting")) {
                    ++checked;
                    $(item).addClass("acting");
                }
                if (lengtext == 0 && $(item).hasClass("acting")) {
                    $(item).removeClass("acting");
                    --checked;
                }

                if (checked > 0) {
                    var a = document.querySelector('button[data-option="' + z + '"]');
                    a.classList.remove("bg-gray-200");
                } else {
                    var a = document.querySelector('button[data-option="' + z + '"]');
                    a.classList.add("bg-gray-200");
                }

            });

        });
    });

    //Check Student Input Fillblank GivenWord Test --- DONE
    var fillContent2 = document.querySelectorAll('#fill-content-given');

    fillContent2.forEach((item) => {
        var fill2 = item.querySelectorAll('select');
        var checkfill = 0;
        fill2.forEach((item) => {
            item.addEventListener("change", () => {
                var z = item.getAttribute('id');
                var a = document.querySelector('button[data-option="' + z + '"]');
                a.classList.remove("bg-gray-200");

                var y = document.querySelectorAll('#' + z);
                y.forEach((item2) => {
                    if (item2.value != '') {
                        checkfill = 0;
                    } else {
                        ++checkfill;
                    }

                });


                if (checkfill >= y.length) {
                    a.classList.add("bg-gray-200");
                }
            });
        });
    });

    const link = document.querySelectorAll("#scroll-to");
    link.forEach((item) => {
        item.addEventListener('click', () => {
            if (item.getAttribute('data-link') != null) {
                const element = document.getElementById(item.getAttribute('data-link'));
                element.scrollIntoView({ behavior: "smooth", block: "center" });
            }

            if (item.getAttribute('data-option') != null) {
                const element = document.getElementById(item.getAttribute('data-option'));
                element.scrollIntoView({ behavior: "smooth", block: "center" });
            }
            
        })
    });



});