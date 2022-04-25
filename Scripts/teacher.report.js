
$(document).ready(function () {
   

    //check edit report
    if ($('#editReport').length != 0) {

        var newReportName = document.querySelector('#newReportName');

        $('#editReport').click(function (e) {
            $('.image-error-1').remove();
            var reportNameEdit = $(newReportName).val();
            $.ajax({
                url: '/Teacher/Report/CheckReportName',
                dataType: "json",
                data: { text: reportNameEdit },
                type: "POST",
                success: function (data) {
                    $('.image-error-1').remove();

                    if (data.check == "0") {
                        if ($('.image-error-1').length == 0) {
                            $("#newReportName").after('<div class="image-error-1">*' + data.mess + '</div>');
                            $('.image-error-1').css("color", "red");
                            $('.image-error-1').css("font-weight", "bold");
                        }
                    }
                    if (data.check == "1") {
                        $('.image-error-1').remove();
                    }

                    $('#form-edit-report-name').submit();


                },
                error: function (xhr) {
                    alert('Error by edit report name');
                }
            });

            //Để không tự submit nữa
            e.preventDefault();
            return false;
        });

        //Khi submit sẽ check xem có class .image-error hay không
        $('#form-edit-report-name').submit(function (e) {
            if ($('.image-error-1').length != 0) {
                e.preventDefault();
                return false;
            }
        });
    }



});