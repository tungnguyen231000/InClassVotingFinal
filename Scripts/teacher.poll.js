
$(document).ready(function () {

if ($('#reopenPoll').length != 0) {
    var newPollName = document.querySelector('#newPollName');

    $('#reopenPoll').click(function (e) {

        $('.image-error-2').remove();
        var newName = $(newPollName).val();
        $.ajax({
            url: '/Teacher/Poll/CheckLengthPollName',
            dataType: "json",
            data: { text: newName},
            type: "POST",
            success: function (data) {

                if (data.check == "0") {
                    if ($('.image-error-2').length == 0) {
                        $('#newPollName').after('<div class="image-error-2">*' + data.mess + '</div>');
                        $('.image-error-2').css("color", "red");
                        $('.image-error-2').css("font-weight", "bold");
                    }
                }
                if (data.check == "1") {
                    $('.image-error-2').remove();
                }

                $('#form-reopen-poll').submit();


            },
            error: function (xhr) {
                alert('Error by poll name');
            }
        });

        //Để không tự submit nữa
        e.preventDefault();
        return false;
    });
    //Khi submit sẽ check xem có class .image-error hay không
    $('#form-reopen-poll').submit(function (e) {
        if ($('.image-error-2').length != 0) {
            e.preventDefault();
            return false;
        }
    });

    }


});