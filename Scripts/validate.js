var arraytxt = [];

$(document).ready(function () {

	$('.data-row').each(function (index) {
		arraytxt.push($(this).text());
		if ($(this).text().length > 100) {
			$(this).text($(this).text().substring(0, 100) + '...');

			$(this).mouseover(function () {
				$(this).text(arraytxt[index]);
			});
			$(this).mouseout(function () {
				$(this).text($(this).text().substring(0, 100) + '...');
			});
		}
	});

	//===============Validate Image ================
	if ($(".drag-content").length != 0) {
		$(".file-input").change(function () {
			let files = $(".file-input").prop('files');
			if (files.length > 0) {
				if (files[0].size > 4 * 1024 * 1024) {
					if ($('.image-error').length == 0) {
						$(".file-input").after('<div class="image-error">*Image size exceeds 4MB</div>');
						$('.image-error').css("color", "red");
						$('.image-error').css("font-weight", "bold");
					}
					$(".file-input").val(null);
					return false;
				} else {
					$('.image-error').remove();
				}
			}
		});



	}
	//===============End Validate Image ================

	//===============Validate FillBlank - Done ================
	$("#fillblank-form").validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true
			},
			"mark": {
				required: true
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			},
			"mark": {
				required: "*Mark is required"
			}
		}
	});
	if ($("#fillblank-form").length != 0) {
		$("#fillblank-form").submit(function () {

			if ($('#questionFill').val().trim() == '') {
				if ($('.question-error').length == 0) {
					$('#questionFill').after('<div class="question-error">*Question is required</div>');
					$('.question-error').css("color", "red");
					$('.question-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$('.question-error').remove();
			}

			const regexGroup = /\(\~[^\(\)]+\)/g;
			var fillText = $("#questionFill").val();
			var arrGroup = fillText.match(regexGroup);

			

			if ($("#givenFill").prop("checked") == true) {
				if (arrGroup != null) {
					$('.question-error').remove();
					for (var i = 0; i < arrGroup.length; i++) {
						var listTotal = [];

						arrGroup[i] = arrGroup[i].replace("(", "");
						arrGroup[i] = arrGroup[i].replace(")", "");
						var listOption = arrGroup[i].match(/\~[^,.=]{0,1}[aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,. ]+/g);
						var listAnswer = arrGroup[i].match(/\~=[^,.]{0,1}[aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,. ]+/g);


						

						if (listOption == null) {
							if ($('.question-error').length == 0) {
								$('#questionFill').after('<div class="question-error">*Option at blank number ' + (i + 1) + ' is invalid (There must be at least 1 option)  </div>');
								$('.question-error').css("color", "red");
								$('.question-error').css("font-weight", "bold");
								return false;
							}
						} else {
							$('.question-error').remove();
						}

						if ( listAnswer ==null) {
							if ($('.question-error').length == 0) {
								$('#questionFill').after('<div class="question-error">*There must be at least 1 correct answer contain "~=" symbol at blank number ' + (i + 1) + '</div>');
								$('.question-error').css("color", "red");
								$('.question-error').css("font-weight", "bold");
								return false;
							}
						}
						else if (listAnswer.length > 1 ) {
							if ($('.question-error').length == 0) {
								$('#questionFill').after('<div class="question-error">*Only 1 correct answer at blank number ' + (i + 1) + ' allowed </div>');
								$('.question-error').css("color", "red");
								$('.question-error').css("font-weight", "bold");
								return false;
							}
                        }
						else {
							$('.question-error').remove();
						}

						for (let j = 0; j < listOption.length; j++) {
							if (listOption[j].trim().length > 50) {
								if ($('.question-error').length == 0) {
									$('#questionFill').after('<div class="question-error">*Option at blank number ' + (i + 1) + ' must be equal or less than 50 characters</div>');
									$('.question-error').css("color", "red");
									$('.question-error').css("font-weight", "bold");
									return false;
								} else {
									$('.question-error').remove();
								}
								return false;
							}

							

							if (jQuery.inArray(listOption[j].replace("~", "").trim(), listTotal) !== -1) {
							} else {
								listTotal.push(listOption[j].replace("~", "").trim());
							}
						}
						for (let j = 0; j < listAnswer.length; j++) {
							if (listAnswer[j].length > 50) {
								if ($('.question-error').length == 0) {
									$('#questionFill').after('<div class="question-error">* Correct answer at blank ' + (j + 1) + ' must be equal or less than 50 characters</div>');
									$('.question-error').css("color", "red");
									$('.question-error').css("font-weight", "bold");
									return false;
								} else {
									$('.question-error').remove();
								}
								return false;
							}

							if (jQuery.inArray(listAnswer[j].replace("~=", "").trim(), listTotal) !== -1) {
							} else {
								listTotal.push(listAnswer[j].replace("~=", "").trim());
							}
						}



						if (listTotal.length != (listOption.length + listAnswer.length)) {
							if ($('.question-error').length == 0) {
								$('#questionFill').after('<div class="question-error">*Duplicate option at blank number ' + (i + 1) +'</div>');
								$('.question-error').css("color", "red");
								$('.question-error').css("font-weight", "bold");
								return false;
							} else {
								$('.question-error').remove();
							}
						}
					}


				} else {
					if ($('.question-error').length == 0) {
						$('#questionFill').after('<div class="question-error">*There must be at least 1 blank inside the question (at least 1 option and 1 correct answer inside bracket "()" required) </div>');
						$('.question-error').css("color", "red");
						$('.question-error').css("font-weight", "bold");
						return false;
					}
				}
			}
			if ($("#givenFill").prop("checked") == false) {

				const regexGroup2 = /\([^\(\)]+\)/g;
				var arrGroup = fillText.match(regexGroup2);
				$('.question-error').remove();


				if (arrGroup == null) {
					if ($('.question-error').length == 0) {
						$('#questionFill').after('<div class="question-error">*There must be at least 1 blank inside the question (at least 1 answer inside bracket "()" required) </div>');
						$('.question-error').css("color", "red");
						$('.question-error').css("font-weight", "bold");
						return false;
					}
				} else {
					$('.question-error').remove();
				}

				for (let i = 0; i < arrGroup.length; i++) {
					let xxx = arrGroup[i];
					xxx = xxx.replace("(", "");
					xxx = xxx.replace(")", "");
					xxx = xxx.trim();


					if (xxx == "") {
						if ($('.question-error').length == 0) {
							$('#questionFill').after('<div class="question-error">*You need to input correct answer for blank number ' + (1 + i) + '</div>');
							$('.question-error').css("color", "red");
							$('.question-error').css("font-weight", "bold");
							return false;
						}
					} else {
						$('.question-error').remove();
					}
				}
			}

			/*
			if ($('#answerFill').val().trim() == '') {
				if ($('.question-error').length == 0) {
					$('#answerFill').after('<div class="question-error">*Answer is required</div>');
					$('.question-error').css("color", "red");
					$('.question-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$('.question-error').remove();
			}
			*/

		});
	}
	//===============End Validate FillBlank - Done================

	//===============Validate Multiple Choice - Done ================
	$('#multiple-form').validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true,
			},
			"mark": {
				required: true,
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			},
			"mark": {
				required: "*Mark is required"
			}
		}
	});

	if ($('#multiple-form').length != 0) {

		$('.ms-error').css("color", "red");
		$('.ms-error').css("font-weight", "bold");

		$('#multiple-form').submit(function () {
			var listCheckbox = document.querySelectorAll('#optionMultiple-checkbox');
			var listOptionText = document.querySelectorAll('#optionMultiple-text');
			let countText = 0;
			let countCheckbox = 0;
			let checkDuplicate = [];

			if ($(".drag-area img").length == 0) {
				if ($('#questionMultiple').val().trim() == '') {
					if($('.question-error').length == 0){
						$('#questionMultiple').after('<div class="question-error">*Question is required</div>');
						$('.question-error').css("color", "red");
						$('.question-error').css("font-weight", "bold");
					}
					return false;
				} else {
					$('.question-error').remove();
				}
			} else {
				$('.question-error').remove();
			}

			for (var i = 0; i < listOptionText.length; i++) {

				if (listOptionText[i].value.trim() != '') {
					countText++;
				}

				if (listCheckbox[i].checked == true) {
					countCheckbox++;
				}

				if (listCheckbox[i].checked && listOptionText[i].value.trim() == '') {
					$('.ms-error').html("*Unable to check null option");
					return false;
				}
			}

			
			if (countText < 2) {
				$('.ms-error').html("*You need to fill at least 2 options");
				return false;
			} else {
				$('.ms-error').html("");
			}
			
			/*
			if (countText <= countCheckbox || countCheckbox == 0) {
				$('.ms-error').html("*Number of checkbox invalid");
				return false;
			} else {
				$('.ms-error').html("");
			}*/

			if (countText <= countCheckbox) {
				$('.ms-error').html("*You cannot check on all options");
				return false;
			} else if (countCheckbox == 0) {
				$('.ms-error').html("You must check on at least 1 option which is the correct answer");
				return false;
			} else {
				$('.ms-error').html("");
			}

			for (var i = 0; i < listOptionText.length; i++) {
				if (listOptionText[i].value.trim() != '' && ! checkDuplicate.includes(listOptionText[i].value.trim()) ) {
					checkDuplicate.push(listOptionText[i].value.trim());
				}
			}

			if (countText != checkDuplicate.length) {
				$('.ms-error').html("*You cannot input duplicate option");
				return false;
			} else {
				$('.ms-error').html("");
			}

		});

	}

	//===============End Validate Multiple Choice================

	//===============Validate Short Answer - Done ================
	$("#shortanswer-form").validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true
			},
			"mark": {
				required: true
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			},
			"mark": {
				required: "*Mark is required"
			}
		}
	});

	if ($("#shortanswer-form").length != 0) {
		$('#shortanswer-form').submit(function () {
			if ($(".drag-area img").length == 0) {
				if ($('#questionShort').val().trim() == '') {
					if ($('.question-error').length == 0) {
						$('#questionShort').after('<div class="question-error">*Question is required</div>');
						$('.question-error').css("color", "red");
						$('.question-error').css("font-weight", "bold");
					}
					return false;
				} else {
					$('.question-error').remove();
				}

			} else {
				$('.question-error').remove();
			}

			if ($('#answerShort').val().trim() == '') {
				if ($('.an-error').length == 0) {
					$('#answerShort').after('<div class="an-error">*Answer is required</div>');
					$('.an-error').css("color", "red");
					$('.an-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$('.an-error').remove();
			}

			if ($('#answerShort').val().trim().length > 100) {
				if ($('.an-error').length == 0) {
					$('#answerShort').after('<div class="an-error">*Correct answer must be equal or less than 100 characters</div>');
					$('.an-error').css("color", "red");
					$('.an-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$('.an-error').remove();
			}

			
		});

    }
	//===============End Validate Short Answer================

	//=============Validate Reading - Done==============
	if ($('#reading-form').length != 0) {
		$('#reading-form').submit(function () {
			var checkValidate = true;
			$('.table-error').remove();
			var listQuestion = document.querySelectorAll("#reading-question #questionReading");
			var listTime = document.querySelectorAll("#reading-question #timeReading");
			var listMark = document.querySelectorAll("#reading-question #markReading");
			var listTableReading = document.querySelectorAll('#reading-question #table-reading');

			if ($(".drag-area img").length == 0) {
				if ($('#paragraphReading').val().trim() == '') {
					if ($('.para-error').length == 0) {
						$('#paragraphReading').after('<div class="para-error">*Passage is required</div>');
						$('.para-error').css("color", "red");
						$('.para-error').css("font-weight", "bold");
                    }
					checkValidate = false;
				} else {
					$('.para-error').remove();
				}
			} else {
				$('.para-error').remove();
			}

			for (var i = 0; i < listQuestion.length; i++) {
				if ($(listQuestion[i]).val().trim() == '') {
					if ($('.question-error').length == 0) {
						$(listQuestion[i]).after('<div class="question-error">*Question is required</div>');
						$('.question-error').css("color", "red");
						$('.question-error').css("font-weight", "bold");
					}
					checkValidate = false;
				} else {
					var questionE = document.querySelectorAll('.question-error');
					$(questionE[i]).remove();
				}
			}
			for (var i = 0; i < listMark.length; i++) {
				if ($(listMark[i]).val().trim() == '') {
					if ($('.mark-error').length == 0) {
						$(listMark[i]).after('<label class="mark-error error">*Mark is required</label>');
						$('.mark-error').css("color", "red");
						$('.mark-error').css("font-weight", "bold");
					}
					checkValidate = false;
				} else {
					var markE = document.querySelectorAll('.mark-error');
					$(markE[i]).remove();
				}
			}
			for (var i = 0; i < listTime.length; i++) {
				if ($(listTime[i]).val().trim() == '') {
					if ($('.time-error').length == 0) {
						$(listTime[i]).after('<label class="time-error error">*Time is required</label>');
						$('.time-error').css("color", "red");
						$('.time-error').css("font-weight", "bold");
					}
					checkValidate = false;
				} else {
					var timeE = document.querySelectorAll('.time-error');
					$(timeE[i]).remove();
				}
			}
			for (var i = 0; i < listTableReading.length; i++) {
				let checkDuplicate = [];
				let listOptionText;
				let listCheckbox;
				let countText = 0;
				let countCheckbox = 0;
				listOptionText = listTableReading[i].querySelectorAll('#optionReading-text');
				listCheckbox = listTableReading[i].querySelectorAll('#optionReading-checkbox');

				$(listTableReading[i]).before('<div class="table-error"></div>');
				$('.table-error').css("color", "red");
				$('.table-error').css("font-weight", "bold");
				for (var j = 0; j < listOptionText.length; j++) {
					if (listOptionText[j].value.trim() != '') {
						countText++;
					}

					if (listCheckbox[j].checked == true) {
						countCheckbox++;
					}

					if (listCheckbox[j].checked && listOptionText[j].value.trim() == '') {
						$('.table-error').html("*Unable to check null option");
						return false;
					}

					if (listOptionText[j].value.trim() != '' && !checkDuplicate.includes(listOptionText[j].value.trim())) {
						checkDuplicate.push(listOptionText[j].value.trim());
					}
				}

				if (countText < 2) {
					$('.table-error').html("*You need to fill at least 2 options");
					return false;
				} else {
					$('.table-error').html("");
				}

				/*if (countText <= countCheckbox || countCheckbox == 0) {
					$('.table-error').html("*Number of checkbox invalid");
					return false;
				} else {
					$('.table-error').html("");
				}*/

				if (countText <= countCheckbox) {
					$('.table-error').html("*You cannot check on all options");
					return false;
				} else if (countCheckbox == 0) {
					$('.table-error').html("You must check on at least 1 option which is the correct answer");
					return false;
				} else {
					$('.table-error').html("");
				}

				if (countText != checkDuplicate.length) {
					$('.table-error').html("*You cannot input duplicate option");
					return false;
				} else {
					$('.table-error').html("");
				}
				

				$('.table-error').remove();
			}

			return checkValidate;
		});
	}
	//=============End Validate Reading==============

	//=============Validate Matching - Tạm vậy==============
	$("#matching-form").validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true
			},
			"mark": {
				required: true
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			},
			"mark": {
				required: "*Mark is required"
			}
		}
	});
	if ($("#matching-form").length != 0) {
		

		$("#matching-form").submit(function () {
			$('.solution-error').remove();
			$('.left-error').remove();
			$('.right-error').remove();

			if ($("#matching-left").val().trim() == '') {
				if ($(".left-error").length == 0) {
					$("#matching-left").after('<div class="left-error">*Column A is required</div>');
					$('.left-error').css("color", "red");
					$('.left-error').css("font-weight", "bold");
                }
				
				return false;

			} else {
				$('.left-error').remove();
			}

			if ($("#matching-right").val().trim() == '') {
				if ($(".right-error").length == 0) {
					$("#matching-right").after('<div class="right-error">*Column B is required</div>');
					$('.right-error').css("color", "red");
					$('.right-error').css("font-weight", "bold");
				}
				return false;

			} else {
				$('.right-error').remove();
			}

			if ($("#matching-solution").length == 0) {
				if ($(".solution-error").length == 0) {
					$("#matching-option").after('<span class="solution-error">* Solution is required</span>');
					$('.solution-error').css("color", "red");
					$('.solution-error').css("font-weight", "bold");
				}
				return false;

			} else {
				$('.solution-error').remove();
			}

			return true;

		});
    }
	//=============End Validate Matching==============

	//=============Validate Indicate Mistake==============
	$("#indicate-form").validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true
			},
			"mark": {
				required: true
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			},
			"mark": {
				required: "*Mark is required"
			}
		}
	});

	var list = [];

	if ($("#indicate-form").length != 0) {
		$("#indicate-form").submit(function () {
			var regexIndicateQuestion = /\([aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ1234567890,.\-\+\=\*\/ ]+\)/g;
			let question = $("#questionIndicate").val();
			list = question.match(regexIndicateQuestion);
			var checkOption = [];
			var checkDuplicateAnswer = [];

			$(".indicate-error").remove();


			if ($('#questionIndicate').val().trim() == "") {
				if ($('.indicate-error').length == 0) {
					$("#questionIndicate").after('<div class="indicate-error">*Question is required</div>');
					$('.indicate-error').css("color", "red");
					$('.indicate-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$(".indicate-error").remove();
			}

			if (typeof list === 'undefined' || list == null) {
				if ($('.indicate-error').length == 0) {
					$("#questionIndicate").after('<div class="indicate-error">*Option is required</div>');
					$('.indicate-error').css("color", "red");
					$('.indicate-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$(".indicate-error").remove();
			}

			if (list.length < 2 || list.length >= 6) {
				if ($(".indicate-error").length == 0) {
					$("#questionIndicate").after('<div class="indicate-error">*Option must be more than 2 and less than 6 </div>');
					$('.indicate-error').css("color", "red");
					$('.indicate-error').css("font-weight", "bold");
					return false;
				}
			} else {
				$(".indicate-error").remove();
			}

			let checkDuplicate = [];

			for (let i = 0; i < list.length; i++) {
				let xxx = list[i];
				xxx =xxx.replace("(", "");
				xxx =xxx.replace(")", "");
				xxx =xxx.trim();

				if (xxx == "" ) {
					if ($(".indicate-error").length == 0) {
						$("#questionIndicate").after('<div class="indicate-error">*Option ('+(i+1)+') can not empty </div>');
						$('.indicate-error').css("color", "red");
						$('.indicate-error').css("font-weight", "bold");
						return false;
					}
				} else {
					$(".indicate-error").remove();
				}
				

				if (checkDuplicate.includes(xxx)) {
					if ($(".indicate-error").length == 0) {
						$("#questionIndicate").after('<div class="indicate-error">*Option (' + (i + 1) + ') already exists </div>');
						$('.indicate-error').css("color", "red");
						$('.indicate-error').css("font-weight", "bold");
						
					}
					return false;
				} else {
					$(".indicate-error").remove();
					checkDuplicate.push(xxx);
                }

				

            }

			if ($('#answerIndicate').val().trim() == "") {
				if ($('.indicatex-error').length == 0) {
					$("#answerIndicate").after('<div class="indicatex-error">*Answer is required</div>');
					$('.indicatex-error').css("color", "red");
					$('.indicatex-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$(".indicatex-error").remove();
			}

			$(".indicate-error").remove();

			var checkAnswer = 0;

			for (var j = 0; j < list.length; j++) {
				list[j] = list[j].replace("(", "");
				list[j] = list[j].replace(")", "");

				$('.indicateAnswer-error').remove();

				if (list[j].trim().length > 50) {
					if ($('.indicateAnswer-error').length == 0) {
						$("#questionIndicate").after('<div class="indicateAnswer-error">*Option ('+ (j+1) +') less than 50 characters</div>');
						$('.indicateAnswer-error').css("color", "red");
						$('.indicateAnswer-error').css("font-weight", "bold");
					}
					return false;
				}
				$('.indicateAnswer-error').remove();


				if (list[j].toLowerCase().trim() == $('#answerIndicate').val().toLowerCase().trim()) {
					++checkAnswer;
				}
			}


			if ($('#answerIndicate').val().trim().length > 50) {
				if ($('.indicateAnswer-error').length == 0) {
					$("#answerIndicate").after('<div class="indicateAnswer-error">*Answer less than 50 characters</div>');
					$('.indicateAnswer-error').css("color", "red");
					$('.indicateAnswer-error').css("font-weight", "bold");
				}
				return false;
			} else {
				$('.indicateAnswer-error').remove();
			}

			if (checkAnswer <= 0) {
				if ($('.indicateAnswer-error').length == 0) {
					$("#answerIndicate").after('<div class="indicateAnswer-error">*No option matching with answer</div>');
					$('.indicateAnswer-error').css("color", "red");
					$('.indicateAnswer-error').css("font-weight", "bold");
                }
				return false;
			} else {
				$('.indicateAnswer-error').remove();
			}

			


			return true;
		});

    }

	//=============End Validate Indicate Mistake==============

	//===============Validate Poll - New ================
	$('#poll-form').validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"time": {
				required: true,
			}
		},
		messages: {
			"time": {
				required: "*Time is required"
			}
		}
	});

	if ($('#poll-form').length != 0) {
		let checkValidate = true;

		$('.ms-error').css("color", "red");
		$('.ms-error').css("font-weight", "bold");

		$('#poll-form').submit(function () {
			$('.question-error').remove();
			$('.question-error1').remove();


			var listOptionText = document.querySelectorAll('#txtPoll');
			let countText = 0;
			let checkDuplicate = [];

			

			if ($('#pollName').val().trim() == '') {
				if ($('.question-error').length == 0) {
					$('#pollName').after('<label class="question-error error">*Poll name is required</label>');
					$('.question-error').css("color", "red");
					$('.question-error').css("font-weight", "bold");
				}
				checkValidate = false;
			} else {
				$('.question-error').remove();
				checkValidate = true;
			}

			

			if ($('#pollName').val().trim().length > 100) {
				if ($('.question-error').length == 0) {
					$('#pollName').after('<label class="question-error error">*Poll name less than 100 characters</label>');
					$('.question-error').css("color", "red");
					$('.question-error').css("font-weight", "bold");
				}
				checkValidate = false;
			} else {
				$('.question-error1').remove();
				if (checkValidate != false) {
					checkValidate = true;
				}
			}


			if ($('#questionPoll').val().trim() == '') {
				if ($('.question-error1').length == 0) {
					$('#questionPoll').after('<div class="question-error1">*Question is required</div>');
					$('.question-error1').css("color", "red");
					$('.question-error1').css("font-weight", "bold");
                }
				checkValidate = false;
			} else {
				$('.question-error1').remove();
				if (checkValidate != false) {
					checkValidate = true;
                }
			}

			for (var i = 0; i < listOptionText.length; i++) {
				if (listOptionText[i].value.trim() != '') {
					countText++;

					if (!checkDuplicate.includes(listOptionText[i].value.trim())) {
						checkDuplicate.push(listOptionText[i].value.trim());
                    }
				}
			}



			if (countText < 2) {
				$('.ms-error').html("*You need to fill at least 2 options");
				checkValidate = false;
			} else {
				$('.ms-error').html("");
				if (checkValidate != false) {
					checkValidate = true;
				}
			}

			if (countText != checkDuplicate.length) {
				$('.ms-error').html("*You cannot input duplicate option");
				checkValidate = false;
			} else {
				
				if (checkValidate != false) {
					$('.ms-error').html("");
					checkValidate = true;
				}
			}


			return checkValidate;

		});

	}

	//===============End Validate Poll================

	/*$('#form-edit-report-name').validate({
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		rules: {
			"newReportName": {
				required: true,
			},
		},
		messages: {
			"newReportName": {
				required: "*Report name is required"
			}
		}
	});*/


});