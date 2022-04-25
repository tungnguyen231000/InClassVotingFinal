$(document).ready(function () {
    if ($('#createCourse').length != 0) {
        var fname = document.querySelector('#fname');
        $('#createCourse').click(function (e) {
            $('.image-error').remove();
            var fnameTxt = $(fname).val();

            $.ajax({
                url: '/Teacher/Course/CheckDuplicateCourse',
                dataType: "json",
                data: { text: fnameTxt },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error').length == 0) {
                            $(fname).after('<div class="image-error">*' + data.mess + '</div>');
                            $('.image-error').css("color", "red");
                            $('.image-error').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error').remove();
                    }

                    $('#form-create-course').submit();



                },
                error: function (xhr) {
                    alert('Error by Course');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });
        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-create-course').submit(function (e) {
            if ($('.image-error').length != 0) {
                e.preventDefault();
                return false;
            }
        });

    }


        //check edit course
    if ($('#editCourse').length != 0) {
        var newCourseName = document.querySelector('#newCourseName');
        var courseID = document.querySelector('#cIdToUpdate');

        $('#editCourse').click(function (e) {
            $('.image-error-2').remove();
            var newName = $(newCourseName).val();
            var oldCourseID = $(courseID).val();
            $.ajax({
                url: '/Teacher/Course/CheckDuplicateEditCourse',
                dataType: "json",
                data: { text: newName, cid: oldCourseID },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error-2').length == 0) {
                            $(newCourseName).after('<div class="image-error-2">*' + data.mess + '</div>');
                            $('.image-error-2').css("color", "red");
                            $('.image-error-2').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error-2').remove();
                    }

                    $('#form-edit-course').submit();



                },
                error: function (xhr) {
                    alert('Error by Course');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });

        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-edit-course').submit(function (e) {
            if ($('.image-error-2').length != 0) {
                e.preventDefault();
                return false;
            }
        });
    }

     //check new chapter 
    if ($('#createChapter').length != 0) {
       
        var chapterName = document.querySelector('#createChapterName');
        var courseIDToCreateChap = document.querySelector('#newChapCID');

        $('#createChapter').click(function (e) {
            $('.image-error-3').remove();
            var chapName = $(chapterName).val();
            var courseIDCreateChap = $(courseIDToCreateChap).val();

            $.ajax({
                url: '/Teacher/Chapter/CheckDuplicateNewChapter',
                dataType: "json",
                data: { text: chapName, cid: courseIDCreateChap },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error-3').length == 0) {
                            $(chapterName).after('<div class="image-error-3">*' + data.mess + '</div>');
                            $('.image-error-3').css("color", "red");
                            $('.image-error-3').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error-3').remove();
                    }
                    $('#form-create-chapter').submit();



                },
                error: function (xhr) {
                    alert('Error by Chapter');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });

        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-create-chapter').submit(function (e) {
            if ($('.image-error-3').length != 0) {
                e.preventDefault();
                return false;
            }
        });

    }


        //check edit chapter
    if ($('#editChapter').length != 0) {

        var newChapterName = document.querySelector('#newNameEditChapter');
        var courseIDEditChapter = document.querySelector('#cidEditChapter');
        var chapterIdToEdit = document.querySelector('#chidEditChapter');

        $('#editChapter').click(function (e) {
            $('.image-error-4').remove();
            var newChapName = $(newChapterName).val();
            var courseIDToUpdate = $(courseIDEditChapter).val();
            var chapIDToUpdate = $(chapterIdToEdit).val();
            $.ajax({
                url: '/Teacher/Chapter/CheckDuplicateEditChapter',
                dataType: "json",
                data: { text: newChapName, cid: courseIDToUpdate, chid: chapIDToUpdate },
                type: "POST",
                success: function (data) {
                    if (data.check == "0") {
                        if ($('.image-error-4').length == 0) {
                            $(newChapterName).after('<div class="image-error-4">*' + data.mess + '</div>');
                            $('.image-error-4').css("color", "red");
                            $('.image-error-4').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error-4').remove();
                    }

                    $('#form-edit-chapter').submit();


                },
                error: function (xhr) {
                    alert('Error by edit chapter');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });

        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-edit-chapter').submit(function (e) {
            if ($('.image-error-4').length != 0) {
                e.preventDefault();
                return false;
            }
        });
    }


});

function deleteQuestion() {
    document.getElementById("formDeleteQuestion").submit();
}
