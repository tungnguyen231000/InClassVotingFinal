$(document).ready(function () {
    var userFromform = document.querySelector('#usernameInput');
    var passFromform = document.querySelector('#passwordInput');

    $('#adminLogin').click(function (e) {
        
        $('.image-error').remove();
        var username = $(userFromform).val();
        var password = $(passFromform).val();

        $.ajax({
            url: '/HomeTotal/CheckAdminAccount',
            dataType: "json",
            data: { user: username,  pass: password },
            type: "POST",
            success: function (data) {
                if (data.check == "0") {
                    if ($('.image-error').length == 0) {
                        $(passFromform).after('<div class="image-error">*' + data.mess + '</div>');
                        $('.image-error').css("color", "red");
                        $('.image-error').css("font-weight", "bold");
                    }
                }
                if (data.check == "1") {
                    $('.image-error').remove();
                }

                $('#form-login-admin').submit();



            },
            error: function (xhr) {
                alert('Error by Login');
            }
        });

        //Để không tự submit nữa
        e.preventDefault();
        return false;
    });

    //Khi submit sẽ check xem có class .image-error hay không
    $('#form-login-admin').submit(function (e) {
        if ($('.image-error').length != 0) {
            e.preventDefault();
            return false;
        }
    });

  
});
