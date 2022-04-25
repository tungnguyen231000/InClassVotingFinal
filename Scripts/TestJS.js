$(document).ready(function () {
   /* console.log('Active');
   // ======= Start Test Dialog ===========
    $('.start-test').click(function () {

        //phần tử ".delete-dialog"
        var dialog = $(".start-dialog");

        //cho hiện hộp đăng nhập trong 300ms
        $(dialog).fadeIn("slow");

        // thêm phần tử id="over" vào sau body
        $('body').append('<div id="over"></div>');
        $('#over').fadeIn(300);

        return false;
    });

    // khi click đóng hộp thoại
    $(document).on('click', ".close, #over", function () {
        $('#over, .start-dialog').fadeOut(300, function () {
            $('#over').remove();
        });
        return false;
    });
   // ======= End Start Test Dialog =========


   // Check radio buton 1 time
        console.log('Active2');
        //Check radio buton 1 time
        $('input[type="radio"]').on('change', function () {
            $('input[type="radio"]').not(this).prop('checked', false);
        });
   // End Check radio buton 1 time
*/
});




//Copy Link function
/*(function () {
    var copyButton = document.querySelector('.copy button');
    var copyInput = document.querySelector('.copy input');

    copyButton.addEventListener('click', function (e) {
        e.preventDefault();
        var text = copyInput.select();
        document.execCommand('copy');
    });

    copyInput.addEventListener('click', function () {
        this.select();
    });

})();
//End Copy Link function



function copyLink() {
    var copyText = document.getElementById("quizLink");

    copyText.select();
    navigator.clipboard.writeText(copyText.value);

}*/